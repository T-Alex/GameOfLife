using System;
using System.Collections.Generic;
using System.Text;

namespace TAlex.Services.Commands.Undo
{
    /// <summary>
    /// Represents manager of the undo/redo commands.
    /// </summary>
    public class UndoManager
    {
        #region Fields

        public const int DefaultUndoLimit = -1;

        public delegate void TransactionEventHandler(object sender, TransactionEventArgs args);
        public event TransactionEventHandler Transaction;

        private UndoState _state = UndoState.Normal;
        private int _undoLimit = DefaultUndoLimit;

        private int _undoCount = 0;
        private int _redoCount = 0;
        private List<IUndoUnit> _undoStack = new List<IUndoUnit>();
        private Stack<IUndoUnit> _redoStack = new Stack<IUndoUnit>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value that indicates whether the most recent action can be undone.
        /// </summary>
        public bool CanUndo
        {
            get
            {
                return UndoCount > 0;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the most recent undo action can be redone.
        /// </summary>
        public bool CanRedo
        {
            get
            {
                return RedoCount > 0;
            }
        }

        /// <summary>
        /// Gets or sets the number of actions stored in the undo queue.
        /// </summary>
        public int UndoLimit
        {
            get
            {
                return _undoLimit;
            }

            set
            {
                _undoLimit = value;
            }
        }

        /// <summary>
        /// Gets the current state of the undo/redo manager.
        /// </summary>
        public UndoState State
        {
            get
            {
                return _state;
            }

            protected set
            {
                _state = value;
            }
        }

        /// <summary>
        /// Gets the number of undo actions available.
        /// </summary>
        public int UndoCount
        {
            get
            {
                return _undoCount;
            }
        }

        /// <summary>
        /// Gets the number of redo actions available.
        /// </summary>
        public int RedoCount
        {
            get
            {
                return _redoCount;
            }
        }

        protected int TopUndoIndex
        {
            get
            {
                return _undoStack.Count - 1;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TAlex.GameOfLife.Controls.UndoManager"/> class.
        /// </summary>
        public UndoManager()
        {
            Reset();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the undo unit to the undo stack.
        /// </summary>
        /// <param name="unit">A unit that can be undone and redone.</param>
        public void Add(IUndoUnit unit)
        {
            // If the number of undo units exceeds the limit then remove the first
            if (UndoCount > 0 && (UndoLimit != -1 && UndoCount >= UndoLimit))
            {
                _undoCount--;

                if (_undoStack[0] == null)
                    _undoStack.RemoveAt(0);

                while (_undoStack.Count > 0 && _undoStack[0] != null)
                {
                    _undoStack.RemoveAt(0);
                }
            }

            // Add undo unit to stack
            _undoStack.Add(unit);
            _redoStack.Clear();
            _redoCount = 0;
        }

        /// <summary>
        /// Undoes the most recent undo command. In other words, undoes the most recent undo unit on the undo stack.
        /// </summary>
        public void Undo()
        {
            Undo(1, false);
        }

        public void Undo(int count)
        {
            Undo(count, false);
        }

        public void Undo(int count, bool idleRun)
        {
            if (_undoStack.Count < 2)
                return;

            State = UndoState.Undo;

            for (int i = 0; i < count; i++)
            {
                // If the transaction is completed, remove the control point
                if (PeekUndoStack() == null)
                    PopUndoStack();

                _redoStack.Push(null);

                _redoCount++;
                _undoCount--;
                while (PeekUndoStack() != null)
                {
                    IUndoUnit unit = PopUndoStack();
                    _redoStack.Push(unit);
                    
                    if (!idleRun) unit.Undo();
                }
            }

            if (Transaction != null)
                Transaction(this, TransactionEventArgs.CommitUndo);

            State = UndoState.Normal;
        }

        /// <summary>
        /// Undoes the most recent undo command. In other words, redoes the most recent undo unit on the undo stack.
        /// </summary>
        public void Redo()
        {
            Redo(1);
        }

        public void Redo(int count)
        {
            if (_redoStack.Count == 0)
                return;

            State = UndoState.Redo;

            for (int i = 0; i < count; i++)
            {
                // If the transaction is not completed, complete it
                if (PeekUndoStack() != null)
                    _undoStack.Add(null);

                while (_redoStack.Peek() != null)
                {
                    IUndoUnit unit = _redoStack.Pop();
                    _undoStack.Add(unit);

                    if (!unit.Redo())
                    {
                        Rollback();
                        return;
                    }
                }

                _redoCount--;
                _undoCount++;
                _redoStack.Pop();
                _undoStack.Add(null);
            }

            if (Transaction != null)
                Transaction(this, TransactionEventArgs.CommitRedo);

            State = UndoState.Normal;
        }

        /// <summary>
        /// Attempts to commit the transaction.
        /// </summary>
        public void Commit()
        {
            object unit = PeekUndoStack();

            if (unit != null)
            {
                _undoCount++;
                _undoStack.Add(null);

                if (Transaction != null)
                    Transaction(this, TransactionEventArgs.Commit);
            }
        }

        /// <summary>
        /// Rolls back (aborts) the transaction.
        /// </summary>
        public void Rollback()
        {
            State = UndoState.Rollback;

            while (PeekUndoStack() != null)
            {
                IUndoUnit unit = PopUndoStack();
                unit.Undo();
            }

            if (Transaction != null)
                Transaction(this, TransactionEventArgs.Rollback);

            State = UndoState.Normal;
        }

        /// <summary>
        /// Clear the entire undo and redo stacks.
        /// </summary>
        public void Reset()
        {
            _undoCount = 0;
            _redoCount = 0;

            _undoStack.Clear();
            _undoStack.Add(null); // Initial Check Point
            _redoStack.Clear();
        }

        #region Helpers

        private IUndoUnit PeekUndoStack()
        {
            if ((TopUndoIndex >= 0) && (TopUndoIndex != _undoStack.Count))
            {
                return _undoStack[TopUndoIndex];
            }

            return null;
        }

        private IUndoUnit PopUndoStack()
        {
            IUndoUnit unit = _undoStack[TopUndoIndex];
            _undoStack.RemoveAt(TopUndoIndex);

            return unit;
        }

        #endregion

        #endregion
    }
}
