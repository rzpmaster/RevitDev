using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtensibleStorageDemo
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command2 : IExternalCommand
    {
        UIDocument uiDoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiDoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;

            //f9b00633-6aa9-47a6-acea-7f74407b9ea4
            IList<Schema> schemas = Schema.ListSchemas();

            #region 没有SchemaId,也不知道Element，如何查找 Schema和element
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> eles = collector.WhereElementIsNotElementType().ToElements();
            Entity entity = null;
            Schema schema = null;
            Element element = null;
            bool hasFinded = false;
            foreach (var sc in schemas)
            {
                schema = sc;
                if (schema.ReadAccessLevel != AccessLevel.Public)
                {
                    continue;
                }
                foreach (var ele in eles)
                {
                    Entity en = ele.GetEntity(schema);
                    if (en.IsValid())
                    {
                        entity = en;
                        element = ele;
                        hasFinded = true;
                        break;
                    }
                }
                if (hasFinded)
                {
                    break;
                }
            }
            if (entity != null)
            {
                XYZ retrievedData = entity.Get<XYZ>(schema.GetField("FieldName"), DisplayUnitType.DUT_DECIMAL_FEET);
                TaskDialog.Show("CBIM", retrievedData.ToString());
            }
            else
            {
                TaskDialog.Show("CBIM", "没找到");
            }
            #endregion

            //FilteredElementCollector collector2 = new FilteredElementCollector(doc);
            //ICollection<ElementId> eles2 = collector2.OfClass(typeof(DataStorage)).ToElementIds();
            //using (Transaction trans=new Transaction(doc, "删除DataStorage"))
            //{
            //    trans.Start();
            //    doc.Delete(eles2);
            //    trans.Commit();
            //}

            return Result.Succeeded;
        }
    }
}
