using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBIM.Terminal.Common.Utils
{
    public static class RoomUtils
    {
        /// <summary>
        /// 获得房间地板到楼板的距离，单位foot
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        /// <remarks>如果该房间没有楼板，返回该房间上下两个标高的高度</remarks>
        public static double GetRoomHeight(Room room)
        {
            //判断有没有楼板
            var floors = GetElementsInRoom(room, BuiltInCategory.OST_Floors);
            if (floors.Count == 0)
            {
                //没有楼板
                return GetRoomHeightByAdjacentElevation(room);
            }

            FilteredElementCollector collector = new FilteredElementCollector(room.Document);
            var view3D = collector.OfClass(typeof(View3D)).Cast<View3D>().First<View3D>(v3 => !(v3.IsTemplate));
            ReferenceIntersector referenceIntersector = new ReferenceIntersector(view3D);
            BoundingBoxXYZ box = room.get_BoundingBox(view3D);
            XYZ center = box.Min.Add(box.Max).Multiply(0.5);
            center = new XYZ(center.X, center.Y, room.Level.Elevation);
            XYZ rayDirection = new XYZ(0, 0, 1);

            IList<ReferenceWithContext> references = referenceIntersector.Find(center, rayDirection);
            double distance = Double.PositiveInfinity;  //储存距离
            XYZ intersection = null;                    //储存交点
            foreach (ReferenceWithContext referenceWithContext in references)
            {
                Reference reference = referenceWithContext.GetReference();
                //楼板
                if (room.Document.GetElement(reference).Category.Id.IntegerValue != (int)BuiltInCategory.OST_Floors)
                    continue;
                double proximity = referenceWithContext.Proximity;
                if (proximity < distance && proximity > 0)
                {
                    distance = proximity;
                    intersection = reference.GlobalPoint;
                }
            }

            return distance;
        }

        public static double GetRoomHeightByAdjacentElevation(Room room, Document document = null)
        {
            Document doc = document == null ? room.Document : document;
            List<Level> allLevel = LevelUtils.GetAllLevels(doc);
            var upperLevel = allLevel.First(x => x.Elevation - room.Level.Elevation > 1);
            return upperLevel.Elevation - room.Level.Elevation;
        }

        /// <summary>
        /// 获得房间内的构建
        /// </summary>
        /// <param name="room"></param>
        /// <param name="builtInCategory">构建的类型</param>
        /// <returns></returns>
        public static IList<Element> GetElementsInRoom(Room room, BuiltInCategory builtInCategory, Document document = null)
        {
            List<Element> elements = new List<Element>();
            Document doc = document == null ? room.Document : document;
            Outline outline = GetRoomMaxBoundingBox(room);
            if (outline == null) return elements;

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var bboxFilter = new BoundingBoxIntersectsFilter(outline);
            var list = collector.OfCategory(builtInCategory)
                                .WhereElementIsNotElementType()
                                .WherePasses(bboxFilter).ToElements();

            foreach (var item in list)
            {
                elements.Add(item);
            }
            return elements;
        }

        /// <summary>
        /// 获得房间地板中心坐标
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static XYZ GetRoomCenterPoint(Room room)
        {
            Document doc = room.Document;
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var view3D = collector.OfClass(typeof(View3D)).Cast<View3D>().First<View3D>(v3 => !(v3.IsTemplate));
            BoundingBoxXYZ box = room.get_BoundingBox(view3D);
            XYZ center = box.Min.Add(box.Max).Multiply(0.5);
            center = new XYZ(center.X, center.Y, room.Level.Elevation);
            return center;
        }

        /// <summary>
        /// 获取包裹房间的Boundingbox，可用于过滤房间内的构建
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static Outline GetRoomMaxBoundingBox(Room room)
        {
            Document doc = room.Document;
            var height = GetRoomHeightByAdjacentElevation(room);
            var level = room.Level.ProjectElevation;
            height += level;//房间的总高度

            var bbox = room.get_BoundingBox(doc.ActiveView);
            var new_posation = new XYZ(bbox.Max.X, bbox.Max.Y, height);//将bbox的高度拉高至房间最高的墙的高度
            Outline outline = new Outline(bbox.Min, new_posation);

            return outline;
        }

        /// <summary>
        /// 获取Room的Curveloop属性
        /// 房间的CsurveLoop 可能有自交的情况（自交的情况下不能拉伸Solid），通过调整SpatialElementBoundaryLocation.Center可以消除
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static IList<CurveLoop> GetRoomCurveLoopBoundary(Room room)
        {
            var bndOpt = new SpatialElementBoundaryOptions();
            foreach (SpatialElementBoundaryLocation spl in Enum.GetValues(typeof(SpatialElementBoundaryLocation)))
            {
                bndOpt.SpatialElementBoundaryLocation = spl;
                try
                {
                    var loops = Autodesk.Revit.DB.IFC.ExporterIFCUtils.GetRoomBoundaryAsCurveLoopArray(room, bndOpt, true);
                    GeometryCreationUtilities.CreateExtrusionGeometry(loops, XYZ.BasisZ, 10);

                    return loops;
                }
                catch (Exception)
                {
                }
            }

            return null;
        }

        /// <summary>
        /// 获得组成房间的墙
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static List<Wall> GetRoomSurroundingWalls(Room room)
        {
            var walls = new List<Wall>();
            if (null == room)
            {
                return walls;
            }

            foreach (var bds in room.GetBoundarySegments(new SpatialElementBoundaryOptions()))
            {
                foreach (var seg in bds)
                {
                    var e = room.Document.GetElement(seg.ElementId);
                    if (null == e)
                        continue;

                    if (e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Walls)
                    {
                        if (walls.Find(x => x.Id == seg.ElementId) == null)
                            walls.Add(e as Wall);
                    }
                }
            }
            return walls;
        }

        /// <summary>
        /// 获取Room的墙和分割线
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static List<Element> GetRoomWallsAndSeparationLines(Room room)
        {
            var borderList = new List<Element>();
            if (null == room)
            {
                return borderList;
            }

            foreach (var bds in room.GetBoundarySegments(new SpatialElementBoundaryOptions()))
            {
                foreach (var seg in bds)
                {
                    var e = room.Document.GetElement(seg.ElementId);
                    if (null == e)
                        continue;

                    if (e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Walls || e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_RoomSeparationLines)
                    {
                        if (borderList.Find(x => x.Id == seg.ElementId) == null)
                            borderList.Add(e);
                    }
                }
            }
            return borderList;
        }

        public static bool HasCeiling(Room room,Document document=null)
        {
            var ceilings = GetElementsInRoom(room, BuiltInCategory.OST_Ceilings, document);
            return ceilings.Count > 0;
        }

        /// <summary>
        /// 计算房间吊顶到房间地面的高度，单位foot
        /// </summary>
        /// <returns></returns>
        public static double GetRoomCeilingHeight(Room room)
        {
            if (!HasCeiling(room))
            {
                return -1;
            }
            
            FilteredElementCollector collector = new FilteredElementCollector(room.Document);
            var view3D = collector.OfClass(typeof(View3D)).Cast<View3D>().First<View3D>(v3 => !(v3.IsTemplate));
            ReferenceIntersector referenceIntersector = new ReferenceIntersector(view3D);
            var start = GetRoomCenterPoint(room);
            XYZ rayDirection = new XYZ(0, 0, 1);

            IList<ReferenceWithContext> references = referenceIntersector.Find(start, rayDirection);
            double distance = Double.PositiveInfinity;  //储存距离
            XYZ intersection = null;                    //储存交点
            foreach (ReferenceWithContext referenceWithContext in references)
            {
                Reference reference = referenceWithContext.GetReference();
                //楼板或者天花板
                if (room.Document.GetElement(reference).Category.Id.IntegerValue != (int)BuiltInCategory.OST_Ceilings)
                    continue;
                double proximity = referenceWithContext.Proximity;
                if (proximity < distance && proximity > 0)
                {
                    distance = proximity;
                    intersection = reference.GlobalPoint;
                }
            }
            return distance;
        }
    }
}
