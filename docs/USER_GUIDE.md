# User Guide

This guide explains how to install and use the Revit AI Architect add-in.

## Installation

### Prerequisites
- Autodesk Revit 2026 (or compatible)
- Windows 10/11
- .NET 8 SDK (for building yourself)

### Option 1: Script (Recommended)
1. Open PowerShell in the project folder.
2. Run:
   ```powershell
   .\scripts\build_and_deploy.ps1
   ```
3. The script builds, runs tests, and deploys the add-in.

### Option 2: Manual
1. Build the project (Debug/Release).
2. Copy `RevitAIArchitect.dll` from `bin\<Config>\net8.0-windows\` to:
   ```
   %APPDATA%\Autodesk\Revit\Addins\2026\
   ```
3. Copy the generated `.addin` manifest to the same folder.
4. Restart Revit.

## Usage

### Opening the Chat
1. Launch Revit.
2. Go to **Add-Ins** → **External Tools** → **Ask AI**.
3. The chat window appears.

### Chatting with AI
- Choose **AI Provider** (OpenAI or Google Gemini) and **Model**.
- Enter your **API Key** and click **Save** (keys and model choice are stored locally in `%APPDATA%\RevitAIArchitect`).
- Type your question or request and click **Send**.

### Include Revit Context
- Check **Include Revit Context** to send: project name/path/version, active view/phase, length unit, element counts, top warnings, and selected elements (up to 10).

### Verify Project
1. Check **Include Revit Context**.
2. Click **Verify Project**.
3. Review the report: warning types with samples, rooms without numbers, unplaced rooms, duplicate Type Marks, and a priority summary.
4. AI returns recommendations based on the report.

## Supported Actions (AI Commands)
- select (no confirmation)
- delete, rename, set_parameter (confirmation required)
- hide, isolate, override_color (confirmation required; color = `#RRGGBB` or `R,G,B`)
- open_view (switch to view by ElementId)
- All commands validate element IDs and required fields; very large element lists are rejected.

## Troubleshooting
- Add-in not appearing: ensure `.addin` and DLL paths are in `%APPDATA%\Autodesk\Revit\Addins\2026\`, then restart Revit.
- API key errors: verify you saved the key in the UI, not in code.
- Build/deploy failures: run `dotnet build` and `dotnet test` to check errors; confirm Revit 2026 API references are present.

## Testing
```powershell
dotnet test
```
Includes provider defaults, model selection, API key handling, and AI command validation.
