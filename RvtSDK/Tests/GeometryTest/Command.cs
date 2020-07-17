using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeometryTest
{
    /// <summary>
    /// 一个元素的 Geometry
    /// 最多只可能有一个 GeometryInstance
    /// 但是可以有多个 Solid
    /// 也可以同时拥有 GeometryInstance 和 Solid
    /// </summary>
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
            DataColumn dataColumn3 = new DataColumn("GeomInstance");
            DataColumn dataColumn4 = new DataColumn("Solid");
            DataColumn dataColumn5 = new DataColumn("Others");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                dataColumn1,dataColumn2,dataColumn3,dataColumn4,dataColumn5
            });
            foreach (var item in geoms)
            {
                var row = dataTable.NewRow();
                row[0] = item.Element.Name;
                row[1] = item.Element.Category.Name;
                row[2] = item.GeometryInstances.Count;
                row[3] = item.Solids.Count;
                row[4] = item.Others.Count;
                dataTable.Rows.Add(row);
            }

            //ExcelHelper excelHelper = new ExcelHelper(@"C:\Users\rzp\Desktop\log.xlsx");
            //excelHelper.DataTableToExcel("1", dataTable, true);

            return Result.Succeeded;
        }
    }

    /// <summary>
    /// 发现柱子的 GeometryInstance 里的 Solid 的面都为空
    /// 只有 GetOriginalGeometry()（未被切割过的） 和 第一层（和 GeometryInstance 同一级）（已经被切割过的）中的 Solid 有值
    /// 而且他们中都包含两个 Solid ，却只有一个有体积
    /// 
    /// 猜想：
    /// 一个元素的 Geometry 中，只有一个 有效的 Solid 
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class GeometryInstanceCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            FamilyInstance fi = doc.GetElement(r) as FamilyInstance;

            if (fi == null)
            {
                TaskDialog.Show("Tip", "选择的不是 族实例");
                return Result.Cancelled;
            }

            var instanceGeomElem = GeometryTestUtils.GetInstanceGeometry(fi);

            if (instanceGeomElem != null)
            {
                // 类型几何 对应模型坐标系
                var symbolGeom = instanceGeomElem.GetSymbolGeometry();
                // 实例几何 对应族坐标系
                var instanceGeom = instanceGeomElem.GetInstanceGeometry();

                // 族坐标系 到 模型坐标系 的转换
                var transform = instanceGeomElem.Transform;

                // 未被切割过的几何 对应模型坐标系
                var geom = fi.GetOriginalGeometry(new Options());
            }


            return Result.Succeeded;
        }
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class SolidCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            

            return Result.Succeeded;
        }
    }
}
