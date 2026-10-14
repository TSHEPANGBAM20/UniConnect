using System.ComponentModel;
using System.Runtime.CompilerServices;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;

namespace UniConnect.MAUI.ViewModels
{
    public class NewProjectViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(3);

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public NewProjectViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<bool> CreateProjectAsync()
        {
            if (string.IsNullOrWhiteSpace(ProjectName))
            {
                ErrorMessage = "Project title is required.";
                return false;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                await _apiService.CreateProjectAsync(new CreateProjectRequest
                {
                    ProjectName = ProjectName,
                    ProjectDescription = ProjectDescription,
                    StartDate = StartDate,
                    EndDate = EndDate
                });
                return true;
            }
            catch (Exception)
            {
                ErrorMessage = "Could not create project. Please try again.";
                return false;
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
