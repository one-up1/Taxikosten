using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Taxikosten
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly TimeSpan frameStart = TimeSpan.Parse("08:00:00");
        private static readonly TimeSpan frameEnd = TimeSpan.Parse("18:00:00");

        private static readonly TimeSpan weekendStart = TimeSpan.Parse("22:00:00");
        private static readonly TimeSpan weekendEnd = TimeSpan.Parse("07:00:00");

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbDistance.Focus();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Calc();
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Calc();
        }

        private void Calc()
        {
            // Parse aantal km.
            double cost;
            try
            {
                cost = double.Parse(tbDistance.Text); // 1 per km dus geen berekening nodig.
                Console.WriteLine("distance=" + cost);
                tbDistance.Background = Brushes.Transparent;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing distance: " + ex.Message);
                tbDistance.Background = Brushes.Red;
                tbDistance.SelectAll();
                tbDistance.Focus();
                return;
            }

            // Parse starttijd.
            TimeSpan start;
            try
            {
                start = TimeSpan.Parse(tbStart.Text);
                Console.WriteLine("start=" + start);
                tbStart.Background = Brushes.Transparent;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing start: " + ex.Message);
                tbStart.Background = Brushes.Red;
                tbStart.SelectAll();
                tbStart.Focus();
                return;
            }

            // Parse eindtijd.
            TimeSpan end;
            try
            {
                end = TimeSpan.Parse(tbEnd.Text);
                Console.WriteLine("end=" + end);
                if (end <= start)
                    throw new Exception("Illegal end time");
                tbEnd.Background = Brushes.Transparent;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing end: " + ex.Message);
                tbEnd.Background = Brushes.Red;
                tbEnd.SelectAll();
                tbEnd.Focus();
                return;
            }

            // Bereken totale duur.
            TimeSpan time = end - start;
            StringBuilder message = new StringBuilder();

            // Bereken tijd tussen 08:00 en 18:00.
            TimeSpan timeInFrame = GetTimeInFrame(start, end);
            if (timeInFrame != TimeSpan.Zero)
            {
                cost += timeInFrame.TotalMinutes * 0.25;
                time -= timeInFrame;

                message.Append("Tijd tussen 08:00 en 18:00: " + timeInFrame);
                message.AppendLine();
            }

            // Bereken tijd buiten 08:00 en 18:00.
            if (time != TimeSpan.Zero)
            {
                cost += time.TotalMinutes * 0.45;

                message.Append("Tijd buiten 08:00 en 18:00: " + time);
                message.AppendLine();
            }

            // Bereken weekendtoeslag.
            message.AppendLine();
            if (isWeekend(start))
            {
                message.Append("Weekendtoeslag: " + formatPrice(cost * 0.15));
                message.AppendLine();

                cost *= 1.15;
            }

            // Toen MessageBox met ritprijs.
            message.Append("Ritprijs: " + formatPrice(cost));
            MessageBox.Show(message.ToString(), "Taxikosten",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private TimeSpan GetTimeInFrame(TimeSpan start, TimeSpan end)
        {
            if (start < frameStart)
            {
                if (end <= frameStart)
                    return TimeSpan.Zero;
                start = frameStart;
            }

            if (end > frameEnd)
            {
                if (start >= frameEnd)
                    return TimeSpan.Zero;
                end = frameEnd;
            }

            return end - start;
        }

        private bool isWeekend(TimeSpan time)
        {
            switch (calendar.SelectedDate.Value.DayOfWeek)
            {
                case DayOfWeek.Friday:
                    return time >= weekendStart;
                case DayOfWeek.Saturday:
                case DayOfWeek.Sunday:
                    return true;
                case DayOfWeek.Monday:
                    return time < weekendEnd;
            }
            return false;
        }

        private static string formatPrice(double d)
        {
            return "€ " + Math.Round(d, 2).ToString("0.00");
        }
    }
}
