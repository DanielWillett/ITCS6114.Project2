using ITCS6114.Project2.Impl.Graphs;

namespace ITCS6114.Project2.Tests;

public class GraphFileFormatTests
{
    [Test]
    public async Task ReadUndirectedGraph()
    {
        using StringReader reader = new StringReader(
            """
            6 10 U
            A B 1
            A C 2
            B C 1
            B D 3
            B E 2
            C D 1
            C E 2
            D E 4
            D F 3
            E F 3
            A
            """
        );

        IGraph readGraph = GraphFileFormat.ReadGraph(reader, out char sourceNode);

        if (readGraph is not UndirectedGraph<char, int> graph)
        {
            Assert.Fail("Not undirected graph.");
            return;
        }

        await Assert.That(sourceNode).IsEqualTo('A');

        await Assert.That(graph.NodeCount).IsEqualTo(6);
        await Assert.That(graph.EdgeCount).IsEqualTo(10);

        UndirectedGraphNode<char, int> a = graph['A'];
        UndirectedGraphNode<char, int> b = graph['B'];
        UndirectedGraphNode<char, int> c = graph['C'];
        UndirectedGraphNode<char, int> d = graph['D'];
        UndirectedGraphNode<char, int> e = graph['E'];
        UndirectedGraphNode<char, int> f = graph['F'];

        await Assert.That(graph.FindPath(a, b)).IsNotNull();
        await Assert.That(graph.FindPath(a, b)!.Cost).IsEqualTo(1);

        await Assert.That(graph.FindPath(a, c)).IsNotNull();
        await Assert.That(graph.FindPath(a, c)!.Cost).IsEqualTo(2);

        await Assert.That(graph.FindPath(b, c)).IsNotNull();
        await Assert.That(graph.FindPath(b, c)!.Cost).IsEqualTo(1);

        await Assert.That(graph.FindPath(b, d)).IsNotNull();
        await Assert.That(graph.FindPath(b, d)!.Cost).IsEqualTo(3);

        await Assert.That(graph.FindPath(b, e)).IsNotNull();
        await Assert.That(graph.FindPath(b, e)!.Cost).IsEqualTo(2);

        await Assert.That(graph.FindPath(c, d)).IsNotNull();
        await Assert.That(graph.FindPath(c, d)!.Cost).IsEqualTo(1);

        await Assert.That(graph.FindPath(c, e)).IsNotNull();
        await Assert.That(graph.FindPath(c, e)!.Cost).IsEqualTo(2);

        await Assert.That(graph.FindPath(d, e)).IsNotNull();
        await Assert.That(graph.FindPath(d, e)!.Cost).IsEqualTo(4);

        await Assert.That(graph.FindPath(d, f)).IsNotNull();
        await Assert.That(graph.FindPath(d, f)!.Cost).IsEqualTo(3);

        await Assert.That(graph.FindPath(e, f)).IsNotNull();
        await Assert.That(graph.FindPath(e, f)!.Cost).IsEqualTo(3);

        StringWriter writer = new StringWriter();

        GraphFileFormat.WriteGraph(graph, writer, sourceNode);

        Console.WriteLine(writer.ToString());
    }

    [Test]
    public async Task ReadDigraph()
    {
        using StringReader reader = new StringReader(
            """
            6 10 D
            A B 1
            A C 2
            B C 1
            B D 3
            B E 2
            C D 1
            C E 2
            D E 4
            D F 3
            E F 3
            A
            """
        );

        IGraph readGraph = GraphFileFormat.ReadGraph(reader, out char sourceNode);

        if (readGraph is not Digraph<char, int> graph)
        {
            Assert.Fail("Not undirected graph.");
            return;
        }

        await Assert.That(sourceNode).IsEqualTo('A');

        await Assert.That(graph.NodeCount).IsEqualTo(6);
        await Assert.That(graph.EdgeCount).IsEqualTo(10);

        DigraphNode<char, int> a = graph['A'];
        DigraphNode<char, int> b = graph['B'];
        DigraphNode<char, int> c = graph['C'];
        DigraphNode<char, int> d = graph['D'];
        DigraphNode<char, int> e = graph['E'];
        DigraphNode<char, int> f = graph['F'];

        await Assert.That(a.FindPathTo(b)).IsNotNull();
        await Assert.That(a.FindPathTo(b)!.Cost).IsEqualTo(1);

        await Assert.That(a.FindPathTo(c)).IsNotNull();
        await Assert.That(a.FindPathTo(c)!.Cost).IsEqualTo(2);

        await Assert.That(b.FindPathTo(c)).IsNotNull();
        await Assert.That(b.FindPathTo(c)!.Cost).IsEqualTo(1);

        await Assert.That(b.FindPathTo(d)).IsNotNull();
        await Assert.That(b.FindPathTo(d)!.Cost).IsEqualTo(3);

        await Assert.That(b.FindPathTo(e)).IsNotNull();
        await Assert.That(b.FindPathTo(e)!.Cost).IsEqualTo(2);

        await Assert.That(c.FindPathTo(d)).IsNotNull();
        await Assert.That(c.FindPathTo(d)!.Cost).IsEqualTo(1);

        await Assert.That(c.FindPathTo(e)).IsNotNull();
        await Assert.That(c.FindPathTo(e)!.Cost).IsEqualTo(2);

        await Assert.That(d.FindPathTo(e)).IsNotNull();
        await Assert.That(d.FindPathTo(e)!.Cost).IsEqualTo(4);

        await Assert.That(d.FindPathTo(f)).IsNotNull();
        await Assert.That(d.FindPathTo(f)!.Cost).IsEqualTo(3);

        await Assert.That(e.FindPathTo(f)).IsNotNull();
        await Assert.That(e.FindPathTo(f)!.Cost).IsEqualTo(3);

        StringWriter writer = new StringWriter();

        GraphFileFormat.WriteGraph(graph, writer, sourceNode);

        Console.WriteLine(writer.ToString());
    }
}
