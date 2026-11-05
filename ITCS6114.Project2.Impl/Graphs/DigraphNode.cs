using System.Buffers;
using System.Collections;
using System.Numerics;

namespace ITCS6114.Project2.Impl.Graphs;

/// <summary>
/// A node in a directed graph.
/// </summary>
public class DigraphNode<TData, TCost> : IReadOnlyCollection<DigraphEdge<TData, TCost>>, IGraphNode<TData>
    where TCost : INumber<TCost>
{
#nullable disable

    internal Digraph<TData, TCost> Graph;

#nullable restore

    /// <summary>
    /// Generic value stored in this node.
    /// </summary>
    public readonly TData Value;

    /// <summary>
    /// Integer data that can be used for mapping to an array for storing algorithmic data.
    /// </summary>
    public int UserIndex { get; set; }

    // linked lists
    private DigraphEdge<TData, TCost>? _in;
    private DigraphEdge<TData, TCost>? _out;

    /// <summary>
    /// Number of connections pointing into this node.
    /// </summary>
    public int InCount { get; private set; }

    /// <summary>
    /// Number of connections pointing out of this node.
    /// </summary>
    public int OutCount { get; private set; }

    public DigraphNode(Digraph<TData, TCost> graph, TData value)
    {
        Value = value;
        graph.AddNode(this);
    }

    /// <summary>
    /// Add a path from the given node to this node.
    /// </summary>
    public DigraphEdge<TData, TCost> CreatePathFrom(DigraphNode<TData, TCost> node, TCost cost)
    {
        if (node.Graph != Graph)
        {
            throw new ArgumentException("This node is not attached to the same graph.", nameof(node));
        }

        return node.CreatePathTo(this, cost);
    }

    /// <summary>
    /// Add a path to the given node from this node.
    /// </summary>
    public DigraphEdge<TData, TCost> CreatePathTo(DigraphNode<TData, TCost> node, TCost cost)
    {
        if (node.Graph != Graph)
        {
            throw new ArgumentException("This node is not attached to the same graph.", nameof(node));
        }

        DigraphEdge<TData, TCost> connection = new DigraphEdge<TData, TCost>(this, node, cost);
        if (_out == null)
        {
            _out = connection;
#if DEBUG
            if (OutCount != 0) throw new InvalidProgramException("OutCount != 0");
#endif
            OutCount = 1;
        }
        else
        {
            DigraphEdge<TData, TCost> head = _out;
            _out = connection;
            connection.FromNext = head;
            head.FromPrev = connection;
            ++OutCount;
        }

        if (node._in == null)
        {
            node._in = connection;
#if DEBUG
            if (node.InCount != 0) throw new InvalidProgramException("node.InCount != 0");
#endif
            node.InCount = 1;
        }
        else
        {
            DigraphEdge<TData, TCost> head = node._in;
            node._in = connection;
            connection.ToNext = head;
            head.ToPrev = connection;
            ++node.InCount;
        }

        ++Graph.EdgeCount;

        return connection;
    }

    /// <summary>
    /// Finds the connection from another node to this node, if it exists.
    /// </summary>
    public DigraphEdge<TData, TCost>? FindPathFrom(DigraphNode<TData, TCost> node)
    {
        if (node.Graph != Graph)
        {
            throw new ArgumentException("This node is not attached to the same graph.", nameof(node));
        }

        AssertAttached();

        for (DigraphEdge<TData, TCost>? inNode = _in; inNode != null; inNode = inNode.ToNext)
        {
            if (inNode.From == node)
            {
                return inNode;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the connection to another node, if it exists.
    /// </summary>
    public DigraphEdge<TData, TCost>? FindPathTo(DigraphNode<TData, TCost> node)
    {
        if (node.Graph != Graph)
        {
            throw new ArgumentException("Given node is not attached to the graph.", nameof(node));
        }

        AssertAttached();

        for (DigraphEdge<TData, TCost>? outNode = _out; outNode != null; outNode = outNode.FromNext)
        {
            if (outNode.To == node)
            {
                return outNode;
            }
        }

        return null;
    }

    /// <summary>
    /// Remove a connection from another node to this node, if it exists.
    /// </summary>
    /// <returns>The removed connection.</returns>
    public DigraphEdge<TData, TCost>? RemovePathFrom(DigraphNode<TData, TCost> node)
    {
        DigraphEdge<TData, TCost>? path = FindPathFrom(node);
        if (path == null)
            return null;

        RemovePath(path);
        return path;
    }

    /// <summary>
    /// Remove a connection to another node, if it exists.
    /// </summary>
    /// <returns>The removed connection.</returns>
    public DigraphEdge<TData, TCost>? RemovePathTo(DigraphNode<TData, TCost> node)
    {
        DigraphEdge<TData, TCost>? path = FindPathTo(node);
        if (path == null)
            return null;

        RemovePath(path);
        return path;
    }

    /// <summary>
    /// Remove a connection from this node.
    /// </summary>
    /// <exception cref="ArgumentException">The given connection isn't connected to this node.</exception>
    public void RemovePath(DigraphEdge<TData, TCost> connection)
    {
        AssertAttached();

        if (connection.From != this)
        {
            if (connection.To != this)
                throw new ArgumentException("Not a connection relating to this node.");

            connection.From.RemovePath(connection);
            return;
        }

        if (connection.IsRemoved)
            return;

        connection.IsRemoved = true;

        if (_out == connection)
            _out = connection.FromNext;

        if (connection.To._in == connection)
            connection.To._in = connection.ToNext;

        --connection.To.InCount;
        --OutCount;

        --Graph.EdgeCount;

#if DEBUG
        if (connection.To.InCount < 0) throw new InvalidProgramException("connection.To.InCount < 0");
        if (OutCount < 0) throw new InvalidProgramException("OutCount < 0");
        if (Graph.EdgeCount < 0) throw new InvalidProgramException("Graph.EdgeCount < 0");
#endif

        connection.FromPrev?.FromNext = connection.FromNext;
        connection.FromNext?.FromPrev = connection.FromPrev;
        connection.ToPrev?.ToNext = connection.ToNext;
        connection.ToNext?.ToPrev = connection.ToPrev;

        connection.FromPrev = null;
        connection.FromNext = null;
        connection.ToPrev = null;
        connection.ToNext = null;
    }

    /// <summary>
    /// Removes all incoming connections from this node.
    /// </summary>
    public void RemoveAllIncomingPaths()
    {
        AssertAttached();

        int count = InCount;

        InCount = 0;
        Graph.EdgeCount -= count;
#if DEBUG
        if (Graph.EdgeCount < 0) throw new InvalidProgramException("Graph.EdgeCount < 0");
#endif

        DigraphEdge<TData, TCost>? inPath = _in;
        _in = null;
        for (; inPath != null; inPath = inPath.ToNext)
        {
            if (inPath.IsRemoved)
                continue;

            inPath.IsRemoved = true;

            if (inPath.From._out == inPath)
                inPath.From._out = inPath.FromNext;

            --inPath.From.OutCount;
#if DEBUG
            if (inPath.From.OutCount < 0) throw new InvalidProgramException("inPath.From.OutCount < 0");
#endif

            inPath.ToPrev?.ToNext = null;
            inPath.ToPrev = null;

            inPath.FromPrev?.FromNext = inPath.FromNext;
            inPath.FromNext?.FromPrev = inPath.FromPrev;
            inPath.FromNext = null;
            inPath.FromPrev = null;
        }

        inPath?.ToNext = null;
    }

    /// <summary>
    /// Removes all outgoing connections from this node.
    /// </summary>
    public void RemoveAllOutgoingPaths()
    {
        AssertAttached();

        int count = OutCount;

        OutCount = 0;
        Graph.EdgeCount -= count;
#if DEBUG
        if (Graph.EdgeCount < 0) throw new InvalidProgramException("Graph.EdgeCount < 0");
#endif

        DigraphEdge<TData, TCost>? outPath = _out;
        _out = null;
        for (; outPath != null; outPath = outPath.FromNext)
        {
            if (outPath.IsRemoved)
                continue;

            outPath.IsRemoved = true;

            if (outPath.To._in == outPath)
                outPath.To._in = outPath.ToNext;

            --outPath.To.InCount;
#if DEBUG
            if (outPath.To.InCount < 0) throw new InvalidProgramException("outPath.To.InCount < 0");
#endif

            outPath.FromPrev?.FromNext = null;
            outPath.FromPrev = null;

            outPath.ToPrev?.ToNext = outPath.ToNext;
            outPath.ToNext?.ToPrev = outPath.ToPrev;
            outPath.ToNext = null;
            outPath.ToPrev = null;
        }

        outPath?.FromNext = null;
    }

    /// <summary>
    /// Removes this node from its graph.
    /// </summary>
    public void Remove()
    {
        AssertAttached();

        Graph.RemoveNode(this);
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
            DigraphEdge<TData, TCost>? connection = _in;
            connection != null;
            connection = connection.ToNext
        )
        {
            connection.IsRemoved = true;
        }
        
        for (
            DigraphEdge<TData, TCost>? connection = _out;
            connection != null;
            connection = connection.FromNext
        )
        {
            connection.IsRemoved = true;
        }

        InCount = 0;
        OutCount = 0;
        _in = null;
        _out = null;
    }

    /// <summary>
    /// Enumerate outgoing connections.
    /// </summary>
    public ConnectionEnumerator GetEnumerator()
    {
        AssertAttached();
        return new ConnectionEnumerator(_out, isIn: false);
    }

    /// <summary>
    /// Enumerate incoming connections.
    /// </summary>
    public ConnectionEnumerator EnumerateIncomingConnections()
    {
        AssertAttached();
        return new ConnectionEnumerator(_in, isIn: true);
    }

    /// <summary>
    /// Determines if <paramref name="node"/> is reachable from this node.
    /// </summary>
    public bool IsParentOf(DigraphNode<TData, TCost> node)
    {
        return node.IsChildOf(this);
    }

    /// <summary>
    /// Determines if this node is reachable from <paramref name="node"/>.
    /// </summary>
    public bool IsChildOf(DigraphNode<TData, TCost> node)
    {
        AssertAttached();

        if (node == this)
            return true;

        BitArray mask = new BitArray(Graph.NodeCount);
        int index = 0;
        foreach (DigraphNode<TData, TCost> n in Graph)
        {
            n.UserIndex = index;
            ++index;
        }
        return IsChildOfIntl(node, mask);
    }

    private bool IsChildOfIntl(DigraphNode<TData, TCost> targetNode, BitArray mask)
    {
        if (targetNode == this)
        {
            return true;
        }

        if (mask[UserIndex])
        {
            // cycle found.
            return false;
        }

        mask[UserIndex] = true;
        foreach (DigraphEdge<TData, TCost> edge in this)
        {
            if (edge.IsSelfLoop)
                continue;
            if (edge.To.IsChildOfIntl(targetNode, mask))
                return true;
        }

        return false;
    }

    public override string ToString() => Value?.ToString() ?? string.Empty;
    public override int GetHashCode() => Value?.GetHashCode() ?? 0;

    int IReadOnlyCollection<DigraphEdge<TData, TCost>>.Count => OutCount;
    IEnumerator<DigraphEdge<TData, TCost>> IEnumerable<DigraphEdge<TData, TCost>>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    TData IGraphNode<TData>.Value => Value;

    /// <summary>
    /// Enumerates through the outgoing connections on a <see cref="DigraphNode{TData,TCost}"/>
    /// </summary>
    public struct ConnectionEnumerator : IEnumerator<DigraphEdge<TData, TCost>>
    {
        private DigraphEdge<TData, TCost>? _first;
        private int _flags;

#nullable disable

        public DigraphEdge<TData, TCost> Current => _first;

#nullable restore

        internal ConnectionEnumerator(DigraphEdge<TData, TCost>? first, bool isIn)
        {
            _first = first;
            _flags = isIn ? 2 : 0;
        }

        public bool MoveNext()
        {
            if (_first == null)
                return false;

            if ((_flags & 1) == 0)
            {
                _flags |= 1;
                return true;
            }

            _first = (_flags & 2) == 0 ? _first.FromNext : _first.ToNext;
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