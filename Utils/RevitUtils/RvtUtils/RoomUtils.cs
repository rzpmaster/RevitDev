using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.IFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitUtils.RvtUtils
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
            ElementFilter wallFilter = new ElementCategoryFilter(BuiltInCategory.OST_Floors);

            //ReferenceIntersector referenceIntersector = new ReferenceIntersector(view3D);
            ReferenceIntersector referenceIntersector = new ReferenceIntersector(wallFilter, FindReferenceTarget.Element, view3D);
            referenceIntersector.FindReferencesInRevitLinks = room.Document.IsLinked;

            var center = GetRoomCenterPoint(room);
            XYZ rayDirection = XYZ.BasisZ;

            IList<ReferenceWithContext> references = referenceIntersector.Find(center, rayDirection);
            double distance = Double.PositiveInfinity;  //储存距离
            XYZ intersection = null;                    //储存交点
            foreach (ReferenceWithContext referenceWithContext in references)
            {
                Reference reference = referenceWithContext.GetReference();
                double proximity = referenceWithContext.Proximity;
                if (proximity < distance && proximity > 0)
                {
                    distance = proximity;
                    intersection = reference.GlobalPoint;
                }
            }

            return distance;
        }

        /// <summary>
        /// 计算给定房间上下 两个标高的距离
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static double GetRoomHeightByAdjacentElevation(Room room)
        {
            List<Level> allLevel = LevelUtils.GetAllLevels(room.Document);
            var upperLevel = allLevel.FirstOrDefault(x => x.Elevation - room.Level.Elevation > 1);
            if (upperLevel == null)
            {
                //当前标高为最高标高
                //返回当前房间的参数“房间标识高度”的值
                return room.get_Parameter(BuiltInParameter.ROOM_HEIGHT).AsDouble();
            }
            return upperLevel.Elevation - room.Level.Elevation;
        }

        /// <summary>
        /// 通过房间的bbox 使用BoundingBoxIntersectsFilter 获得房间内的构建
        /// 速度快，但是不够准确
        /// </summary>
        /// <param name="room"></param>
        /// <param name="builtInCategory">目标构建的类型</param>
        /// <param name="document">目标构建该document中找，默认为room.Document，如果给定房间和目标构建不在同一个文件中（例如有一个在链接文件中），需要给定这个参数</param>
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

        private static XYZ GetRoomCenterPoint(Room room)
        {
            Document doc = room.Document;
            //FilteredElementCollector collector = new FilteredElementCollector(doc);
            //var view3D = collector.OfClass(typeof(View3D)).Cast<View3D>().First<View3D>(v3 => !(v3.IsTemplate));
            BoundingBoxXYZ box = room.get_BoundingBox(null);
            XYZ center = box.Min.Add(box.Max).Multiply(0.5);
            center = new XYZ(center.X, center.Y, room.Level.Elevation);
            return center;
        }

        private static Outline GetRoomMaxBoundingBox(Room room)
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
        /// 通过IFC工具 获取Room的Curveloop属性
        /// 房间的CsurveLoop 可能有自交的情况（自交的情况下不能拉伸Solid），通过调整SpatialElementBoundaryLocation.Center可以消除
        /// </summary>
        /// <param name="room"></param>
        /// <returns>返回该房间边界的CurveLoop，包括墙、分隔线、柱等</returns>
        public static IList<CurveLoop> GetRoomCurveLoopBoundary(Room room)
        {
            var bndOpt = new SpatialElementBoundaryOptions();
            foreach (SpatialElementBoundaryLocation spl in Enum.GetValues(typeof(SpatialElementBoundaryLocation)))
            {
                //获取房间边界的定位点，可以是边界、中心、核心层中心、核心层边界等
                bndOpt.SpatialElementBoundaryLocation = spl;
                try
                {
                    var loops = ExporterIFCUtils.GetRoomBoundaryAsCurveLoopArray(room, bndOpt, true);

                    // 验证CurveLoop是否合法（因为有可能存在自交的情况）
                    GeometryCreationUtilities.CreateExtrusionGeometry(loops, XYZ.BasisZ, 10);

                    return loops;
                }
                catch (Exception)
                {
                }
            }

            return GetRoomCurveLoopBoundaryByRoomSoild(room);
        }

        /// <summary>
        /// 通过房间 Solid 获取房间底部轮廓
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static IList<CurveLoop> GetRoomCurveLoopBoundaryByRoomSoild(Room room)
        {
            var solid = GetRoomSolid(room);
            var bottomFace = FaceUtils.GetBottomFace(solid);
            if (bottomFace != null)
            {
                var loops = bottomFace.GetEdgesAsCurveLoops();

                return loops;
            }

            //没有找到底面
            return null;
        }

        /// <summary>
        /// 获取房间Solid
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static Solid GetRoomSolid(Room room)
        {
            var bndOpt = new SpatialElementBoundaryOptions();
            SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(room.Document, bndOpt);
            SpatialElementGeometryResults results = calculator.CalculateSpatialElementGeometry(room); // compute the room geometry
            Solid roomSolid = results.GetGeometry(); // get the solid representing the room's geometry

            return roomSolid;
        }

        /// <summary>
        /// 获得房间边界线
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static List<List<Curve>> GetCurveRoomBoundary(Room room)
        {
            List<List<Curve>> curvesOfRoom = new List<List<Curve>>();
            var bndOpt = new SpatialElementBoundaryOptions();
            //bndOpt.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish;    //默认值
            IList<IList<BoundarySegment>> segmentsloops = room.GetBoundarySegments(bndOpt);
            if (null != segmentsloops)
            {
                foreach (IList<BoundarySegment> loop in segmentsloops)
                {
                    List<Curve> curves = new List<Curve>();
                    foreach (BoundarySegment segment in loop)
                    {
                        if (segment != null)
                        {
                            Curve segmentCure = segment.GetCurve();
                            curves.Add(segmentCure);
                        }
                    }
                    curvesOfRoom.Add(curves);
                }
            }
            return curvesOfRoom;
        }

        /// <summary>
        /// 返回所有房间边界线对应的元素
        /// 不能通过 BoundarySegment 找到的，使用射线法找
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static List<List<Element>> GetRoomSurroundingElements(Room room)
        {
            List<List<Element>> elementsList = new List<List<Element>>();
            var bndOpt = new SpatialElementBoundaryOptions();
            //bndOpt.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish;    //默认值
            IList<IList<BoundarySegment>> segmentsloops = room.GetBoundarySegments(bndOpt);
            if (null != segmentsloops)
            {
                int maxLength = GetMaxLoopIndex(segmentsloops);
                int i = 0;
                foreach (IList<BoundarySegment> loop in segmentsloops)
                {
                    var elements = GetElementsByBoundarySegments(room.Document, loop, i++ == maxLength);
                    elementsList.Add(elements);
                }
            }

            return elementsList;
        }

        /// <summary>
        /// 获取房间最外圈的房间边界对应的元素
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static List<Tuple<Element, Curve>> GetRoomMaxSurroundingBoundarySegments(Room room)
        {
            List<Tuple<Element, Curve>> elements = new List<Tuple<Element, Curve>>();

            var bndOpt = new SpatialElementBoundaryOptions();
            //bndOpt.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish;    //默认值
            IList<IList<BoundarySegment>> segmentsloops = room.GetBoundarySegments(bndOpt);
            if (null != segmentsloops)
            {
                int maxLength = GetMaxLoopIndex(segmentsloops);
                var maxLoop = segmentsloops[maxLength];

                var elems = GetElementsByBoundarySegments(room.Document, maxLoop, true);
                var curves = maxLoop.Select(e => e.GetCurve()).ToList();

                for (int i = 0; i < maxLoop.Count; i++)
                {
                    elements.Add(new Tuple<Element, Curve>(elems[i], curves[i]));
                }
            }

            return elements;
        }

        private static int GetMaxLoopIndex(IList<IList<BoundarySegment>> segmentsloops)
        {
            List<double> segmentLength = new List<double>();
            foreach (IList<BoundarySegment> loop in segmentsloops)
            {
                double length = 0;
                foreach (var segment in loop)
                {
                    Curve segmentCure = segment.GetCurve();
                    length += segmentCure.ApproximateLength;
                }
                segmentLength.Add(length);
            }
            var maxLength = segmentLength.IndexOf(segmentLength.Max());
            return maxLength;
        }

        private static List<Element> GetElementsByBoundarySegments(Document document, IList<BoundarySegment> loop, bool isMaxLoop)
        {
            List<Element> elements = new List<Element>();
            foreach (BoundarySegment segment in loop)
            {
                if (segment != null)
                {
                    Element element = document.GetElement(segment.ElementId);
                    if (element == null)
                    {
                        element = document.GetElement(segment.LinkElementId);
                    }

                    if (element == null)
                    {//使用射线法找到该 segment 对应的元素
                        Curve segmentCure = segment.GetCurve();
                        element = GetSegmentElementByRay(segmentCure, document, isMaxLoop);
                    }
                    elements.Add(element);
                }
            }

            return elements;
        }

        private static Element GetSegmentElementByRay(Curve segmentCure, Document document, bool isMaxCurveLoop)
        {
            Element boundaryElement = null;

            double stepInRoom = 0.1;
            XYZ direction = (segmentCure.GetEndPoint(1) - segmentCure.GetEndPoint(0)).Normalize();
            var leftDirection = new XYZ(-direction.Y, direction.X, direction.Z);
            XYZ upDir = 1 * XYZ.BasisZ;

            XYZ toRoomVec = isMaxCurveLoop ? stepInRoom * leftDirection : stepInRoom * leftDirection.Negate();
            XYZ pointBottomInRoom = segmentCure.Evaluate(0.5, true) + toRoomVec;
            XYZ startPoint = pointBottomInRoom + upDir;

            //默认过滤 墙和柱子
            List<ElementFilter> filters = new List<ElementFilter>{
                new ElementCategoryFilter(BuiltInCategory.OST_Walls),
                new ElementCategoryFilter(BuiltInCategory.OST_Columns),
                new ElementCategoryFilter(BuiltInCategory.OST_Doors),
                new ElementCategoryFilter(BuiltInCategory.OST_CurtainWallPanels),   //幕墙嵌板
            };
            LogicalOrFilter orFilter = new LogicalOrFilter(filters);

            FilteredElementCollector collector = new FilteredElementCollector(document);
            var view3D = collector.OfClass(typeof(View3D)).Cast<View3D>().First<View3D>(v3 => !(v3.IsTemplate));
            ReferenceIntersector intersector = new ReferenceIntersector(orFilter, FindReferenceTarget.Element, view3D);
            intersector.FindReferencesInRevitLinks = document.IsLinked;

            XYZ toWallDir = isMaxCurveLoop ? leftDirection.Negate() : leftDirection;
            ReferenceWithContext context = intersector.FindNearest(startPoint, toWallDir);

            if (context != null)
            {
                if ((context.Proximity > 10e-6) &&
                    (context.Proximity < 0.01 + stepInRoom))
                {
                    var reference = context.GetReference();
                    if (reference != null)
                    {
                        boundaryElement = document.GetElement(reference);
                    }
                }
            }

            return boundaryElement;
        }

        /// <summary>
        /// 获取房间最外圈的元素
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static List<Element> GetRoomMaxSurroundingElements(Room room)
        {
            var list = GetRoomMaxSurroundingBoundarySegments(room);
            return list.Select(e => e.Item1).ToList();
        }

        /// <summary>
        /// 获取房间最外圈的CurveLoop
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static CurveLoop GetRoomMaxCurveLooop(Room room)
        {
            var loops = GetRoomCurveLoopBoundary(room);
            if (loops != null && loops.Count > 0)
            {
                return loops.OrderBy(x =>
                ExporterIFCUtils.ComputeAreaOfCurveLoops(new List<CurveLoop> { x })).LastOrDefault();
            }
            return null;
        }

        /// <summary>
        /// 获得组成房间的墙
        /// </summary>
        /// <param name="room"></param>
        /// <returns>只返回房间边界中的墙</returns>
        public static List<Wall> GetRoomSurroundingWalls(Room room)
        {
            List<Wall> wallsOfRoom = new List<Wall>();
            IList<IList<BoundarySegment>> segmentsloops = room.GetBoundarySegments(new SpatialElementBoundaryOptions());
            if (null != segmentsloops)
            {
                foreach (IList<BoundarySegment> loop in segmentsloops)
                {
                    foreach (BoundarySegment segment in loop)
                    {
                        Element wallElement = room.Document.GetElement(segment.ElementId);
                        if (wallElement != null && wallElement.Category != null &&
                            wallElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Walls)
                        {
                            if (wallElement != null)
                            {
                                wallsOfRoom.Add(wallElement as Wall);
                            }
                        }
                    }
                }
            }
            return wallsOfRoom;
        }

        /// <summary>
        /// 判断所给房间是否有吊顶
        /// </summary>
        /// <param name="room"></param>
        /// <param name="document">吊顶在该document中找，默认为room.Document，如果给定房间和吊顶不在同一个文件中（例如有一个在链接文件中），需要给定这个参数</param>
        /// <returns></returns>
        public static bool HasCeiling(Room room)
        {
            var ceilings = GetElementsInRoom(room, BuiltInCategory.OST_Ceilings, room.Document);
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
                return double.NaN;
            }

            FilteredElementCollector collector = new FilteredElementCollector(room.Document);
            var view3D = collector.OfClass(typeof(View3D)).Cast<View3D>().First<View3D>(v3 => !(v3.IsTemplate));
            ElementFilter ceilingFilter = new ElementCategoryFilter(BuiltInCategory.OST_Ceilings);

            ReferenceIntersector referenceIntersector = new ReferenceIntersector(ceilingFilter, FindReferenceTarget.Element, view3D);
            referenceIntersector.FindReferencesInRevitLinks = room.Document.IsLinked;

            var start = GetRoomCenterPoint(room);
            XYZ rayDirection = new XYZ(0, 0, 1);

            IList<ReferenceWithContext> references = referenceIntersector.Find(start, rayDirection);
            double distance = Double.PositiveInfinity;  //储存距离
            XYZ intersection = null;                    //储存交点
            foreach (ReferenceWithContext referenceWithContext in references)
            {
                Reference reference = referenceWithContext.GetReference();
                double proximity = referenceWithContext.Proximity;
                if (proximity < distance && proximity > 0)
                {
                    distance = proximity;
                    intersection = reference.GlobalPoint;
                }
            }
            return distance;
        }

        /// <summary>
        /// 获得房间的最外圈轮廓，组成房间轮廓的元素只包含墙和房间边界，并且会忽略长度小于300的边界
        /// </summary>
        /// <param name="room"></param>
        /// <returns>如果能构造出 CurveLoop ，返回构造的 CurveLoop ； 如果不能构造，放回房间真实的 CurveLoop </returns>
        public static CurveLoop GetRoomMaxCurveLooopWithoutColumns(Room room)
        {
            var maxBoundarySegments = GetRoomMaxSurroundingBoundarySegments(room);
            if (maxBoundarySegments == null)
            {
                return null;
            }

            List<Curve> curves = new List<Curve>();
            List<Curve> spareCurves = new List<Curve>();

            foreach (var boundarySegment in maxBoundarySegments)
            {
                var element = boundarySegment.Item1;
                if (element != null && (
                    element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Walls ||
                    element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_RoomSeparationLines)
                    )
                {
                    if (boundarySegment.Item2.ApproximateLength >= MathHelper.Mm2Foot * 300)
                    {
                        curves.Add(boundarySegment.Item2);
                    }
                }

                spareCurves.Add(boundarySegment.Item2);
            }

            CurveLoop spareCurveLoop = null;
            try
            {
                spareCurveLoop = CurveLoop.Create(spareCurves);
                // 验证CurveLoop是否合法（因为有可能存在自交的情况）
                GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { spareCurveLoop }, XYZ.BasisZ, 10);
            }
            catch { spareCurveLoop = null; }
#if DEBUG
            if (spareCurveLoop != null)
            {
                int index = 1;
                foreach (var item in spareCurveLoop)
                {
                    System.Diagnostics.Debug.WriteLine($"第{index++}条边：\n起点：{item.GetEndPoint(0).ToString()}\n终点：{item.GetEndPoint(1).ToString()}\n");
                }
            }
#endif

            if (curves.Count < 4)
            {
                return spareCurveLoop;
            }
            else
            {
                //调整curves，补全被去除的柱子所形成的线
                curves = AdjustCurveList(curves);
            }

            CurveLoop curveLoop = null;
            try
            {
                curveLoop = CurveLoop.Create(curves);

#if DEBUG
                if (curveLoop != null)
                {
                    int index = 1;
                    foreach (var item in curves)
                    {
                        System.Diagnostics.Debug.WriteLine($"第{index++}条边：\n起点：{item.GetEndPoint(0).ToString()}\n终点：{item.GetEndPoint(1).ToString()}\n");
                    }
                }
#endif
                // 验证CurveLoop是否合法（因为有可能存在自交的情况）
                GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { curveLoop }, XYZ.BasisZ, 10);
            }
            catch { curveLoop = null; }


            return curveLoop == null ? spareCurveLoop : curveLoop;
        }

        private static List<Curve> AdjustCurveList(List<Curve> curves)
        {
            List<Curve> lines = new List<Curve>();

            var enumerator = curves.GetEnumerator();
            Queue<Curve> queue = new Queue<Curve>(2);
            while (enumerator.MoveNext())
            {
                Curve currCurve = enumerator.Current;
                queue.Enqueue(currCurve);

                if (queue.Count != 2)
                {
                    continue;
                }

                Curve preCurve = queue.Dequeue();
                //处理当前两条直线
                DealWithCurrTwoCurvres(ref currCurve, ref preCurve, out Line tempLine);

                lines.Add(preCurve);
                if (tempLine != null) lines.Add(tempLine);

                //更新一个在队列中的Curve
                queue.Dequeue();
                queue.Enqueue(currCurve);
            }

#if DEBUG
            int index = 1;
            foreach (var item in lines)
            {
                System.Diagnostics.Debug.WriteLine($"第{index++}条边：\n起点：{item.GetEndPoint(0).ToString()}\n终点：{item.GetEndPoint(1).ToString()}\n");
            }
#endif

            //while循环之后 需要判断第一条和最后一条直线的关系
            Curve lastCurve = queue.Dequeue();
            Curve firstCurve = lines.First();
            bool isMoved2 = DealWithCurrTwoCurvres(ref firstCurve, ref lastCurve, out Line tempLine2);

            lines.Add(lastCurve);
            if (tempLine2 != null) lines.Add(tempLine2);
            //把firstCurve替换了
            lines.RemoveAt(0);
            lines.Insert(0, firstCurve);

            //如果移动过currCurve(firstCurve)
            if (isMoved2)
            {
                List<Curve> parallelFirstCurves = new List<Curve>();    //第一条线 后面(相邻的) 和第一条线平行的线
                Curve notParallelFirstCurve = null;                     //第一条线 后面 第一个和他不平行的线
                for (int i = 1; i < lines.Count; i++)
                {
                    var tempLine = lines[i--];
                    lines.Remove(tempLine);

                    if (CurveUtils.CheckParallel(firstCurve, tempLine))
                    {
                        parallelFirstCurves.Add(tempLine);
                        continue;
                    }
                    else
                    {
                        notParallelFirstCurve = tempLine;
                        break;
                    }
                }

                if (parallelFirstCurves.Count == 0)
                {
                    ExtendTwoCurves(ref notParallelFirstCurve, ref firstCurve);
                    lines.Insert(1, notParallelFirstCurve);
                }
                else
                {//先把所有和第一条直线平行的线，平移一定距离
                    double distance = CurveUtils.GetParallelLinesDistance(firstCurve, parallelFirstCurves.First());
                    XYZ dir = CurveUtils.GetCurveNormal(parallelFirstCurves.First(), firstCurve.GetEndPoint(0));
                    for (int i = 0; i < parallelFirstCurves.Count; i++)
                    {
                        parallelFirstCurves[i] = parallelFirstCurves[i].CreateTransformed(Transform.CreateTranslation(dir * distance));
                    }

                    //自后向前一次重新加入
                    var lastParallelCurve = parallelFirstCurves.Last();
                    ExtendTwoCurves(ref notParallelFirstCurve, ref lastParallelCurve);
                    lines.Insert(1, notParallelFirstCurve);

                    for (int i = parallelFirstCurves.Count - 1; i > -1; i--)
                    {
                        lines.Insert(1, parallelFirstCurves[i]);
                    }
                }
            }

            return lines;
        }

        private static bool DealWithCurrTwoCurvres(ref Curve currCurve, ref Curve preCurve, out Line tempLine)
        {
            bool isMovedCurrCurve = false;
            tempLine = null;

            if (preCurve.GetEndPoint(1).IsAlmostEqualTo(currCurve.GetEndPoint(0)))
            {//首尾相连，不需要处理
            }
            else
            {//需要处理
                bool isParallel = CurveUtils.CheckParallel(preCurve, currCurve);
                if (isParallel)
                {//平行，求距离，距离小于300mm，将第二条直线移动到第一条直线的延长线上；否则直接相连
                    double distance = CurveUtils.GetParallelLinesDistance(preCurve, currCurve);
                    if (distance <= 300 * MathHelper.Mm2Foot /*&&
                        distance > 1e-6*/)
                    {
                        //先把第二条直线移动到和前一条直线共线
                        XYZ dir = CurveUtils.GetCurveNormal(currCurve, preCurve.GetEndPoint(0));
                        currCurve = currCurve.CreateTransformed(Transform.CreateTranslation(dir * distance));

                        isMovedCurrCurve = true;
                    }

                    //直接相连
                    var pts = CurveUtils.GetNearestPointsByTwoLine(preCurve, currCurve);
                    if (!pts[0].IsAlmostEqualTo(pts[1]))
                    {
                        tempLine = Line.CreateBound(pts[0], pts[1]);
                    }
                }
                else
                {//不平行，延长两条线，求交点
                    ExtendTwoCurves(ref currCurve, ref preCurve);
                }
            }

            return isMovedCurrCurve;
        }

        private static void ExtendTwoCurves(ref Curve currCurve, ref Curve preCurve)
        {
            Curve preTemp = preCurve.Clone();
            preTemp.MakeUnbound();
            Curve currTemp = currCurve.Clone();
            currTemp.MakeUnbound();

            _ = preTemp.Intersect(currTemp, out IntersectionResultArray resultArray);
            XYZ point = resultArray.get_Item(0).XYZPoint;  //交点


            IntersectionResult intersectionResult_0, intersectionResult_1;
            double start, end;

            //preCurve
            intersectionResult_0 = preTemp.Project(preCurve.GetEndPoint(0));
            start = intersectionResult_0.Parameter;
            intersectionResult_1 = preTemp.Project(point);
            end = intersectionResult_1.Parameter;
            preTemp.MakeBound(start, end);
            preCurve = preTemp.Clone();

            //currCurve
            intersectionResult_0 = currTemp.Project(point);
            start = intersectionResult_0.Parameter;
            intersectionResult_1 = currTemp.Project(currCurve.GetEndPoint(1));
            end = intersectionResult_1.Parameter;
            currTemp.MakeBound(start, end);
            currCurve = currTemp.Clone();
        }
    }
}
