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

        public string WelcomeMessage => $"Welcome Back, {_apiService.CurrentUserName ?? "User"}!";

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public DashboardViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadProjectsAsync()
        {
            IsBusy = true;
            try
            {
                var projects = await _apiService.GetProjectsAsync();
                Projects.Clear();
                foreach (var p in projects)
                    Projects.Add(p);
            }
            catch (Exception)
            {
                // TODO: surface a friendly error banner once error-state UI is designed
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
