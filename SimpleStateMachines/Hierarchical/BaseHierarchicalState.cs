using System;
using System.Collections.Generic;

namespace SimpleStateMachines.Hierarchical
{
    public abstract class BaseHierarchicalState<TId, TState> : IHierarchicalState<TId, TState>
        where TState : class, IHierarchicalState<TId, TState>
    {
        private readonly TId m_stateId;
        private readonly TState m_parent;
        private readonly Dictionary<TId, TState> m_states;
        private IStateTree<TId, TState> m_tree;

        public BaseHierarchicalState(TId id, IStateTree<TId, TState> tree, TState parent = null)
        {
            if (tree == null)
                throw new ArgumentNullException(nameof(tree));

            m_stateId = id;
            m_parent = parent;
            m_states = new Dictionary<TId, TState>();
            m_tree = tree;
        }

        public TId Id => m_stateId;
        public TState Parent => m_parent;
        public IReadOnlyCollection<TState> States => m_states.Values;

        public abstract void Enter();
        public abstract void Exit();

        /// <summary>Adds state if a state with this id is not alredy in the tree</summary>
        public bool TryAddState(TState state)
        {
            if (m_tree.GetStateById(state.Id) != null)
                return false;

            return m_states.TryAdd(state.Id, state);
        }

        /// <summary>Removes state if is not currently active in hierarchy</summary>
        public bool TryRemoveState(TId id)
        {
            if (m_tree.IsStateActiveInHierarchy(id))
                return false;

            return m_states.Remove(id);
        }

        /// <summary>Removes state if is not currently active in hierarchy</summary>
        public bool TryRemoveState(TState state)
        {
            if (m_tree.IsStateActiveInHierarchy(state))
                return false;

            return m_states.Remove(state.Id);
        }

        public TState GetChildStateById(TId id)
        {
            if (m_states.TryGetValue(id, out TState state))
                return state;

            return null;
        }
    }
}
