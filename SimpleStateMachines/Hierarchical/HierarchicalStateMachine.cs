using System;
using System.Collections.Generic;

namespace SimpleStateMachines.Hierarchical
{
    /// <summary>
    /// State machine which holds a n-ary tree structure of states.
    /// Allows for non-hierarchical states. It will create it's own wrapper around states to allow tree structure.
    /// Can hold multiple state trees
    /// </summary>
    public class HierarchicalStateMachine<TId, TState> : IReadonlyStateMachine<TId, TState>
        where TState : BaseState<TId>
    {
        private readonly Dictionary<TId, StateNode<TId, TState>> m_nodes;
        private readonly ITransitionManager<TId> m_transitionManager;
        private StateNode<TId, TState> m_root;
        private StateNode<TId, TState> m_lowestActiveNode;
        private TId m_previousStateId;

        public HierarchicalStateMachine(ITransitionManager<TId> transitionManager)
        {
            m_nodes = new Dictionary<TId, StateNode<TId, TState>>();
            m_transitionManager = transitionManager;
            m_root = null;
            m_lowestActiveNode = null;
            m_previousStateId = default;
        }

        public TId ActiveStateId => (m_lowestActiveNode != null) ? m_lowestActiveNode.State.Id : default;
        public TId PreviousStateId => m_previousStateId;
        public ITransitionManager<TId> Transitions => m_transitionManager;

        protected IReadOnlyDictionary<TId, StateNode<TId, TState>> Nodes => m_nodes;
        protected StateNode<TId, TState> Root => m_root;
        protected StateNode<TId, TState> LowestActiveNode => m_lowestActiveNode;

        public event Action OnStateChanged;

        /// <summary>Sets state with this id as current root of state hierarchy</summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void SetRoot(TId id)
        {
            if (!m_nodes.TryGetValue(id, out StateNode<TId, TState> node))
                throw new InvalidOperationException("State machine does not contain a node with this id.");

            if (node.Parent != null)
                throw new InvalidOperationException("Can not set a node as root. Root must have no parent.");

            m_root?.Exit();
            m_root = node;
        }

        public void AddState(TState state)
        {
            if (m_nodes.ContainsKey(state.Id))
                throw new InvalidOperationException("Can not add state. State with this id has alredy been added.");

            m_nodes.Add(state.Id, new StateNode<TId, TState>(state));
        }

        public bool TryAddState(TState state)
        {
            return m_nodes.TryAdd(state.Id, new StateNode<TId, TState>(state));
        }

        public bool RemoveState(TState state)
        {
            return RemoveState(state.Id);
        }

        public bool RemoveState(TId id)
        {
            if (!m_nodes.TryGetValue(id, out StateNode<TId, TState> node))
                return false;

            if (node.IsActive())
                return false;

            node.Parent.RemoveChild(node);
            node.SetParent(null);
            m_nodes.Remove(id);
            return true;
        }

        /// <summary>Exits all active states and deletes all states from state machine</summary>
        public void Clear()
        {
            m_root?.Exit();
            m_root = null;
            m_lowestActiveNode = null;
            m_previousStateId = default;
            ClearAllConnections();
            m_nodes.Clear();
        }

        /// <summary>Exits all active states</summary>
        public void Exit()
        {
            if (m_root == null)
                return;

            m_root.Exit();
            m_lowestActiveNode = null;
            OnStateChanged?.Invoke();
        }

        public TState GetState(TId id)
        {
            if (m_nodes.TryGetValue(id, out StateNode<TId, TState> node))
                return node.State;

            return null;
        }

        public TState[] GetAllStates()
        {
            int i = 0;
            TState[] states = new TState[m_nodes.Values.Count];

            foreach (StateNode<TId, TState> node in m_nodes.Values)
            {
                states[i] = node.State;
                i++;
            }

            return states;
        }

        public TId[] GetAllIds()
        {
            int i = 0;
            TId[] ids = new TId[m_nodes.Keys.Count];

            foreach (TId id in m_nodes.Keys)
            {
                ids[i] = id;
                i++;
            }

            return ids;
        }

        /// <summary>Creates parent-child connection between states</summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void MakeParent(TId stateId, TId parentId)
        {
            if (CompareIds(stateId, parentId))
                throw new InvalidOperationException("Can not set a state as a parent of itself.");

            if (!m_nodes.TryGetValue(stateId, out StateNode<TId, TState> node))
                throw new InvalidOperationException($"Can not set parent to state. State with {stateId} is not assigned to this state machine.");

            if (!m_nodes.TryGetValue(parentId, out StateNode<TId, TState> parent))
                throw new InvalidOperationException($"Can not set child to state. State with {parentId} is not assigned to this state machine.");

            if (node == m_root)
                throw new InvalidOperationException("Can not set parent to the root node.");

            if (node.Parent != null)
                throw new InvalidOperationException($"Can not set parent to state. The state {stateId} already has a parent. Remove current parent before calling this method.");

            if (parent.IsParentOf(node))
                throw new InvalidOperationException("Can not set parent to state. This parent is already set.");

            if (node.IsParentOf(parent))
                throw new InvalidOperationException($"Can not set parent to state as this state {stateId} is parent of {parentId}");

            parent.AddChild(node);
            node.SetParent(parent);
        }

        /// <summary>Creates parent-child connection between states</summary>
        public bool TryMakeParent(TId stateId, TId parentId)
        {
            if (CompareIds(stateId, parentId))
                return false;

            if (!m_nodes.TryGetValue(stateId, out StateNode<TId, TState> node))
                return false;

            if (!m_nodes.TryGetValue(parentId, out StateNode<TId, TState> parent))
                return false;

            if (node == m_root)
                return false;

            if (node.Parent != null)
                return false;

            if (parent.IsParentOf(node))
                return false;

            if (node.IsParentOf(parent))
                return false;

            parent.AddChild(node);
            node.SetParent(parent);
            return true;
        }

        /// <summary>Removes parent-child connection from state</summary>
        public bool ClearParent(TId stateId)
        {
            if (!m_nodes.TryGetValue(stateId, out StateNode<TId, TState> node))
                return false;

            node.SetParent(null);
            node?.Parent.RemoveChild(node);
            return true;
        }

        /// <summary>Removes all connections from all states</summary>
        public void ClearAllConnections()
        {
            foreach (StateNode<TId, TState> node in m_nodes.Values)
            {
                node.Parent.RemoveChild(node);
                node.SetParent(null);
            }
        }

        /// <summary>Checks if any transitions possible from currently active lowest state. If so - performs transition</summary>
        public void TickTransitions()
        {
            if (m_lowestActiveNode == null)
                return;

            if (m_transitionManager.ShouldTransition(m_lowestActiveNode.State.Id, out TId to))
                ChangeState(to);
        }

        /// <summary>Transitions to another state if state connections allow such transition</summary>
        public bool ChangeState(TId to)
        {
            StateNode<TId, TState> active = m_lowestActiveNode;
            StateNode<TId, TState> lowestCommonAncestor = null;

            if (m_root == null)
                return false;

            if (!m_nodes.TryGetValue(to, out StateNode<TId, TState> next))
                return false;

            if (active == null)
                active = m_root;
            else if (active == next)
                return false;

            lowestCommonAncestor = active.GetLowestCommonAncestor(next);

            if (lowestCommonAncestor == null)
                return false;

            if (lowestCommonAncestor.Active != null)
                lowestCommonAncestor.Active.Exit();

            m_lowestActiveNode = next;
            next.Enter();
            OnStateChanged?.Invoke();
            return true;
        }

        private bool CompareIds(TId a, TId b)
        {
            return EqualityComparer<TId>.Default.Equals(a, b);
        }
    }
}