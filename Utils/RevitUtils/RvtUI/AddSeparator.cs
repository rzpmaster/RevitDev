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
    /// 分割线
    /// </summary>
    class AddSeparator : IRvtRibbonItem
    {
        XElement xnode;
        String relativeRoot;

        public AddSeparator(XElement xe, String root)
        {
            xnode = xe;
            relativeRoot = root;
        }

        public void AddRibbonItemToPanel(ref RibbonPanel panel, string tabname)
        {
            if (null == panel)
                return;

            panel.AddSeparator();
        }
    }
}
