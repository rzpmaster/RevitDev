using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerCircuit
{
    [Transaction(TransactionMode.Manual)]
    class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var elemIds = commandData.Application.ActiveUIDocument.Selection.GetElementIds();

                if (elemIds.Count == 0)
                {
                    message = "SelectPowerElements";
                    return Result.Failed;
                }

                // Collect information from selected elements and show operation dialog
                CircuitOperationData optionData = new CircuitOperationData(commandData);
                using (CircuitOperationForm mainForm = new CircuitOperationForm(optionData))
                {
                    if (mainForm.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                    {
                        return Result.Cancelled;
                    }
                }

                // Show the dialog for user to select a circuit if more than one circuit available
                if (optionData.Operation != Operation.CreateCircuit &&
                    optionData.ElectricalSystemCount > 1)
                {
                    using (SelectCircuitForm selectForm = new SelectCircuitForm(optionData))
                    {
                        if (selectForm.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                        {
                            return Result.Cancelled;
                        }
                    }
                }

                // If user choose to edit circuit, display the circuit editing dialog
                if (optionData.Operation == Operation.EditCircuit)
                {
                    using (EditCircuitForm editForm = new EditCircuitForm(optionData))
                    {
                        if (editForm.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                        {
                            return Result.Cancelled;
                        }
                    }
                }

                // Perform the operation
                optionData.Operate();
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }
}
