using System;
using System.IO;
using System.Linq;

using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

using SHU2ICS.DataTypes;
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
            var strICS = new CalendarSerializer().SerializeToString(GenerateICalendar(schedule));
            var writer = new StreamWriter("out.ics");
            writer.WriteLine(strICS);
            writer.Close();
        }

        private static Calendar GenerateICalendar(Course[] courses, bool combineSameCourses = true)
        {
            string[] startTimeList = { "08:00", "08:55", "10:00", "10:55", "13:00", "13:55", "15:00", "15:55", "18:00", "18:55", "20:00", "20:55" };
            var firstDay = new DateTime(2021, 9, 6);
            var calendar = new Calendar();
            //calendar.Name = "课表";
            calendar.AddTimeZone("Asia/Shanghai");
            foreach (var course in courses)
            {
                foreach (var schedule in course.CourseSchedules)
                {
                    var currentDate = firstDay + new TimeSpan((schedule.Week - 1) * 7 + (int)schedule.DayOfWeek - 1, 0, 0, 0);

                    if (combineSameCourses)
                    {
                        var currentEvent = new CalendarEvent();
                        currentEvent.Summary = course.CourseName;
                        currentEvent.Location = course.Classroom;
                        currentEvent.Organizer = new Organizer(course.TeacherName);

                        var startTime = DateTime.Parse(startTimeList[schedule.Classes[0] - 1]);
                        currentEvent.Start = new CalDateTime(new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, startTime.Hour, startTime.Minute, startTime.Second));

                        var endTime = DateTime.Parse(startTimeList[schedule.Classes.Last() - 1]) + new TimeSpan(0, 45, 0);
                        currentEvent.End = new CalDateTime(new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, endTime.Hour, endTime.Minute, endTime.Second));
                        calendar.Events.Add(currentEvent);
                    }
                    else
                    {
                        foreach (var singleClass in schedule.Classes)
                        {
                            var currentEvent = new CalendarEvent();
                            currentEvent.Summary = course.CourseName;
                            currentEvent.Location = course.Classroom;
                            currentEvent.Organizer = new Organizer(course.TeacherName);
                            var newCourse = new CalendarEvent();
                            var startTime = DateTime.Parse(startTimeList[singleClass - 1]);
                            currentEvent.Start = new CalDateTime(new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, startTime.Hour, startTime.Minute, startTime.Second));
                            currentEvent.Duration = new TimeSpan(0, 45, 0);
                            calendar.Events.Add(currentEvent);
                        }
                    }
                }
            }
            return calendar;
        }
    }
}
