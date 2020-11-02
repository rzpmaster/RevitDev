using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketDemo.IISServer
{
    public class HttpResponse
    {
        private byte[] buffer;

        public HttpResponse(byte[] buffer)
        {
            this.buffer = buffer;
        }

        public HttpResponse(byte[] buffer, string filePath) : this(buffer)
        {
            string fileEx = Path.GetExtension(filePath);
            switch (fileEx)
            {
                case ".html":
                case ".htm":
                    ContentType = "text/html";
                    break;
                default:
                    break;
            }
        }

        public byte[] GetHeader()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("HTTP/1.1 200 ok\r\n");
            sb.AppendFormat("Content-Type:{0};charset=utf-8\r\n", ContentType);
            sb.AppendFormat("Content-Length:{0}\r\n", buffer.Length);
            sb.Append("\r\n");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public string ContentType { get; set; }
    }
}
