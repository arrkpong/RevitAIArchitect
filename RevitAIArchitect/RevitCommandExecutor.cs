using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

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
