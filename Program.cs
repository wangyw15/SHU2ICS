using System;
using System.IO;

using SHU2ICS.Utils;

namespace SHU2ICS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("请输入学期第一天（默认这天周一）（例如 2021-09-06）：");
            var firstDay = DateTime.Parse(Console.ReadLine());

            Console.WriteLine("请在打开的浏览器中登录并打开课表查询页面（最多等待120秒）");
            var rawData = ScheduleUtil.GetRawSchedule();
            var schedule = ScheduleUtil.ParseSchedule(rawData);
            var strICS = ScheduleUtil.SerializeToString(ScheduleUtil.GenerateICalendar(schedule, firstDay));
            var writer = new StreamWriter("out.ics");
            writer.WriteLine(strICS);
            writer.Close();
            
            Console.WriteLine("文件名：out.ics，按任意键退出");
            Console.ReadKey();
        }
    }
}
