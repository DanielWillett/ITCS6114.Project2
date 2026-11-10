using ITCS6114.Project2.Impl.Graphs;
using System.Collections.Immutable;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace ITCS6114.Project2.Impl.Algorithms;

/// <summary>
/// Functions to calculate the shortest path from one node to all others or one other.
/// </summary>
/// <remarks>Uses Dijkstra's algorithm to find the shortest path.</remarks>
public struct ShortestPath<TData, TCost>
    where TCost : INumber<TCost>
{

#nullable disable

    private NodeState[] _states;
    private PriorityQueue<int, Cost> _queue;

#nullable restore

    private struct Cost : IComparable<TCost>, IComparable<Cost>
    {
        public static Cost Infinite
        {
            get
            {
                Cost cost = default;
                cost.IsInfinite = true;
                return cost;
            }
        }

        public readonly TCost CostValue;
        public bool IsInfinite;

        public Cost(TCost cost)
        {
            CostValue = cost;
            IsInfinite = false;
        }
#nullable disable
        public readonly int CompareTo(TCost cost)
        {
            return IsInfinite ? 1 : CostValue.CompareTo(cost);
        }
        public readonly int CompareTo(Cost other)
        {
            if (IsInfinite)
            {
                if (other.IsInfinite)
                    return 0;

                return 1;
            }
            if (other.IsInfinite)
            {
                return -1;
            }

            return CostValue.CompareTo(other.CostValue);
        }
#nullable restore
    }

    /// <summary>
    /// Gets the shortest path from one node to all other nodes on a directed graph.
    /// </summary>
    public static ShortestDigraphPathResults<TData, TCost> Get(
        DigraphNode<TData, TCost> sourceNode,
        DigraphNode<TData, TCost>? destinationNode = null)
    {
        Digraph<TData, TCost>? graph = sourceNode.Graph;
        if (graph == null)
        {
            throw new ArgumentException("Node is not attached to a graph.", nameof(sourceNode));
        }

        if (destinationNode != null && destinationNode.Graph != graph)
        {
            throw new ArgumentException("Destination node is not part of the same graph.", nameof(destinationNode));
        }

        ShortestPath<TData, TCost> runner = new ShortestPath<TData, TCost>();
        return runner.DirectedDijkstra(graph, sourceNode, destinationNode);
    }

    /// <summary>
    /// Gets the shortest path from one node to all other nodes on a directed graph.
    /// </summary>
    public static ShortestUndirectedGraphPathResults<TData, TCost> Get(
        UndirectedGraphNode<TData, TCost> sourceNode,
        UndirectedGraphNode<TData, TCost>? destinationNode = null)
    {
        UndirectedGraph<TData, TCost>? graph = sourceNode.Graph;
        if (graph == null)
        {
            throw new ArgumentException("Node is not attached to a graph.", nameof(sourceNode));
        }

        if (destinationNode != null && destinationNode.Graph != graph)
        {
            throw new ArgumentException("Destination node is not part of the same graph.", nameof(destinationNode));
        }

        ShortestPath<TData, TCost> runner = new ShortestPath<TData, TCost>();
        return runner.UndirectedDijkstra(graph, sourceNode, destinationNode);
    }

    private ShortestDigraphPathResults<TData, TCost> DirectedDijkstra(
        Digraph<TData, TCost> graph,
        DigraphNode<TData, TCost> sourceNode,
        DigraphNode<TData, TCost>? destinationNode = null
    )
    {
        _states = new NodeState[graph.NodeCount];

        _queue = new PriorityQueue<int, Cost>(graph.NodeCount * 2);

        using (Digraph<TData, TCost>.GraphEnumerator enumerator = graph.GetEnumerator())
        {
            Cost defaultCost = Cost.Infinite;
            for (int i = 0; i < _states.Length && enumerator.MoveNext(); ++i)
            {
                DigraphNode<TData, TCost> node = enumerator.Current;

                ref NodeState state = ref _states[i];
                state.Node = node;
                state.Distance = state.Node != sourceNode ? defaultCost : new Cost(TCost.Zero);
                state.Previous = -1;

                node.UserIndex = i;
                _queue.Enqueue(i, state.Distance);
            }
        }

        while (_queue.Count > 0)
        {
            int index = _queue.Dequeue();
            ref NodeState fromState = ref _states[index];
            if (fromState.IsFinalized || fromState.Distance.IsInfinite)
                continue;

            DigraphNode<TData, TCost> node = Unsafe.As<IGraphNode<TData>, DigraphNode<TData, TCost>>(
                ref fromState.Node
            );

            foreach (DigraphEdge<TData, TCost> edge in node)
            {
                Relax(node.UserIndex, edge.To.UserIndex, edge);
            }

            fromState.IsFinalized = true;
        }

        ShortestDigraphPathResults<TData, TCost> results;

        if (destinationNode == null)
        {
            ImmutableDictionary<DigraphNode<TData, TCost>, ShortestPathResult<DigraphNode<TData, TCost>, DigraphEdge<TData, TCost>, TCost>>.Builder
                bldr = ImmutableDictionary.CreateBuilder<DigraphNode<TData, TCost>, ShortestPathResult<DigraphNode<TData, TCost>, DigraphEdge<TData, TCost>, TCost>>();

            for (int i = 0; i < _states.Length; ++i)
            {
                ref NodeState rootState = ref _states[i];

                if (rootState.Distance.IsInfinite)
                    continue;

                DigraphNode<TData, TCost> node = Unsafe.As<IGraphNode<TData>, DigraphNode<TData, TCost>>(ref rootState.Node);

                (ImmutableArray<DigraphEdge<TData, TCost>> edges, TCost totalCost)
                    = TracePath<DigraphEdge<TData, TCost>>(i);

                bldr[node] = new ShortestDigraphPathResult<TData, TCost>(edges, totalCost, node);
            }

            results = new ShortestDigraphPathResults<TData, TCost>(sourceNode, bldr.ToImmutable(), null);
        }
        else
        {
            ShortestDigraphPathResult<TData, TCost>? result;
            if (_states[destinationNode.UserIndex].Distance.IsInfinite)
            {
                result = null;
            }
            else
            {
                (ImmutableArray<DigraphEdge<TData, TCost>> edges, TCost totalCost)
                    = TracePath<DigraphEdge<TData, TCost>>(destinationNode.UserIndex);

                result = new ShortestDigraphPathResult<TData, TCost>(edges, totalCost, destinationNode);
            }

            results = new ShortestDigraphPathResults<TData, TCost>(
                sourceNode,
                ImmutableDictionary<DigraphNode<TData, TCost>, ShortestPathResult<DigraphNode<TData, TCost>, DigraphEdge<TData, TCost>, TCost>>.Empty,
                result
            );
        }

        return results;
    }

    private ShortestUndirectedGraphPathResults<TData, TCost> UndirectedDijkstra(
        UndirectedGraph<TData, TCost> graph,
        UndirectedGraphNode<TData, TCost> sourceNode,
        UndirectedGraphNode<TData, TCost>? destinationNode = null
    )
    {
        _states = new NodeState[graph.NodeCount];

        _queue = new PriorityQueue<int, Cost>(graph.NodeCount * 2);

        using (UndirectedGraph<TData, TCost>.GraphEnumerator enumerator = graph.GetEnumerator())
        {
            Cost defaultCost = Cost.Infinite;
            for (int i = 0; i < _states.Length && enumerator.MoveNext(); ++i)
            {
                UndirectedGraphNode<TData, TCost> node = enumerator.Current;

                ref NodeState state = ref _states[i];
                state.Node = node;
                state.Distance = state.Node != sourceNode ? defaultCost : new Cost(TCost.Zero);
                state.Previous = -1;

                node.UserIndex = i;
                if (state.Node == sourceNode)
                {
                    _queue.Enqueue(i, state.Distance);
                }
            }
        }

        while (_queue.Count > 0)
        {
            int index = _queue.Dequeue();
            ref NodeState fromState = ref _states[index];
            if (fromState.IsFinalized)
                continue;

            UndirectedGraphNode<TData, TCost> node = Unsafe.As<IGraphNode<TData>, UndirectedGraphNode<TData, TCost>>(ref fromState.Node);

            foreach (UndirectedGraphEdge<TData, TCost> edge in node)
            {
                UndirectedGraphNode<TData, TCost> otherNode = edge.Other(node);
                Relax(node.UserIndex, otherNode.UserIndex, edge);
            }

            fromState.IsFinalized = true;
        }

        ShortestUndirectedGraphPathResults<TData, TCost> results;

        if (destinationNode == null)
        {
            ImmutableDictionary<UndirectedGraphNode<TData, TCost>, ShortestPathResult<UndirectedGraphNode<TData, TCost>, UndirectedGraphEdge<TData, TCost>, TCost>>.Builder
                bldr = ImmutableDictionary.CreateBuilder<UndirectedGraphNode<TData, TCost>, ShortestPathResult<UndirectedGraphNode<TData, TCost>, UndirectedGraphEdge<TData, TCost>, TCost>>();

            for (int i = 0; i < _states.Length; ++i)
            {
                ref NodeState rootState = ref _states[i];
                UndirectedGraphNode<TData, TCost> node = Unsafe.As<IGraphNode<TData>, UndirectedGraphNode<TData, TCost>>(ref rootState.Node);
                
                (ImmutableArray<UndirectedGraphEdge<TData, TCost>> edges, TCost totalCost)
                    = TracePath<UndirectedGraphEdge<TData, TCost>>(i);

                bldr[node] = new ShortestUndirectedGraphPathResult<TData, TCost>(edges, totalCost, node);
            }

            results = new ShortestUndirectedGraphPathResults<TData, TCost>(sourceNode, bldr.ToImmutable(), null);
        }
        else
        {
            ShortestUndirectedGraphPathResult<TData, TCost>? result;
            if (_states[destinationNode.UserIndex].Distance.IsInfinite)
            {
                result = null;
            }
            else
            {
                (ImmutableArray<UndirectedGraphEdge<TData, TCost>> edges, TCost totalCost)
                    = TracePath<UndirectedGraphEdge<TData, TCost>>(destinationNode.UserIndex);

                result = new ShortestUndirectedGraphPathResult<TData, TCost>(edges, totalCost, destinationNode);
            }


            results = new ShortestUndirectedGraphPathResults<TData, TCost>(
                sourceNode,
                ImmutableDictionary<UndirectedGraphNode<TData, TCost>, ShortestPathResult<UndirectedGraphNode<TData, TCost>, UndirectedGraphEdge<TData, TCost>, TCost>>.Empty,
                result
            );
        }

        return results;
    }

    private void Relax(int from, int to, IGraphEdge<TCost> edge)
    {
        ref NodeState fromState = ref _states[from];
        ref NodeState toState = ref _states[to];
        if (toState.IsFinalized)
            return;

        TCost newCost = fromState.Distance.CostValue + edge.Cost;
        if (toState.Distance.CompareTo(newCost) <= 0)
            return;

        toState.Distance = new Cost(newCost);
        toState.Previous = from;
        toState.MaxCount = fromState.MaxCount + 1;
        toState.PreviousEdge = edge;
        _queue.Enqueue(to, toState.Distance);
    }

    private (ImmutableArray<TEdge> edges, TCost totalCost) TracePath<TEdge>(int i)
    {
        ref NodeState rootState = ref _states[i];

        ImmutableArray<TEdge>.Builder edgeBldr = ImmutableArray.CreateBuilder<TEdge>(rootState.MaxCount);

        int next = i;
        TCost totalCost = TCost.Zero;
        while (true)
        {
            ref NodeState state = ref _states[next];
            next = state.Previous;
            if (state.PreviousEdge == null)
                break;

            TEdge edge = Unsafe.As<IGraphEdge<TCost>, TEdge>(ref state.PreviousEdge);
            edgeBldr.Add(edge);
            totalCost += state.PreviousEdge.Cost;
        }

        return (edgeBldr.DrainToImmutable(), totalCost);
    }

    private struct NodeState
    {
        public IGraphNode<TData> Node;
        public Cost Distance;
        public int Previous;
        public IGraphEdge<TCost>? PreviousEdge;
        public int MaxCount;
        public bool IsFinalized;
    }
}

/// <summary>
/// Information about the results of the shortest path algorithm for an undirected graph.
/// </summary>
public sealed class ShortestUndirectedGraphPathResults<TData, TCost> : ShortestPathResults<UndirectedGraphNode<TData, TCost>, UndirectedGraphEdge<TData, TCost>, TCost>
    where TCost : INumber<TCost>
{
    internal ShortestUndirectedGraphPathResults(
        UndirectedGraphNode<TData, TCost> sourceNode,
        ImmutableDictionary<UndirectedGraphNode<TData, TCost>, ShortestPathResult<UndirectedGraphNode<TData, TCost>, UndirectedGraphEdge<TData, TCost>, TCost>> paths,
        ShortestPathResult<UndirectedGraphNode<TData, TCost>, UndirectedGraphEdge<TData, TCost>, TCost>? destinationNodePath
    ) : base(sourceNode, paths, destinationNodePath) { }
}

/// <summary>
/// Information about the results of the shortest path algorithm for a directed graph.
/// </summary>
public sealed class ShortestDigraphPathResults<TData, TCost> : ShortestPathResults<DigraphNode<TData, TCost>, DigraphEdge<TData, TCost>, TCost>
    where TCost : INumber<TCost>
{
    internal ShortestDigraphPathResults(
        DigraphNode<TData, TCost> sourceNode,
        ImmutableDictionary<DigraphNode<TData, TCost>, ShortestPathResult<DigraphNode<TData, TCost>, DigraphEdge<TData, TCost>, TCost>> paths,
        ShortestPathResult<DigraphNode<TData, TCost>, DigraphEdge<TData, TCost>, TCost>? destinationNodePath
    ) : base(sourceNode, paths, destinationNodePath) { }
}

/// <summary>
/// Information about the results of the shortest path algorithm for a graph.
/// </summary>
public abstract class ShortestPathResults<TNode, TEdge, TCost>
    where TCost : INumber<TCost>
    where TNode : notnull
    where TEdge : IGraphEdge<TCost>
{
    /// <summary>
    /// The node from which the shortest paths are calculated.
    /// </summary>
    public TNode SourceNode { get; }

    /// <summary>
    /// Maps each node's data to the shortest path from the source node to this node.
    /// </summary>
    /// <remarks>This dictionary doesn't include the source node.</remarks>
    public ImmutableDictionary<TNode, ShortestPathResult<TNode, TEdge, TCost>> Paths { get; }

    /// <summary>
    /// If a destination node was given, the path to the destination node.
    /// </summary>
    public ShortestPathResult<TNode, TEdge, TCost>? DestinationNodePath { get; }

    protected ShortestPathResults(
        TNode sourceNode,
        ImmutableDictionary<TNode, ShortestPathResult<TNode, TEdge, TCost>> paths,
        ShortestPathResult<TNode, TEdge, TCost>? destinationNodePath
    )
    {
        SourceNode = sourceNode;
        Paths = paths;
        DestinationNodePath = destinationNodePath;
    }
}

/// <summary>
/// Results regarding a single shortest path from one node to another in an undirected graph.
/// </summary>
public sealed class ShortestUndirectedGraphPathResult<TData, TCost> : ShortestPathResult<UndirectedGraphNode<TData, TCost>, UndirectedGraphEdge<TData, TCost>, TCost>
    where TCost : INumber<TCost>
{
    public UndirectedGraphNode<TData, TCost> EndNode { get; }

    public ShortestUndirectedGraphPathResult(ImmutableArray<UndirectedGraphEdge<TData, TCost>> edges, TCost totalCost, UndirectedGraphNode<TData, TCost> endNode)
        : base(edges, totalCost)
    {
        EndNode = endNode;
    }

    public override UndirectedGraphNode<TData, TCost>[] CreatePath()
    {
        if (Edges.Length == 0)
        {
            return [ EndNode ];
        }

        int index = Edges.Length;

        UndirectedGraphNode<TData, TCost>[] array = new UndirectedGraphNode<TData, TCost>[index + 1];

        UndirectedGraphNode<TData, TCost> prevNode = EndNode;
        array[index] = prevNode;
        --index;
        for (int i = 0; i < Edges.Length; ++i)
        {
            prevNode = Edges[i].Other(prevNode);
            array[index] = prevNode;
            --index;
        }

        return array;
    }
}

/// <summary>
/// Results regarding a single shortest path from one node to another in a directed graph.
/// </summary>
public sealed class ShortestDigraphPathResult<TData, TCost> : ShortestPathResult<DigraphNode<TData, TCost>, DigraphEdge<TData, TCost>, TCost>
    where TCost : INumber<TCost>
{
    public DigraphNode<TData, TCost> EndNode { get; }

    public ShortestDigraphPathResult(ImmutableArray<DigraphEdge<TData, TCost>> edges, TCost totalCost, DigraphNode<TData, TCost> endNode)
        : base(edges, totalCost)
    {
        EndNode = endNode;
    }

    public override DigraphNode<TData, TCost>[] CreatePath()
    {
        if (Edges.Length == 0)
        {
            return [ EndNode ];
        }

        DigraphNode<TData, TCost>[] array = new DigraphNode<TData, TCost>[Edges.Length + 1];

        array[0] = Edges[^1].From;

        int index = 0;
        for (int i = Edges.Length - 1; i >= 0; --i)
        {
            array[++index] = Edges[i].To;
        }

        return array;
    }
}

/// <summary>
/// Results regarding a single shortest path from one node to another in a graph.
/// </summary>
public abstract class ShortestPathResult<TNode, TEdge, TCost>
    where TEdge : IGraphEdge<TCost>
    where TCost : INumber<TCost>
{
    public ImmutableArray<TEdge> Edges { get; }
    public TCost TotalCost { get; }

    protected ShortestPathResult(ImmutableArray<TEdge> edges, TCost totalCost)
    {
        Edges = edges;
        TotalCost = totalCost;
    }

    /// <summary>
    /// Creates an array of nodes in the order they should be visited.
    /// </summary>
    public abstract TNode[] CreatePath();

    public override string ToString()
    {
        StringBuilder bldr = new StringBuilder()
            .Append(CultureInfo.CurrentCulture, $"{{ Cost = {TotalCost}, Edges = ");

        TNode[] path = CreatePath();

        for (int i = 0; i < path.Length; ++i)
        {
            if (i != 0)
                bldr.Append(" -> ");
            bldr.Append(path[i]);
        }

        bldr.Append(" }");
        return bldr.ToString();
    }
}