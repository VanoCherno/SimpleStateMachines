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

Any state you create has to inherit from ```BaseState<TId>``` class.
```csharp
public abstract class BaseDoorState<TId> : BaseState<TId>
{
  protected BaseDoorState(TId id) : base(id) {}
}

public class OpenState<TId> : BaseDoorState<TId>
{
  public OpenState(TId id) : base(id) {}

  public override void Enter()
  {
    Console.WriteLine("door is open");
  }

  public override void Exit() {}
}

public class ClosedState<TId> : BaseDoorState<TId>
{
  public ClosedState(TId id) : base(id) {}

  public override void Enter()
  {
    Console.WriteLine("door is closed");
  }

  public override void Exit() {}
}
```

Now the ```Door``` class which will open whenever we press ```Space```.

```csharp
using UnityEngine;

public class Door : MonoBehaviour
{
  private FiniteStateMachine<string, BaseDoorState<string>> m_stateMachine;

  private void Awake()
  {
    m_stateMachine = new FiniteStateMachine<string, BaseDoorState<string>>(new TransitionManager());

    // adding states
    m_stateMachine.AddState(new OpenState("open_state"));
    m_stateMachine.AddState(new ClosedState("closed_state"));

    // adding transitions between states
    m_stateMachine.Transitions.Add(new Transition<string>("open_state", "closed_state", () => Input.GetKeyDown(KeyCode.Space)));
    m_stateMachine.Transitions.Add(new Transition<string>("closed_state", "open_state", () => Input.GetKeyDown(KeyCode.Space)));
  }

  private void Update()
  {
    m_stateMachine.TickTransitions();
  }
}
```
