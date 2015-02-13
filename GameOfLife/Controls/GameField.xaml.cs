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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading;

using TAlex.GameOfLife.Engine;
using TAlex.GameOfLife.FileFormats;
using TAlex.GameOfLife.Helpers;
using TAlex.Common.Extensions;
using TAlex.Common.Services.Commands.Undo;


namespace TAlex.GameOfLife.Controls
{
    /// <summary>
    /// Interaction logic for GameField.xaml
    /// </summary>
    public partial class GameField : UserControl
    {
        #region Fields

        private const int DefaultScaleFactor = 3;

        private const int DefaultAlternationCount = 10;

        private const int MinScaleFactorWhenShowGrid = 2;


        private bool _pasting = false;

        private ICollection<Cell> _pastingCells = null;

        private Int32Rect _pastingCellsRect = Int32Rect.Empty;

        private GameFieldPasteMode _pasteMode = GameFieldPasteMode.Or;

        private GameFieldCursorMode _cursorMode = GameFieldCursorMode.Draw;

        private Int32Rect _previousSelectedRegion = Int32Rect.Empty;

        private Int32Rect _selectedRegion = Int32Rect.Empty;

        private WriteableBitmap _wb;

        private QuickLifeEngine _game = new QuickLifeEngine();

        private int _prevGeneration;

        private IList<CellInfo> _drawingCells = new List<CellInfo>();

        private IDictionary<Cell, byte> _prevCells = new Dictionary<Cell, byte>();

        private int _scaleFactor = DefaultScaleFactor;

        private byte _currPressedCellReverseState;

        private Cell _topLeftCell = new Cell();

        private Cell _currPressedCell = new Cell();

        private Point _currPressedPoint;

        private int[] _statesCellColors = new int[256];

        private int _sel_c;
        private double _sel_c_transp;
        private double _sel_c_r_o;
        private double _sel_c_g_o;
        private double _sel_c_b_o;

        private int _undoUnitCountForReset = 0;

        private GameFieldMemento _mementoForReset = null;

        private UndoManager _undoManager = new UndoManager();

        private DispatcherTimer _updateTimer = new DispatcherTimer();


        private static readonly DependencyPropertyKey GenerationPropertyKey;
        public static readonly DependencyProperty GenerationProperty;

        private static readonly DependencyPropertyKey PopulationPropertyKey;
        public static readonly DependencyProperty PopulationProperty;

        public static readonly DependencyProperty ScaleFactorProperty;
        public static readonly DependencyProperty ShowGridlinesProperty;
        public static readonly DependencyProperty BackColorProperty;
        public static readonly DependencyProperty GridlinesColorProperty;
        public static readonly DependencyProperty AlternatingGridlinesColorProperty;
        public static readonly DependencyProperty CellColorProperty;
        public static readonly DependencyProperty SelectionColorProperty;
        public static readonly DependencyProperty UpdateIntervalProperty;

        public static RoutedUICommand DrawCommand;
        public static RoutedUICommand MoveCommand;
        public static RoutedUICommand SelectCommand;

        public static RoutedUICommand StartCommand;
        public static RoutedUICommand StopCommand;
        public static RoutedUICommand NextGenerationCommand;
        public static RoutedUICommand ResetCommand;

        public static RoutedUICommand DeselectAllCommand;
        public static RoutedUICommand Rotate180Command;

        public static RoutedUICommand Rotate90CWCommand;
        public static RoutedUICommand Rotate90CCWCommand;
        public static RoutedUICommand FlipHorizontalCommand;
        public static RoutedUICommand FlipVerticalCommand;


        public static RoutedUICommand FitPatternCommand;
        public static RoutedUICommand FitSelectionCommand;
        public static RoutedUICommand CenteringPatternCommand;

        public static RoutedEvent PatternChangedEvent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value that indicates whether to show grid lines.
        /// </summary>
        public bool ShowGridlines
        {
            get
            {
                return (bool)base.GetValue(ShowGridlinesProperty);
            }

            set
            {
                base.SetValue(ShowGridlinesProperty, value);
            }
        }

        public Color BackColor
        {
            get
            {
                return (Color)base.GetValue(BackColorProperty);
            }

            set
            {
                base.SetValue(BackColorProperty, value);
            }
        }

        public Color GridlinesColor
        {
            get
            {
                return (Color)base.GetValue(GridlinesColorProperty);
            }

            set
            {
                base.SetValue(GridlinesColorProperty, value);
            }
        }

        public Color AlternatingGridlinesColor
        {
            get
            {
                return (Color)base.GetValue(AlternatingGridlinesColorProperty);
            }

            set
            {
                base.SetValue(AlternatingGridlinesColorProperty, value);
            }
        }

        public Color CellColor
        {
            get
            {
                return (Color)base.GetValue(CellColorProperty);
            }

            set
            {
                base.SetValue(CellColorProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the cell color of the current cell selection.
        /// </summary>
        public Color SelectionColor
        {
            get
            {
                return (Color)base.GetValue(SelectionColorProperty);
            }

            set
            {
                base.SetValue(SelectionColorProperty, value);
            }
        }

        public int ScaleFactor
        {
            get
            {
                return (int)GetValue(ScaleFactorProperty);
            }

            set
            {
                SetValue(ScaleFactorProperty, value);
            }
        }

        public double Scale
        {
            get
            {
                return GetScale(_scaleFactor);
            }
        }

        public int MinScaleFactor
        {
            get
            {
                return -10;
            }
        }

        public int MaxScaleFactor
        {
            get
            {
                return 4;
            }
        }

        public GameFieldCursorMode CursorMode
        {
            get
            {
                return _cursorMode;
            }

            set
            {
                SetCursorMode(value, true);
            }
        }

        public GameFieldPasteMode PasteMode
        {
            get
            {
                return _pasteMode;
            }

            set
            {
                _pasteMode = value;
            }
        }

        /// <summary>
        /// Gets the current population size.
        /// </summary>
        public int Population
        {
            get
            {
                return (int)GetValue(PopulationProperty);
            }

            protected set
            {
                SetValue(PopulationPropertyKey, value);
            }
        }

        /// <summary>
        /// Gets the current generation number.
        /// </summary>
        public int Generation
        {
            get
            {
                return (int)GetValue(GenerationProperty);
            }

            protected set
            {
                SetValue(GenerationPropertyKey, value);
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the size of the population equals to zero.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return _game.IsEmpty;
            }
        }

        /// <summary>
        /// Gets or sets the interval generating in miliseconds of new generations.
        /// </summary>
        public int UpdateInterval
        {
            get
            {
                return (int)GetValue(UpdateIntervalProperty);
            }

            set
            {
                SetValue(UpdateIntervalProperty, value);
            }
        }

        public Dictionary<Cell, byte> Cells
        {
            get
            {
                return _game.Cells;
            }
        }

        public int FieldWidth
        {
            get
            {
                return (int)(ActualWidth / Scale);
            }
        }

        public int FieldHeight
        {
            get
            {
                return (int)(ActualHeight / Scale);
            }
        }

        public LifeRule Rule
        {
            get
            {
                return _game.Rule;
            }

            set
            {
                _game.Rule = value;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the most recent action can be undone.
        /// </summary>
        public bool CanUndo
        {
            get
            {
                if (_updateTimer.IsEnabled)
                    return false;
                else
                    return _undoManager.CanUndo;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the most recent undo action can be redone.
        /// </summary>
        public bool CanRedo
        {
            get
            {
                if (_updateTimer.IsEnabled)
                    return false;
                else
                    return _undoManager.CanRedo;
            }
        }

        /// <summary>
        /// Gets or sets the number of actions stored in the undo queue.
        /// </summary>
        public int UndoLimit
        {
            get
            {
                return _undoManager.UndoLimit;
            }

            set
            {
                _undoManager.UndoLimit = value;
            }
        }

        #endregion

        #region Events

        public event RoutedEventHandler PatternChanged
        {
            add { AddHandler(PatternChangedEvent, value); }
            remove { RemoveHandler(PatternChangedEvent, value); }
        }

        #endregion

        #region Constructors

        static GameField()
        {
            InitializeCommands();
            InitializeEvents();

            GenerationPropertyKey = DependencyProperty.RegisterReadOnly("Generation", typeof(int), typeof(GameField), new PropertyMetadata());
            GenerationProperty = GenerationPropertyKey.DependencyProperty;

            PopulationPropertyKey = DependencyProperty.RegisterReadOnly("Population", typeof(int), typeof(GameField), new PropertyMetadata());
            PopulationProperty = PopulationPropertyKey.DependencyProperty;

            ScaleFactorProperty = DependencyProperty.Register("ScaleFactor", typeof(int), typeof(GameField), new PropertyMetadata(DefaultScaleFactor, ScaleFactorPropertyChanged));
            ShowGridlinesProperty = DependencyProperty.Register("ShowGridlines", typeof(bool), typeof(GameField), new PropertyMetadata(true, NeededRenderingPropertyChanged));
            BackColorProperty = DependencyProperty.Register("BackColor", typeof(Color), typeof(GameField), new PropertyMetadata(Colors.WhiteSmoke, NeededRenderingPropertyChanged));
            GridlinesColorProperty = DependencyProperty.Register("GridlinesColor", typeof(Color), typeof(GameField), new PropertyMetadata(Colors.LightGray, NeededRenderingPropertyChanged));
            AlternatingGridlinesColorProperty = DependencyProperty.Register("AlternatingGridlinesColor", typeof(Color), typeof(GameField), new PropertyMetadata(Colors.DarkGray, NeededRenderingPropertyChanged));
            CellColorProperty = DependencyProperty.Register("CellColor", typeof(Color), typeof(GameField), new PropertyMetadata(Colors.CornflowerBlue, CellColorPropertyChanged));
            SelectionColorProperty = DependencyProperty.Register("SelectionColor", typeof(Color), typeof(GameField), new PropertyMetadata(Color.FromArgb(128, 30, 144, 255), SelectionColorPropertyChanged));
            UpdateIntervalProperty = DependencyProperty.Register("UpdateInterval", typeof(int), typeof(GameField), new PropertyMetadata(100, UpdateIntervalPropertyChanged));
        }

        public GameField()
        {
            InitializeComponent();

            SetCursorMode(_cursorMode, false);

            _updateTimer.Tick += new EventHandler(updateTimer_Tick);

            UpdateSelectionColor(SelectionColor);

            _wb = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgr32, null);
            gameFieldImage.Source = _wb;
        }

        #endregion

        #region Methods

        private static void InitializeCommands()
        {
            DrawCommand = new RoutedUICommand("_Draw", "Draw", typeof(GameField));
            DrawCommand.InputGestures.Add(new KeyGesture(Key.F2));
            MoveCommand = new RoutedUICommand("_Move", "Move", typeof(GameField));
            MoveCommand.InputGestures.Add(new KeyGesture(Key.F3));
            SelectCommand = new RoutedUICommand("_Select", "Select", typeof(GameField));
            SelectCommand.InputGestures.Add(new KeyGesture(Key.F4));

            StartCommand = new RoutedUICommand("Star_t Game", "Start", typeof(GameField));
            StartCommand.InputGestures.Add(new KeyGesture(Key.Enter));
            StopCommand = new RoutedUICommand("Sto_p Game", "Stop", typeof(GameField));
            StopCommand.InputGestures.Add(new KeyGesture(Key.Enter, ModifierKeys.Shift));
            NextGenerationCommand = new RoutedUICommand("_Next Generation", "NextGeneration", typeof(GameField));
            NextGenerationCommand.InputGestures.Add(new KeyGesture(Key.Space));
            ResetCommand = new RoutedUICommand("_Reset Game", "Reset", typeof(GameField));
            ResetCommand.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));

            DeselectAllCommand = new RoutedUICommand("D_eselect All", "DeselectAll", typeof(GameField));
            DeselectAllCommand.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));

            Rotate180Command = new RoutedUICommand("Rotate _180°", "Rotate180", typeof(GameField));
            Rotate90CWCommand = new RoutedUICommand("Rotate _90° CW", "Rotate90CW", typeof(GameField));
            Rotate90CCWCommand = new RoutedUICommand("Rotate 9_0° CCW", "Rotate90CCW", typeof(GameField));
            FlipHorizontalCommand = new RoutedUICommand("Flip _Horizontal", "FlipHorizontal", typeof(GameField));
            FlipVerticalCommand = new RoutedUICommand("Flip _Vertical", "FlipVertical", typeof(GameField));

            FitPatternCommand = new RoutedUICommand("Fit _pattern", "FitPattern", typeof(GameField));
            FitSelectionCommand = new RoutedUICommand("Fit _selection", "FitSelection", typeof(GameField));
            CenteringPatternCommand = new RoutedUICommand("_Centering Pattern", "CenteringPattern", typeof(GameField));
        }

        private static void InitializeEvents()
        {
            PatternChangedEvent = EventManager.RegisterRoutedEvent("PatternChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GameField));
        }

        private static void NeededRenderingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GameField)d).Render();
        }

        private static void ScaleFactorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GameField)d).ScaleFactorChanged((int)e.NewValue);
        }

        private static void CellColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GameField)d).SetStateCellColor(1, (Color)e.NewValue);
        }

        private void ScaleFactorChanged(int newScaleFactor)
        {
            Cell oldCentreCell = new Cell(_topLeftCell.X + FieldWidth / 2, _topLeftCell.Y + FieldHeight / 2);
            _scaleFactor = newScaleFactor;
            Cell newCentreCell = new Cell(_topLeftCell.X + FieldWidth / 2, _topLeftCell.Y + FieldHeight / 2);
            _topLeftCell = new Cell(_topLeftCell.X - (newCentreCell.X - oldCentreCell.X), _topLeftCell.Y - (newCentreCell.Y - oldCentreCell.Y));

            Render();
        }

        private static void SelectionColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GameField field = d as GameField;

            field.UpdateSelectionColor((Color)e.NewValue);
            field.Render();
        }

        private static void UpdateIntervalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var field = d as GameField;
            field._updateTimer.Interval = TimeSpan.FromMilliseconds((int)e.NewValue);
        }

        public GameFieldMemento CreateMemento()
        {
            return new GameFieldMemento(Generation, _game.AliveCells, _selectedRegion);
        }

        public void SetMemento(GameFieldMemento m)
        {
            if (m != null)
            {
                _selectedRegion = m.SelectedRegion;
                SetGeneration(m.Cells, m.Generation);
            }
        }   

        public void Render()
        {
            if (_wb == null || _game == null)
            {
                return;
            }

            int back_c = ColorToBgra32(BackColor);
            int grid_c = ColorToBgra32(GridlinesColor);
            int altgrid_c = ColorToBgra32(AlternatingGridlinesColor);

            int intScale = Math.Max(1, (int)Scale);
            double floatScale = Scale;

            int w = _wb.PixelWidth;
            int h = _wb.PixelHeight;

            int pixelCount = h * w;
            int backBufferStride = _wb.BackBufferStride;

            // Reserve the back buffer for updates.
            _wb.Lock();

            unsafe
            {
                // Get a pointer to the back buffer.
                var pBackBufferOrigin = (long)_wb.BackBuffer;
                var pBackBuffer = pBackBufferOrigin;

                // Draw background
                for (int i = 0; i < pixelCount; i++)
                {
                    *((int*)pBackBuffer) = back_c;
                    pBackBuffer += 4;
                }

                // Draw alive cells
                pBackBuffer = pBackBufferOrigin;

                int n = FieldHeight;
                int m = FieldWidth;
                int nn = n + 1;
                int mm = m + 1;

                int lastColWidth = (int)(w - m * floatScale);
                int lastRowHeight = (int)(h - n * floatScale);

                int top = _topLeftCell.Y;
                int left = _topLeftCell.X;

                Dictionary<Cell, byte> cells = _game.AliveCells;

                lock (cells)
                {
                    foreach (KeyValuePair<Cell, byte> pair in cells)
                    {
                        int x = pair.Key.X - left;
                        int y = pair.Key.Y - top;

                        if (y < 0 || x < 0 || y >= nn || x >= mm)
                            continue;

                        int col_offset = (int)(x * floatScale) * 4;
                        int row_offset = (int)(y * floatScale) * backBufferStride;
                        var offset = pBackBuffer + row_offset + col_offset;

                        int cellWidth = (x < m) ? intScale : lastColWidth;
                        int cellHeight = (y < n) ? intScale : lastRowHeight;

                        int color = _statesCellColors[pair.Value];

                        for (int p = 0; p < cellHeight; p++)
                        {
                            for (int k = 0; k < cellWidth; k++)
                            {
                                *((int*)(offset + p * backBufferStride + k * 4)) = color;
                            }
                        }
                    }
                }

                // Draw pasting cells
                if (_pasting)
                {
                    pBackBuffer = pBackBufferOrigin;

                    Point point = Mouse.GetPosition(this);
                    int cur_x, cur_y;
                    PointToCoordinate(point, out cur_x, out cur_y);

                    int hals_w = _pastingCellsRect.Width / 2;
                    int half_h = _pastingCellsRect.Height / 2;

                    if (_pastingCells != null)
                    {
                        lock (_pastingCells)
                        {
                            int color = _statesCellColors[1];

                            foreach (Cell cell in _pastingCells)
                            {
                                int x = cell.X - left + cur_x - hals_w;
                                int y = cell.Y - top + cur_y - half_h;

                                if (y < 0 || x < 0 || y >= nn || x >= mm)
                                    continue;

                                int col_offset = (int)(x * floatScale) * 4;
                                int row_offset = (int)(y * floatScale) * backBufferStride;
                                var offset = pBackBuffer + row_offset + col_offset;

                                int cellWidth = (x < m) ? intScale : lastColWidth;
                                int cellHeight = (y < n) ? intScale : lastRowHeight;

                                for (int p = 0; p < cellHeight; p++)
                                {
                                    for (int k = 0; k < cellWidth; k++)
                                    {
                                        *((int*)(offset + p * backBufferStride + k * 4)) = color;
                                    }
                                }
                            }
                        }
                    }
                }


                // Draw gridlines
                if (ShowGridlines && _scaleFactor >= MinScaleFactorWhenShowGrid)
                {
                    int cols = (w % intScale == 0) ? (w / intScale) : (w / intScale + 1);
                    int rows = (h % intScale == 0) ? (h / intScale) : (h / intScale + 1);


                    pBackBuffer = pBackBufferOrigin;

                    // Draw vertical gridlines
                    for (int i = 0; i < cols; i++)
                    {
                        int col_offset = i * 4 * intScale;

                        int c = grid_c;
                        if (((i + left) % DefaultAlternationCount) == 0) c = altgrid_c;

                        for (int j = 0; j < h; j++)
                        {
                            *((int*)(pBackBuffer + j * backBufferStride + col_offset)) = c;
                        }
                    }
                    
                    // Draw horizontal gridlines
                    for (int i = 0; i < rows; i++)
                    {
                        int row_offset = i * backBufferStride * intScale;

                        int c = grid_c;
                        if (((i + top) % DefaultAlternationCount) == 0) c = altgrid_c;

                        for (int j = 0; j < w; j++)
                        {
                            *((int*)(pBackBuffer + row_offset + j * 4)) = c;
                        }
                    }
                }

                // Draw selected region
                if (!RectIsEmpty(_selectedRegion))
                {
                    pBackBuffer = pBackBufferOrigin;

                    int x = (int)(Math.Max(_selectedRegion.X - left, 0) * floatScale);
                    int y = (int)(Math.Max(_selectedRegion.Y - top, 0) * floatScale);

                    int sel_w = Math.Min((int)(((_selectedRegion.X - left) + _selectedRegion.Width) * floatScale), w) - x;
                    int sel_h = Math.Min((int)(((_selectedRegion.Y - top) + _selectedRegion.Height) * floatScale), h) - y;

                    int col_offset = x * 4;
                    int row_offset = y * backBufferStride;
                    var offset = pBackBuffer + row_offset + col_offset;

                    for (int i = 0; i < sel_h; i++)
                    {
                        for (int j = 0; j < sel_w; j++)
                        {
                            var d = offset + i * backBufferStride + j * 4;

                            var old_color = *((int*)d);
                            *((int*)d) = OverlayColors(old_color);
                        }
                    }
                }
            }

            // Specify the area of the bitmap that changed.
            _wb.AddDirtyRect(new Int32Rect(0, 0, w, h));

            // Release the back buffer and make it available for display.
            _wb.Unlock();
        }

        private void RenderCell(Cell cell, byte state)
        {
            if (_wb == null || _game == null)
            {
                return;
            }

            int back_c = ColorToBgra32(BackColor);
            int grid_c = ColorToBgra32(GridlinesColor);
            int altgrid_c = ColorToBgra32(AlternatingGridlinesColor);

            int intScale = Math.Max(1, (int)Scale);
            double floatScale = Scale;

            int w = _wb.PixelWidth;
            int h = _wb.PixelHeight;

            int backBufferStride = _wb.BackBufferStride;

            // Reserve the back buffer for updates.
            _wb.Lock();

            unsafe
            {
                // Get a pointer to the back buffer.
                var pBackBufferOrigin = (long)_wb.BackBuffer;
                var pBackBuffer = pBackBufferOrigin;

                // Draw cell
                int n = FieldHeight;
                int m = FieldWidth;
                int nn = n + 1;
                int mm = m + 1;

                int lastColWidth = (int)(w - m * floatScale);
                int lastRowHeight = (int)(h - n * floatScale);

                int top = _topLeftCell.Y;
                int left = _topLeftCell.X;

                int x = cell.X - left;
                int y = cell.Y - top;

                int cellWidth = (x < m) ? intScale : lastColWidth;
                int cellHeight = (y < n) ? intScale : lastRowHeight;

                if (y < 0 || x < 0 || y >= nn || x >= mm)
                {
                    return;
                }
                else
                {
                    int col_offset = (int)(x * floatScale) * 4;
                    int row_offset = (int)(y * floatScale) * backBufferStride;
                    var offset = pBackBuffer + row_offset + col_offset;

                    int cellColor = _statesCellColors[state];
                    if (state == 0) cellColor = back_c;

                    for (int p = 0; p < cellHeight; p++)
                    {
                        for (int k = 0; k < cellWidth; k++)
                        {
                            *((int*)(offset + p * backBufferStride + k * 4)) = cellColor;
                        }
                    }
                }

                // Draw gridlines
                if (ShowGridlines && _scaleFactor >= MinScaleFactorWhenShowGrid)
                {
                    int cols = (w % intScale == 0) ? (w / intScale) : (w / intScale + 1);
                    int rows = (h % intScale == 0) ? (h / intScale) : (h / intScale + 1);

                    pBackBuffer = pBackBufferOrigin;

                    // Draw vertical gridlines
                    int x_max = Math.Min(x + 1, cols);
                    int y_max = Math.Min(y + 1, rows);

                    for (int i = x; i < x_max; i++)
                    {
                        int col_offset = i * 4 * intScale;

                        int c = grid_c;
                        if (((i + left) % DefaultAlternationCount) == 0) c = altgrid_c;

                        for (int j = y * intScale; j < y_max * intScale; j++)
                        {
                            *((int*)(pBackBuffer + j * backBufferStride + col_offset)) = c;
                        }
                    }

                    // Draw horizontal gridlines
                    for (int i = y; i < y_max; i++)
                    {
                        int row_offset = i * backBufferStride * intScale;

                        int c = grid_c;
                        if (((i + top) % DefaultAlternationCount) == 0) c = altgrid_c;

                        for (int j = x * intScale; j < x_max * intScale; j++)
                        {
                            *((int*)(pBackBuffer + row_offset + j * 4)) = c;
                        }
                    }
                }

                // Draw selected region
                if (!RectIsEmpty(_selectedRegion))
                {
                    if (cell.X >= _selectedRegion.X && cell.X <= _selectedRegion.X + _selectedRegion.Width &&
                        cell.Y >= _selectedRegion.Y && cell.Y <= _selectedRegion.Y + _selectedRegion.Height)
                    {
                        pBackBuffer = pBackBufferOrigin;

                        int col_offset = (int)(x * floatScale) * 4;
                        int row_offset = (int)(y * floatScale) * backBufferStride;
                        var offset = pBackBuffer + row_offset + col_offset;

                        for (int p = 0; p < cellHeight; p++)
                        {
                            for (int k = 0; k < cellWidth; k++)
                            {
                                var d = offset + p * backBufferStride + k * 4;

                                int old_color = *((int*)d);
                                *((int*)d) = OverlayColors(old_color);
                            }
                        }
                    }
                }

                // Specify the area of the bitmap that changed.
                _wb.AddDirtyRect(new Int32Rect((int)(x * floatScale), (int)(y * floatScale), cellWidth, cellHeight));
            }

            // Release the back buffer and make it available for display.
            _wb.Unlock();
        }

        public void Initialize()
        {
            _undoManager.Reset();

            if (_game == null)
                _game = new QuickLifeEngine();
            else
                _game.Initialize();

            GC.Collect();

            Generation = 0;
            UpdatePopulationSize();

            if ((int)ActualWidth != 0 || (int)ActualHeight != 0)
            {
                UpdateImageSource();
                Render();
            }
        }

        public void Initialize(Dictionary<Cell, byte> cells)
        {
            _undoManager.Reset();

            _game.Initialize(cells);
            UpdateImageSource();

            GC.Collect();

            Generation = 0;
            UpdatePopulationSize();

            FitPattern();
        }

        public void Start()
        {
            if (!_updateTimer.IsEnabled)
            {
                _prevGeneration = Generation;
                _prevCells = new Dictionary<Cell, byte>(_game.AliveCells);

                _updateTimer.Start();
            }
        }

        public void Stop()
        {
            if (_updateTimer.IsEnabled)
            {
                _updateTimer.Stop();

                AddUndoUnit(new GenerationUndoUnit(this, _prevGeneration, _prevCells,
                    Generation, new Dictionary<Cell, byte>(_game.AliveCells)));
            }
        }

        public void Reset()
        {
            Stop();

            int undoUnitCount = _undoUnitCountForReset;
            Undo(Math.Min(undoUnitCount, UndoLimit), true);

            if (undoUnitCount > UndoLimit)
                _undoManager.Reset();

            SetMemento(_mementoForReset);
            _undoUnitCountForReset = 0;
        }

        public bool NextGeneration()
        {
            Stop();

            int prevGeneration = Generation;
            IDictionary<Cell, byte> prevCells = new Dictionary<Cell, byte>(_game.AliveCells);

            bool finished = _NextGeneration();

            int nextGeneration = Generation;
            IDictionary<Cell, byte> nextCells = new Dictionary<Cell, byte>(_game.AliveCells);

            AddUndoUnit(new GenerationUndoUnit(this, prevGeneration, prevCells, nextGeneration, nextCells));

            return finished;
        }

        private bool _NextGeneration()
        {
            bool finished = _game.NextGeneration();
            Render();

            Generation++;
            UpdatePopulationSize();

            return finished;
        }

        public void Clear()
        {
            _undoManager.Reset();

            _game.Clear();
            Render();

            GC.Collect();

            Generation = 0;
            UpdatePopulationSize();
        }

        public void SetCursorMode(GameFieldCursorMode cursorMode, bool savingMode)
        {
            if (savingMode)
            {
                _cursorMode = cursorMode;
            }

            switch (cursorMode)
            {
                case GameFieldCursorMode.Draw:
                    Cursor = Cursors.Pen;
                    break;

                case GameFieldCursorMode.Move:
                    Cursor = Cursors.SizeAll;
                    break;

                case GameFieldCursorMode.Select:
                    Cursor = Cursors.Cross;
                    break;
            }
        }

        public void SetStateCellColor(int state, Color color)
        {
            _statesCellColors[state] = ColorToBgra32(color);
            Render();
        }

        public void SetStatesCellColors(IEnumerable<Color> colors)
        {
            if (colors != null)
            {
                int state = 1;
                foreach (Color color in colors)
                {
                    _statesCellColors[state++] = ColorToBgra32(color);
                }

                Render();
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public void FitPattern()
        {
            FitPattern(MinScaleFactor, MaxScaleFactor);
        }

        public void FitPattern(int minScaleFactor, int maxScaleFactor)
        {
            int x, y, w, h;
            LifeHelpers.GetBoundedRect(Cells.Keys, out x, out y, out w, out h);

            FitSelection(new Int32Rect(x, y, w, h), minScaleFactor, maxScaleFactor);
        }

        public void FitSelection()
        {
            FitSelection(_selectedRegion, MinScaleFactor, MaxScaleFactor);
        }

        public void FitSelection(Int32Rect rect, int minScaleFactor, int maxScaleFactor)
        {
            if (rect.Width == 0 || rect.Height == 0)
            {
                _topLeftCell = new Cell(0, 0);
            }
            else
            {
                int left = rect.X;
                int top = rect.Y;
                int w = rect.Width;
                int h = rect.Height;

                for (int i = maxScaleFactor; i >= minScaleFactor; i--)
                {
                    double scale = GetScale(i);

                    if ((w * scale <= ActualWidth && h * scale <= ActualHeight) || i == minScaleFactor)
                    {
                        ScaleFactor = i;

                        _topLeftCell.X = left + w / 2 - FieldWidth / 2;
                        _topLeftCell.Y = top + h / 2 - FieldHeight / 2;
                        break;
                    }
                }
            }

            Render();
        }

        public void CenteringPattern()
        {
            if (_game.AliveCells.Count == 0)
            {
                _topLeftCell = new Cell(0, 0);
            }
            else
            {
                int left, top;
                int w, h;

                LifeHelpers.GetBoundedRect(Cells.Keys, out left, out top, out w, out h);

                _topLeftCell.X = left + w / 2 - FieldWidth / 2;
                _topLeftCell.Y = top + h / 2 - FieldHeight / 2;
            }

            Render();
        }

        private void UpdateImageSource()
        {
            _wb = new WriteableBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Bgr32, null);
            gameFieldImage.Source = _wb;
        }

        private void UpdateSelectionColor(Color selColor)
        {
            int sel_c = ColorToBgra32(selColor);
            _sel_c = sel_c;

            int sel_c_a = (sel_c >> 24) & 255;
            int sel_c_r = (sel_c >> 16) & 255;
            int sel_c_g = (sel_c >> 8) & 255;
            int sel_c_b = sel_c & 255;
            double sel_c_opacity = sel_c_a / 255.0;

            _sel_c_transp = 1 - sel_c_opacity;
            _sel_c_r_o = sel_c_r * sel_c_opacity;
            _sel_c_g_o = sel_c_g * sel_c_opacity;
            _sel_c_b_o = sel_c_b * sel_c_opacity;
        }

        public void Undo()
        {
            Undo(1, false);
        }

        protected void Undo(int count, bool idleRun)
        {
            if (CanUndo)
            {
                if (Generation > 0)
                    _undoUnitCountForReset -= count;

                _undoManager.Undo(count, idleRun);
                UpdatePopulationSize();
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                _undoManager.Redo();
                UpdatePopulationSize();

                if (Generation > 0)
                    _undoUnitCountForReset++;
            }
        }

        public void Cut()
        {
            DeleteUndo(_selectedRegion);

            LifePattern pattern = new LifePattern();
            pattern.Rule = Rule;
            Dictionary<Cell, byte> selectedCells = LifeHelpers.GetSelectedCells(Cells, _selectedRegion.X, _selectedRegion.Y, _selectedRegion.Width, _selectedRegion.Height);
            pattern.Cells = selectedCells;

            string text;
            LifePatternFileFormatManager.SavePatternToString(pattern, out text);
            Clipboard.SetText(text);

            foreach (Cell cell in selectedCells.Keys)
            {
                _game[cell.X, cell.Y] = 0;
            }

            RaisePatternChangedEvent();
            UpdatePopulationSize();
            Render();
        }

        public void Copy()
        {
            LifePattern pattern = new LifePattern();
            pattern.Rule = Rule;
            pattern.Cells = LifeHelpers.GetSelectedCells(Cells, _selectedRegion.X, _selectedRegion.Y, _selectedRegion.Width, _selectedRegion.Height);

            string text;
            LifePatternFileFormatManager.SavePatternToString(pattern, out text);
            Clipboard.SetText(text);
        }

        public void Paste()
        {
            Focus();
            
            LifePattern pattern = LifePatternFileFormatManager.LoadPatternFromString(Clipboard.GetText());

            _pastingCells = LifeHelpers.CellsAlignToOrigin(pattern.Cells.Keys);
            int x, y, w, h;
            LifeHelpers.GetBoundedRect(_pastingCells, out x, out y, out w, out h);
            _pastingCellsRect = new Int32Rect(x, y, w, h);

            _pasting = true;

            SetCursorMode(GameFieldCursorMode.Move, false);

            Render();
        }

        private void PasteComplete(int left, int top)
        {
            if (_pastingCells != null)
            {
                Int32Rect rect = new Int32Rect(left + _pastingCellsRect.X, top + _pastingCellsRect.Y,
                    _pastingCellsRect.Width, _pastingCellsRect.Height);

                List<CellInfo> pastingCellsInfo = new List<CellInfo>();

                switch (_pasteMode)
                {
                    case GameFieldPasteMode.Copy:
                        ICollection<Cell> selectedCells1 = LifeHelpers.GetSelectedCells(Cells, rect.X, rect.Y, rect.Width, rect.Height).Keys;

                        foreach (Cell cell in selectedCells1)
                        {
                            if (!_pastingCells.Contains(new Cell(cell.X - left, cell.Y - top)))
                            {
                                pastingCellsInfo.Add(new CellInfo(cell, GetInternalCellState(cell), 0));
                                _game[cell.X, cell.Y] = 0;
                            }
                        }

                        foreach (Cell cell in _pastingCells)
                        {
                            Cell newCell = new Cell(left + cell.X, top + cell.Y);
                            pastingCellsInfo.Add(new CellInfo(newCell, GetInternalCellState(newCell), 1));
                            _game[newCell.X, newCell.Y] = 1;
                        }
                        break;

                    case GameFieldPasteMode.And:
                        ICollection<Cell> selectedCells2 = LifeHelpers.GetSelectedCells(Cells, rect.X, rect.Y, rect.Width, rect.Height).Keys;

                        foreach (Cell cell in selectedCells2)
                        {
                            if (!_pastingCells.Contains(new Cell(cell.X - left, cell.Y - top)))
                            {
                                pastingCellsInfo.Add(new CellInfo(cell, GetInternalCellState(cell), 0));
                                _game[cell.X, cell.Y] = 0;
                            }
                        }
                        break;

                    case GameFieldPasteMode.Or:
                        foreach (Cell cell in _pastingCells)
                        {
                            Cell newCell = new Cell(left + cell.X, top + cell.Y);
                            pastingCellsInfo.Add(new CellInfo(newCell, GetInternalCellState(newCell), 1));
                            _game[newCell.X, newCell.Y] = 1;
                        }
                        break;

                    case GameFieldPasteMode.Xor:
                        foreach (Cell cell in _pastingCells)
                        {
                            Cell newCell = new Cell(left + cell.X, top + cell.Y);
                            byte oldState = GetInternalCellState(newCell);

                            if (oldState == 0)
                            {
                                pastingCellsInfo.Add(new CellInfo(newCell, oldState, 1));
                                _game[newCell.X, newCell.Y] = 1;
                            }
                            else
                            {
                                pastingCellsInfo.Add(new CellInfo(newCell, oldState, 0));
                                _game[newCell.X, newCell.Y] = 0;
                            }
                        }
                        break;
                }

                _pastingCells = null;
                AddUndoUnit(new DrawCellsUndoUnit(this, pastingCellsInfo));

                RaisePatternChangedEvent();
                UpdatePopulationSize();
                Render();
            }
        }

        private void CancelPaste()
        {
            _pasting = false;
            _pastingCells = null;
            SetCursorMode(_cursorMode, false);

            Render();
        }

        public void Delete()
        {
            Delete(_selectedRegion);
        }

        public void Delete(Int32Rect region)
        {
            DeleteUndo(region);
            _Delete(region);
            RaisePatternChangedEvent();
        }

        private void _Delete(Int32Rect region)
        {
            Dictionary<Cell, byte> selectedCells =
                LifeHelpers.GetSelectedCells(Cells, region.X, region.Y, region.Width, region.Height);

            foreach (Cell cell in selectedCells.Keys)
            {
                _game[cell.X, cell.Y] = 0;
            }

            UpdatePopulationSize();
            Render();
        }

        private void DeleteUndo(Int32Rect region)
        {
            ICollection<Cell> selectedCells =
                LifeHelpers.GetSelectedCells(Cells, region.X, region.Y, region.Width, region.Height).Keys;

            IList<CellInfo> internalCellStates = new List<CellInfo>();

            foreach (Cell cell in selectedCells)
            {
                internalCellStates.Add(new CellInfo(cell, GetInternalCellState(cell), 0));
            }

            AddUndoUnit(new DrawCellsUndoUnit(this, internalCellStates));
        }

        public void Select(Int32Rect region)
        {
            AddUndoUnit(new SelectRegionUndoUnit(this, region, _selectedRegion));
            _Select(region);
        }

        private void _Select(Int32Rect region)
        {
            _selectedRegion = region;
            Render();
        }

        public void SelectAll()
        {
            int x, y, w, h;
            LifeHelpers.GetBoundedRect(Cells.Keys, out x, out y, out w, out h);
            Int32Rect newSelectedRegion = new Int32Rect(x, y, w, h);

            AddUndoUnit(new SelectRegionUndoUnit(this, newSelectedRegion, _selectedRegion));
            _Select(newSelectedRegion);
        }

        public void DeselectAll()
        {
            AddUndoUnit(new SelectRegionUndoUnit(this, Int32Rect.Empty, _selectedRegion));
            _Select(Int32Rect.Empty);
        }


        public void Rotate180()
        {
            AddUndoUnit(new RotateAndFlipUndoUnit(this, _Rotate180, _selectedRegion,
                LifeHelpers.GetSelectedCells(_game.AliveCells, _selectedRegion.X,
                _selectedRegion.Y, _selectedRegion.Width, _selectedRegion.Height)));

            _Rotate180();
            RaisePatternChangedEvent();
        }

        private void _Rotate180()
        {
            int right = _selectedRegion.X + _selectedRegion.Width - 1;
            int bottom = _selectedRegion.Y + _selectedRegion.Height - 1;

            lock (_game)
            {
                Dictionary<Cell, byte> selectedCells = LifeHelpers.GetSelectedCells(Cells,
                    _selectedRegion.X, _selectedRegion.Y, _selectedRegion.Width, _selectedRegion.Height);

                foreach (Cell cell in selectedCells.Keys)
                {
                    int i = cell.X - _selectedRegion.X;
                    int j = cell.Y - _selectedRegion.Y;

                    int x = cell.X;
                    int y = cell.Y;

                    byte temp = _game[x, y];
                    _game[x, y] = _game[right - i, bottom - j];
                    _game[right - i, bottom - j] = temp;
                }
            }

            Render();
        }

        public void Rotate90CW()
        {
            Int32Rect newSelectionRegion = Rotate90Rect(_selectedRegion);

            Dictionary<Cell, byte> oldCells = LifeHelpers.GetSelectedCells(_game.AliveCells,
                    _selectedRegion.X, _selectedRegion.Y, _selectedRegion.Width, _selectedRegion.Height,
                    newSelectionRegion.X, newSelectionRegion.Y, newSelectionRegion.Width, newSelectionRegion.Height);

            AddUndoUnit(new Rotate90UndoUnit(this, true, _selectedRegion, newSelectionRegion, oldCells));
            _Rotate90CW();
            RaisePatternChangedEvent();
        }

        private void _Rotate90CW()
        {
            Int32Rect newSelectionRegion = Rotate90Rect(_selectedRegion);
            int new_x = newSelectionRegion.X;
            int new_y = newSelectionRegion.Y;
            int new_w = newSelectionRegion.Width;
            int new_h = newSelectionRegion.Height;

            int right = new_x + new_w - 1;

            lock (_game)
            {
                Dictionary<Cell, byte> selectedCells = LifeHelpers.GetSelectedCells(Cells,
                    _selectedRegion.X, _selectedRegion.Y, _selectedRegion.Width, _selectedRegion.Height);

                foreach (Cell cell in selectedCells.Keys)
                {
                    _game[cell.X, cell.Y] = 0;
                }

                foreach (KeyValuePair<Cell, byte> pair in selectedCells)
                {
                    int i = pair.Key.X - _selectedRegion.X;
                    int j = pair.Key.Y - _selectedRegion.Y;
                    int y = new_y + i;

                    _game[right - j, y] = pair.Value;
                }
            }

            _selectedRegion = newSelectionRegion;

            UpdatePopulationSize();
            Render();
        }

        public void Rotate90CCW()
        {
            Int32Rect newSelectionRegion = Rotate90Rect(_selectedRegion);

            Dictionary<Cell, byte> oldCells = LifeHelpers.GetSelectedCells(_game.AliveCells,
                    _selectedRegion.X, _selectedRegion.Y, _selectedRegion.Width, _selectedRegion.Height,
                    newSelectionRegion.X, newSelectionRegion.Y, newSelectionRegion.Width, newSelectionRegion.Height);

            AddUndoUnit(new Rotate90UndoUnit(this, false, _selectedRegion, newSelectionRegion, oldCells));
            _Rotate90CCW();
            RaisePatternChangedEvent();
        }

        private void _Rotate90CCW()
        {
            Int32Rect newSelectionRegion = Rotate90Rect(_selectedRegion);
            int new_x = newSelectionRegion.X;
            int new_y = newSelectionRegion.Y;
            int new_w = newSelectionRegion.Width;
            int new_h = newSelectionRegion.Height;

            int bottom = new_y + new_h - 1;

            lock (_game)
            {
                Dictionary<Cell, byte> selectedCells = LifeHelpers.GetSelectedCells(Cells,
                    _selectedRegion.X, _selectedRegion.Y, _selectedRegion.Width, _selectedRegion.Height);

                foreach (Cell cell in selectedCells.Keys)
                {
                    _game[cell.X, cell.Y] = 0;
                }

                foreach (KeyValuePair<Cell, byte> pair in selectedCells)
                {
                    int i = pair.Key.X - _selectedRegion.X;
                    int j = pair.Key.Y - _selectedRegion.Y;
                    int x = new_x + j;

                    _game[x, bottom - i] = pair.Value;
                }
            }

            _selectedRegion = newSelectionRegion;

            UpdatePopulationSize();
            Render();
        }

        public void FlipHorizontal()
        {
            AddUndoUnit(new RotateAndFlipUndoUnit(this, _FlipHorizontal, _selectedRegion,
                LifeHelpers.GetSelectedCells(_game.AliveCells, _selectedRegion.X,
                _selectedRegion.Y, _selectedRegion.Width, _selectedRegion.Height)));

            _FlipHorizontal();
            RaisePatternChangedEvent();
        }

        private void _FlipHorizontal()
        {
            int right = _selectedRegion.X + _selectedRegion.Width - 1;

            lock (_game)
            {
                Dictionary<Cell, byte> selectedCells = LifeHelpers.GetSelectedCells(Cells,
                    _selectedRegion.X, _selectedRegion.Y, _selectedRegion.Width, _selectedRegion.Height);

                foreach (Cell cell in selectedCells.Keys)
                {
                    int i = cell.X - _selectedRegion.X;

                    int x = _selectedRegion.X + i;
                    int y = cell.Y;

                    byte temp = _game[x, y];
                    _game[x, y] = _game[right - i, y];
                    _game[right - i, y] = temp;
                }
            }

            Render();
        }

        public void FlipVertical()
        {
            AddUndoUnit(new RotateAndFlipUndoUnit(this, _FlipVertical, _selectedRegion,
                LifeHelpers.GetSelectedCells(_game.AliveCells, _selectedRegion.X,
                _selectedRegion.Y, _selectedRegion.Width, _selectedRegion.Height)));

            _FlipVertical();
            RaisePatternChangedEvent();
        }

        private void _FlipVertical()
        {
            int bottom = _selectedRegion.Y + _selectedRegion.Height - 1;

            lock (_game)
            {
                Dictionary<Cell, byte> selectedCells = LifeHelpers.GetSelectedCells(Cells,
                    _selectedRegion.X, _selectedRegion.Y, _selectedRegion.Width, _selectedRegion.Height);

                foreach (Cell cell in selectedCells.Keys)
                {
                    int j = cell.Y - _selectedRegion.Y;

                    int x = cell.X;
                    int y = _selectedRegion.Y + j;

                    byte temp = _game[x, y];
                    _game[x, y] = _game[x, bottom - j];
                    _game[x, bottom - j] = temp;
                }
            }

            Render();
        }


        private void RaisePatternChangedEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(PatternChangedEvent);
            RaiseEvent(newEventArgs);
        }

        #region Event Handlers

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(this);
            int x, y;
            PointToCoordinate(point, out x, out y);

            if (_pasting)
            {
                PasteComplete(x - _pastingCellsRect.Width / 2, y - _pastingCellsRect.Height / 2);
                return;
            }

            switch (_cursorMode)
            {
                case GameFieldCursorMode.Draw:
                    _currPressedCell = new Cell(x, y);
                    byte oldState = GetInternalCellState(_currPressedCell);
                    _currPressedCellReverseState = (oldState != 0) ? (byte)0 : (byte)1;

                    _drawingCells.Clear();
                    _drawingCells.Add(new CellInfo(_currPressedCell, oldState, _currPressedCellReverseState));

                    _game[x, y] = _currPressedCellReverseState;

                    RaisePatternChangedEvent();
                    UpdatePopulationSize();
                    RenderCell(_currPressedCell, _currPressedCellReverseState);
                    break;

                case GameFieldCursorMode.Move:
                    _currPressedPoint = point;
                    CaptureMouse();
                    Render();
                    break;

                case GameFieldCursorMode.Select:
                    _previousSelectedRegion = _selectedRegion;
                    _currPressedPoint = point;
                    CaptureMouse();
                    Render();
                    break;
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            CancelPaste();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_pasting)
            {
                _pasting = false;
                SetCursorMode(_cursorMode, false);
                return;
            }

            switch (_cursorMode)
            {
                case GameFieldCursorMode.Move:
                    ReleaseMouseCapture();
                    break;

                case GameFieldCursorMode.Select:
                    if (!RectIsEmpty(_selectedRegion) || !RectIsEmpty(_previousSelectedRegion))
                    {
                        AddUndoUnit(new SelectRegionUndoUnit(this, _selectedRegion, _previousSelectedRegion));
                    }
                    ReleaseMouseCapture();
                    break;

                case GameFieldCursorMode.Draw:
                    AddUndoUnit(new DrawCellsUndoUnit(this, _drawingCells));
                    break;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point point = e.GetPosition(this);
                int x, y;

                PointToCoordinate(point, out x, out y);

                switch (_cursorMode)
                {
                    case GameFieldCursorMode.Draw:
                        if (_currPressedCell.X != x || _currPressedCell.Y != y)
                        {
                            _currPressedCell = new Cell(x, y);
                            byte oldState = GetInternalCellState(_currPressedCell);

                            if (oldState != _currPressedCellReverseState)
                            {
                                _drawingCells.Add(new CellInfo(_currPressedCell, oldState, _currPressedCellReverseState));
                                _game[x, y] = _currPressedCellReverseState;

                                UpdatePopulationSize();
                                RenderCell(_currPressedCell, _currPressedCellReverseState);
                            }
                        }
                        break;

                    case GameFieldCursorMode.Move:
                        if (IsMouseCaptured)
                        {
                            if (Math.Abs(_currPressedPoint.X - point.X) >= Scale || Math.Abs(_currPressedPoint.Y - point.Y) >= Scale)
                            {
                                int offset_x = (int)((_currPressedPoint.X - point.X) / Scale);
                                int offset_y = (int)((_currPressedPoint.Y - point.Y) / Scale);

                                _topLeftCell.X += offset_x;
                                _topLeftCell.Y += offset_y;

                                _currPressedPoint.X -= offset_x * Scale;
                                _currPressedPoint.Y -= offset_y * Scale;

                                Render();
                            }
                        }
                        break;

                    case GameFieldCursorMode.Select:
                        if (IsMouseCaptured)
                        {
                            Cell selectedTopLeftCell = new Cell();
                            PointToCoordinate(_currPressedPoint, out selectedTopLeftCell.X, out selectedTopLeftCell.Y);

                            _selectedRegion = new Int32Rect(Math.Min(selectedTopLeftCell.X, x), Math.Min(selectedTopLeftCell.Y, y),
                                Math.Abs(x - selectedTopLeftCell.X), Math.Abs(y - selectedTopLeftCell.Y));

                            Render();
                        }
                        break;
                }
            }
            else if (_pasting)
            {
                Render();
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (ScaleFactor < MaxScaleFactor)
                {
                    ScaleFactor++;
                }
            }
            else
            {
                if (ScaleFactor > MinScaleFactor)
                {
                    ScaleFactor--;
                }
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            // Centering
            int oldFieldWidth = (int)(sizeInfo.PreviousSize.Width / Scale);
            int oldFieldHeight = (int)(sizeInfo.PreviousSize.Height / Scale);

            int newFieldWidth = (int)(sizeInfo.NewSize.Width / Scale);
            int newFieldHeight = (int)(sizeInfo.NewSize.Height / Scale);

            Cell oldCentreCell = new Cell(_topLeftCell.X + oldFieldWidth / 2, _topLeftCell.Y + oldFieldHeight / 2);
            Cell newCentreCell = new Cell(_topLeftCell.X + newFieldWidth / 2, _topLeftCell.Y + newFieldHeight / 2);

            _topLeftCell = new Cell(_topLeftCell.X - (newCentreCell.X - oldCentreCell.X), _topLeftCell.Y - (newCentreCell.Y - oldCentreCell.Y));


            UpdateImageSource();
            Render();
        }


        private void updateTimer_Tick(object sender, EventArgs e)
        {
            _NextGeneration();

            if (IsEmpty)
            {
                Stop();
            }
        }

        #region Command bindings

        private void selectionNeededCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _selectedRegion.Width != 0 && _selectedRegion.Height != 0;
            e.Handled = true;
        }

        private void aliveCellsNeededCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (Cells.Count > 0);
            e.Handled = true;
        }

        private void stopCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _updateTimer.IsEnabled;
            e.Handled = true;
        }

        private void resetCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (Generation > 0);
            e.Handled = true;
        }


        private void drawCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CursorMode = GameFieldCursorMode.Draw;
        }

        private void moveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CursorMode = GameFieldCursorMode.Move;
        }

        private void selectCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CursorMode = GameFieldCursorMode.Select;
        }


        private void startCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Start();
        }

        private void stopCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Stop();
        }

        private void nextGenerationCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NextGeneration();
        }

        private void resetCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Reset();
        }


        private void undoCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanUndo;
            e.Handled = true;
        }

        private void undoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Undo();
        }

        private void redoCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanRedo;
            e.Handled = true;
        }

        private void redoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Redo();
        }


        private void cutCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Cut();
        }

        private void copyCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Copy();
        }

        private void pasteCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            bool canExecute = false;

            if (Clipboard.ContainsText())
            {
                try
                {
                    LifePatternFileFormatManager.LoadPatternFromString(Clipboard.GetText());
                    canExecute = true;
                }
                catch (FormatException)
                {
                }
            }

            e.CanExecute = canExecute;
            e.Handled = true;
        }

        private void pasteCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paste();
        }

        private void deleteCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Delete();
        }

        private void selectAllCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectAll();
        }

        private void deselectAllCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DeselectAll();
        }


        private void rotate180CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Rotate180();
        }

        private void rotate90CWCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Rotate90CW();
        }

        private void rotate90CCWCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Rotate90CCW();
        }

        private void flipHorizontalCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FlipHorizontal();
        }

        private void flipVerticalCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FlipVertical();
        }


        private void fitPatternCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FitPattern();
        }

        private void fitSelectionCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FitSelection();
        }

        private void centeringPatternCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CenteringPattern();
        }

        #endregion

        #endregion

        #region Helpers

        public void PointToCoordinate(Point point, out int x, out int y)
        {
            x = (int)Math.Floor(point.X / Scale) + _topLeftCell.X;
            y = (int)Math.Floor(point.Y / Scale) + _topLeftCell.Y;
        }

        private static int ColorToBgra32(Color color)
        {
            return (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
        }

        private static int ColorToBgra32(int a, int r, int g, int b)
        {
            return (a << 24) | (r << 16) | (g << 8) | b;
        }

        private static double GetScale(int scaleFactor)
        {
            return Math.Pow(2, scaleFactor);
        }

        private static bool RectIsEmpty(Int32Rect rect)
        {
            return (rect.Height == 0) && (rect.Width == 0);
        }

        private static Int32Rect Rotate90Rect(Int32Rect rect)
        {
            int new_x = rect.X + rect.Width / 2 - rect.Height / 2;
            int new_y = rect.Y + rect.Height / 2 - rect.Width / 2;
            int new_w = rect.Height;
            int new_h = rect.Width;

            return new Int32Rect(new_x, new_y, new_w, new_h);
        }

        private void UpdatePopulationSize()
        {
            Population = _game.Population;
        }

        private byte GetInternalCellState(Cell cell)
        {
            return _game.GetInternalCellState(cell);
        }

        private void SetCellState(Cell cell, byte state)
        {
            _game[cell.X, cell.Y] = state;
        }

        private void SetInternalCellState(Cell cell, byte state)
        {
            _game.SetInternalCellState(cell, state);
        }

        private void SetInternalCellStates(IDictionary<Cell, byte> cells)
        {
            foreach (KeyValuePair<Cell, byte> pair in cells)
            {
                _game.SetInternalCellState(pair.Key, pair.Value);
            }

            Render();
        }

        private void SetGeneration(IDictionary<Cell, byte> cells, int generation)
        {
            _game.Clear();

            foreach (KeyValuePair<Cell, byte> pair in cells)
            {
                SetInternalCellState(pair.Key, pair.Value);
            }

            Render();

            Generation = generation;
            UpdatePopulationSize();
        }

        private void AddUndoUnit(IUndoUnit unit)
        {
            if (!_updateTimer.IsEnabled)
            {
                _undoManager.Add(unit);
                _undoManager.Commit();

                if (Generation > 0)
                    _undoUnitCountForReset++;

                if (unit is GenerationUndoUnit)
                {
                    GenerationUndoUnit genUnit = unit as GenerationUndoUnit;

                    if (genUnit.PrevGeneration == 0)
                    {
                        _mementoForReset = new GameFieldMemento(genUnit.PrevGeneration,
                            genUnit.PreviousCells, _selectedRegion);
                        _undoUnitCountForReset = 1;
                    }
                }
            }
        }


        private int OverlayColors(int bgraColor)
        {
            int first_r = (bgraColor >> 16) & 255;
            int first_g = (bgraColor >> 8) & 255;
            int first_b = bgraColor & 255;

            const int new_a = 255;
            int new_r = (int)(_sel_c_r_o + first_r * _sel_c_transp);
            int new_g = (int)(_sel_c_g_o + first_g * _sel_c_transp);
            int new_b = (int)(_sel_c_b_o + first_b * _sel_c_transp);

            return ColorToBgra32(new_a, new_r, new_g, new_b);
        }

        #endregion

        #endregion

        #region Nested types

        public enum GameFieldCursorMode
        {
            Draw,
            Move,
            Select
        }

        public enum GameFieldPasteMode
        {
            Copy,
            And,
            Or,
            Xor
        }

        public class GameFieldMemento
        {
            #region Fields

            private int _generation;

            private IDictionary<Cell, byte> _cells;

            private Int32Rect _selectedRegion;

            #endregion

            #region Properties

            public int Generation
            {
                get
                {
                    return _generation;
                }
            }

            public IDictionary<Cell, byte> Cells
            {
                get
                {
                    return _cells;
                }
            }

            public Int32Rect SelectedRegion
            {
                get
                {
                    return _selectedRegion;
                }
            }

            #endregion

            #region Constructors

            public GameFieldMemento(int generation, IDictionary<Cell, byte> cells, Int32Rect selectedRegion)
            {
                _generation = generation;
                _cells = new Dictionary<Cell, byte>(cells);
                _selectedRegion = selectedRegion;
            }

            #endregion
        }


        class DrawCellsUndoUnit : IUndoUnit
        {
            #region Fields

            private GameField _target;

            private IList<CellInfo> _cells;

            #endregion

            #region Constructors

            public DrawCellsUndoUnit(GameField target, IList<CellInfo> cells)
            {
                _target = target;
                _cells = new List<CellInfo>(cells);
            }

            #endregion

            #region Methods

            public bool Redo()
            {
                foreach (CellInfo cellInfo in _cells)
                {
                    _target.SetInternalCellState(cellInfo.Cell, cellInfo.NewState);
                }

                _target.Render();

                return true;
            }

            public bool Undo()
            {
                foreach (CellInfo cellInfo in _cells)
                {
                    _target.SetInternalCellState(cellInfo.Cell, cellInfo.OldState);
                }

                _target.Render();

                return true;
            }

            public void Dispose()
            {
            }

            #endregion
        }

        class GenerationUndoUnit : IUndoUnit
        {
            #region Fields

            private GameField _target;

            private int _prevGeneration;
            private int _nextGeneration;
            private IDictionary<Cell, byte> _prevCells;
            private IDictionary<Cell, byte> _nextCells;

            #endregion

            #region Properties

            public int PrevGeneration
            {
                get
                {
                    return _prevGeneration;
                }
            }

            public int NextGeneration
            {
                get
                {
                    return _nextGeneration;
                }
            }

            public IDictionary<Cell, byte> PreviousCells
            {
                get
                {
                    return _prevCells;
                }
            }

            public IDictionary<Cell, byte> NextCells
            {
                get
                {
                    return _nextCells;
                }
            }

            #endregion

            #region Constructors

            public GenerationUndoUnit(GameField target, int prevGeneration, IDictionary<Cell, byte> prevCells, int nextGeneration, IDictionary<Cell, byte> nextCells)
            {
                _target = target;

                _prevGeneration = prevGeneration;
                _nextGeneration = nextGeneration;
                _prevCells = prevCells;
                _nextCells = nextCells;
            }

            #endregion

            #region Methods

            public bool Redo()
            {
                _target.SetGeneration(_nextCells, _nextGeneration);
                return true;
            }

            public bool Undo()
            {
                _target.SetGeneration(_prevCells, _prevGeneration);
                return true;
            }

            public void Dispose()
            {
            }

            #endregion
        }

        class SelectRegionUndoUnit : IUndoUnit
        {
            #region Fields

            private GameField _target;

            private Int32Rect _region1;
            private Int32Rect _region2;

            #endregion

            #region Constructors

            public SelectRegionUndoUnit(GameField target, Int32Rect region1, Int32Rect region2)
            {
                _target = target;
                _region1 = region1;
                _region2 = region2;
            }

            #endregion

            #region Methods

            public bool Redo()
            {
                _target._Select(_region1);
                return true;
            }

            public bool Undo()
            {
                _target._Select(_region2);
                return true;
            }

            public void Dispose()
            {
            }

            #endregion
        }

        class RotateAndFlipUndoUnit : IUndoUnit
        {
            #region Fields

            private GameField _target;

            private Delegate _doFunction;

            private Int32Rect _selectedRegion;

            private IDictionary<Cell, byte> _oldCells;

            #endregion

            #region Constructors

            public RotateAndFlipUndoUnit(GameField target, ThreadStart doFunction,
                Int32Rect selectedRegion, IDictionary<Cell, byte> oldCells)
            {
                _target = target;
                _doFunction = doFunction;
                _selectedRegion = selectedRegion;
                _oldCells = new Dictionary<Cell, byte>(oldCells);
            }

            #endregion

            #region Methods

            public bool Redo()
            {
                _doFunction.DynamicInvoke();
                return true;
            }

            public bool Undo()
            {
                _target._Delete(_selectedRegion);
                _target.SetInternalCellStates(_oldCells);

                return true;
            }

            public void Dispose()
            {
            }

            #endregion
        }

        class Rotate90UndoUnit : IUndoUnit
        {
            #region Fields

            private GameField _target;

            private bool _clockWiseRotate;

            private Int32Rect _selectedRegion;

            private Int32Rect _newSelectionRegion;

            private IDictionary<Cell, byte> _oldCells;

            #endregion

            #region Constructors

            public Rotate90UndoUnit(GameField target, bool clockWiseRotate,
                Int32Rect selectedRegion, Int32Rect newSelectionRegion, IDictionary<Cell, byte> oldCells)
            {
                _target = target;
                _clockWiseRotate = clockWiseRotate;
                _selectedRegion = selectedRegion;
                _newSelectionRegion = newSelectionRegion;
                _oldCells = new Dictionary<Cell, byte>(oldCells);
            }

            #endregion

            #region Methods

            public bool Redo()
            {
                if (_clockWiseRotate)
                {
                    _target._Rotate90CW();
                }
                else
                {
                    _target._Rotate90CCW();
                }

                return true;
            }

            public bool Undo()
            {
                _target._Select(_selectedRegion);
                _target._Delete(_newSelectionRegion);
                _target.SetInternalCellStates(_oldCells);

                return true;
            }

            public void Dispose()
            {
            }

            #endregion
        }

        class MethodUndoUnit : IUndoUnit
        {
            #region Fields

            private IInputElement _target;

            private Delegate _doFunction;
            private object _doParam = null;

            private Delegate _undoFunction;
            private object _undoParam = null;

            #endregion

            #region Constructors

            public MethodUndoUnit(IInputElement target, ThreadStart doFunction, ThreadStart undoFunction)
                : this(target, doFunction, null, undoFunction, null)
            {
            }

            public MethodUndoUnit(IInputElement target, ThreadStart doFunction, object doParam, ThreadStart undoFunction, object undoParam)
            {
                _target = target;
                _doFunction = doFunction;
                _doParam = doParam;
                _undoFunction = undoFunction;
                _undoParam = undoParam;
            }

            #endregion

            #region Methods

            public bool Redo()
            {
                _doFunction.DynamicInvoke();
                return true;
            }

            public bool Undo()
            {
                _undoFunction.DynamicInvoke();
                return true;
            }

            public void Dispose()
            {
            }

            #endregion
        }

        #endregion
    }
}
