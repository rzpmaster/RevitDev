using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketDemo.IISServer
{
    public class HttpApplication
    {
        private Socket socket;
        private Action<string> UpdateInfo;

        public HttpApplication(Socket socket, Action<string> updateInfo)
        {
            this.socket = socket;
            this.UpdateInfo = updateInfo;

            byte[] buffer = new byte[1024 * 1024 * 2];
            int length = socket.Receive(buffer);

            if (length > 0)
            {
                // 请求报文
                string msg = Encoding.UTF8.GetString(buffer, 0, length);
                //UpdateInfo.Invoke(msg);

                // 分析请求报文
                HttpRequest request = new HttpRequest(msg);
                //UpdateInfo.Invoke(request.FilePath);

                // 查找文件，构建响应报文
                ProcessRequest(request);
            }
        }

        private void ProcessRequest(HttpRequest request)
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            if (dir.EndsWith(@"\bin\Debug\") || dir.EndsWith(@"\bin\Release\"))
            {
                dir = Directory.GetParent(dir).Parent.Parent.FullName.ToString();
            }
            string fullName = Path.Combine(dir + request.FilePath);

            using (FileStream fs = new FileStream(fullName, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);

                // 构建响应报文
                HttpResponse response = new HttpResponse(buffer, request.FilePath);

                // 发送数据
                socket.Send(response.GetHeader());
                socket.Send(buffer);
            }
        }
    }
}
