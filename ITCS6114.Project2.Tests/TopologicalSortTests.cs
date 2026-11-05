using ITCS6114.Project2.Impl.Algorithms;
using ITCS6114.Project2.Impl.Graphs;

namespace ITCS6114.Project2.Tests;

public class TopologicalSortTests
{
    private async Task CheckValidResult(Digraph<string, int> graph, string[] sortedValues)
    {
        for (int i = 0; i < sortedValues.Length; ++i)
        {
            string value = sortedValues[i];

            DigraphNode<string, int> node = graph[value];
            for (int j = 0; j < i; ++j)
            {
                DigraphNode<string, int> prevNode = graph[sortedValues[j]];
                if (node.IsParentOf(prevNode))
                {
                    Assert.Fail($"{node.Value} should come after  {prevNode.Value}");
                }
            }
        }
    }

    [Test]
    public async Task Test1_Directed()
    {
        // example from slides
        using StreamReader reader = new StreamReader("Assets/ts-1.txt");
        Digraph<string, int> graph = (Digraph<string, int>)GraphFileFormat.ReadGraph<string, int>(reader, out _);

        string[] sorted = TopologicalSort<string, int>.Sort(graph);
        foreach (string str in sorted)
        {
            Console.Write(str);
            Console.Write(' ');
        }
        
        Console.WriteLine();

        // string[] expected = [ "u", "v", "y", "x", "w", "z" ];

        await CheckValidResult(graph, sorted);
    }

    [Test]
    public async Task Test2_Directed()
    {
        // example from handout
        using StreamReader reader = new StreamReader("Assets/ts-2.txt");
        Digraph<string, int> graph = (Digraph<string, int>)GraphFileFormat.ReadGraph<string, int>(reader, out _);

        string[] sorted = TopologicalSort<string, int>.Sort(graph);
        foreach (string str in sorted)
        {
            Console.Write(str);
            Console.Write(' ');
        }
        
        Console.WriteLine();

        // string[] expected = [ "p", "n", "o", "s", "m", "r", "y", "v", "x", "w", "z", "u", "q", "t" ];

        await CheckValidResult(graph, sorted);
    }

    [Test]
    public async Task Test3_Directed()
    {
        // example from sheets
        using StreamReader reader = new StreamReader("Assets/ts-3.txt");
        Digraph<string, int> graph = (Digraph<string, int>)GraphFileFormat.ReadGraph<string, int>(reader, out _);

        string[] sorted = TopologicalSort<string, int>.Sort(graph);
        foreach (string str in sorted)
        {
            Console.Write(str);
            Console.Write(' ');
        }
        
        Console.WriteLine();

        // string[] expected = [ "socks", "undershorts", "pants", "shoes", "watch", "shirt", "belt", "tie", "jacket" ];

        await CheckValidResult(graph, sorted);
    }
}