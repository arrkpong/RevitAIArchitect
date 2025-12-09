using System;
using System.Collections.Generic;
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
            _ => "Unknown"
        };
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
