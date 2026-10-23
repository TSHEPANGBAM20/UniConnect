namespace UniConnect.MAUI.Services
{
  

  //handles navigation on dash bottom nav
    public static class TabNavigator
    {
        public static async Task GoToTabAsync(Page page, string tab, string current)
        {
            if (tab == current) return;

            switch (tab)
            {
                case "Home":
                    await Shell.Current.GoToAsync("//DashboardPage");
                    break;
                case "Projects":
                    await Shell.Current.GoToAsync("//ProjectsPage");
                    break;
                case "Calendar":
                    await Shell.Current.GoToAsync(nameof(Views.CalendarPage)); // pushed route (AppShell.xaml.cs)
                    break;
                default:
                    await page.DisplayAlertAsync("Coming soon", $"{tab} isn't built yet.", "OK");
                    break;
            }
        }

        public static async Task HandleMenuItemAsync(Page page, string item)
        {
            if (item == "Logout")
            {
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            if (item == "MyAccount")
            {
                await Shell.Current.GoToAsync("//MyAccountPage");
                return;
            }
            await page.DisplayAlertAsync("Coming soon", $"{item} isn't built yet.", "OK");
        }
    }
}
