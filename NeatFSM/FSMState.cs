namespace NeatFSM;

public partial class FSMBuilder<TState, TEvent, TDataContainer>
{
    /// <summary>
    /// State of a FSM.
    /// </summary>
    public class FSMState
    {
        /// <summary>
        /// Reference to the parent FSM instance.
        /// </summary>
        public readonly FSM FSM;

        /// <summary>
        /// Current key that corresponds to this state instance.
        /// </summary>
        private readonly TState Key;

        // Default actions
        internal FSMAction? OnEnterAction;
        internal FSMAction? OnLeaveAction;
        internal FSMAction? OnUpdateAction;

        // List of defined command actions
        internal readonly Dictionary<TEvent, FSMTrigger> triggers = new();

        public FSMState(FSM fsm, TState key)
        {
            Key = key;
            FSM = fsm;
        }

        internal void FireCommand(TEvent command)
        {
            // fsm does not allow commands that are not registered explicitly
            if (!triggers.ContainsKey(command))
                throw new InvalidOperationException($"Command `{command}` has not been defined for state `{Key}` in FSM `{FSM.Name}`.");

            // check for guard clause (or if it's empty it will always return true)
            if (triggers[command].GuardClause.Invoke())
            {
                // invoke corresponding action for this command
                triggers[command].Action.Invoke(FSM);
            }
        }

        /// <summary>
        /// Simply executes the update action if it is assigned.
        /// </summary>
        internal void Update()
        {
            OnUpdateAction?.Invoke(FSM);
        }

    }

}