using Xunit;
using RevitAIArchitect;
using System.Threading.Tasks;

namespace RevitAIArchitect.Tests
{
    public class AiProviderTests
    {
        [Fact]
        public async Task OpenAiProvider_ReturnsErrorMessage_WhenNoApiKey()
        {
            // Arrange
            var provider = new OpenAiProvider();

            // Act
            string result = await provider.GetReplyAsync("Hello");

            // Assert
            Assert.Contains("Please enter your OpenAI API Key", result);
        }

        [Fact]
        public async Task GeminiProvider_ReturnsErrorMessage_WhenNoApiKey()
        {
            // Arrange
            var provider = new GeminiProvider();

            // Act
            string result = await provider.GetReplyAsync("Hello");

            // Assert
            Assert.Contains("Please enter your Google Gemini API Key", result);
        }

        [Fact]
        public void OpenAiProvider_HasCorrectName()
        {
            var provider = new OpenAiProvider();
            Assert.Equal("OpenAI", provider.Name);
        }

        [Fact]
        public void GeminiProvider_HasCorrectName()
        {
            var provider = new GeminiProvider();
            Assert.Equal("Google Gemini", provider.Name);
        }
    }
}
