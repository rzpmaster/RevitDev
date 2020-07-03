using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindColumns
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class Command : IExternalCommand
    {
        UIApplication m_app;
        Document m_doc;
        View3D m_view3D;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_app = commandData.Application;
            m_doc = commandData.Application.ActiveUIDocument.Document;

            Get3DView("{3D}");

            Selection selection = m_app.ActiveUIDocument.Selection;
            List<Wall> wallsToCheck = new List<Wall>();

            // If wall(s) are selected, process them.
            if (selection.GetElementIds().Count > 0)
            {
                foreach (ElementId eId in selection.GetElementIds())
                {
                    Element e = m_doc.GetElement(eId);
                    if (e is Wall)
                    {
                        wallsToCheck.Add((Wall)e);
                    }
                }

                if (wallsToCheck.Count <= 0)
                {
                    message = "No walls were found in the active document selection";
                    return Result.Cancelled;
                }
            }
            // Find all walls in the document and process them.
            else
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                FilteredElementIterator iter = collector.OfClass(typeof(Wall)).GetElementIterator();
                while (iter.MoveNext())
                {
                    wallsToCheck.Add((Wall)iter.Current);
                }
            }

            // Execute the check for embedded columns
            CheckWallsForEmbeddedColumns(wallsToCheck);

            // Process the results, in this case set the active selection to contain all embedded columns
            ICollection<ElementId> toSelected = new List<ElementId>();
            if (m_allColumnsOnWalls.Count > 0)
            {
                foreach (ElementId id in m_allColumnsOnWalls)
                {
                    ElementId familyInstanceId = id;
                    Element familyInstance = m_doc.GetElement(familyInstanceId);
                    toSelected.Add(familyInstance.Id);
                }
                selection.SetElementIds(toSelected);
            }
            return Result.Succeeded;
        }

        private void CheckWallsForEmbeddedColumns(List<Wall> wallsToCheck)
        {
            foreach (Wall wall in wallsToCheck)
            {
                CheckWallForEmbeddedColumns(wall);
            }
        }

        private void CheckWallForEmbeddedColumns(Wall wall)
        {
            LocationCurve locationCurve = wall.Location as LocationCurve;
            Curve wallCurve = locationCurve.Curve;
            if (wallCurve is Line)
            {
                CheckLinearWallForEmbeddedColumns(wall, (Line)wallCurve);
            }
            else
            {
                CheckProfiledWallForEmbeddedColumns(wall, wallCurve);
            }
        }

        private void CheckProfiledWallForEmbeddedColumns(Wall wall, Curve wallCurve)
        {
            double bottomHeight = GetElevationForRay(wall);

            // 曲线化直
            double parameterIncrement = WallIncrement / wallCurve.Length;//param增量
            // 管线查找长度为 2 ，再小的话，容易找不到相交柱子的面
            double findColumnWithin = 2;

            for (double parameter = 0; parameter < 1.0; parameter += parameterIncrement)
            {
                FindColumnsOnEitherSideOfWall(wall, wallCurve, parameter, bottomHeight, findColumnWithin);
            }
        }

        private void CheckLinearWallForEmbeddedColumns(Wall wall, Line wallCurve)
        {
            double bottomHeight = GetElevationForRay(wall);

            FindColumnsOnEitherSideOfWall(wall, wallCurve, 0, bottomHeight, wallCurve.Length);
        }

        /// <summary>
        /// 该方法寻找了墙的内外两侧相交的柱子
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="wallCurve"></param>
        /// <param name="parameter"></param>
        /// <param name="elevation">光线查找高度</param>
        /// <param name="within">光线查找长度</param>
        private void FindColumnsOnEitherSideOfWall(Wall wall, Curve wallCurve, double parameter, double elevation, double within)
        {
            XYZ rayDirection = GetTangentAt(wallCurve, parameter);
            XYZ wallNormal = GetNormalAt(wallCurve, parameter);
            double wallWidth = wall.Width;

            XYZ wallDelta = wallNormal * wallWidth / 2 + new XYZ(WALL_EPSILON, WALL_EPSILON, 0);

            XYZ wallLocation = wallCurve.Evaluate(parameter, true);
            XYZ rayStart = new XYZ(wallLocation.X, wallLocation.Y, elevation) + wallDelta;
            FindColumnsByDirection(rayStart, rayDirection, within, wall);

            rayStart = new XYZ(wallLocation.X, wallLocation.Y, elevation) - wallDelta;
            FindColumnsByDirection(rayStart, rayDirection, within, wall);
        }

        /// <summary>
        /// 寻找与光线相交的柱子
        /// </summary>
        /// <param name="rayStart"></param>
        /// <param name="rayDirection"></param>
        /// <param name="proximity"></param>
        /// <param name="wall"></param>
        private void FindColumnsByDirection(XYZ rayStart, XYZ rayDirection, double proximity, Wall wall)
        {
            ReferenceIntersector referenceIntersector = new ReferenceIntersector(m_view3D);
            IList<ReferenceWithContext> intersectedReferences = referenceIntersector.Find(rayStart, rayDirection);

            //处理找到的柱子，加入集合中
            foreach (ReferenceWithContext reference in intersectedReferences)
            {
                // Exclude items too far from the start point.
                if (reference.Proximity < proximity)
                {
                    Element referenceElement = wall.Document.GetElement(reference.GetReference());
                    if (referenceElement is FamilyInstance)
                    {
                        FamilyInstance familyInstance = (FamilyInstance)referenceElement;
                        ElementId familyInstanceId = familyInstance.Id;
                        ElementId wallId = wall.Id;
                        int categoryIdValue = referenceElement.Category.Id.IntegerValue;
                        if (categoryIdValue == (int)BuiltInCategory.OST_Columns || categoryIdValue == (int)BuiltInCategory.OST_StructuralColumns)
                        {
                            // Add the column to the map of wall->columns
                            if (m_columnsOnWall.ContainsKey(wallId))
                            {
                                List<ElementId> columnsOnWall = m_columnsOnWall[wallId];
                                if (!columnsOnWall.Contains(familyInstanceId))
                                    columnsOnWall.Add(familyInstanceId);
                            }
                            else
                            {
                                List<ElementId> columnsOnWall = new List<ElementId>();
                                columnsOnWall.Add(familyInstanceId);
                                m_columnsOnWall.Add(wallId, columnsOnWall);
                            }
                            // Add the column to the complete list of all embedded columns
                            if (!m_allColumnsOnWalls.Contains(familyInstanceId))
                                m_allColumnsOnWalls.Add(familyInstanceId);
                        }//end if is column
                    }//end if is instance
                }//end if is in wallLength
            }//end foreach
        }

        /// <summary>
        /// 获得曲线给定参数点的法线向量
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private XYZ GetNormalAt(Curve curve, double parameter)
        {
            if (curve is Line)
            {
                XYZ wallDirection = GetTangentAt(curve, 0);
                XYZ wallNormal = new XYZ(wallDirection.Y, wallDirection.X, 0);
                return wallNormal;
            }
            else
            {
                Transform t = curve.ComputeDerivatives(parameter, true);
                // For non-linear curves, BasisY is the normal vector to the curve.
                return t.BasisY.Normalize();
            }
        }

        /// <summary>
        /// 获取曲线参数点处的切线向量
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private XYZ GetTangentAt(Curve curve, double parameter)
        {
            Transform t = curve.ComputeDerivatives(parameter, true);
            // BasisX is the tangent vector of the curve.
            return t.BasisX.Normalize();
        }

        private double GetElevationForRay(Wall wall)
        {
            Level level = m_doc.GetElement(wall.LevelId) as Level;

            // Start at 1 foot above the bottom level
            double bottomHeight = level.Elevation + 1.0;

            return bottomHeight;
        }

        private void Get3DView(string viewName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(m_doc);
            foreach (View3D v in collector.OfClass(typeof(View3D)).ToElements())
            {
                // skip view template here because view templates are invisible in project browsers
                if (v != null && !v.IsTemplate && v.Name == viewName)
                {
                    m_view3D = v as View3D;
                    break;
                }
            }
        }

        static double WallIncrement = 0.5;  // Check every 1/2'
        static double WALL_EPSILON = (1.0 / 8.0) / 12.0;  // 1/8"

        Dictionary<ElementId, List<ElementId>> m_columnsOnWall = new Dictionary<ElementId, List<ElementId>>();
        List<ElementId> m_allColumnsOnWalls = new List<ElementId>();
    }
}
