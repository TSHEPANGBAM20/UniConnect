using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;

namespace UniConnect.MAUI.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        public ObservableCollection<ProjectModel> Projects { get; } = new();
        public ObservableCollection<NotificationModel> Notifications { get; } = new();

        public string WelcomeMessage =>
            $"Welcome Back, {_apiService.CurrentUserName ?? "User"}!";

        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowEmptyProjects));
                OnPropertyChanged(nameof(ShowEmptyNotifications));
            }
        }

        private bool _hasProjects;

        public bool HasProjects
        {
            get => _hasProjects;
            set
            {
                _hasProjects = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowEmptyProjects));
            }
        }

        public bool ShowEmptyProjects =>
            !HasProjects && !IsBusy;

        private bool _hasNotifications;

        public bool HasNotifications
        {
            get => _hasNotifications;
            set
            {
                _hasNotifications = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowEmptyNotifications));
            }
        }

        public bool ShowEmptyNotifications =>
            !HasNotifications && !IsBusy;

        public DashboardViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadProjectsAsync()
        {
            IsBusy = true;

            // The user's name is set after login,
            // so refresh the welcome message when the Dashboard appears.
            OnPropertyChanged(nameof(WelcomeMessage));

            try
            {
                // Backend endpoint:
                // GET /projects
                //
                // The ApiService is responsible for communicating
                // with the Spring Boot backend.
                var projects = await _apiService.GetProjectsAsync(_apiService.CurrentUserId);

                Projects.Clear();

                foreach (var project in projects)
                {
                    Projects.Add(project);
                }

                HasProjects = Projects.Count > 0;

                // Notifications will eventually be loaded from
                // the backend notification endpoint.
                //
                // For now, we keep the collection empty rather than
                // displaying fake/sample notification data.
                Notifications.Clear();
                HasNotifications = false;
            }
            catch (Exception)
            {
                // If the backend is unavailable, don't display
                // fake project data. Simply show the empty state.
                Projects.Clear();
                Notifications.Clear();

                HasProjects = false;
                HasNotifications = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void LoadPreviewProjects()
        {
            IsBusy = true;

            try
            {
                Projects.Clear();
                Notifications.Clear();

                Projects.Add(new ProjectModel
                {
                    ProjectId = 1,
                    ProjectName = "UniConnect",
                    ProjectDescription = "University team collaboration platform",
                    StartDate = new DateOnly(2026, 10, 1),
                    EndDate = new DateOnly(2026, 11, 30),
                    
                });

                Projects.Add(new ProjectModel
                {
                    ProjectId = 2,
                    ProjectName = "Handy Market",
                    ProjectDescription = "Marketplace for local service providers",
                    StartDate = new DateOnly(2026, 9, 15),
                    EndDate = new DateOnly(2026, 12, 15),
                    ProjectStatus = "IN PROGRESS"
                    
                });

                Projects.Add(new ProjectModel
                {
                    ProjectId = 3,
                    ProjectName = "Student Portal",
                    ProjectDescription = "A platform for student resources",
                    StartDate = new DateOnly(2026, 10, 5),
                    EndDate = new DateOnly(2026, 12, 1),
                    ProjectStatus = "IN PROGRESS"
                });

                HasProjects = Projects.Count > 0;
                HasNotifications = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected void OnPropertyChanged(
            [CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(name));
        }
    }
}