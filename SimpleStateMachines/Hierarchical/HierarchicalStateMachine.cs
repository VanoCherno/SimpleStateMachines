using System;
using System.Collections.Generic;

namespace SimpleStateMachines.Hierarchical
{
    /// <summary>
    /// Can hold multiple state trees.
    /// Same instance of state can be used with different ids.
    /// </summary>
    public class HierarchicalStateMachine<TId, TState> : IStateMachine<TId, TState>, IReadOnlyStateMachine<TId, TState>, IStateSwitcher<TId>
        where TState : BaseState
    {
        private readonly Dictionary<TId, StateNode<TState>> m_nodes;
        private StateNode<TState> m_root;
        private StateNode<TState> m_lowestActiveNode;
        private TId m_activeStateId;
        private TId m_previousStateId;
        private TId m_rootId;

        public HierarchicalStateMachine()
        {
            m_nodes = new Dictionary<TId, StateNode<TState>>();
            m_lowestActiveNode = null;
            m_activeStateId = default;
            m_previousStateId = default;
        }

        public TId ActiveStateId => m_activeStateId;
        public TId PreviousStateId => m_previousStateId;
        public TId RootId => m_rootId;

        protected IReadOnlyDictionary<TId, StateNode<TState>> Nodes => m_nodes;
        protected StateNode<TState> Root => m_root;
        protected StateNode<TState> LowestActiveNode => m_lowestActiveNode;

        public event Action OnStateChanged;
        public event Action OnRootChanged;

        public void AddState(TId id, TState state)
        {
            if (m_nodes.ContainsKey(id))
                throw new InvalidOperationException("Can not add state. State machine already has a state with this id.");

            m_nodes.Add(id, new StateNode<TState>(state));
        }

        public void RemoveState(TId id)
        {
            if (!m_nodes.ContainsKey(id))
                return;

            m_nodes.Remove(id);
        }

        public TState GetState(TId id)
        {
            if (!m_nodes.ContainsKey(id))
                return null;

            return m_nodes[id].State;
        }

        public bool HasState(TId id)
        {
            return m_nodes.ContainsKey(id);
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

        /// <summary>Exits all active states and deletes all states</summary>
        public void Clear()
        {
            Exit();
            m_nodes.Clear();
        }

        /// <summary>Exits all active states</summary>
        public void Exit()
        {
            m_root?.Exit();
            m_root = null;
            m_lowestActiveNode = null;
            m_previousStateId = default;
            m_activeStateId = default;
            OnStateChanged?.Invoke();
        }

        public bool ChangeState(TId to)
        {
            StateNode<TState> active = m_lowestActiveNode;
            StateNode<TState> lowestCommonAncestor = null;

            if (m_root == null)
                return false;

            if (!m_nodes.TryGetValue(to, out StateNode<TState> next))
                return false;

            if (active == null)
                active = m_root;
            else if (next == active)
                return false;

            lowestCommonAncestor = active.GetLowestCommonAncestor(next);

            if (lowestCommonAncestor == null)
                return false;

            if (lowestCommonAncestor.Active != null)
                lowestCommonAncestor.Active.Exit();

            m_previousStateId = m_activeStateId;
            m_activeStateId = to;
            m_lowestActiveNode = next;
            next.Enter();
            OnStateChanged?.Invoke();
            return true;
        }

        /// <summary>Whether the state is active in hierarchy</summary>
        public bool IsInState(TId id)
        {
            if (!m_nodes.TryGetValue(id, out StateNode<TState> node))
                return false;

            return node.IsActive;
        }

        /// <summary>Sets root state</summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void SetRoot(TId id)
        {
            if (!m_nodes.TryGetValue(id, out StateNode<TState> node))
                throw new InvalidOperationException($"Can not set state with id:{id} as root. State machine does not have state with this id.");

            if (node.Parent != null)
                throw new InvalidOperationException($"Can not set state with id{id} as root. Root state must have no parent.");

            m_root?.Exit();
            m_root = node;
            m_rootId = id;
            OnRootChanged?.Invoke();
        }

        /// <summary>Exits root state and sets root to null</summary>
        public void ClearRoot()
        {
            m_root?.Exit();
            m_lowestActiveNode = null;
            m_root = null;
            m_rootId = default;
            m_activeStateId = default;
            m_previousStateId = default;
            OnStateChanged?.Invoke();
            OnRootChanged?.Invoke();
        }

        public bool IsRoot(TId id)
        {
            if (!m_nodes.TryGetValue(id, out StateNode<TState> node))
                return false;

            return m_root == node;
        }

        /// <summary>Creates a parent-child connection between two states</summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void MakeConnection(TId stateId, TId parentId)
        {
            if (!m_nodes.TryGetValue(stateId, out StateNode<TState> node))
                throw new InvalidOperationException($"Can not set parent to state with id:{stateId}. State machine does not have a state with this id.");

            if (!m_nodes.TryGetValue(parentId, out StateNode<TState> parent))
                throw new InvalidOperationException($"Can not set child to state with id:{parentId}. State machine does not have a state with this id.");

            if (node == parent)
                throw new InvalidOperationException($"Can not set state as a parent of itself.");

            if (node == m_root)
                throw new InvalidOperationException($"Can not set parent to the root state.");

            if (node.Parent != null)
                throw new InvalidOperationException($"Can not set parent to state with id:{stateId}. The state already has a parent. Remove current parent before assigning a new one.");

            if (node.IsParentOf(parent))
                throw new InvalidOperationException($"Can not set parent to state with id:{stateId}. As it is current parent of {parentId}.");

            if (!parent.IsParentOf(node))
                parent.AddChild(node);

            node.SetParent(parent);
        }

        /// <summary>Removes parent-child connection between two states</summary>
        public void ClearConnection(TId stateA, TId stateB)
        {
            if (!m_nodes.TryGetValue(stateA, out StateNode<TState> nodeA))
                return;

            if (!m_nodes.TryGetValue(stateB, out StateNode<TState> nodeB))
                return;

            if (nodeA.IsParentOf(nodeB))
            {
                nodeB.SetParent(null);
                nodeA.RemoveChild(nodeB);
            }
            else if (nodeB.IsParentOf(nodeA))
            {
                nodeA.SetParent(null);
                nodeB.RemoveChild(nodeA);
            }
        }

        /// <summary>Removes parent from state</summary>
        public void ClearParentConnection(TId stateId)
        {
            if (!m_nodes.TryGetValue(stateId, out StateNode<TState> node))
                return;

            if (node.Parent != null)
                node.Parent.RemoveChild(node);

            node.SetParent(null);
        }

        /// <summary>Removes all connections of a given state</summary>
        public void ClearAllConnectionsFrom(TId stateId)
        {
            if (!m_nodes.TryGetValue(stateId, out StateNode<TState> node))
                return;

            foreach (StateNode<TState> child in node.Children)
                child.SetParent(null);

            node.RemoveAllChildren();
            node.Parent.RemoveChild(node);
            node.SetParent(null);
        }

        /// <summary>Removes all connections for all states</summary>
        public void ClearAllStateConnections()
        {
            foreach (TId id in m_nodes.Keys)
                ClearAllConnectionsFrom(id);
        }

        /// <summary>Whether state with a given id has a parent</summary>
        public bool HasParent(TId id)
        {
            if (m_nodes.TryGetValue(id, out StateNode<TState> node))
                return node.Parent != null;

            return false;
        }

        /// <summary>Whether state is parent of another state</summary>
        public bool IsParentOf(TId parentId, TId stateId)
        {
            if (!m_nodes.TryGetValue(stateId, out StateNode<TState> node))
                return false;

            if (!m_nodes.TryGetValue(parentId, out StateNode<TState> parent))
                return false;

            return parent.IsParentOf(node);
        }

        /// <summary>Whether states have a parent-child connection</summary>
        public bool AreConnected(TId stateA, TId stateB)
        {
            if (!m_nodes.TryGetValue(stateA, out StateNode<TState> nodeA))
                return false;

            if (!m_nodes.TryGetValue(stateB, out StateNode<TState> nodeB))
                return false;

            return nodeA.IsParentOf(nodeB) | nodeB.IsParentOf(nodeA);
        }

        /// <returns>Id of the parent or default</returns>
        public TId GetParentId(TId id)
        {
            if (m_nodes.TryGetValue(id, out StateNode<TState> child))
            {
                foreach (KeyValuePair<TId, StateNode<TState>> node in m_nodes)
                {
                    if (child.Parent == node.Value)
                        return node.Key;
                }
            }

            return default;
        }

        public TId[] GetChildIds(TId id)
        {
            if (!m_nodes.TryGetValue(id, out StateNode<TState> parent))
                return new TId[0];

            List<TId> childIds = new List<TId>();

            foreach (KeyValuePair<TId, StateNode<TState>> node in m_nodes)
            {
                foreach (StateNode<TState> child in parent.Children)
                {
                    if (node.Value == child)
                        childIds.Add(node.Key);
                }
            }

            return childIds.ToArray();
        }
    }
}
