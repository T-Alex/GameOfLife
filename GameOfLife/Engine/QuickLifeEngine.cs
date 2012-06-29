using System;
using System.Collections.Generic;

namespace TAlex.GameOfLife.Engine
{
    public class QuickLifeEngine : LifeEngine
    {
        #region Field

        private const int DefaultCapacity = 1000000;
        private const int LabelLiveCell = 100;

        private Dictionary<Cell, byte> _aliveCells;

        private List<Cell> _candidateCells;

        private Dictionary<Cell, byte> _cells;

        private LifeRule _rule = LifeRule.StandardLifeRule;

        #endregion

        #region Properties

        public override byte this[int x, int y]
        {
            get
            {
                Cell cell = new Cell(x, y);
                byte value;
                _aliveCells.TryGetValue(cell, out value);
                return value;
            }

            set
            {
                Cell cell = new Cell(x, y);

                if (value == 0)
                {
                    _cells.Remove(cell);
                    _aliveCells.Remove(cell);
                }
                else
                {
                    _cells[cell] = LabelLiveCell;
                    _aliveCells[cell] = 1;
                }
            }
        }

        public Dictionary<Cell, byte> Cells
        {
            get
            {
                return _cells;
            }
        }

        public Dictionary<Cell, byte> AliveCells
        {
            get
            {
                return _aliveCells;
            }
        }

        public override int Population
        {
            get
            {
                return _aliveCells.Count;
            }
        }

        public LifeRule Rule
        {
            get
            {
                return _rule;
            }

            set
            {
                _rule = value;
            }
        }

        #endregion

        #region Constructors

        public QuickLifeEngine()
        {
            Initialize();
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            _cells = new Dictionary<Cell, byte>(DefaultCapacity);
            _aliveCells = new Dictionary<Cell, byte>(DefaultCapacity);
            _candidateCells = new List<Cell>(DefaultCapacity);
        }

        public void Initialize(IDictionary<Cell, byte> cells)
        {
            Initialize();
            
            foreach (KeyValuePair<Cell, byte> cellPair in cells)
            {
                this[cellPair.Key.X, cellPair.Key.Y] = cellPair.Value;
            }
        }

        public void Clear()
        {
            _cells.Clear();
            _aliveCells.Clear();
            _candidateCells.Clear();
        }

        public override bool NextGeneration()
        {
            lock (this)
            {
                // Stage I (Creating a list of candidate cells, define the number of neighboring cells)
                _candidateCells.Clear();

                foreach (Cell currentCell in _aliveCells.Keys)
                {
                    int col = currentCell.X;
                    int row = currentCell.Y;

                    _candidateCells.Add(currentCell);

                    for (int x = col - 1; x <= col + 1; x++)
                    {
                        for (int y = row - 1; y <= row + 1; y++)
                        {
                            if (x != col || y != row)
                            {
                                Cell neighborCell = new Cell(x, y);
                                byte neighborCellValue;

                                if (_cells.TryGetValue(neighborCell, out neighborCellValue))
                                {
                                    if (neighborCellValue >= LabelLiveCell)
                                        _cells[currentCell]++;
                                    else
                                        _cells[neighborCell]++;
                                }
                                else
                                {
                                    _cells.Add(neighborCell, 1);
                                    _candidateCells.Add(neighborCell);
                                }
                            }
                        }
                    }
                }


                // Stage II (define a new generation of living cells)
                _aliveCells.Clear();

                foreach (Cell currentCell in _candidateCells)
                {
                    byte state = _cells[currentCell];

                    bool dead = true;

                    if (state < LabelLiveCell)
                    {
                        for (int i = 0; i < _rule.BN; i++)
                        {
                            if (state == _rule.B[i])
                            {
                                _aliveCells.Add(currentCell, (byte)(state + 1));
                                _cells[currentCell] = LabelLiveCell;
                                dead = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        int neighborsCount = state - LabelLiveCell;

                        for (int i = 0; i < _rule.SN; i++)
                        {
                            if (neighborsCount == _rule.S[i])
                            {
                                _aliveCells.Add(currentCell, (byte)(neighborsCount + 10));
                                _cells[currentCell] = LabelLiveCell;
                                dead = false;
                                break;
                            }
                        }
                    }

                    if (dead)
                        _cells.Remove(currentCell);
                }

                return IsEmpty;
            }
        }

        #region Helpers

        internal byte GetInternalCellState(Cell cell)
        {
            byte state;
            _aliveCells.TryGetValue(cell, out state);
            return state;
        }

        internal void SetInternalCellState(Cell cell, byte state)
        {
            if (state == 0)
            {
                _cells.Remove(cell);
                _aliveCells.Remove(cell);
            }
            else
            {
                _cells[cell] = LabelLiveCell;
                _aliveCells[cell] = state;
            }
        }

        #endregion

        #endregion
    }
}