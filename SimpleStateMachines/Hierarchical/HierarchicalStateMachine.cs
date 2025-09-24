using System;
using System.Collections.Generic;

namespace SimpleStateMachines.Hierarchical
{
    public class HierarchicalStateMachine<TId, TState> : IStateTree<TId, TState>, IStateMachine<TId, TState>
        where TState : class, IHierarchicalState<TId, TState>
    {
        private readonly Dictionary<TId, List<ITransition<TId>>> m_transitions;
        private TState m_rootState;
        private TState m_currentState;
        private TId m_previousStateId;

        public HierarchicalStateMachine()
        {
            m_transitions = new Dictionary<TId, List<ITransition<TId>>>();
        }
        
        public TId PreviousStateId => m_previousStateId;
        public TId CurrentStateId => (m_currentState != null) ? m_currentState.Id : default;
        protected TState CurrentState => m_currentState;
        protected TState Root => m_rootState;

        public event Action OnStateChanged;

        public void AddTransition(ITransition<TId> transition)
        {
            if (m_transitions.TryGetValue(transition.From, out List<ITransition<TId>> transitions))
                transitions.Add(transition);
            else
                m_transitions.Add(transition.From, new List<ITransition<TId>> { transition });
        }

        public void RemoveTransition(ITransition<TId> transition)
        {
            if (m_transitions.TryGetValue(transition.From, out List<ITransition<TId>> transitions))
                transitions.Remove(transition);
        }

        /// <summary>Provides list of transitions from a given state, if state has no transitions returns empty list</summary>
        public IReadOnlyList<ITransition<TId>> GetTransitionsFromState(TId id)
        {
            if (m_transitions.TryGetValue(id, out List<ITransition<TId>> transitions))
                return transitions.AsReadOnly();

            return new List<ITransition<TId>>().AsReadOnly();
        }

        public void RemoveAllTransitionsFromState(TId id)
        {
            m_transitions.Remove(id);
        }

        public void ClearTransitions()
        {
            m_transitions.Clear();
        }

        /// <summary>Sets and enters root state, parent of root must be always set to null</summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void SetRootState(TState root)
        {
            if (root == null)
                throw new ArgumentNullException($"{root}");

            if (root.Parent != null)
                throw new ArgumentException("The root state must have no parent.");

            m_rootState = root;
            m_currentState = root;
            m_currentState.Enter();
            OnStateChanged?.Invoke();
        }

        /// <summary>Exits all active states including root, removes root. After this the state tree will be empty</summary>
        public void RemoveRootState()
        {
            if (m_rootState == null)
                return;

            TryChangeState(m_currentState, m_rootState);
            m_rootState.Exit();
            m_rootState = null;
            m_currentState = null;
            m_previousStateId = default;
            OnStateChanged?.Invoke();
        }

        /// <summary>Checks all transitions from the active state, if a transition should be made changes state</summary>
        public void TickTransitions()
        {
            List<ITransition<TId>> transitions;

            if (m_currentState == null)
                return;

            if (!m_transitions.TryGetValue(m_currentState.Id, out transitions))
                return;

            for (int i = 0; i < transitions.Count; i++)
            {
                if (!transitions[i].ShouldTransition())
                    continue;

                if (TryChangeState(transitions[i].To))
                    return;
            }
        }

        /// <summary>Checks if state is currently active lowest state</summary>
        public bool IsInState(TId id)
        {
            if (m_currentState == null)
                return false;

            return EqualityComparer<TId>.Default.Equals(m_currentState.Id, id);
        }

        public bool IsStateActiveInHierarchy(TId id)
        {
            return IsStateActiveInHierarchy(GetStateById(id));
        }

        public bool IsStateActiveInHierarchy(TState state)
        {
            List<TState> pathToRoot = GetPathToRootFromState(m_currentState);
            for (int i = 0; i < pathToRoot.Count; i++)
            {
                if (state == pathToRoot[i])
                    return true;
            }

            return false;
        }

        /// <summary>Performs depth first search on the state tree, returns null if state with this id is not found</summary>
        public TState GetStateById(TId id)
        {
            TState state = null;
            Action<TState> onNode = (node) =>
            {
                if (EqualityComparer<TId>.Default.Equals(node.Id, id))
                    state = node;
            };

            DepthFirstSearch(onNode);
            return state;
        }

        /// <summary>Performs depth first search, returns all states of the tree</summary>
        public List<TState> GetAllStates()
        {
            List<TState> states = new List<TState>();
            DepthFirstSearch((TState state) => states.Add(state));
            return states;
        }

        /// <summary>Immediately changes state of state machine, if state with this id exists and is not currently active lowest state</summary>
        public bool TryChangeState(TId id)
        {
            return TryChangeState(m_currentState, GetStateById(id));
        }

        protected bool TryChangeState(TState from, TState to)
        {
            if (from == to)
                return false;

            if (from == null || to == null)
                return false;

            List<TState> fromPathToRoot = GetPathToRootFromState(from);
            List<TState> toPathToRoot = GetPathToRootFromState(to);
            int lowestCommonAncestorOffset = 0;

            // checking if nodes are in the same tree by comparing their roots
            if (toPathToRoot[toPathToRoot.Count - 1] != fromPathToRoot[fromPathToRoot.Count - 1])
                return false;

            // walking down from root to lowest common ancestor
            while (lowestCommonAncestorOffset < fromPathToRoot.Count && lowestCommonAncestorOffset < toPathToRoot.Count &&
                fromPathToRoot[fromPathToRoot.Count - lowestCommonAncestorOffset - 1] == toPathToRoot[toPathToRoot.Count - lowestCommonAncestorOffset - 1])
            {
                lowestCommonAncestorOffset++;
            }

            // exiting from all states up to LCA
            for (int i = 0; i < fromPathToRoot.Count - lowestCommonAncestorOffset; i++)
                fromPathToRoot[i].Exit();

            // entering to all states from LCA
            for (int i = toPathToRoot.Count - lowestCommonAncestorOffset - 1; i > -1; i--)
                toPathToRoot[i].Enter();

            m_previousStateId = from.Id;
            m_currentState = to;
            OnStateChanged?.Invoke();
            return true;
        }

        protected List<TState> GetPathToRootFromState(TState state)
        {
            List<TState> path = new List<TState>();

            while (state != null)
            {
                path.Add(state);
                state = state.Parent;
            }

            return path;
        }

        protected void DepthFirstSearch(Action<TState> onNodeFound)
        {
            List<TState> states = new List<TState>();

            if (m_rootState == null || onNodeFound == null)
                return;

            Stack<TState> stateStack = new Stack<TState>();
            stateStack.Push(m_rootState);

            while (stateStack.Count > 0)
            {
                TState current = stateStack.Pop();

                onNodeFound.Invoke(current);

                foreach (TState state in current.States)
                    stateStack.Push(state);
            }
        }
    }
}
