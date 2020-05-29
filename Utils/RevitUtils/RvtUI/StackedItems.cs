using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RevitRibbonsUI
{
    /// <summary>
    /// 层叠按钮（最多3层）
    /// </summary>
    class StackedItems : IRvtRibbonItem
    {
        XElement xnode;
        String relativeRoot;

        public StackedItems(XElement xe, String root)
        {
            xnode = xe;
            relativeRoot = root;

        }

        public void AddRibbonItemToPanel(ref RibbonPanel panel, string tabname)
        {
            if (null == panel)
                return;

            List<RibbonItemData> arr = new List<RibbonItemData>(3);
            int n = 0;
            foreach (var e in xnode.Elements())
            {
                if (n > 2) break;
                PushButtonData obj = new PushButtonData(e, relativeRoot);
                arr.Add(obj.PushButton);
                n++;
            }

            if (arr.Count == 2)
                panel.AddStackedItems(arr[0], arr[1]);
            else if (arr.Count == 3)
                panel.AddStackedItems(arr[0], arr[1], arr[2]);
        }
    }
}
