using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ei8.Cortex.Diary.Plugins.Kanban
{
    public enum ContextMenuOption
    {
        NotSet,
        New,
        Edit,
        Delete,
        AddRelative
    }

    public struct ErrorMessage
    {
        public const string MissingExternalInstantiatesTask = "Required External Reference 'Instantiates, Task' was not found.";
        public const string MissingExternalHasStatusOfBacklog = "Required External Reference 'Has Status Of, Backlog' was not found.";
        public const string MissingExternalHasStatusOfPrioritized = "Required External Reference 'Has Status Of, Prioritized' was not found.";
        public const string MissingExternalHasStatusOfInProgress = "Required External Reference 'Has Status Of, Inprogress' was not found.";
        public const string MissingExternalHasStatusOfDone = "Required External Reference 'Has Status Of, Done' was not found.";
    }
}
