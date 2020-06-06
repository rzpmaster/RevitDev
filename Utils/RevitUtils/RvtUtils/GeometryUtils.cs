using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitUtils.RvtUtils
{
    public class GeometryUtils
    {
        public static Solid GetSolid(Element e)
        {
            GeometryElement geo = e.get_Geometry(new Options());

            // 有些Element没有几何实体，必须从族类型中获取
            var solid = GetGeometryElementSolid(geo);

            return solid;
        }

        public static Solid GetGeometryElementSolid(GeometryElement geoElem)
        {
            Solid solid = null;

            foreach (GeometryObject obj in geoElem)
            {
                if (obj is Solid)
                {
                    solid = obj as Solid;
                    if (null != solid && 0 < solid.Faces.Size)
                    {
                        break;
                    }
                }
                else if (obj is GeometryInstance)
                {
                    GeometryElement instGeo = (obj as GeometryInstance).GetInstanceGeometry();
                    solid = GetGeometryElementSolid(instGeo);
                    if (null != solid && 0 < solid.Faces.Size)
                    {
                        break;
                    }
                    else
                    {
                        GeometryElement syGeo = (obj as GeometryInstance).GetSymbolGeometry();
                        solid = GetGeometryElementSolid(syGeo);
                    }
                }
            }

            return solid;
        }

        public static IEnumerable<Solid> GetSolids(Element element)
        {
            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            options.ComputeReferences = true;

            GeometryElement geomElem = element.get_Geometry(options);
            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj == null)
                {
                    continue;
                }

                if (geomObj is Solid)
                {
                    Solid solid = geomObj as Solid;
                    if (solid != null && solid.Faces.Size > 0 && solid.Volume > 0.0)
                    {
                        yield return solid;
                    }
                }
                else if (geomObj is GeometryInstance)
                {
                    GeometryInstance geomInst = geomObj as GeometryInstance;
                    GeometryElement instGeomElem = geomInst.GetInstanceGeometry();
                    foreach (GeometryObject instGeomObj in instGeomElem)
                    {
                        if (instGeomObj == null)
                        {
                            continue;
                        }

                        if (instGeomObj is Solid)
                        {
                            Solid solid = instGeomObj as Solid;
                            if (solid != null && solid.Faces.Size > 0 && solid.Volume > 0.0)
                            {
                                yield return solid;
                            }
                        }
                    }
                }
            }

            yield break;
        }

        /// <summary>
        /// 判断一个点是否在多边形内部（xoy平面内）
        /// http://alienryderflex.com/polygon/
        /// https://github.com/wieslawsoltes/Math.Spatial/blob/master/src/Math.Spatial/Polygon2.cs
        /// </summary>
        /// <param name="polygon">多边形顶点集合</param>
        /// <param name="point">要判断的点</param>
        /// <returns></returns>
        public static bool IsPointInPolygon(List<XYZ> polygon, XYZ point)
        {
            bool contains = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y))
                    && (point.X < (((polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y)) / (polygon[j].Y - polygon[i].Y)) + polygon[i].X))
                {
                    contains = !contains;
                }
            }
            return contains;
        }

        /// <summary>
        /// 判断一个点是否在多边形内部（xoy平面内）
        /// based on http://geomalgorithms.com/a03-_inclusion.html	
        /// </summary>
        /// <param name="loop"></param>
        /// <param name="P"></param>
        /// <returns></returns>
        private static bool IsPointInPolygon(CurveLoop loop, UV P)
        {
            if (loop == null)
                return false;
            int nWindingNumber = 0;
            UV a = UV.Zero;
            UV b = UV.Zero;
            foreach (var curve in loop)
            {
                List<XYZ> listPt = curve.Tessellate().ToList();
                for (int j = 0; j < listPt.Count - 1; j++)
                {
                    XYZ p1 = listPt[j];
                    XYZ p2 = listPt[j + 1];
                    a = listPt[j].ToUV();
                    b = listPt[(j + 1)].ToUV();

                    if (a.V <= P.V)
                    {   // y <= P.y (below)
                        if (b.V > P.V)
                        {                         // an upward crossing
                            if (IsLeft(a, b, P) > 0)  // P left of edge
                                ++nWindingNumber;                                      // have a valid up intersect
                        }
                    }
                    else
                    {    // y > P.y  (above)
                        if (b.V <= P.V)
                        {                        // a downward crossing
                            if (IsLeft(a, b, P) < 0)  // P right of edge
                                --nWindingNumber;                                      // have a valid down intersect
                        }
                    }
                    a = b;
                }

            }
            return nWindingNumber != 0;
        }

        private static int IsLeft(UV P0, UV P1, UV P2)
        {
            return Math.Sign(((P1.U - P0.U) * (P2.V - P0.V) - (P2.U - P0.U) * (P1.V - P0.V)));
        }

        /// <summary>
        /// 判断一个点是否在多边形内部（CurveLoop平面）
        /// </summary>
        /// <param name="loop"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private static bool IsPointInPolygon(CurveLoop loop, XYZ point)
        {
            //不考虑z值
            point = new XYZ(point.X, point.Y, loop.First().GetEndPoint(0).Z);

            //过点Point随便做条射线，判断与多边形的交点个数
            Line line = Line.CreateUnbound(point, new XYZ(1, 0, 0));//注意这里构造的是直线

            //储存交点
            List<XYZ> interPoints = new List<XYZ>();
            IntersectionResultArray resultArray = new IntersectionResultArray();
            foreach (Curve curve in loop)
            {
                SetComparisonResult intRst = line.Intersect(curve, out resultArray);
                if (intRst == SetComparisonResult.Overlap)
                {
                    foreach (IntersectionResult result in resultArray)
                    {
                        if (resultArray.IsEmpty)
                            continue;
                        XYZ xyz = result.XYZPoint;

                        //判断交点和点是不是相等
                        if (xyz.IsAlmostEqualTo(point))
                        {
                            //说明在点在边上，这里当作内部处理
                            return true;
                        }
                        interPoints.Add(xyz);
                    }
                }
            }
            int num = 0;
            //判断X坐标比Point的X坐标小的
            foreach (XYZ p in interPoints)
            {
                if (p.X < point.X)
                    num++;
            }
            //交点个数是奇数，则在内；偶数，内在外
            if (num % 2 == 1)
                return true;
            return false;
        }
    }
}
