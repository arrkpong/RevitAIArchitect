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
        private readonly RevitCommandExecutor _commandExecutor;

        public ObservableCollection<string> Messages { get; set; }

        // Path to save settings locally
        private static readonly string SettingsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RevitAIArchitect"
        );
        private static readonly string OpenAiKeyPath = Path.Combine(SettingsDir, "openai_key.txt");
        private static readonly string GeminiKeyPath = Path.Combine(SettingsDir, "gemini_key.txt");
        private static readonly string ProviderPath = Path.Combine(SettingsDir, "provider.txt");
        private static readonly string OpenAiModelPath = Path.Combine(SettingsDir, "openai_model.txt");
        private static readonly string GeminiModelPath = Path.Combine(SettingsDir, "gemini_model.txt");

        public ChatWindow() : this(null) { }

        public ChatWindow(UIDocument? uidoc)
        {
            InitializeComponent();

            _openAiProvider = new OpenAiProvider();
            _geminiProvider = new GeminiProvider();
            _currentProvider = _openAiProvider; // Default
            _contextService = new RevitContextService(uidoc);
            _commandExecutor = new RevitCommandExecutor(uidoc);

            Messages = new ObservableCollection<string>();
            ChatHistory.ItemsSource = Messages;

            // Load saved settings
            LoadSettings();

            // Add initial welcome message
            string modelInfo = GetCurrentModelInfo();
            string contextInfo = _contextService.HasDocument ? " [Revit Context Available]" : " [No Revit Document]";
            Messages.Add($"AI: Welcome! Using {_currentProvider.Name} ({modelInfo}).{contextInfo}");
        }

        private string GetCurrentModelInfo()
        {
            if (_currentProvider is OpenAiProvider op)
                return op.Model;
            if (_currentProvider is GeminiProvider gp)
                return gp.Model;
            return "Unknown";
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
                    }
                    else
                    {
                        ProviderCombo.SelectedIndex = 0;
                        _currentProvider = _openAiProvider;
                    }
                }
                else
                {
                    ProviderCombo.SelectedIndex = 0;
                }

                // Load OpenAI model
                if (File.Exists(OpenAiModelPath))
                {
                    _openAiProvider.Model = File.ReadAllText(OpenAiModelPath).Trim();
                }

                // Load Gemini model
                if (File.Exists(GeminiModelPath))
                {
                    _geminiProvider.Model = File.ReadAllText(GeminiModelPath).Trim();
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

                // Update UI
                UpdateModelDropdown();
                UpdateApiKeyDisplay();
            }
            catch
            {
                ProviderCombo.SelectedIndex = 0;
                UpdateModelDropdown();
            }
        }

        private void UpdateModelDropdown()
        {
            ModelCombo.Items.Clear();
            
            if (_currentProvider is OpenAiProvider)
            {
                foreach (var model in OpenAiProvider.AvailableModels)
                {
                    var item = new ComboBoxItem
                    {
                        Content = GetOpenAiModelDisplayName(model),
                        Tag = model
                    };
                    ModelCombo.Items.Add(item);
                    if (model == _openAiProvider.Model)
                        ModelCombo.SelectedItem = item;
                }
            }
            else
            {
                foreach (var model in GeminiProvider.AvailableModels)
                {
                    var item = new ComboBoxItem
                    {
                        Content = GetGeminiModelDisplayName(model),
                        Tag = model
                    };
                    ModelCombo.Items.Add(item);
                    if (model == _geminiProvider.Model)
                        ModelCombo.SelectedItem = item;
                }
            }

            if (ModelCombo.SelectedItem == null && ModelCombo.Items.Count > 0)
                ModelCombo.SelectedIndex = 0;
        }

        private string GetOpenAiModelDisplayName(string model)
        {
            return model switch
            {
                "gpt-4o" => "GPT-4o (Latest)",
                "gpt-4o-mini" => "GPT-4o Mini (Fast)",
                "gpt-4-turbo" => "GPT-4 Turbo",
                "gpt-3.5-turbo" => "GPT-3.5 Turbo (Legacy)",
                _ => model
            };
        }

        private string GetGeminiModelDisplayName(string model)
        {
            return model switch
            {
                "gemini-3-pro-preview" => "Gemini 3 Pro (Latest)",
                "gemini-3-pro-image-preview" => "Gemini 3 Pro Image",
                "gemini-2.5-flash" => "Gemini 2.5 Flash (Stable)",
                _ => model
            };
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
                }
                else
                {
                    _currentProvider = _openAiProvider;
                }
                
                UpdateModelDropdown();
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
                string model = item.Tag?.ToString() ?? "";
                
                if (_currentProvider is OpenAiProvider)
                {
                    _openAiProvider.Model = model;
                    try
                    {
                        EnsureSettingsDir();
                        File.WriteAllText(OpenAiModelPath, model);
                    }
                    catch { }
                }
                else
                {
                    _geminiProvider.Model = model;
                    try
                    {
                        EnsureSettingsDir();
                        File.WriteAllText(GeminiModelPath, model);
                    }
                    catch { }
                }
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
            if (string.IsNullOrEmpty(_currentProvider.ApiKey))
            {
                Messages.Add("System: Please enter and save your API Key first.");
                return;
            }

            string userMessage = InputBox.Text;
            if (string.IsNullOrWhiteSpace(userMessage)) return;

            Messages.Add($"You: {userMessage}");
            InputBox.Clear();
            InputBox.IsEnabled = false;
            SendButton.IsEnabled = false;

            try
            {
                string? context = null;
                if (IncludeContextCheck.IsChecked == true && _contextService.HasDocument)
                {
                    context = _contextService.BuildContextString();
                    context += _contextService.GetSelectionInfo();
                }

                string modelInfo = GetCurrentModelInfo();
                string contextIndicator = context != null ? " [with context]" : "";
                
                string aiOutput = await _currentProvider.GetReplyAsync(userMessage, context);
                
                // Parse response for commands
                var aiResponse = AiResponse.Parse(aiOutput);
                
                // Show AI message
                Messages.Add($"AI ({_currentProvider.Name} [{modelInfo}]){contextIndicator}: {aiResponse.Message}");
                
                // Process command if present
                if (aiResponse.Command != null)
                {
                    await ProcessCommand(aiResponse.Command);
                }
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
                if (ChatHistory.Items.Count > 0)
                {
                    ChatHistory.ScrollIntoView(ChatHistory.Items[ChatHistory.Items.Count - 1]);
                }
            }
        }

        private async System.Threading.Tasks.Task ProcessCommand(AiCommand command)
        {
            if (!_commandExecutor.HasDocument)
            {
                Messages.Add("Cannot execute command: No Revit document open.");
                return;
            }

            // Show command info
            string elemCount = command.ElementIds?.Count.ToString() ?? "0";
            Messages.Add($"Command detected: {command.Action.ToUpper()} ({elemCount} elements)");
            Messages.Add($"Description: {command.Description}");

            // Ask for confirmation if needed
            if (command.RequiresConfirmation)
            {
                var result = MessageBox.Show(
                    $"AI wants to perform: {command.Action.ToUpper()}\n\n" +
                    $"Description: {command.Description}\n" +
                    $"Elements: {elemCount}\n" +
                    $"Risk Level: {command.RiskLevel}\n\n" +
                    "Do you want to proceed?",
                    "Confirm AI Action",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result != MessageBoxResult.Yes)
                {
                    Messages.Add("Command cancelled by user.");
                    return;
                }
            }

            // Execute command
            Messages.Add("Executing command...");
            var execResult = _commandExecutor.Execute(command);
            
            if (execResult.Success)
            {
                Messages.Add(execResult.Message);
            }
            else
            {
                Messages.Add($"Error: {execResult.Message}");
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
                string verificationReport = _contextService.RunVerificationReport();
                Messages.Add($"Verification Report:\n{verificationReport}");

                string aiPrompt = "Based on the verification report below, please provide:\n" +
                                  "1. A summary of the main issues found\n" +
                                  "2. Priority recommendations to fix them\n" +
                                  "3. Best practices to prevent these issues\n\n" +
                                  verificationReport;

                string modelInfo = GetCurrentModelInfo();
                string aiResponse = await _currentProvider.GetReplyAsync(aiPrompt, null);
                Messages.Add($"AI Analysis ({_currentProvider.Name} [{modelInfo}]): {aiResponse}");
            }
            catch (Exception ex)
            {
                Messages.Add($"Error during verification: {ex.Message}");
            }
            finally
            {
                VerifyButton.IsEnabled = true;
                SendButton.IsEnabled = true;
                if (ChatHistory.Items.Count > 0)
                {
                    ChatHistory.ScrollIntoView(ChatHistory.Items[ChatHistory.Items.Count - 1]);
                }
            }
        }
    }
}
