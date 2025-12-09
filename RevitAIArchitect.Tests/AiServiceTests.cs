using Xunit;
using RevitAIArchitect;
using System.Threading.Tasks;

namespace RevitAIArchitect.Tests
{
    public class AiProviderTests
    {
        #region API Key Tests

        [Fact]
        public async Task OpenAiProvider_ReturnsErrorMessage_WhenNoApiKey()
        {
            var provider = new OpenAiProvider();
            string result = await provider.GetReplyAsync("Hello");
            Assert.Contains("Please enter your OpenAI API Key", result);
        }

        [Fact]
        public async Task GeminiProvider_ReturnsErrorMessage_WhenNoApiKey()
        {
            var provider = new GeminiProvider();
            string result = await provider.GetReplyAsync("Hello");
            Assert.Contains("Please enter your Google Gemini API Key", result);
        }

        #endregion

        #region Provider Name Tests

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

        #endregion

        #region Model Selection Tests

        [Fact]
        public void OpenAiProvider_DefaultModel_IsGpt4o()
        {
            var provider = new OpenAiProvider();
            Assert.Equal("gpt-4o", provider.Model);
        }

        [Fact]
        public void GeminiProvider_DefaultModel_IsGemini3Pro()
        {
            var provider = new GeminiProvider();
            Assert.Equal("gemini-3-pro-preview", provider.Model);
        }

        [Fact]
        public void OpenAiProvider_CanChangeModel()
        {
            var provider = new OpenAiProvider();
            provider.Model = "gpt-4o-mini";
            Assert.Equal("gpt-4o-mini", provider.Model);
        }

        [Fact]
        public void GeminiProvider_CanChangeModel()
        {
            var provider = new GeminiProvider();
            provider.Model = "gemini-2.5-flash";
            Assert.Equal("gemini-2.5-flash", provider.Model);
        }

        [Fact]
        public void OpenAiProvider_HasAvailableModels()
        {
            Assert.NotEmpty(OpenAiProvider.AvailableModels);
            Assert.Contains("gpt-4o", OpenAiProvider.AvailableModels);
            Assert.Contains("gpt-4o-mini", OpenAiProvider.AvailableModels);
        }

        [Fact]
        public void GeminiProvider_HasAvailableModels()
        {
            Assert.NotEmpty(GeminiProvider.AvailableModels);
            Assert.Contains("gemini-3-pro-preview", GeminiProvider.AvailableModels);
            Assert.Contains("gemini-2.5-flash", GeminiProvider.AvailableModels);
        }

        #endregion

        #region API Key Property Tests

        [Fact]
        public void OpenAiProvider_ApiKey_DefaultEmpty()
        {
            var provider = new OpenAiProvider();
            Assert.Equal(string.Empty, provider.ApiKey);
        }

        [Fact]
        public void GeminiProvider_ApiKey_DefaultEmpty()
        {
            var provider = new GeminiProvider();
            Assert.Equal(string.Empty, provider.ApiKey);
        }

        [Fact]
        public void OpenAiProvider_CanSetApiKey()
        {
            var provider = new OpenAiProvider();
            provider.ApiKey = "test-key";
            Assert.Equal("test-key", provider.ApiKey);
        }

        [Fact]
        public void GeminiProvider_CanSetApiKey()
        {
            var provider = new GeminiProvider();
            provider.ApiKey = "test-key";
            Assert.Equal("test-key", provider.ApiKey);
        }

        #endregion
    }
}
