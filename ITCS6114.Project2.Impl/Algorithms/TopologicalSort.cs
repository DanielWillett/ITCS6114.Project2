using ITCS6114.Project2.Impl.Graphs;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace ITCS6114.Project2.Impl.Algorithms;

/// <summary>
/// Runs a topological sort on graphs.
/// </summary>
public struct TopologicalSort<TData, TCost>
    where TCost : INumber<TCost>
{
    private static IComparer<DigraphEdge<TData, TCost>>? _digraphComparer;

    private NodeState[] _states;
    private DigraphNode<TData, TCost>[] _sortOutput;
    // private int _time;
    private int _sortOutputIndex;
    private ImmutableArray<Cycle>.Builder? _cycles;
    private ImmutableArray<DigraphNode<TData, TCost>>.Builder? _cycleBuilder;

    public static Result Sort(Digraph<TData, TCost> graph)
    {
        TopologicalSort<TData, TCost> runner = new TopologicalSort<TData, TCost>();

        return runner.SortIntl(graph);
    }
    
    private Result SortIntl(Digraph<TData, TCost> graph)
    {
        // _time = 0;
        _states = new NodeState[graph.NodeCount];
        _sortOutput = new DigraphNode<TData, TCost>[graph.NodeCount];

        _digraphComparer ??= Comparer<DigraphEdge<TData, TCost>>.Create(
            (x, y) => Comparer<TData>.Default.Compare(x.To.Value, y.To.Value)
        );

        int maxEdgeCount = 0;

        int index = 0;
        foreach (DigraphNode<TData, TCost> node in graph)
        {
            ref NodeState nodeState = ref _states[index];

            // nodeState.StartTime = 0;
            // nodeState.EndTime = -1;
            nodeState.State = NodeVisitState.Unvisited;
            nodeState.Node = node;

            maxEdgeCount = Math.Max(maxEdgeCount, node.OutCount);

            node.UserIndex = index;

            ++index;
        }

        bool anyStartingNodes = false;

        for (int i = 0; i < _states.Length; ++i)
        {
            ref NodeState nodeState = ref _states[i];
            if (nodeState.State != NodeVisitState.Unvisited)
                continue;

            DigraphNode<TData, TCost> node = nodeState.Node;
            if (node.InCount > 0)
                continue;

            anyStartingNodes = true;
            DfsVisitBigraph(node);
        }

        if (!anyStartingNodes)
        {
            // handles a situation where the entire graph is a cycle (theres no nodes without incoming connections)
            switch (_states.Length)
            {
                case 1:
                    if (_states[0].Node.InCount > 0)
                    {
                        return new Result(ImmutableArray.Create(new Cycle(ImmutableArray.Create(_states[0].Node))));
                    }

                    _sortOutput[0] = _states[0].Node;
                    break;

                case 0:
                    break;

                default:
                    for (int i = 0; i < _states.Length; ++i)
                    {
                        ref NodeState nodeState = ref _states[i];
                        if (nodeState.State != NodeVisitState.Unvisited)
                            continue;

                        DigraphNode<TData, TCost> node = nodeState.Node;
                        if (node.InCount > 0 && anyStartingNodes)
                            continue;

                        anyStartingNodes = true;
                        DfsVisitBigraph(node);
                    }

                    break;
            }
        }

        return _cycles != null ? new Result(_cycles.DrainToImmutable()) : new Result(_sortOutput);
    }

    private void DfsVisitBigraph(DigraphNode<TData, TCost> node)
    {
        // ++_time;
        ref NodeState nodeState = ref _states[node.UserIndex];

        // nodeState.StartTime = _time;
        nodeState.State = NodeVisitState.Visiting;

        foreach (DigraphEdge<TData, TCost> adjacent in node)
        {
            if (adjacent.IsSelfLoop)
            {
                _cycles ??= ImmutableArray.CreateBuilder<Cycle>();
                _cycles.Add(new Cycle([ adjacent.From ]));
                continue;
            }

            ref NodeState adjState = ref _states[adjacent.To.UserIndex];

            switch (adjState.State)
            {
                // tree edge
                case NodeVisitState.Unvisited:
                    adjState.Predecessor = node;
                    DfsVisitBigraph(adjacent.To);
                    break;

                // back edge
                case NodeVisitState.Visiting:
                    ResolveCycle(node, adjacent);
                    break;
            }
        }

        nodeState.State = NodeVisitState.Visited;
        // ++_time;
        // nodeState.EndTime = _time;

        _sortOutput[_sortOutput.Length - _sortOutputIndex - 1] = node;
        ++_sortOutputIndex;
    }

    private void ResolveCycle(DigraphNode<TData, TCost> visitedNode, DigraphEdge<TData, TCost> backEdge)
    {
        _cycleBuilder ??= ImmutableArray.CreateBuilder<DigraphNode<TData, TCost>>();

        DigraphNode<TData, TCost> predecessor = visitedNode;

        do
        {
            _cycleBuilder.Add(predecessor);
            predecessor = _states[predecessor.UserIndex].Predecessor;
        } while (predecessor != backEdge.To);

        _cycleBuilder.Add(predecessor);

        _cycleBuilder.Reverse();

        _cycles ??= ImmutableArray.CreateBuilder<Cycle>();
        _cycles.Add(new Cycle(_cycleBuilder.DrainToImmutable()));
    }

    private struct NodeState
    {
        public DigraphNode<TData, TCost> Node;
        public DigraphNode<TData, TCost> Predecessor;
        public NodeVisitState State;
    }

    private enum NodeVisitState : byte
    {
        Unvisited,
        Visiting,
        Visited
    }

    public readonly struct Result
    {
        public readonly DigraphNode<TData, TCost>[]? Sorted;
        public readonly ImmutableArray<Cycle> Cycles;

        [MemberNotNullWhen(false, nameof(Sorted))]
        public bool ContainsCycles => Sorted == null;

        public Result(DigraphNode<TData, TCost>[]? sorted)
        {
            Sorted = sorted;
            Cycles = ImmutableArray<Cycle>.Empty;
        }

        public Result(ImmutableArray<Cycle> cycles)
        {
            Sorted = null;
            Cycles = cycles;
        }
    }

    public readonly struct Cycle
    {
        public readonly ImmutableArray<DigraphNode<TData, TCost>> Nodes;

        public Cycle(ImmutableArray<DigraphNode<TData, TCost>> nodes)
        {
            Nodes = nodes;
        }
    }
}