using Autodesk.Revit.UI;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DuplicateViews
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class Application : IExternalApplication
    {
        #region IExternalApplication Members

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            CreateCopyPastePanel(application);

            return Result.Succeeded;
        }

        #endregion

        private void CreateCopyPastePanel(UIControlledApplication application)
        {
            RibbonPanel rp = application.CreateRibbonPanel("CopyPaste");

            PushButtonData pbd2 = new PushButtonData("DuplicateAll", "Duplicate across documents",
                   addAssemblyPath,
                   typeof(DuplicateAcrossDocumentsCommand).FullName);

            pbd2.LongDescription = "Duplicate all duplicatable drafting views and schedules.";

            PushButton duplicateAllPB = rp.AddItem(pbd2) as PushButton;
            SetIconsForPushButton(duplicateAllPB, Properties.Resources.ViewCopyAcrossFiles);
        }

        private static void SetIconsForPushButton(PushButton button, System.Drawing.Icon icon)
        {
            button.LargeImage = GetStdIcon(icon);
            button.Image = GetSmallIcon(icon);
        }

        private static BitmapSource GetStdIcon(System.Drawing.Icon icon)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        private static BitmapSource GetSmallIcon(System.Drawing.Icon icon)
        {
            System.Drawing.Icon smallIcon = new System.Drawing.Icon(icon, new System.Drawing.Size(16, 16));
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                smallIcon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        static String addAssemblyPath = typeof(Application).Assembly.Location;
    }
}
