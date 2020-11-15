using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressWindow
{
    class SimpleProgressModel : ObservableObject
    {

        private String mainText;

        public String MainText
        {
            get { return mainText; }
            set { mainText = value; RaisePropertyChanged(() => MainText); }
        }

    }
}
