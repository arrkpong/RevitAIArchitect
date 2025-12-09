using Xunit;
using RevitAIArchitect;
using System.Threading.Tasks;

namespace RevitAIArchitect.Tests
{
    public class AiServiceTests
    {
        [Fact]
        public async Task GetReplyAsync_ReturnsMockMessage_WhenNoApiKey()
        {
            // Arrange
            var service = new AiService();

            // Act
            string result = await service.GetReplyAsync("Hello");

            // Assert
            Assert.Contains("Mock Response", result);
        }

        // Add more tests here
    }
}
