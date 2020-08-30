using System;
using System.IO;

namespace Log4NetDemo.Layout.Data.DataFormatter
{
    public interface IDateFormatter
    {
        void FormatDate(DateTime dateToFormat, TextWriter writer);
    }
}
