using System;
using System.IO;

namespace Log4NetDemo.Layout.PatternStringConverters
{
    /// <summary>
    /// 字面意思
    /// </summary>
    internal class LiteralPatternConverter : PatternConverter
    {
        public override PatternConverter SetNext(PatternConverter pc)
        {
            LiteralPatternConverter literalPc = pc as LiteralPatternConverter;
            if (literalPc != null)
            {
                // Combine the two adjacent literals together
                Option = Option + literalPc.Option;

                // We are the next converter now
                return this;
            }

            return base.SetNext(pc);
        }

        override public void Format(TextWriter writer, object state)
        {
            writer.Write(Option);
        }

        override protected void Convert(TextWriter writer, object state)
        {
            throw new InvalidOperationException("Should never get here because of the overridden Format method");
        }
    }
}
