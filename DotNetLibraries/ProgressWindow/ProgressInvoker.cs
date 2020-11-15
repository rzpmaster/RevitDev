using ProgressWindow.Constants;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProgressWindow
{
    public class ProgressInvoker : IDisposable
    {
        private Process _process;
        private NamedPipeClientStream pipe;
        public ProgressInvoker() : this(false)
        {
        }

        public ProgressInvoker(bool isSimpleModel)
        {
            if (isSimpleModel)
            {
                InitialProgressProcess("SimpleProgressView.xaml");
            }
            else
            {
                InitialProgressProcess("ProgressView.xaml");
            }

            ProgressViewLoaded += ProgressInvoker_ProgressViewLoaded;
        }

        protected virtual void ProgressInvoker_ProgressViewLoaded(object sender, EventArgs e)
        {
            SetProgressViewOwner();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // NOTE: Leave out the finalizer altogether if this class doesn't 
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are. 
        ~ProgressInvoker()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (!_process.HasExited)
                {
                    CloseProgressWindow();
                    _process.Close();
                }
            }
        }


        #region Members

        /// <summary>
        /// 父窗口句柄
        /// </summary>
        public Int32 OwnerPtrNum { get; set; }

        private bool _isCancelled;
        /// <summary>
        /// 是否取消
        /// </summary>
        public bool IsCancelled
        {
            get
            {
                return _isCancelled;
            }
        }

        /// <summary>
        /// 进度窗口进程的 Id
        /// </summary>
        public Int32 ProcessId
        {
            get
            {
                if (_process is null)
                    return -1;

                return _process.Id;
            }
        }

        /// <summary>
        /// 当进度条窗口载入时发生
        /// </summary>
        public event EventHandler ProgressViewLoaded;

        #endregion


        #region Public Methods

        /// <summary>
        /// (延迟)启动 进度窗口
        /// </summary>
        /// <returns></returns>
        public Boolean LaunchProgressWindow()
        {
            Boolean isLaunched = false;
            try
            {
                if (_process != null)
                {
                    isLaunched = _process.Start();
                    System.Threading.Thread.Sleep(300);
                    ProgressViewLoaded?.Invoke(this, null);
                }
            }
            catch (Exception)
            {
                _isCancelled = true;
            }

            return isLaunched;
        }

        /// <summary>
        /// (即是)关闭 进度窗口
        /// </summary>
        public void CloseProgressWindow()
        {
            SendMessageAsync(ConstData.Progress_CloseTag);
        }

        /// <summary>
        /// 设置 进度窗体 标题
        /// </summary>
        /// <param name="win_Title"></param>
        public void SetupTitle(string win_Title)
        {
            SendMessageAsync(ConstData.Process_HeaderTag + win_Title);
        }

        /// <summary>
        /// 设置 进度窗体 位置
        /// </summary>
        /// <param name="pos"></param>
        public void SetupProgressWindowPosition(System.Windows.Point pos)
        {
            SendMessageAsync(string.Format(ConstData.Position_HeaderTag + "{0}_{1}", pos.X, pos.Y));
        }

        #region Operate Complete ProgressView

        /// <summary>
        /// 更新主提示
        /// </summary>
        /// <param name="mainTipMesg"></param>
        public void UpdateMainTipInfo(string mainTipMesg)
        {
            SendMessageAsync(ConstData.MainTip_Info + mainTipMesg);
        }

        /// <summary>
        /// 设置主进度 循环总次数
        /// </summary>
        /// <param name="count"></param>
        public void SetMainProgressCount(int count)
        {
            SendMessageAsync(ConstData.MainIteration_Count + count.ToString());
        }

        /// <summary>
        /// 更新 主进度条 比例
        /// </summary>
        /// <param name="mainRate"></param>
        public void UpdateMainProgress(double mainRate)
        {
            SendMessageAsync(ConstData.UpdateMainProgress + mainRate.ToString());
        }

        public void UpdateSubTipInfo(string subTipMesg)
        {
            SendMessageAsync(ConstData.SubTip_Info + subTipMesg);
        }

        /// <summary>
        /// 设置子进度 循环总次数
        /// </summary>
        /// <param name="count"></param>
        public void SetSubProgressCount(int count)
        {
            SendMessageAsync(ConstData.SubIteration_Count + count.ToString());
        }

        /// <summary>
        /// 更新 子进度条 比例
        /// </summary>
        /// <param name="subRate"></param>
        public void UpdateSubProgress(double subRate)
        {
            SendMessageAsync(ConstData.UpdateSubProgress + string.Format("{0:N2}", subRate));
        }

        /// <summary>
        /// 更新详细信息 (追加)
        /// </summary>
        /// <param name="detailInfo"></param>
        public void UpdateDetailInfo(string detailInfo)
        {
            SendMessageAsync(ConstData.Detail_Info + detailInfo);
        }

        #endregion

        /// <summary>
        /// 开始一个子任务
        /// </summary>
        /// <param name="subTipMesg"></param>
        public void StartOneIteration(string subTipMesg)
        {
            SendMessageAsync(ConstData.SubTip_Info + subTipMesg);
        }

        /// <summary>
        /// 结束一个子任务
        /// </summary>
        /// <returns></returns>
        public bool EndOneIteration()
        {
            SendMessageAsync(ConstData.UpdateProgress_Info);

            return IsCancelled;
        }

        #endregion


        #region private Methods

        /// <summary>
        /// 启动进度条窗口进程
        /// </summary>
        /// <param name="progressType">进度条窗口的类型(xaml文件名)</param>
        /// <param name="revitPtrNum">设置进度条窗口的父窗口句柄</param>
        private void InitialProgressProcess(String progressType)
        {
            _isCancelled = false;

            // launch the progress bar window.
            var assembly = typeof(ProgressInvoker).Assembly;
            FileInfo di = new FileInfo(assembly.Location);

            if (!di.Exists)
            {
                _isCancelled = true;
                return;
            }

            // 关闭所有已经打开的进度窗口，避免影响（开启多个Revit同时执行可能有问题）
            var winName = assembly.GetName().Name;
            var existWins = Process.GetProcessesByName(winName);
            foreach (var pw in existWins)
            {
                pw.Kill();
            }

            _process = new Process();
            _process.StartInfo.FileName = di.FullName;
            _process.StartInfo.Arguments = progressType;
            //_process.Start();//延迟启动
            //System.Threading.Thread.Sleep(300);
        }

        /// <summary>
        /// 设置进度条窗口的父窗体
        /// </summary>
        private void SetProgressViewOwner()
        {
            if (OwnerPtrNum != 0)
            {
                IntPtr revitPtr = new IntPtr(OwnerPtrNum);
                var phandle = _process.MainWindowHandle;
                int c = 0;
                while (phandle == IntPtr.Zero && c < 10000)
                {
                    phandle = _process.MainWindowHandle;
                    c++;
                }

                if (phandle != IntPtr.Zero)
                {
                    SetWindowLongPtr64(_process.MainWindowHandle, -8, revitPtr);
                    // MessageBox.Show("set to revit child widow " + c.ToString());
                }
            }
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        /// <summary>
        /// 像窗口发送消息
        /// </summary>
        /// <param name="message"></param>
        private async void SendMessageAsync(string message)
        {
            if (_isCancelled)
                return;

            try
            {
                using (pipe = new NamedPipeClientStream(".", ConstData.pipe_name, PipeDirection.InOut, PipeOptions.Asynchronous))
                {
                    if (pipe.IsConnected != true) { pipe.Connect(2000); }
                    using (var stream = new StreamWriter(pipe))
                    {
                        // write the message to the pipe stream 
                        await stream.WriteAsync(message);
                    }
                }
            }
            catch (Exception)
            {
                _isCancelled = true;
            }
        }

        #endregion
    }
}
