namespace Snap
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            // Auto-redirigir al login si de alguna manera el usuario llega aquí
            Dispatcher.Dispatch(async () => {
                await Shell.Current.GoToAsync("//Login");
            });
        }
    }

}
