using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RevitAIArchitect
{
    /// <summary>
    /// Represents a command that AI wants to execute in Revit.
    /// </summary>
    public class AiCommand
    {
        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;

        [JsonPropertyName("elementIds")]
        public List<long>? ElementIds { get; set; }

        [JsonPropertyName("parameterName")]
        public string? ParameterName { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Check if command requires user confirmation.
        /// </summary>
        public bool RequiresConfirmation => Action switch
        {
            "select" => false,
            "delete" => true,
            "rename" => true,
            "set_parameter" => true,
            "hide" => true,
            "isolate" => true,
            "override_color" => true,
            "open_view" => false,
            _ => true
        };

        /// <summary>
        /// Get risk level for display.
        /// </summary>
        public string RiskLevel => Action switch
        {
            "select" => "Safe",
            "delete" => "High",
            "rename" => "Medium",
            "set_parameter" => "Medium",
            "hide" => "Medium",
            "isolate" => "Medium",
            "override_color" => "Medium",
            "open_view" => "Low",
            _ => "Unknown"
        };

        /// <summary>
        /// Basic validation before executing the command.
        /// </summary>
        public (bool IsValid, string Error) Validate()
        {
            if (string.IsNullOrWhiteSpace(Action))
                return (false, "Missing action.");

            var action = Action.ToLowerInvariant();
            var supported = new[] { "select", "delete", "rename", "set_parameter", "hide", "isolate", "override_color", "open_view" };
            if (!supported.Contains(action))
                return (false, $"Unknown action: {Action}");

            if (action == "select" || action == "delete" || action == "rename" || action == "set_parameter" || action == "hide" || action == "isolate" || action == "override_color")
            {
                if (ElementIds == null || ElementIds.Count == 0)
                    return (false, "Element IDs are required.");
            }

            if (action == "rename")
            {
                if (string.IsNullOrWhiteSpace(Value))
                    return (false, "New name/value is required for rename.");
            }

            if (action == "set_parameter")
            {
                if (string.IsNullOrWhiteSpace(ParameterName))
                    return (false, "Parameter name is required for set_parameter.");
                if (Value == null)
                    return (false, "Value is required for set_parameter.");
            }

            if (action == "override_color")
            {
                if (string.IsNullOrWhiteSpace(Value))
                    return (false, "Color value is required for override_color (hex like #FF0000 or R,G,B).");
                if (!TryParseColor(Value, out _))
                    return (false, "Color value must be hex (#RRGGBB) or R,G,B (0-255).");
            }

            if (action == "open_view")
            {
                if (string.IsNullOrWhiteSpace(Value))
                    return (false, "View ID is required for open_view.");
                if (!int.TryParse(Value, out _))
                    return (false, "View ID must be a number (ElementId value).");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Parse color input to Revit color (byte r,g,b).
        /// </summary>
        public static bool TryParseColor(string input, out (byte r, byte g, byte b) color)
        {
            color = (0, 0, 0);
            if (string.IsNullOrWhiteSpace(input)) return false;

            var trimmed = input.Trim();
            if (trimmed.StartsWith("#"))
            {
                trimmed = trimmed.TrimStart('#');
                if (trimmed.Length == 6 && int.TryParse(trimmed, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int hex))
                {
                    byte r = (byte)((hex >> 16) & 0xFF);
                    byte g = (byte)((hex >> 8) & 0xFF);
                    byte b = (byte)(hex & 0xFF);
                    color = (r, g, b);
                    return true;
                }
            }
            else
            {
                var parts = trimmed.Split(',');
                if (parts.Length == 3
                    && byte.TryParse(parts[0].Trim(), out byte r)
                    && byte.TryParse(parts[1].Trim(), out byte g)
                    && byte.TryParse(parts[2].Trim(), out byte b))
                {
                    color = (r, g, b);
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Response structure from AI that may contain commands.
    /// </summary>
    public class AiResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("command")]
        public AiCommand? Command { get; set; }

        /// <summary>
        /// Try to parse AI response for embedded command.
        /// </summary>
        public static AiResponse Parse(string aiOutput)
        {
            var response = new AiResponse { Message = aiOutput };

            // Look for JSON command block in response
            int jsonStart = aiOutput.IndexOf("```json");
            int jsonEnd = aiOutput.LastIndexOf("```");

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                try
                {
                    string jsonBlock = aiOutput.Substring(jsonStart + 7, jsonEnd - jsonStart - 7).Trim();
                    var parsed = JsonSerializer.Deserialize<AiResponse>(jsonBlock);
                    if (parsed != null)
                    {
                        response.Message = parsed.Message;
                        response.Command = parsed.Command;
                    }
                }
                catch { }
            }

            // Also try parsing the whole response as JSON
            if (response.Command == null && aiOutput.TrimStart().StartsWith("{"))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<AiResponse>(aiOutput);
                    if (parsed != null)
                    {
                        response.Message = parsed.Message;
                        response.Command = parsed.Command;
                    }
                }
                catch { }
            }

            return response;
        }
    }
}
