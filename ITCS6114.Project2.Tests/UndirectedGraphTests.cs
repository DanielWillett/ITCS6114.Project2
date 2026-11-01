using UndirectedGraph = ITCS6114.Project2.Impl.Graphs.UndirectedGraph<string?, int>;
using UndirectedGraphEdge = ITCS6114.Project2.Impl.Graphs.UndirectedGraphEdge<string?, int>;
using UndirectedGraphNode = ITCS6114.Project2.Impl.Graphs.UndirectedGraphNode<string?, int>;
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedVariable

namespace ITCS6114.Project2.Tests;

public class UndirectedGraphTests
{
    [Test]
    public async Task SingleNode()
    {
        UndirectedGraph graph = new UndirectedGraph();

        await Assert.That(graph.NodeCount).IsZero();
        await Assert.That(graph.EdgeCount).IsZero();

        UndirectedGraphNode node = graph.CreateNode(null);

        await Assert.That(graph.NodeCount).IsEqualTo(1);
        await Assert.That(graph.EdgeCount).IsZero();

        node.Remove();

        await Assert.That(graph.NodeCount).IsZero();
        await Assert.That(graph.EdgeCount).IsZero();

        graph.AddNode(node);

        await Assert.That(graph.NodeCount).IsEqualTo(1);
        await Assert.That(graph.EdgeCount).IsZero();

        graph.Clear();

        await Assert.That(graph.NodeCount).IsZero();
        await Assert.That(graph.EdgeCount).IsZero();
    }

    [Test]
    public async Task SingleEdge()
    {
        UndirectedGraph graph = new UndirectedGraph();

        UndirectedGraphNode nodeA = graph.CreateNode("A");
        UndirectedGraphNode nodeB = graph.CreateNode("B");

        await Assert.That(graph.NodeCount).IsEqualTo(2);
        await Assert.That(graph.EdgeCount).IsZero();

        const int cost = 1;

        graph.CreatePath(nodeA, nodeB, cost);

        await Assert.That(nodeA.Count).IsEqualTo(1);
        await Assert.That(nodeB.Count).IsEqualTo(1);

        await Assert.That(graph.NodeCount).IsEqualTo(2);
        await Assert.That(graph.EdgeCount).IsEqualTo(1);

        UndirectedGraphEdge? foundPath = graph.FindPath(nodeA, nodeB);

        await Assert.That(foundPath).IsNotNull();
        await Assert.That(foundPath!.Cost).IsEqualTo(cost);
        await Assert.That(foundPath.IsRemoved).IsFalse();

        foundPath = graph.FindPath(nodeB, nodeA);

        await Assert.That(foundPath).IsNotNull();

        foundPath!.Remove();

        await Assert.That(graph.NodeCount).IsEqualTo(2);
        await Assert.That(graph.EdgeCount).IsZero();
        await Assert.That(foundPath.IsRemoved).IsTrue();

        await Assert.That(nodeA.Count).IsEqualTo(0);
        await Assert.That(nodeB.Count).IsEqualTo(0);

        foundPath = graph.FindPath(nodeA, nodeB);

        await Assert.That(foundPath).IsNull();

        foundPath = graph.FindPath(nodeB, nodeA);

        await Assert.That(foundPath).IsNull();

        graph.Clear();

        await Assert.That(graph.NodeCount).IsZero();
        await Assert.That(graph.EdgeCount).IsZero();
    }

    [Test]
    public async Task SingleSelfLoop()
    {
        UndirectedGraph graph = new UndirectedGraph();

        UndirectedGraphNode nodeA = graph.CreateNode("A");

        await Assert.That(graph.NodeCount).IsEqualTo(1);
        await Assert.That(graph.EdgeCount).IsZero();

        const int cost = 1;

        graph.CreatePath(nodeA, nodeA, cost);

        await Assert.That(nodeA.Count).IsEqualTo(2);

        await Assert.That(graph.NodeCount).IsEqualTo(1);
        await Assert.That(graph.EdgeCount).IsEqualTo(1);

        UndirectedGraphEdge? foundPath = graph.FindPath(nodeA, nodeA);

        await Assert.That(foundPath).IsNotNull();
        await Assert.That(foundPath!.Cost).IsEqualTo(cost);
        await Assert.That(foundPath.IsRemoved).IsFalse();

        foundPath.Remove();

        await Assert.That(graph.NodeCount).IsEqualTo(1);
        await Assert.That(graph.EdgeCount).IsZero();
        await Assert.That(foundPath.IsRemoved).IsTrue();

        await Assert.That(nodeA.Count).IsEqualTo(0);

        foundPath = graph.FindPath(nodeA, nodeA);

        await Assert.That(foundPath).IsNull();

        graph.Clear();

        await Assert.That(graph.NodeCount).IsZero();
        await Assert.That(graph.EdgeCount).IsZero();
    }

    [Test]
    public async Task SingleEdgeCleared()
    {
        UndirectedGraph graph = new UndirectedGraph();

        UndirectedGraphNode nodeA = graph.CreateNode("A");
        UndirectedGraphNode nodeB = graph.CreateNode("B");

        const int cost = 1;

        graph.CreatePath(nodeA, nodeB, cost);

        UndirectedGraphEdge? foundPath = graph.FindPath(nodeA, nodeB);

        await Assert.That(foundPath).IsNotNull();

        graph.Clear();

        await Assert.That(foundPath!.IsRemoved).IsTrue();
        await Assert.That(graph.NodeCount).IsZero();
        await Assert.That(graph.EdgeCount).IsZero();
        await Assert.That(nodeA.Count).IsEqualTo(0);
        await Assert.That(nodeB.Count).IsEqualTo(0);
    }

    [Test]
    public async Task AddingAndRemovingNodes()
    {
        UndirectedGraph graph = new UndirectedGraph();

        UndirectedGraphNode nodeA = graph.CreateNode("A");
        UndirectedGraphNode nodeB = graph.CreateNode("B");
        UndirectedGraphNode nodeC = graph.CreateNode("C");

        await Assert.That(graph["A"]).IsEqualTo(nodeA);
        await Assert.That(graph["B"]).IsEqualTo(nodeB);
        await Assert.That(graph["C"]).IsEqualTo(nodeC);
        await Assert.That(graph.TryGetNode("D", out _)).IsFalse();

        nodeA.Remove();

        await Assert.That(graph.NodeCount).IsEqualTo(2);

        nodeC.Remove();

        await Assert.That(graph.NodeCount).IsEqualTo(1);

        graph.Clear();

        nodeA = graph.CreateNode("A");
        nodeB = graph.CreateNode("B");
        nodeC = graph.CreateNode("C");

        nodeB.Remove();

        await Assert.That(graph.NodeCount).IsEqualTo(2);

        using UndirectedGraph.GraphEnumerator enumerator = graph.GetEnumerator();

        await Assert.That(enumerator.MoveNext()).IsTrue();

        await Assert.That(enumerator.Current).IsEqualTo(nodeA);

        await Assert.That(enumerator.MoveNext()).IsTrue();

        await Assert.That(enumerator.Current).IsEqualTo(nodeC);

        await Assert.That(enumerator.MoveNext()).IsFalse();

        graph.Clear();

        await Assert.That(graph.NodeCount).IsZero();
    }

    [Test]
    public async Task EnumeratesNodesInOrder()
    {
        UndirectedGraph graph = new UndirectedGraph();

        UndirectedGraphNode nodeB = graph.CreateNode("B");
        UndirectedGraphNode nodeA = graph.CreateNode("A");
        UndirectedGraphNode nodeC = graph.CreateNode("C");

        using UndirectedGraph.GraphEnumerator enumerator = graph.GetEnumerator();

        await Assert.That(enumerator.MoveNext()).IsTrue();

        await Assert.That(enumerator.Current).IsEqualTo(nodeA);

        await Assert.That(enumerator.MoveNext()).IsTrue();

        await Assert.That(enumerator.Current).IsEqualTo(nodeB);

        await Assert.That(enumerator.MoveNext()).IsTrue();

        await Assert.That(enumerator.Current).IsEqualTo(nodeC);

        await Assert.That(enumerator.MoveNext()).IsFalse();
    }

    [Test]
    public async Task EnumeratesEdges()
    {
        UndirectedGraph graph = new UndirectedGraph();

        UndirectedGraphNode nodeA = graph.CreateNode("A");
        UndirectedGraphNode nodeB = graph.CreateNode("B");
        UndirectedGraphNode nodeC = graph.CreateNode("C");

        const int cost = 4;

        UndirectedGraphEdge aAndB = graph.CreatePath(nodeA, nodeB, cost);
        UndirectedGraphEdge aAndC = graph.CreatePath(nodeA, nodeC, cost);
        UndirectedGraphEdge bAndC = graph.CreatePath(nodeB, nodeC, cost);
        UndirectedGraphEdge aAndA = graph.CreatePath(nodeA, nodeA, cost);
        UndirectedGraphEdge bAndB = graph.CreatePath(nodeB, nodeB, cost);
        UndirectedGraphEdge cAndC = graph.CreatePath(nodeC, nodeC, cost);

        using (UndirectedGraphNode.ConnectionEnumerator enumerator = nodeA.GetEnumerator())
        {
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(aAndA);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(aAndA);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(aAndC);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(aAndB);
            await Assert.That(enumerator.MoveNext()).IsFalse();
        }

        using (UndirectedGraphNode.ConnectionEnumerator enumerator = nodeB.GetEnumerator())
        {
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(bAndB);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(bAndB);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(bAndC);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(aAndB);
            await Assert.That(enumerator.MoveNext()).IsFalse();
        }

        using (UndirectedGraphNode.ConnectionEnumerator enumerator = nodeC.GetEnumerator())
        {
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(cAndC);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(cAndC);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(bAndC);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(aAndC);
            await Assert.That(enumerator.MoveNext()).IsFalse();
        }

        using (UndirectedGraphNode.ConnectionEnumerator enumerator = nodeA.GetEnumerator(selfLoopsOnlyOnce: true))
        {
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(aAndA);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(aAndC);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(aAndB);
            await Assert.That(enumerator.MoveNext()).IsFalse();
        }

        using (UndirectedGraphNode.ConnectionEnumerator enumerator = nodeB.GetEnumerator(selfLoopsOnlyOnce: true))
        {
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(bAndB);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(bAndC);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(aAndB);
            await Assert.That(enumerator.MoveNext()).IsFalse();
        }

        using (UndirectedGraphNode.ConnectionEnumerator enumerator = nodeC.GetEnumerator(selfLoopsOnlyOnce: true))
        {
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(cAndC);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(bAndC);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(aAndC);
            await Assert.That(enumerator.MoveNext()).IsFalse();
        }
    }

    [Test]
    public async Task RemoveMultipleEdges()
    {
        UndirectedGraph graph = new UndirectedGraph();

        UndirectedGraphNode nodeA = graph.CreateNode("A");
        UndirectedGraphNode nodeB = graph.CreateNode("B");
        UndirectedGraphNode nodeC = graph.CreateNode("C");

        const int cost = 4;

        UndirectedGraphEdge aAndB = graph.CreatePath(nodeA, nodeB, cost);
        UndirectedGraphEdge aAndC = graph.CreatePath(nodeA, nodeC, cost);
        UndirectedGraphEdge bAndC = graph.CreatePath(nodeB, nodeC, cost);
        UndirectedGraphEdge aAndA = graph.CreatePath(nodeA, nodeA, cost);
        UndirectedGraphEdge bAndB = graph.CreatePath(nodeB, nodeB, cost);
        UndirectedGraphEdge cAndC = graph.CreatePath(nodeC, nodeC, cost);

        await Assert.That(nodeA.Count).IsEqualTo(4);
        await Assert.That(nodeB.Count).IsEqualTo(4);
        await Assert.That(nodeC.Count).IsEqualTo(4);

        nodeA.RemoveAllPaths();

        await Assert.That(nodeA.Count).IsEqualTo(0);
        await Assert.That(nodeB.Count).IsEqualTo(3);
        await Assert.That(nodeC.Count).IsEqualTo(3);

        nodeB.RemoveAllPaths();

        await Assert.That(nodeA.Count).IsEqualTo(0);
        await Assert.That(nodeB.Count).IsEqualTo(0);
        await Assert.That(nodeC.Count).IsEqualTo(2);
    }
}
