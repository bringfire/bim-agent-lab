using System;
using Autodesk.Revit.UI;
namespace BimAgentLab.RevitReadiness
{
    internal sealed class ReadinessExternalEventHandler : IExternalEventHandler
    {
        readonly Action<UIApplication> execute;
        internal ReadinessExternalEventHandler(Action<UIApplication> execute) { this.execute = execute; }
        public void Execute(UIApplication application) { execute(application); }
        public string GetName() => "BIM Agent Lab read-only readiness bridge";
    }
}
