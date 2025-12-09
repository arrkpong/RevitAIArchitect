using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RevitAIArchitect
{
    public class AiService
    {
        private static readonly HttpClient client = new HttpClient();
        private const string ApiUrl = "https://api.openai.com/v1/chat/completions";
        
        // API Key is now set from UI
        public string ApiKey { get; set; } = string.Empty;

        public async Task<string> GetReplyAsync(string userMessage)
        {
            // Check if API key is set
            if (string.IsNullOrEmpty(ApiKey))
            {
                return "Please enter your OpenAI API Key in the settings above.";
            }

            try
            {
                var requestBody = new
                {
                    model = "gpt-4o", // or gpt-3.5-turbo
                    messages = new[]
                    {
                        new { role = "system", content = "You are a helpful assistant for Autodesk Revit. You help architects and engineers with their Revit models." },
                        new { role = "user", content = userMessage }
                    }
                };

                string json = System.Text.Json.JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Use fresh request with authorization header
                using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);
                request.Content = content;

                var response = await client.SendAsync(request);
                string responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Parse the response to get the content
                    using (var doc = System.Text.Json.JsonDocument.Parse(responseString))
                    {
                        return doc.RootElement
                                  .GetProperty("choices")[0]
                                  .GetProperty("message")
                                  .GetProperty("content")
                                  .GetString() ?? "No response received.";
                    }
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
