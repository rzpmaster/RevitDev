using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PostCommandWorkflow
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class Application : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            CreateUIPanel(application);
            return Result.Succeeded;
        }

        private void CreateUIPanel(UIControlledApplication application)
        {
            RibbonPanel rp = application.CreateRibbonPanel("UI");
            PushButtonData setupMonitor = new PushButtonData("Setup_Revision_Monitor", "点击设置修订号监视器",
                                                            typeof(Application).Assembly.Location,
                                                            typeof(PostCommandRevisionMonitorCommand).FullName);
            PushButton setupMonitorPB = rp.AddItem(setupMonitor) as PushButton;

            var baseFolder = Path.GetDirectoryName(typeof(Application).Assembly.Location);
            var icon = new System.Drawing.Icon(baseFolder + @"\Resources\RevisionIcon.ico");
            setupMonitorPB.LargeImage = GetStdIcon(icon);
            setupMonitorPB.Image = GetSmallIcon(icon);

            PostCommandRevisionMonitorCommand.SetPushButton(setupMonitorPB);
        }

        private ImageSource GetSmallIcon(System.Drawing.Icon icon)
        {
            System.Drawing.Icon smallIcon = new System.Drawing.Icon(icon, new System.Drawing.Size(16, 16));
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                smallIcon.Handle,
                System.Windows.Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }

        private ImageSource GetStdIcon(System.Drawing.Icon icon)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                System.Windows.Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
