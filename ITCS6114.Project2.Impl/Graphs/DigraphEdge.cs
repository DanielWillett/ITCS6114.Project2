using System.Numerics;

namespace ITCS6114.Project2.Impl.Graphs;

/// <summary>
/// A connection between two nodes on a directed graph.
/// </summary>
public sealed class DigraphEdge<TData, TCost> : IGraphEdge<TCost>
    where TCost : INumber<TCost>
{
    internal DigraphEdge<TData, TCost>? FromNext;
    internal DigraphEdge<TData, TCost>? FromPrev;
    internal DigraphEdge<TData, TCost>? ToNext;
    internal DigraphEdge<TData, TCost>? ToPrev;

    internal bool IsRemoved;

    /// <summary>
    /// The node this connection is originating from.
    /// </summary>
    public readonly DigraphNode<TData, TCost> From;

    /// <summary>
    /// The node this connection points towards.
    /// </summary>
    public readonly DigraphNode<TData, TCost> To;

    /// <summary>
    /// The cost of traversing this connection.
    /// </summary>
    public TCost Cost;

    /// <summary>
    /// Whether or not this connection represents a self-loop (to and from the same node).
    /// </summary>
    public bool IsSelfLoop => From == To;

    internal DigraphEdge(DigraphNode<TData, TCost> fromNode, DigraphNode<TData, TCost> toNode, TCost cost)
    {
        From = fromNode;
        To = toNode;
        Cost = cost;
    }

    /// <summary>
    /// Remove this connection.
    /// </summary>
    public void Remove()
    {
        From.RemovePath(this);
    }

    public override string ToString() => $"{From} ==> {To}";

    TCost IGraphEdge<TCost>.Cost => Cost;
}