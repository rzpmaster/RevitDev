using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitUtils.RvtUtils
{
    public static class SwitchViewUtils
    {
        public static void ZoomToFitElementsInView(UIDocument UIDoc, View view, List<Element> elements, Double zoomFactor = 0.8)
        {
            if (null == UIDoc || null == view)
            {
                return;
            }

            //跳转视图
            UIDoc.ActiveView = view;
            //zoom
            ZoomAndFitElements(elements, UIDoc, zoomFactor);
        }

        public static void HighLightElements(UIDocument UIDoc, List<ElementId> elementsToHighLight, bool bIsHighLight)
        {
            if (null == UIDoc)
            {
                return;
            }

            using (Transaction tr = new Transaction(UIDoc.Document, "高亮显示"))
            {
                tr.Start();

                ICollection<ElementId> elementIds = UIDoc.Selection.GetElementIds();

                elementIds.Clear();

                if (bIsHighLight)
                {
                    for (int i = 0; i < elementsToHighLight.Count(); i++)
                    {
                        elementIds.Add(elementsToHighLight[i]);
                    }
                }

                UIDoc.Selection.SetElementIds(elementIds);

                tr.Commit();
            }
        }

        public static void ZoomAndFitElements(List<Element> elements, UIDocument UIDoc, Double zoomFactor = 0.8)
        {
            UIView uiView = GetUIView(UIDoc);

            if (null == uiView)
            {
                return;
            }

            if (null != elements)
            {
                BoundingBoxXYZ bbox = BoundingBoxUtils.GetElementsMaxBounding(elements);

                uiView.ZoomAndCenterRectangle(bbox.Max, bbox.Min);
            }

            uiView.Zoom(zoomFactor);
        }

        private static UIView GetUIView(UIDocument UIDoc)
        {
            IList<UIView> UIViews = UIDoc.GetOpenUIViews();

            UIView uiView = null;

            for (int i = 0; i < UIViews.Count(); i++)
            {
                if (UIViews[i].ViewId == UIDoc.Document.ActiveView.Id)
                {
                    uiView = UIViews[i];
                    break;
                }
            }

            return uiView;
        }
    }
}
