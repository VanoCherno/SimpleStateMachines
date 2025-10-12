# SimpleStateMachines
Extendable and easy to use finite and hierarchical state machines to use in your projects.
Primarily designed for Unity but since it does not depend on it you can use it outside of Unity as well.

# Overview:
Every state has ```Id``` by which state machine will allow operations on states. It's a generic value so you can use what ever you like as an ```Id```.

You may use interfaces ```IStateMachine<TId, TState>```, ```IReadonlyStateMachine<TId, TState>```, ```IStateSwitcher<TId>``` to interact with state machines with different levels of access to their functionality.
You can change state of a state machine by calling ```ChangeState(TId id)``` method, use built-in transitions, or create your own. State machines do not depend on transitions, rather transition is a controlling module for changing state of a state machine.
So whether to use transitions, or make states responsible for state changing, or create a custom transitioning module, it's up to you.

Also both Finite and Hierarchical state machines accept ```BaseState<TId>```, it's done to provide reusability of states that already had been written for another type of state machine. If you would like to constrain your state to only be used in hierarchical state machine you can inherit from ```BaseHierarchicalState<TId>``` instead. Inside hierarchical state machine uses wrapper class ```StateNode<TId, TState>``` around ```BaseState<TId>``` to allow tree-like structure.

# 1. Finite State Machine:
Let's use ```FiniteStateMachine<TId, TState>``` to create a ```Door``` as an example. A door can be open or closed so we can use states to describe that.

First let's create states for our ```Door```. We will leave ```TId``` as a generic to be able to change it in the future.

Any state you create has to inherit from ```BaseState<TId>``` class to be used in a state machine.
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
    Console.WriteLine("door is open");
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
    Console.WriteLine("door is closed");
  }

  public override void Exit() {}
}
```
Now for the ```Door``` class. We'll use ```TriggeredTransition<TId>``` for controlling state flow of the state machine in this example.
```csharp
public class Door
{
  private FiniteStateMachine<string, BaseDoorState<string>> m_stateMachine;
  private TriggeredTransition<string> m_closedToOpenTransition;
  private TriggeredTransition<string> m_openToClosedTransition;

  public Door()
  {
    // instantiating state machine and transitions
    m_stateMachine = new FiniteStateMachine<string, BaseDoorState<string>>();
    m_closedToOpenTransition = new TriggeredTransition<string>(m_stateMachine, "closed_state", "open_state");
    m_openToClosedTransition = new TriggeredTransition<string>(m_stateMachine, "open_state", "closed_state");

    // adding states
    m_stateMachine.AddState(new OpenState("open_state"));
    m_stateMachine.AddState(new ClosedState("closed_state"));

    // switching to initial state
    m_stateMachine.ChangeState("closed_state");
  }

  public void Open()
  {
    m_closedToOpenTransition.Trigger();
  }

  public void Close()
  {
    m_openToClosedTransition.Trigger();
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
public class Door
{
  private DoorStateMachine<string, BaseDoorState<string>> m_stateMachine;
  private TriggeredTransition<string> m_closedToOpenTransition;
  private TriggeredTransition<string> m_openToClosedTransition;

  public Door()
  {
    // instantiating state machine and transitions
    m_stateMachine = new DoorStateMachine<string, BaseDoorState<string>>();
    m_closedToOpenTransition = new TriggeredTransition<string>(m_stateMachine, "closed_state", "open_state");
    m_openToClosedTransition = new TriggeredTransition<string>(m_stateMachine, "open_state", "closed_state");

    // adding states
    m_stateMachine.AddState(new OpenState("open_state"));
    m_stateMachine.AddState(new ClosedState("closed_state"));

    // switching to initial state
    m_stateMachine.ChangeState("closed_state");
  }

  public void Open()
  {
    m_closedToOpenTransition.Trigger();
  }

  public void Close()
  {
    m_openToClosedTransition.Trigger();
  }

  public void Tick(float deltaTime)
  {
    m_stateMachine.Tick(deltaTime);
  }
}
```

# 2. Hierarchical State Machine:
Hierarchical state machine supports states inherited from ```BaseState<TId>``` so you can we can use states from ```FiniteStateMachine<TId, TState>```.

But despite ```HierarchicalStateMachine<TId, TState>``` having some similarities in interface with ```FiniteStateMachine<TId, TState>```. They function differently and we need to handle inheritance from it differently.

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
    Console.WriteLine("door is locked");
  }

  public override void Exit()
  {
    Console.WriteLine("door is unlocked");
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

Now we will have to change the ```DoorStateMachine<TId>``` transitioning it to ```HierarchicalStateMachine<TId, TState>```.
HSM does not have ```protected TState ActiveState```, instead it uses ```StateNode<TId, TState>``` as a wrapper around ```BaseState<TId>``` to allow tree-like structure of hierarchical state machines.
We can use properties ```protected StateNode<TId, TState> Root``` and ```protected StateNode<TId, TState> LowestActiveNode```.

We will modify our class ```DoorStateMachine<TId>``` to update states sequentially eg. before updating current state - update it's parent.
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

Only thing thats left is to change the ```Door``` class. And now lets imagine the door uses a smart lock which will lock the door after 5 seconds of door being closed. And to unlock the door we will have to provide a password.
For these we will use ```TimedTransition<TId>``` and ```ConditionalTransition<TId>```.
```csharp
public class Door
{
  private DoorStateMachine<string, BaseDoorState<string>> m_stateMachine;
  private TriggeredTransition<string> m_closedToOpenTransition;
  private TriggeredTransition<string> m_openToClosedTransition;
  private TimedTransition<string> m_closedToLockedTransition;
  private ConditionalTransition<string> m_lockedToClosedTransition;

  private float m_closeDelaySeconds = 5f;
  private string m_password = "123";
  private string m_inputPassword;

  public Door()
  {
    // instantiating state machine and transitions
    m_stateMachine = new DoorStateMachine<string, BaseDoorState<string>>();
    m_closedToOpenTransition = new TriggeredTransition<string>(m_stateMachine, "closed_state", "open_state");
    m_openToClosedTransition = new TriggeredTransition<string>(m_stateMachine, "open_state", "closed_state");

    // TimedTransition will automatically start the timer when state machine changes state to state we transition from
    m_closedToLockedTransition = new TimedTransition<string>(m_stateMachine, "closed_state", "locked_state", m_closeDelaySeconds);

    // ConditionalTransition requires you to call TickCondition() method
    m_lockedToClosedTransition = new ConditionalTransition<string>(m_stateMachine, "locked_state", "closed_state", () => m_password == m_inputPassword);

    // adding states
    m_stateMachine.AddState(new OpenState("open_state"));
    m_stateMachine.AddState(new ClosedState("closed_state"));

    // switching to initial state
    m_stateMachine.ChangeState("closed_state");
  }

  public void Open()
  {
    m_closedToOpenTransition.Trigger();
  }

  public void Close()
  {
    m_openToClosedTransition.Trigger();
  }

  public void Tick(float deltaTime)
  {
    m_stateMachine.Tick(deltaTime);
    m_closedToLockedTransition.Tick(deltaTime); // ticking timer for the TimedTransition
  }

  public void InputPassword(string password)
  {
    m_inputPassword = password;
    m_lockedToClosedTransition.TickCondition(); // checking condition once after password is attempted
  }
}
```

Well that's it, have fun making cool projects!
