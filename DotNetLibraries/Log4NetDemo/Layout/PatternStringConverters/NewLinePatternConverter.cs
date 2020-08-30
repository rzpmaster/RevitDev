using Log4NetDemo.Core.Interface;
using Log4NetDemo.Util;

namespace Log4NetDemo.Layout.PatternStringConverters
{
    /// <summary>
    /// 换行
    /// </summary>
    internal sealed class NewLinePatternConverter : LiteralPatternConverter, IOptionHandler
    {
        public void ActivateOptions()
        {
            if (SystemInfo.EqualsIgnoringCase(Option, "DOS"))
            {
                Option = "\r\n";
            }
            else if (SystemInfo.EqualsIgnoringCase(Option, "UNIX"))
            {
                Option = "\n";
            }
            else
            {
                Option = SystemInfo.NewLine;
            }
        }
    }
}
