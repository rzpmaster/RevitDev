using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitUtils.RvtElem
{
    public class BoundingBoxUtils
    {
        public static BoundingBoxXYZ GetElementsBounding(List<Element> elements)
        {
            List<BoundingBoxXYZ> bBoxs = elements.Select(e => e.get_BoundingBox(null)).ToList();
            BoundingBoxXYZ retBBox = GetElementsBounding(bBoxs);

            return retBBox;
        }

        public static BoundingBoxXYZ GetElementsBounding(List<BoundingBoxXYZ> bBoxs)
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
    }
}
