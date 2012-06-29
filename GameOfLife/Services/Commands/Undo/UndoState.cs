using System;

namespace TAlex.Services.Commands.Undo
{
    public enum UndoState
    {
        Normal,
        Undo,
        Redo,
        Rollback
    }
}
