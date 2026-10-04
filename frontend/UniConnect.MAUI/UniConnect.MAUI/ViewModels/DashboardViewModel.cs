using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;

namespace UniConnect.MAUI.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        // DEV SWITCH: while the backend has no /projects endpoint, fall back to sample
        // data so the UI can be built and tested. Set to false once the API works.
        private const bool UseSampleDataOnFailure = true;

        private readonly ApiService _apiService;

        public ObservableCollection<ProjectModel> Projects { get; } = new();
        public ObservableCollection<NotificationModel> Notifications { get; } = new();

        public string WelcomeMessage => $"Welcome Back, {_apiService.CurrentUserName ?? "User"}!";

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        private bool _hasProjects;
        public bool HasProjects
        {
            get => _hasProjects;
            set { _hasProjects = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowEmptyProjects)); }
        }
        public bool ShowEmptyProjects => !HasProjects && !IsBusy;

        private bool _hasNotifications;
        public bool HasNotifications
        {
            get => _hasNotifications;
            set { _hasNotifications = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowEmptyNotifications)); }
        }
        public bool ShowEmptyNotifications => !HasNotifications && !IsBusy;

        public DashboardViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadProjectsAsync()
        {
            IsBusy = true;
            OnPropertyChanged(nameof(WelcomeMessage)); // name is set after login, so refresh it

            List<ProjectModel> projects;
            try
            {
                projects = await _apiService.GetProjectsAsync();
            }
            catch (Exception)
            {
                projects = UseSampleDataOnFailure ? SampleProjects() : new List<ProjectModel>();
            }

            Projects.Clear();
            foreach (var p in projects)
                Projects.Add(p);

            BuildNotifications(projects);

            HasProjects = Projects.Count > 0;
            HasNotifications = Notifications.Count > 0;
            IsBusy = false;
            OnPropertyChanged(nameof(ShowEmptyProjects));
            OnPropertyChanged(nameof(ShowEmptyNotifications));
        }

        // Deadline notifications derived from project end dates.
        // Task-level notifications can be added here once the calendar/tasks endpoint is ready.
        private void BuildNotifications(IEnumerable<ProjectModel> projects)
        {
            Notifications.Clear();
            var today = DateTime.Today;
            int nextId = 1; // local placeholder ids until the backend supplies NotificationId

            foreach (var p in projects
                .Where(p => p.EndDate.Date >= today)
                .OrderBy(p => p.EndDate)
                .Take(5))
            {
                int days = (p.EndDate.Date - today).Days;
                string when = days switch
                {
                    0 => "today",
                    1 => "tomorrow",
                    < 14 => $"in {days} days",
                    _ => $"in {days / 7} weeks"
                };

                // NotificationModel (defined in Models/MessageModel.cs) has only:
                // NotificationId, Message, IsRead.
                Notifications.Add(new NotificationModel
                {
                    NotificationId = nextId++,
                    Message = $"{p.ProjectName} is due {when}.",
                    IsRead = false
                });
            }
        }

        private static List<ProjectModel> SampleProjects() => new()
        {
            new ProjectModel { ProjectId = 1, ProjectName = "UniConnect", Role = "TEAM_LEAD", EndDate = DateTime.Today.AddDays(2) },
            new ProjectModel { ProjectId = 2, ProjectName = "Database Assignment", Role = "TEAM_MEMBER", EndDate = DateTime.Today.AddDays(21) },
        };

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}