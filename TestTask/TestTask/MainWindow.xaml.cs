using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using TestTask.Controls;

namespace TestTask
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            intervalCalendar.dataChanged += intevalCalendarDataChangedHandler;
            button.Content = "Назначить дату с 01.01.2017 по " + DateTime.Now.ToShortDateString();
        }

        public void intevalCalendarDataChangedHandler(object sender, intervalCalendar.dataChangedEventArg e)
        {
            lable.Content = "Выбран период в " + e.newIntervalEnd.Subtract(e.newIntervalStart).Days.ToString() + " дней";
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            intervalCalendar.intevalStart = new DateTime(2017, 1, 1);
            intervalCalendar.intevalEnd = DateTime.Now;
        }
    }
}
