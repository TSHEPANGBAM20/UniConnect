namespace UniConnect.Views;

public partial class SignUpPage : ContentPage
{
    public SignUpPage()
    {
        InitializeComponent();
    }

    private void OnTogglePasswordVisibility(object? sender, TappedEventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;

        ShowPasswordLabel.Text =
            PasswordEntry.IsPassword ? "Show" : "Hide";
    }

    private async void OnSignUpClicked(object? sender, EventArgs e)
    {
        // Clear any previous error message.
        ErrorLabel.Text = "";

        // Check that all fields have been filled in.
        if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
            string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text) ||
            string.IsNullOrWhiteSpace(ConfirmPasswordEntry.Text))
        {
            ErrorLabel.Text = "Please fill in all fields.";
            return;
        }

        // Check that both passwords match.
        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            ErrorLabel.Text = "Passwords do not match.";
            return;
        }

        // Temporary message until we connect the backend.
        await DisplayAlertAsync(
            "Sign Up",
            "Your details are valid. Backend registration will be added next.",
            "OK");
    }

    private async void OnLoginTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//LoginPage"); // register
    }
}
