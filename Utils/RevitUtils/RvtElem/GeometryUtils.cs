using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBIM.Terminal.Common.Utils
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
    }

    public class FaceUtils
    {
        public static FaceArray GetElementSolidFaces(Element elem)
        {
            var solid = GeometryUtils.GetSolid(elem);
            if (null == solid)
            {
                return new FaceArray();
            }

            return solid.Faces;
        }

        public static XYZ FaceNormal(Face f)
        {
            var bbox = f.GetBoundingBox();

            return f.ComputeNormal(bbox.Min);
        }
    }

    public class VectorUtiles
    {
        /// <summary>
        /// 输入构件的方向向量 判断是否垂直
        /// </summary>
        /// <param name="xyz1"></param>
        /// <param name="xyz2"></param>
        /// <returns></returns>
        public static bool CheckVertical(XYZ xyz1, XYZ xyz2)
        {
            return Math.Abs((xyz1.X * xyz2.X + xyz1.Y * xyz2.Y + xyz1.Z * xyz2.Z)) <= 0.0001 ? true : false; //数据精度问题~
        }

        /// <summary>
        /// 判断两向量是否平行
        /// </summary>
        /// <param name="xyz1"></param>
        /// <param name="xyz2"></param>
        /// <returns></returns>
        public static bool CheckParallel(XYZ xyz1, XYZ xyz2)
        {
            bool tag = Math.Abs(xyz1.X * xyz2.Y - xyz1.Y * xyz2.X) < 0.00001 &&
                    Math.Abs(xyz1.Y * xyz2.Z - xyz1.Z * xyz2.Y) < 0.00001 &&
                    Math.Abs(xyz1.X * xyz2.Z - xyz1.Z * xyz2.X) < 0.00001;
            return tag;
        }

        /// <summary>
        /// 计算两条平行线的距离
        /// </summary>
        /// <returns></returns>
        public static double GetDistanceByParallelLine(Line line1,Line line2)
        {
            if (!CheckParallel(line1.Direction,line2.Direction))
            {
                return 0;
            }

            Line unbundLine1 = Line.CreateUnbound(line1.Origin,line1.Direction);
            double dis = unbundLine1.Distance(line2.Origin);

            return dis;
        }
    }
}
