
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UniConnect.MAUI.Services;

namespace UniConnect.MAUI.ViewModels
{
    public class MyAccountViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        private long? _userId;

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;

                _name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FullName));
                OnPropertyChanged(nameof(Initials));
            }
        }

        private string _surname = string.Empty;
        public string Surname
        {
            get => _surname;
            set
            {
                if (_surname == value)
                    return;

                _surname = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FullName));
                OnPropertyChanged(nameof(Initials));
            }
        }

        // Combines first name and surname for display.
        public string FullName =>
            $"{Name} {Surname}".Trim();

        // Creates profile initials.
        public string Initials
        {
            get
            {
                string fullName = FullName;

                if (string.IsNullOrWhiteSpace(fullName))
                    return "U";

                string[] parts = fullName.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 1)
                    return parts[0][0].ToString().ToUpperInvariant();

                return $"{char.ToUpperInvariant(parts[0][0])}" +
                       $"{char.ToUpperInvariant(parts[^1][0])}";
            }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                if (_email == value)
                    return;

                _email = value;
                OnPropertyChanged();
            }
        }

        // Original values used when cancelling edits.
        private string _originalName = string.Empty;
        private string _originalSurname = string.Empty;
        private string _originalEmail = string.Empty;

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            private set
            {
                if (_isEditing == value)
                    return;

                _isEditing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotEditing));
            }
        }

        public bool IsNotEditing => !IsEditing;

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                if (_errorMessage == value)
                    return;

                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public MyAccountViewModel(ApiService apiService)
        {
            _apiService = apiService;

            _userId = _apiService.CurrentUserId;

            Name = _apiService.CurrentUserName ?? string.Empty;
            Surname = _apiService.CurrentUserSurname ?? string.Empty;
            Email = _apiService.CurrentUserEmail ?? string.Empty;
        }

        // Save original values before editing.
        public void BeginEditing()
        {
            _originalName = Name;
            _originalSurname = Surname;
            _originalEmail = Email;

            ErrorMessage = string.Empty;
            IsEditing = true;
        }

        // Validate profile information.
        public bool ValidateProfile()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "Please enter your first name.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Surname))
            {
                ErrorMessage = "Please enter your surname.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email) ||
                !Email.Contains('@') ||
                !Email.Contains('.') ||
                Email.StartsWith('@') ||
                Email.EndsWith('@'))
            {
                ErrorMessage = "Please enter a valid email address.";
                return false;
            }

            return true;
        }

        // Restore original values when cancelling.
        public void CancelEditing()
        {
            Name = _originalName;
            Surname = _originalSurname;
            Email = _originalEmail;

            ErrorMessage = string.Empty;
            IsEditing = false;
        }

        // Update the student profile through the backend.
        public async Task<bool> PrepareProfileUpdateAsync()
        {
            if (!ValidateProfile())
                return false;

            if (_userId == null || _userId <= 0)
            {
                ErrorMessage = "Please log in again.";
                return false;
            }

            try
            {
                await _apiService.UpdateStudentProfileAsync(
                    Name.Trim(),
                    Surname.Trim(),
                    Email.Trim());

                // Update local values after successful backend response.
                Name = Name.Trim();
                Surname = Surname.Trim();
                Email = Email.Trim();

                IsEditing = false;
                ErrorMessage = string.Empty;

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Profile update failed: {ex.Message}";
                return false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
