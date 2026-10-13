namespace UniConnect.MAUI.Services
{
    // One place for what the BottomNavBar does, so every page behaves the same.
    // Home and Projects are top-level pages (absolute routes, which also clear any
    // pushed pages such as the Task Board). Calendar/Profile items aren't built yet.
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
                    await Shell.Current.GoToAsync("//CalendarPage");
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
            await page.DisplayAlertAsync("Coming soon", $"{item} isn't built yet.", "OK");
        }
    }
}
