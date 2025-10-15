using System;
using System.Collections.Generic;

namespace SimpleStateMachines.Hierarchical
{
    /// <summary>
    /// Wrapper class around state class to allow tree hierarchy
    /// </summary>
    public class StateNode<TId, TState> where TState : BaseState<TId>
    {
        private readonly TState m_state;
        private StateNode<TId, TState> m_parent;
        private StateNode<TId, TState> m_activeChild;
        private List<StateNode<TId, TState>> m_children;
        private bool m_isActive;

        public StateNode(TState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            m_state = state;
            m_children = new List<StateNode<TId, TState>>();
        }

        public TState State => m_state;
        public StateNode<TId, TState> Parent => m_parent;
        public StateNode<TId, TState> Active => m_activeChild;
        public IReadOnlyList<StateNode<TId, TState>> Children => m_children;

        public void SetParent(StateNode<TId, TState> node)
        {
            if (node == this)
                throw new InvalidOperationException("Can not make this instance a parent of itself.");

            m_parent = node;
        }

        public void AddChild(StateNode<TId, TState> node)
        {
            if (node == this)
                throw new InvalidOperationException("Can not make this instance a child of itself.");

            if (m_children.Contains(node))
                throw new InvalidOperationException("Can not add a child since the object already contains this instance as a child.");

            m_children.Add(node);
        }

        public void RemoveChild(StateNode<TId, TState> node)
        {
            m_children.Remove(node);
        }

        public bool IsParentOf(StateNode<TId, TState> node)
        {
            return m_children.Contains(node);
        }

        public bool IsActive()
        {
            return m_isActive;
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
            m_state.Exit();
            m_isActive = false;
        }

        public List<StateNode<TId, TState>> GetPathToRoot()
        {
            List<StateNode<TId, TState>> path = new List<StateNode<TId, TState>>();
            StateNode<TId, TState> node = this;

            while (node != null)
            {
                path.Add(node);
                node = node.Parent;
            }

            return path;
        }

        public List<StateNode<TId, TState>> GetPathToLowestActive()
        {
            List<StateNode<TId, TState>> path = new List<StateNode<TId, TState>>();
            StateNode<TId, TState> node = this;

            while (node != null)
            {
                path.Add(node);
                node = node.Active;
            }

            return path;
        }

        public StateNode<TId, TState> GetLowestCommonAncestor(StateNode<TId, TState> other)
        {
            StateNode<TId, TState> node = other;
            HashSet<StateNode<TId, TState>> nodes = new HashSet<StateNode<TId, TState>>();

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

        private void SetActiveChild(StateNode<TId, TState> node)
        {
            m_activeChild = node;
        }
    }
}




