using ITCS6114.Project2.Impl.Graphs;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace ITCS6114.Project2.Impl.Algorithms;

/// <summary>
/// Runs a topological sort on graphs.
/// </summary>
public struct TopologicalSort<TData, TCost>
    where TCost : INumber<TCost>
{
    private static IComparer<DigraphEdge<TData, TCost>>? _digraphComparer;

    private NodeState[] _states;
    private TData[] _sortOutput;
    private int _time, _sortOutputIndex;
    private DigraphEdge<TData, TCost>[] _sortEdgesBuffer;

    public static TData[] Sort(Digraph<TData, TCost> graph)
    {
        TopologicalSort<TData, TCost> runner = new TopologicalSort<TData, TCost>();
        return runner.SortIntl(graph);
    }
    
    public static TData[] Sort(UndirectedGraph<TData, TCost> graph)
    {
        TopologicalSort<TData, TCost> runner = new TopologicalSort<TData, TCost>();
        return runner.SortIntl(graph);
    }

    private TData[] SortIntl(Digraph<TData, TCost> graph)
    {
        // _time = 0;
        _states = new NodeState[graph.NodeCount];
        _sortOutput = new TData[graph.NodeCount];

        _digraphComparer ??= Comparer<DigraphEdge<TData, TCost>>.Create(
            (x, y) => Comparer<TData>.Default.Compare(x.To.Value, y.To.Value)
        );

        int maxEdgeCount = 0;

        int index = 0;
        foreach (DigraphNode<TData, TCost> node in graph)
        {
            ref NodeState nodeState = ref _states[index];

            nodeState.StartTime = 0;
            nodeState.EndTime = -1;
            nodeState.State = NodeVisitState.Unvisited;
            nodeState.Node = node;

            maxEdgeCount = Math.Max(maxEdgeCount, node.OutCount);

            node.UserIndex = index;

            ++index;
        }

        _sortEdgesBuffer = new DigraphEdge<TData, TCost>[maxEdgeCount];

        for (int i = 0; i < _states.Length; ++i)
        {
            ref NodeState nodeState = ref _states[i];
            if (nodeState.State != NodeVisitState.Unvisited)
                continue;

            DigraphNode<TData, TCost> node = Unsafe.As<IGraphNode<TData>, DigraphNode<TData, TCost>>(ref nodeState.Node);
            if (node.InCount > 0)
                continue;

            DfsVisitBigraph(node);
        }

        return _sortOutput;
    }

    private void DfsVisitBigraph(DigraphNode<TData, TCost> node)
    {
        ++_time;
        ref NodeState nodeState = ref _states[node.UserIndex];

        nodeState.StartTime = _time;
        nodeState.State = NodeVisitState.Visiting;

        int edgeIndex = 0;
        foreach (DigraphEdge<TData, TCost> adjacent in node)
        {
            _sortEdgesBuffer[edgeIndex] = adjacent;
            ++edgeIndex;
        }

        Console.WriteLine($"{node.Value} {nodeState.StartTime} /");

        // insertion sort on all edges connected to 'node'
        Array.Sort(_sortEdgesBuffer, 0, edgeIndex, _digraphComparer);

        for (int i = 0; i < edgeIndex; ++i)
        {
            DigraphEdge<TData, TCost> adjacent = _sortEdgesBuffer[i];
            ref NodeState adjState = ref _states[adjacent.To.UserIndex];
            if (adjState.State != NodeVisitState.Unvisited)
                continue;

            adjState.Predecessor = node;
            DfsVisitBigraph(adjacent.To);
        }

        nodeState.State = NodeVisitState.Visited;
        ++_time;
        nodeState.EndTime = _time;

        _sortOutput[_sortOutput.Length - _sortOutputIndex - 1] = node.Value;
        ++_sortOutputIndex;

        Console.WriteLine($"{node.Value} {nodeState.StartTime} / {nodeState.EndTime}");
    }

    private TData[] SortIntl(UndirectedGraph<TData, TCost> graph)
    {
        return null;
    }

    private struct NodeState
    {
        public IGraphNode<TData> Node;
        public IGraphNode<TData>? Predecessor;
        public int StartTime, EndTime;
        public NodeVisitState State;
    }

    private enum NodeVisitState : byte
    {
        Unvisited,
        Visiting,
        Visited
    }
}