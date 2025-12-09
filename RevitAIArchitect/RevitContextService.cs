using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RevitAIArchitect
{
    /// <summary>
    /// Service to extract context information from the current Revit document.
    /// </summary>
    public class RevitContextService
    {
        private readonly Document _doc;
        private readonly UIDocument _uidoc;

        public RevitContextService(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = uidoc?.Document;
        }

        /// <summary>
        /// Check if we have a valid document.
        /// </summary>
        public bool HasDocument => _doc != null;

        /// <summary>
        /// Build a context string with project information for the AI.
        /// </summary>
        public string BuildContextString()
        {
            if (_doc == null)
                return "[No Revit document open]";

            var sb = new StringBuilder();
            sb.AppendLine("=== REVIT PROJECT CONTEXT ===");
            
            // Project Info
            sb.AppendLine($"Project: {_doc.Title}");
            sb.AppendLine($"Path: {_doc.PathName ?? "Not saved"}");
            
            // Units
            try
            {
                var units = _doc.GetUnits();
                var lengthSpec = units.GetFormatOptions(SpecTypeId.Length);
                sb.AppendLine($"Length Unit: {lengthSpec.GetUnitTypeId().TypeId}");
            }
            catch { sb.AppendLine("Units: Unknown"); }

            sb.AppendLine();

            // Element Counts
            sb.AppendLine("Element Counts:");
            var categories = new Dictionary<BuiltInCategory, string>
            {
                { BuiltInCategory.OST_Walls, "Walls" },
                { BuiltInCategory.OST_Doors, "Doors" },
                { BuiltInCategory.OST_Windows, "Windows" },
                { BuiltInCategory.OST_Floors, "Floors" },
                { BuiltInCategory.OST_Roofs, "Roofs" },
                { BuiltInCategory.OST_Columns, "Columns" },
                { BuiltInCategory.OST_StructuralColumns, "Structural Columns" },
                { BuiltInCategory.OST_StructuralFraming, "Beams" },
                { BuiltInCategory.OST_Rooms, "Rooms" },
                { BuiltInCategory.OST_Furniture, "Furniture" }
            };

            foreach (var cat in categories)
            {
                try
                {
                    var count = new FilteredElementCollector(_doc)
                        .OfCategory(cat.Key)
                        .WhereElementIsNotElementType()
                        .GetElementCount();
                    if (count > 0)
                        sb.AppendLine($"- {cat.Value}: {count}");
                }
                catch { }
            }

            sb.AppendLine();

            // Warnings
            try
            {
                var warnings = _doc.GetWarnings();
                if (warnings.Count > 0)
                {
                    sb.AppendLine($"Warnings ({warnings.Count} total):");
                    
                    // Group by description and show top 5
                    var groupedWarnings = warnings
                        .GroupBy(w => w.GetDescriptionText())
                        .OrderByDescending(g => g.Count())
                        .Take(5);

                    int index = 1;
                    foreach (var group in groupedWarnings)
                    {
                        sb.AppendLine($"{index}. {group.Key} ({group.Count()})");
                        index++;
                    }
                }
                else
                {
                    sb.AppendLine("Warnings: None");
                }
            }
            catch { sb.AppendLine("Warnings: Unable to retrieve"); }

            sb.AppendLine("===");

            return sb.ToString();
        }

        /// <summary>
        /// Get info about currently selected elements.
        /// </summary>
        public string GetSelectionInfo()
        {
            if (_uidoc == null) return string.Empty;

            try
            {
                var selectedIds = _uidoc.Selection.GetElementIds();
                if (selectedIds.Count == 0)
                    return string.Empty;

                var sb = new StringBuilder();
                sb.AppendLine($"\nSelected Elements ({selectedIds.Count}):");

                foreach (var id in selectedIds.Take(10)) // Limit to 10
                {
                    var elem = _doc.GetElement(id);
                    if (elem != null)
                    {
                        string typeName = elem.GetType().Name;
                        string name = elem.Name ?? "Unnamed";
                        sb.AppendLine($"- [{typeName}] {name} (ID: {id.Value})");
                    }
                }

                if (selectedIds.Count > 10)
                    sb.AppendLine($"... and {selectedIds.Count - 10} more");

                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
