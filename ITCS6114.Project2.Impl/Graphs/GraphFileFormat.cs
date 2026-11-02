using System.ComponentModel;
using System.Globalization;
using System.Numerics;

namespace ITCS6114.Project2.Impl.Graphs;

public static class GraphFileFormat
{
    public enum ReadMode
    {
        DetectFromHeader,
        Undirected,
        Directed
    }
    
    /// <summary>
    /// Read a graph from a <see cref="TextReader"/>.
    /// </summary>
    /// <exception cref="FormatException">Malformed data.</exception>
    public static IGraph ReadGraph<TData, TCost>(TextReader reader, out TData? sourceNodeValue, ReadMode mode = ReadMode.DetectFromHeader)
        where TData : ISpanParsable<TData>
        where TCost : ISpanParsable<TCost>, INumber<TCost>
    {
        string? firstLine = reader.ReadLine();

        if (firstLine == null)
            throw new FormatException("Missing header.");

        int nodeCt = -1, edgeCt = -1;
        bool isDirected = mode == ReadMode.Directed;
        if (mode == ReadMode.DetectFromHeader)
        {
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

            if (!int.TryParse(nodeCtStr, out nodeCt))
                throw new FormatException("Malformed header. Invalid node count.");

            if (!int.TryParse(edgeCtStr, out edgeCt))
                throw new FormatException("Malformed header. Invalid edge count.");

            if (typeStr.Length != 1 || typeStr[0] is not 'U' and not 'D')
                throw new FormatException("Malformed header. Invalid graph type.");

            isDirected = typeStr[0] == 'D';
        }
        else if (!isDirected && mode != ReadMode.Undirected)
        {
            throw new InvalidEnumArgumentException(nameof(mode), (int)mode, typeof(ReadMode));
        }

        return isDirected
            ? ReadDigraphIntl<TData, TCost>(reader, nodeCt, edgeCt, out sourceNodeValue)
            : ReadUndirectedGraphIntl<TData, TCost>(reader, nodeCt, edgeCt, out sourceNodeValue);
    }

    /// <summary>
    /// Write a directed graph to a <see cref="TextWriter"/>.
    /// </summary>
    public static void WriteGraph<TData, TCost>(Digraph<TData, TCost> graph, TextWriter writer, DigraphNode<TData, TCost>? sourceNode = null)
        where TCost : INumber<TCost>
    {
        writer.Write(graph.NodeCount);
        writer.Write(' ');
        writer.Write(graph.EdgeCount);
        writer.WriteLine(" D");

        HashSet<TData> dataNotWritten = new HashSet<TData>(graph.Select(x => x.Value));

        foreach (DigraphNode<TData, TCost> node in graph)
        {
            foreach (DigraphEdge<TData, TCost> edge in node)
            {
                writer.Write(ValueToString(node.Value));
                writer.Write(' ');
                writer.Write(ValueToString(edge.To.Value));
                writer.Write(' ');
                writer.WriteLine(edge.Cost.ToString(null, CultureInfo.InvariantCulture));
                dataNotWritten.Remove(node.Value);
                dataNotWritten.Remove(edge.To.Value);
            }
        }

        foreach (TData data in dataNotWritten)
        {
            writer.WriteLine(ValueToString(data));
        }

        if (sourceNode != null)
        {
            writer.Write(ValueToString(sourceNode.Value));
        }
    }

    /// <summary>
    /// Write a directed graph to a <see cref="TextWriter"/>.
    /// </summary>
    public static void WriteGraph<TData, TCost>(UndirectedGraph<TData, TCost> graph, TextWriter writer, UndirectedGraphNode<TData, TCost>? sourceNode = null)
        where TCost : INumber<TCost>
    {
        writer.Write(graph.NodeCount);
        writer.Write(' ');
        writer.Write(graph.EdgeCount);
        writer.WriteLine(" U");

        HashSet<(TData, TData)> edgesAlreadyWritten = new HashSet<(TData, TData)>();
        HashSet<TData> dataNotWritten = new HashSet<TData>(graph.Select(x => x.Value));

        foreach (UndirectedGraphNode<TData, TCost> node in graph)
        {
            foreach (UndirectedGraphEdge<TData, TCost> edge in node)
            {
                if (!edgesAlreadyWritten.Add((edge.A.Value, edge.B.Value)))
                    continue;

                writer.Write(ValueToString(edge.A.Value));
                writer.Write(' ');
                writer.Write(ValueToString(edge.B.Value));
                writer.Write(' ');
                writer.WriteLine(edge.Cost.ToString(null, CultureInfo.InvariantCulture));
                dataNotWritten.Remove(edge.A.Value);
                dataNotWritten.Remove(edge.B.Value);
            }
        }

        foreach (TData data in dataNotWritten)
        {
            writer.WriteLine(ValueToString(data));
        }

        if (sourceNode != null)
        {
            writer.Write(ValueToString(sourceNode.Value));
        }
    }

    private static string ValueToString<TData>(TData val)
    {
        if (val is IFormattable fmt)
            return fmt.ToString(null, CultureInfo.InvariantCulture);

        return val?.ToString() ?? string.Empty;
    }

    private static Digraph<TData, TCost> ReadDigraphIntl<TData, TCost>(TextReader reader, int nodeCt, int edgeCt, out TData? sourceNodeValue)
        where TData : ISpanParsable<TData>
        where TCost : ISpanParsable<TCost>, INumber<TCost>
    {
        Span<Range> dest = stackalloc Range[3];

        int lineNum = 2;

        sourceNodeValue = default;

        TCost defaultCost = TCost.CreateChecked(1);

        Digraph<TData, TCost> graph = new Digraph<TData, TCost>(Math.Max(0, nodeCt));
        TData? lastLine = default;
        bool hasLastLine = false;
        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            hasLastLine = false;

            ReadOnlySpan<char> lineSpan = line;

            int splitCt = lineSpan.Split(dest, ' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (splitCt > 3)
                throw new FormatException($"Line {lineNum} malformed, expected 1, 2, or 3 values.");

            ReadOnlySpan<char> nodeAStr = lineSpan[dest[0]];

            if (!TData.TryParse(nodeAStr, CultureInfo.InvariantCulture, out TData? dataA))
                throw new FormatException($"Line {lineNum} malformed, first node invalid.");

            if (!graph.TryGetNode(dataA, out DigraphNode<TData, TCost>? nodeAObj))
                nodeAObj = graph.CreateNode(dataA);

            if (splitCt == 1)
            {
                lastLine = dataA;
                hasLastLine = true;
                continue;
            }

            ReadOnlySpan<char> nodeBStr = lineSpan[dest[1]];

            if (!TData.TryParse(nodeBStr, CultureInfo.InvariantCulture, out TData? dataB))
                throw new FormatException($"Line {lineNum} malformed, second node invalid.");

            if (!graph.TryGetNode(dataB, out DigraphNode<TData, TCost>? nodeBObj))
                nodeBObj = graph.CreateNode(dataB);

            TCost? cost = defaultCost;
            if (splitCt > 2)
            {
                ReadOnlySpan<char> costStr = lineSpan[dest[2]];
                if (!TCost.TryParse(costStr, CultureInfo.InvariantCulture, out cost))
                    throw new FormatException($"Line {lineNum} malformed, invalid cost.");
            }

            if (nodeAObj.FindPathTo(nodeBObj) != null)
                throw new FormatException($"Duplicate edge ({nodeAStr} ==> {nodeBStr}) on line {lineNum}.");

            nodeAObj.CreatePathTo(nodeBObj, cost);

            ++lineNum;
        }

        if (hasLastLine)
        {
            sourceNodeValue = lastLine;
        }

        if (nodeCt >= 0 && graph.NodeCount != nodeCt)
        {
            throw new FormatException($"Number of nodes listed ({graph.NodeCount}) doesn't match expected node count ({nodeCt}).");
        }

        if (edgeCt >= 0 && graph.EdgeCount != edgeCt)
        {
            throw new FormatException($"Number of edges listed ({graph.EdgeCount}) doesn't match expected edge count ({edgeCt}).");
        }

        return graph;
    }

    private static UndirectedGraph<TData, TCost> ReadUndirectedGraphIntl<TData, TCost>(TextReader reader, int nodeCt, int edgeCt, out TData? sourceNodeValue)
        where TData : ISpanParsable<TData>
        where TCost : ISpanParsable<TCost>, INumber<TCost>
    {
        Span<Range> dest = stackalloc Range[3];

        int lineNum = 2;

        sourceNodeValue = default;

        TCost defaultCost = TCost.CreateChecked(1);

        UndirectedGraph<TData, TCost> graph = new UndirectedGraph<TData, TCost>(Math.Max(0, nodeCt));
        TData? lastLine = default;
        bool hasLastLine = false;
        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            hasLastLine = false;

            ReadOnlySpan<char> lineSpan = line;

            int splitCt = lineSpan.Split(dest, ' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (splitCt > 3)
                throw new FormatException($"Line {lineNum} malformed, expected 1, 2, or 3 values.");

            ReadOnlySpan<char> nodeAStr = lineSpan[dest[0]];

            if (!TData.TryParse(nodeAStr, CultureInfo.InvariantCulture, out TData? dataA))
                throw new FormatException($"Line {lineNum} malformed, first node invalid.");

            if (!graph.TryGetNode(dataA, out UndirectedGraphNode<TData, TCost>? nodeAObj))
                nodeAObj = graph.CreateNode(dataA);

            if (splitCt == 1)
            {
                lastLine = dataA;
                hasLastLine = true;
                continue;
            }

            ReadOnlySpan<char> nodeBStr = lineSpan[dest[1]];

            if (!TData.TryParse(nodeBStr, CultureInfo.InvariantCulture, out TData? dataB))
                throw new FormatException($"Line {lineNum} malformed, second node invalid.");

            if (!graph.TryGetNode(dataB, out UndirectedGraphNode<TData, TCost>? nodeBObj))
                nodeBObj = graph.CreateNode(dataB);

            TCost? cost = defaultCost;
            if (splitCt > 2)
            {
                ReadOnlySpan<char> costStr = lineSpan[dest[2]];
                if (!TCost.TryParse(costStr, CultureInfo.InvariantCulture, out cost))
                    throw new FormatException($"Line {lineNum} malformed, invalid cost.");
            }

            if (graph.FindPath(nodeAObj, nodeBObj) != null)
                throw new FormatException($"Duplicate edge ({nodeAStr} <==> {nodeBStr}) on line {lineNum}.");

            graph.CreatePath(nodeAObj, nodeBObj, cost);

            ++lineNum;
        }

        if (hasLastLine)
        {
            sourceNodeValue = lastLine;
        }

        if (nodeCt >= 0 && graph.NodeCount != nodeCt)
        {
            throw new FormatException($"Number of nodes listed ({graph.NodeCount}) doesn't match expected node count ({nodeCt}).");
        }

        if (edgeCt >= 0 && graph.EdgeCount != edgeCt)
        {
            throw new FormatException($"Number of edges listed ({graph.EdgeCount}) doesn't match expected edge count ({edgeCt}).");
        }

        return graph;
    }
}