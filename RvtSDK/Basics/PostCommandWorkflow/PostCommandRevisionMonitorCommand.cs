using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCommandWorkflow
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class PostCommandRevisionMonitorCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            if (monitor == null)
            {
                monitor = new PostCommandRevisionMonitor(doc);
                monitor.Activate();
                commandButton.ItemText = "Remove Revision Monitor";
            }
            else
            {
                monitor.Deactivate();
                monitor = null;
                commandButton.ItemText = "Setup Revision Monitor";
            }

            return Result.Succeeded;
        }

        public static void SetPushButton(PushButton pushButton)
        {
            commandButton = pushButton;
        }

        /// <summary>
        /// The monitor.
        /// </summary>
        private static PostCommandRevisionMonitor monitor = null;

        /// <summary>
        /// The handle to the command's PushButton.
        /// </summary>
        private static PushButton commandButton;
    }
}
