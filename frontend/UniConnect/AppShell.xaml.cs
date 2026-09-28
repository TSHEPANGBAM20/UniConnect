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
    }
}
