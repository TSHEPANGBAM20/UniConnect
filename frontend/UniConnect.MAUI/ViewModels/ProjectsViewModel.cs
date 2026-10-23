using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;

namespace UniConnect.MAUI.ViewModels
{
    public class ProjectsViewModel : INotifyPropertyChanged
    {
        private static readonly bool UseSampleDataOnFailure = false;

        private readonly ApiService _apiService;

        public ObservableCollection<ProjectModel> Projects { get; } = new();

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ProjectsViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadProjectsAsync()
        {
            IsBusy = true;

            List<ProjectModel> projects;
            try
            {
                projects = await _apiService.GetProjectsAsync(_apiService.CurrentUserId); //invokes the projects to appear according the loaded 
            }
            catch (Exception)
            {
                projects = UseSampleDataOnFailure ? SampleProjects() : new List<ProjectModel>();
            }

            Projects.Clear();
            foreach (var p in projects)
                Projects.Add(p);

            IsBusy = false;
        }

        // Join a project with the 6-digit code from its Team Lead, then open it.
        public async Task<(long? ProjectId, string? Error)> JoinByCodeAsync(string code)
        {
            if (code.Length != 6 || !int.TryParse(code, out int projectCode))
                return (null, "Enter the 6-digit project code.");

            try
            {
                long id = await _apiService.JoinProjectByCodeAsync(projectCode);
                return (id, null);
            }
            catch (Exception ex)
            {
                if (UseSampleDataOnFailure)
                    // Sample mode: 123456 "joins" the first sample project so the flow can be tried.
                    return code == "123456" ? (1, null) : (null, "No project found for that code. (sample mode: try 123456)");

                if (ex is HttpRequestException http)
                {
                    return http.StatusCode switch
                    {
                        System.Net.HttpStatusCode.NotFound => (null, "No project found for that code."),
                        System.Net.HttpStatusCode.Conflict => (null, "You are already a member of that project."),
                        _ => (null, "Couldn't join the project. Please try again.")
                    };
                }
                return (null, "Couldn't join the project. Please try again.");
            }
        }

        private static List<ProjectModel> SampleProjects() => new()
        {
            new ProjectModel { ProjectId = 1, ProjectName = "UniConnect", ProjectStatus = "IN PROGRESS", EndDate = DateOnly.FromDateTime(DateTime.Today).AddDays(2) },
            new ProjectModel { ProjectId = 2, ProjectName = "Database Assignment", ProjectStatus = "IN PROGRESS", EndDate = DateOnly.FromDateTime(DateTime.Today).AddDays(21) },
            new ProjectModel { ProjectId = 3, ProjectName = "Mobile App Development", ProjectStatus = "IN PROGRESS", EndDate = DateOnly.FromDateTime(DateTime.Today).AddDays(45) },
        };

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

