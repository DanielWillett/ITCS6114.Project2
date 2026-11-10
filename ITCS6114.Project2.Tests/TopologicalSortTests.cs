using System.Collections;
using System.Collections.Immutable;
using ITCS6114.Project2.Impl.Algorithms;
using ITCS6114.Project2.Impl.Graphs;
using TUnit.Assertions.Enums;

namespace ITCS6114.Project2.Tests;

public class TopologicalSortTests
{
    private static void CheckValidResult(DigraphNode<string, int>[] sortedValues)
    {
        for (int i = 0; i < sortedValues.Length; ++i)
        {
            DigraphNode<string, int> node = sortedValues[i];
            for (int j = 0; j < i; ++j)
            {
                DigraphNode<string, int> prevNode = sortedValues[j];
                if (prevNode.IsParentOf(node))
                {
                    Assert.Fail($"{node.Value} should come before {prevNode.Value}");
                }
            }
        }
    }

    [Test]
    public async Task Test1_Directed_Acyclic()
    {
        // example from handout
        using StreamReader reader = new StreamReader("Assets/ts-1.txt");
        Digraph<string, int> graph = (Digraph<string, int>)GraphFileFormat.ReadGraph<string, int>(reader, out _);

        TopologicalSort<string, int>.Result res = TopologicalSort<string, int>.Sort(graph);

        await Assert.That(res.ContainsCycles).IsFalse();

        foreach (DigraphNode<string, int> node in res.Sorted!)
        {
            Console.Write(node.Value);
            Console.Write(' ');
        }
        
        Console.WriteLine();

        // string[] expected = [ "p", "n", "o", "s", "m", "r", "y", "v", "x", "w", "z", "u", "q", "t" ];
        
        CheckValidResult(res.Sorted);
    }

    [Test]
    public async Task Test2_Directed_Acyclic()
    {
        // example from slides
        using StreamReader reader = new StreamReader("Assets/ts-2.txt");
        Digraph<string, int> graph = (Digraph<string, int>)GraphFileFormat.ReadGraph<string, int>(reader, out _);

        TopologicalSort<string, int>.Result res = TopologicalSort<string, int>.Sort(graph);

        await Assert.That(res.ContainsCycles).IsFalse();

        foreach (DigraphNode<string, int> node in res.Sorted!)
        {
            Console.Write(node.Value);
            Console.Write(' ');
        }

        Console.WriteLine();

        // string[] expected = [ "socks", "undershorts", "pants", "shoes", "watch", "shirt", "belt", "tie", "jacket" ];

        CheckValidResult(res.Sorted);
    }

    [Test]
    public async Task Test3_Directed_Cyclic()
    {
        // example from slides
        using StreamReader reader = new StreamReader("Assets/ts-3.txt");
        Digraph<string, int> graph = (Digraph<string, int>)GraphFileFormat.ReadGraph<string, int>(reader, out _);

        TopologicalSort<string, int>.Result res = TopologicalSort<string, int>.Sort(graph);

        await Assert.That(res.ContainsCycles).IsTrue();

        foreach (TopologicalSort<string, int>.Cycle cycle in res.Cycles)
        {
            bool first = true;
            foreach (DigraphNode<string, int> node in cycle.Nodes)
            {
                if (first)
                    first = false;
                else
                    Console.Write(" -> ");

                Console.Write(node.Value);
            }

            Console.WriteLine();
        }

        await Assert.That(res.Cycles).Count().IsEqualTo(2);
        await Assert.That(res.Cycles[0].Nodes).IsEquivalentTo([ graph["v"], graph["y"], graph["x"] ], CollectionOrdering.Matching);
        await Assert.That(res.Cycles[1].Nodes).IsEquivalentTo([ graph["z"] ], CollectionOrdering.Matching);

        // string[] expected = [ "socks", "undershorts", "pants", "shoes", "watch", "shirt", "belt", "tie", "jacket" ];
    }

    [Test]
    public async Task Test4_Directed_Cyclic_NoInNodes()
    {
        using StreamReader reader = new StreamReader("Assets/ts-4.txt");
        Digraph<string, int> graph = (Digraph<string, int>)GraphFileFormat.ReadGraph<string, int>(reader, out _);

        TopologicalSort<string, int>.Result res = TopologicalSort<string, int>.Sort(graph);

        await Assert.That(res.ContainsCycles).IsTrue();

        foreach (TopologicalSort<string, int>.Cycle cycle in res.Cycles)
        {
            bool first = true;
            foreach (DigraphNode<string, int> node in cycle.Nodes)
            {
                if (first)
                    first = false;
                else
                    Console.Write(" -> ");

                Console.Write(node.Value);
            }

            Console.WriteLine();
        }

        await Assert.That(res.Cycles).Count().IsEqualTo(1);

        DigraphNode<string, int>[][] possible =
        [
            [ graph["0"], graph["1"], graph["2"], graph["3"] ],
            [ graph["1"], graph["2"], graph["3"], graph["0"] ],
            [ graph["2"], graph["3"], graph["0"], graph["1"] ],
            [ graph["3"], graph["0"], graph["1"], graph["2"] ]
        ];

        TopologicalSort<string, int>.Cycle c = res.Cycles[0];

        bool any = false;
        foreach (DigraphNode<string, int>[] option in possible)
        {
            try
            {
                await Assert.That(res.Cycles[0].Nodes).IsEquivalentTo(option, CollectionOrdering.Matching);
                any = true;
                break;
            }
            catch
            {
                // ignored
            }
        }

        await Assert.That(any).IsTrue();
    }
}