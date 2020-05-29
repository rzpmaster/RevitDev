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
    /// 下拉记忆按钮
    /// </summary>
    class SplitButtonData : IRvtRibbonItem
    {
        #region Data
        Autodesk.Revit.UI.SplitButtonData splitButton;
        XElement xnode;
        String relativeRoot;
        #endregion

        #region Prop
        public Autodesk.Revit.UI.SplitButtonData SplitButton
        {
            get
            {
                return splitButton;
            }
            private set
            {
                splitButton = value;
            }
        }
        #endregion

        public SplitButtonData(XElement xe, String root)
        {
            xnode = xe;
            relativeRoot = root;

            SplitButton = new Autodesk.Revit.UI.SplitButtonData(xe.Attribute("name").Value, xe.Attribute("text").Value);
        }

        public void AddRibbonItemToPanel(ref RibbonPanel panel, string tabname)
        {
            if (null == panel)
                return;

            Autodesk.Revit.UI.SplitButton sb = panel.AddItem(splitButton) as Autodesk.Revit.UI.SplitButton;

            foreach (var e in xnode.Elements())
            {
                PushButtonData obj = new PushButtonData(e, relativeRoot);
                PushButton pb_atom = sb.AddPushButton(obj.PushButton);

                pb_atom.Enabled = true;
            }
        }
    }
}
