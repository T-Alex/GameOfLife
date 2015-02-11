using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Media.Animation;
using TAlex.Common.Extensions;


namespace TAlex.GameOfLife.Views
{
    /// <summary>
    /// Interaction logic for WindowAbout.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        #region Fields

        private static Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the application's title.
        /// </summary>
        public static string ProductTitle
        {
            get
            {
                return ExecutingAssembly.GetTitle();
            }
        }

        /// <summary>
        /// Gets the application's description.
        /// </summary>
        public static string Description
        {
            get
            {
                return ExecutingAssembly.GetDescription();
            }
        }

        /// <summary>
        /// Gets the application's company.
        /// </summary>
        public static string Company
        {
            get
            {
                return ExecutingAssembly.GetCompany();
            }
        }

        /// <summary>
        /// Gets the application's product.
        /// </summary>
        public static string Product
        {
            get
            {
                return ExecutingAssembly.GetProduct();
            }
        }

        /// <summary>
        /// Gets the application's copyright.
        /// </summary>
        public static string Copyright
        {
            get
            {
                return String.Format("{0}. All rights reserved.", ExecutingAssembly.GetCopyright());
            }
        }

        /// <summary>
        /// Gets the application's version.
        /// </summary>
        public static Version Version
        {
            get
            {
                return ExecutingAssembly.GetVersion();
            }
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

            Title = "About " + ProductTitle;
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
                Process.Start(new ProcessStartInfo(uri));
                e.Handled = true;
            }
        }

        #endregion
    }
}
