using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace TestTask.Controls
{
    public partial class intervalCalendar : UserControl
    {
        private bool _isOpen; //развернут ли календарь
        public bool isOpen
        {
            get
            {
                return _isOpen;
            }
            private set
            {
                //Отрисовка стрелки в кнопке открытия календаря
                Polyline newLine = new Polyline();
                newLine.Stroke = Brushes.DarkSlateGray;

                Point firstPoint;
                Point secondPoint;
                Point endPoint;

                if (value == true)
                {
                    firstPoint = new Point(1, dropDownBtnLine.Height - 1);
                    secondPoint = new Point(dropDownBtnLine.Width / 2, 3);
                    endPoint = new Point(dropDownBtnLine.Width - 1, dropDownBtnLine.Height - 1);
                }
                else
                {
                    firstPoint = new Point(1, 3);
                    secondPoint = new Point(dropDownBtnLine.Width / 2, dropDownBtnLine.Height - 1);
                    endPoint = new Point(dropDownBtnLine.Width - 1, 3);
                }

                newLine.Points.Add(firstPoint);
                newLine.Points.Add(secondPoint);
                newLine.Points.Add(endPoint);
                dropDownButton.Content = newLine;

                _isOpen = value;
            }
        }
        public double closeOpenAnimationDuration = 0.5;

        public string textBoxContent // строка, которая отображается в TextBox
        {
            get { return GetValue(textBoxContentrProperty).ToString(); }
            set
            {
                SetValue(textBoxContentrProperty, value);
            }
        }

        //Открытые свойства для биндинга с View, предназначеные для панели навигации
        private string previousYear
        {
            get { return string.Empty; }
            set
            {
                SetValue(previousYearProperty, value);
            }
        }
        private string currentYear
        {
            get { return string.Empty; }
            set
            {
                SetValue(currentYearProperty, value);
            }
        }
        private string nextYear
        {
            get { return string.Empty; }
            set
            {
                SetValue(nextYearProperty, value);
            }
        }

        private string previousMonth
        {
            get { return string.Empty; }
            set
            {
                SetValue(previousMonthProperty, value);
            }
        }
        private string currentMonth
        {
            get { return string.Empty; }
            set
            {
                SetValue(currentMonthProperty, value);
            }
        }
        private string nextMonth
        {
            get { return string.Empty; }
            set
            {
                SetValue(nextMonthProperty, value);
            }
        }

        public readonly DependencyProperty textBoxContentrProperty;
        public readonly DependencyProperty previousYearProperty;
        public readonly DependencyProperty currentYearProperty;
        public readonly DependencyProperty nextYearProperty;
        public readonly DependencyProperty previousMonthProperty;
        public readonly DependencyProperty currentMonthProperty;
        public readonly DependencyProperty nextMonthProperty;

        //результирующий интервал. Из вне взаимодействие происходит с ними
        private DateTime _intevalStart;
        public DateTime intevalStart
        {
            get { return _intevalStart; }
            set
            {
                if (value > _intevalEnd & _intevalEnd != DateTime.MinValue)
                {
                    throw new Exception("The beginning of the interval must be greater than the end of the interval");
                }
                else
                {
                    _intevalStart = value;
                    textBoxContentRefresh();
                    dataChanged?.Invoke(this, new dataChangedEventArg(_intevalStart, _intevalEnd));
                }
            }
        }
        private DateTime _intevalEnd;
        public DateTime intevalEnd
        {
            get { return _intevalEnd; }
            set
            {
                if (value < _intevalStart.Date & _intevalStart != DateTime.MinValue)
                {
                    throw new Exception("The beginning of the interval must be greater than the end of the interval");
                }
                else
                {
                    _intevalEnd = value;
                    textBoxContentRefresh();
                    dataChanged?.Invoke(this, new dataChangedEventArg(_intevalStart, _intevalEnd));
                }
            }
        }

        //интервал дат, который отображается в открытом календаре в конкретный момент
        private DateTime displayDate;
        //интервал выделенных дат, который отображается в открытом календаре в конкретный момент
        private Nullable<DateTime> selectedIntevalStart;
        private Nullable<DateTime> selectedIntevalEnd;

        public EventHandler<dataChangedEventArg> dataChanged;
        public class dataChangedEventArg
        {
            public DateTime newIntervalStart;
            public DateTime newIntervalEnd;

            public dataChangedEventArg(DateTime start, DateTime end)
            {
                this.newIntervalStart = start;
                this.newIntervalEnd = end;
            }
        }//аргумент для ивента изменения результирующего интервала

        public intervalCalendar()
        {
            InitializeComponent();

            textBoxContentrProperty = DependencyProperty.Register("textBoxContent", typeof(string), typeof(intervalCalendar));
            previousYearProperty = DependencyProperty.Register("previousYear", typeof(string), typeof(intervalCalendar));
            currentYearProperty = DependencyProperty.Register("currentYear", typeof(string), typeof(intervalCalendar));
            nextYearProperty = DependencyProperty.Register("nextYear", typeof(string), typeof(intervalCalendar));
            previousMonthProperty = DependencyProperty.Register("previousMonth", typeof(string), typeof(intervalCalendar));
            currentMonthProperty = DependencyProperty.Register("currentMonth", typeof(string), typeof(intervalCalendar));
            nextMonthProperty = DependencyProperty.Register("nextMonth", typeof(string), typeof(intervalCalendar));

            intevalStart = DateTime.Now.Date;
            intevalEnd = DateTime.Now.Date;
            textBoxContentRefresh();

            DataContext = this;

            Window parent = App.Current.MainWindow;
            parent.PreviewMouseDown += mouseDownHandler;
        }

        private void textBoxContentRefresh()//обновление содержимого Textbox-а
        {
            textBoxContent = _intevalStart.ToShortDateString() + "-" + _intevalEnd.ToShortDateString();
        }
        private void updateDisplyDate(DateTime value)//обновление выбранных дат в панели навигации
        {
            displayDate = value;

            previousYear = (value.Year - 1).ToString();
            currentYear = value.Year.ToString();
            nextYear = (value.Year + 1).ToString();

            previousMonth = value.AddMonths(-1).ToString("MMMM").Substring(0, 3);
            currentMonth = value.ToString("MMMM");
            nextMonth = value.AddMonths(1).ToString("MMMM").Substring(0, 3);

            fillCalendarGrid();
            tuneCalendarIntervalSelectedState();
        }
        private void fillCalendarGrid()//заполнение календаря
        {
            DateTime firstDayOfCurrMonth = new DateTime(displayDate.Year, displayDate.Month, 1);
            DateTime lastDayOfCurrMonth = firstDayOfCurrMonth.AddDays(DateTime.DaysInMonth(firstDayOfCurrMonth.Year, firstDayOfCurrMonth.Month)).AddDays(-1);
            int currentMonthFirstDayOfWeek = Convert.ToInt32(firstDayOfCurrMonth.DayOfWeek.ToString("d"));
            int currentMonthLastDayOfWeek = Convert.ToInt32(lastDayOfCurrMonth.DayOfWeek.ToString("d"));

            //Потому что Вс в enum daysOfWeek это 0, а не 7
            currentMonthFirstDayOfWeek = (currentMonthFirstDayOfWeek == 0) ? 7 : currentMonthFirstDayOfWeek;
            currentMonthLastDayOfWeek = (currentMonthFirstDayOfWeek == 0) ? 7 : currentMonthFirstDayOfWeek;

            //Период, который будет отображаться
            DateTime displayPeriodStart = firstDayOfCurrMonth.AddDays(-1 * (currentMonthFirstDayOfWeek - 1));
            DateTime displayPeriodEnd = lastDayOfCurrMonth.AddDays(7 - currentMonthLastDayOfWeek);


            while (displayPeriodStart < displayPeriodEnd)
            {
                for (int i = 1; i <= 6; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        Label newDateLable = new Label();
                        newDateLable.Content = displayPeriodStart.Day;
                        //Входит ли эта дата в выбранный месяц
                        if (displayPeriodStart < firstDayOfCurrMonth || displayPeriodStart > lastDayOfCurrMonth)
                        {
                            Style bodyLableStyle = calendarGrid.FindResource("calendarOtherMonthCellLable") as Style;
                            newDateLable.Style = bodyLableStyle;
                        }
                        else
                        {
                            Style bodyLableStyle = calendarGrid.FindResource("calendarCurrMonthCellLable") as Style;
                            newDateLable.Style = bodyLableStyle;
                            newDateLable.MouseLeftButtonDown += currMonthCellClickHandler;//для обработки выбора дней
                        }

                        removeLableFromGrid(i, j);
                        Grid.SetRow(newDateLable, i);
                        Grid.SetColumn(newDateLable, j);
                        calendarGrid.Children.Add(newDateLable);

                        displayPeriodStart = displayPeriodStart.AddDays(1);
                    }
                }
            }

        }
        private void removeLableFromGrid(int row, int column) //удаление UI элемента из конкретной ячейки Grid-а
        {
            foreach (UIElement item in calendarGrid.Children)
            {
                if (Grid.GetRow(item) == row & Grid.GetColumn(item) == column)
                {
                    calendarGrid.Children.Remove(item);
                    break;
                }
            }
        }
        private void dropDownButtonClickHandler()
        {
            if (isOpen)
            {
                closeCalendar(false);
            }
            else
            {
                string dateStringFirst = txtResultDateInterval.Text.Split('-')[0];
                string dateStringSecond = txtResultDateInterval.Text.Split('-')[1];
                DateTime.TryParse(dateStringFirst, out _intevalStart);
                DateTime.TryParse(dateStringSecond, out _intevalEnd);

                selectedIntevalStart = new Nullable<DateTime>(intevalStart);
                selectedIntevalEnd = new Nullable<DateTime>(intevalEnd); ;

                updateDisplyDate(selectedIntevalEnd.Value);

                openCalendar();
            }
        }
        private void currMonthCellClickHandler(object sender, MouseButtonEventArgs e)//обработчик выбора дат
        {
            Label currLable = (sender as Label);
            Nullable<DateTime> clickedDate = new Nullable<DateTime>(new DateTime(displayDate.Year, displayDate.Month, (int)currLable.Content));//дата, на которую произошло нажатие

            if ((clickedDate.Value >= selectedIntevalStart.Value & clickedDate.Value <= selectedIntevalEnd.Value) ||
                 (selectedIntevalStart.Value != selectedIntevalEnd.Value)//если выбрали дату, которая входит в уже выделенный интервал -> сброс выделения
               )
            {
                selectedIntevalStart = clickedDate;
                selectedIntevalEnd = clickedDate;
                tuneCalendarIntervalSelectedState();
            }
            else//выделение нового интервала
            {
                if (clickedDate.Value < selectedIntevalStart.Value)
                {
                    selectedIntevalStart = clickedDate;
                }
                else
                {
                    selectedIntevalEnd = clickedDate;
                }

                closeCalendar(true);

            }

        }
        private void tuneCalendarIntervalSelectedState()//определение, какие ячайки календаря входят в выделенный интервал -> применение соответсвующего стиля
        {
            if (selectedIntevalStart == null)
            {
                return;
            }
            foreach (UIElement control in calendarGrid.Children)
            {
                //узнаем принадлежит ли дата выбранному месяцу
                if (control is Label &&
                        (
                            (control as Label).Style == calendarGrid.FindResource("calendarCurrMonthCellLable") as Style ||
                            (control as Label).Style == calendarGrid.FindResource("selectedCellLable") as Style
                        )
                    )
                {
                    Label buffLable = control as Label;
                    DateTime currDate = new DateTime(displayDate.Year, displayDate.Month, (int)buffLable.Content);//определяем, какую дату представляет эта ячейка
                    if (currDate >= selectedIntevalStart.Value & currDate <= selectedIntevalEnd.Value)//входит ли она в выделенный интервал
                    {
                        buffLable.Style = calendarGrid.FindResource("selectedCellLable") as Style;
                    }
                    else
                    {
                        buffLable.Style = calendarGrid.FindResource("calendarCurrMonthCellLable") as Style;
                    }
                }
            }
        }
        private void closeCalendar(bool withSave)//закрытие календаря. withSave - с сохранением изменений или нет
        {
            if (withSave)
            {
                if (selectedIntevalStart.Value < selectedIntevalEnd.Value)
                {
                    _intevalStart = selectedIntevalStart.Value;
                    _intevalEnd = selectedIntevalEnd.Value;
                }
                else
                {
                    _intevalStart = selectedIntevalEnd.Value;
                    _intevalEnd = selectedIntevalStart.Value;
                }

                textBoxContentRefresh();
                dataChanged?.Invoke(this, new dataChangedEventArg(intevalStart, intevalEnd));
            }

            DoubleAnimation closingAnimation = new DoubleAnimation();
            closingAnimation.From = this.ActualHeight;
            closingAnimation.To = 0;
            closingAnimation.Duration = TimeSpan.FromSeconds(closeOpenAnimationDuration);
            this.BeginAnimation(FrameworkElement.HeightProperty, closingAnimation);

            isOpen = false;
        }
        private void openCalendar()
        {
            this.MaxHeight = 360;
            DoubleAnimation closingAnimation = new DoubleAnimation();
            closingAnimation.From = 0;
            closingAnimation.To = 360;
            closingAnimation.Duration = TimeSpan.FromSeconds(closeOpenAnimationDuration);
            this.BeginAnimation(FrameworkElement.HeightProperty, closingAnimation);

            isOpen = true;
        }

        //Events handlers
        private void dropDownButton_Click(object sender, RoutedEventArgs e)
        {
            dropDownButtonClickHandler();
        }

        private void leftYearSelectorButton_Click(object sender, RoutedEventArgs e)
        {
            updateDisplyDate(displayDate.AddYears(-1));
        }
        private void rightYearSelectorButton_Click(object sender, RoutedEventArgs e)
        {
            updateDisplyDate(displayDate.AddYears(1));
        }
        private void leftMonthrSelectorButton_Click(object sender, RoutedEventArgs e)
        {
            updateDisplyDate(displayDate.AddMonths(-1));
        }
        private void rightMonthSelectorButton_Click(object sender, RoutedEventArgs e)
        {
            updateDisplyDate(displayDate.AddMonths(1));
        }

        private void previousYearLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            updateDisplyDate(displayDate.AddYears(-1));
        }
        private void nextYearLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            updateDisplyDate(displayDate.AddYears(1));
        }
        private void previousMonthLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            updateDisplyDate(displayDate.AddMonths(-1));
        }
        private void nextMonthLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            updateDisplyDate(displayDate.AddMonths(1));
        }

        private void weekAgoButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedIntevalStart.Value == selectedIntevalEnd.Value || selectedIntevalEnd == null)
            {
                selectedIntevalEnd = new Nullable<DateTime>(selectedIntevalStart.Value.AddDays(-7));
                closeCalendar(true);
            }
        }
        private void monthAgoButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedIntevalStart.Value == selectedIntevalEnd.Value || selectedIntevalEnd == null)
            {
                selectedIntevalEnd = new Nullable<DateTime>(selectedIntevalStart.Value.AddMonths(-1));
                closeCalendar(true);
            }
        }
        private void yearAgoButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedIntevalStart.Value == selectedIntevalEnd.Value || selectedIntevalEnd == null)
            {
                selectedIntevalEnd = new Nullable<DateTime>(selectedIntevalStart.Value.AddYears(-1));
                closeCalendar(true);
            }
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                closeCalendar(false);
            }
        }
        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            closeCalendar(false);
        }
        private void mouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            Point mousePosition = e.GetPosition(this);
            if (mousePosition.X > this.ActualWidth || mousePosition.Y > this.ActualHeight)
            {
                closeCalendar(false);
            }
        }

        private void txtResultDateInterval_KeyDown(object sender, KeyEventArgs e)
        {
            int cursorPosition = txtResultDateInterval.SelectionStart;
    
            char downKeyChar;
            downKeyChar = GetCharFromKey(e.Key);

            if (!char.IsDigit(downKeyChar) & downKeyChar != '.' & downKeyChar != '-' & 
                e.Key != Key.Back & e.Key != Key.Delete)
            {
                e.Handled = (e.Key != Key.Left & e.Key != Key.Up & e.Key != Key.Right & e.Key != Key.Down);
                return;
            }

            string futureString = txtResultDateInterval.Text.Insert(cursorPosition, downKeyChar.ToString());
            if (!futureString.Contains("-"))
            {
                txtResultDateInterval.Style = this.FindResource("textBoxIncorrectStyle") as Style;
                dropDownButton.IsEnabled = false;
                return;
            }
            string dateStringFirst = futureString.Split('-')[0];
            string dateStringSecond = futureString.Split('-')[1];

            DateTime dateFirst;
            DateTime dateSecond;
            var shortDatePattern = System.Globalization.DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            DateTime.TryParseExact(dateStringFirst, shortDatePattern, new System.Globalization.CultureInfo("en-Us"), System.Globalization.DateTimeStyles.None, out dateFirst);
            DateTime.TryParseExact(dateStringSecond, shortDatePattern, new System.Globalization.CultureInfo("en-Us"), System.Globalization.DateTimeStyles.None, out dateSecond);

            if (dateFirst == DateTime.MinValue || dateSecond == DateTime.MinValue || dateFirst > dateSecond)
            {
                txtResultDateInterval.Style = this.FindResource("textBoxIncorrectStyle") as Style;
                dropDownButton.IsEnabled = false;
            }
            else
            {
                txtResultDateInterval.Style = this.FindResource("textBoxCorrectStyle") as Style;
                dropDownButton.IsEnabled = true;
                _intevalStart = dateFirst;
                _intevalEnd = dateSecond;
                dataChanged?.Invoke(this,new dataChangedEventArg(_intevalStart, _intevalEnd));
            }

        }
        private void txtResultDateInterval_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            closeCalendar(false);
        }

        //Преобразование Key в Char
        private enum MapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3,
        }

        [DllImport("user32.dll")]
        private static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
            StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        private static char GetCharFromKey(Key key)
        {
            char ch = ' ';

            int virtualKey = KeyInterop.VirtualKeyFromKey(key);
            byte[] keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            uint scanCode = MapVirtualKey((uint)virtualKey, MapType.MAPVK_VK_TO_VSC);
            StringBuilder stringBuilder = new StringBuilder(2);

            int result = ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result)
            {
                case -1:
                    break;
                case 0:
                    break;
                case 1:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
                default:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
            }
            return ch;
        }

    }
}
