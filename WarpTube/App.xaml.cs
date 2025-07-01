namespace WarpTube;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(Handler.MauiContext!.Services.GetRequiredService<MainPage>()) { Title = "WarpTube" };
    }
}
