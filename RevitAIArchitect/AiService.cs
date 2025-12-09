using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
// using Newtonsoft.Json; // We might need to add Newtonsoft.Json if we want robust parsing, but for now simple string manip for demo

namespace RevitAIArchitect
{
    public class AiService
    {
        private static readonly HttpClient client = new HttpClient();
        // TODO: Replace with your actual API Key or load from config
        private const string ApiKey = "YOUR_OPENAI_API_KEY_HERE"; 
        private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

        public async Task<string> GetReplyAsync(string userMessage)
        {
            // For safety, if no key is set, return a mock response
            if (ApiKey == "YOUR_OPENAI_API_KEY_HERE")
            {
                await Task.Delay(1000); // Simulate network delay
                return "I am ready to help! Please add your OpenAI API Key in AiService.cs to connect to the real AI. (Mock Response)";
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

                // Simple JSON serialization (using System.Text.Json would require reference update or newer .NET)
                // For simplicity/compatibility let's use a quick string format or check if we have System.Text.Json available in .NET 8 (we do!)
                
                string json = System.Text.Json.JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);

                var response = await client.PostAsync(ApiUrl, content);
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
                                  .GetString();
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
