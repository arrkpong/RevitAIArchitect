using System.Threading.Tasks;

namespace RevitAIArchitect
{
    public interface IAiProvider
    {
        string Name { get; }
        string ApiKey { get; set; }
        Task<string> GetReplyAsync(string userMessage);
        Task<string> GetReplyAsync(string userMessage, string? context);
    }
}
