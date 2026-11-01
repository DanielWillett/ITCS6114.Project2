namespace ITCS6114.Project2.Impl.Graphs;

public static class GraphFileFormat
{
    /// <summary>
    /// Read a graph from a <see cref="TextReader"/>.
    /// </summary>
    /// <exception cref="FormatException">Malformed data.</exception>
    public static IGraph ReadGraph(TextReader reader, out char sourceNodeValue)
    {
        string? firstLine = reader.ReadLine();

        if (firstLine == null)
            throw new FormatException("Missing header.");

        ReadOnlySpan<char> firstLineSpan = firstLine;


        Span<Range> dest = stackalloc Range[3];
        int splitCt = firstLineSpan.Split(dest, ' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (splitCt != 3)
        {
            throw new FormatException("Malformed header. Expected 3 values.");
        }

        ReadOnlySpan<char> nodeCtStr = firstLineSpan[dest[0]];
        ReadOnlySpan<char> edgeCtStr = firstLineSpan[dest[1]];
        ReadOnlySpan<char> typeStr = firstLineSpan[dest[2]];

        if (!int.TryParse(nodeCtStr, out int nodeCt))
            throw new FormatException("Malformed header. Invalid node count.");

        if (!int.TryParse(edgeCtStr, out int edgeCt))
            throw new FormatException("Malformed header. Invalid edge count.");

        if (typeStr.Length != 1 || typeStr[0] is not 'U' and not 'D')
            throw new FormatException("Malformed header. Invalid graph type.");

        return typeStr[0] == 'D'
            ? ReadDigraphIntl(reader, nodeCt, edgeCt, out sourceNodeValue)
            : ReadUndirectedGraphIntl(reader, nodeCt, edgeCt, out sourceNodeValue);
    }

    /// <summary>
    /// Write a directed graph to a <see cref="TextWriter"/>.
    /// </summary>
    public static void WriteGraph(Digraph<char, int> graph, TextWriter writer, char sourceNode)
    {
        writer.Write(graph.NodeCount);
        writer.Write(' ');
        writer.Write(graph.EdgeCount);
        writer.WriteLine(" D");

        foreach (DigraphNode<char, int> node in graph)
        {
            foreach (DigraphEdge<char, int> edge in node)
            {
                writer.Write(node.Value);
                writer.Write(' ');
                writer.Write(edge.To.Value);
                writer.Write(' ');
                writer.WriteLine(edge.Cost);
            }
        }

        if (sourceNode != '\0')
        {
            writer.Write(sourceNode);
        }
    }

    /// <summary>
    /// Write a directed graph to a <see cref="TextWriter"/>.
    /// </summary>
    public static void WriteGraph(UndirectedGraph<char, int> graph, TextWriter writer, char sourceNode)
    {
        writer.Write(graph.NodeCount);
        writer.Write(' ');
        writer.Write(graph.EdgeCount);
        writer.WriteLine(" U");

        HashSet<(char, char)> edgesAlreadyWritten = new HashSet<(char, char)>();

        foreach (UndirectedGraphNode<char, int> node in graph)
        {
            foreach (UndirectedGraphEdge<char, int> edge in node)
            {
                if (!edgesAlreadyWritten.Add((edge.A.Value, edge.B.Value)))
                    continue;

                writer.Write(edge.A.Value);
                writer.Write(' ');
                writer.Write(edge.B.Value);
                writer.Write(' ');
                writer.WriteLine(edge.Cost);
            }
        }

        if (sourceNode != '\0')
        {
            writer.Write(sourceNode);
        }
    }

    private static Digraph<char, int> ReadDigraphIntl(TextReader reader, int nodeCt, int edgeCt, out char sourceNodeValue)
    {
        Span<Range> dest = stackalloc Range[3];

        int lineNum = 2;

        sourceNodeValue = '\0';

        Digraph<char, int> graph = new Digraph<char, int>(nodeCt);
        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            ReadOnlySpan<char> lineSpan = line;

            int splitCt = lineSpan.Split(dest, ' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (splitCt == 1)
            {
                while (reader.ReadLine() is { } line2)
                {
                    if (!string.IsNullOrEmpty(line2))
                        throw new FormatException($"Line {lineNum} malformed, expected 3 values.");
                }

                ReadOnlySpan<char> sourceNodeStr = lineSpan[dest[0]];
                if (sourceNodeStr.Length != 1)
                {
                    throw new FormatException($"Line {lineNum} malformed, source node not one character.");
                }

                sourceNodeValue = sourceNodeStr[0];
                break;
            }

            if (splitCt != 3)
                throw new FormatException($"Line {lineNum} malformed, expected 3 values.");

            ReadOnlySpan<char> nodeAStr = lineSpan[dest[0]];
            ReadOnlySpan<char> nodeBStr = lineSpan[dest[1]];
            ReadOnlySpan<char> costStr = lineSpan[dest[2]];

            if (nodeAStr.Length != 1)
                throw new FormatException($"Line {lineNum} malformed, first node not one character.");

            if (nodeBStr.Length != 1)
                throw new FormatException($"Line {lineNum} malformed, second node not one character.");

            if (!int.TryParse(costStr, out int cost))
                throw new FormatException($"Line {lineNum} malformed, invalid cost.");

            char nodeA = nodeAStr[0], nodeB = nodeBStr[0];

            if (!graph.TryGetNode(nodeA, out DigraphNode<char, int>? nodeAObj))
                nodeAObj = graph.CreateNode(nodeA);

            if (!graph.TryGetNode(nodeB, out DigraphNode<char, int>? nodeBObj))
                nodeBObj = graph.CreateNode(nodeB);

            if (nodeAObj.FindPathTo(nodeBObj) != null)
                throw new FormatException($"Duplicate edge on line {lineNum}.");

            nodeAObj.CreatePathTo(nodeBObj, cost);

            ++lineNum;
        }

        if (graph.NodeCount != nodeCt)
        {
            throw new FormatException($"Number of nodes listed ({graph.NodeCount}) doesn't match expected node count ({nodeCt}).");
        }

        if (graph.EdgeCount != edgeCt)
        {
            throw new FormatException($"Number of edges listed ({graph.EdgeCount}) doesn't match expected edge count ({edgeCt}).");
        }

        return graph;
    }

    private static UndirectedGraph<char, int> ReadUndirectedGraphIntl(TextReader reader, int nodeCt, int edgeCt, out char sourceNodeValue)
    {
        Span<Range> dest = stackalloc Range[3];

        int lineNum = 2;

        sourceNodeValue = '\0';

        UndirectedGraph<char, int> graph = new UndirectedGraph<char, int>(nodeCt);
        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            ReadOnlySpan<char> lineSpan = line;

            int splitCt = lineSpan.Split(dest, ' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (splitCt == 1)
            {
                while (reader.ReadLine() is { } line2)
                {
                    if (!string.IsNullOrEmpty(line2))
                        throw new FormatException($"Line {lineNum} malformed, expected 3 values.");
                }

                ReadOnlySpan<char> sourceNodeStr = lineSpan[dest[0]];
                if (sourceNodeStr.Length != 1)
                {
                    throw new FormatException($"Line {lineNum} malformed, source node not one character.");
                }

                sourceNodeValue = sourceNodeStr[0];
                break;
            }

            if (splitCt != 3)
                throw new FormatException($"Line {lineNum} malformed, expected 3 values.");

            ReadOnlySpan<char> nodeAStr = lineSpan[dest[0]];
            ReadOnlySpan<char> nodeBStr = lineSpan[dest[1]];
            ReadOnlySpan<char> costStr = lineSpan[dest[2]];

            if (nodeAStr.Length != 1)
                throw new FormatException($"Line {lineNum} malformed, first node not one character.");

            if (nodeBStr.Length != 1)
                throw new FormatException($"Line {lineNum} malformed, second node not one character.");

            if (!int.TryParse(costStr, out int cost))
                throw new FormatException($"Line {lineNum} malformed, invalid cost.");

            char nodeA = nodeAStr[0], nodeB = nodeBStr[0];

            if (!graph.TryGetNode(nodeA, out UndirectedGraphNode<char, int>? nodeAObj))
                nodeAObj = graph.CreateNode(nodeA);

            if (!graph.TryGetNode(nodeB, out UndirectedGraphNode<char, int>? nodeBObj))
                nodeBObj = graph.CreateNode(nodeB);

            if (graph.FindPath(nodeAObj, nodeBObj) != null)
                throw new FormatException($"Duplicate edge on line {lineNum}.");

            graph.CreatePath(nodeAObj, nodeBObj, cost);

            ++lineNum;
        }

        if (graph.NodeCount != nodeCt)
        {
            throw new FormatException($"Number of nodes listed ({graph.NodeCount}) doesn't match expected node count ({nodeCt}).");
        }

        if (graph.EdgeCount != edgeCt)
        {
            throw new FormatException($"Number of edges listed ({graph.EdgeCount}) doesn't match expected edge count ({edgeCt}).");
        }

        return graph;
    }
}
