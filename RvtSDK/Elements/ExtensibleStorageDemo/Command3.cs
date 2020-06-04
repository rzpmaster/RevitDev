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
    public class Command3 : IExternalCommand
    {
        UIDocument uiDoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiDoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;

            //项目信息
            Element element = doc.ProjectInformation;

            //构造 Schema
            Guid guid = new Guid("f9b00633-6aa9-47a6-acea-7f74407b9ea4");
            Schema schema = CreateSchema(guid);

            Entity entity = new Entity(schema);

            Field XYZ = schema.GetField("XYZ");
            entity.Set<XYZ>(XYZ, new XYZ(1, 1, 1), DisplayUnitType.DUT_DECIMAL_FEET);

            Field Double = schema.GetField("Double");
            entity.Set<double>(Double, 0.1, DisplayUnitType.DUT_METERS);

            Field ilist = schema.GetField("Ilist<string>");
            entity.Set<IList<string>>(ilist, new List<string>() { "1"});

            Field idictionary = schema.GetField("IDictinary<string,int>");
            Dictionary<string,int> dic = new Dictionary<string, int>();
            dic.Add("1", 1);
            entity.Set<IDictionary<string,int>>(idictionary, dic);

            //Field SubEntity = schema.GetField("SubEntity");

            using (Transaction trans = new Transaction(doc, "给Element设置Entity"))
            {
                trans.Start();
                element.SetEntity(entity);
                //element.SetEntity(entity2);
                trans.Commit();
            }


            //
            // 取
            //

            //IList<Guid> guids = element.GetEntitySchemaGuids();
            //TaskDialog.Show("CBIM", guids.ToString());

            Entity retrievedEntity = element.GetEntity(schema);
            XYZ retrievedData = retrievedEntity.Get<XYZ>(schema.GetField("XYZ"), DisplayUnitType.DUT_DECIMAL_FEET);
            //TaskDialog.Show("CBIM", retrievedData.ToString());

            double retrievedData2 = retrievedEntity.Get<double>(schema.GetField("Double"), DisplayUnitType.DUT_DECIMAL_FEET);
            //TaskDialog.Show("CBIM", retrievedData2.ToString());

            IList<string> retrievedData3 = retrievedEntity.Get<IList<string>>(schema.GetField("List"));

            IDictionary<string, int> retrievedData4 = retrievedEntity.Get<IDictionary<string, int>>(schema.GetField("Dictionary"));

            Entity retrievedData5 = retrievedEntity.Get<Entity>(schema.GetField("SubEntity"));


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

                FieldBuilder fieldBuilder = schemaBuilder.AddSimpleField("XYZ", typeof(XYZ));
                fieldBuilder.SetUnitType(UnitType.UT_Length);
                fieldBuilder.SetDocumentation("I'm XYZ");

                FieldBuilder fieldBuilder1 = schemaBuilder.AddSimpleField("Double", typeof(double));
                fieldBuilder1.SetUnitType(UnitType.UT_Length);
                fieldBuilder1.SetDocumentation("I'm Double");

                //Ilist<string>
                FieldBuilder fieldBuilder2 = schemaBuilder.AddArrayField("List", typeof(string));
                //fieldBuilder2.SetUnitType(UnitType.UT_Length);
                fieldBuilder2.SetDocumentation("I'm Ilist<string>");

                //IDictinary<string,int>
                FieldBuilder fieldBuilder3 = schemaBuilder.AddMapField("Dictionary", typeof(string), typeof(int));
                //fieldBuilder3.SetUnitType(UnitType.UT_Length);
                fieldBuilder3.SetDocumentation("I'm IDictinary<string,int>");

                //SubSchema
                //Guid guid = new Guid("f9b00633-6aa9-47a6-acea-7f74407b9ea5");
                //Schema subSchema = Schema.Lookup(guid);
                //if (subSchema == null)
                //{
                //    SchemaBuilder subSchemaBuilder = new SchemaBuilder(guid);
                //    subSchemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                //    subSchemaBuilder.SetWriteAccessLevel(AccessLevel.Public);

                //    FieldBuilder sub = subSchemaBuilder.AddSimpleField("subInt", typeof(int));
                //    sub.SetDocumentation("I'm Int");

                //    subSchemaBuilder.SetSchemaName("SubSchema");
                //    subSchema = subSchemaBuilder.Finish();
                //}
                //Entity subEntity = new Entity(subSchema);
                //subEntity.Set<int>(subSchema.GetField("subInt"), 11);

                //FieldBuilder fieldBuilder4 = schemaBuilder.AddSimpleField("SubEntity", typeof(Entity));
                //fieldBuilder4.SetDocumentation("I'm SubSchema");


                schemaBuilder.SetSchemaName("SchemaName");
                schema = schemaBuilder.Finish();
            }

            return schema;
        }
    }
}
