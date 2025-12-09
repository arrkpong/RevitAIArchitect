# Revit AI Architect

A powerful Revit Add-in that integrates AI capabilities directly into the Revit environment.

## Features

- **Multi-AI Provider Support:** Choose between OpenAI (GPT-4o) and Google Gemini
- **Model Selection:** Select from multiple models for each provider
- **Revit Context Integration:** AI can access your project info (elements, warnings)
- **Project Verification:** Automated checks for common Revit issues
- **WPF Interface:** Modern, responsive chat interface
- **Persistent Settings:** API keys and preferences saved locally

## Supported Models

### OpenAI

- GPT-4o (Latest)
- GPT-4o Mini (Fast)
- GPT-4 Turbo
- GPT-3.5 Turbo (Legacy)

### Google Gemini

- Gemini 3 Pro (Latest)
- Gemini 3 Pro Image
- Gemini 2.5 Flash (Stable)

## Getting Started

### Prerequisites

- Autodesk Revit 2026
- Visual Studio 2022 (with .NET Desktop Development workload)
- OpenAI or Google Gemini API Key

### Installation

1. Clone this repository
2. Open `RevitAIArchitect.sln` in Visual Studio
3. Build the solution
4. Run `.\scripts\build_and_deploy.ps1`

### Usage

1. Open Revit 2026
2. Navigate to **Add-Ins** → **External Tools** → **Ask AI**
3. Select your AI Provider and Model
4. Enter your API Key and click Save
5. Start chatting!

### Verify Project

Click **🔍 Verify Project** to:

- Check for warnings with affected elements
- Find rooms without numbers
- Detect duplicate Type Marks
- Get AI recommendations for fixes

## License

MIT License - see LICENSE file for details.
