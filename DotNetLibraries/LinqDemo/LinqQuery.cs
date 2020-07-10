using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LinqDemo
{
    class LinqQuery
    {
        /// <summary>
        /// 热身
        /// </summary>
        public void QueryChinaChampions()
        {
            var query = from r in Formulal.GetChampions()
                        where r.Country == "China"
                        orderby r.Wins descending
                        select r;

            //注意，执行foreach时，查询语句才会执行

            foreach (Racer r in query)
            {
                Console.WriteLine($"{r:A}");
            }

            Console.WriteLine("----------------------------------");

            var champions = new List<Racer>(Formulal.GetChampions());
            IEnumerable<Racer> brazilChampions = champions.Where(r => r.Country == "China")
                 .OrderByDescending(r => r.Wins)
            .Select(r => r);

            foreach (Racer r in brazilChampions)
            {
                Console.WriteLine($"{r:A}");
            }
        }

        /// <summary>
        /// 验证延时查询
        /// </summary>
        public void ValidateQueryDelayed()
        {
            //解决延迟加载的办法就是使用ToArray()、ToList()等方法
            var names = new List<string> { "Nino", "Alberto", "Juan", "Mike", "Phil" };
            var namesWithJToList = (from n in names
                                    where n.StartsWith("J")
                                    orderby n
                                    select n).ToList();

            Console.WriteLine("调用ToList():");
            foreach (string name in namesWithJToList)
            {
                Console.WriteLine(name);
            }
            names.Add("John");
            names.Add("Jim");
            names.Add("Jack");
            names.Add("Denny");
            Console.WriteLine("添加了新的元素后：");
            foreach (string name in namesWithJToList)
            {
                Console.WriteLine(name);
            }
        }

        /// <summary>
        /// 筛选
        /// </summary>
        public void WhereDemo()
        {
            var racers = from r in Formulal.GetChampions()
                         where r.Wins > 15 && (r.Country == "China" || r.Country == "UK")
                         select r;
            foreach (var r in racers)
            {
                Console.WriteLine(r);
            }

            var racers2 = Formulal.GetChampions()
                .Where(r => r.Wins > 15 && (r.Country == "China" || r.Country == "UK"))
                .Select(r => r);
            foreach (var r in racers2)
            {
                Console.WriteLine(r);
            }

            var racers3 = Formulal.GetChampions()
                .Where((r, index) => r.LastName.StartsWith("A") && index % 2 != 0);
            foreach (var r in racers3)
            {
                Console.WriteLine(r);
            }
        }

        /// <summary>
        /// 类型筛选
        /// </summary>
        public void OfTypeDemo()
        {
            object[] data = { "one", 2, 3, "four", "five", 6 };
            var query = data.OfType<string>();
            foreach (var s in query)
            {
                Console.WriteLine(s);
            }
        }

        /// <summary>
        /// 多表查询
        /// </summary>
        public void SelectManyDemo()
        {
            //Formulal.GetChampions()返回Racer集合，每一个Racer的属性Cars是一个字符串数组
            var ferrariDrivers = from r in Formulal.GetChampions()
                                 from c in r.Cars
                                 where c == "Ferrari"
                                 orderby r.LastName
                                 select r.FirstName + " " + r.LastName;

            var ferrariDrivers2 = Formulal.GetChampions()
                .SelectMany(r => r.Cars, (r, c) => new { Racer = r, Car = c })
                .Where(r => r.Car == "Ferrari")
                .OrderBy(r => r.Racer.LastName)
                .Select(r => r.Racer.FirstName + " " + r.Racer.LastName);
        }

        /// <summary>
        /// 排序
        /// </summary>
        public void OrderByDemo()
        {
            var racers = (from r in Formulal.GetChampions()
                          orderby r.Country, r.LastName, r.FirstName ascending
                          select r).Take(10);

            var racers2 = Formulal.GetChampions()
                .OrderBy(r => r.Country)
                .ThenBy(r => r.LastName)
                .ThenByDescending(r => r.FirstName)
                .Take(10);
        }

        /// <summary>
        /// 分组
        /// </summary>
        public void GroupByDemo()
        {
            var countries = from r in Formulal.GetChampions()
                            group r by r.Country into g //将分组信息放入标识符g中
                            orderby g.Count() descending, g.Key
                            where g.Count() >= 2
                            select new
                            {
                                Country = g.Key,
                                Count = g.Count()
                            };

            var countries2 = Formulal.GetChampions()
                .GroupBy(r => r.Country)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .Where(g => g.Count() >= 2)
                .Select(g => new { Country = g.Key, Count = g.Count() });
        }

        /// <summary>
        /// 重命名
        /// </summary>
        public void LetRenameDemo()
        {
            var countries = from r in Formulal.GetChampions()
                            group r by r.Country into g
                            let count = g.Count()
                            orderby count descending, g.Key
                            where count >= 2
                            select new
                            {
                                Country = g.Key,
                                Count = count
                            };

            var countries2 = Formulal.GetChampions()
                .GroupBy(r => r.Country)
                .Select(g => new { Group = g, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ThenBy(g => g.Group.Key)
                .Where(g => g.Count >= 2)
                .Select(g => new
                {
                    Country = g.Group.Key,
                    Count = g.Count
                });
        }

        /// <summary>
        /// 嵌套查询
        /// </summary>
        public void InnerQueryDemo()
        {
            var countries = from r in Formulal.GetChampions()
                            group r by r.Country into g
                            let count = g.Count()
                            orderby count descending, g.Key
                            where count >= 2
                            select new
                            {
                                Country = g.Key,
                                Count = count,
                                //使用内部子句嵌套
                                Racers = from r1 in g
                                         orderby r1.LastName
                                         select r1.FirstName + " " + r1.LastName
                            };
            foreach (var item in countries)
            {
                Console.WriteLine($"{item.Country,-10} {item.Count}");
                foreach (var name in item.Racers)
                {
                    Console.Write(name + ";");
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// 内联接
        /// 精确返回同时匹配到左右两张表的数据
        /// </summary>
        public void InnerJoinDemo()
        {
            var racers = from r in Formulal.GetChampions()
                         from y in r.Years
                         select new
                         {
                             Year = y,
                             Name = r.FirstName + " " + r.LastName
                         };
            var teams = from t in Formulal.GetContructorChampions()
                        from y in t.Years
                        select new
                        {
                            Year = y,
                            Name = t.Name
                        };

            var racersAndTeams = (from r in racers
                                  join t in teams on r.Year equals t.Year
                                  orderby t.Year
                                  select new
                                  {
                                      r.Year,
                                      Champion = r.Name,
                                      Constructor = t.Name
                                  }).Take(10);


            var racersAndTeams2 =
                (from r in from r1 in Formulal.GetChampions()
                           from yr in r1.Years
                           select new
                           {
                               Year = yr,
                               Name = r1.FirstName + " " + r1.LastName
                           }
                 join t in
                 from t1 in Formulal.GetContructorChampions()
                 from yt in t1.Years
                 select new
                 {
                     Year = yt,
                     Name = t1.Name
                 }
                 on r.Year equals t.Year
                 orderby t.Year
                 select new
                 {
                     Year = r.Year,
                     Champion = r.Name,
                     Constructor = t.Name
                 }
                 ).Take(10);

            Console.WriteLine("输出结果：");
            foreach (var item in racersAndTeams2)
            {
                Console.WriteLine($"{item.Year}:{item.Champion,-20} {item.Constructor}");
            }
        }

        /// <summary>
        /// 左外联接
        /// 返回第一个表的所有数据，和匹配到第二张表上的数据，没有匹配上，填充null
        /// </summary>
        public void LeftOuterJoinDemo()
        {
            var racers = from r in Formulal.GetChampions()
                         from y in r.Years
                         select new
                         {
                             Year = y,
                             Name = r.FirstName + " " + r.LastName
                         };
            var teams = from t in Formulal.GetContructorChampions()
                        from y in t.Years
                        select new
                        {
                            Year = y,
                            Name = t.Name
                        };

            var racersAndTeams = (from r in racers
                                  join t in teams on r.Year equals t.Year into rt
                                  from t in rt.DefaultIfEmpty()
                                  orderby r.Year
                                  select new
                                  {
                                      r.Year,
                                      Champion = r.Name,
                                      Constructor = t == null ? "no constructor" : t.Name
                                  }).Take(10);

            Console.WriteLine("输出结果：");
            foreach (var item in racersAndTeams)
            {
                Console.WriteLine($"{item.Year}:{item.Champion,-10} {item.Constructor}");
            }
        }

        /// <summary>
        /// 分组联接
        /// 将第一个表中的元素与第二个表中的一个或多个匹配元素相关联，如果没有匹配上，填充null
        /// </summary>
        public void IntoJoinDemo()
        {
            var racers = Formulal.GetChampionships()
                .SelectMany(cs => new List<RacerInfo>()
                {
                    new RacerInfo
                    {
                        Year=cs.Year,
                        Positon=1,
                        FirstName=cs.First.Split(' ')[0],
                        LastName=cs.First.Split(' ')[1]
                    },
                    new RacerInfo
                    {
                        Year=cs.Year,
                        Positon=2,
                        FirstName=cs.Second.Split(' ')[0],
                        LastName=cs.Second.Split(' ')[1]
                    },
                    new RacerInfo
                    {
                        Year=cs.Year,
                        Positon=3,
                        FirstName=cs.Third.Split(' ')[0],
                        LastName=cs.Third.Split(' ')[1]
                    }
                });

            foreach (var r in racers)
            {
                Console.WriteLine(r.FirstName + " " + r.LastName);
            }

            var q = (from r in Formulal.GetChampions()
                     join r2 in racers on
                     new
                     {
                         FirstName = r.FirstName,
                         LastName = r.LastName
                     }
                     equals
                     new
                     {
                         r2.FirstName,
                         r2.LastName
                     }
                     into yearResults
                     select new
                     {
                         r.FirstName,
                         r.LastName,
                         r.Wins,
                         r.Starts,
                         Results = yearResults
                     });

            foreach (var r in q)
            {
                Console.WriteLine(r.FirstName + " " + r.LastName);
                foreach (var results in r.Results)
                {
                    Console.WriteLine(results.Year + " " + results.Positon);
                }
            }
        }

        /// <summary>
        /// 集合操作
        /// Intersect()：通过使用的默认相等比较器对值进行比较，生成两个序列的交集。
        /// Distinct()：返回序列中通过使用指定的非重复元素。
        /// Union()：通过使用默认的相等比较器生成的两个序列的并集。
        /// Except()：通过使用默认的相等比较器对值进行比较，生成两个序列的差集。
        /// </summary>
        public void AggregateDemo()
        {
            Func<string, IEnumerable<Racer>> racersByCar =
                car => from r in Formulal.GetChampions()
                       from c in r.Cars
                       where c == car
                       orderby r.LastName
                       select r;

            Console.WriteLine("调用Intersect()方法，显示结果：");
            foreach (var racer in racersByCar("Ferrari").Intersect(racersByCar("Lotus")))
            {
                Console.WriteLine(racer);
            }



            var racernames = from r in Formulal.GetChampions()
                             where r.Country == "Italy"
                             orderby r.Wins descending
                             select new
                             {
                                 Name = r.FirstName + " " + r.LastName
                             };
            var racerNamesAndStarts = from r in Formulal.GetChampions()
                                      where r.Country == "Italy"
                                      orderby r.Wins descending
                                      select new
                                      {
                                          LastName = r.LastName,
                                          Starts = r.Starts
                                      };
            //第一个集合中的第一项会与第二个集合中的第一项合并
            //第一个集合中的第二项会与第二个集合中的第二项合并，依次类推
            //如果两个序列的项数不同，Zip()方法就在到达较小集合的末尾时停止
            var racers = racernames.Zip(racerNamesAndStarts
                , (first, second) => first.Name + ", starts: " + second.Starts);
        }

        /// <summary>
        /// 分区操作
        /// </summary>
        public void TakeDemo()
        {
            int pageSize = 5;
            int numberPages = (int)Math.Ceiling(Formulal.GetChampions().Count() / (double)pageSize);

            for (int page = 0; page < numberPages; page++)
            {
                Console.WriteLine("Page " + page);

                var racers = (from r in Formulal.GetChampions()
                              orderby r.LastName, r.FirstName
                              select r.FirstName + " " + r.LastName)
                            .Skip(page * pageSize).Take(pageSize);

                foreach (var name in racers)
                {
                    Console.WriteLine(name);
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// 聚合操作
        /// </summary>
        public void Polymerize()
        {
            Console.WriteLine("Count():");
            var query = from r in Formulal.GetChampions()
                        let numberYears = r.Years.Count()
                        where numberYears >= 3
                        orderby numberYears descending, r.LastName
                        select new
                        {
                            Name = r.FirstName + " " + r.LastName,
                            TimesChampion = numberYears
                        };

            foreach (var r in query)
            {
                Console.WriteLine(r.Name + " " + r.TimesChampion);
            }

            Console.WriteLine();
            Console.WriteLine("Sum():");

            var countries = (from c in from r in Formulal.GetChampions()
                                       group r by r.Country into c
                                       select new
                                       {
                                           Country = c.Key,
                                           Wins = (from r1 in c select r1.Wins).Sum()
                                       }
                             orderby c.Wins descending, c.Country
                             select c).Take(5);
            foreach (var country in countries)
            {
                Console.WriteLine(country.Country + " " + country.Wins);
            }
        }

        public void ToListDemo()
        {
            List<Racer> racers = (from r in Formulal.GetChampions()
                                  where r.Starts > 150
                                  orderby r.Starts descending
                                  select r).ToList();
            //ToLookup
            var racers2 = (from r in Formulal.GetChampions()
                           from c in r.Cars
                           select new
                           {
                               Car = c,
                               Racer = r
                           })
                          .ToLookup(cr => cr.Car, cr => cr.Racer);
            if (racers2.Contains("Williams"))
            {
                foreach (var winll in racers2["Williams"])
                {
                    Console.WriteLine(winll);
                }
            }


            var list = new ArrayList(Formulal.GetChampions() as ICollection);
            //基于Object类型的ArrayList集合用Racer对象填充
            var query = from r in list.Cast<Racer>()
                        where r.Country == "China"
                        orderby r.Wins descending
                        select r;
        }

        /// <summary>
        /// 生成操作符
        /// Range()：生成指定范围内的整数序列。 注意：该方法不返回填充了所定义值的集合，这个方法与其他方法一样，也推迟执行查询。
        /// Empty()：返回具有指定类型参数的空IEnumerable<T>。它可以用于需要一个集合的参数，其中可以给参数传递空集合。
        /// Repeat()：生成包含一个重复值的序列。
        /// </summary>
        public void RangeDemo()
        {
            var values = Enumerable.Range(1, 20);
            foreach (var item in values)
            {
                Console.WriteLine($"{item}");
            }
        }

        /// <summary>
        /// 并行Linq
        /// </summary>
        public void ParallelLINQDemo()
        {
            var data = SampleData();
            //查询表达式写法
            var res = (from x in data.AsParallel() where Math.Log(x) < 4 select x).Average();
            //扩展方法写法
            var res2 = data.AsParallel().Where(x => Math.Log(x) < 4).Select(x => x).Average();

            //Partitioner类用于为数组，列表和枚举提供通用的分区策略
            var result = (from x in Partitioner.Create((List<int>)data, true).AsParallel()
                          where Math.Log(x) < 4
                          select x).Average();

            
        }

        public static IEnumerable<int> SampleData()
        {
            const int arraySize = 50000000;
            var r = new Random();
            //连续50000000次随机取出小于140的数字
            return Enumerable.Range(0, arraySize).Select(x => r.Next(140)).ToList();
        }

        public void CancelParallelQueryDemo()
        {
            var cts = new CancellationTokenSource();
            var data = SampleData();
            Task.Run(() => {
                try
                {
                    var res = (from x in data.AsParallel().WithCancellation(cts.Token)
                               where Math.Log(x) < 4
                               select x).Average();
                    Console.WriteLine(res);
                }
                catch (OperationCanceledException ex)
                {
                    Console.WriteLine(ex.Message);
                }

            });

            Console.WriteLine("取消吗?");
            string input = Console.ReadLine();
            if (input.ToLower().Equals("y"))
            {
                cts.Cancel();
            }
        }
    }
}
