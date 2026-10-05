namespace UniConnect.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    // Opens the Login page.
    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("LoginPage");
    }

    // Opens the Sign Up page.
    private async void OnSignUpClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("SignUpPage");
    }
}
