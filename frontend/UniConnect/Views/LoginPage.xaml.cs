namespace UniConnect.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private void OnTogglePasswordVisibility(object? sender, TappedEventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;

        ShowPasswordLabel.Text =
            PasswordEntry.IsPassword ? "Show" : "Hide";
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        await DisplayAlertAsync(
            "Login",
            "Login button clicked.",
            "OK");
    }

    private async void OnForgotPasswordTapped(object? sender, TappedEventArgs e)
    {
        await DisplayAlertAsync(
            "Forgot Password",
            "Password recovery will be added later.",
            "OK");
    }

    private async void OnSignUpTapped(object? sender, TappedEventArgs e)
    {
        await DisplayAlertAsync(
            "Sign Up",
            "The Sign Up page will be added next.",
            "OK");
    }
}
