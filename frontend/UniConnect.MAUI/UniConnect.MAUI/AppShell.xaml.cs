using UniConnect.MAUI.Views;

namespace UniConnect.MAUI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // These pages are navigated to with GoToAsync rather than shown as
            // permanent tabs, so they need explicit route registration here.
            Routing.RegisterRoute(nameof(SignUpPage), typeof(SignUpPage));
            Routing.RegisterRoute(nameof(NewProjectPage), typeof(NewProjectPage));
            Routing.RegisterRoute(nameof(TaskBoardPage), typeof(TaskBoardPage));
            Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage));
        }
    }
}
