
using System.Globalization;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;

namespace UniConnect.MAUI.Views;

public partial class CalendarPage : ContentPage
{
    private readonly ApiService _apiService;

    private List<TaskModel> _tasks = new();
    private List<ProjectModel> _projects = new();

    private DateTime _currentMonth =
        new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    public CalendarPage(ApiService apiService)
    {
        InitializeComponent();

        _apiService = apiService;

        UpdateMonthTitle();
        BuildCalendar();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadTasksAsync();
        await LoadProjectsAsync();
    }

    private static bool IsTaskDone(TaskModel task)
    {
        return string.Equals(
                   task.Status?.Trim(),
                   "DONE",
                   StringComparison.OrdinalIgnoreCase)
               ||
               string.Equals(
                   task.Status?.Trim(),
                   "COMPLETED",
                   StringComparison.OrdinalIgnoreCase);
    }

    private async Task LoadTasksAsync()
    {
        try
        {
            var loadedTasks =
                await _apiService.GetCalendarTasksAsync()
                ?? new List<TaskModel>();

            _tasks = loadedTasks
                .Where(t => !IsTaskDone(t))
                .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Failed to load calendar tasks: {ex.Message}");

            _tasks = new List<TaskModel>();
        }

        BuildCalendar();
        BuildUpcomingList();
    }

    private async Task LoadProjectsAsync()
    {
        try
        {
            _projects = await _apiService.GetProjectsAsync(
                _apiService.CurrentUserId
            ) ?? new List<ProjectModel>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Failed to load calendar projects: {ex.Message}");

            _projects = new List<ProjectModel>();
        }

        BuildCalendar();
        BuildUpcomingProjectsList();
    }

    private void BuildUpcomingProjectsList()
    {
        UpcomingProjectsList.Children.Clear();

        bool isDark =
            Application.Current?.RequestedTheme == AppTheme.Dark;

        var mainText = isDark
            ? Colors.White
            : Color.FromArgb("#111111");

        var subText = isDark
            ? Color.FromArgb("#AAAAAA")
            : Color.FromArgb("#777777");

        var upcomingProjects = _projects
            .Where(p =>
                p.EndDate != default &&
                p.EndDate >= DateOnly.FromDateTime(DateTime.Today))
            .OrderBy(p => p.EndDate)
            .Take(5)
            .ToList();

        if (upcomingProjects.Count == 0)
        {
            UpcomingProjectsList.Children.Add(new Label
            {
                Text = "No upcoming project deadlines.",
                FontSize = 13,
                TextColor = subText
            });

            return;
        }

        foreach (var project in upcomingProjects)
        {
            var projectName = new Label
            {
                Text = project.ProjectName,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = mainText
            };

            var deadline = new Label
            {
                Text = $"Due: {project.EndDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)}",
                FontSize = 12,
                TextColor = subText
            };

            var content = new VerticalStackLayout
            {
                Spacing = 5
            };

            content.Children.Add(projectName);
            content.Children.Add(deadline);

            UpcomingProjectsList.Children.Add(new Border
            {
                Padding = 16,
                BackgroundColor = isDark
                    ? Color.FromArgb("#191919")
                    : Color.FromArgb("#F7F7F7"),
                Stroke = isDark
                    ? Color.FromArgb("#2A2A2A")
                    : Color.FromArgb("#E4E4E4"),
                StrokeThickness = 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                {
                    CornerRadius = 16
                },
                Content = content
            });
        }
    }

    private void BuildUpcomingList()
    {
        UpcomingList.Children.Clear();

        var upcoming = _tasks
            .Where(t =>
                t.DueDate.HasValue &&
                t.DueDate.Value.Date >= DateTime.Today &&
                !IsTaskDone(t))
            .OrderBy(t => t.DueDate)
            .Take(5)
            .ToList();

        bool isDark =
            Application.Current?.RequestedTheme == AppTheme.Dark;

        var mainText = isDark
            ? Colors.White
            : Color.FromArgb("#111111");

        var subText = isDark
            ? Color.FromArgb("#AAAAAA")
            : Color.FromArgb("#777777");

        if (upcoming.Count == 0)
        {
            UpcomingList.Children.Add(new Label
            {
                Text = "No upcoming tasks.",
                FontSize = 13,
                TextColor = subText
            });

            return;
        }

        foreach (var task in upcoming)
        {
            var date = task.DueDate.Value;

            var dateColumn = new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center
            };

            dateColumn.Children.Add(new Label
            {
                Text = date.ToString("dd", CultureInfo.InvariantCulture),
                FontSize = 22,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = mainText
            });

            dateColumn.Children.Add(new Label
            {
                Text = date.ToString("MMM", CultureInfo.InvariantCulture).ToUpper(),
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = subText
            });

            var textColumn = new VerticalStackLayout
            {
                Spacing = 3
            };

            textColumn.Children.Add(new Label
            {
                Text = task.TaskTitle,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = mainText
            });

            textColumn.Children.Add(new Label
            {
                Text = string.IsNullOrWhiteSpace(task.Priority)
                    ? "Task due"
                    : $"{task.Priority} priority",
                FontSize = 12,
                TextColor = subText
            });

            var row = new Grid
            {
                ColumnSpacing = 14
            };

            row.ColumnDefinitions.Add(
                new ColumnDefinition(GridLength.Auto));

            row.ColumnDefinitions.Add(
                new ColumnDefinition(GridLength.Star));

            row.Children.Add(dateColumn);
            row.Children.Add(textColumn);

            Grid.SetColumn(dateColumn, 0);
            Grid.SetColumn(textColumn, 1);

            UpcomingList.Children.Add(new Border
            {
                Padding = 16,
                BackgroundColor = isDark
                    ? Color.FromArgb("#191919")
                    : Color.FromArgb("#F7F7F7"),
                Stroke = isDark
                    ? Color.FromArgb("#2A2A2A")
                    : Color.FromArgb("#E4E4E4"),
                StrokeThickness = 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                {
                    CornerRadius = 16
                },
                Content = row
            });
        }
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

        for (int row = 0; row < 6; row++)
        {
            CalendarGrid.RowDefinitions.Add(
                new RowDefinition { Height = GridLength.Auto });
        }

        int daysInMonth = DateTime.DaysInMonth(
            _currentMonth.Year,
            _currentMonth.Month);

        int firstDayOffset =
            ((int)_currentMonth.DayOfWeek + 6) % 7;

        bool isDark =
            Application.Current?.RequestedTheme == AppTheme.Dark;

        var normalText = isDark
            ? Colors.White
            : Color.FromArgb("#111111");

        var taskColor = Color.FromArgb("#111111");
        var projectColor = Color.FromArgb("#999999");

        for (int day = 1; day <= daysInMonth; day++)
        {
            int position = firstDayOffset + day - 1;

            int row = position / 7;
            int column = position % 7;

            var thisDay = new DateTime(
                _currentMonth.Year,
                _currentMonth.Month,
                day);

            bool hasTask = _tasks.Any(t =>
                !IsTaskDone(t) &&
                t.DueDate.HasValue &&
                t.DueDate.Value.Date == thisDay.Date);

            bool hasProjectDeadline = _projects.Any(p =>
                p.EndDate != default &&
                p.EndDate.ToDateTime(TimeOnly.MinValue).Date == thisDay.Date);

            bool hasBoth = hasTask && hasProjectDeadline;

            Brush dayBackground = Colors.Transparent;
            Color dayTextColor = normalText;

            if (hasBoth)
            {
                dayBackground = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0.5),
                    EndPoint = new Point(1, 0.5),
                    GradientStops =
                    {
                        new GradientStop(taskColor, 0.0f),
                        new GradientStop(taskColor, 0.5f),
                        new GradientStop(projectColor, 0.5f),
                        new GradientStop(projectColor, 1.0f)
                    }
                };

                dayTextColor = Colors.White;
            }
            else if (hasTask)
            {
                dayBackground = new SolidColorBrush(taskColor);
                dayTextColor = Colors.White;
            }
            else if (hasProjectDeadline)
            {
                dayBackground = new SolidColorBrush(projectColor);
                dayTextColor = Colors.White;
            }

            var dayNumber = new Label
            {
                Text = day.ToString(),
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                TextColor = dayTextColor
            };

            var daySquare = new Border
            {
                WidthRequest = 38,
                HeightRequest = 38,
                Padding = 0,
                Background = dayBackground,
                StrokeThickness = 0,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                {
                    CornerRadius = 6
                },
                Content = dayNumber
            };

            var dayCell = new Grid
            {
                HeightRequest = 48,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    daySquare
                }
            };

            CalendarGrid.Children.Add(dayCell);

            Grid.SetRow(dayCell, row);
            Grid.SetColumn(dayCell, column);
        }
    }

    // Bottom navigation.
    private async void OnTabSelected(object? sender, string tab)
        => await TabNavigator.GoToTabAsync(
            this, tab, current: "Calendar");

    private async void OnMenuItemSelected(object? sender, string item)
        => await TabNavigator.HandleMenuItemAsync(this, item);
}
