# Revit AI Architect

A Revit Add-in that integrates AI capabilities directly into the Revit environment.

## Features

- **AI Chat Assistant:** Ask questions and get answers about Revit or general construction topics without leaving the software.
- **WPF Interface:** Modern, responsive chat interface.
- **Extensible Service:** Built with a modular `AiService` that can be configured to use OpenAI or other providers.

## Getting Started

### Prerequisites

- Autodesk Revit 2026 (or compatible version).
- Visual Studio 2022 (with .NET Desktop Development workload).
- OpenAI API Key.

### Installation

1. Clone this repository.
2. Open `RevitAIArchitect.sln` in Visual Studio.
3. Build the solution.
4. Run the `scripts/build_and_deploy.ps1` script (or manually copy files to `Addins` folder).

## Usage

1. Open Revit 2026.
2. Navigate to the **Add-Ins** tab.
3. Click on **Ask AI**.
4. Type your query in the chat window.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
