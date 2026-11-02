using System.Numerics;

namespace ITCS6114.Project2.Impl.Graphs;

/// <summary>
/// A connection between two nodes on an undirected graph.
/// </summary>
public sealed class UndirectedGraphEdge<TData, TCost> : IGraphEdge<TCost>
    where TCost : INumber<TCost>
{
    internal UndirectedGraphEdge<TData, TCost>? ANext;
    internal UndirectedGraphEdge<TData, TCost>? APrev;
    internal UndirectedGraphEdge<TData, TCost>? BNext;
    internal UndirectedGraphEdge<TData, TCost>? BPrev;

    internal int Flags;

    internal bool IsRemoved
    {
        get => (Flags & 1) != 0;
        set => Flags = value ? Flags | 1 : Flags & ~1;
    }

    internal int UserIndex
    {
        get => Flags >>> 1;
        set => Flags = (Flags & 1) | (value << 1);
    }

    /// <summary>
    /// One of the two nodes forming this connection.
    /// </summary>
    public readonly UndirectedGraphNode<TData, TCost> A;

    /// <summary>
    /// One of the two nodes forming this connection.
    /// </summary>
    public readonly UndirectedGraphNode<TData, TCost> B;

    /// <summary>
    /// The cost of traversing this connection.
    /// </summary>
    public TCost Cost;

    /// <summary>
    /// Whether or not this connection represents a self-loop (to and from the same node).
    /// </summary>
    public bool IsSelfLoop => A == B;

    internal UndirectedGraphEdge(UndirectedGraphNode<TData, TCost> a, UndirectedGraphNode<TData, TCost> b, TCost cost)
    {
        A = a;
        B = b;
        Cost = cost;
    }

    internal ref UndirectedGraphEdge<TData, TCost>? Next(UndirectedGraphNode<TData, TCost> context)
    {
        if (context == A)
            return ref ANext;
        return ref BNext;
    }

    internal ref UndirectedGraphEdge<TData, TCost>? Prev(UndirectedGraphNode<TData, TCost> context)
    {
        if (context == A)
            return ref APrev;
        return ref BPrev;
    }

    internal UndirectedGraphNode<TData, TCost> Other(UndirectedGraphNode<TData, TCost> context)
    {
        return context == A ? B : A;
    }

    public void Remove()
    {
        if (IsRemoved)
            return;

        A.Graph.RemovePath(this);
    }

    public override string ToString() => $"{A} <==> {B}";

    TCost IGraphEdge<TCost>.Cost => Cost;
}