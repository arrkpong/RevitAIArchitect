using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RevitAIArchitect
{
    public class OpenAiProvider : IAiProvider
    {
        private static readonly HttpClient client = new HttpClient();
        private const string ApiUrl = "https://api.openai.com/v1/chat/completions";
        
        public string Name => "OpenAI";
        public string ApiKey { get; set; } = string.Empty;
        
        // Model selection - default to latest (gpt-4o)
        public string Model { get; set; } = "gpt-4o";

        // Available OpenAI models
        public static readonly string[] AvailableModels = new[]
        {
            "gpt-4o",              // Latest flagship
            "gpt-4o-mini",         // Fast & cheap
            "gpt-4-turbo",         // Previous flagship
            "gpt-3.5-turbo"        // Legacy fast
        };

        public Task<string> GetReplyAsync(string userMessage)
        {
            return GetReplyAsync(userMessage, null);
        }

        public async Task<string> GetReplyAsync(string userMessage, string? context)
        {
            if (string.IsNullOrEmpty(ApiKey))
            {
                return "Please enter your OpenAI API Key.";
            }

            try
            {
            // Build system prompt with context and command instructions
            string systemPrompt = @"You are a helpful assistant for Autodesk Revit. You help architects and engineers with their Revit models.

When the user asks you to DO something in Revit, you MUST respond with a JSON object like this:
{
  ""message"": ""Your explanation in Thai or English"",
  ""command"": {
    ""action"": ""select|delete|rename|set_parameter|hide|isolate|override_color|open_view"",
    ""elementIds"": [123456, 789012],
    ""parameterName"": ""required for set_parameter"",
    ""value"": ""new value / color / view id"",
    ""description"": ""Brief description of what this does""
  }
}

Available actions:
- select: Select elements by ID (no confirmation)
- delete: Delete elements (requires confirmation)
- rename: Set element comments to value (requires confirmation)
- set_parameter: Set parameter to value (requires confirmation)
- hide: Hide elements in active view (requires confirmation)
- isolate: Temporarily isolate elements in active view (requires confirmation)
- override_color: Override element color in active view (value = #RRGGBB or R,G,B)
- open_view: Switch to a view by ElementId (value = view ID)

Important:
- Always include elementIds when relevant.
- Only propose actions you are certain about; otherwise answer normally without JSON.
- Keep descriptions short.";

                if (!string.IsNullOrEmpty(context))
                {
                    systemPrompt += $"\n\nHere is the current Revit project context:\n{context}";
                }

                var requestBody = new
                {
                    model = Model, // Use selected model
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userMessage }
                    }
                };

                string json = System.Text.Json.JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);
                request.Content = content;

                var response = await client.SendAsync(request);
                string responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(responseString);
                    return doc.RootElement
                              .GetProperty("choices")[0]
                              .GetProperty("message")
                              .GetProperty("content")
                              .GetString() ?? "No response received.";
                }
                else
                {
                    return $"Error: {response.StatusCode} - {responseString}";
                }
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }
    }
}
