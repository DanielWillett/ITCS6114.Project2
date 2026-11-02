using ITCS6114.Project2.Impl.Graphs;
using System.Numerics;

namespace ITCS6114.Project2.Impl.Algorithms;

/// <summary>
/// Functions to calculate the minimum spanning tree for undirected graphs.
/// </summary>
/// <remarks>Uses Kruskal's algorithm.</remarks>
public struct MinimumSpanningTree<TData, TCost>
    where TCost : INumber<TCost>
{
#nullable disable

    private SetState[] _sets;

#nullable restore

    /// <summary>
    /// Create a new directed graph which is the minimum spanning tree of <paramref name="original"/>.
    /// </summary>
    public static UndirectedGraph<TData, TCost> Get(UndirectedGraph<TData, TCost> original, out TCost totalCost)
    {
        MinimumSpanningTree<TData, TCost> runner = new MinimumSpanningTree<TData, TCost>();
        return runner.Kruskal(original, out totalCost);
    }

    private UndirectedGraph<TData, TCost> Kruskal(UndirectedGraph<TData, TCost> original, out TCost totalCost)
    {
        UndirectedGraph<TData, TCost> newGraph = new UndirectedGraph<TData, TCost>(original.NodeCount);

        int set = 0;

        // .NET uses an array-based heap for PriorityQueue
        PriorityQueue<UndirectedGraphEdge<TData, TCost>, TCost> edges
            = new PriorityQueue<UndirectedGraphEdge<TData, TCost>, TCost>();

        NodeState[] states = new NodeState[original.NodeCount];
        _sets = new SetState[original.NodeCount];

        // random sentinal number for UserIndex
        const int indexValue = 1431821410;

        foreach (UndirectedGraphNode<TData, TCost> node in original)
        {
            node.UserIndex = set;
            foreach (UndirectedGraphEdge<TData, TCost> edge in node)
            {
                if (edge.UserIndex == indexValue)
                    continue;

                edge.UserIndex = indexValue;
                edges.Enqueue(edge, edge.Cost);
            }

            UndirectedGraphNode<TData, TCost> copiedNode = newGraph.CreateNode(node.Value);
            copiedNode.UserIndex = set;

            ref NodeState state = ref states[set];
            state.Copied = copiedNode;
            state.SetIndex = set;

            ref SetState setState = ref _sets[set];
            setState.Count = 1;
            setState.Parent = set;

            ++set;
        }

        TCost ttl = TCost.Zero;
        while (edges.Count > 0)
        {
            UndirectedGraphEdge<TData, TCost> edge = edges.Dequeue();
            edge.UserIndex = 0;
            ref NodeState nodeStateA = ref states[edge.A.UserIndex];
            ref NodeState nodeStateB = ref states[edge.B.UserIndex];

            if (nodeStateA.GetSetIndex(_sets) == nodeStateB.GetSetIndex(_sets))
                continue;

            int ctA = _sets[nodeStateA.SetIndex].Count,
                ctB = _sets[nodeStateB.SetIndex].Count;

            if (ctA > ctB)
            {
                Union(ref nodeStateB, ref nodeStateA);
            }
            else
            {
                Union(ref nodeStateA, ref nodeStateB);
            }

            newGraph.CreatePath(nodeStateA.Copied, nodeStateB.Copied, edge.Cost);

            ttl += edge.Cost;
        }

        totalCost = ttl;
        return newGraph;
    }

    private void Union(ref NodeState less, ref NodeState more)
    {
        int removingSetIndex = less.SetIndex;
        int expandingSetIndex = more.SetIndex;

        ref SetState lessState = ref _sets[removingSetIndex];
        _sets[expandingSetIndex].Count += lessState.Count;
        lessState.Parent = expandingSetIndex;

        less.SetIndex = expandingSetIndex;
    }

    private struct SetState
    {
        public int Count;
        public int Parent;
    }

    private struct NodeState
    {
        public UndirectedGraphNode<TData, TCost> Copied;
        public int SetIndex;
        public int GetSetIndex(SetState[] sets)
        {
            int set = SetIndex;
            while (true)
            {
                int parent = sets[set].Parent;
                if (parent == set)
                    break;

                set = parent;
                SetIndex = set;
            }

            return set;
        }
    }
}