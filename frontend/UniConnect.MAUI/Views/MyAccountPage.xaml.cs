
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

            // Displays the profile details currently held by the ViewModel.
            // BACKEND: These values currently come from the details stored
            // after login. Load the latest saved profile from the database
            // once the backend profile GET endpoint has been confirmed.

            FullNameEntry.Text = _viewModel.FullName;
            EmailEntry.Text = _viewModel.Email;

            SetEditingState(false);
            UpdateDisplayName();
        }

        // Updates the displayed name and avatar initials.
        private void UpdateDisplayName()
        {
            string name = FullNameEntry.Text?.Trim() ?? string.Empty;

            DisplayNameLabel.Text = string.IsNullOrWhiteSpace(name)
                ? "Your Name"
                : name;

            InitialsLabel.Text = GetInitials(name);
        }

        // Gets the user's initials from their name.
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

        // Opens edit mode and stores the original values in the ViewModel.
        private void OnEditProfileClicked(object? sender, EventArgs e)
        {
            _viewModel.BeginEditing();

            FullNameEntry.Text = _viewModel.FullName;
            EmailEntry.Text = _viewModel.Email;

            SetEditingState(true);
            FullNameEntry.Focus();
        }

        // Validates the entered values and prepares the profile update.
        private async void OnSaveProfileClicked(object? sender, EventArgs e)
        {
            string fullName = FullNameEntry.Text?.Trim() ?? string.Empty;
            string email = EmailEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(fullName))
            {
                await DisplayAlertAsync(
                    "Name required",
                    "Please enter your full name.",
                    "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(email) ||
                !email.Contains('@') ||
                !email.Contains('.') ||
                email.StartsWith('@') ||
                email.EndsWith('@'))
            {
                await DisplayAlertAsync(
                    "Invalid email",
                    "Please enter a valid email address.",
                    "OK");
                return;
            }

            // Splits the full name into the ViewModel's name and surname.
            string[] nameParts = fullName.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

            _viewModel.Name = nameParts[0];
            _viewModel.Surname = nameParts.Length > 1
                ? string.Join(" ", nameParts, 1, nameParts.Length - 1)
                : string.Empty;

            _viewModel.Email = email;

            // BACKEND: Connect this Save action to the confirmed profile
            // update method in ApiService through MyAccountViewModel.
            //
            // BACKEND: Send the student's ID (or use the authenticated
            // student identity) together with the updated name, surname
            // and email, using the agreed request format.
            //
            // BACKEND: Wait for the server response. Only close edit mode
            // and display a success message after the database update
            // has been confirmed.
            //
            // IMPORTANT: PrepareProfileUpdate currently validates values
            // but does not send an API request or save to the database.

            bool saved = _viewModel.PrepareProfileUpdate();

            if (!saved)
            {
                await DisplayAlertAsync(
                    "Profile not saved",
                    _viewModel.ErrorMessage,
                    "OK");
                return;
            }

            UpdateDisplayName();
            SetEditingState(false);

            await DisplayAlertAsync(
                "Profile updated",
                "Your profile has been updated.",
                "OK");
        }

        // Restores the original values when editing is cancelled.
        private void OnCancelEditClicked(object? sender, EventArgs e)
        {
            _viewModel.CancelEditing();

            FullNameEntry.Text = _viewModel.FullName;
            EmailEntry.Text = _viewModel.Email;

            UpdateDisplayName();
            SetEditingState(false);
        }

        // Controls whether the fields and buttons are editable or visible.
        private void SetEditingState(bool editing)
        {
            FullNameEntry.IsReadOnly = !editing;
            EmailEntry.IsReadOnly = !editing;

            FullNameEntry.IsEnabled = true;
            EmailEntry.IsEnabled = true;

            EditProfileButton.IsVisible = !editing;
            SaveProfileButton.IsVisible = editing;
            CancelEditButton.IsVisible = editing;
        }

        // Returns to the previous page and checks before discarding edits.
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
