using ITCS6114.Project2.Impl.Algorithms;
using ITCS6114.Project2.Impl.Graphs;

namespace ITCS6114.Project2.Tests;

public class ShortestPathTests
{
    [Test]
    public async Task Test1_Undirected_Single()
    {
        using StreamReader reader = new StreamReader("Assets/sp-1.txt");
        UndirectedGraph<string, int> graph = (UndirectedGraph<string, int>)GraphFileFormat.ReadGraph<string, int>(reader, out string? sourceNode);

        UndirectedGraphNode<string, int> src = graph[sourceNode!];
        UndirectedGraphNode<string, int> dst = graph["F"];

        ShortestUndirectedGraphPathResults<string, int> results = ShortestPath<string, int>.Get(src, dst);

        await Assert.That(results.Paths).IsEmpty();
        await Assert.That(results.DestinationNodePath).IsNotNull();
        await Assert.That(results.DestinationNodePath!.ToString())
            .Contains("= A -> C -> D -> F }")
            .Or
            .Contains("= A -> B -> E -> F }");
        await Assert.That(results.DestinationNodePath!.TotalCost).IsEqualTo(6);

        Console.WriteLine(results.DestinationNodePath);
    }

    [Test]
    public async Task Test1_Undirected_All()
    {
        using StreamReader reader = new StreamReader("Assets/sp-1.txt");
        UndirectedGraph<string, int> graph = (UndirectedGraph<string, int>)GraphFileFormat.ReadGraph<string, int>(reader, out string? sourceNode);

        UndirectedGraphNode<string, int> src = graph[sourceNode!];

        ShortestUndirectedGraphPathResults<string, int> results = ShortestPath<string, int>.Get(src);

        foreach (var kvp in results.Paths)
        {
            Console.WriteLine(kvp.Key.Value + ": " + kvp.Value);
        }

        await Assert.That(results.Paths.Count).IsEqualTo(6);
        await Assert.That(results.Paths[graph["A"]].ToString()).IsEqualTo("{ Cost = 0, Edges = A }");
        await Assert.That(results.Paths[graph["B"]].ToString()).IsEqualTo("{ Cost = 1, Edges = A -> B }");
        await Assert.That(results.Paths[graph["C"]].ToString()).IsEqualTo("{ Cost = 2, Edges = A -> C }");
        await Assert.That(results.Paths[graph["D"]].ToString()).IsEqualTo("{ Cost = 3, Edges = A -> C -> D }");
        await Assert.That(results.Paths[graph["E"]].ToString()).IsEqualTo("{ Cost = 3, Edges = A -> B -> E }");
        await Assert.That(results.Paths[graph["F"]].ToString())
            .IsEqualTo("{ Cost = 6, Edges = A -> C -> D -> F }")
            .Or
            .IsEqualTo("{ Cost = 6, Edges = A -> B -> E -> F }");
    }

    [Test]
    public async Task Test1_Undirected_SingleNode()
    {
        UndirectedGraph<string, int> graph = new UndirectedGraph<string, int>(1);
        UndirectedGraphNode<string, int> src = graph.CreateNode("A");

        ShortestUndirectedGraphPathResults<string, int> results = ShortestPath<string, int>.Get(src);

        foreach (var kvp in results.Paths)
        {
            Console.WriteLine(kvp.Key.Value + ": " + kvp.Value);
        }

        await Assert.That(results.Paths.Count).IsEqualTo(1);
        await Assert.That(results.Paths[graph["A"]].ToString()).IsEqualTo("{ Cost = 0, Edges = A }");
    }

    [Test]

    public async Task Test1_Directed_Single()
    {
        using StreamReader reader = new StreamReader("Assets/sp-2.txt");
        Digraph<string, int> graph = (Digraph<string, int>)GraphFileFormat.ReadGraph<string, int>(reader, out string? sourceNode);

        DigraphNode<string, int> src = graph[sourceNode!];
        DigraphNode<string, int> dst = graph["F"];

        ShortestDigraphPathResults<string, int> results = ShortestPath<string, int>.Get(src, dst);

        await Assert.That(results.Paths).IsEmpty();
        await Assert.That(results.DestinationNodePath).IsNotNull();
        await Assert.That(results.DestinationNodePath!.ToString())
            .Contains("= A -> B -> C -> D -> F }")
            .Or
            .Contains("= A -> B -> E -> F }");
        await Assert.That(results.DestinationNodePath!.TotalCost).IsEqualTo(6);

        Console.WriteLine(results.DestinationNodePath);
    }

    [Test]
    public async Task Test1_Directed_All()
    {
        using StreamReader reader = new StreamReader("Assets/sp-2.txt");
        Digraph<string, int> graph = (Digraph<string, int>)GraphFileFormat.ReadGraph<string, int>(reader, out string? sourceNode);

        DigraphNode<string, int> src = graph[sourceNode!];

        ShortestDigraphPathResults<string, int> results = ShortestPath<string, int>.Get(src);

        foreach (var kvp in results.Paths)
        {
            Console.WriteLine(kvp.Key.Value + ": " + kvp.Value);
        }

        await Assert.That(results.Paths.Count).IsEqualTo(6);
        await Assert.That(results.Paths[graph["A"]].ToString()).IsEqualTo("{ Cost = 0, Edges = A }");
        await Assert.That(results.Paths[graph["B"]].ToString()).IsEqualTo("{ Cost = 1, Edges = A -> B }");
        await Assert.That(results.Paths[graph["C"]].ToString()).IsEqualTo("{ Cost = 2, Edges = A -> B -> C }");
        await Assert.That(results.Paths[graph["D"]].ToString()).IsEqualTo("{ Cost = 3, Edges = A -> B -> C -> D }");
        await Assert.That(results.Paths[graph["E"]].ToString()).IsEqualTo("{ Cost = 3, Edges = A -> B -> E }");
        await Assert.That(results.Paths[graph["F"]].ToString())
            .IsEqualTo("{ Cost = 6, Edges = A -> B -> C -> D -> F }")
            .Or
            .IsEqualTo("{ Cost = 6, Edges = A -> B -> E -> F }");
    }

    [Test]
    public async Task Test1_Directed_SingleNode()
    {
        Digraph<string, int> graph = new Digraph<string, int>(1);
        DigraphNode<string, int> src = graph.CreateNode("A");

        ShortestDigraphPathResults<string, int> results = ShortestPath<string, int>.Get(src);

        foreach (var kvp in results.Paths)
        {
            Console.WriteLine(kvp.Key.Value + ": " + kvp.Value);
        }

        await Assert.That(results.Paths.Count).IsEqualTo(1);
        await Assert.That(results.Paths[graph["A"]].ToString()).IsEqualTo("{ Cost = 0, Edges = A }");
    }

    [Test]
    public async Task UnreachableNode()
    {
        using StreamReader reader = new StreamReader("Assets/sp-3.txt");
        Digraph<string, int> graph = (Digraph<string, int>)GraphFileFormat.ReadGraph<string, int>(reader, out string? sourceNode);

        DigraphNode<string, int> src = graph["4"];

        ShortestDigraphPathResults<string, int> results = ShortestPath<string, int>.Get(src);

        foreach (var kvp in results.Paths)
        {
            Console.WriteLine(kvp.Key.Value + ": " + kvp.Value);
        }

        // unreachable in this graph
        await Assert.That(results.Paths.Keys).DoesNotContain(graph["5"]);
        await Assert.That(results.Paths.Keys).DoesNotContain(graph["9"]);
    }
}