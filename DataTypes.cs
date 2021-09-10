using System;

namespace SHU2ICS.DataTypes
{
    public class Course
    {
        public string CourseName { get; set; }
        public string Classroom { get; set; }
        public string TeacherName { get; set; }
        public Schedule[] CourseSchedules { get; set; }

        public class Schedule
        {
            public int Week { get; set; }
            public DayOfWeek DayOfWeek { get; set; }
            public int[] Classes { get; set; }
        }
    }
}