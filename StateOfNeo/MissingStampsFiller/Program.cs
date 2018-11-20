using Microsoft.EntityFrameworkCore;
using StateOfNeo.Common.Extensions;
using StateOfNeo.Data;
using System;
using System.Linq;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "Server=.\\;Database=neo-monitor-main;Trusted_Connection=True;MultipleActiveResultSets=true";
            var db = StateOfNeoContext.Create(connectionString);

            var items = db.Blocks.Where(x => x.DailyStamp == 0).Take(50_000).ToList();
            var count = 0;
            var iteration = 1;
            while (items.Any())
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                foreach (var item in items)
                {
                    var date = item.Timestamp.ToUnixDate();
                    var hourStamp = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0).ToUnixTimestamp();
                    var dayStamp = new DateTime(date.Year, date.Month, date.Day).ToUnixTimestamp();
                    var monthStamp = new DateTime(date.Year, date.Month, 1).ToUnixTimestamp();

                    item.HourlyStamp = hourStamp;
                    item.DailyStamp = dayStamp;
                    item.MonthlyStamp = monthStamp;
                }

                db.SaveChanges();

                sw.Stop();

                Console.WriteLine($"{iteration} - {sw.ElapsedMilliseconds}");

                count += 50_000;
                if (count % 150_000 == 0)
                {
                    db = StateOfNeoContext.Create(connectionString);
                }

                items = db.Blocks.Where(x => x.DailyStamp == 0).Take(50_000).ToList();

                iteration++;
            }

            db.SaveChanges();
        }
    }
}
