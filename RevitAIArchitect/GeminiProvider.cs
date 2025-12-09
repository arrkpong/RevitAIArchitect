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

        public async Task<string> GetReplyAsync(string userMessage)
        {
            if (string.IsNullOrEmpty(ApiKey))
            {
                return "Please enter your Google Gemini API Key.";
            }

            try
            {
                // Gemini API endpoint
                string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={ApiKey}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = $"You are a helpful assistant for Autodesk Revit. You help architects and engineers with their Revit models.\n\nUser: {userMessage}" }
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
