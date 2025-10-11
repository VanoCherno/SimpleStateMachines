# SimpleStateMachines
Extendable and easy to use finite and hierarchical state machines to use in your personal project.
You can use it with Unity or .Net and extend or modify for all your needs and purposes.

# How To Use:
Every state has ```Id``` by which state machine will allow operations on states. It's a generic value so you can use what ever you like as an ```Id```.

To use any of state machines firstly you have to create an instance of ```ITransitionManager<TId>``` which stores all transitions and provides a quick way to check if a transition is possible.
```csharp
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
```
```csharp
public class OpenState<TId> : BaseDoorState<TId>
{
  public OpenState(TId id) : base(id) {}

  public override void Enter()
  {
    Debug.Log("door is open");
  }

  public override void Exit() {}
}
```
```csharp
public class ClosedState<TId> : BaseDoorState<TId>
{
  public ClosedState(TId id) : base(id) {}

  public override void Enter()
  {
    Debug.Log("door is closed");
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

    // switching to initial state
    m_stateMachine.ChangeState("closed_state");
  }

  private void Update()
  {
    m_stateMachine.TickTransitions();
  }
}
```

# Extending FiniteStateMachine:
Let's assume we need also to be able to update the door each frame.
To do this we should modify our door states, adding ```Tick(float deltaTime)``` to each state.

I will only show how ```BaseDoorState<TId>``` class changes.
```csharp
public abstract class BaseDoorState<TId> : BaseState<TId>
{
  protected BaseDoorState(TId id) : base(id) {}

  public abstract void Tick(float deltaTime);
}
```
Next we'll need to extend ```FiniteStateMachine<TId, TState>``` class to support this change. This class has ```protected TState ActiveState``` which we will use to access our current state.
```csharp
public class DoorStateMachine<TId> : FiniteStateMachine<TId, BaseDoorState<TId>>
{
  public void Tick(float deltaTime)
  {
    base.ActiveState.Tick(deltaTime);
  }
}
```
The ```Door``` class will chagne only a little:
```csharp
using UnityEngine;

public class Door : MonoBehaviour
{
  private DoorStateMachine<string> m_stateMachine; // here

  private void Awake()
  {
    m_stateMachine = new DoorStateMachine<string>(new TransitionManager()); // here

    // adding states
    m_stateMachine.AddState(new OpenState("open_state"));
    m_stateMachine.AddState(new ClosedState("closed_state"));

    // adding transitions between states
    m_stateMachine.Transitions.Add(new Transition<string>("open_state", "closed_state", () => Input.GetKeyDown(KeyCode.Space)));
    m_stateMachine.Transitions.Add(new Transition<string>("closed_state", "open_state", () => Input.GetKeyDown(KeyCode.Space)));

    // switching to initial state
    m_stateMachine.ChangeState("closed_state");
  }

  private void Update()
  {
    m_stateMachine.TickTransitions();
    m_stateMachine.Tick(Time.deltaTime); // and here
  }
}
```

# 2. Hierarchical State Machine:
Hierarchical state machine supports states inherited from ```BaseState<TId>``` so you can use your states from ```FiniteStateMachine<TId, TState>```.

But despite ```HierarchicalStateMachine<TId, TState>``` having some similarities in interface with ```FiniteStateMachine<TId, TState>```. They function differently and you need to handle inheritance from it differently.

Now let's add some more functionality to the ```Door```. Now the ```Door``` can be locked.
For this we will need a Hierarchical State Machine, since the door can only be locked when it's closed. So it makes sence to make ```LockedState<TId>``` a child of ```ClosedState<TId>```.
So we will need to transition our whole door system from using ```FiniteStateMachine<TId, TState>``` to ```HierarchicalStateMachine<TId, TState>```.

State hierarchy will look like this:
```
          Root
       /       \
OpenState    ClosedState
                   \
                 LockedState
```

Firstly we will create a new state for the door.
```csharp
public class LockedState<TId> : BaseDoorState<TId>
{
  public LockedState(TId id) : base(id) {}

  public override void Enter()
  {
    Debug.Log("door is locked");
  }

  public override void Exit()
  {
    Debug.Log("door is unlocked");
  }

  public override void Tick(float deltaTime) {}
}
```

Now we need another state to function as a root. It's not really functional so we will leave it empty.
```csharp
public class RootState<TId> : BaseDoorState<TId>
{
  public RootState(TId id) : base(id) {}

  public override void Enter() {}

  public override void Exit() {}

  public override void Tick(float deltaTime) {}
}
```

Now we will have to change the ```DoorStateMachine<TId>``` and transition to using ```HierarchicalStateMachine<TId, TState>```.
HSM does not have ```protected TState ActiveState```, instead it uses ```StateNode<TId, TState>``` as a wrapper around ```BaseState<TId>``` to allow tree-like structure of hierarchical state machines.
So we can use properties like ```protected StateNode<TId, TState> Root``` and ```protected StateNode<TId, TState> LowestActiveNode```.

We will modify our state machine to update states sequentially eg. before updating current state - update it's parent.
```csharp
public class DoorStateMachine<TId> : HierarchicalStateMachine<TId, BaseDoorState<TId>>
{
  public void Tick(float deltaTime)
  {
    // walking from Root to CurrentlyActive and updating all states we meet on the way
    StateNode<TId, BaseDoorState<TId>> node = base.Root;

    while(node != null)
    {
      node.State.Tick(deltaTime);
      node = node.Active; // "Active" is currently active child of the node
    }
  }
}
```

Only thing thats left is to change our ```Door``` class. Making door locked or unlocked when we press ```L``` button.
```csharp
using UnityEngine;

public class Door : MonoBehaviour
{
  private DoorStateMachine<string> m_stateMachine;

  private void Awake()
  {
    m_stateMachine = new DoorStateMachine<string>(new TransitionManager());

    // adding states
    m_stateMachine.AddState(new RootState("root_state"));
    m_stateMachine.AddState(new OpenState("open_state"));
    m_stateMachine.AddState(new ClosedState("closed_state"));
    m_stateMachine.AddState(new LockedState("locked_state"));

    // building state hierarchy
    m_stateMachine.SetRoot("root_state");
    m_stateMachine.MakeParent("open_state", "root_state");
    m_stateMachine.MakeParent("closed_state", "root_state");
    m_stateMachine.MakeParent("locked_state", "closed_state");

    // adding transitions between states
    m_stateMachine.Transitions.Add(new Transition<string>("open_state", "closed_state", () => Input.GetKeyDown(KeyCode.Space)));
    m_stateMachine.Transitions.Add(new Transition<string>("closed_state", "open_state", () => Input.GetKeyDown(KeyCode.Space)));
    m_stateMachine.Transitions.Add(new Transition<string>("closed_state", "locked_state", () => Input.GetKeyDown(KeyCode.L)));
    m_stateMachine.Transitions.Add(new Transition<string>("locked_state", "closed_state", () => Input.GetKeyDown(KeyCode.L)));

    // switching to initial state
    m_stateMachine.ChangeState("closed_state");
  }

  private void Update()
  {
    m_stateMachine.TickTransitions();
    m_stateMachine.Tick(Time.deltaTime);
  }
}
```

Well that's it, have fun making cool projects!
