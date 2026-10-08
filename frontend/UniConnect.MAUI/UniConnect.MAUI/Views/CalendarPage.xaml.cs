using System.Globalization;

namespace UniConnect.MAUI.Views;

public partial class CalendarPage : ContentPage
{
    // The month currently being displayed.
    private DateTime _currentMonth = new DateTime(2026, 10, 1);

    public CalendarPage()
    {
        InitializeComponent();
        UpdateMonthTitle();
        BuildCalendar();
    }

    // Moves to the previous month.
    private void OnPreviousMonthClicked(object sender, EventArgs e)
    {
        _currentMonth = _currentMonth.AddMonths(-1);

        UpdateMonthTitle();
        BuildCalendar();
    }

    // Moves to the next month.
    private void OnNextMonthClicked(object sender, EventArgs e)
    {
        _currentMonth = _currentMonth.AddMonths(1);

        UpdateMonthTitle();
        BuildCalendar();
    }

    // Updates the month/year shown at the top of the calendar.
    private void UpdateMonthTitle()
    {
        MonthTitleLabel.Text = _currentMonth.ToString(
            "MMMM yyyy",
            CultureInfo.InvariantCulture);
    }

    // Creates all the calendar dates dynamically.
    private void BuildCalendar()
    {
        CalendarGrid.Children.Clear();
        CalendarGrid.RowDefinitions.Clear();

        // Six rows are enough for any possible month layout.
        for (int row = 0; row < 6; row++)
        {
            CalendarGrid.RowDefinitions.Add(
                new RowDefinition
                {
                    Height = GridLength.Auto
                });
        }

        // Gets the number of days in the current month.
        int daysInMonth = DateTime.DaysInMonth(
            _currentMonth.Year,
            _currentMonth.Month);

        // Converts Sunday-based DayOfWeek into Monday-based positioning.
        //
        // Monday = 0
        // Tuesday = 1
        // ...
        // Sunday = 6
        int firstDayOffset =
            ((int)_currentMonth.DayOfWeek + 6) % 7;

        // Create each day of the month.
        for (int day = 1; day <= daysInMonth; day++)
        {
            int position = firstDayOffset + day - 1;

            int row = position / 7;
            int column = position % 7;

            var dayLabel = new Label
            {
                Text = day.ToString(),
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                HeightRequest = 38,

                TextColor =
                    Application.Current?.RequestedTheme == AppTheme.Dark
                        ? Colors.White
                        : Color.FromArgb("#111111")
            };

            CalendarGrid.Children.Add(dayLabel);

            Grid.SetRow(dayLabel, row);
            Grid.SetColumn(dayLabel, column);
        }
    }
}
