using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvoidObstruction
{
    [Transaction(TransactionMode.Manual)]
    class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;

            using (Transaction tr=new Transaction(document,"翻管Demo"))
            {
                try
                {
                    tr.Start();

                    Resolver resolver = new Resolver(commandData);
                    resolver.Resolve();
                }
                catch (Exception ex)
                {
                    tr.RollBack();
                    message += ex.ToString();
                    return Result.Failed;
                }
                finally
                {
                    tr.Commit();
                }
            }

            return Result.Succeeded;
        }
    }
}
