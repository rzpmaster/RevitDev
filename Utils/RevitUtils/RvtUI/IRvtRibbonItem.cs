using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitRibbonsUI
{
    public interface IRvtRibbonItem
    {
        /// <summary>
        /// 将RibbonPanel添加到指定tab中
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="tabname"></param>
        void AddRibbonItemToPanel(ref RibbonPanel panel, string tabname);
    }
}
