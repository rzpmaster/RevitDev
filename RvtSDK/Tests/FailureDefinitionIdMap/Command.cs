using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailureDefinitionIdMap
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Command()
        {
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new TextWriterTraceListener(@"C:\Users\rzp\Desktop\BuiltInFailuresIdMaps.txt"));
            Trace.AutoFlush = true;
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            DataSet dataSet = new DataSet();

            var type = typeof(BuiltInFailures);

            var nestedTypes = type.GetNestedTypes();

            foreach (var tp in nestedTypes)
            {
                Trace.WriteLine(tp.Name);
                Trace.Indent();

                DataTable dt = new DataTable(tp.Name);
                dataSet.Tables.Add(dt);

                DataColumn[] columns = new DataColumn[]
                {
                    new DataColumn("Name"),
                    new DataColumn("Guid")
                };
                dt.Columns.AddRange(columns);

                var props = tp.GetProperties();
                foreach (var prop in props)
                {
                    Trace.WriteLine(string.Format("{0}{1}{2}",
                        prop.Name.PadRight(55),
                        "\t",
                        (prop.GetValue(null) as FailureDefinitionId).Guid));

                    DataRow dataRow = dt.NewRow();
                    dataRow[0] = prop.Name;
                    dataRow[1] = (prop.GetValue(null) as FailureDefinitionId).Guid;
                    dt.Rows.Add(dataRow);
                }

                Trace.Unindent();
            }

            return Result.Succeeded;
        }

    }
}
