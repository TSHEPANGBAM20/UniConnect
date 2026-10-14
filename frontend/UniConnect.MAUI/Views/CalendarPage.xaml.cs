using System.Globalization;

namespace UniConnect.MAUI.Views;

public partial class CalendarPage : ContentPage
{
    private DateTime _currentMonth = new DateTime(2026, 10, 1);

    public CalendarPage()
    {
        InitializeComponent();

        // Build the calendar when the page first opens.
        BuildCalendar();
    }

    private void OnPreviousMonthClicked(object sender, EventArgs e)
    {
        _currentMonth = _currentMonth.AddMonths(-1);

        UpdateMonthTitle();
        BuildCalendar();
    }

    private void OnNextMonthClicked(object sender, EventArgs e)
    {
        _currentMonth = _currentMonth.AddMonths(1);

        UpdateMonthTitle();
        BuildCalendar();
    }

    private void UpdateMonthTitle()
    {
        MonthTitleLabel.Text = _currentMonth.ToString(
            "MMMM yyyy",
            CultureInfo.InvariantCulture);
    }

    private void BuildCalendar()
    {
        CalendarGrid.Children.Clear();
        CalendarGrid.RowDefinitions.Clear();

        // Always create 6 rows so every month fits neatly.
        for (int row = 0; row < 6; row++)
        {
            CalendarGrid.RowDefinitions.Add(
                new RowDefinition { Height = GridLength.Auto });
        }

        int daysInMonth = DateTime.DaysInMonth(
            _currentMonth.Year,
            _currentMonth.Month);

        // Monday = 0, Tuesday = 1, ... Sunday = 6
        int firstDayOffset =
            ((int)_currentMonth.DayOfWeek + 6) % 7;

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
                TextColor = Application.Current?.RequestedTheme == AppTheme.Dark
                    ? Colors.White
                    : Color.FromArgb("#111111"),
                HeightRequest = 38
            };

            CalendarGrid.Children.Add(dayLabel);
            Grid.SetRow(dayLabel, row);
            Grid.SetColumn(dayLabel, column);
        }
    }
}