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
    /// ProgressView.xaml 的交互逻辑
    /// </summary>
    public partial class ProgressView : Window
    {
        private NamedPipeServerStream pipe;
        private ProgressModel mainModel;

        public ProgressView()
        {
            InitializeComponent();

            mainModel = new ProgressModel();
            mainModel.InitialRate();
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


                if (msg.StartsWith(ConstData.MainIteration_Count))
                { // 设置 主进度 循环总次数
                    msg = msg.Substring(ConstData.MainIteration_Count.Length);
                    mainModel.MainProgress.TotalNum = Double.Parse(msg);
                }
                else if (msg.StartsWith(ConstData.SubIteration_Count))
                { // 更新 子进度 循环总次数
                    msg = msg.Substring(ConstData.SubIteration_Count.Length);
                    mainModel.SubProgress.TotalNum = Double.Parse(msg);
                    mainModel.SubProgress.CurrentNum = 0; // 初始化 子进度 当前进度值为 0
                }
                else if (msg.StartsWith(ConstData.MainTip_Info))
                { // 更新主进度提示信息
                    msg = msg.Substring(ConstData.MainTip_Info.Length);
                    mainModel.MainProgress.MainTip = msg;
                }
                else if (msg.StartsWith(ConstData.SubTip_Info))
                { // 更新子进度提示信息
                    msg = msg.Substring(ConstData.SubTip_Info.Length);
                    mainModel.SubProgress.MainTip = msg;
                }
                else if (msg.StartsWith(ConstData.Detail_Info))
                { // 更新 详细信息
                    msg = msg.Substring(ConstData.Detail_Info.Length);
                    if (!String.IsNullOrWhiteSpace(msg))
                        mainModel.DetailMesg.Add(msg);
                    // mainModel.DetailText = msg;
                    this.DetailMesgListBox.ScrollIntoView(DetailMesgListBox.Items[DetailMesgListBox.Items.Count - 1]);
                }
                else if (msg.StartsWith(ConstData.UpdateProgress_Info))
                { // 更新进度条状态
                    mainModel.MainProgress.CurrentNum++;
                    mainModel.SubProgress.CurrentNum++;
                    if (mainModel.MainProgress.TotalNum == mainModel.MainProgress.CurrentNum)
                    {
                        mainModel.StopTimer();
                        mainModel.MainProgress.RemainTime = new TimeSpan(0, 0, 0);
                        mainModel.SubProgress.RemainTime = new TimeSpan(0, 0, 0);
                        this.Close();
                    }
                }
                else if (msg.StartsWith(ConstData.UpdateMainProgress))
                { // 更新 主进度条 比例
                    msg = msg.Substring(ConstData.UpdateMainProgress.Length);
                    mainModel.MainProgress.Rate = Math.Round(Double.Parse(msg), 2);

                    if (mainModel.MainProgress.Rate == 1.0)
                    {
                        mainModel.StopTimer();
                        mainModel.MainProgress.RemainTime = new TimeSpan(0, 0, 0);
                        mainModel.SubProgress.RemainTime = new TimeSpan(0, 0, 0);
                        this.OkButton.IsEnabled = true;
                        this.CancelButton.IsEnabled = false;
                    }
                }
                else if (msg.StartsWith(ConstData.UpdateSubProgress))
                { // 更新 子进度条 比例
                    msg = msg.Substring(ConstData.UpdateSubProgress.Length);
                    mainModel.SubProgress.Rate = Math.Round(Double.Parse(msg), 2);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }

        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var relt = MessageBox.Show("确定要取消操作？", "确认", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (relt == MessageBoxResult.OK)
            {
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (pipe.IsConnected)
            {
                pipe.Disconnect();// must disconnect 
            }

            base.OnClosed(e);
        }
    }
}
