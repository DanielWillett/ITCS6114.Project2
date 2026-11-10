using ITCS6114.Project2.Impl.Algorithms;
using ITCS6114.Project2.Impl.Graphs;

namespace ITCS6114.Project2.Tests;

public class MinimumSpanningTreeTests
{
    [Test]
    public async Task Test1()
    {
        // test graph from https://www.geeksforgeeks.org/dsa/kruskals-minimum-spanning-tree-algorithm-greedy-algo-2/

        using StreamReader reader = new StreamReader("Assets/mst-1.txt");
        UndirectedGraph<int, int> graph = (UndirectedGraph<int, int>)GraphFileFormat.ReadGraph<int, int>(reader, out _);

        UndirectedGraph<int, int> mst = MinimumSpanningTree<int, int>.Get(graph, out int totalCost);

        GraphFileFormat.WriteGraph(mst, Console.Out);
        Console.WriteLine();

        await Assert.That(mst.NodeCount).IsEqualTo(mst.NodeCount);
        await Assert.That(mst.EdgeCount).IsEqualTo(mst.NodeCount - 1);

        await Assert.That(mst.FindPath(mst[0], mst[1])).IsNotNull();
        await Assert.That(mst.FindPath(mst[0], mst[1])!.Cost).IsEqualTo(4);

        // two solutions are valid
        try
        {
            await Assert.That(mst.FindPath(mst[1], mst[2])).IsNotNull();
            await Assert.That(mst.FindPath(mst[1], mst[2])!.Cost).IsEqualTo(8);
        }
        catch
        {
            await Assert.That(mst.FindPath(mst[0], mst[7])).IsNotNull();
            await Assert.That(mst.FindPath(mst[0], mst[7])!.Cost).IsEqualTo(8);
        }

        await Assert.That(mst.FindPath(mst[6], mst[7])).IsNotNull();
        await Assert.That(mst.FindPath(mst[6], mst[7])!.Cost).IsEqualTo(1);

        await Assert.That(mst.FindPath(mst[5], mst[6])).IsNotNull();
        await Assert.That(mst.FindPath(mst[5], mst[6])!.Cost).IsEqualTo(2);

        await Assert.That(mst.FindPath(mst[2], mst[5])).IsNotNull();
        await Assert.That(mst.FindPath(mst[2], mst[5])!.Cost).IsEqualTo(4);

        await Assert.That(mst.FindPath(mst[2], mst[8])).IsNotNull();
        await Assert.That(mst.FindPath(mst[2], mst[8])!.Cost).IsEqualTo(2);

        await Assert.That(mst.FindPath(mst[2], mst[3])).IsNotNull();
        await Assert.That(mst.FindPath(mst[2], mst[3])!.Cost).IsEqualTo(7);

        await Assert.That(mst.FindPath(mst[3], mst[4])).IsNotNull();
        await Assert.That(mst.FindPath(mst[3], mst[4])!.Cost).IsEqualTo(9);

        await Assert.That(totalCost).IsEqualTo(37);

        GraphFileFormat.WriteGraph(mst, Console.Out);
        Console.Out.WriteLine();
    }

    [Test]
    public async Task Test2()
    {
        // airport graph from slides

        using StreamReader reader = new StreamReader("Assets/mst-2.txt");
        UndirectedGraph<string, int> graph = (UndirectedGraph<string, int>)GraphFileFormat.ReadGraph<string, int>(reader, out _);

        UndirectedGraph<string, int> mst = MinimumSpanningTree<string, int>.Get(graph, out int totalCost);

        await Assert.That(mst.NodeCount).IsEqualTo(mst.NodeCount);
        await Assert.That(mst.EdgeCount).IsEqualTo(mst.NodeCount - 1);

        await Assert.That(mst.FindPath(mst["BOS"], mst["JFK"])).IsNotNull();
        await Assert.That(mst.FindPath(mst["BOS"], mst["JFK"])!.Cost).IsEqualTo(187);

        await Assert.That(mst.FindPath(mst["PVD"], mst["JFK"])).IsNotNull();
        await Assert.That(mst.FindPath(mst["PVD"], mst["JFK"])!.Cost).IsEqualTo(144);

        await Assert.That(mst.FindPath(mst["JFK"], mst["BWI"])).IsNotNull();
        await Assert.That(mst.FindPath(mst["JFK"], mst["BWI"])!.Cost).IsEqualTo(184);

        await Assert.That(mst.FindPath(mst["BWI"], mst["MIA"])).IsNotNull();
        await Assert.That(mst.FindPath(mst["BWI"], mst["MIA"])!.Cost).IsEqualTo(946);

        await Assert.That(mst.FindPath(mst["BWI"], mst["ORD"])).IsNotNull();
        await Assert.That(mst.FindPath(mst["BWI"], mst["ORD"])!.Cost).IsEqualTo(621);

        await Assert.That(mst.FindPath(mst["ORD"], mst["DFW"])).IsNotNull();
        await Assert.That(mst.FindPath(mst["ORD"], mst["DFW"])!.Cost).IsEqualTo(802);

        await Assert.That(mst.FindPath(mst["DFW"], mst["LAX"])).IsNotNull();
        await Assert.That(mst.FindPath(mst["DFW"], mst["LAX"])!.Cost).IsEqualTo(1235);

        await Assert.That(mst.FindPath(mst["LAX"], mst["SFO"])).IsNotNull();
        await Assert.That(mst.FindPath(mst["LAX"], mst["SFO"])!.Cost).IsEqualTo(337);

        await Assert.That(totalCost).IsEqualTo(4456);

        GraphFileFormat.WriteGraph(mst, Console.Out);
        Console.Out.WriteLine();
    }
}