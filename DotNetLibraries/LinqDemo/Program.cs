using StreamDemo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // 客串管道通讯 客户机
            //PipeClient.NamedPipesWriter();

            LinqQuery linqQuery = new LinqQuery();

            //linqQuery.QueryChinaChampions();
            linqQuery.ValidateQueryDelayed();


            Console.ReadLine();
        }
    }
}
