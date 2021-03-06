using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

using SHU2ICS.DataTypes;

namespace SHU2ICS.Utils
{
    public static class MyUtil
    {
        public static DayOfWeek ParseDayOfWeek(string str)
        {
            var ret = DayOfWeek.Sunday;
            switch (str)
            {
                case "日":
                    ret = DayOfWeek.Sunday;
                    break;
                case "一":
                    ret = DayOfWeek.Monday;
                    break;
                case "二":
                    ret = DayOfWeek.Tuesday;
                    break;
                case "三":
                    ret = DayOfWeek.Wednesday;
                    break;
                case "四":
                    ret = DayOfWeek.Thursday;
                    break;
                case "五":
                    ret = DayOfWeek.Friday;
                    break;
                case "六":
                    ret = DayOfWeek.Saturday;
                    break;
            }
            return ret;
        }


    }

    public static class ScheduleUtil
    {
        public static string SerializeToString(Calendar calendar) => new CalendarSerializer().SerializeToString(calendar);

        public static Course[] ParseSchedule(string[][] rawData)
        {
            var ret = new List<Course>();
            foreach (var rawCourse in rawData)
            {
                var currentCourse = new Course() { CourseName = rawCourse[2], Classroom = rawCourse[7], TeacherName = rawCourse[5] };
                var rawSchedule = rawCourse[6];

                var weeks = Enumerable.Range(1, 10).ToArray();

                // 匹配 x-x周
                var matchWeekRange = Regex.Match(rawSchedule, @"\([0-9]{1,2}-[0-9]{1,2}周\)");
                if (matchWeekRange.Length != 0)
                {
                    var weekRange = Regex.Matches(matchWeekRange.ToString(), "[0-9]{1,2}");
                    var startWeek = int.Parse(weekRange[0].ToString());
                    var endWeek = int.Parse(weekRange[1].ToString());
                    weeks = Enumerable.Range(startWeek, endWeek - startWeek + 1).ToArray();
                    goto GenerateSchedule;
                }

                // 匹配 x周,x周
                var matchSpecifiedWeeks = Regex.Match(rawSchedule, @"\(([0-9]{1,2}周,)*[0-9]{1,2}周\)");
                if (matchSpecifiedWeeks.Length != 0)
                {
                    weeks = (from x in Regex.Matches(matchSpecifiedWeeks.ToString(), "[0-9]{1,2}") select int.Parse(x.ToString())).ToArray();
                    goto GenerateSchedule;
                }

            GenerateSchedule:
                var currentSchedules = new List<Course.Schedule>();
                // 匹配课程
                foreach (var week in weeks)
                {
                    var classes = Regex.Matches(rawSchedule, "[一|二|三|四|五][0-9]{1,2}-[0-9]{1,2}[单|双]?");
                    foreach (var oneClass in classes)
                    {
                        var currentClass = oneClass.ToString();
                        var ScurrentingleSchedule = new Course.Schedule();
                        var jumpThisWeek = (currentClass.Contains("单") && week % 2 == 0) || (currentClass.Contains("双") && week % 2 == 1);
                        if (!jumpThisWeek)
                        {
                            ScurrentingleSchedule.Week = week;
                            ScurrentingleSchedule.DayOfWeek = MyUtil.ParseDayOfWeek(currentClass[0].ToString());
                            var classRange = (from x in Regex.Matches(currentClass, "[0-9]{1,2}") select int.Parse(x.ToString())).ToArray();
                            ScurrentingleSchedule.Classes = Enumerable.Range(classRange[0], classRange[1] - classRange[0] + 1).ToArray();
                            currentSchedules.Add(ScurrentingleSchedule);
                        }
                    }
                }
                currentCourse.CourseSchedules = currentSchedules.ToArray();

                ret.Add(currentCourse);
            }
            return ret.ToArray();
        }

        public static string[][] GetRawSchedule()
        {
            var ret = new List<string[]>();
            var option = new EdgeOptions() { UseChromium = true };
            var driver = new EdgeDriver(option);
            driver.Url = "http://xk.shu.edu.cn/";

            var tabelWait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));
            var tableElement = tabelWait.Until<IWebElement>(drv => drv.FindElement(By.XPath("/html/body/div[2]/div[1]/div[2]/div/table/tbody/tr[2]/td/table")));
            var rows = tableElement.FindElement(By.TagName("tbody")).FindElements(By.TagName("tr"));

            for (var i = 1; i < rows.Count - 1; i++)
            {
                var current = new List<string>();
                var cells = rows[i].FindElements(By.TagName("td"));
                foreach (var cell in cells)
                {
                    current.Add(cell.Text);
                }
                ret.Add(current.ToArray());
            }
            driver.Close();
            return ret.ToArray();
        }

        public static Calendar GenerateICalendar(Course[] courses, DateTime termFirstDay, bool combineSameCourses = true)
        {
            string[] startTimeList = { "08:00", "08:55", "10:00", "10:55", "13:00", "13:55", "15:00", "15:55", "18:00", "18:55", "20:00", "20:55" };
            //var firstDay = new DateTime(2021, 9, 6);
            var calendar = new Calendar();
            //calendar.Name = "课表";
            calendar.AddTimeZone("Asia/Shanghai");
            foreach (var course in courses)
            {
                foreach (var schedule in course.CourseSchedules)
                {
                    var currentDate = termFirstDay + new TimeSpan((schedule.Week - 1) * 7 + (int)schedule.DayOfWeek - 1, 0, 0, 0);

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