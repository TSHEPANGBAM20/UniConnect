using System.ComponentModel;
using System.Runtime.CompilerServices;
using UniConnect.MAUI.Models;
using UniConnect.MAUI.Services;

namespace UniConnect.MAUI.ViewModels
{
    public class SignUpViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

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

        public SignUpViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<bool> RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please fill in all required fields.";
                return false;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return false;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                await _apiService.RegisterAsync(new RegisterRequest
                {
                    Name = Name,
                    Surname = Surname,
                    Email = Email,
                    Password = Password
                });
                return true;
            }
            catch (Exception)
            {
                ErrorMessage = "Registration failed. Try a different email.";
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
