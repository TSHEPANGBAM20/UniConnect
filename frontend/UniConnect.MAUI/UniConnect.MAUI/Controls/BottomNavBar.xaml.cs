using ShapePath = Microsoft.Maui.Controls.Shapes.Path;

namespace UniConnect.MAUI.Controls
{
    // Floating capsule navigation bar. Reusable: drop it on any page and set ActiveTab.
    // Tapping Home/Projects/Calendar raises TabSelected.
    // Tapping Profile opens a popup menu; its items raise MenuItemSelected.
    public partial class BottomNavBar : ContentView
    {
        public static readonly BindableProperty ActiveTabProperty = BindableProperty.Create(
            nameof(ActiveTab), typeof(string), typeof(BottomNavBar), "Home",
            propertyChanged: (b, _, _) => ((BottomNavBar)b).ApplyState());

        public string ActiveTab
        {
            get => (string)GetValue(ActiveTabProperty);
            set => SetValue(ActiveTabProperty, value);
        }

        public event EventHandler<string>? TabSelected;
        public event EventHandler<string>? MenuItemSelected;

        private static readonly Color ActiveColor = Color.FromArgb("#1E3A8A");   // dark blue
        private static readonly Color ActiveBg = Color.FromArgb("#E8F1FE");      // light blue pill
        private static readonly Color InactiveColor = Color.FromArgb("#6B7280"); // gray

        private readonly Dictionary<string, (Border Tab, ShapePath Icon, Label Text)> _tabs;
        private bool _menuOpen;

        public BottomNavBar()
        {
            InitializeComponent();
            _tabs = new()
            {
                ["Home"] = (HomeTab, HomeIcon, HomeLabel),
                ["Projects"] = (ProjectsTab, ProjectsIcon, ProjectsLabel),
                ["Calendar"] = (CalendarTab, CalendarIcon, CalendarLabel),
                ["Profile"] = (ProfileTab, ProfileIcon, ProfileLabel),
            };
            ApplyState();
        }

        private void ApplyState()
        {
            if (_tabs == null) return;
            foreach (var (name, (tab, icon, text)) in _tabs)
            {
                bool active = _menuOpen ? name == "Profile" : name == ActiveTab;
                var color = active ? ActiveColor : InactiveColor;
                tab.BackgroundColor = active ? ActiveBg : Colors.Transparent;
                icon.Stroke = new SolidColorBrush(color);
                text.TextColor = color;
            }
        }

        private async void OnTabTapped(object? sender, TappedEventArgs e)
        {
            string name = ((Element)sender!).ClassId;

            if (name == "Profile")
            {
                if (_menuOpen) await CloseMenuAsync(); else await OpenMenuAsync();
                return;
            }

            if (_menuOpen) await CloseMenuAsync();
            TabSelected?.Invoke(this, name);
        }

        private async void OnMenuItemTapped(object? sender, TappedEventArgs e)
        {
            string item = ((Element)sender!).ClassId;
            await CloseMenuAsync();
            MenuItemSelected?.Invoke(this, item);
        }

        // One combined animation (fade + grow from the bottom edge + small slide)
        // instead of three separate ones: cheaper per frame and stays in sync.
        private Task AnimatePopupAsync(double from, double to, uint ms, Easing easing)
        {
            var tcs = new TaskCompletionSource();
            var anim = new Animation(v =>
            {
                MenuPopup.Opacity = v;
                MenuPopup.Scale = 0.92 + 0.08 * v;
                MenuPopup.TranslationY = 10 * (1 - v);
            }, from, to, easing);
            anim.Commit(this, "MenuPopup", 16, ms, easing, (_, _) => tcs.TrySetResult());
            return tcs.Task;
        }

        private async Task OpenMenuAsync()
        {
            _menuOpen = true;
            ApplyState();
            MenuPopup.Opacity = 0;
            MenuPopup.Scale = 0.92;
            MenuPopup.TranslationY = 10;
            MenuPopup.IsVisible = true;
            await AnimatePopupAsync(0, 1, 280, Easing.CubicOut);
        }

        private async Task CloseMenuAsync()
        {
            _menuOpen = false;
            ApplyState();
            await AnimatePopupAsync(1, 0, 200, Easing.CubicIn);
            MenuPopup.IsVisible = false;
        }
    }
}