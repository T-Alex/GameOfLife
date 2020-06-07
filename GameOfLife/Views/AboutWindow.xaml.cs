using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Media.Animation;
using TAlex.Common.Extensions;
using TAlex.Common.Models;


namespace TAlex.GameOfLife.Views
{
    /// <summary>
    /// Interaction logic for WindowAbout.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        #region Properties

        /// <summary>
        /// Gets the application info.
        /// </summary>
        public static AssemblyInfo ApplicationInfo
        {
            get { return Assembly.GetExecutingAssembly().GetAssemblyInfo(); }
        }

        /// <summary>
        /// Gets the email support title for this product.
        /// </summary>
        public static string EmailTitle
        {
            get
            {
                return EmailAddress.Replace("mailto:", String.Empty);
            }
        }

        /// <summary>
        /// Gets the email support for this product.
        /// </summary>
        public static string EmailAddress
        {
            get
            {
                return Properties.Resources.SupportEmail;
            }
        }

        /// <summary>
        /// Gets the homepage title of this product.
        /// </summary>
        public static string HomepageTitle
        {
            get
            {
                return HomepageUrl.Replace("http://", String.Empty);
            }
        }

        /// <summary>
        /// Gets the homepage url of this product.
        /// </summary>
        public static string HomepageUrl
        {
            get
            {
                return Properties.Resources.HomepageUrl;
            }
        }

        #endregion

        #region Constructors

        public AboutWindow()
        {
            InitializeComponent();

            Title = "About " + ApplicationInfo.Title;
        }

        #endregion

        #region Methods

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            Close();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            if (e.Uri != null && string.IsNullOrEmpty(e.Uri.OriginalString) == false)
            {
                string uri = e.Uri.AbsoluteUri;
                Process.Start(new ProcessStartInfo
                {
                    FileName = uri,
                    UseShellExecute = true
                });

                e.Handled = true;
            }
        }

        #endregion
    }
}
