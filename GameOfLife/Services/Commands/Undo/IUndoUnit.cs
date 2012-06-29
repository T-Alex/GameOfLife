using System;

namespace TAlex.Services.Commands.Undo
{
    /// <summary>
    /// Implements a unit that can be undone and redone.
    /// </summary>
    public interface IUndoUnit : IDisposable
    {
        /// <summary>
        /// Redoes the change that is represented by this unit.
        /// </summary>
        /// <returns>true if the operation completed successfully; otherwise false.</returns>
        bool Redo();

        /// <summary>
        /// Undoes the change that is represented by this unit.
        /// </summary>
        /// <returns>true if the operation completed successfully; otherwise false.</returns>
        bool Undo();
    }
}
