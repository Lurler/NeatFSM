namespace NeatFSM;

public partial class FSMBuilder<TState, TEvent, TDataContainer>
{
    /// <summary>
    /// State of a FSM.
    /// </summary>
    public class FSMBuilderState
    {
        /// <summary>
        /// Reference to the parent FSM.
        /// </summary>
        internal readonly FSMBuilder<TState, TEvent, TDataContainer> FSM;

        /// <summary>
        /// Current key that corresponds to this state instance.
        /// </summary>
        internal readonly TState Key;

        // Default actions
        internal FSMAction? OnEnterAction;
        internal FSMAction? OnLeaveAction;
        internal FSMAction? OnUpdateAction;

        // List of defined command actions
        internal readonly Dictionary<TEvent, FSMTrigger> triggers = new();

        internal FSMBuilderState(FSMBuilder<TState, TEvent, TDataContainer> fsm, TState key)
        {
            Key = key;
            FSM = fsm;
        }

        /// <summary>
        /// Allows defining a callback which will be fired when entering this state.
        /// </summary>
        public FSMBuilderState OnEnter(FSMAction value)
        {
            OnEnterAction = value;
            return this;
        }

        /// <summary>
        /// Allows defining a callback which will be fired when leaving this state.
        /// </summary>
        public FSMBuilderState OnLeave(FSMAction value)
        {
            OnLeaveAction = value;
            return this;
        }

        /// <summary>
        /// Allows defining a callback which will be fired every time <see cref="FSM{TState, TEvent, TDataContainer}.Update()" /> is called.
        /// </summary>
        public FSMBuilderState OnUpdate(FSMAction value)
        {
            OnUpdateAction = value;
            return this;
        }

        /// <summary>
        /// Creates a trigger entry to store reactions to events.
        /// </summary>
        private void CreateTrigger(TEvent command, TState newState, Func<bool> guardClause, FSMAction action)
        {
            if (triggers.ContainsKey(command))
                throw new ArgumentException($"Trigger `{command}` for state '{Key}' is already defined.");

            // create new entry if it doesn't exists
            triggers.Add(command, new(newState, guardClause, action));
        }

        /// <summary>
        /// Implements basic transition to a new state based on command given.
        /// </summary>
        public FSMBuilderState OnCommand(TEvent command, TState state)
        {
            CreateTrigger(command, state, () => true, fsm => fsm.SwitchState(state));
            return this;
        }

        /// <summary>
        /// Implements transition to a new state with an additional Guard clause.
        /// </summary>
        public FSMBuilderState OnCommand(TEvent command, TState state, Func<bool> guardClause)
        {
            CreateTrigger(command, state, guardClause, fsm => fsm.SwitchState(state));
            return this;
        }

        /// <summary>
        /// Allows to run custom logic when a particular event is fired.
        /// </summary>
        public FSMBuilderState OnCommand(TEvent command, FSMAction action)
        {
            CreateTrigger(command, Key, () => true, action);
            return this;
        }

        /// <summary>
        /// Returns a list of all defined triggers for the current state.
        /// </summary>
        public IReadOnlyCollection<TEvent> DefinedTriggers()
        {
            return triggers.Keys;
        }

    }
}