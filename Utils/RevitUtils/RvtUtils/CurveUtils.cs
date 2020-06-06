using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitUtils.RvtUtils
{
    public static class CurveUtils
    {
        /// <summary>
        /// 判断两条曲线是否垂直
        /// </summary>
        /// <param name="curve1"></param>
        /// <param name="curve2"></param>
        /// <returns></returns>
        public static bool CheckVertical(Curve curve1, Curve curve2)
        {
            if (curve1 is Line && curve2 is Line)
            {
                XYZ xyz1 = (curve1 as Line).Direction;
                XYZ xyz2 = (curve2 as Line).Direction;
                return VectorUtiles.CheckVertical(xyz1, xyz2);
            }
            return false;
        }

        /// <summary>
        /// 判断两条曲线是否平行
        /// </summary>
        /// <param name="curve1"></param>
        /// <param name="curve2"></param>
        /// <returns></returns>
        public static bool CheckParallel(Curve curve1, Curve curve2)
        {
            if (curve1 is Line && curve2 is Line)
            {
                XYZ xyz1 = (curve1 as Line).Direction;
                XYZ xyz2 = (curve2 as Line).Direction;
                return VectorUtiles.CheckParallel(xyz1, xyz2);
            }
            return false;
        }

        /// <summary>
        /// 计算两条平行线的距离
        /// </summary>
        /// <returns></returns>
        public static double GetParallelLinesDistance(Curve line1, Curve line2)
        {
            if (!CheckParallel(line1, line2))
            {
                return double.NaN;
            }

            //Curve temp = line1.Clone();
            //temp.MakeUnbound();
            //double dis = temp.Distance(line2.GetEndPoint(0));

            //平行一定是直线
            XYZ direction = (line1 as Line).Direction;
            XYZ start = line1.GetEndPoint(0);
            Line tempLine1 = Line.CreateUnbound(start, direction);
            Line tempLine2 = Line.CreateUnbound(start, direction.Negate());

            double dis1 = tempLine1.Distance(line2.GetEndPoint(0));
            double dis2 = tempLine2.Distance(line2.GetEndPoint(0));

            return Math.Min(dis1, dis2);
        }

        /// <summary>
        /// 获取 给定曲线在xoy平面内，给定点一侧的法线，向量已单位化
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static XYZ GetCurveNormal(Curve curve, XYZ point)
        {
            XYZ nor = null;
            if (curve is Line)
            {
                Line line = curve as Line;
                Plane plane = Plane.CreateByOriginAndBasis(line.GetEndPoint(0), XYZ.BasisZ, line.Direction);
                nor = plane.Normal;
            }
            else
            {
                var transf = curve.ComputeDerivatives(0.5, true);
                nor = transf.BasisY.Normalize();
            }

            XYZ start = curve.GetEndPoint(0);
            XYZ tempVector = new XYZ(point.X, point.Y, start.Z) - start;     //指向p点，并忽略高度
            if (tempVector.DotProduct(nor) > 0)
            {
                //夹角小于90°
                return nor;
            }
            else
            {
                return nor.Negate();
            }
        }

        /// <summary>
        /// 获得 CurveLoop 的所有点
        /// </summary>
        /// <param name="loop"></param>
        /// <returns></returns>
        public static IList<XYZ> ToPointsList(this CurveLoop loop)
        {
            int n = loop.Count();
            List<XYZ> polygon = new List<XYZ>();

            foreach (Curve e in loop)
            {
                IList<XYZ> pts = e.Tessellate();

                n = polygon.Count;

                if (0 < n)
                {
                    //Debug.Assert(pts[0].IsAlmostEqualTo(polygon[n - 1]),
                    //  "expected last edge end point to equal next edge start point");

                    polygon.RemoveAt(n - 1);
                }
                polygon.AddRange(pts);
            }
            n = polygon.Count;

            //Debug.Assert(polygon[0].IsAlmostEqualTo(polygon[n - 1]),
            //  "expected first edge start point to equal last edge end point");

            polygon.RemoveAt(n - 1);

            return polygon;
        }

        /// <summary>
        /// 将一组点构造成一个首位相连的 CurveLoop
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static CurveLoop ToCurveLoop(this IList<XYZ> points)
        {
            List<Curve> curves = new List<Curve>();
            Queue<XYZ> queue = new Queue<XYZ>(2);
            var enumerator = points.GetEnumerator();
            while (enumerator.MoveNext())
            {
                XYZ curr = enumerator.Current;
                queue.Enqueue(curr);
                if (queue.Count == 2)
                {
                    XYZ prePoint = queue.Dequeue();
                    Line line = Line.CreateBound(prePoint, curr);
                    curves.Add(line);
                }
            }
            XYZ lastPoint = queue.Dequeue();
            XYZ firstPoint = points[0];
            Line line1 = Line.CreateBound(lastPoint, firstPoint);
            curves.Add(line1);

            return CurveLoop.Create(curves);
        }

        /// <summary>
        /// 将CurveLoop压平在xoy平面
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static CurveLoop ProjectXoy(this CurveLoop loop)
        {
            var points = loop.ToPointsList();
            List<XYZ> newPoints = new List<XYZ>();
            foreach (var p in points)
            {
                var temp = p.ProjectXoy();
                newPoints.Add(temp);
            }

            return newPoints.ToCurveLoop();
        }

        /// <summary>
        /// 获取一个点在XOY平面上的投影点
        /// </summary>
        /// <param name="xyz"></param>
        /// <returns></returns>
        public static XYZ ProjectXoy(this XYZ xyz)
        {
            if (xyz == null)
            {
                return null;
            }
            return new XYZ(xyz.X, xyz.Y, 0);
        }

        public static UV ToUV(this XYZ xYZ)
        {
            if (xYZ == null)
                return UV.Zero;

            UV uv = new UV(xYZ.X, xYZ.Y);
            return uv;
        }

        #region Useless Method
        /// <summary>
        /// 获取两条曲线的顶点最接近的两个点
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public static XYZ[] GetNearestPointsByTwoLine(Curve line1, Curve line2)
        {
            var p1 = line1.GetEndPoint(0);
            var p2 = line1.GetEndPoint(1);
            XYZ[] c1 = new XYZ[2] { p1, p2 };

            var p3 = line2.GetEndPoint(0);
            var p4 = line2.GetEndPoint(1);
            XYZ[] c2 = new XYZ[2] { p3, p4 };

            XYZ pt1 = null, pt2 = null;
            double minDis = double.MaxValue;
            foreach (var i in c1)
            {
                foreach (var j in c2)
                {
                    var distance = i.DistanceTo(j);
                    if (distance < minDis)
                    {
                        minDis = distance;
                        pt1 = i;
                        pt2 = j;
                    }
                }
            }

            return new XYZ[2] { pt1, pt2 };
        }

        /// <summary>
        /// 获取两条曲线的顶点最远的两个点
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns>[0]是line1上的点，[1]是line2上的点</returns>
        public static XYZ[] GetFarthestPointsByTwoLine(Curve line1, Curve line2)
        {
            var p1 = line1.GetEndPoint(0);
            var p2 = line1.GetEndPoint(1);
            XYZ[] c1 = new XYZ[2] { p1, p2 };

            var p3 = line2.GetEndPoint(0);
            var p4 = line2.GetEndPoint(1);
            XYZ[] c2 = new XYZ[2] { p3, p4 };

            XYZ pt1 = null, pt2 = null;
            double maxDis = 0;
            foreach (var i in c1)
            {
                foreach (var j in c2)
                {
                    var distance = i.DistanceTo(j);
                    if (distance > maxDis)
                    {
                        maxDis = distance;
                        pt1 = i;
                        pt2 = j;
                    }
                }
            }

            return new XYZ[2] { pt1, pt2 };
        }

        /// 找的该曲线的两个端点中，距离目标点较近的端点
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static XYZ GetNearestPoint(this Curve curve, XYZ target)
        {
            var p1 = curve.GetEndPoint(0);
            var p2 = curve.GetEndPoint(1);
            XYZ[] c1 = new XYZ[2] { p1, p2 };

            XYZ pt1 = null;
            double minDis = double.MaxValue;
            foreach (var i in c1)
            {
                var distance = i.DistanceTo(target);
                if (distance < minDis)
                {
                    minDis = distance;
                    pt1 = i;
                }
            }

            return pt1;
        }
        #endregion
    }
}
