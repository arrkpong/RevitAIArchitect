using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace RevitAIArchitect
{
    public partial class ChatWindow : Window
    {
        private readonly AiService _aiService;
        public ObservableCollection<string> Messages { get; set; }

        public ChatWindow()
        {
            InitializeComponent();
            _aiService = new AiService();
            Messages = new ObservableCollection<string>();
            ChatHistory.ItemsSource = Messages;
            
            // Add initial welcome message
            Messages.Add("AI: Hello! How can I help you with your Revit model today?");
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string userMessage = InputBox.Text;
            if (string.IsNullOrWhiteSpace(userMessage)) return;

            // Add User Message
            Messages.Add($"You: {userMessage}");
            InputBox.Clear();
            InputBox.IsEnabled = false;

            try
            {
                // Call AI Service
                string aiResponse = await _aiService.GetReplyAsync(userMessage);
                Messages.Add($"AI: {aiResponse}");
            }
            catch (Exception ex)
            {
                Messages.Add($"Error: {ex.Message}");
            }
            finally
            {
                InputBox.IsEnabled = true;
                InputBox.Focus();
                // Scroll to bottom
                if (ChatHistory.Items.Count > 0)
                {
                    ChatHistory.ScrollIntoView(ChatHistory.Items[ChatHistory.Items.Count - 1]);
                }
            }
        }
    }
}
