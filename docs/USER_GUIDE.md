# User Guide

This guide explains how to install and use the Revit AI Architect add-in.

## Installation

### Prerequisites

- Autodesk Revit 2026 (or compatible version)
- Windows 10/11

### Option 1: Using the Script (Recommended)

1. Open PowerShell in the project folder
2. Run the deployment script:
   ```powershell
   .\scripts\build_and_deploy.ps1
   ```
3. The script will automatically build and deploy the add-in

### Option 2: Manual Installation

1. Build the project in Visual Studio (Debug or Release)
2. Copy `RevitAIArchitect.dll` from `bin\Debug\net8.0-windows\` to:
   ```
   %APPDATA%\Autodesk\Revit\Addins\2026\
   ```
3. Create a `.addin` manifest file or copy the generated one

## Usage

### Opening the AI Chat

1. Launch Revit 2026
2. Go to the **Add-Ins** tab in the ribbon
3. Click **External Tools** → **Ask AI**
4. The chat window will appear

### Chatting with AI

- Type your question in the input box at the bottom
- Press **Send** or hit Enter
- Wait for the AI response to appear

### Example Questions

- "What is the best practice for creating walls in Revit?"
- "How do I export a schedule to Excel?"
- "Explain the difference between hosted and non-hosted families"

## Configuration

### Setting Your API Key

1. Open `AiService.cs` in the project
2. Find the line:
   ```csharp
   private const string ApiKey = "YOUR_OPENAI_API_KEY_HERE";
   ```
3. Replace with your actual OpenAI API key
4. Rebuild the project

### Changing AI Model

By default, the add-in uses `gpt-4o`. To change:

1. Open `AiService.cs`
2. Find `model = "gpt-4o"`
3. Change to your preferred model (e.g., `gpt-3.5-turbo`)

## Troubleshooting

### Add-in not appearing in Revit

- Ensure the `.addin` file is in the correct folder
- Check that the DLL path in the `.addin` file is correct
- Restart Revit

### "API Key not found" error

- Verify your API key is correctly set in `AiService.cs`
- Rebuild and redeploy the project

### Chat window crashes

- Check the Revit journal file for errors
- Ensure you have internet connectivity
