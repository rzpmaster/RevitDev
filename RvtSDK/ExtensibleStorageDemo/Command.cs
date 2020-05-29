using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace ExtensibleStorageDemo
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        UIDocument uiDoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiDoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;

            //
            // 存
            //

            //墙
            Selection sel = uiDoc.Selection;
            Reference reference = sel.PickObject(ObjectType.Element, "选择墙");
            Element element = doc.GetElement(reference);

            //项目信息
            //Element element = doc.ProjectInformation;

            //DataStorage
            //FilteredElementCollector collector = new FilteredElementCollector(doc);
            //ICollection<Element> eles = collector.OfClass(typeof(DataStorage)).ToElements();
            //Element element = eles.FirstOrDefault();
            //if (element == null)
            //{
            //    using (Transaction trans = new Transaction(doc, "创建DataStorage"))
            //    {
            //        trans.Start();
            //        element = DataStorage.Create(doc);
            //        trans.Commit();
            //    }
            //}

            //构造 Schema
            Guid guid = new Guid("f9b00633-6aa9-47a6-acea-7f74407b9ea4");
            Schema schema = CreateSchema(guid);

            Guid guid2 = new Guid("f9b00633-6aa9-47a6-acea-7f74407b9ea5");
            Schema schema2 = CreateSchema(guid2);

            //构造 Entity
            Entity entity2 = new Entity(schema2);
            Field field2 = schema2.GetField("FieldName");
            entity2.Set<XYZ>(field2, new XYZ(2, 2, 2), DisplayUnitType.DUT_DECIMAL_FEET);

            Entity entity = new Entity(schema);
            Field field = schema.GetField("FieldName");
            entity.Set<XYZ>(field, new XYZ(1, 1, 1), DisplayUnitType.DUT_DECIMAL_FEET);

            Field doubleFiled = schema.GetField("Double");
            entity.Set<double>(doubleFiled, 0.1, DisplayUnitType.DUT_METERS);

            using (Transaction trans = new Transaction(doc, "给Element设置Entity"))
            {
                trans.Start();
                element.SetEntity(entity);
                element.SetEntity(entity2);
                trans.Commit();
            }


            //
            // 取
            //

            IList<Guid> guids = element.GetEntitySchemaGuids();
            TaskDialog.Show("CBIM", guids.ToString());

            Entity retrievedEntity = element.GetEntity(schema);
            XYZ retrievedData = retrievedEntity.Get<XYZ>(schema.GetField("FieldName"), DisplayUnitType.DUT_DECIMAL_INCHES);
            TaskDialog.Show("CBIM", retrievedData.ToString());

            double retrievedData2 = retrievedEntity.Get<double>(schema.GetField("Double"), DisplayUnitType.DUT_DECIMAL_FEET);
            TaskDialog.Show("CBIM", retrievedData2.ToString());

            //Entity retrievedEntity2 = element.GetEntity(schema2);
            //XYZ retrievedData2 = retrievedEntity2.Get<XYZ>(schema2.GetField("FieldName"), DisplayUnitType.DUT_DECIMAL_FEET);
            //TaskDialog.Show("CBIM", retrievedData2.ToString());



            //
            // 删除
            //

            IList<Schema> schemas = Schema.ListSchemas();   //从内存中找所有schemas
            TaskDialog.Show("CBIM1", schemas.Count.ToString());

            #region 只删除了Element与Entity的关系
            bool isSuccess = false;
            using (Transaction trans = new Transaction(doc, "删除entity"))
            {
                trans.Start();
                isSuccess = element.DeleteEntity(schema);      //只删除了Element与Entity的关系
                trans.Commit();
            }
            if (isSuccess)
            {
                schemas = Schema.ListSchemas();
            }
            TaskDialog.Show("CBIM2", schemas.Count.ToString());

            Entity retrievedEntity2 = element.GetEntity(schema);
            if (retrievedEntity2.Schema == null)
            {
                TaskDialog.Show("CBIM3", "Schema为空");
            }
            #endregion

            #region 彻底删除Schema
            //删除schema
            //using (Transaction trans = new Transaction(doc, "删除Schema"))
            //{
            //    trans.Start();
            //    Schema.EraseSchemaAndAllEntities(schema, false);
            //    Schema.EraseSchemaAndAllEntities(schema2, false);
            //    trans.Commit();
            //}
            //schemas = Schema.ListSchemas();
            //TaskDialog.Show("CBIM4", schemas.Count.ToString());
            #endregion


            return Result.Succeeded;
        }

        public Schema CreateSchema(Guid schemaGuid)
        {
            Schema schema = Schema.Lookup(schemaGuid);
            if (schema == null)
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(schemaGuid);
                schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
                //schemaBuilder.SetWriteAccessLevel(AccessLevel.Vendor);
                //schemaBuilder.SetVendorId("CBIM");

                //XYZ FieldName;
                FieldBuilder fieldBuilder = schemaBuilder.AddSimpleField("FieldName", typeof(XYZ));
                fieldBuilder.SetUnitType(UnitType.UT_Length);
                fieldBuilder.SetDocumentation("I'm description");

                FieldBuilder fieldBuilderDouble = schemaBuilder.AddSimpleField("Double", typeof(double));
                fieldBuilderDouble.SetUnitType(UnitType.UT_Length);
                fieldBuilderDouble.SetDocumentation("I'm Double");

                //Ilist<string>
                //FieldBuilder fieldBuilder2 = schemaBuilder.AddArrayField("FieldName2", typeof(string));
                ////fieldBuilder2.SetUnitType(UnitType.UT_Length);
                //fieldBuilder2.SetDocumentation("I'm description2");

                //IDictinary<string,int>
                //FieldBuilder fieldBuilder3 = schemaBuilder.AddMapField("FieldName3", typeof(string),typeof(int));
                ////fieldBuilder3.SetUnitType(UnitType.UT_Length);
                //fieldBuilder3.SetDocumentation("I'm description3");

                schemaBuilder.SetSchemaName("SchemaName");
                schema = schemaBuilder.Finish();
            }

            return schema;
        }
    }
}
