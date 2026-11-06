using Engine.Services;
using Engine.Services.Implements;
using Engine.Services.Interfaces;
using System.Windows;
using UI.Services;
using UI.Windows;

namespace UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // 1. Create the transparent screen-area overlay
            var overlay = new OverlayWindow();

            // Load persistent configuration (MR position & size, Sub-regions, intervals, grids, heal settings)
            var config = ConfigService.Load();
            if (config != null)
            {
                ConfigService.ApplyToOverlay(config, overlay);
            }

            overlay.Show();

            IScreenScanner scanner = new ScreenScanner();

            // 2. Create the floating toolbar (holds a reference to the overlay)
            var toolbar = new ToolbarWindow(overlay, scanner);
            toolbar.Show();
        }
    }
}
