using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RevitAIArchitect
{
    public class GeminiProvider : IAiProvider
    {
        private static readonly HttpClient client = new HttpClient();
        
        public string Name => "Google Gemini";
        public string ApiKey { get; set; } = string.Empty;
        
        // Model selection - default to latest (gemini-3-pro-preview)
        public string Model { get; set; } = "gemini-3-pro-preview";

        // Available Gemini models
        public static readonly string[] AvailableModels = new[]
        {
            "gemini-3-pro-preview",      // Latest (Preview)
            "gemini-3-pro-image-preview", // Image generation (Preview)
            "gemini-2.5-flash"           // Stable
        };

        public Task<string> GetReplyAsync(string userMessage)
        {
            return GetReplyAsync(userMessage, null);
        }

        public async Task<string> GetReplyAsync(string userMessage, string? context)
        {
            if (string.IsNullOrEmpty(ApiKey))
            {
                return "Please enter your Google Gemini API Key.";
            }

            try
            {
                // Gemini API endpoint - dynamic model selection
                string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent?key={ApiKey}";

                // Build prompt with context and command instructions
                string systemPart = @"You are a helpful assistant for Autodesk Revit. You help architects and engineers with their Revit models.

When the user asks you to DO something in Revit (select, delete, rename, set parameter), you MUST respond with a JSON object like this:
{
  ""message"": ""Your explanation in Thai or English"",
  ""command"": {
    ""action"": ""select"" or ""delete"" or ""rename"" or ""set_parameter"",
    ""elementIds"": [123456, 789012],
    ""parameterName"": ""optional for set_parameter"",
    ""value"": ""optional new value"",
    ""description"": ""Brief description of what this does""
  }
}

Available actions:
- select: Select elements by ID
- delete: Delete elements (requires confirmation)
- rename: Set element comments
- set_parameter: Set a parameter value

If user is just asking a question (not requesting an action), respond normally without JSON.";

                if (!string.IsNullOrEmpty(context))
                {
                    systemPart += $"\n\nHere is the current Revit project context:\n{context}";
                }
                string fullPrompt = $"{systemPart}\n\nUser: {userMessage}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = fullPrompt }
                            }
                        }
                    }
                };

                string json = System.Text.Json.JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(apiUrl, content);
                string responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(responseString);
                    return doc.RootElement
                              .GetProperty("candidates")[0]
                              .GetProperty("content")
                              .GetProperty("parts")[0]
                              .GetProperty("text")
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
