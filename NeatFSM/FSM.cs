namespace NeatFSM;

public partial class FSMBuilder<TState, TEvent, TDataContainer>
{
    /// <summary>
    /// Runnable version of FSM.
    /// </summary>
    public class FSM
    {
        // state data
        internal readonly Dictionary<TState, FSMState> states = new();

        /// <summary>
        /// Name of the current instance of FSM.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Current state of the FSM.
        /// </summary>
        public TState CurrentStateKey { get; internal set; }

        /// <summary>
        /// Reference to the data container of this FSM.
        /// </summary>
        public TDataContainer Data { get; }

        // auto properties
        protected FSMState? CurrentState =>
            CurrentStateKey is not null && states.ContainsKey(CurrentStateKey)
                ? states[CurrentStateKey]
                : null;

        /// <summary>
        /// How many states are registered in this FSM.
        /// </summary>
        public int Count => states.Count;

        // general transition action
        internal FSMTransitionAction? OnTransition;

        internal FSM(string fsmName, TState initialState)
        {
            this.Name = fsmName;
            this.CurrentStateKey = initialState;
            this.Data = new TDataContainer();
        }

        /// <summary>
        /// Updates the FSM which will also call OnUpdate action defined for the current state.
        /// </summary>
        public void Update()
        {
            CurrentState?.Update();
        }

        /// <summary>
        /// Fires a command into the FSM. Commands must be registered beforehand.
        /// </summary>
        public void FireCommand(TEvent command)
        {
            CurrentState?.FireCommand(command);
        }

        /// <summary>
        /// Switches the state manually. Normally state switching should only
        /// be done inside the callback function (such as OnUpdate).
        /// </summary>
        public void SwitchState(TState newState)
        {
            // invoke general transition action if it is defined
            OnTransition?.Invoke(this, CurrentStateKey, newState);

            // invoke OnLeave on the current state first
            if (CurrentStateKey is not null && states.ContainsKey(CurrentStateKey))
                states[CurrentStateKey].OnLeaveAction?.Invoke(this);

            // set new state
            CurrentStateKey = newState;

            // invoke OnEnter for this new state
            states[CurrentStateKey].OnEnterAction?.Invoke(this);
        }

    }
}