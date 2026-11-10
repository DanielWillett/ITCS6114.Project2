using System.Collections.Immutable;
using ITCS6114.Project2.Impl.Algorithms;
using ITCS6114.Project2.Impl.Graphs;
using System.CommandLine;

/* Syntax */

// exe <mst|spath|topo> <file> [-m <Directed|Undirected>]
//
// file ~ the graph file to read
//
// -m ~ specifies whether the file is directed or undirected.
//      if excluded, will read the U/D from the header.
//
// -s ~ starting node for spath.
//      defaults to the last line of the file.

RootCommand rootCommand = new RootCommand("ITCS6114 Project 2 algorithms.");

Argument<FileInfo> fileArgument = new Argument<FileInfo>("file")
{
    Description = "The graph file to read"
}.AcceptExistingOnly();

Option<GraphFileFormat.ReadMode> readModeOption = new("--mode", "-m")
{
    DefaultValueFactory = _ => GraphFileFormat.ReadMode.DetectFromHeader
};

Option<string?> startNode = new("--start", "-s")
{
    DefaultValueFactory = _ => null
};



// Minimum Spanning Tree
Command minSpanTreeCmd = new Command("mst", "Run the minimum spanning tree algorithm on a graph.");

minSpanTreeCmd.Arguments.Add(fileArgument);
minSpanTreeCmd.Options.Add(readModeOption);

minSpanTreeCmd.SetAction(result =>
{
    FileInfo file = result.GetValue(fileArgument)!;

    using StreamReader reader = new StreamReader(file.FullName);

    IGraph graph = GraphFileFormat.ReadGraph<string, double>(
        reader, out _, result.GetValue(readModeOption)
    );

    if (graph is not UndirectedGraph<string, double> unigraph)
    {
        throw new InvalidOperationException(
            "The Minimum Spanning Tree can only be calculated with an undirected graph."
        );
    }

    UndirectedGraph<string, double> outputGraph
        = MinimumSpanningTree<string, double>.Get(unigraph, out double totalCost);

    Console.WriteLine($"Total Cost: {totalCost}.");

    Console.WriteLine();
    Console.WriteLine("Output Graph:");
    GraphFileFormat.WriteGraph(outputGraph, Console.Out);
    Console.WriteLine();
    Console.WriteLine("Nodes:");

    foreach (UndirectedGraphNode<string, double> node in outputGraph)
    {
        Console.WriteLine();
        Console.WriteLine("- " + node.Value);
        Console.WriteLine("  Connections:");
        foreach (UndirectedGraphEdge<string, double> edge in node)
        {
            if (edge.A == node)
                Console.WriteLine("  - " + node.Value + " <==> " + edge.B.Value);
            else
                Console.WriteLine("  - " + node.Value + " <==> " + edge.A.Value);
        }
    }
    Console.WriteLine();
});



// Shortest Path (Dijkstra)
Command dijkstrasCmd = new Command("spath", "Run Dijkstra's shortest path algorithm on a graph.");

dijkstrasCmd.Arguments.Add(fileArgument);
dijkstrasCmd.Options.Add(readModeOption);
dijkstrasCmd.Options.Add(startNode);

dijkstrasCmd.SetAction(result =>
{
    FileInfo file = result.GetValue(fileArgument)!;

    using StreamReader reader = new StreamReader(file.FullName);

    IGraph graph = GraphFileFormat.ReadGraph<string, double>(
        reader, out string? sourceNode, result.GetValue(readModeOption)
    );

    string? sourceNodeFromOptions = result.GetValue(startNode);
    if (sourceNodeFromOptions != null)
    {
        sourceNode = sourceNodeFromOptions;
    }

    if (sourceNode == null)
    {
        throw new InvalidOperationException(
            "The given graph must specify a source node."
        );
    }

    Console.WriteLine($"Shortest paths from {sourceNode} to:");

    if (graph is UndirectedGraph<string, double> unigraph)
    {
        UndirectedGraphNode<string, double> src = unigraph[sourceNode];
        ShortestUndirectedGraphPathResults<string, double> results
            = ShortestPath<string, double>.Get(src);

        foreach (var node in results.Paths.OrderBy(x => x.Key.Value, StringComparer.Ordinal))
        {
            Console.WriteLine($"Node: {node.Key.Value}");
            Console.WriteLine($"      {node.Value}");
        }

        Console.WriteLine();
    }
    else if (graph is Digraph<string, double> digraph)
    {
        DigraphNode<string, double> src = digraph[sourceNode];
        ShortestDigraphPathResults<string, double> results
            = ShortestPath<string, double>.Get(src);

        foreach (var node in results.Paths.OrderBy(x => x.Key.Value, StringComparer.Ordinal))
        {
            Console.WriteLine($"Node: {node.Key.Value}");
            Console.WriteLine($"      {node.Value}");
        }

        Console.WriteLine();
    }
    else throw new InvalidProgramException("this shouldn't happen.");
});



// Topological Sort
Command topologicalSortCmd = new Command("topo", "Run a DFS Topological Sort algorithm on a graph.");

topologicalSortCmd.Arguments.Add(fileArgument);
topologicalSortCmd.Options.Add(readModeOption);

topologicalSortCmd.SetAction(result =>
{
    FileInfo file = result.GetValue(fileArgument)!;

    using StreamReader reader = new StreamReader(file.FullName);

    IGraph graph = GraphFileFormat.ReadGraph<string, double>(
        reader, out _, result.GetValue(readModeOption)
    );

    if (graph is not Digraph<string, double> digraph)
    {
        throw new InvalidOperationException(
            "The Topological Sort algorithm can only be ran on a directed graph."
        );
    }

    TopologicalSort<string, double>.Result res = TopologicalSort<string, double>.Sort(digraph);

    if (res.ContainsCycles)
    {
        Console.WriteLine("Cycle(s) Detected:");
        foreach (TopologicalSort<string, double>.Cycle cycle in res.Cycles)
        {
            bool first = true;
            Console.Write(" * ");
            foreach (DigraphNode<string, double> node in cycle.Nodes)
            {
                if (first)
                    first = false;
                else
                    Console.Write(" -> ");

                Console.Write(node.Value);
            }

            Console.WriteLine();
        }
    }
    else
    {
        Console.Write("Sorted:");
        for (int i = 0; i < res.Sorted.Length; ++i)
        {
            Console.Write(' ');
            Console.Write(res.Sorted[i]);
        }
    }

    Console.WriteLine();
});



rootCommand.Add(minSpanTreeCmd);
rootCommand.Add(dijkstrasCmd);
rootCommand.Add(topologicalSortCmd);

return rootCommand.Parse(args).Invoke();