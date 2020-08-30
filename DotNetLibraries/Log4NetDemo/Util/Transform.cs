using System.Text.RegularExpressions;
using System.Xml;

namespace Log4NetDemo.Util
{
    public sealed class Transform
    {
        private Transform()
        {
        }

        #region MyRegion

        public static void WriteEscapedXmlString(XmlWriter writer, string textData, string invalidCharReplacement)
        {
            string stringData = MaskXmlInvalidCharacters(textData, invalidCharReplacement);
            // Write either escaped text or CDATA sections

            int weightCData = 12 * (1 + CountSubstrings(stringData, CDATA_END));
            int weightStringEscapes = 3 * (CountSubstrings(stringData, "<") + CountSubstrings(stringData, ">")) + 4 * CountSubstrings(stringData, "&");

            if (weightStringEscapes <= weightCData)
            {
                // Write string using string escapes
                writer.WriteString(stringData);
            }
            else
            {
                // Write string using CDATA section

                int end = stringData.IndexOf(CDATA_END);

                if (end < 0)
                {
                    writer.WriteCData(stringData);
                }
                else
                {
                    int start = 0;
                    while (end > -1)
                    {
                        writer.WriteCData(stringData.Substring(start, end - start));
                        if (end == stringData.Length - 3)
                        {
                            start = stringData.Length;
                            writer.WriteString(CDATA_END);
                            break;
                        }
                        else
                        {
                            writer.WriteString(CDATA_UNESCAPABLE_TOKEN);
                            start = end + 2;
                            end = stringData.IndexOf(CDATA_END, start);
                        }
                    }

                    if (start < stringData.Length)
                    {
                        writer.WriteCData(stringData.Substring(start));
                    }
                }
            }
        }

        public static string MaskXmlInvalidCharacters(string textData, string mask)
        {
            return INVALIDCHARS.Replace(textData, mask);
        }

        #endregion

        #region Private Helper Methods

        private static int CountSubstrings(string text, string substring)
        {
            int count = 0;
            int offset = 0;
            int length = text.Length;
            int substringLength = substring.Length;

            if (length == 0)
            {
                return 0;
            }
            if (substringLength == 0)
            {
                return 0;
            }

            while (offset < length)
            {
                int index = text.IndexOf(substring, offset);

                if (index == -1)
                {
                    break;
                }

                count++;
                offset = index + substringLength;
            }
            return count;
        }

        #endregion

        private const string CDATA_END = "]]>";
        private const string CDATA_UNESCAPABLE_TOKEN = "]]";

        private static Regex INVALIDCHARS = new Regex(@"[^\x09\x0A\x0D\x20-\uD7FF\uE000-\uFFFD]", RegexOptions.Compiled);
    }
}
