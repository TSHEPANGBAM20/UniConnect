using UniConnect.MAUI.Models;
using UniConnect.MAUI.ViewModels;

namespace UniConnect.MAUI.Views
{
    public partial class ProjectDetailsPage : ContentPage
    {
        private readonly ProjectDetailsViewModel _viewModel;

        public ProjectDetailsPage(ProjectDetailsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAsync();
        }

        private async void OnBackTapped(object? sender, TappedEventArgs e)
            => await Shell.Current.GoToAsync("..");

        private async void OnCopyCodeClicked(object? sender, EventArgs e)
        {
            await Clipboard.Default.SetTextAsync(_viewModel.CodeText);
            await this.DisplayAlertAsync("Copied", "The project code was copied. Share it with your teammates.", "OK");
        }

        // Any member can upload. FilePicker shows the system picker, which also lists Photos / Gallery.
        private async void OnUploadFileClicked(object? sender, EventArgs e)
        {
            FileResult? picked = await FilePicker.Default.PickAsync(
                new PickOptions { PickerTitle = "Choose a project file or picture" });
            if (picked == null) return; // cancelled

            string? error = await _viewModel.UploadFileAsync(picked);
            if (error != null)
                await this.DisplayAlertAsync("Couldn't upload file", error, "OK");
        }

        // Tap a file row: download it, then open it with the phone's viewer.
        private async void OnFileTapped(object? sender, TappedEventArgs e)
        {
            if (sender is not BindableObject { BindingContext: ProjectFileModel file }) return;

            var (path, error) = await _viewModel.DownloadFileAsync(file);
            if (path == null)
            {
                await this.DisplayAlertAsync("Couldn't open file", error ?? "Something went wrong.", "OK");
                return;
            }
            await Launcher.Default.OpenAsync(new OpenFileRequest(file.FileName, new ReadOnlyFile(path)));
        }

        // Uploader or Team Lead only (the button is hidden for everyone else)
        private async void OnDeleteFileClicked(object? sender, EventArgs e)
        {
            if (sender is not Button { BindingContext: ProjectFileModel file }) return;

            bool confirm = await this.DisplayAlertAsync("Delete file",
                $"Delete \"{file.FileName}\" for everyone in this project?", "Delete", "Cancel");
            if (!confirm) return;

            string? error = await _viewModel.DeleteFileAsync(file);
            if (error != null)
                await this.DisplayAlertAsync("Couldn't delete file", error, "OK");
        }

        // Team Lead only (the section is hidden for members)
        private async void OnAddMemberClicked(object? sender, EventArgs e)
        {
            string? error = await _viewModel.AddMemberAsync(AddEmailEntry.Text);
            if (error == null)
                AddEmailEntry.Text = string.Empty;
            else
                await this.DisplayAlertAsync("Couldn't add member", error, "OK");
        }

        // Team Lead only (the button is hidden for members and for the lead's own row)
        private async void OnRemoveMemberClicked(object? sender, EventArgs e)
        {
            if (sender is not Button { BindingContext: ProjectMemberModel member }) return;

            bool confirm = await this.DisplayAlertAsync("Remove member",
                $"Remove {member.FullName} from this project?", "Remove", "Cancel");
            if (!confirm) return;

            string? error = await _viewModel.RemoveMemberAsync(member);
            if (error != null)
                await this.DisplayAlertAsync("Couldn't remove member", error, "OK");
        }

        // Team Lead only
        private async void OnDisbandClicked(object? sender, EventArgs e)
        {
            bool confirm = await this.DisplayAlertAsync("Disband project",
                $"Permanently delete \"{_viewModel.ProjectName}\" for everyone? This can't be undone.",
                "Disband", "Cancel");
            if (!confirm) return;

            string? error = await _viewModel.DisbandAsync();
            if (error == null)
                await Shell.Current.GoToAsync("//ProjectsPage");
            else
                await this.DisplayAlertAsync("Couldn't disband project", error, "OK");
        }

        // Team Members only
        private async void OnLeaveClicked(object? sender, EventArgs e)
        {
            bool confirm = await this.DisplayAlertAsync("Leave project",
                $"Leave \"{_viewModel.ProjectName}\"?", "Leave", "Cancel");
            if (!confirm) return;

            string? error = await _viewModel.LeaveAsync();
            if (error == null)
                await Shell.Current.GoToAsync("//ProjectsPage");
            else
                await this.DisplayAlertAsync("Couldn't leave project", error, "OK");
        }
    }
}
