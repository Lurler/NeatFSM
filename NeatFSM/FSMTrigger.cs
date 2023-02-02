namespace NeatFSM;

public partial class FSMBuilder<TState, TEvent, TDataContainer>
{
    /// <summary>
    /// Trigger definition container.
    /// </summary>
    internal class FSMTrigger
    {
        internal readonly TState? State;
        internal readonly Func<bool> GuardClause;
        internal readonly FSMAction Action;

        // This constructor is used for the FSM builder and requires state info
        internal FSMTrigger(TState state, Func<bool> guardClause, FSMAction action)
        {
            State = state;
            GuardClause = guardClause;
            Action = action;
        }

        // This constructor is used for the actual FMS instance and does not require state info
        internal FSMTrigger(Func<bool> guardClause, FSMAction action)
        {
            GuardClause = guardClause;
            Action = action;
        }
    }
}