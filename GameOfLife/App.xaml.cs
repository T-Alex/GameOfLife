using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace TAlex.GameOfLife
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Constructors

        public App()
        {
            DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        #endregion

        #region Methods

        protected override void OnNavigating(NavigatingCancelEventArgs e)
        {
            base.OnNavigating(e);
            TAlex.WPFThemes.Twilight.TwilightThemeManager.ApplyTheme("Blue");
        }
        
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ProcessingUnhandledException(e.Exception);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ProcessingUnhandledException(e.ExceptionObject as Exception);
        }

        private void ProcessingUnhandledException(Exception e)
        {
            Trace.TraceError(e.ToString());
        }

        #endregion
    }
}
