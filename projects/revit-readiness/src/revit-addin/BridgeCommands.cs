using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BimAgentLab.RevitReadiness
{
    [Transaction(TransactionMode.ReadOnly)]
    public sealed class StartReadinessBridgeCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData data, ref string message, ElementSet elements)
        {
            try
            {
                var app = BridgeApplication.Current ?? throw new InvalidOperationException("The bridge application entry is not loaded. Install the application manifest with Revit closed.");
                app.Start(); return Result.Succeeded;
            }
            catch (Exception error) { message = "Could not start readiness bridge: " + error.Message; return Result.Failed; }
        }
    }
    [Transaction(TransactionMode.ReadOnly)]
    public sealed class StopReadinessBridgeCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData data, ref string message, ElementSet elements)
        {
            try { BridgeApplication.Current?.Stop(); return Result.Succeeded; }
            catch (Exception error) { message = "Bridge cleanup needs attention: " + error.Message; return Result.Failed; }
        }
    }
    public sealed class BridgeCommandAvailable : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication application, CategorySet categories) => BridgeApplication.Current != null;
    }
}
