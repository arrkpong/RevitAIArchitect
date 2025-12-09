# AI Coding Agent Instructions

This file contains instructions for AI coding agents (like GitHub Copilot, Cursor, etc.) working on this project.

## Project Overview

- **Type:** Revit Add-in (Plugin)
- **Framework:** .NET 8.0 (net8.0-windows)
- **UI:** WPF (Windows Presentation Foundation)
- **Target:** Autodesk Revit 2026

## Key Files

- `Command.cs` - Revit entry point implementing `IExternalCommand`
- `ChatWindow.xaml/.cs` - WPF chat interface
- `AiService.cs` - AI communication service (OpenAI by default)
- `RevitAIArchitect.addin` - Revit manifest file

## Build & Deploy

```powershell
.\scripts\build_and_deploy.ps1
```

## Testing

```powershell
dotnet test
```

## Important Notes

- Revit API calls must happen in the valid Revit context.
- The `.addin` file is generated dynamically by the deploy script.
- Keep `Private=False` for Revit API references to avoid DLL conflicts.
