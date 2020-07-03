using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FilterTreeControlWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectionCalculation
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class FindSouthFacingWallsWithoutProjectLocation : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var findSouthWall = new FindSouthFacingWalls(commandData);
            Transaction trans = new Transaction(commandData.Application.ActiveUIDocument.Document, "FindSouthFacingWallsWithoutProjectLocation");
            trans.Start();
            findSouthWall.Execute(false);
            trans.Commit();

            return Result.Succeeded;
        }
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class FindSouthFacingWallsWithProjectLocation : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var findSouthWall = new FindSouthFacingWalls(commandData);
            Transaction trans = new Transaction(commandData.Application.ActiveUIDocument.Document, "FindSouthFacingWallsWithProjectLocation");
            trans.Start();
            findSouthWall.Execute(true);
            trans.Commit();

            return Result.Succeeded;
        }
    }
}
