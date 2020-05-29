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
    /// 下拉按钮
    /// </summary>
    class PulldownButtonData : IRvtRibbonItem
    {
        #region Data
        Autodesk.Revit.UI.PulldownButtonData pullButton;
        XElement xnode;
        String relativeRoot;
        String uiButtonIcon;
        #endregion

        public PulldownButtonData(XElement xe, String root)
        {
            xnode = xe;
            relativeRoot = root;
            uiButtonIcon = System.IO.Path.Combine(root, "Icons");

            //PulldownButton Name
            String pdbText = xe.Attribute("text").Value;
            pdbText = pdbText.Replace("\\n", "\n");

            //PulldownButton 里面放的PushButton，所以不用绑定dll

            //创建PulldownButtonData
            pullButton = new Autodesk.Revit.UI.PulldownButtonData(
                xe.Attribute("name").Value, pdbText);

            //绑定LargeImage
            AddImage();
        }

        private void AddImage()
        {
            String nodeValue = null == xnode.Attribute("LargeImage") ? String.Empty : xnode.Attribute("LargeImage").Value;
            if (!String.IsNullOrEmpty(nodeValue) &&
                (nodeValue.EndsWith(".ico") || nodeValue.EndsWith(".png") || nodeValue.EndsWith(".jpg") || nodeValue.EndsWith(".bmp")))
                pullButton.LargeImage = new BitmapImage(new Uri(System.IO.Path.Combine(uiButtonIcon, nodeValue)));
        }

        public void AddRibbonItemToPanel(ref RibbonPanel panel, string tabname)
        {
            if (null == panel)
                return;

            //将PulldownButton放入RibbonPanel中
            Autodesk.Revit.UI.PulldownButton pb = panel.AddItem(pullButton) as Autodesk.Revit.UI.PulldownButton;

            //解析xnode子元素，循环创建PushButton，并放入PulldownButton中
            foreach (var e in xnode.Elements())
            {
                //创建PushButtonData对象，在其构造函数中创建了PushButton
                PushButtonData obj = new PushButtonData(e, relativeRoot);
                //将PushButton加入PulldownButton中，接收一下
                PushButton pb_atom = pb.AddPushButton(obj.PushButton);
                //设置可用
                pb_atom.Enabled = true;
            }
        }
    }
}
