using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Media.Animation;


namespace TAlex.GameOfLife.Views
{
    /// <summary>
    /// Interaction logic for WindowAbout.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        #region Fields

        private const string propertyNameTitle = "Title";

        private const string propertyNameDescription = "Description";

        private const string propertyNameCompany = "Company";

        private const string propertyNameProduct = "Product";

        private const string propertyNameCopyright = "Copyright";

        #endregion

        #region Properties

        /// <summary>
        /// Gets the application's title.
        /// </summary>
        public static string ProductTitle
        {
            get
            {
                return GetAssemblyProperty<AssemblyTitleAttribute>(propertyNameTitle);
            }
        }

        /// <summary>
        /// Gets the application's description.
        /// </summary>
        public static string Description
        {
            get
            {
                return GetAssemblyProperty<AssemblyDescriptionAttribute>(propertyNameDescription);
            }
        }

        /// <summary>
        /// Gets the application's company.
        /// </summary>
        public static string Company
        {
            get
            {
                return GetAssemblyProperty<AssemblyCompanyAttribute>(propertyNameCompany);
            }
        }

        /// <summary>
        /// Gets the application's product.
        /// </summary>
        public static string Product
        {
            get
            {
                return GetAssemblyProperty<AssemblyProductAttribute>(propertyNameProduct);
            }
        }

        /// <summary>
        /// Gets the application's copyright.
        /// </summary>
        public static string Copyright
        {
            get
            {
                return String.Format("{0}. All rights reserved.",
                    GetAssemblyProperty<AssemblyCopyrightAttribute>(propertyNameCopyright));
            }
        }

        /// <summary>
        /// Gets the application's version.
        /// </summary>
        public static Version Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Easter egg.
            if (e.Key == Key.L && licTextLabel.Opacity == 1.0)
            {
                DoubleAnimationUsingKeyFrames licenseInfoOpacityAnim = new DoubleAnimationUsingKeyFrames();
                licenseInfoOpacityAnim.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))));
                licenseInfoOpacityAnim.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(5000))));
                licenseInfoOpacityAnim.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(42000))));
                licenseInfoOpacityAnim.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(47000))));
                licTextLabel.BeginAnimation(UIElement.OpacityProperty, licenseInfoOpacityAnim);


                DoubleAnimationUsingKeyFrames dedicatedOpacityAnim = new DoubleAnimationUsingKeyFrames();
                dedicatedOpacityAnim.BeginTime = TimeSpan.FromMilliseconds(7000);
                dedicatedOpacityAnim.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))));
                dedicatedOpacityAnim.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(3000))));
                dedicatedOpacityAnim.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(6000))));
                dedicatedLabel.BeginAnimation(UIElement.OpacityProperty, dedicatedOpacityAnim);


                DoubleAnimationUsingKeyFrames LoveOpacityAnim = new DoubleAnimationUsingKeyFrames();
                LoveOpacityAnim.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))));
                LoveOpacityAnim.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(3000))));
                LoveOpacityAnim.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(14000))));
                LoveOpacityAnim.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(17000))));

                // Show "I" word
                LoveOpacityAnim.BeginTime = TimeSpan.FromMilliseconds(14000);
                ILabel.BeginAnimation(UIElement.OpacityProperty, LoveOpacityAnim);

                // Show "Love" word
                LoveOpacityAnim.BeginTime = TimeSpan.FromMilliseconds(16000);
                LoveLabel.BeginAnimation(UIElement.OpacityProperty, LoveOpacityAnim);

                // Show "You" word
                LoveOpacityAnim.BeginTime = TimeSpan.FromMilliseconds(18000);
                YouLabel.BeginAnimation(UIElement.OpacityProperty, LoveOpacityAnim);

                // Show "Ksenia" word
                LoveOpacityAnim.BeginTime = TimeSpan.FromMilliseconds(21000);
                KseniaLabel.BeginAnimation(UIElement.OpacityProperty, LoveOpacityAnim);

                // Show "Savitskaya" word
                LoveOpacityAnim.BeginTime = TimeSpan.FromMilliseconds(23000);
                SavitskayaLabel.BeginAnimation(UIElement.OpacityProperty, LoveOpacityAnim);
            }
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

        private static string GetAssemblyProperty<T>(string propertyName)
        {
            string result = String.Empty;

            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(T), false);

            if (attributes.Length > 0)
            {
                T attribute = (T)attributes[0];
                PropertyInfo property = attribute.GetType().GetProperty(propertyName);

                if (property != null)
                {
                    result = property.GetValue(attributes[0], null) as string;
                }
            }

            return result;
        }

        #endregion
    }
}
