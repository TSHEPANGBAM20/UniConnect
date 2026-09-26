using UniConnect.MAUI.ViewModels;

namespace UniConnect.MAUI.Views
{
    public partial class NewProjectPage : ContentPage
    {
        private readonly NewProjectViewModel _viewModel;

        public NewProjectPage(NewProjectViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        private async void OnCreateClicked(object sender, EventArgs e)
        {
            _viewModel.ProjectName = TitleEntry.Text;
            _viewModel.ProjectDescription = DescriptionEditor.Text;

            bool success = await _viewModel.CreateProjectAsync();

            if (success)
                await Shell.Current.GoToAsync("..");
        }
    }
}
