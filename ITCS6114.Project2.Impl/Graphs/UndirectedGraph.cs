using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace ITCS6114.Project2.Impl.Graphs;

/// <summary>
/// An undirected graph.
/// </summary>
/// <typeparam name="TData">Generic data stored in the nodes.</typeparam>
/// <typeparam name="TCost">Number type to use for the cost.</typeparam>
public class UndirectedGraph<TData, TCost> : IReadOnlyCollection<UndirectedGraphNode<TData, TCost>>, IGraph
    where TCost : INumber<TCost>
{
    private readonly List<UndirectedGraphNode<TData, TCost>> _nodes;

    private readonly IComparer<TData> _comparer;
    private readonly IComparer<UndirectedGraphNode<TData, TCost>> _nodeComparer;

    private int _version;

    /// <summary>
    /// Number of nodes in the graph.
    /// </summary>
    public int NodeCount => _nodes.Count;

    /// <summary>
    /// Number of edges (connections) in the graph.
    /// </summary>
    public int EdgeCount { get; internal set; }

    public UndirectedGraph(int capacity = 0)
    {
        _nodes = new List<UndirectedGraphNode<TData, TCost>>(capacity);
        _comparer = Comparer<TData>.Default;
        _nodeComparer = new NodeComparer(_comparer);
    }

    /// <summary>
    /// Create a new node with no connections.
    /// </summary>
    public UndirectedGraphNode<TData, TCost> CreateNode(TData data)
    {
        return new UndirectedGraphNode<TData, TCost>(this, data);
    }

    /// <summary>
    /// Create a unidirectional connection between two nodes.
    /// </summary>
    public UndirectedGraphEdge<TData, TCost> CreatePath(UndirectedGraphNode<TData, TCost> a, UndirectedGraphNode<TData, TCost> b, TCost cost)
    {
        if (a.Graph != this)
            throw new ArgumentException("The given node is not attached to this graph.", nameof(a));
        
        if (b.Graph != this)
            throw new ArgumentException("The given node is not attached to this graph.", nameof(b));

        UndirectedGraphEdge<TData, TCost> connection = new UndirectedGraphEdge<TData, TCost>(a, b, cost);

        a.AddPath(connection);
        if (!connection.IsSelfLoop)
        {
            b.AddPath(connection);
        }

        ++EdgeCount;

        return connection;
    }

    /// <summary>
    /// Clears all nodes and connections from this graph.
    /// </summary>
    public void Clear()
    {
        foreach (UndirectedGraphNode<TData, TCost> node in _nodes)
        {
            node.ApplyRemoved();
        }

        _nodes.Clear();
        EdgeCount = 0;
        ++_version;
    }

    /// <summary>
    /// Removes a node from the graph and all its relevant connections.
    /// </summary>
    public bool RemoveNode(UndirectedGraphNode<TData, TCost> node)
    {
        int binarySearch = _nodes.BinarySearch(node, _nodeComparer);
        if (binarySearch < 0)
        {
            return false;
        }

        _nodes.RemoveAt(binarySearch);
        node.RemoveAllPaths();
        node.Graph = null;
        node.ApplyRemoved();
        return true;
    }

    /// <summary>
    /// Removes a path between two nodes.
    /// </summary>
    public void RemovePath(UndirectedGraphEdge<TData, TCost> connection)
    {
        if (connection.A.Graph != this || connection.B.Graph != this)
            throw new ArgumentException("The given connection's nodes are not attached to this graph.", nameof(connection));

        if (connection.IsRemoved)
            return;

        connection.IsRemoved = true;

        --EdgeCount;

        connection.A.RemovePath(connection);
        if (!connection.IsSelfLoop)
        {
            connection.B.RemovePath(connection);
        }
    }

    /// <summary>
    /// Finds a connection between two nodes, which can be used to determine the cost.
    /// </summary>
    public UndirectedGraphEdge<TData, TCost>? FindPath(UndirectedGraphNode<TData, TCost> a, UndirectedGraphNode<TData, TCost> b)
    {
        if (a.Graph != this)
            throw new ArgumentException("The given node is not attached to this graph.", nameof(a));

        if (b.Graph != this)
            throw new ArgumentException("The given node is not attached to this graph.", nameof(b));

        return a.Count <= b.Count ? a.FindPath(b) : b.FindPath(a);
    }

    /// <summary>
    /// Adds a new node to the graph. This is not necessary when calling <see cref="CreateNode"/>, but only if you want to re-add a node after removing it.
    /// </summary>
    public void AddNode(UndirectedGraphNode<TData, TCost> node)
    {
        if (node.Graph == null)
        {
            int index = _nodes.BinarySearch(node, _nodeComparer);
            if (index < 0)
            {
                _nodes.Insert(~index, node);
            }
            else
            {
                // insert after any equal values
                TData value = _nodes[index].Value;
                while (index + 1 < _nodes.Count && _comparer.Compare(value, _nodes[index + 1].Value) == 0)
                {
                    ++index;
                }

                _nodes.Insert(index + 1, node);
            }

            ++_version;
            node.Graph = this;
        }
        else if (node.Graph != this)
        {
            throw new ArgumentException("Already registered with another graph.", nameof(node));
        }
    }

    /// <summary>
    /// Get the first node in the graph with the given data.
    /// </summary>
    /// <exception cref="KeyNotFoundException">There are no nodes with that value in the graph.</exception>
    public UndirectedGraphNode<TData, TCost> this[TData data]
    {
        get
        {
            if (!TryGetNode(data, out UndirectedGraphNode<TData, TCost>? node))
                throw new KeyNotFoundException($"Node \"{data}\" not present in graph.");

            return node;
        }
    }

    /// <summary>
    /// Find the first node with a value matching <paramref name="data"/>.
    /// </summary>
    public bool TryGetNode(TData data, [NotNullWhen(true)] out UndirectedGraphNode<TData, TCost>? node)
    {
        int lo = 0, hi = _nodes.Count - 1;
        while (lo <= hi)
        {
            int mid = lo + (hi - lo) / 2;
            int cmp = _comparer.Compare(_nodes[mid].Value, data);
            if (cmp == 0)
            {
                node = _nodes[mid];
                return true;
            }

            if (cmp < 0)
                lo = mid + 1;
            else
                hi = mid - 1;
        }

        node = null;
        return false;
    }

    public GraphEnumerator GetEnumerator()
    {
        return new GraphEnumerator(this);
    }

    IEnumerator<UndirectedGraphNode<TData, TCost>> IEnumerable<UndirectedGraphNode<TData, TCost>>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    int IReadOnlyCollection<UndirectedGraphNode<TData, TCost>>.Count => _nodes.Count;
    bool IGraph.IsDirected => false;

    public struct GraphEnumerator : IEnumerator<UndirectedGraphNode<TData, TCost>>
    {
        private readonly UndirectedGraph<TData, TCost> _graph;
        private int _index, _version;

        public UndirectedGraphNode<TData, TCost> Current => _graph._nodes[_index];

        public GraphEnumerator(UndirectedGraph<TData, TCost> graph)
        {
            _graph = graph;
            _version = _graph._version;
            _index = -1;
        }

        public void Reset()
        {
            _index = -1;
            _version = _graph._version;
        }

        public bool MoveNext()
        {
            if (_version != _graph._version)
                throw new InvalidOperationException("Graph node collection modified during enumeration.");

            ++_index;
            return _index < _graph._nodes.Count;
        }

        public void Dispose() { }

        object? IEnumerator.Current => Current;
    }

    private class NodeComparer : IComparer<UndirectedGraphNode<TData, TCost>>
    {
        private readonly IComparer<TData> _dataComparer;

        public NodeComparer(IComparer<TData> dataComparer)
        {
            _dataComparer = dataComparer;
        }

        public int Compare(UndirectedGraphNode<TData, TCost>? x, UndirectedGraphNode<TData, TCost>? y)
        {
            if (x == null)
            {
                if (y == null)
                    return 0;

                return -1;
            }

            if (y == null)
                return 1;

            return _dataComparer.Compare(x.Value, y.Value);
        }
    }
}