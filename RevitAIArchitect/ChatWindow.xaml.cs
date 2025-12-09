using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace RevitAIArchitect
{
    public partial class ChatWindow : Window
    {
        private readonly AiService _aiService;
        public ObservableCollection<string> Messages { get; set; }
        
        // Path to save API key locally
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RevitAIArchitect",
            "settings.txt"
        );

        public ChatWindow()
        {
            InitializeComponent();
            _aiService = new AiService();
            Messages = new ObservableCollection<string>();
            ChatHistory.ItemsSource = Messages;
            
            // Load saved API key if exists
            LoadApiKey();
            
            // Add initial welcome message
            if (string.IsNullOrEmpty(_aiService.ApiKey))
            {
                Messages.Add("AI: Welcome! Please enter your OpenAI API Key above to get started.");
            }
            else
            {
                Messages.Add("AI: Hello! How can I help you with your Revit model today?");
            }
        }

        private void LoadApiKey()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string savedKey = File.ReadAllText(SettingsPath).Trim();
                    if (!string.IsNullOrEmpty(savedKey))
                    {
                        _aiService.ApiKey = savedKey;
                        ApiKeyBox.Password = savedKey;
                    }
                }
            }
            catch
            {
                // Ignore errors loading settings
            }
        }

        private void SaveKeyButton_Click(object sender, RoutedEventArgs e)
        {
            string apiKey = ApiKeyBox.Password.Trim();
            if (string.IsNullOrEmpty(apiKey))
            {
                MessageBox.Show("Please enter an API key.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Ensure directory exists
                string dir = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // Save API key
                File.WriteAllText(SettingsPath, apiKey);
                _aiService.ApiKey = apiKey;

                Messages.Add("System: API Key saved successfully!");
            }
            catch (Exception ex)
            {
                Messages.Add($"Error saving API Key: {ex.Message}");
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            // Check API key first
            if (string.IsNullOrEmpty(_aiService.ApiKey))
            {
                Messages.Add("System: Please enter and save your API Key first.");
                return;
            }

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
