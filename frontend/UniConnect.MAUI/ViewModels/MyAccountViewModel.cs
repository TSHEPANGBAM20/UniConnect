
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UniConnect.MAUI.Services;

namespace UniConnect.MAUI.ViewModels
{
    public class MyAccountViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        // stores the ID of the currently logged-in student.
        // The backend team must confirm how this ID should be used
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

        // Combines the student's first name and surname for display.
        public string FullName =>
            $"{Name} {Surname}".Trim();

        // Creates profile initials from the student's name.
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

        // Stores the original values so Cancel can restore them.
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

            string[] nameParts =
                _apiService.CurrentUserName.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

            if (nameParts.Length > 0)
            {
                Name = nameParts[0];
                Surname = nameParts.Length > 1
                    ? string.Join(" ", nameParts, 1, nameParts.Length - 1)
                    : string.Empty;
            }

            Email = _apiService.CurrentUserEmail;
        }

        // Saves the current values before editing begins.
        public void BeginEditing()
        {
            _originalName = Name;
            _originalSurname = Surname;
            _originalEmail = Email;

            ErrorMessage = string.Empty;
            IsEditing = true;
        }

        // Validates the profile fields on the frontend.
       
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

        // Restores the original values when the user cancels editing.
        public void CancelEditing()
        {
            Name = _originalName;
            Surname = _originalSurname;
            Email = _originalEmail;

            ErrorMessage = string.Empty;
            IsEditing = false;
        }

        // BACKEND: Connect this method to an ApiService profile-update
        // method after the backend team confirms the endpoint.
        //
        // The update request will need the student's identity and
        // the agreed fields for name, surname and email.
        //
        // BACKEND: Update the existing student record in the database.
        // Return a response indicating whether the update succeeded.
        // The frontend should only report success after confirmation.
        //
        // IMPORTANT: This method currently validates the profile only.
        // It does not send a request or save changes to the database.
        public bool PrepareProfileUpdate()
        {
            if (!ValidateProfile())
                return false;

            if (_userId == null)
            {
                ErrorMessage =
                    "Student details are unavailable. Please log in again.";
                return false;
            }

            ErrorMessage =
                "Profile saving will be connected when the backend endpoint is ready.";

            return false;
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
