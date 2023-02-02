using NeatFSM;

namespace FSMTest;

public class CustomDataContainer
{
    public string customField;
}

public class Program
{
    enum States
    {
        Wait,
        Chase,
        Attack,
    }

    enum Events
    {
        PlayerFound,
        PlayerNearby,
        PlayerFar,
        PlayerLost,
    }

    static void Main(string[] args)
    {
        // create state machine builder
        FSMBuilder<States, Events, CustomDataContainer> fsmBuilder = new();

        fsmBuilder.State(States.Wait) // returns given state
                .OnEnter(_ => Console.WriteLine("Starting: Wait")) //define events
                .OnLeave(_ => Console.WriteLine("Finished: Wait"))
                .OnUpdate(_ => Console.WriteLine("Now: Waiting"))
                .OnCommand(Events.PlayerFound, States.Chase);

        fsmBuilder.State(States.Chase)
                .OnEnter(_ => Console.WriteLine("Starting: Chase"))
                .OnLeave(_ => Console.WriteLine("Finished: Chase"))
                .OnUpdate(_ => Console.WriteLine("Now: Chasing"))
                .OnCommand(Events.PlayerNearby, States.Attack, HasAmmo) // guard clause
                // we can also switch the state manually or execute any arbitrary code
                // by accessing the data container (which also provides a reference to
                // the current instance of FSM).
                .OnCommand(Events.PlayerLost, fsm => fsm.SwitchState(States.Wait));

        fsmBuilder.State(States.Attack)
                .OnEnter(_ => Console.WriteLine("Starting: Attack"))
                .OnLeave(_ => Console.WriteLine("Finished: Attack"))
                .OnUpdate(_ => Console.WriteLine("Now: Attacking"))
                .OnCommand(Events.PlayerFar, States.Chase)
                .OnCommand(Events.PlayerLost, States.Wait);

        // Generic on transition method can be defined for the whole FSM.
        fsmBuilder.OnTransition = (fsm, from, to) =>
        {
            Console.WriteLine($"OnTransition called. Changing \"{from}\" -> \"{to}\" inside `{fsm.Name}`.\n Also, custom data value is `{fsm.Data.customField}`.");
        };

        // Export to DOT graph
        // It can be useful to visualize the state machine to confirm if it was setup correctly.
        Console.WriteLine(fsmBuilder.ToDotGraph());

        // Build the FSM to be executed.
        var fsmInstance = fsmBuilder.Build("Enemy FSM instance", States.Wait);

        // assign data to the container, it could be a reference to your game character for example
        fsmInstance.Data.customField = "Yay, it's working!";

        // Now execute example program
        fsmInstance.FireCommand(Events.PlayerFound);
        fsmInstance.FireCommand(Events.PlayerLost);
        fsmInstance.FireCommand(Events.PlayerFound);
        fsmInstance.FireCommand(Events.PlayerNearby);
        fsmInstance.FireCommand(Events.PlayerFar);
        fsmInstance.FireCommand(Events.PlayerNearby);
    }

    private static bool HasAmmo()
    {
        // just return true, since it is a demo :)
        return true;
    }
}