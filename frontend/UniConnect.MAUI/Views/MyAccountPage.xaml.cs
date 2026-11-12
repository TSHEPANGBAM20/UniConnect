
using System;
using UniConnect.MAUI.ViewModels;

namespace UniConnect.MAUI.Views
{
    public partial class MyAccountPage : ContentPage
    {
        private readonly MyAccountViewModel _viewModel;

        public MyAccountPage(MyAccountViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            BindingContext = _viewModel;

            // Load the student's profile details.
            NameEntry.Text = _viewModel.Name;
            SurnameEntry.Text = _viewModel.Surname;
            EmailEntry.Text = _viewModel.Email;

            SetEditingState(false);
            UpdateDisplayName();
        }

        // Updates the displayed name and avatar initials.
        private void UpdateDisplayName()
        {
            string fullName =
                $"{_viewModel.Name} {_viewModel.Surname}".Trim();

            DisplayNameLabel.Text =
                string.IsNullOrWhiteSpace(fullName)
                    ? "Your Name"
                    : fullName;

            InitialsLabel.Text = GetInitials(fullName);
        }

        // Gets the user's initials.
        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "U";

            string[] parts = name.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
                return parts[0][0].ToString().ToUpperInvariant();

            return $"{char.ToUpperInvariant(parts[0][0])}" +
                   $"{char.ToUpperInvariant(parts[^1][0])}";
        }

        // Opens profile editing.
        private void OnEditProfileClicked(object? sender, EventArgs e)
        {
            _viewModel.BeginEditing();

            NameEntry.Text = _viewModel.Name;
            SurnameEntry.Text = _viewModel.Surname;
            EmailEntry.Text = _viewModel.Email;

            SetEditingState(true);
            NameEntry.Focus();
        }

        // Saves the updated profile.
        private async void OnSaveProfileClicked(object? sender, EventArgs e)
        {
            _viewModel.Name = NameEntry.Text?.Trim() ?? string.Empty;
            _viewModel.Surname = SurnameEntry.Text?.Trim() ?? string.Empty;
            _viewModel.Email = EmailEntry.Text?.Trim() ?? string.Empty;

            bool saved = await _viewModel.PrepareProfileUpdateAsync();

            if (!saved)
            {
                await DisplayAlertAsync(
                    "Profile not saved",
                    _viewModel.ErrorMessage,
                    "OK");

                return;
            }

            // Refresh the displayed profile after backend confirmation.
            NameEntry.Text = _viewModel.Name;
            SurnameEntry.Text = _viewModel.Surname;
            EmailEntry.Text = _viewModel.Email;

            UpdateDisplayName();
            SetEditingState(false);

            await DisplayAlertAsync(
                "Profile updated",
                "Your profile has been updated.",
                "OK");
        }

        // Restores original details when editing is cancelled.
        private void OnCancelEditClicked(object? sender, EventArgs e)
        {
            _viewModel.CancelEditing();

            NameEntry.Text = _viewModel.Name;
            SurnameEntry.Text = _viewModel.Surname;
            EmailEntry.Text = _viewModel.Email;

            UpdateDisplayName();
            SetEditingState(false);
        }

        // Controls editing permissions and button visibility.
        private void SetEditingState(bool editing)
        {
            NameEntry.IsReadOnly = !editing;
            SurnameEntry.IsReadOnly = !editing;
            EmailEntry.IsReadOnly = !editing;

            NameEntry.IsEnabled = true;
            SurnameEntry.IsEnabled = true;
            EmailEntry.IsEnabled = true;

            EditProfileButton.IsVisible = !editing;
            SaveProfileButton.IsVisible = editing;
            CancelEditButton.IsVisible = editing;
        }

        // Returns to the dashboard.
        private async void OnBackClicked(object? sender, EventArgs e)
        {
            if (_viewModel.IsEditing)
            {
                bool discard = await DisplayAlertAsync(
                    "Discard changes?",
                    "Do you want to leave without saving your changes?",
                    "Leave",
                    "Stay");

                if (!discard)
                    return;

                _viewModel.CancelEditing();
            }

            await Shell.Current.GoToAsync("//DashboardPage");
        }
    }
}
