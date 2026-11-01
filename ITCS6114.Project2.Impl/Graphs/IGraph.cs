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
