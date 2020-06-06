using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvoidObstruction
{
    /// <summary>
    /// 用于寻找某个方向上的障碍物的类
    /// </summary>
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

        /// <summary>
        /// 寻找给定线段上的障碍物,并按距离排序
        /// 超出线段长度会被忽略
        /// </summary>
        /// <param name="boundLine"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 寻找给定点及给定方向上的障碍物,并按距离排序
        /// 寻找的障碍物的类型为 face ，可以准确的计算距离
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public List<ReferenceWithContext> Obstructions(XYZ origin, XYZ dir)
        {
            List<ReferenceWithContext> result = new List<ReferenceWithContext>();
            ReferenceIntersector referenceIntersector = new ReferenceIntersector(view3d);
            referenceIntersector.TargetType = FindReferenceTarget.Face;
            IList<ReferenceWithContext> obstructionsOnUnboundLine = referenceIntersector.Find(origin, dir);
            foreach (ReferenceWithContext gRef in obstructionsOnUnboundLine)
            {
                if (!InArray(result, gRef))
                {
                    result.Add(gRef);
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
