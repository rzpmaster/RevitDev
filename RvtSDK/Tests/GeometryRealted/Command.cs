using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DotNetUtils.File;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeometryRealted
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var eleIds = uidoc.Selection.GetElementIds();
            var geoms = eleIds.Select(e => doc.GetElement(e)).Select(e => new GeometryInfo(e)).ToList();

            DataTable dataTable = new DataTable();
            DataColumn dataColumn1 = new DataColumn("元素名称");
            DataColumn dataColumn2 = new DataColumn("元素类型");
            DataColumn dataColumn3 = new DataColumn("是否简单类型");
            DataColumn dataColumn4 = new DataColumn("GeomInstance");
            DataColumn dataColumn5 = new DataColumn("Solid");
            DataColumn dataColumn6 = new DataColumn("Others");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                dataColumn1,dataColumn2,dataColumn3,dataColumn4,dataColumn5,dataColumn6
            });
            foreach (var item in geoms)
            {
                var row = dataTable.NewRow();
                row[0] = item.Element.Name;
                row[1] = item.Element.Category.Name;
                row[2] = item.IsSimilarType;
                row[3] = item.GeometryInstances.Count;
                row[4] = item.Solids.Count;
                row[5] = item.Others.Count;
                dataTable.Rows.Add(row);
            }

            ExcelHelper excelHelper = new ExcelHelper(@"C:\Users\rzp\Desktop\log.xlsx");
            excelHelper.DataTableToExcel("1", dataTable, true);

            return Result.Succeeded;
        }
    }
}
