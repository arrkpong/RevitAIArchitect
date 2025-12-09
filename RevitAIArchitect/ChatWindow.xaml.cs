using Autodesk.Revit.UI;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace RevitAIArchitect
{
    public partial class ChatWindow : Window
    {
        private IAiProvider _currentProvider;
        private readonly OpenAiProvider _openAiProvider;
        private readonly GeminiProvider _geminiProvider;
        private readonly RevitContextService _contextService;

        public ObservableCollection<string> Messages { get; set; }

        // Path to save settings locally
        private static readonly string SettingsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RevitAIArchitect"
        );
        private static readonly string OpenAiKeyPath = Path.Combine(SettingsDir, "openai_key.txt");
        private static readonly string GeminiKeyPath = Path.Combine(SettingsDir, "gemini_key.txt");
        private static readonly string ProviderPath = Path.Combine(SettingsDir, "provider.txt");
        private static readonly string GeminiModelPath = Path.Combine(SettingsDir, "gemini_model.txt");

        public ChatWindow() : this(null) { }

        public ChatWindow(UIDocument? uidoc)
        {
            InitializeComponent();

            _openAiProvider = new OpenAiProvider();
            _geminiProvider = new GeminiProvider();
            _currentProvider = _openAiProvider; // Default
            _contextService = new RevitContextService(uidoc);

            Messages = new ObservableCollection<string>();
            ChatHistory.ItemsSource = Messages;

            // Load saved settings
            LoadSettings();

            // Add initial welcome message
            string modelInfo = _currentProvider is GeminiProvider gp ? $" (Model: {gp.Model})" : "";
            string contextInfo = _contextService.HasDocument ? " [Revit Context Available]" : " [No Revit Document]";
            Messages.Add($"AI: Welcome! Using {_currentProvider.Name}{modelInfo}.{contextInfo}");
        }

        private void LoadSettings()
        {
            try
            {
                // Load provider selection
                if (File.Exists(ProviderPath))
                {
                    string provider = File.ReadAllText(ProviderPath).Trim();
                    if (provider == "gemini")
                    {
                        ProviderCombo.SelectedIndex = 1;
                        _currentProvider = _geminiProvider;
                        ModelSection.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ProviderCombo.SelectedIndex = 0;
                        _currentProvider = _openAiProvider;
                        ModelSection.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    ProviderCombo.SelectedIndex = 0;
                    ModelSection.Visibility = Visibility.Collapsed;
                }

                // Load Gemini model selection
                if (File.Exists(GeminiModelPath))
                {
                    string model = File.ReadAllText(GeminiModelPath).Trim();
                    _geminiProvider.Model = model;
                    
                    // Set combo box selection
                    for (int i = 0; i < ModelCombo.Items.Count; i++)
                    {
                        if (ModelCombo.Items[i] is ComboBoxItem item && item.Tag?.ToString() == model)
                        {
                            ModelCombo.SelectedIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    ModelCombo.SelectedIndex = 0; // Default to first (gemini-3-pro-preview)
                }

                // Load OpenAI key
                if (File.Exists(OpenAiKeyPath))
                {
                    _openAiProvider.ApiKey = File.ReadAllText(OpenAiKeyPath).Trim();
                }

                // Load Gemini key
                if (File.Exists(GeminiKeyPath))
                {
                    _geminiProvider.ApiKey = File.ReadAllText(GeminiKeyPath).Trim();
                }

                // Show current provider's key in the box
                UpdateApiKeyDisplay();
            }
            catch
            {
                ProviderCombo.SelectedIndex = 0;
                ModelSection.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateApiKeyDisplay()
        {
            if (_currentProvider is OpenAiProvider)
            {
                ApiKeyBox.Password = _openAiProvider.ApiKey;
            }
            else
            {
                ApiKeyBox.Password = _geminiProvider.ApiKey;
            }
        }

        private void ProviderCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProviderCombo.SelectedItem is ComboBoxItem item)
            {
                string tag = item.Tag?.ToString() ?? "openai";
                if (tag == "gemini")
                {
                    _currentProvider = _geminiProvider;
                    ModelSection.Visibility = Visibility.Visible;
                }
                else
                {
                    _currentProvider = _openAiProvider;
                    ModelSection.Visibility = Visibility.Collapsed;
                }
                UpdateApiKeyDisplay();

                // Save provider selection
                try
                {
                    EnsureSettingsDir();
                    File.WriteAllText(ProviderPath, tag);
                }
                catch { }
            }
        }

        private void ModelCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModelCombo.SelectedItem is ComboBoxItem item)
            {
                string model = item.Tag?.ToString() ?? "gemini-3-pro-preview";
                _geminiProvider.Model = model;

                // Save model selection
                try
                {
                    EnsureSettingsDir();
                    File.WriteAllText(GeminiModelPath, model);
                }
                catch { }
            }
        }

        private void EnsureSettingsDir()
        {
            if (!Directory.Exists(SettingsDir))
            {
                Directory.CreateDirectory(SettingsDir);
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
                EnsureSettingsDir();

                // Save to correct file based on provider
                if (_currentProvider is OpenAiProvider)
                {
                    File.WriteAllText(OpenAiKeyPath, apiKey);
                    _openAiProvider.ApiKey = apiKey;
                }
                else
                {
                    File.WriteAllText(GeminiKeyPath, apiKey);
                    _geminiProvider.ApiKey = apiKey;
                }

                Messages.Add($"System: {_currentProvider.Name} API Key saved successfully!");
            }
            catch (Exception ex)
            {
                Messages.Add($"Error saving API Key: {ex.Message}");
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            // Check API key first
            if (string.IsNullOrEmpty(_currentProvider.ApiKey))
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
            SendButton.IsEnabled = false;

            try
            {
                // Build context if enabled
                string? context = null;
                if (IncludeContextCheck.IsChecked == true && _contextService.HasDocument)
                {
                    context = _contextService.BuildContextString();
                    context += _contextService.GetSelectionInfo();
                }

                // Show model info for Gemini
                string modelInfo = _currentProvider is GeminiProvider gp ? $" [{gp.Model}]" : "";
                string contextIndicator = context != null ? " 📋" : "";
                
                // Call AI Service with context
                string aiResponse = await _currentProvider.GetReplyAsync(userMessage, context);
                Messages.Add($"AI ({_currentProvider.Name}{modelInfo}){contextIndicator}: {aiResponse}");
            }
            catch (Exception ex)
            {
                Messages.Add($"Error: {ex.Message}");
            }
            finally
            {
                InputBox.IsEnabled = true;
                SendButton.IsEnabled = true;
                InputBox.Focus();
                // Scroll to bottom
                if (ChatHistory.Items.Count > 0)
                {
                    ChatHistory.ScrollIntoView(ChatHistory.Items[ChatHistory.Items.Count - 1]);
                }
            }
        }

        private async void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_contextService.HasDocument)
            {
                Messages.Add("System: No Revit document open. Please open a project first.");
                return;
            }

            if (string.IsNullOrEmpty(_currentProvider.ApiKey))
            {
                Messages.Add("System: Please enter and save your API Key first.");
                return;
            }

            Messages.Add("System: Running verification...");
            VerifyButton.IsEnabled = false;
            SendButton.IsEnabled = false;

            try
            {
                // Get verification report from Revit
                string verificationReport = _contextService.RunVerificationReport();
                
                // Show raw report first
                Messages.Add($"📋 Verification Report:\n{verificationReport}");

                // Ask AI to analyze and provide recommendations
                string aiPrompt = "Based on the verification report below, please provide:\n" +
                                  "1. A summary of the main issues found\n" +
                                  "2. Priority recommendations to fix them\n" +
                                  "3. Best practices to prevent these issues\n\n" +
                                  verificationReport;

                string modelInfo = _currentProvider is GeminiProvider gp ? $" [{gp.Model}]" : "";
                string aiResponse = await _currentProvider.GetReplyAsync(aiPrompt, null);
                Messages.Add($"AI Analysis ({_currentProvider.Name}{modelInfo}): {aiResponse}");
            }
            catch (Exception ex)
            {
                Messages.Add($"Error during verification: {ex.Message}");
            }
            finally
            {
                VerifyButton.IsEnabled = true;
                SendButton.IsEnabled = true;
                // Scroll to bottom
                if (ChatHistory.Items.Count > 0)
                {
                    ChatHistory.ScrollIntoView(ChatHistory.Items[ChatHistory.Items.Count - 1]);
                }
            }
        }
    }
}
