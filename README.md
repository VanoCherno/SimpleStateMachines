# SimpleStateMachines
Extendable and easy to use finite and hierarchical state machines to use in your personal project.
You can use it with Unity or .Net

# How To Use:
Every state has Id by which state machine will allow operations on states. It's a generic value so you can use what ever you like as an Id.
To use any of state machines firstly you have to create an instance of ITransitionManager<TId> which stores all transitions and provides a quick way to check if a transition is possible.
```csharp
// Example on built in transition manager
// Using string as id
TransitionManager<string> transitions = new TransitionManager();
```

# 1. Finite State Machine:
```csharp
public class Enemy
{
  StateMachine m_sm;
}
```
