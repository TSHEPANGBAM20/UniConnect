using UniConnect.MAUI.ViewModels;

namespace UniConnect.MAUI.Views
{
    public partial class SignUpPage : ContentPage
    {
        private readonly SignUpViewModel _viewModel;

        public SignUpPage(SignUpViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        private async void OnSignUpClicked(object sender, EventArgs e)
        {
            _viewModel.Name = NameEntry.Text;
            _viewModel.Surname = SurnameEntry.Text;
            _viewModel.Email = EmailEntry.Text;
            _viewModel.Password = PasswordEntry.Text;
            _viewModel.ConfirmPassword = ConfirmPasswordEntry.Text;

            bool success = await _viewModel.RegisterAsync();

            if (success)
                await Shell.Current.GoToAsync("..");
        }

        private async void OnLoginTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
