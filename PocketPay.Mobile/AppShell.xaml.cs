namespace PocketPay.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }

    public void ShowMenu()
    {
        FlyoutBehavior = FlyoutBehavior.Flyout;
    }

    public void HideMenu()
    {
        FlyoutBehavior = FlyoutBehavior.Disabled;
    }
}