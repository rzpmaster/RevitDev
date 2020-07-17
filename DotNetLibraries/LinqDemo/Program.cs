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
            //linqQuery.ValidateQueryDelayed();
            //linqQuery.WhereDemo();
            //linqQuery.OfTypeDemo();
            //linqQuery.SelectManyDemo();
            //linqQuery.OrderByDemo();
            //linqQuery.GroupByDemo();
            //linqQuery.LetRenameDemo();
            //linqQuery.InnerQueryDemo();
            //linqQuery.InnerJoinDemo();
            //linqQuery.LeftOuterJoinDemo();
            linqQuery.IntoJoinDemo();
            //linqQuery.AggregateDemo();
            //linqQuery.TakeDemo();
            //linqQuery.Polymerize();
            //linqQuery.ToListDemo();
            //linqQuery.RangeDemo();

            //linqQuery.ParallelLINQDemo();
            //linqQuery.CancelParallelQueryDemo();


            Console.ReadLine();
        }
    }
}
