using System.Globalization;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;

namespace UniConnect.MAUI.Views;

public partial class CalendarPage : ContentPage
{
    private readonly ApiService _apiService;
    
    //Every task that has a due date will be displayed in the calendar from the backend
    private List<TaskModel> _tasks = new();

    private List<ProjectModel> _projects = new(); // a list that 




    //Start the calendar on the current month
    private DateTime _currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

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

    private async Task LoadTasksAsync()
    {
        try
        {
            _tasks = await _apiService.GetCalendarTasksAsync() ?? new List<TaskModel>();
        }
        catch (Exception)
        {
            _tasks = new List<TaskModel>();
        }

        BuildCalendar();
        BuildUpcomingList();
    }



  private async Task LoadProjectsAsync()
{
    try
    {
        // Get projects belonging to the logged-in student.
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


bool isDark = Application.Current?.RequestedTheme == AppTheme.Dark;

var mainText = isDark ? Colors.White : Color.FromArgb("#111111");
var subText = isDark ? Color.FromArgb("#AAAAAA") : Color.FromArgb("#777777");

var upcomingProjects = _projects
    .Where(p => p.EndDate != default &&
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

        //5 tasks that are due in the future, sorted by due date
        var upcoming = _tasks
            .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date >= DateTime.Today && t.Status != "Completed")
            .OrderBy(t => t.DueDate)
            .Take(5)
            .ToList();

        bool isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        var mainText = isDark ? Colors.White : Color.FromArgb("#111111");
        var subText = isDark ? Color.FromArgb("AAAAA") : Color.FromArgb("#777777");


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

            var dateColumn = new VerticalStackLayout { HorizontalOptions = LayoutOptions.Center, };
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

            var textColumn = new VerticalStackLayout {Spacing = 3};
            textColumn.Children.Add(new Label
            {
                Text = task.TaskTitle,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = mainText
            });
            textColumn.Children.Add(new Label
            {
                Text = string.IsNullOrWhiteSpace(task.Priority) ? "Task due" : $"{task.Priority} priority",
                FontSize = 12,
                TextColor = subText
            });

            var row  = new Grid {ColumnSpacing = 14};
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto)); 
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));  
            row.Children.Add(dateColumn);
            row.Children.Add(textColumn);
            Grid.SetColumn(dateColumn, 0);
            Grid.SetColumn(textColumn, 1);

            UpcomingList.Children.Add(new Border
            {
                Padding = 16,
                BackgroundColor = isDark ? Color.FromArgb("#191919") : Color.FromArgb("#F7F7F7"),
                Stroke = isDark ? Color.FromArgb("#2A2A2A") : Color.FromArgb("#E4E4E4"),
                StrokeThickness = 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle {CornerRadius = 16 },
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


    private void BuildCalendar(){
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

    // Monday = 0, Tuesday = 1, ... Sunday = 6.
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

        // The date represented by this calendar cell.
        var thisDay = new DateTime(
        _currentMonth.Year,
        _currentMonth.Month,
        day);

// Check for tasks due on this date.
bool hasTask = _tasks.Any(t =>
    t.DueDate.HasValue &&
    t.DueDate.Value.Date == thisDay);

// Check for projects ending on this date.
bool hasProjectDeadline = _projects.Any(p =>
    p.EndDate.ToDateTime(TimeOnly.MinValue).Date == thisDay.Date);

// Highlight dates containing tasks or project deadlines.
if (hasTask || hasProjectDeadline)
{
    dayLabel.BackgroundColor = Color.FromArgb("#3B82F6");
    dayLabel.TextColor = Colors.White;
        }

        CalendarGrid.Children.Add(dayLabel);

        Grid.SetRow(dayLabel, row);
        Grid.SetColumn(dayLabel, column);
    }
}


    // Bottom navigation: Home/Projects switch pages, Calendar is the current tab.
    private async void OnTabSelected(object? sender, string tab)
        => await TabNavigator.GoToTabAsync(this, tab, current: "Calendar");

    private async void OnMenuItemSelected(object? sender, string item)
        => await TabNavigator.HandleMenuItemAsync(this, item);
}
