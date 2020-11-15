using ProgressWindow.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProgressWindow
{
    /// <summary>
    /// SimpleProgressView.xaml 的交互逻辑
    /// </summary>
    public partial class SimpleProgressView : Window
    {
        private NamedPipeServerStream pipe;
        private SimpleProgressModel mainModel;
        public SimpleProgressView()
        {
            InitializeComponent();

            mainModel = new SimpleProgressModel();
            this.DataContext = mainModel;

            StartListeningAsync(new Action<string>(this.MessageRecieved));
        }

        private async void StartListeningAsync(Action<string> messageRecieved)
        {
            try
            {
                while (true)
                {
                    using (pipe = new NamedPipeServerStream(ConstData.pipe_name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                    {
                        await Task.Factory.FromAsync(pipe.BeginWaitForConnection, pipe.EndWaitForConnection, null);

                        using (StreamReader reader = new StreamReader(pipe))
                        {
                            // read the message from the stream - async
                            var message = await reader.ReadToEndAsync();
                            // invoke the message received action 
                            messageRecieved?.Invoke(message);
                        }

                    }
                }
            }
            catch { }
        }

        private void MessageRecieved(String msg)
        {
            try
            {
                if (msg.StartsWith(ConstData.Progress_CloseTag))
                { // 关闭进度窗口
                    this.Close();
                }

                if (msg.StartsWith(ConstData.Process_HeaderTag))
                { // 设置进度窗口标题
                    msg = msg.Substring(ConstData.Process_HeaderTag.Length + 1);
                    this.Title = String.Format("{0} - {1}", ConstData.Titlt_main, msg);
                }

                if (msg.StartsWith(ConstData.Position_HeaderTag))
                { // 设置进度窗口位置
                    msg = msg.Substring(ConstData.Position_HeaderTag.Length);
                    var strs = msg.Split('_');
                    if (strs.Length != 2)
                        return;

                    double px = double.Parse(strs[0]);
                    double py = double.Parse(strs[1]);

                    this.Left = px;
                    this.Top = py;
                }

                if (msg.StartsWith(ConstData.MainTip_Info))
                { // 更新主进度提示信息
                    msg = msg.Substring(ConstData.MainTip_Info.Length);
                    mainModel.MainText = msg;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }

        }
    }
}
