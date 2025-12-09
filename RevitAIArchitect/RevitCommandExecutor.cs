using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace RevitAIArchitect
{
    /// <summary>
    /// Executes AI commands in Revit via Transactions.
    /// </summary>
    public class RevitCommandExecutor
    {
        private readonly Document? _doc;
        private readonly UIDocument? _uidoc;

        public RevitCommandExecutor(UIDocument? uidoc)
        {
            _uidoc = uidoc;
            _doc = uidoc?.Document;
        }

        public bool HasDocument => _doc != null;

        /// <summary>
        /// Execute a command from AI.
        /// </summary>
        public CommandResult Execute(AiCommand command)
        {
            if (_doc == null || _uidoc == null)
                return new CommandResult(false, "No Revit document open.");

            try
            {
                return command.Action.ToLower() switch
                {
                    "select" => ExecuteSelect(command),
                    "delete" => ExecuteDelete(command),
                    "rename" => ExecuteRename(command),
                    "set_parameter" => ExecuteSetParameter(command),
                    "hide" => ExecuteHide(command),
                    "isolate" => ExecuteIsolate(command),
                    "override_color" => ExecuteOverrideColor(command),
                    "open_view" => ExecuteOpenView(command),
                    _ => new CommandResult(false, $"Unknown command: {command.Action}")
                };
            }
            catch (Exception ex)
            {
                return new CommandResult(false, $"Error: {ex.Message}");
            }
        }

        private CommandResult ExecuteSelect(AiCommand command)
        {
            if (command.ElementIds == null || command.ElementIds.Count == 0)
                return new CommandResult(false, "No element IDs provided.");

            var ids = command.ElementIds
                .Select(id => new ElementId(id))
                .Where(id => _doc!.GetElement(id) != null)
                .ToList();

            if (ids.Count == 0)
                return new CommandResult(false, "No valid elements found.");

            _uidoc!.Selection.SetElementIds(ids);
            return new CommandResult(true, $"Selected {ids.Count} element(s).");
        }

        private CommandResult ExecuteDelete(AiCommand command)
        {
            if (command.ElementIds == null || command.ElementIds.Count == 0)
                return new CommandResult(false, "No element IDs provided.");

            var ids = command.ElementIds
                .Select(id => new ElementId(id))
                .Where(id => _doc!.GetElement(id) != null)
                .ToList();

            if (ids.Count == 0)
                return new CommandResult(false, "No valid elements found.");

            using (Transaction tx = new Transaction(_doc, "AI: Delete Elements"))
            {
                tx.Start();
                _doc!.Delete(ids);
                tx.Commit();
            }

            return new CommandResult(true, $"Deleted {ids.Count} element(s). (Ctrl+Z to undo)");
        }

        private CommandResult ExecuteRename(AiCommand command)
        {
            if (command.ElementIds == null || command.ElementIds.Count == 0)
                return new CommandResult(false, "No element IDs provided.");

            if (string.IsNullOrEmpty(command.Value))
                return new CommandResult(false, "No new name provided.");

            var ids = command.ElementIds.Select(id => new ElementId(id)).ToList();
            int renamed = 0;

            using (Transaction tx = new Transaction(_doc, "AI: Rename Elements"))
            {
                tx.Start();
                foreach (var id in ids)
                {
                    var elem = _doc!.GetElement(id);
                    if (elem != null)
                    {
                        var nameParam = elem.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
                        if (nameParam != null && !nameParam.IsReadOnly)
                        {
                            nameParam.Set(command.Value);
                            renamed++;
                        }
                    }
                }
                tx.Commit();
            }

            return new CommandResult(true, $"Renamed {renamed} element(s).");
        }

        private CommandResult ExecuteSetParameter(AiCommand command)
        {
            if (command.ElementIds == null || command.ElementIds.Count == 0)
                return new CommandResult(false, "No element IDs provided.");

            if (string.IsNullOrEmpty(command.ParameterName))
                return new CommandResult(false, "No parameter name provided.");

            var ids = command.ElementIds.Select(id => new ElementId(id)).ToList();
            int updated = 0;

            using (Transaction tx = new Transaction(_doc, "AI: Set Parameter"))
            {
                tx.Start();
                foreach (var id in ids)
                {
                    var elem = _doc!.GetElement(id);
                    if (elem != null)
                    {
                        var param = elem.LookupParameter(command.ParameterName);
                        if (param != null && !param.IsReadOnly)
                        {
                            if (param.StorageType == StorageType.String)
                            {
                                param.Set(command.Value ?? "");
                                updated++;
                            }
                            else if (param.StorageType == StorageType.Double && double.TryParse(command.Value, out double dVal))
                            {
                                param.Set(dVal);
                                updated++;
                            }
                            else if (param.StorageType == StorageType.Integer && int.TryParse(command.Value, out int iVal))
                            {
                                param.Set(iVal);
                                updated++;
                            }
                        }
                    }
                }
                tx.Commit();
            }

            return new CommandResult(true, $"Updated parameter on {updated} element(s).");
        }

        private CommandResult ExecuteHide(AiCommand command)
        {
            if (command.ElementIds == null || command.ElementIds.Count == 0)
                return new CommandResult(false, "No element IDs provided.");
            if (_doc?.ActiveView == null)
                return new CommandResult(false, "No active view to hide elements.");

            var ids = command.ElementIds
                .Select(id => new ElementId(id))
                .Where(id => _doc!.GetElement(id) != null)
                .ToList();

            if (ids.Count == 0)
                return new CommandResult(false, "No valid elements found.");

            using (Transaction tx = new Transaction(_doc, "AI: Hide Elements"))
            {
                tx.Start();
                _doc.ActiveView.HideElements(ids);
                tx.Commit();
            }

            return new CommandResult(true, $"Hidden {ids.Count} element(s) in view.");
        }

        private CommandResult ExecuteIsolate(AiCommand command)
        {
            if (command.ElementIds == null || command.ElementIds.Count == 0)
                return new CommandResult(false, "No element IDs provided.");
            if (_doc?.ActiveView == null)
                return new CommandResult(false, "No active view to isolate.");

            var ids = command.ElementIds
                .Select(id => new ElementId(id))
                .Where(id => _doc!.GetElement(id) != null)
                .ToList();

            if (ids.Count == 0)
                return new CommandResult(false, "No valid elements found.");

            using (Transaction tx = new Transaction(_doc, "AI: Isolate Elements"))
            {
                tx.Start();
                _uidoc!.ActiveView.IsolateElementsTemporary(ids);
                tx.Commit();
            }

            return new CommandResult(true, $"Isolated {ids.Count} element(s) temporarily.");
        }

        private CommandResult ExecuteOverrideColor(AiCommand command)
        {
            if (command.ElementIds == null || command.ElementIds.Count == 0)
                return new CommandResult(false, "No element IDs provided.");
            if (string.IsNullOrWhiteSpace(command.Value))
                return new CommandResult(false, "Color value not provided.");
            if (!AiCommand.TryParseColor(command.Value, out var color))
                return new CommandResult(false, "Color must be hex (#RRGGBB) or R,G,B (0-255).");
            if (_doc?.ActiveView == null)
                return new CommandResult(false, "No active view to override graphics.");

            var ids = command.ElementIds
                .Select(id => new ElementId(id))
                .Where(id => _doc!.GetElement(id) != null)
                .ToList();

            if (ids.Count == 0)
                return new CommandResult(false, "No valid elements found.");

            var ogs = new OverrideGraphicSettings();
            ogs.SetProjectionLineColor(new Color(color.r, color.g, color.b));
            ogs.SetSurfaceForegroundPatternColor(new Color(color.r, color.g, color.b));

            using (Transaction tx = new Transaction(_doc, "AI: Override Color"))
            {
                tx.Start();
                foreach (var id in ids)
                {
                    _doc.ActiveView.SetElementOverrides(id, ogs);
                }
                tx.Commit();
            }

            return new CommandResult(true, $"Applied color override to {ids.Count} element(s).");
        }

        private CommandResult ExecuteOpenView(AiCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Value))
                return new CommandResult(false, "View ID not provided.");
            if (!int.TryParse(command.Value, out int viewIdInt))
                return new CommandResult(false, "View ID must be numeric.");

            var viewId = new ElementId(viewIdInt);
            var view = _doc?.GetElement(viewId) as View;
            if (view == null)
                return new CommandResult(false, $"No view found with ID {viewIdInt}.");

            if (_uidoc == null)
                return new CommandResult(false, "No UI document available.");

            bool changed = _uidoc.RequestViewChange(view);
            return changed
                ? new CommandResult(true, $"Switched to view: {view.Name} (ID:{viewIdInt}).")
                : new CommandResult(false, "Unable to switch view.");
        }
    }

    /// <summary>
    /// Result of command execution.
    /// </summary>
    public class CommandResult
    {
        public bool Success { get; }
        public string Message { get; }

        public CommandResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}
