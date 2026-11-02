using System.Numerics;

namespace ITCS6114.Project2.Impl.Graphs;

/// <summary>
/// Interface common to <see cref="UndirectedGraph{TData,TCost}"/> and <see cref="Digraph{TData,TCost}"/>.
/// </summary>
public interface IGraph
{
    /// <summary>
    /// Whether or not this graph is a <see cref="Digraph{TData,TCost}"/>.
    /// </summary>
    bool IsDirected { get; }

    /// <summary>
    /// The number of nodes in this graph.
    /// </summary>
    int NodeCount { get; }

    /// <summary>
    /// The number of edges in this graph.
    /// </summary>
    int EdgeCount { get; }
}

/// <summary>
/// A node on a graph.
/// </summary>
public interface IGraphNode<out TData>
{
    /// <summary>
    /// The value of this edge.
    /// </summary>
    TData Value { get; }

    /// <summary>
    /// Integer data that can be used for mapping to an array for storing algorithmic data.
    /// </summary>
    int UserIndex { get; set; }
}

/// <summary>
/// An edge of a graph.
/// </summary>
public interface IGraphEdge<out TCost>
    where TCost : INumber<TCost>
{
    /// <summary>
    /// The cost of this edge.
    /// </summary>
    TCost Cost { get; }
}