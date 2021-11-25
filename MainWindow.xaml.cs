using Microsoft.Win32;
using SHU2ICS.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SHU2ICS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Generate_Click(object sender, RoutedEventArgs e)
        {
            if (browser.Source == new Uri("http://xk.autoisp.shu.edu.cn/StudentQuery/QueryCourseTable"))
            {
                if (firstDayPicker.SelectedDate.HasValue)
                {
                    if (firstDayPicker.SelectedDate.Value.DayOfWeek == DayOfWeek.Monday)
                    {
                        var result = await browser.ExecuteScriptAsync(ScheduleUtil.ReadTableScript);
                        var rawData = JsonSerializer.Deserialize<string[][]>(result);

                        var schedule = ScheduleUtil.ParseSchedule(rawData);
                        var strICS = ScheduleUtil.SerializeToString(ScheduleUtil.GenerateICalendar(schedule, firstDayPicker.SelectedDate.Value));
                        var dialog = new SaveFileDialog();
                        dialog.Filter = "iCalendar文件|*.ics";
                        dialog.FileName = "schedule.ics";
                        dialog.InitialDirectory = Environment.CurrentDirectory;
                        dialog.AddExtension = true;
                        if (dialog.ShowDialog() ?? false)
                        {
                            var writer = new StreamWriter(dialog.FileName);
                            writer.WriteLine(strICS);
                            writer.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("学期第一天不是周一！");
                    }
                }
                else
                {
                    MessageBox.Show("请选择学期第一天！");
                }
            }
            else
            {
                MessageBox.Show("请打开课表查询页面！");
            }
        }
    }
}
