using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvoidObstruction
{
    class Detector
    {
        Document document;
        View3D view3d;
        public Detector(Document document)
        {
            this.document = document;

            FilteredElementCollector collector = new FilteredElementCollector(document);
            view3d = collector.OfClass(typeof(View3D)).Cast<View3D>().FirstOrDefault(e => e != null && !e.IsTemplate);
        }

        public List<ReferenceWithContext> Obstructions(Line boundLine)
        {
            List<ReferenceWithContext> result = new List<ReferenceWithContext>();
            XYZ start = boundLine.GetEndPoint(0);
            XYZ end = boundLine.GetEndPoint(1);
            XYZ direcion = (end - start).Normalize();

            ReferenceIntersector referenceIntersector = new ReferenceIntersector(view3d);
            referenceIntersector.TargetType = FindReferenceTarget.Face;
            IList<ReferenceWithContext> obstructionsOnUnboundLine = referenceIntersector.Find(start, direcion);
            foreach (ReferenceWithContext gRefWithContext in obstructionsOnUnboundLine)
            {
                Reference gRef = gRefWithContext.GetReference();
                //如果点到线段的距离 为0，说明点在直线上
                if (boundLine.Distance(gRef.GlobalPoint) < 1e-9)
                {
                    if (!InArray(result, gRefWithContext))
                    {
                        result.Add(gRefWithContext);
                    }
                }
            }
            result.Sort(CompareReferencesWithContext);

            return result;
        }

        private bool InArray(List<ReferenceWithContext> arr, ReferenceWithContext entry)
        {
            foreach (ReferenceWithContext tmp in arr)
            {
                if (Math.Abs(tmp.Proximity - entry.Proximity) < 1e-9 &&
                    tmp.GetReference().ElementId == entry.GetReference().ElementId)
                {
                    return true;
                }
            }
            return false;
        }

        private int CompareReferencesWithContext(ReferenceWithContext a, ReferenceWithContext b)
        {
            if (a.Proximity > b.Proximity)
            {
                return 1;
            }

            if (a.Proximity < b.Proximity)
            {
                return -1;
            }

            return 0;
        }
    }
}
