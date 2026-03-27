namespace WsiuEngine.Core.System
{
    internal class Application
    {
        private static Application instance = null!;
        internal static void Initialize()
        {
            instance = new();
        }

        private Application()
        {

        }

        public static void Quit()
        {
            Microsoft.UI.Xaml.Application.Current.Exit();
        }
    }
}
