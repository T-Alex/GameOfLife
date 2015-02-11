using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Globalization;

using TAlex.WPF.Controls;
using TAlex.GameOfLife.Helpers;

namespace TAlex.GameOfLife.Views
{
    /// <summary>
    /// Interaction logic for PreferencesWindow.xaml
    /// </summary>
    public partial class PreferencesWindow : Window
    {
        #region Constructors

        public PreferencesWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        #endregion

        #region Methods

        private void LoadSettings()
        {
            Properties.Settings settings = Properties.Settings.Default;

            gridlinesColorChip.SelectedColor = settings.GridlinesColor;
            alternatingGridlinesColorChip.SelectedColor = settings.AlternatingGridlinesColor;
            backColorChip.SelectedColor = settings.BackColor;
            selectionColorChip.SelectedColor = settings.SelectionColor;

            if (settings.StatesCellColors != null && settings.StatesCellColors.Count == 18)
            {
                for (int i = 0; i < 9; i++)
                {
                    ((ColorChip)birthStatesListView.Items[i]).SelectedColor = settings.StatesCellColors[i];
                }

                for (int i = 0; i < 9; i++)
                {
                    ((ColorChip)survivalStatesListView.Items[i]).SelectedColor = settings.StatesCellColors[i + 9];
                }
            }
        }

        private void SaveSettings()
        {
            Properties.Settings settings = Properties.Settings.Default;

            settings.GridlinesColor = gridlinesColorChip.SelectedColor;
            settings.AlternatingGridlinesColor = alternatingGridlinesColorChip.SelectedColor;
            settings.BackColor = backColorChip.SelectedColor;
            settings.SelectionColor = selectionColorChip.SelectedColor;

            settings.StatesCellColors.Clear();

            foreach (ColorChip chip in birthStatesListView.Items)
            {
                settings.StatesCellColors.Add(chip.SelectedColor);
            }

            foreach (ColorChip chip in survivalStatesListView.Items)
            {
                settings.StatesCellColors.Add(chip.SelectedColor);
            }

            settings.Save();
        }

        #region Event Handlers

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            DialogResult = true;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion

        #region Helpers

        

        #endregion

        #endregion
    }
}
