using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectionCalculation
{
    class FindSouthFacingWalls : FindSouthFacingBase
    {
        public FindSouthFacingWalls(ExternalCommandData commandData) : base(commandData)
        {
        }

        public override void Execute(bool useProjectLocationNorth)
        {
            UIDocument uiDoc = new UIDocument(Document);
            ElementSet selElements = new ElementSet();

            foreach (ElementId elementId in uiDoc.Selection.GetElementIds())
            {
                selElements.Insert(uiDoc.Document.GetElement(elementId));
            }

            IEnumerable<Wall> walls = CollectExteriorWalls();
            foreach (Wall wall in walls)
            {
                XYZ exteriorDirection = GetWallDirection(wall);

                if (useProjectLocationNorth)
                    exteriorDirection = TransformByProjectLocation(exteriorDirection);

                bool isSouthFacing = IsSouthFacing(exteriorDirection);
                if (isSouthFacing)
                    selElements.Insert(wall);
            }

            // Select all walls which had the proper direction.
            List<ElementId> elemIdList = new List<ElementId>();
            foreach (Element element in selElements)
            {
                elemIdList.Add(element.Id);
            }
            uiDoc.Selection.SetElementIds(elemIdList);
        }

        private XYZ GetWallDirection(Wall wall)
        {
            LocationCurve locationCurve = wall.Location as LocationCurve;
            XYZ wallDirection = XYZ.BasisZ;

            if (locationCurve != null)
            {
                Curve curve = locationCurve.Curve;

                //Write("Wall line endpoints: ", curve);

                XYZ direction = XYZ.BasisX; //切线向量
                if (curve is Line)
                {
                    // Obtains the tangent vector of the wall.
                    direction = curve.ComputeDerivatives(0, true).BasisX.Normalize();
                }
                else
                {
                    // An assumption, for non-linear walls, that the "tangent vector" is the direction
                    // from the start of the wall to the end.
                    direction = (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize();
                }
                // Calculate the normal vector via cross product.
                wallDirection = XYZ.BasisZ.CrossProduct(direction);

                // Flipped walls need to reverse the calculated direction
                if (wall.Flipped) wallDirection = -wallDirection;
            }

            return wallDirection;
        }

        /// <summary>
        /// 过滤出所有外墙
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Wall> CollectExteriorWalls()
        {
            FilteredElementCollector collector = new FilteredElementCollector(Document);
            IList<Element> elementsToProcess = collector.OfClass(typeof(Wall)).ToElements();
            // Use a LINQ query to filter out only Exterior walls
            IEnumerable<Wall> exteriorWalls = from wall in elementsToProcess.Cast<Wall>()
                                              where IsExterior(Document.GetElement(wall.GetTypeId()) as ElementType)
                                              select wall;
            return exteriorWalls;
        }

        private bool IsExterior(ElementType wallType)
        {
            Parameter wallFunction = wallType.get_Parameter(BuiltInParameter.FUNCTION_PARAM);
            WallFunction value = (WallFunction)wallFunction.AsInteger();

            return (value == WallFunction.Exterior);
        }
    }
}
