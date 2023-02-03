# NeatFSM
This frameworks offers a simple FSM (finite state machine) implementation which you can use for your game's AI or possibly UI logic.

NeatFSM offers clean, minimalist, and fully documented API that is easy to learn and use.

## Features
* Super lightweight
* Support for generic "keys" for states and triggers (you can use any .NET type such as enums, strings, numbers, etc.)
* Various events for states: entry/exit/update (+high level OnTransition event for FSM as a whole)
* Guard clauses for triggers to support conditional transitions
* Export your FSM configuration to DOT graph (which can be visualized using services such as: https://edotor.net )
* Build support (you can configure FSM once with a builder and create any number of copies)
* Per-instance data container binding (e.g. to make your character accessible inside the FSM instance logic).

## Installation
Use provided nuget package or download the source.

:wrench: Nuget: `dotnet add package NeatFSM`

## Quick start
In the included Test project you can find a simple example of basic AI for an enemy. It waits until a player appears, then chases the player and when the player is nearby it will fire its weapon as long as it has ammo.
In a real game it will obviously be a bit more complex and the actual logic will be implemented **inside** the FSM trigger calbacks, rather than outside like in that example, but for a demo it should be enough to demonstrate basic usage without making it excessively complex.

Now, let's look at how to use the FSM.

First, you need to decide what `Type` you will be using for your states and events. It is recommended to use `Enums`, but you can also use strings, numbers, etc.
```csharp
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
```

You might also want to create a data container for your FSM instances, so the FSM is actually able to reference and manipulate the entity it is assigned to (e.g. an enemy inside the game).
```csharp
public class CustomDataContainer
{
	// this field will reference the enemy instance
	public EnemyClass enemyReference;
}
```

Next, create the `FSMBuilder`. There are two ways to do that: with and without data container.
```csharp
// create state machine builder with custom data container
FSMBuilder<States, Events, CustomContainer> fsmBuilder = new();

// or without
FSMBuilder<States, Events> fsmBuilder = new();
```

Now, define your FSM configuration using fluent interface approach.
```csharp
fsmBuilder.State(States.Wait) // returns given state
	.OnEnter(_ => Console.WriteLine("Starting: Wait")) //define events
	.OnLeave(_ => Console.WriteLine("Finished: Wait"))
	.OnUpdate(_ => Console.WriteLine("Now: Waiting"))
	.OnCommand(Events.PlayerFound, States.Chase); // define a trigger with simple transition

fsmBuilder.State(States.Chase)
	.OnEnter(_ => Console.WriteLine("Starting: Chase"))
	.OnLeave(_ => Console.WriteLine("Finished: Chase"))
	.OnUpdate(_ => Console.WriteLine("Now: Chasing"))
	.OnCommand(Events.PlayerNearby, States.Attack, HasAmmo) // add guard clause "HasAmmo" (it is a simple function that returns bool)
	// we can also switch the state manually or execute any arbitrary code
	// by accessing the fsm instance from inside callbacks
	// you can also access data container this way: fsm.Data
	.OnCommand(Events.PlayerLost, fsm => fsm.SwitchState(States.Wait));

fsmBuilder.State(States.Attack)
	.OnEnter(_ => Console.WriteLine("Starting: Attack"))
	.OnLeave(_ => Console.WriteLine("Finished: Attack"))
	.OnUpdate(_ => Console.WriteLine("Now: Attacking"))
	.OnCommand(Events.PlayerFar, States.Chase)
	.OnCommand(Events.PlayerLost, States.Wait);
```

Guard clauses are implemented as simple fuctions:
```csharp
private static bool HasAmmo()
{
	// add some proper logic here :)
	return true;
}
```

We can also define general `OnTransition` callback to monitor FSM behaviour if needed.
```csharp
// Generic on transition method can be defined for the whole FSM.
fsmBuilder.OnTransition = (fsm, from, to) =>
{
	Console.WriteLine($"OnTransition called. Changing \"{from}\" -> \"{to}\" inside `{fsm.Name}`.\n Also, custom data value is `{fsm.Data.customField}`.");
};
```

If we are creating a very complex FSM structure we can export it to DOT graph to visually examine how it looks.
```csharp
// Export to DOT graph
// It can be useful to visualize the state machine to confirm if it was setup correctly.
Console.WriteLine(fsmBuilder.ToDotGraph());
```

Finally, when we are done we can build an actual instance of runnable FSM. The second argument is the initial state of FSM (in this case `States.Wait`).
```csharp
// Build the FSM to be executed.
var fsmInstance = fsmBuilder.Build("Enemy FSM instance", States.Wait);
```

If we are using data container inside the FSM it is at this point we should populate it with data or define any references.
```csharp
// assign data to the container, it could be a reference to your game character for example
fsmInstance.Data.monsterReference = monsterInstance;
fsmInstance.Data.customField = "Yay, it's working!";
```

Finally, just run the FSM.
```csharp
// put this inside your update loop
fsmInstance.Update();
```

Or fire events from outside if needed
```csharp
fsmInstance.FireCommand(Events.PlayerFound);
```

Now, a bit more explanation is needed about binding your character to the FSM.

The idea is to pass the needed references or other data inside the FSM to be stored in a container. This way this data will be accessible directly to all functions assigned to FSM actions.

Here's an example how it can be done:
```csharp
// create an instance as usual
var fsmInstance = fsmBuilder.Build("Enemy FSM instance", States.Wait);

// and then bind your character to the fsm
// you will probably create an instance of the FSM inside the character constructor, hence "this"
fsmInstance.Data.character = this;

// also bind some other data (just an example)
fsmInstance.Data.gameMap = GameWorld.currentMap;
```

To use this data container functionality you will need to create your own class with the fields you need. Usually you will only need to have a field to store a reference to your game character (enemy, etc.).

Also, as mentioned in the overview if you would like to use the FSM **without** the data container - use this constructor instead:
```csharp
FSMBuilder<States, Events> fsmBuilder = new();
```

All instances of this FSM will have empty data container with no fields you can assign data to.

## Changes
 - v1.0 - Initial release.

## Contribution
Contributions are welcome!

You can start with submitting an [issue on GitHub](https://github.com/Lurler/NeatFSM/issues).

## License
**NeatFSM** is released under the [MIT License](../master/LICENSE).