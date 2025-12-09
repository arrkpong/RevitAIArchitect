using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RevitAIArchitect
{
    /// <summary>
    /// Service to extract context and verification info from Revit document.
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

        public bool HasDocument => _doc != null;

        /// <summary>
        /// Build basic context string.
        /// </summary>
        public string BuildContextString()
        {
            if (_doc == null)
                return "[No Revit document open]";

            var sb = new StringBuilder();
            sb.AppendLine("=== REVIT PROJECT CONTEXT ===");
            
            sb.AppendLine($"Project: {_doc.Title}");
            sb.AppendLine($"Path: {_doc.PathName ?? "Not saved"}");
            
            try
            {
                var units = _doc.GetUnits();
                var lengthSpec = units.GetFormatOptions(SpecTypeId.Length);
                sb.AppendLine($"Length Unit: {lengthSpec.GetUnitTypeId().TypeId}");
            }
            catch { sb.AppendLine("Units: Unknown"); }

            sb.AppendLine();
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

            // Simple warnings summary
            try
            {
                var warnings = _doc.GetWarnings();
                if (warnings.Count > 0)
                {
                    sb.AppendLine($"Warnings ({warnings.Count} total):");
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
        /// Run comprehensive verification and return detailed report.
        /// </summary>
        public string RunVerificationReport()
        {
            if (_doc == null)
                return "[No Revit document open]";

            var sb = new StringBuilder();
            sb.AppendLine("=== VERIFICATION REPORT ===");
            sb.AppendLine($"Project: {_doc.Title}");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine();

            int totalIssues = 0;

            // 1. Warnings with Element IDs
            sb.AppendLine("--- WARNINGS ---");
            try
            {
                var warnings = _doc.GetWarnings();
                if (warnings.Count > 0)
                {
                    sb.AppendLine($"⚠️ Total Warnings: {warnings.Count}");
                    sb.AppendLine();

                    var groupedWarnings = warnings
                        .GroupBy(w => w.GetDescriptionText())
                        .OrderByDescending(g => g.Count())
                        .Take(10);

                    foreach (var group in groupedWarnings)
                    {
                        sb.AppendLine($"📌 {group.Key} ({group.Count()}):");
                        
                        // Show first 5 element IDs per warning type
                        int shown = 0;
                        foreach (var warning in group.Take(5))
                        {
                            var elementIds = warning.GetFailingElements();
                            foreach (var id in elementIds.Take(3))
                            {
                                var elem = _doc.GetElement(id);
                                string elemName = elem?.Name ?? "Unknown";
                                string category = elem?.Category?.Name ?? "Unknown";
                                sb.AppendLine($"   - [{category}] {elemName} (ID:{id.Value})");
                            }
                            shown++;
                        }
                        if (group.Count() > 5)
                            sb.AppendLine($"   ... and {group.Count() - 5} more");
                        sb.AppendLine();
                        totalIssues += group.Count();
                    }
                }
                else
                {
                    sb.AppendLine("✅ No warnings found");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"❌ Error checking warnings: {ex.Message}");
            }

            sb.AppendLine();

            // 2. Rooms without Numbers
            sb.AppendLine("--- ROOMS CHECK ---");
            try
            {
                var rooms = new FilteredElementCollector(_doc)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .Cast<Room>()
                    .ToList();

                var roomsWithoutNumber = rooms.Where(r => 
                    string.IsNullOrWhiteSpace(r.Number) || r.Number == "0").ToList();

                var unplacedRooms = rooms.Where(r => r.Area <= 0).ToList();

                if (roomsWithoutNumber.Count > 0)
                {
                    sb.AppendLine($"⚠️ Rooms without Number: {roomsWithoutNumber.Count}");
                    foreach (var room in roomsWithoutNumber.Take(10))
                    {
                        sb.AppendLine($"   - Room ID:{room.Id.Value} (Name: {room.Name ?? "No name"})");
                    }
                    if (roomsWithoutNumber.Count > 10)
                        sb.AppendLine($"   ... and {roomsWithoutNumber.Count - 10} more");
                    totalIssues += roomsWithoutNumber.Count;
                }
                else
                {
                    sb.AppendLine("✅ All rooms have numbers");
                }

                if (unplacedRooms.Count > 0)
                {
                    sb.AppendLine($"⚠️ Unplaced/Invalid Rooms: {unplacedRooms.Count}");
                    foreach (var room in unplacedRooms.Take(5))
                    {
                        sb.AppendLine($"   - Room ID:{room.Id.Value} ({room.Number ?? "No number"})");
                    }
                    totalIssues += unplacedRooms.Count;
                }
                else
                {
                    sb.AppendLine("✅ All rooms are properly placed");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"❌ Error checking rooms: {ex.Message}");
            }

            sb.AppendLine();

            // 3. Duplicate Type Marks
            sb.AppendLine("--- DUPLICATE CHECK ---");
            try
            {
                var duplicateMarks = CheckDuplicateTypeMarks();
                if (duplicateMarks.Count > 0)
                {
                    sb.AppendLine($"⚠️ Duplicate Type Marks found: {duplicateMarks.Count} groups");
                    foreach (var dup in duplicateMarks.Take(5))
                    {
                        sb.AppendLine($"   - Mark \"{dup.Key}\": {dup.Value.Count} instances");
                        foreach (var id in dup.Value.Take(3))
                        {
                            var elem = _doc.GetElement(id);
                            sb.AppendLine($"      • {elem?.Name ?? "Unknown"} (ID:{id.Value})");
                        }
                    }
                    totalIssues += duplicateMarks.Count;
                }
                else
                {
                    sb.AppendLine("✅ No duplicate Type Marks found");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"❌ Error checking duplicates: {ex.Message}");
            }

            sb.AppendLine();

            // Summary
            sb.AppendLine("--- SUMMARY ---");
            if (totalIssues == 0)
            {
                sb.AppendLine("✅ No issues found! Project looks clean.");
            }
            else
            {
                sb.AppendLine($"⚠️ Total Issues Found: {totalIssues}");
                sb.AppendLine("Please review and fix the issues listed above.");
            }
            sb.AppendLine("===");

            return sb.ToString();
        }

        /// <summary>
        /// Check for duplicate Type Marks.
        /// </summary>
        private Dictionary<string, List<ElementId>> CheckDuplicateTypeMarks()
        {
            var result = new Dictionary<string, List<ElementId>>();
            
            var elements = new FilteredElementCollector(_doc)
                .WhereElementIsNotElementType()
                .Where(e => e.Category != null)
                .ToList();

            var markGroups = new Dictionary<string, List<ElementId>>();
            
            foreach (var elem in elements)
            {
                try
                {
                    var markParam = elem.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK);
                    if (markParam != null && markParam.HasValue)
                    {
                        string mark = markParam.AsString();
                        if (!string.IsNullOrWhiteSpace(mark))
                        {
                            if (!markGroups.ContainsKey(mark))
                                markGroups[mark] = new List<ElementId>();
                            markGroups[mark].Add(elem.Id);
                        }
                    }
                }
                catch { }
            }

            // Only return duplicates (more than 1)
            foreach (var group in markGroups.Where(g => g.Value.Count > 1))
            {
                result[group.Key] = group.Value;
            }

            return result;
        }

        /// <summary>
        /// Get info about selected elements.
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

                foreach (var id in selectedIds.Take(10))
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
