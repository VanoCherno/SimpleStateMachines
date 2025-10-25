using System;
using System.Collections.Generic;

namespace SimpleStateMachines.Hierarchical
{
    /// <summary>
    /// Wrapper class around state to allow tree-like structure of hierarchical state machines
    /// </summary>
    public class StateNode<TState> where TState : BaseState
    {
        private readonly TState m_state;
        private readonly List<StateNode<TState>> m_children;
        private StateNode<TState> m_parent;
        private StateNode<TState> m_activeChild;
        private bool m_isActive;

        public StateNode(TState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            m_state = state;
            m_children = new List<StateNode<TState>>();
        }

        public bool IsActive => m_isActive;
        public TState State => m_state;
        public StateNode<TState> Parent => m_parent;
        public StateNode<TState> Active => m_activeChild;
        public IReadOnlyList<StateNode<TState>> Children => m_children;

        public void SetParent(StateNode<TState> node)
        {
            if (node == this)
                throw new InvalidOperationException("Can not make this instance a parent of itself.");

            m_parent = node;
        }

        public void AddChild(StateNode<TState> node)
        {
            if (node == this)
                throw new InvalidOperationException("Can not make this instance a child of itself.");

            if (m_children.Contains(node))
                throw new InvalidOperationException("Can not add child. Node already has this child.");

            m_children.Add(node);
        }

        public void RemoveChild(StateNode<TState> node)
        {
            m_children.Remove(node);
        }

        public void RemoveAllChildren()
        {
            m_children.Clear();
        }

        public bool IsParentOf(StateNode<TState> node)
        {
            return m_children.Contains(node);
        }

        public void Enter()
        {
            if (m_parent != null)
            {
                m_parent.SetActiveChild(this);
                m_parent.Enter();
            }

            if (m_isActive)
                return;

            m_state.Enter();
            m_isActive = true;
        }

        public void Exit()
        {
            m_activeChild?.Exit();
            m_activeChild = null;

            if (!m_isActive)
                return;

            m_state.Exit();
            m_isActive = false;
        }

        public StateNode<TState> GetLowestCommonAncestor(StateNode<TState> other)
        {
            StateNode<TState> node = other;
            HashSet<StateNode<TState>> nodes = new HashSet<StateNode<TState>>();

            while (node != null)
            {
                nodes.Add(node);
                node = node.Parent;
            }

            node = this;

            while (node != null)
            {
                if (nodes.Contains(node))
                    return node;

                node = node.Parent;
            }

            return null;
        }

        private void SetActiveChild(StateNode<TState> node)
        {
            m_activeChild = node;
        }
    }
}
