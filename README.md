# RevitAIArchitect

AI-powered assistant for Autodesk Revit with multi-provider support.

---

## Features
- Multi-AI Provider: OpenAI (GPT-4o) and Google Gemini
- Model Selection per provider
- Revit Context: project info, element counts, warnings, selection info
- Project Verification: automated checks + AI analysis with priorities
- Command Execution with Safety: select/delete/rename/set_parameter + hide/isolate/override_color/open_view (validation + confirmation)
- Persistent Settings stored locally
- Build & Deploy Script includes `dotnet test` before deploy

## Supported Models
**OpenAI**
- gpt-4o (default)
- gpt-4o-mini
- gpt-4-turbo
- gpt-3.5-turbo

**Google Gemini**
- gemini-3-pro-preview (default)
- gemini-3-pro-image-preview
- gemini-2.5-flash

## Context Data (Include Revit Context)
When enabled, AI receives:
- Project: name, file path, Revit version
- Active view: view name and phase (when available)
- Units: length unit
- Elements: count by category (Walls, Doors, Windows, Floors, etc.)
- Warnings: top warning messages with counts
- Selection: selected elements (Category/Name/Type/ID, up to 10 items)

## Project Structure
```
RevitAIArchitect/
├─ RevitAIArchitect/               # Add-in source
│  ├─ Command.cs                   # IExternalCommand entry point
│  ├─ ChatWindow.xaml(.cs)         # WPF chat UI
│  ├─ IAiProvider.cs               # Provider interface
│  ├─ OpenAiProvider.cs            # OpenAI implementation
│  ├─ GeminiProvider.cs            # Gemini implementation
│  ├─ AiCommand.cs                 # AI command schema/validation
│  ├─ RevitCommandExecutor.cs      # Executes AI commands in Revit
│  ├─ RevitContextService.cs       # Extracts context/verification info
│  └─ RevitAIArchitect.csproj
├─ RevitAIArchitect.Tests/         # Unit tests (xUnit)
├─ scripts/
│  └─ build_and_deploy.ps1         # Build/test/deploy script
├─ docs/
│  ├─ USER_GUIDE.md                # English guide
│  └─ USER_GUIDE.th.md             # Thai guide
└─ README.md
```

## Requirements
- .NET 8 SDK (Windows x64)
- Autodesk Revit 2026
- API Key from OpenAI or Google AI Studio

## Build
```powershell
cd RevitAIArchitect
dotnet build
```

## Install / Deploy
### Script (Recommended)
```powershell
.\scripts\build_and_deploy.ps1
```

### Manual
1. Build the solution
2. Copy `RevitAIArchitect.dll` to `%AppData%\Autodesk\Revit\Addins\2026\`
3. Copy `.addin` manifest to the same folder
4. Restart Revit

## Usage
1. Open Revit → Add-Ins → External Tools → Ask AI
2. Select AI Provider and Model
3. Enter API Key and click Save
4. Tick **Include Revit Context** if you want to share project context
5. Send your question or request

### Verify Project
1. Tick **Include Revit Context**
2. Click **Verify Project**
3. View report with warnings (by type), room issues, duplicate Type Marks, and priorities
4. AI provides analysis and recommendations

## Development Notes
- Settings stored in `%AppData%\RevitAIArchitect\`
- API keys: `openai_key.txt`, `gemini_key.txt`
- Model selections: `openai_model.txt`, `gemini_model.txt`
- Revit API references set to `Private=false`

## Testing
Run the full suite:
```powershell
dotnet test
```
Includes provider defaults, model selection, API key handling, and AI command validation.

## License
MIT License - see [LICENSE](LICENSE).
