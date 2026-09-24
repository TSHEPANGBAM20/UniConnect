// This file contains the C# code-behind for the ChatPage.
// The XAML file (ChatPage.xaml) defines the visual layout,
// while this file will contain the page's behaviour and logic.

namespace UniConnect.MAUI;

// Represents the Chat page in the UniConnect application.
// This class is connected to ChatPage.xaml through the x:Class
// declaration in the XAML file.
public partial class ChatPage : ContentPage
{
    // Constructor for the ChatPage.
    // InitializeComponent() loads the user interface defined
    // in ChatPage.xaml.
    public ChatPage()
    {
        InitializeComponent();
    }
}