using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using Microsoft.Win32;

using TAlex.GameOfLife.Engine;
using TAlex.GameOfLife.FileFormats;
using TAlex.GameOfLife.Helpers;
using TAlex.GameOfLife.Controls;
using TAlex.Common.Extensions;
using System.Deployment.Application;
using TAlex.GameOfLife.Views;
using System.IO;
using System.Reflection;
using TAlex.Common.Models;


namespace TAlex.GameOfLife.Views
{
    public partial class MainWindow : Window
    {
        #region Fields

        private const string NumericFormat = "N0";

        private const string DefaultFilePath = "Untitled";

        private bool _patternChanged = false;

        private LifePatternFileFormat _currentOpenedFileFormat;

        private string _currentFilePath = DefaultFilePath;

        private string _patternsDirPath;

        public static AssemblyInfo AssemblyInfo = Assembly.GetExecutingAssembly().GetAssemblyInfo();

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();

            SetTitle(_currentFilePath);
            aboutMenuItem.Header = "_About " + AssemblyInfo.Title;

            _patternsDirPath = GetPatternsDirectory();
        }

        #endregion

        #region Methods

        private void LoadSettings()
        {
            Properties.Settings settings = Properties.Settings.Default;

            WindowState = (settings.WindowState == System.Windows.WindowState.Minimized) ? System.Windows.WindowState.Normal : settings.WindowState;

            Rect windowBounds = settings.WindowBounds;
            if (!(windowBounds.Width == 0 || windowBounds.Height == 0))
            {
                WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                Left = windowBounds.Left;
                Top = windowBounds.Top;
                Width = windowBounds.Width;
                Height = windowBounds.Height;
            }

            gameField.GridlinesColor = settings.GridlinesColor;
            gameField.AlternatingGridlinesColor = settings.AlternatingGridlinesColor;
            gameField.BackColor = settings.BackColor;
            gameField.SelectionColor = settings.SelectionColor;
            gameField.ShowGridlines = settings.ShowGrid;
            gameField.SetStatesCellColors(settings.StatesCellColors);
            gameField.PasteMode = (GameField.GameFieldPasteMode)Enum.Parse(typeof(GameField.GameFieldPasteMode), settings.PasteMode, true);
            gameField.UpdateInterval = settings.UpdateInterval;
        }

        private void SaveSettings()
        {
            Properties.Settings settings = Properties.Settings.Default;

            settings.ShowGrid = gameField.ShowGridlines;
            settings.UpdateInterval = gameField.UpdateInterval;
            settings.PasteMode = gameField.PasteMode.ToString();

            settings.WindowState = WindowState;
            settings.WindowBounds = RestoreBounds;

            settings.Save();
        }

        private void NewPattern()
        {
            gameField.Stop();

            if (SaveAsBeforeAction() == MessageBoxResult.Cancel)
                return;

            gameField.DeselectAll();
            gameField.CursorMode = TAlex.GameOfLife.Controls.GameField.GameFieldCursorMode.Draw;

            gameField.Clear();

            _currentFilePath = DefaultFilePath;
            _patternChanged = false;
            SetTitle(_currentFilePath);
        }

        private async void LoadPattern(string path)
        {
            gameField.Stop();

            try
            {
                using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    LifePattern pattern = await LifePatternFileFormatManager.LoadPatternFromStreamAsync(file, System.IO.Path.GetExtension(path));
                    _currentOpenedFileFormat = pattern.Format;

                    ruleComboBox.Text = pattern.Rule.ToString();

                    gameField.Initialize(pattern.Cells);
                }

                _currentFilePath = path;
                _patternChanged = false;
                SetTitle(path);
            }
            catch (UnauthorizedAccessException exc)
            {
                MessageBox.Show(this, exc.Message, AssemblyInfo.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                MessageBox.Show(this, "Can't load the pattern. File is corrupted.",
                    AssemblyInfo.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SavePattern(string path, int filterIndex)
        {
            _currentOpenedFileFormat = LifePatternFileFormatManager.GetPatternFileFormatFromFilterIndex(filterIndex);
            SavePattern(path, _currentOpenedFileFormat);
        }

        private async void SavePattern(string path, LifePatternFileFormat format)
        {
            LifePattern pattern = new LifePattern();
            pattern.Name = System.IO.Path.GetFileNameWithoutExtension(path);
            pattern.Cells = gameField.Cells;
            pattern.Rule = gameField.Rule;

            if (format.CanSave)
            {
                try
                {
                    using (FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write))
                    {
                        await format.SavePatternAsync(pattern, file);
                    }
                }
                catch (UnauthorizedAccessException exc)
                {
                    MessageBox.Show(this, exc.Message, AssemblyInfo.Title,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show(this, "This format does not support saving.",
                    AssemblyInfo.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _currentFilePath = path;
            _patternChanged = false;
            SetTitle(path);
        }

        private void Initialize()
        {
            gameField.Stop();

            try
            {
                gameField.Initialize();
            }
            catch (FormatException)
            {
                MessageBox.Show(this, "Invalid data. Please enter the correct information.",
                    AssemblyInfo.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void ShowPreferencesDialog()
        {
            PreferencesWindow window = new PreferencesWindow();
            window.Owner = this;

            SaveSettings();
            if (window.ShowDialog() == true)
            {
                LoadSettings();
            }
        }

        private string GetPatternsDirectory()
        {
            string basePath = null;
            try
            {
                basePath = ApplicationDeployment.CurrentDeployment.DataDirectory;
            }
            catch (InvalidDeploymentException)
            {
                basePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            }
            return System.IO.Path.Combine(basePath, "Patterns");
        }

        #region Event Handlers

        #region Command Binding

        private void newCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NewPattern();
        }

        private void openCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (SaveAsBeforeAction() == MessageBoxResult.Cancel)
                return;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = _patternsDirPath;
            ofd.Filter = LifePatternFileFormatManager.OpenFilter;
            ofd.FilterIndex = -1;

            if (ofd.ShowDialog() == true)
            {
                gameField.DeselectAll();
                gameField.CursorMode = TAlex.GameOfLife.Controls.GameField.GameFieldCursorMode.Move;
                LoadPattern(ofd.FileName);
            }
        }

        private void saveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_currentFilePath == null)
            {
                return;
            }

            if (_currentFilePath == DefaultFilePath)
            {
                ApplicationCommands.SaveAs.Execute(null, null);
            }
            else if (!String.IsNullOrEmpty(_currentFilePath))
            {
                SavePattern(_currentFilePath, _currentOpenedFileFormat);
            }
        }

        private void saveAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveAs();
        }


        private void drawCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GameField.DrawCommand.Execute(null, gameField);
        }

        private void moveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GameField.MoveCommand.Execute(null, gameField);
        }

        private void selectCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GameField.SelectCommand.Execute(null, gameField);
        }


        private void startCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GameField.StartCommand.Execute(null, gameField);
        }

        private void startCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GameField.StartCommand.CanExecute(null, gameField);
        }

        private void stopCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GameField.StopCommand.Execute(null, gameField);
        }

        private void stopCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GameField.StopCommand.CanExecute(null, gameField);
        }

        private void nextGenerationCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GameField.NextGenerationCommand.Execute(null, gameField);
        }

        private void nextGenerationCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GameField.NextGenerationCommand.CanExecute(null, gameField);
        }

        private void resetCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GameField.ResetCommand.Execute(null, gameField);
        }

        private void resetCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GameField.ResetCommand.CanExecute(null, gameField);
        }


        private void undoCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ApplicationCommands.Undo.CanExecute(null, gameField);
        }

        private void undoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ApplicationCommands.Undo.Execute(null, gameField);
        }

        private void redoCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ApplicationCommands.Redo.CanExecute(null, gameField);
        }

        private void redoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ApplicationCommands.Redo.Execute(null, gameField);
        }


        private void selectionNeededCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ApplicationCommands.Copy.CanExecute(null, gameField);
        }


        private void cutCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ApplicationCommands.Cut.Execute(null, gameField);
        }

        private void copyCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ApplicationCommands.Copy.Execute(null, gameField);
        }

        private void pasteCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ApplicationCommands.Paste.CanExecute(null, gameField);
        }

        private void pasteCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ApplicationCommands.Paste.Execute(null, gameField);
        }

        private void deleteCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ApplicationCommands.Delete.Execute(null, gameField);
        }

        private void selectAllCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ApplicationCommands.SelectAll.CanExecute(null, gameField);
        }

        private void selectAllCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ApplicationCommands.SelectAll.Execute(null, gameField);
        }

        private void deselectAllCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GameField.DeselectAllCommand.Execute(null, gameField);
        }


        private void rotate180CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GameField.Rotate180Command.Execute(null, gameField);
        }

        private void rotate90CWCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GameField.Rotate90CWCommand.Execute(null, gameField);
        }

        private void rotate90CCWCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GameField.Rotate90CCWCommand.Execute(null, gameField);
        }

        private void flipHorizontalCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GameField.FlipHorizontalCommand.Execute(null, gameField);
        }

        private void flipVerticalCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GameField.FlipVerticalCommand.Execute(null, gameField);
        }

        #endregion

        #region Window

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
            
            string[] args = Environment.GetCommandLineArgs();
            
            var activationData = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData;
            if (args.Length > 1 || activationData != null)
            {
                LoadPattern(args.Length > 1 ? args[1] : activationData[0]);
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (SaveAsBeforeAction() == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            SaveSettings();
        }

        #endregion

        #region Menu

        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void preferencesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowPreferencesDialog();
        }

        private void homepageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Properties.Resources.HomepageUrl);
        }

        private void abountMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Window window = new AboutWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        #endregion

        #region Others

        private void scaleFactorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int scaleFactor = gameField.ScaleFactor;

            if (scaleFactor >= 0)
            {
                scaleFactorRun.Text = String.Format(CultureInfo.InvariantCulture, "1:{0:F0}", gameField.Scale);
                expRun.Text = String.Empty;
            }
            else
            {
                scaleFactorRun.Text = "1:2";
                expRun.Text = scaleFactor.ToString("F0", CultureInfo.InvariantCulture);
            }
        }

        private void ruleComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = ruleComboBox.Text;

            TextBox textBox = e.OriginalSource as TextBox;
            int selectionLenght = textBox.SelectionLength;
            int selectionStart = textBox.SelectionStart;

            if (!String.IsNullOrEmpty(text))
            {
                LifeRule rule;

                if (LifeRule.TryParse(text, out rule))
                {
                    gameField.Rule = rule;
                    ruleComboBox.Tag = text;
                }
                else
                {
                    ruleComboBox.Text = (string)ruleComboBox.Tag;
                    textBox.SelectionStart = (selectionStart == 0) ? 0 : (selectionStart - 1);
                    textBox.SelectionLength = selectionLenght;
                }
            }
        }

        private void ruleComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            gameField.Focus();
        }

        private void gameField_MouseMove(object sender, MouseEventArgs e)
        {
            if (xCoordLabelStatusBarItem.Visibility == Visibility.Hidden)
            {
                xCoordLabelStatusBarItem.Visibility = Visibility.Visible;
                xCoordStatusBarItem.Visibility = Visibility.Visible;
                yCoordLabelStatusBarItem.Visibility = Visibility.Visible;
                yCoordStatusBarItem.Visibility = Visibility.Visible;
            }

            Point point = e.GetPosition((IInputElement)sender);
            int x, y;

            gameField.PointToCoordinate(point, out x, out y);

            xCoordStatusBarItem.Content = x.ToString(NumericFormat, NumberFormatInfo.InvariantInfo);
            yCoordStatusBarItem.Content = y.ToString(NumericFormat, NumberFormatInfo.InvariantInfo);
        }

        private void gameField_MouseLeave(object sender, MouseEventArgs e)
        {
            xCoordLabelStatusBarItem.Visibility = Visibility.Hidden;
            xCoordStatusBarItem.Visibility = Visibility.Hidden;
            yCoordLabelStatusBarItem.Visibility = Visibility.Hidden;
            yCoordStatusBarItem.Visibility = Visibility.Hidden;
        }

        private void gameField_PatternChanged(object sender, RoutedEventArgs e)
        {
            PatternChanged();
        }

        private void gameFieldContextMenu_Loaded(object sender, RoutedEventArgs e)
        {
            gameField.Focus();
        }


        private void copyToMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Helpers.Snapshot.ToClipboard(gameField);
        }

        private void saveImageAsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Helpers.Snapshot.ToFile(gameField, this);
        }


        private void cursorModeMenuItem_Loaded(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string tag = item.Tag as String;

            item.IsChecked = (tag == gameField.CursorMode.ToString());
        }

        private void pasteModeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string tag = item.Tag as String;

            var mode = (GameField.GameFieldPasteMode)Enum.Parse(typeof(GameField.GameFieldPasteMode), tag);
            gameField.PasteMode = mode;
        }

        private void pasteModeMenuItem_Loaded(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string tag = item.Tag as String;

            item.IsChecked = (tag == gameField.PasteMode.ToString());
        }

        #endregion

        #endregion

        #region Helpers

        private void SetTitle(string path)
        {
            string filename = System.IO.Path.GetFileName(path);

            if (_patternChanged)
                Title = String.Format("{0}* - {1}", filename, AssemblyInfo.Title);
            else
                Title = String.Format("{0} - {1}", filename, AssemblyInfo.Title);
        }

        private bool? SaveAs()
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.InitialDirectory = _patternsDirPath;
            sfd.FileName = System.IO.Path.GetFileName(_currentFilePath);
            sfd.DefaultExt = LifePatternFileFormatManager.DefaultExt;
            sfd.Filter = LifePatternFileFormatManager.SaveFilter;
            sfd.FilterIndex = LifePatternFileFormatManager.GetSaveFilterIndex(_currentOpenedFileFormat);

            bool? result = sfd.ShowDialog(this);

            if (result == true)
            {
                SavePattern(sfd.FileName, sfd.FilterIndex);
            }

            return result;
        }

        private MessageBoxResult SaveAsBeforeAction()
        {
            if (_patternChanged)
            {
                MessageBoxResult result = MessageBox.Show(this,
                    String.Format("Do you want to save changes to {0} before the action?", System.IO.Path.GetFileName(_currentFilePath)),
                    AssemblyInfo.Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    bool? saveAsResult = SaveAs();
                    if (saveAsResult != true) result = MessageBoxResult.Cancel;
                }

                return result;
            }

            return MessageBoxResult.None;
        }

        private void PatternChanged()
        {
            if (!_patternChanged)
            {
                _patternChanged = true;
                SetTitle(_currentFilePath);
            }
        }

        #endregion

        #endregion
    }
}