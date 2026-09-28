namespace UniConnect.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    // Shows or hides the password.
    private void OnTogglePasswordVisibility(object? sender, TappedEventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;

        ShowPasswordLabel.Text =
            PasswordEntry.IsPassword ? "Show" : "Hide";
    }

    // Temporary login button test.
    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        await DisplayAlertAsync(
            "Login",
            "Login button clicked.",
            "OK");
    }

    // Temporary forgot-password message.
    private async void OnForgotPasswordTapped(
        object? sender,
        TappedEventArgs e)
    {
        await DisplayAlertAsync(
            "Forgot Password",
            "Password recovery will be added later.",
            "OK");
    }

    // Opens the Sign Up page.
    private async void OnSignUpTapped(
        object? sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("SignUpPage");
    }
}
