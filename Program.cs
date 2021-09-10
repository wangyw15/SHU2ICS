using System.IO;

using SHU2ICS.Utils;

namespace SHU2ICS
{
    class Program
    {
        static void Main(string[] args)
        {
            var rawData = ScheduleUtil.GetRawSchedule();
            // var json = JsonSerializer.Serialize(rawData, new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });


            //var rawData = JsonSerializer.Deserialize<string[][]>(File.ReadAllText("a.json"));
            var schedule = ScheduleUtil.ParseSchedule(rawData);

            // var json = JsonSerializer.Serialize(schedule, new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            // var writer = new StreamWriter("parsed.json");
            // writer.WriteLine(json);
            // writer.Close();

            //var schedule = JsonSerializer.Deserialize<Course[]>(File.ReadAllText("parsed.json"));
            var strICS = ScheduleUtil.SerializeToString(ScheduleUtil.GenerateICalendar(schedule));
            var writer = new StreamWriter("out.ics");
            writer.WriteLine(strICS);
            writer.Close();
        }
    }
}
