# Documentation

This directory contains technical documentation for the Revit AI Architect project.

## Contents

- **[Architecture](Architecture.md):** (Planned) Overview of system architecture.
- **[API Reference](API.md):** (Planned) Details on the internal API services.

## Development Guide

### Adding New Commands

To add a new command to Revit, implement the `IExternalCommand` interface and register it in the `.addin` manifest file.

### AI Configuration

The `AiService` class manages communication with the LLM provider. currently hardcoded to OpenAI, but can be refactored to support local models or Azure OpenAI.
