namespace UniConnect;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register the Sign Up page so we can navigate to it.
        Routing.RegisterRoute(
            "SignUpPage",
            typeof(Views.SignUpPage));

        Routing.RegisterRoute(
            "HomePage",
            typeof(Views.HomePage));

        Routing.RegisterRoute(
            "LoginPage",
            typeof(Views.LoginPage));
    }
}
