using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitUtils.RvtUtils
{
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

        public static Face GetElementBottomFace(Element elem)
        {
            return GetElementFaceByDirection(elem, XYZ.BasisZ.Negate());
        }

        public static Face GetElementTopFace(Element elem)
        {
            return GetElementFaceByDirection(elem, XYZ.BasisZ);
        }

        public static Face GetBottomFace(Solid solid)
        {
            return GetSoildFaceByDirection(solid, XYZ.BasisZ.Negate());
        }

        public static Face GetTopFace(Solid solid)
        {
            return GetSoildFaceByDirection(solid, XYZ.BasisZ);
        }

        /// <summary>
        /// 返回以给定方向为法线方向的面
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Face GetElementFaceByDirection(Element elem, XYZ direction)
        {
            Face face = null;
            Face almostFace = null;

            var faceArray = GetElementSolidFaces(elem);
            foreach (Face f in faceArray)
            {
                var nor = FaceNormal(f);
                if (nor.IsAlmostEqualTo(direction))
                {
                    face = f;
                }
                if (nor.AngleTo(direction) < (Math.PI / 2))
                {
                    almostFace = f;
                }
            }

            var relt = face != null ? face : almostFace;

            return relt == null ? null : relt;
        }

        /// <summary>
        /// 返回以给定方向为法线方向的面
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Face GetSoildFaceByDirection(Solid solid,XYZ direction)
        {
            Face face = null;
            Face almostFace = null;

            var faceArray = solid.Faces;
            foreach (Face f in faceArray)
            {
                var nor = FaceNormal(f);
                if (nor.IsAlmostEqualTo(direction))
                {
                    face = f;
                }
                if (nor.AngleTo(direction) < (Math.PI / 2))
                {
                    almostFace = f;
                }
            }

            var relt = face != null ? face : almostFace;

            return relt == null ? null : relt;
        }
    }
}
