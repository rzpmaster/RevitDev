using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ProgressWindow
{
    class ProgressModel : ObservableObject
    {
        DispatcherTimer timer;  // 设置定时器，每一秒更新一下剩余时间
        public ProgressModel()
        {
            mainProgress = new ProgressInfo();
            subProgress = new ProgressInfo();
            detailMesg = new ObservableCollection<string>();
            detailText = String.Empty;

            initialTimer();
        }

        #region Timer

        private void initialTimer()
        {
            //设置定时器
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(1000000);   //时间间隔为一秒
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        { // 更新 已用时间
            mainProgress.CostTime = DateTime.Now - mainProgress.StartTime;
            subProgress.CostTime = DateTime.Now - subProgress.StartTime;

            if (subProgress.Rate > 0)
            {
                subProgress.UpdateRemainTimeByRate();

                if (Math.Abs(subProgress.Rate - 1) <= 0.01)
                {
                    mainProgress.UpdateRemainTimeByRate();
                }
            }
            else
            {
                subProgress.UpdateRemainTime();
                mainProgress.UpdateRemainTime();
            }
        }

        #endregion


        public void InitialRate()
        {
            MainProgress.TotalNum = 1;
            MainProgress.Rate = 0;

            SubProgress.TotalNum = 1;
            SubProgress.Rate = 0;
        }

        public void StopTimer()
        {
            if (timer != null)
                timer.Stop();
        }


        private ProgressInfo mainProgress;

        public ProgressInfo MainProgress
        {
            get { return mainProgress; }
            set { mainProgress = value; RaisePropertyChanged(() => MainProgress); }
        }


        private ProgressInfo subProgress;

        public ProgressInfo SubProgress
        {
            get { return subProgress; }
            set { subProgress = value; RaisePropertyChanged(() => SubProgress); }
        }


        private String cancelQuit;

        public String CancelQuit
        {
            get { return cancelQuit; }
            set { cancelQuit = value; RaisePropertyChanged(() => CancelQuit); }
        }


        private ObservableCollection<String> detailMesg;
        /// <summary>
        /// 所有的详细信息记录，多次调用不会覆盖删除
        /// </summary>
        public ObservableCollection<String> DetailMesg
        {
            get { return detailMesg; }
            set { detailMesg = value; }
        }


        /// <summary>
        /// 所有的详细信息记录，多次调用会覆盖删除，只保留最新的
        /// </summary>
        // public String DetailText { get; set; }
        private String detailText;

        public String DetailText
        {
            get { return detailText; }
            set { detailText = value; RaisePropertyChanged(() => DetailText); }
        }

    }

    class ProgressInfo : ObservableObject
    {
        Double totalNum;
        Double currentNum;
        Double rate;

        String mainTip;

        DateTime startTime;
        TimeSpan remainTime;
        TimeSpan costTime;

        public ProgressInfo()
        {
            StartTime = DateTime.Now;
            // remainTime = new TimeSpan(23, 59, 59);
            currentNum = 0;
            totalNum = 100;

            rate = -1;
        }

        public void UpdateRemainTime()
        {
            if (totalNum == currentNum || currentNum == 0)
            {
                RemainTime = new TimeSpan(0, 0, 0);
                return;
            }

            Double remain = (costTime.TotalSeconds / currentNum) * (totalNum - currentNum);
            Int32 h = (Int32)(remain / 3600);
            Int32 m = (Int32)((remain % 3600) / 60);
            Int32 s = (Int32)remain % 60;
            RemainTime = new TimeSpan(h, m, s);
        }

        public void UpdateRemainTimeByRate()
        {
            if (rate <= 0)
                return;

            Double remain = costTime.TotalSeconds * ((1.0 - rate) / rate);
            Int32 h = (Int32)(remain / 3600);
            Int32 m = (Int32)((remain % 3600) / 60);
            Int32 s = (Int32)remain % 60;

            RemainTime = new TimeSpan(h, m, s);
        }

        public Double TotalNum
        {
            get
            {
                return totalNum;
            }

            set
            {
                totalNum = value;
                RaisePropertyChanged(() => TotalNum);
            }
        }

        public Double CurrentNum
        {
            get
            {
                return currentNum;
            }

            set
            {
                currentNum = value;
                if (currentNum == 0)
                    StartTime = DateTime.Now;
                RaisePropertyChanged(() => CurrentNum);
            }
        }

        public Double Rate
        {
            get
            {
                return rate;
            }

            set
            {
                rate = value;
                RaisePropertyChanged(() => Rate);
            }
        }

        public String MainTip
        {
            get
            {
                return mainTip;
            }

            set
            {
                mainTip = value;
                RaisePropertyChanged(() => MainTip);
            }
        }

        public TimeSpan CostTime
        {
            get
            {
                return costTime;
            }

            set
            {
                costTime = value;
                RaisePropertyChanged(() => CostTime);
            }
        }

        public DateTime StartTime
        {
            get
            {
                return startTime;
            }

            set
            {
                startTime = value;
            }
        }

        public TimeSpan RemainTime
        {
            get
            {
                return remainTime;
            }

            set
            {
                remainTime = value;
                RaisePropertyChanged(() => RemainTime);
                RaisePropertyChanged(() => RemainTimeStr);
            }
        }

        public String RemainTimeStr
        {
            get
            {
                if (rate == 0)
                {
                    return "剩余时间: 正在计算...";
                }
                return "剩余时间: " + remainTime.ToString(@"hh\:mm\:ss");
            }
        }

    }
}
