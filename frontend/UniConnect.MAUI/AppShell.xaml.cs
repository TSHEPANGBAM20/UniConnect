using UniConnect.MAUI.Views;

namespace UniConnect.MAUI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Account pages
            Routing.RegisterRoute(nameof(SignUpPage), typeof(SignUpPage));

            // Project and collaboration pages
            Routing.RegisterRoute(nameof(NewProjectPage), typeof(NewProjectPage));
            Routing.RegisterRoute(nameof(TaskBoardPage), typeof(TaskBoardPage));
            Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage));
            Routing.RegisterRoute(nameof(CalendarPage), typeof(CalendarPage));

            // Separate route for dashboard preview mode
            Routing.RegisterRoute("DashboardPreviewPage", typeof(DashboardPage));
        }
    }
}