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

        HideMenu();
    }

    public void ShowMenu()
    {
        FlyoutBehavior = FlyoutBehavior.Flyout;

        
        LoginItem.IsVisible = false;
    }

    public void HideMenu()
    {
        FlyoutBehavior = FlyoutBehavior.Disabled;


        LoginItem.IsVisible = true;
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert(
            "Logout",
            "Are you sure you want to logout?",
            "OK",
            "Cancel");

        if (!answer)
            return;

        
        SecureStorage.Default.Remove("accessToken");
        SecureStorage.Default.Remove("refreshToken");
        SecureStorage.Default.Remove("userId");
        SecureStorage.Default.Remove("fullName");
        SecureStorage.Default.Remove("email");

        
        HideMenu();

        
        await Shell.Current.GoToAsync("//MainPage");
    }
}