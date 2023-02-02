using System.Text;

namespace NeatFSM;

/// <summary>
/// Finite State Machine builder class. Use it to build your FSM instances.
/// This version does not requires a data container type.
/// </summary>
/// <typeparam name="TState">Defines a type that will be used to represent states (usually enum or string)</typeparam>
/// <typeparam name="TEvent">Defines a type that will be used to represent events (usually enum or string)</typeparam>
public partial class FSMBuilder<TState, TEvent>
    : FSMBuilder<TState, TEvent, FSMEmptyDataContainer>
    where TState : notnull, IComparable
    where TEvent : notnull, IComparable
{

}

/// <summary>
/// Finite State Machine builder class. Use it to build your FSM instances.
/// Requires a data container type.
/// </summary>
/// <typeparam name="TState">Defines a type that will be used to represent states (usually enum or string)</typeparam>
/// <typeparam name="TEvent">Defines a type that will be used to represent events (usually enum or string)</typeparam>
/// <typeparam name="TDataContainer">Defines a type that will be used for data container of this FSM.</typeparam>
public partial class FSMBuilder<TState, TEvent, TDataContainer>
    where TState : notnull, IComparable
    where TEvent : notnull, IComparable
    where TDataContainer : new()
{
    /// <summary>
    /// Default delegate for FSM actions.
    /// </summary>
    public delegate void FSMAction(FSM fsm);

    /// <summary>
    /// Default delegate for FSM transition actions.
    /// </summary>
    public delegate void FSMTransitionAction(FSM fsm, TState from, TState to);

    // state data
    private readonly Dictionary<TState, FSMBuilderState> states = new();

    /// <summary>
    /// Generic transition action (fired any time transition happens).
    /// </summary>
    public FSMTransitionAction? OnTransition;

    /// <summary>
    /// Returns how many states have been registered.
    /// </summary>
    public int Count => states.Count;

    /// <summary>
    /// Creates a new state (if it has not been defined yet) and returns a handle to work
    /// with this state.
    /// </summary>
    public FSMBuilderState State(TState state)
    {
        // add a state if it hasn't been added yet
        if (!states.ContainsKey(state))
        {
            states.Add(state, new(this, state));
        }

        // return it to continue working with it
        return states[state];
    }

    /// <summary>
    /// Returns a collection of all triggers that are defined for at least one state of the FSM.
    /// </summary>
    public IReadOnlyCollection<TEvent> DefinedTriggers()
    {
        HashSet<TEvent> result = new();

        foreach (var item in states.Values)
        {
            result.UnionWith(item.DefinedTriggers());
        }

        return result;
    }

    /// <summary>
    /// Creates a DOT graph of the current FSM for visualization purposes.
    /// </summary>
    public string ToDotGraph()
    {
        StringBuilder result = new();
        result.Append("digraph FSM { \n");

        // work on individual states
        foreach (var (stateKey, stateInstance) in states)
        {
            foreach (var trigger in stateInstance.triggers.Values)
            {
                result.Append($"    {stateKey} -> {trigger.State} ");
                if (EqualityComparer<TState>.Default.Equals(stateKey, trigger.State))
                {
                    result.Append("[label= \"Custom logic\"]");
                }
                result.Append('\n');
            }
        }

        result.Append('}');
        return result.ToString();
    }

    /// <summary>
    /// Builds FSM and creates a copy that can be run.
    /// </summary>
    public FSM Build(string fsmName, TState initialState)
    {
        // create runnable fsm
        var fsm = new FSM(fsmName, initialState);

        // assign OnTransition action
        fsm.OnTransition = this.OnTransition;

        // now create states and assign their actions and triggers as well
        foreach (var (stateKey, stateInstance) in states)
        {
            // first, create state instance
            var state = new FSMState(fsm, stateKey);
            state.OnUpdateAction = stateInstance.OnUpdateAction;
            state.OnEnterAction = stateInstance.OnEnterAction;
            state.OnLeaveAction = stateInstance.OnLeaveAction;

            // then define all triggers for it
            foreach (var (triggerKey, triggerInstance) in stateInstance.triggers)
            {
                state.triggers.Add(triggerKey, new(triggerInstance.GuardClause, triggerInstance.Action));
            }

            // then add this newly created state to the instance of runnable fsm
            fsm.states.Add(stateKey, state);
        }

        // return created fsm instance (which can now be run by the user)
        return fsm;
    }

}

