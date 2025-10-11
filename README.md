# SimpleStateMachines
Extendable and easy to use finite and hierarchical state machines to use in your personal project.
You can use it with Unity or .Net

# How To Use:
Every state has ```Id``` by which state machine will allow operations on states. It's a generic value so you can use what ever you like as an ```Id```.

To use any of state machines firstly you have to create an instance of ```ITransitionManager<TId>``` which stores all transitions and provides a quick way to check if a transition is possible.

```csharp
// Example on built-in transition manager
TransitionManager<string> transitions = new TransitionManager();
```

# 1. Finite State Machine:
Let's use ```FiniteStateMachine<TId, TState>``` to create a ```Door``` as an example.

First let's create states for our ```Door```. We will leave ```TId``` undefined to be able to easily change it later if such need shall arise.
```csharp
public class OpenState<TId> : BaseState<TId>
{
  public OpenState(TId id) : base(id) {}

  public override void Enter()
  {
    Console.WriteLine("door is open");
  }

  public override void Exit() {}
}

public class ClosedState<TId> : BaseState<TId>
{
  public ClosedState(TId id) : base(id) {}

  public override void Enter()
  {
    Console.WriteLine("door is closed");
  }

  public override void Exit() {}
}
```

```csharp
public class Door
{
  private FiniteStateMachine
}
```
