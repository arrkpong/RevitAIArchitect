# RevitAIArchitect

<div align="center">

![Revit](https://img.shields.io/badge/Revit-2026-blue?style=for-the-badge&logo=autodesk)
![.NET](https://img.shields.io/badge/.NET-8.0-purple?style=for-the-badge&logo=dotnet)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

**AI-powered assistant for Autodesk Revit with multi-provider support**

</div>

---

## ✨ Features

- 🤖 **Multi-AI Provider** - OpenAI (GPT-4o) and Google Gemini support
- 🔄 **Model Selection** - Choose from multiple models per provider
- 📋 **Revit Context** - AI can access project info, element counts, warnings
- 🔍 **Project Verification** - Automated checks with AI analysis
- 💾 **Persistent Settings** - API keys and preferences saved locally
- 🎨 **Modern UI** - Clean WPF chat interface

## 🤖 Supported Models

### OpenAI

| Model           | Description               |
| --------------- | ------------------------- |
| `gpt-4o`        | Latest flagship (Default) |
| `gpt-4o-mini`   | Fast & affordable         |
| `gpt-4-turbo`   | Previous flagship         |
| `gpt-3.5-turbo` | Legacy                    |

### Google Gemini

| Model                        | Description      |
| ---------------------------- | ---------------- |
| `gemini-3-pro-preview`       | Latest (Default) |
| `gemini-3-pro-image-preview` | Image generation |
| `gemini-2.5-flash`           | Stable & fast    |

## 📋 Context Data

When enabled, AI receives:
| Data | Description |
|------|-------------|
| `Project` | Name and file path |
| `Elements` | Count by category (Walls, Doors, etc.) |
| `Warnings` | Top warnings with element IDs |
| `Selection` | Currently selected elements |

## 📁 Project Structure

```
RevitAIArchitect/
├── RevitAIArchitect/
│   ├── Command.cs              # IExternalCommand entry point
│   ├── ChatWindow.xaml         # WPF chat interface
│   ├── ChatWindow.xaml.cs      # UI logic
│   ├── IAiProvider.cs          # Provider interface
│   ├── OpenAiProvider.cs       # OpenAI implementation
│   ├── GeminiProvider.cs       # Gemini implementation
│   ├── RevitContextService.cs  # Revit data extraction
│   └── RevitAIArchitect.csproj
├── RevitAIArchitect.Tests/     # Unit tests (xUnit)
├── scripts/
│   └── build_and_deploy.ps1    # Build & deploy script
├── docs/
│   ├── USER_GUIDE.md           # English user guide
│   └── USER_GUIDE.th.md        # Thai user guide
└── README.md
```

## 🔧 Requirements

- **.NET 8 SDK** (Windows x64)
- **Autodesk Revit 2026**
- **API Key** from OpenAI or Google AI Studio

## 🚀 Build

```powershell
cd RevitAIArchitect
dotnet build
```

## 📦 Install / Deploy

### Option 1: Script (Recommended)

```powershell
.\scripts\build_and_deploy.ps1
```

### Option 2: Manual

1. Build the solution
2. Copy `RevitAIArchitect.dll` to `%AppData%\Autodesk\Revit\Addins\2026\`
3. Copy `.addin` manifest to the same folder
4. Restart Revit

## 🎮 Usage

1. Open Revit → **Add-Ins** → **External Tools** → **Ask AI**
2. Select **AI Provider** (OpenAI or Gemini)
3. Select **Model**
4. Enter **API Key** and click Save
5. Start chatting!

### Verify Project

1. Check ✅ **Include Revit Context**
2. Click **🔍 Verify Project**
3. View report with:
   - Warnings and affected elements
   - Rooms without numbers
   - Duplicate Type Marks
4. AI provides analysis and recommendations

## 🛠 Development Notes

- Settings stored in `%AppData%\RevitAIArchitect\`
- API Key files: `openai_key.txt`, `gemini_key.txt`
- Model selection files: `openai_model.txt`, `gemini_model.txt`
- Revit API references set to `Private=false`

## 🧪 Testing

```powershell
dotnet test
```

14 unit tests covering:

- Provider names and defaults
- Model selection
- API key handling

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details.
