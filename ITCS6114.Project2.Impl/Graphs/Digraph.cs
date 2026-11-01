using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace ITCS6114.Project2.Impl.Graphs;

/// <summary>
/// A directed graph.
/// </summary>
/// <typeparam name="TData">Generic data stored in the nodes.</typeparam>
/// <typeparam name="TCost">Number type to use for the cost.</typeparam>
public class Digraph<TData, TCost> : IReadOnlyCollection<DigraphNode<TData, TCost>>, IGraph
    where TCost : INumber<TCost>
{
    private readonly List<DigraphNode<TData, TCost>> _nodes;

    private readonly IComparer<TData> _comparer;
    private readonly IComparer<DigraphNode<TData, TCost>> _nodeComparer;

    private int _version;


    /// <summary>
    /// Number of nodes in the graph.
    /// </summary>
    public int NodeCount => _nodes.Count;

    /// <summary>
    /// Number of edges (connections) in the graph.
    /// </summary>
    public int EdgeCount { get; internal set; }

    public Digraph(int capacity = 0)
    {
        _comparer = Comparer<TData>.Default;
        _nodeComparer = new NodeComparer(_comparer);
        _nodes = new List<DigraphNode<TData, TCost>>(capacity);
    }
    
    /// <summary>
    /// Clears all nodes and connections from this graph.
    /// </summary>
    public void Clear()
    {
        foreach (DigraphNode<TData, TCost> node in _nodes)
        {
            node.ApplyRemoved();
        }

        _nodes.Clear();
        EdgeCount = 0;
        ++_version;
    }

    /// <summary>
    /// Create a new node with no connections.
    /// </summary>
    public DigraphNode<TData, TCost> CreateNode(TData data)
    {
        return new DigraphNode<TData, TCost>(this, data);
    }

    /// <summary>
    /// Removes a node from the graph and all its relevant connections.
    /// </summary>
    public bool RemoveNode(DigraphNode<TData, TCost> node)
    {
        int binarySearch = _nodes.BinarySearch(node, _nodeComparer);
        if (binarySearch < 0)
        {
            return false;
        }

        _nodes.RemoveAt(binarySearch);
        node.RemoveAllIncomingPaths();
        node.RemoveAllOutgoingPaths();
        node.Graph = null;
        return true;
    }

    /// <summary>
    /// Adds a new node to the graph. This is not necessary when calling <see cref="CreateNode"/>, but only if you want to re-add a node after removing it.
    /// </summary>
    public void AddNode(DigraphNode<TData, TCost> node)
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
    public DigraphNode<TData, TCost> this[TData data]
    {
        get
        {
            if (!TryGetNode(data, out DigraphNode<TData, TCost>? node))
                throw new KeyNotFoundException($"Node \"{data}\" not present in graph.");

            return node;
        }
    }

    /// <summary>
    /// Find the first node with a value matching <paramref name="data"/>.
    /// </summary>
    public bool TryGetNode(TData data, [NotNullWhen(true)] out DigraphNode<TData, TCost>? node)
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

    IEnumerator<DigraphNode<TData, TCost>> IEnumerable<DigraphNode<TData, TCost>>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    int IReadOnlyCollection<DigraphNode<TData, TCost>>.Count => _nodes.Count;
    bool IGraph.IsDirected => true;

    public struct GraphEnumerator : IEnumerator<DigraphNode<TData, TCost>>
    {
        private readonly Digraph<TData, TCost> _graph;
        private int _index, _version;

        public DigraphNode<TData, TCost> Current => _graph._nodes[_index];

        public GraphEnumerator(Digraph<TData, TCost> graph)
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

    private class NodeComparer : IComparer<DigraphNode<TData, TCost>>
    {
        private readonly IComparer<TData> _dataComparer;

        public NodeComparer(IComparer<TData> dataComparer)
        {
            _dataComparer = dataComparer;
        }

        public int Compare(DigraphNode<TData, TCost>? x, DigraphNode<TData, TCost>? y)
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