using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace RevitAIArchitect
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Show the Chat Window
                // We use Show() instead of ShowDialog() so the user can still interact with Revit if we make it modeless (though WPF window on top usually blocks or needs handle)
                // For a simple tool, ShowDialog is safer to prevent Revit crashes from concurrency, but Show is better for a helper.
                // However, Revit API interactions must happen in the valid context.
                // For this simple UI that just talks to AI, it's fine.
                
                ChatWindow window = new ChatWindow();
                window.ShowDialog(); // Use ShowDialog to keep it simple for now (Model pauses)

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
