namespace PocketPay.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(
            nameof(RegisterPage),
            typeof(RegisterPage));

        Routing.RegisterRoute(
            nameof(HomePage),
            typeof(HomePage));

        Routing.RegisterRoute(
            nameof(DepositPage),
            typeof(DepositPage));

        Routing.RegisterRoute(
            nameof(SendMoneyPage),
            typeof(SendMoneyPage));

        Routing.RegisterRoute(
            nameof(TransactionsPage),
            typeof(TransactionsPage));

        Routing.RegisterRoute(
            nameof(AccountPage),
            typeof(AccountPage));
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