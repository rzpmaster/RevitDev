using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SocketDemo.Server
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        bool hasStart;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (hasStart)
            {
                hasStart = false;
                this.btn.Content = "开启服务";
                this.info.Text += "服务已关闭\r\n";
                return;
            }
            else
            {
                hasStart = true;
                this.btn.Content = "关闭服务";
                this.info.Text += "服务已开启\r\n";
            }


            // AddressFamily.InterNetwork ipv4
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ip = IPAddress.Parse(this.ip.Text);
            IPEndPoint port = new IPEndPoint(ip, Convert.ToInt32(this.port.Text));

            // 监听地址
            listenSocket.Bind(port);
            // 监听最大长度
            listenSocket.Listen(10);

            Task.Factory.StartNew(
                    () =>
                    {
                        while (hasStart)
                        {
                            // 用于通信的 socket
                            Socket newSocket = listenSocket.Accept();

                            UpdateInfo($"{newSocket.RemoteEndPoint}已连接！\r\n");
                            HttpApplication application = new HttpApplication(newSocket, UpdateInfo);
                        }
                    }
                    );
        }

        private void UpdateInfo(string message)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.info.Text += message;
            });
        }
    }
}
