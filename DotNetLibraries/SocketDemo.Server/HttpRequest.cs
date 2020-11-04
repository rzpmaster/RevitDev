using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketDemo.IISServer
{
    public class HttpRequest
    {
        public HttpRequest(string request)
        {
            FilePath = request.Split(
                new char[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault()
                .Split(' ')
                .ElementAt(1);
        }

        public string FilePath { get; private set; }
    }
}
