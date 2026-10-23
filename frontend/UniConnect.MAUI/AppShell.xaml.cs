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
            Routing.RegisterRoute(nameof(MyAccountPage),typeof(MyAccountPage));

            // Project and collaboration pages
            Routing.RegisterRoute(nameof(NewProjectPage), typeof(NewProjectPage));
            Routing.RegisterRoute(nameof(TaskBoardPage), typeof(TaskBoardPage));
            Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage));
            Routing.RegisterRoute(nameof(ProjectDetailsPage), typeof(ProjectDetailsPage));
            Routing.RegisterRoute(nameof(CalendarPage), typeof(CalendarPage));
              // Task creation page.
            Routing.RegisterRoute(nameof(CreateTaskPage),typeof(CreateTaskPage));

            // Separate route for dashboard preview mode
            Routing.RegisterRoute("DashboardPreviewPage", typeof(DashboardPage));
        }
    }
}