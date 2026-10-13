using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;

namespace UniConnect.MAUI.ViewModels
{
    public class ProjectsViewModel : INotifyPropertyChanged
    {
        // DEV SWITCH: the backend has no /projects endpoint yet. While true, a failed
        // call falls back to sample data so the UI can be built and tested.
        // Set to false once the API works, otherwise a broken backend looks like a working app.
        private const bool UseSampleDataOnFailure = true;

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
                projects = await _apiService.GetProjectsAsync();
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

        private static List<ProjectModel> SampleProjects() => new()
        {
            new ProjectModel { ProjectId = 1, ProjectName = "UniConnect", Role = "TEAM_LEAD", EndDate = DateTime.Today.AddDays(2) },
            new ProjectModel { ProjectId = 2, ProjectName = "Database Assignment", Role = "TEAM_MEMBER", EndDate = DateTime.Today.AddDays(21) },
            new ProjectModel { ProjectId = 3, ProjectName = "Mobile App Development", Role = "TEAM_MEMBER", EndDate = DateTime.Today.AddDays(45) },
        };

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
