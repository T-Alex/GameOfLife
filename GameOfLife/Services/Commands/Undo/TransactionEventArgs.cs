using System;

namespace TAlex.Services.Commands.Undo
{
    public enum TransactionEventArgs
    {
        Commit,
        CommitUndo,
        CommitRedo,
        Rollback
    }
}
