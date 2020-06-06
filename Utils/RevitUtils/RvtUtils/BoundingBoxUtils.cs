using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitUtils.RvtUtils
{
    public static class BoundingBoxUtils
    {
        public static BoundingBoxXYZ GetElementsMaxBounding(List<Element> elements)
        {
            List<BoundingBoxXYZ> bBoxs = elements.Select(e => e.get_BoundingBox(null)).ToList();

            return GetElementsMaxBounding(bBoxs);
        }

        public static BoundingBoxXYZ GetElementsMaxBounding(List<BoundingBoxXYZ> bBoxs)
        {
            double dMaxX = bBoxs[0].Max.X;
            double dMaxY = bBoxs[0].Max.Y;
            double dMaxZ = bBoxs[0].Max.Z;
            double dMinX = bBoxs[0].Min.X;
            double dMinY = bBoxs[0].Min.Y;
            double dMinZ = bBoxs[0].Min.Z;

            for (int i = 1; i < bBoxs.Count(); i++)
            {
                if (bBoxs[i].Max.X > dMaxX)
                {
                    dMaxX = bBoxs[i].Max.X;
                }

                if (bBoxs[i].Max.Y > dMaxY)
                {
                    dMaxY = bBoxs[i].Max.Y;
                }

                if (bBoxs[i].Max.Z > dMaxZ)
                {
                    dMaxZ = bBoxs[i].Max.Z;
                }

                if (bBoxs[i].Min.X < dMinX)
                {
                    dMinX = bBoxs[i].Min.X;
                }

                if (bBoxs[i].Min.Y < dMinY)
                {
                    dMinY = bBoxs[i].Min.Y;
                }

                if (bBoxs[i].Min.Z < dMinZ)
                {
                    dMinZ = bBoxs[i].Min.Z;
                }
            }

            BoundingBoxXYZ retBBox = new BoundingBoxXYZ();
            retBBox.Min = new XYZ(dMinX, dMinY, dMinZ);
            retBBox.Max = new XYZ(dMaxX, dMaxY, dMaxZ);

            return retBBox;
        }

        /// <summary>
        /// 获得element的BoundingBoxXYZ
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static BoundingBoxXYZ GetBoundingBox(this Element element)
        {
            if (null == element)
                return null;

            // 获取实例的BoundingBox
            if (element is FamilyInstance)
            {
                var bBox = new BoundingBoxXYZ() { Enabled = false };
                var solids = GeometryUtils.GetSolids(element);
                foreach (var solid in solids)
                {
                    if (solid.Faces.Size <= 0 || solid.Volume <= 1e-6)
                    {
                        continue;
                    }

                    foreach (Edge e in solid.Edges)
                    {
                        var crv = e.AsCurve();
                        bBox.Add(crv.GetEndPoint(0));
                        bBox.Add(crv.GetEndPoint(1));
                    }
                }

                return bBox;
            }

            Options ops = new Options()
            {
                ComputeReferences = true,
                DetailLevel = ViewDetailLevel.Fine
            };

            GeometryElement geoElem = element.get_Geometry(ops);

            return geoElem?.GetBoundingBox();
        }

        /// <summary>
        /// 通过指定点修改包围盒的大小
        /// </summary>
        /// <param name="bBox"></param>
        /// <param name="pt"></param>
        public static void Add(this BoundingBoxXYZ bBox, XYZ pt)
        {
            if (bBox.Enabled)
            {
                bBox.Max = new XYZ(Math.Max(bBox.Max.X, pt.X), Math.Max(bBox.Max.Y, pt.Y), Math.Max(bBox.Max.Z, pt.Z));
                bBox.Min = new XYZ(Math.Min(bBox.Min.X, pt.X), Math.Min(bBox.Min.Y, pt.Y), Math.Min(bBox.Min.Z, pt.Z));
            }
            else
            {
                bBox.Max = pt;
                bBox.Min = pt;
                bBox.Enabled = true;
            }
        }

        /// <summary>
        /// Expand the given bounding box to include and contain the given point.
        /// </summary>
        public static void ExpandToContain(this BoundingBoxXYZ bb, XYZ p)
        {
            bb.Min = new XYZ(Math.Min(bb.Min.X, p.X),
              Math.Min(bb.Min.Y, p.Y),
              Math.Min(bb.Min.Z, p.Z));

            bb.Max = new XYZ(Math.Max(bb.Max.X, p.X),
              Math.Max(bb.Max.Y, p.Y),
              Math.Max(bb.Max.Z, p.Z));
        }

        /// <summary>
        /// whether the point is in bbox
        /// </summary>
        /// <param name="bb"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool Contain(this BoundingBoxXYZ bb, XYZ p)
        {
            if (bb.Max.X > p.X && bb.Max.Y > p.Y && bb.Max.Z > p.Z &&
                bb.Min.X < p.X && bb.Min.Y < p.Y && bb.Min.Z < p.Z)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Expand the given bounding box to include and contain the given other one.
        /// </summary>
        public static void ExpandToContain(this BoundingBoxXYZ bb, BoundingBoxXYZ other)
        {
            bb.ExpandToContain(other.Min);
            bb.ExpandToContain(other.Max);
        }
    }
}
