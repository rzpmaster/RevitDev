using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace RevitRibbonsUI
{
    /// <summary>
    /// 按钮
    /// </summary>
    class PushButtonData : IRvtRibbonItem
    {
        #region Data
        Autodesk.Revit.UI.PushButtonData pushButton;
        XElement xnode;
        String relativeRoot;
        String uiButtonIcon;
        #endregion

        #region Property
        public Autodesk.Revit.UI.PushButtonData PushButton
        {
            get
            {
                return pushButton;
            }
            private set
            {
                pushButton = value;
            }
        }
        #endregion

        public PushButtonData(XElement xe, String root)
        {
            xnode = xe;
            relativeRoot = root;
            uiButtonIcon = System.IO.Path.Combine(root, "Icons");

            //PushButton Name
            String pbText = xe.Element("Text").Value;
            pbText = pbText.Replace("\\n", "\n");

            //解析xml得到与之绑定的dll
            var ns = System.IO.Path.Combine(root, xe.Element("AssemblyName").Value);

            //创建PushButtonData
            PushButton = new Autodesk.Revit.UI.PushButtonData(xe.Element("Name").Value, pbText, ns, xe.Element("ClassName").Value);

            AddImage();
        }

        private void AddImage()
        {
            //绑定Image
            String nodeValue = null == xnode.Element("Image") ? String.Empty
                                : xnode.Element("Image").Value;
            if (!String.IsNullOrEmpty(nodeValue) &&
                (nodeValue.EndsWith(".ico") || nodeValue.EndsWith(".png") || nodeValue.EndsWith(".jpg") || nodeValue.EndsWith(".bmp")))
                pushButton.Image = new BitmapImage(new Uri(System.IO.Path.Combine(uiButtonIcon, nodeValue)));

            //绑定LargeImage
            nodeValue = null == xnode.Element("LargeImage") ? String.Empty
                                : xnode.Element("LargeImage").Value;
            if (!String.IsNullOrEmpty(nodeValue) &&
                (nodeValue.EndsWith(".ico") || nodeValue.EndsWith(".png") || nodeValue.EndsWith(".jpg") || nodeValue.EndsWith(".bmp")))
                pushButton.LargeImage = new BitmapImage(new Uri(System.IO.Path.Combine(uiButtonIcon, nodeValue)));

            //绑定ToolTip
            nodeValue = null == xnode.Element("ToolTip") ? String.Empty
                                : xnode.Element("ToolTip").Value;
            if (!String.IsNullOrEmpty(nodeValue))
                pushButton.ToolTip = nodeValue;
        }

        public void AddRibbonItemToPanel(ref RibbonPanel panel, string tabname)
        {
            if (null == panel)
                return;

            //将PushButton放入RibbonPanel中
            Autodesk.Revit.UI.PushButton pb = panel.AddItem(PushButton) as Autodesk.Revit.UI.PushButton;

            //设置PushButton为可用
            pb.Enabled = true;
        }
    }
}
