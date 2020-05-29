using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RevitRibbonsUI
{
    /// <summary>
    /// 通过解析Debug文件下UIConfig文件夹中的xml文件，创建RvtUI的类
    /// </summary>
    public class RvtUIBuilder
    {
        public static UIControlledApplication uiApp;
        //当前程序集路径
        public static String currentAssemblyPath = typeof(RvtUIBuilder).Assembly.Location;
        //Debug Folder 相对根路径
        public static String relativeRootFolderPath = "";
        public static void BuildRevitRibbonsUI(UIControlledApplication app)
        {
            uiApp = app;
            relativeRootFolderPath = System.IO.Path.GetDirectoryName(currentAssemblyPath);

            //UIConfig文件夹路径
            String uiConfigFolder = System.IO.Path.GetDirectoryName(currentAssemblyPath) + @"\UIConfig";

            if (!System.IO.Directory.Exists(uiConfigFolder))
            {
                System.Windows.MessageBox.Show("Revit Ribbon 配置文件不存在。\n路径：" + uiConfigFolder);
                return;
            }

            //得到所有xml配置文件
            var xmlFiles = System.IO.Directory.GetFiles(uiConfigFolder, "*.xml");

            foreach (var file in xmlFiles)
            {
                parseXmlConfig(file);
            }
        }

        private static void parseXmlConfig(string filePath)
        {
            XElement root = XElement.Load(filePath);

            try
            {
                Int32 count = 0;
                foreach (var tab in root.Elements())
                {
                    String tabName = tab.Attribute("tabName").Value;
                    
                    Autodesk.Windows.RibbonControl ribbon = Autodesk.Windows.ComponentManager.Ribbon;
                    if (!ribbon.Tabs.Any(a => a.AutomationName == tabName))
                        uiApp.CreateRibbonTab(tabName);
                    var ribbonPanels = uiApp.GetRibbonPanels(tabName);

                    foreach (var pan in tab.Elements())
                    {
                        RibbonPanel panel;
                        if (ribbonPanels.Any(b => b.Name == pan.Attribute("panelName").Value))
                        {
                            panel = ribbonPanels.Where(b => b.Name == pan.Attribute("panelName").Value).First();
                        }
                        else
                        {
                            panel = uiApp.CreateRibbonPanel(tabName, pan.Attribute("panelName").Value);
                        }

                        //解析tab下元素的slideOut属性
                        String beginSildeNum = (null == pan.Attribute("slideOutNumber")) ? String.Empty
                                                : pan.Attribute("slideOutNumber").Value;

                        foreach (var item in pan.Elements())
                        {
                            if (!String.IsNullOrEmpty(beginSildeNum) &&
                                beginSildeNum == count.ToString())
                                panel.AddSlideOut();//slideOut

                            IRvtRibbonItem obj = null;

                            //利用反射动态创建
                            //获得程序集
                            System.Reflection.Assembly assembly = typeof(IRvtRibbonItem).Assembly;
                            //命名空间
                            String nameSpace = typeof(IRvtRibbonItem).Namespace + "." + item.Name;
                            //实现接口类的构造函数两个参数
                            object[] parameters = new object[2];
                            parameters[0] = item;
                            parameters[1] = relativeRootFolderPath;
                            // 反射创建接口实例
                            obj = (IRvtRibbonItem)assembly.CreateInstance(nameSpace, true,
                                System.Reflection.BindingFlags.Default, null, parameters, null, null);
                            if (null != obj)
                            {
                                //调用接口函数
                                obj.AddRibbonItemToPanel(ref panel, tabName);
                                count++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message + "\n" + ex.StackTrace, "加载异常");
            }
        }

        /// <summary>
        /// 给RibbonTab排序
        /// </summary>
        /// <param name="mode">排序模式：0 放最前面，1 放附加模块后米娜</param>
        void SortRibbonTabs(int mode)
        {
            try
            {
                //sort RibbonTab
                Autodesk.Windows.RibbonControl ribbon = Autodesk.Windows.ComponentManager.Ribbon;
                //存放所有CBIM插件
                List<Autodesk.Windows.RibbonTab> ribbonTabs = new List<Autodesk.Windows.RibbonTab>();

                Autodesk.Windows.RibbonTab tab;
                int iStartPos = 0;
                for (int i = 0; i < ribbon.Tabs.Count; i++)
                {
                    tab = ribbon.Tabs[i];
                    if (tab.AutomationName == null) continue;

                    if (tab.AutomationName.Contains("CBIM"))
                        ribbonTabs.Add(tab);
                    else if (tab.AutomationName == "附加模块")
                        iStartPos = i;//找到附加模块的位置
                }
                ribbonTabs.Reverse();

                foreach (var item in ribbonTabs)
                {
                    ribbon.Tabs.Remove(item);
                    if (mode == 0)
                        ribbon.Tabs.Insert(0, item);//放最前面
                    else
                        ribbon.Tabs.Insert(iStartPos + 1, item);//放附加模块后面
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
