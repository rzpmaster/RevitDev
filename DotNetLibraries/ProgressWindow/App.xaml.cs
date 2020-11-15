using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ProgressWindow
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public void InitializeWindow(String winType)
        {
            if (!string.IsNullOrWhiteSpace(winType))
            {
                this.StartupUri = new System.Uri(winType, System.UriKind.Relative);
            }
            
        }
    }

    public class MainApp
    {
        //private static string defaultView = "ProgressView.xaml";

        [STAThread]
        public static void Main(string[] args)
        {
            if (args != null && args.Length == 1)
            {
                string winType = args[0].Trim();
                InitApp(winType);
            }
            else
            {
                InitApp(null);
            }
        }

        public static void InitApp(String winType)
        {
            var assembly = typeof(ProgressInvoker).Assembly;
            try
            {
                var pros = Process.GetProcessesByName(assembly.GetName().Name);
                foreach (var p in pros)
                {
                    if (p.MainWindowHandle == IntPtr.Zero)
                        continue;

                    p.Kill(); // 关闭已经启动的进度窗口进程
                }

                App app = new App();
                app.InitializeWindow(winType);
                app.Run();
            }
            catch (Exception ex)
            {
                var dir = assembly.Location;
                MessageBox.Show(dir + "\n" + ex.ToString());
            }
        }
    }
}
