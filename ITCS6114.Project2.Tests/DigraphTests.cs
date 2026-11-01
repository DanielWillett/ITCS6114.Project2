using Digraph = ITCS6114.Project2.Impl.Graphs.Digraph<string?, int>;
using DigraphEdge = ITCS6114.Project2.Impl.Graphs.DigraphEdge<string?, int>;
using DigraphNode = ITCS6114.Project2.Impl.Graphs.DigraphNode<string?, int>;
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedVariable

namespace ITCS6114.Project2.Tests;

public class DigraphTests
{
    [Test]
    public async Task SingleNode()
    {
        Digraph digraph = new Digraph();

        await Assert.That(digraph.NodeCount).IsZero();
        await Assert.That(digraph.EdgeCount).IsZero();

        DigraphNode node = digraph.CreateNode(null);

        await Assert.That(digraph.NodeCount).IsEqualTo(1);
        await Assert.That(digraph.EdgeCount).IsZero();

        node.Remove();

        await Assert.That(digraph.NodeCount).IsZero();
        await Assert.That(digraph.EdgeCount).IsZero();

        digraph.AddNode(node);

        await Assert.That(digraph.NodeCount).IsEqualTo(1);
        await Assert.That(digraph.EdgeCount).IsZero();

        digraph.Clear();

        await Assert.That(digraph.NodeCount).IsZero();
        await Assert.That(digraph.EdgeCount).IsZero();
    }

    [Test]
    public async Task SingleEdge()
    {
        Digraph digraph = new Digraph();

        DigraphNode nodeA = digraph.CreateNode("A");
        DigraphNode nodeB = digraph.CreateNode("B");

        await Assert.That(digraph.NodeCount).IsEqualTo(2);
        await Assert.That(digraph.EdgeCount).IsZero();

        const int cost = 1;

        nodeA.CreatePathTo(nodeB, cost);

        await Assert.That(nodeA.InCount).IsEqualTo(0);
        await Assert.That(nodeA.OutCount).IsEqualTo(1);
        await Assert.That(nodeB.InCount).IsEqualTo(1);
        await Assert.That(nodeB.OutCount).IsEqualTo(0);

        await Assert.That(digraph.NodeCount).IsEqualTo(2);
        await Assert.That(digraph.EdgeCount).IsEqualTo(1);

        DigraphEdge? foundPath = nodeA.FindPathTo(nodeB);

        await Assert.That(foundPath).IsNotNull();
        await Assert.That(foundPath!.Cost).IsEqualTo(cost);
        await Assert.That(foundPath.IsRemoved).IsFalse();

        foundPath = nodeB.FindPathFrom(nodeA);

        await Assert.That(foundPath).IsNotNull();

        foundPath!.Remove();

        await Assert.That(digraph.NodeCount).IsEqualTo(2);
        await Assert.That(digraph.EdgeCount).IsZero();
        await Assert.That(foundPath.IsRemoved).IsTrue();

        await Assert.That(nodeA.InCount).IsEqualTo(0);
        await Assert.That(nodeA.OutCount).IsEqualTo(0);
        await Assert.That(nodeB.InCount).IsEqualTo(0);
        await Assert.That(nodeB.OutCount).IsEqualTo(0);

        foundPath = nodeA.FindPathTo(nodeB);

        await Assert.That(foundPath).IsNull();

        foundPath = nodeB.FindPathFrom(nodeA);

        await Assert.That(foundPath).IsNull();

        digraph.Clear();

        await Assert.That(digraph.NodeCount).IsZero();
        await Assert.That(digraph.EdgeCount).IsZero();
    }

    [Test]
    public async Task SingleSelfLoop()
    {
        Digraph digraph = new Digraph();

        DigraphNode nodeA = digraph.CreateNode("A");

        await Assert.That(digraph.NodeCount).IsEqualTo(1);
        await Assert.That(digraph.EdgeCount).IsZero();

        const int cost = 1;

        nodeA.CreatePathTo(nodeA, cost);

        await Assert.That(nodeA.InCount).IsEqualTo(1);
        await Assert.That(nodeA.OutCount).IsEqualTo(1);

        await Assert.That(digraph.NodeCount).IsEqualTo(1);
        await Assert.That(digraph.EdgeCount).IsEqualTo(1);

        DigraphEdge? foundPath = nodeA.FindPathTo(nodeA);

        await Assert.That(foundPath).IsNotNull();
        await Assert.That(foundPath!.Cost).IsEqualTo(cost);
        await Assert.That(foundPath.IsRemoved).IsFalse();

        foundPath.Remove();

        await Assert.That(digraph.NodeCount).IsEqualTo(1);
        await Assert.That(digraph.EdgeCount).IsZero();
        await Assert.That(foundPath.IsRemoved).IsTrue();

        await Assert.That(nodeA.InCount).IsEqualTo(0);
        await Assert.That(nodeA.OutCount).IsEqualTo(0);

        foundPath = nodeA.FindPathTo(nodeA);

        await Assert.That(foundPath).IsNull();

        digraph.Clear();

        await Assert.That(digraph.NodeCount).IsZero();
        await Assert.That(digraph.EdgeCount).IsZero();
    }

    [Test]
    public async Task SingleEdgeCleared()
    {
        Digraph digraph = new Digraph();

        DigraphNode nodeA = digraph.CreateNode("A");
        DigraphNode nodeB = digraph.CreateNode("B");

        const int cost = 1;

        nodeA.CreatePathTo(nodeB, cost);

        DigraphEdge? foundPath = nodeA.FindPathTo(nodeB);

        await Assert.That(foundPath).IsNotNull();

        digraph.Clear();

        await Assert.That(foundPath!.IsRemoved).IsTrue();
        await Assert.That(digraph.NodeCount).IsZero();
        await Assert.That(digraph.EdgeCount).IsZero();
        await Assert.That(nodeA.InCount).IsEqualTo(0);
        await Assert.That(nodeA.OutCount).IsEqualTo(0);
        await Assert.That(nodeB.InCount).IsEqualTo(0);
        await Assert.That(nodeB.OutCount).IsEqualTo(0);
    }

    [Test]
    public async Task AddingAndRemovingNodes()
    {
        Digraph digraph = new Digraph();

        DigraphNode nodeA = digraph.CreateNode("A");
        DigraphNode nodeB = digraph.CreateNode("B");
        DigraphNode nodeC = digraph.CreateNode("C");

        await Assert.That(digraph["A"]).IsEqualTo(nodeA);
        await Assert.That(digraph["B"]).IsEqualTo(nodeB);
        await Assert.That(digraph["C"]).IsEqualTo(nodeC);
        await Assert.That(digraph.TryGetNode("D", out _)).IsFalse();

        nodeA.Remove();

        await Assert.That(digraph.NodeCount).IsEqualTo(2);

        nodeC.Remove();

        await Assert.That(digraph.NodeCount).IsEqualTo(1);

        digraph.Clear();

        nodeA = digraph.CreateNode("A");
        nodeB = digraph.CreateNode("B");
        nodeC = digraph.CreateNode("C");

        nodeB.Remove();

        await Assert.That(digraph.NodeCount).IsEqualTo(2);

        using Digraph.GraphEnumerator enumerator = digraph.GetEnumerator();

        await Assert.That(enumerator.MoveNext()).IsTrue();

        await Assert.That(enumerator.Current).IsEqualTo(nodeA);

        await Assert.That(enumerator.MoveNext()).IsTrue();

        await Assert.That(enumerator.Current).IsEqualTo(nodeC);

        await Assert.That(enumerator.MoveNext()).IsFalse();

        digraph.Clear();

        await Assert.That(digraph.NodeCount).IsZero();
    }

    [Test]
    public async Task EnumeratesNodesInOrder()
    {
        Digraph digraph = new Digraph();

        DigraphNode nodeB = digraph.CreateNode("B");
        DigraphNode nodeA = digraph.CreateNode("A");
        DigraphNode nodeC = digraph.CreateNode("C");

        using Digraph.GraphEnumerator enumerator = digraph.GetEnumerator();

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
        Digraph digraph = new Digraph();

        DigraphNode nodeA = digraph.CreateNode("A");
        DigraphNode nodeB = digraph.CreateNode("B");
        DigraphNode nodeC = digraph.CreateNode("C");

        const int cost = 4;

        DigraphEdge aToA = nodeA.CreatePathTo(nodeA, cost);
        DigraphEdge aToB = nodeA.CreatePathTo(nodeB, cost);
        DigraphEdge aToC = nodeA.CreatePathTo(nodeC, cost);

        DigraphEdge bToA = nodeB.CreatePathTo(nodeA, cost);
        DigraphEdge bToB = nodeB.CreatePathTo(nodeB, cost);
        DigraphEdge bToC = nodeB.CreatePathTo(nodeC, cost);

        DigraphEdge cToA = nodeC.CreatePathTo(nodeA, cost);
        DigraphEdge cToB = nodeC.CreatePathTo(nodeB, cost);
        DigraphEdge cToC = nodeC.CreatePathTo(nodeC, cost);

        using (DigraphNode.ConnectionEnumerator enumerator = nodeA.GetEnumerator())
        {
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(aToC);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(aToB);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(aToA);
            await Assert.That(enumerator.MoveNext()).IsFalse();
        }

        using (DigraphNode.ConnectionEnumerator enumerator = nodeB.GetEnumerator())
        {
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(bToC);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(bToB);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(bToA);
            await Assert.That(enumerator.MoveNext()).IsFalse();
        }

        using (DigraphNode.ConnectionEnumerator enumerator = nodeC.GetEnumerator())
        {
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(cToC);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(cToB);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(cToA);
            await Assert.That(enumerator.MoveNext()).IsFalse();
        }

        using (DigraphNode.ConnectionEnumerator enumerator = nodeA.EnumerateIncomingConnections())
        {
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(cToA);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(bToA);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(aToA);
            await Assert.That(enumerator.MoveNext()).IsFalse();
        }

        using (DigraphNode.ConnectionEnumerator enumerator = nodeB.EnumerateIncomingConnections())
        {
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(cToB);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(bToB);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(aToB);
            await Assert.That(enumerator.MoveNext()).IsFalse();
        }

        using (DigraphNode.ConnectionEnumerator enumerator = nodeC.EnumerateIncomingConnections())
        {
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(cToC);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(bToC);
            await Assert.That(enumerator.MoveNext()).IsTrue();
            await Assert.That(enumerator.Current).IsEqualTo(aToC);
            await Assert.That(enumerator.MoveNext()).IsFalse();
        }
    }

    [Test]
    public async Task RemoveMultipleEdges()
    {
        Digraph digraph = new Digraph();

        DigraphNode nodeA = digraph.CreateNode("A");
        DigraphNode nodeB = digraph.CreateNode("B");
        DigraphNode nodeC = digraph.CreateNode("C");

        const int cost = 4;

        DigraphEdge aToA = nodeA.CreatePathTo(nodeA, cost);
        DigraphEdge aToB = nodeA.CreatePathTo(nodeB, cost);
        DigraphEdge aToC = nodeA.CreatePathTo(nodeC, cost);

        DigraphEdge bToA = nodeB.CreatePathTo(nodeA, cost);
        DigraphEdge bToB = nodeB.CreatePathTo(nodeB, cost);
        DigraphEdge bToC = nodeB.CreatePathTo(nodeC, cost);

        DigraphEdge cToA = nodeC.CreatePathTo(nodeA, cost);
        DigraphEdge cToB = nodeC.CreatePathTo(nodeB, cost);
        DigraphEdge cToC = nodeC.CreatePathTo(nodeC, cost);

        await Assert.That(nodeA.InCount).IsEqualTo(3);
        await Assert.That(nodeB.InCount).IsEqualTo(3);
        await Assert.That(nodeC.InCount).IsEqualTo(3);

        await Assert.That(nodeA.OutCount).IsEqualTo(3);
        await Assert.That(nodeB.OutCount).IsEqualTo(3);
        await Assert.That(nodeC.OutCount).IsEqualTo(3);

        nodeA.RemoveAllIncomingPaths();

        await Assert.That(nodeA.InCount).IsEqualTo(0);
        await Assert.That(nodeB.InCount).IsEqualTo(3);
        await Assert.That(nodeC.InCount).IsEqualTo(3);

        await Assert.That(nodeA.OutCount).IsEqualTo(2);
        await Assert.That(nodeB.OutCount).IsEqualTo(2);
        await Assert.That(nodeC.OutCount).IsEqualTo(2);

        nodeA.RemoveAllOutgoingPaths();

        await Assert.That(nodeA.InCount).IsEqualTo(0);
        await Assert.That(nodeB.InCount).IsEqualTo(2);
        await Assert.That(nodeC.InCount).IsEqualTo(2);

        await Assert.That(nodeA.OutCount).IsEqualTo(0);
        await Assert.That(nodeB.OutCount).IsEqualTo(2);
        await Assert.That(nodeC.OutCount).IsEqualTo(2);

        nodeB.RemoveAllIncomingPaths();

        await Assert.That(nodeA.InCount).IsEqualTo(0);
        await Assert.That(nodeB.InCount).IsEqualTo(0);
        await Assert.That(nodeC.InCount).IsEqualTo(2);

        await Assert.That(nodeA.OutCount).IsEqualTo(0);
        await Assert.That(nodeB.OutCount).IsEqualTo(1);
        await Assert.That(nodeC.OutCount).IsEqualTo(1);

        nodeB.RemoveAllOutgoingPaths();

        await Assert.That(nodeA.InCount).IsEqualTo(0);
        await Assert.That(nodeB.InCount).IsEqualTo(0);
        await Assert.That(nodeC.InCount).IsEqualTo(1);

        await Assert.That(nodeA.OutCount).IsEqualTo(0);
        await Assert.That(nodeB.OutCount).IsEqualTo(0);
        await Assert.That(nodeC.OutCount).IsEqualTo(1);

        nodeC.RemoveAllIncomingPaths();

        await Assert.That(nodeA.InCount).IsEqualTo(0);
        await Assert.That(nodeB.InCount).IsEqualTo(0);
        await Assert.That(nodeC.InCount).IsEqualTo(0);

        await Assert.That(nodeA.OutCount).IsEqualTo(0);
        await Assert.That(nodeB.OutCount).IsEqualTo(0);
        await Assert.That(nodeC.OutCount).IsEqualTo(0);

        nodeC.RemoveAllOutgoingPaths();

        await Assert.That(nodeA.InCount).IsEqualTo(0);
        await Assert.That(nodeB.InCount).IsEqualTo(0);
        await Assert.That(nodeC.InCount).IsEqualTo(0);

        await Assert.That(nodeA.OutCount).IsEqualTo(0);
        await Assert.That(nodeB.OutCount).IsEqualTo(0);
        await Assert.That(nodeC.OutCount).IsEqualTo(0);
    }
}
