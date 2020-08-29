using System;
using System.Net;

namespace Log4NetDemo.Util.Converters.TypeConverters
{
    internal class IPAddressConverter : IConvertFrom
    {
        #region Implementation of IConvertFrom

        /// <summary>
        /// Can the source type be converted to the type supported by this object
        /// </summary>
        /// <param name="sourceType">the type to convert</param>
        /// <returns>true if the conversion is possible</returns>
        /// <remarks>
        /// <para>
        /// Returns <c>true</c> if the <paramref name="sourceType"/> is
        /// the <see cref="String"/> type.
        /// </para>
        /// </remarks>
        public bool CanConvertFrom(Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        /// <summary>
        /// Overrides the ConvertFrom method of IConvertFrom.
        /// </summary>
        /// <param name="source">the object to convert to an IPAddress</param>
        /// <returns>the IPAddress</returns>
        /// <remarks>
        /// <para>
        /// Uses the <see cref="IPAddress.Parse"/> method to convert the
        /// <see cref="String"/> argument to an <see cref="IPAddress"/>.
        /// If that fails then the string is resolved as a DNS hostname.
        /// </para>
        /// </remarks>
        /// <exception cref="ConversionNotSupportedException">
        /// The <paramref name="source"/> object cannot be converted to the
        /// target type. To check for this condition use the <see cref="CanConvertFrom"/>
        /// method.
        /// </exception>
        public object ConvertFrom(object source)
        {
            string str = source as string;
            if (str != null && str.Length > 0)
            {
                try
                {
                    // Try an explicit parse of string representation of an IPAddress (v4 or v6)
                    IPAddress result;
                    if (IPAddress.TryParse(str, out result))
                    {
                        return result;
                    }

                    // Try to resolve via DNS. This is a blocking call. 
                    // GetHostEntry works with either an IPAddress string or a host name
                    IPHostEntry host = Dns.GetHostEntry(str);
                    if (host != null &&
                        host.AddressList != null &&
                        host.AddressList.Length > 0 &&
                        host.AddressList[0] != null)
                    {
                        return host.AddressList[0];
                    }
                }
                catch (Exception ex)
                {
                    throw ConversionNotSupportedException.Create(typeof(IPAddress), source, ex);
                }
            }
            throw ConversionNotSupportedException.Create(typeof(IPAddress), source);
        }

        #endregion

        /// <summary>
        /// Valid characters in an IPv4 or IPv6 address string. (Does not support subnets)
        /// </summary>
        private static readonly char[] validIpAddressChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'A', 'B', 'C', 'D', 'E', 'F', 'x', 'X', '.', ':', '%' };
    }
}
