using System.Collections;
using System.Numerics;

namespace ITCS6114.Project2.Impl.Graphs;

/// <summary>
/// A node in an undirected graph.
/// </summary>
public class UndirectedGraphNode<TData, TCost> : IReadOnlyCollection<UndirectedGraphEdge<TData, TCost>>, IGraphNode<TData>
    where TCost : INumber<TCost>
{
#nullable disable

    internal UndirectedGraph<TData, TCost> Graph;

#nullable restore

    // linked list
    private UndirectedGraphEdge<TData, TCost>? _paths;

    /// <summary>
    /// Generic value stored in this node.
    /// </summary>
    public readonly TData Value;

    /// <summary>
    /// Integer data that can be used for mapping to an array for storing algorithmic data.
    /// </summary>
    public int UserIndex { get; set; }

    /// <summary>
    /// Number of connections on this node. Self-loops count as <c>2</c>.
    /// </summary>
    public int Count { get; private set; }

    public UndirectedGraphNode(UndirectedGraph<TData, TCost> graph, TData data)
    {
        Value = data;
        graph.AddNode(this);
    }

    internal UndirectedGraphEdge<TData, TCost>? FindPath(UndirectedGraphNode<TData, TCost> otherNode)
    {
        if (otherNode == this)
        {
            for (
                UndirectedGraphEdge<TData, TCost>? connection = _paths;
                connection != null;
                connection = this == connection.A ? connection.ANext : connection.BNext
            )
            {
                if (connection.A == otherNode && connection.B == otherNode)
                    return connection;
            }
        }
        else
        {
            for (
                UndirectedGraphEdge<TData, TCost>? connection = _paths;
                connection != null;
                connection = this == connection.A ? connection.ANext : connection.BNext
            )
            {
                if (connection.A == otherNode || connection.B == otherNode)
                    return connection;
            }
        }

        return null;
    }

    internal void AddPath(UndirectedGraphEdge<TData, TCost> connection)
    {
        bool selfLoop = connection.IsSelfLoop;

        if (_paths == null)
        {
            _paths = connection;
#if DEBUG
            if (Count != 0) throw new InvalidProgramException("Count != 0");
#endif
            Count = selfLoop ? 2 : 1;
        }
        else
        {
            UndirectedGraphEdge<TData, TCost> head = _paths;
            _paths = connection;
            if (head.A == this)
                head.APrev = connection;
            else
                head.BPrev = connection;

            if (connection.A == this)
                connection.ANext = head;
            else
                connection.BNext = head;

            if (selfLoop)
                Count += 2;
            else
                ++Count;
        }
    }

    internal void RemovePath(UndirectedGraphEdge<TData, TCost> connection)
    {
        bool selfLoop = connection.IsSelfLoop;

        ref UndirectedGraphEdge<TData, TCost>?
            next = ref connection.Next(this),
            prev = ref connection.Prev(this);

        UndirectedGraphEdge<TData, TCost>? nextVal = next, prevVal = prev;

        if (_paths == connection)
            _paths = nextVal;

        if (selfLoop)
            Count -= 2;
        else
            --Count;

#if DEBUG
        if (Count < 0) throw new InvalidProgramException("Count < 0");
#endif

        prevVal?.Next(this) = nextVal;
        nextVal?.Prev(this) = prevVal;

        next = null;
        prev = null;
    }

    /// <summary>
    /// Removes this node from its graph.
    /// </summary>
    public void Remove()
    {
        AssertAttached();

        Graph.RemoveNode(this);
    }

    /// <summary>
    /// Remove all connections to this node.
    /// </summary>
    public void RemoveAllPaths()
    {
        AssertAttached();

        int edgeCount = 0;
        Count = 0;

        UndirectedGraphEdge<TData, TCost>? path = _paths;
        _paths = null;
        while (path != null)
        {
            if (path.IsRemoved)
            {
                path = path.Next(this);
                continue;
            }

            path.IsRemoved = true;
            ++edgeCount;

            if (!path.IsSelfLoop)
            {
                UndirectedGraphNode<TData, TCost> otherNode = path.Other(this);
                if (otherNode._paths == path)
                    otherNode._paths = path.Next(otherNode);

                --otherNode.Count;
#if DEBUG
                if (otherNode.Count < 0) throw new InvalidProgramException("otherNode.Count < 0");
#endif
                ref UndirectedGraphEdge<TData, TCost>?
                    otherPrev = ref path.Prev(otherNode),
                    otherNext = ref path.Next(otherNode);

                UndirectedGraphEdge<TData, TCost>? otherPrevVal = otherPrev, otherNextVal = otherNext;
                otherPrevVal?.Next(otherNode) = otherNextVal;
                otherNextVal?.Prev(otherNode) = otherPrevVal;
                otherPrev = null;
                otherNext = null;
            }
            else
            {
                path.BNext = null;
                path.BPrev = null;
            }

            ref UndirectedGraphEdge<TData, TCost>? prev = ref path.Prev(this);

            prev?.Next(this) = null;
            prev = null;

            ref UndirectedGraphEdge<TData, TCost>? next = ref path.Next(this);

            UndirectedGraphEdge<TData, TCost>? newPath = next;
            next = null;
            path = newPath;
        }

        Graph.EdgeCount -= edgeCount;
#if DEBUG
        if (Graph.EdgeCount < 0) throw new InvalidProgramException("Graph.EdgeCount < 0");
#endif
    }

    private void AssertAttached()
    {
        if (Graph == null)
        {
            throw new InvalidOperationException("This node is no longer attached to a graph.");
        }
    }

    internal void ApplyRemoved()
    {
        for (
            UndirectedGraphEdge<TData, TCost>? connection = _paths;
            connection != null;
            connection = this == connection.A ? connection.ANext : connection.BNext
        )
        {
            connection.IsRemoved = true;
        }

        Count = 0;
        _paths = null;
    }

    /// <summary>
    /// Enumerate outgoing connections.
    /// </summary>
    public ConnectionEnumerator GetEnumerator()
    {
        AssertAttached();
        return new ConnectionEnumerator(this);
    }

    /// <summary>
    /// Enumerate outgoing connections.
    /// </summary>
    public ConnectionEnumerator GetEnumerator(bool selfLoopsOnlyOnce)
    {
        AssertAttached();
        return new ConnectionEnumerator(this, selfLoopsOnlyOnce);
    }

    public override string ToString() => Value?.ToString() ?? string.Empty;
    public override int GetHashCode() => Value?.GetHashCode() ?? 0;

    IEnumerator<UndirectedGraphEdge<TData, TCost>> IEnumerable<UndirectedGraphEdge<TData, TCost>>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    TData IGraphNode<TData>.Value => Value;

    /// <summary>
    /// Enumerates through the outgoing connections on a <see cref="DigraphNode{TData,TCost}"/>
    /// </summary>
    public struct ConnectionEnumerator : IEnumerator<UndirectedGraphEdge<TData, TCost>>
    {
        private readonly UndirectedGraphNode<TData, TCost>? _node;
        private UndirectedGraphEdge<TData, TCost>? _first;
        private int _flags;

#nullable disable

        public UndirectedGraphEdge<TData, TCost> Current => _first;

#nullable restore

        internal ConnectionEnumerator(UndirectedGraphNode<TData, TCost> node, bool selfLoopsOnlyOnce = false)
        {
            _node = node;
            _first = node._paths;
            _flags = selfLoopsOnlyOnce ? 4 : 0;
        }

        public bool MoveNext()
        {
            if (_first == null)
                return false;

            if ((_flags & 1) == 0)
            {
                _flags |= 1;
                if (_first.IsSelfLoop && (_flags & 4) == 0)
                {
                    _flags |= 2;
                }
                return true;
            }

            if ((_flags & 2) != 0)
            {
                // self loop, repeat once
                _flags &= ~2;
                return true;
            }

            _first = _node == _first.A ? _first.ANext : _first.BNext;
            if (_first == null)
                return false;

            if (_first.IsSelfLoop)
                _flags |= 2;
            
            return _first != null;
        }


        public void Dispose() { }
        object? IEnumerator.Current => _first;
        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }
    }
}
