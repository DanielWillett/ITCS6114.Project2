using ITCS6114.Project2.Impl.Algorithms;
using ITCS6114.Project2.Impl.Graphs;

namespace ITCS6114.Project2.Tests;

public class TopologicalSortTests
{
    private static void CheckValidResult(Digraph<string, int> graph, string[] sortedValues)
    {
        for (int i = 0; i < sortedValues.Length; ++i)
        {
            string value = sortedValues[i];

            DigraphNode<string, int> node = graph[value];
            for (int j = 0; j < i; ++j)
            {
                DigraphNode<string, int> prevNode = graph[sortedValues[j]];
                if (prevNode.IsParentOf(node))
                {
                    Assert.Fail($"{node.Value} should come before {prevNode.Value}");
                }
            }
        }
    }

    [Test]
    public void Test1_Directed()
    {
        // example from handout
        using StreamReader reader = new StreamReader("Assets/ts-1.txt");
        Digraph<string, int> graph = (Digraph<string, int>)GraphFileFormat.ReadGraph<string, int>(reader, out _);

        string[] sorted = TopologicalSort<string, int>.Sort(graph);
        foreach (string str in sorted)
        {
            Console.Write(str);
            Console.Write(' ');
        }
        
        Console.WriteLine();

        // string[] expected = [ "p", "n", "o", "s", "m", "r", "y", "v", "x", "w", "z", "u", "q", "t" ];
        
        CheckValidResult(graph, sorted);
    }

    [Test]
    public void Test2_Directed()
    {
        // example from sheets
        using StreamReader reader = new StreamReader("Assets/ts-2.txt");
        Digraph<string, int> graph = (Digraph<string, int>)GraphFileFormat.ReadGraph<string, int>(reader, out _);

        string[] sorted = TopologicalSort<string, int>.Sort(graph);
        foreach (string str in sorted)
        {
            Console.Write(str);
            Console.Write(' ');
        }
        
        Console.WriteLine();

        // string[] expected = [ "socks", "undershorts", "pants", "shoes", "watch", "shirt", "belt", "tie", "jacket" ];

        CheckValidResult(graph, sorted);
    }
}