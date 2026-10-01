using UniConnect.MAUI.ViewModels;

namespace UniConnect.MAUI.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly LoginViewModel _viewModel;

        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            _viewModel.Email = EmailEntry.Text;
            _viewModel.Password = PasswordEntry.Text;

            bool success = await _viewModel.LoginAsync();

            if (success)
                await Shell.Current.GoToAsync("//DashboardPage");
        }

        private void OnTogglePasswordVisibility(object sender, EventArgs e)
        {
            PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
            ShowPasswordLabel.Text = PasswordEntry.IsPassword ? "Show" : "Hide";
        }

        private async void OnForgotPasswordTapped(object sender, EventArgs e)
        {
            await DisplayAlert("Forgot Password", "Password reset isn't implemented yet.", "OK");
        }

        private async void OnSignUpTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(SignUpPage));
        }
    }
}
