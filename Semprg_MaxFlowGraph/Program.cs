//https://github.com/hakenr/Programiste.CSharp/tree/master/MaxFlow#readme

using System.Diagnostics;
using System.Runtime.InteropServices;

var setting = Parse("input.txt");

// Vytvoříme kopii grafu (reziduální graf) – ten uchovává nejen původní kapacity, ale i aktuální zbytkové kapacity.
// Opakovaně hledáme augmentační cestu (cestu, kterou lze ještě poslat další tok).
// Najdeme minimální kapacitu na této cestě (to je maximální množství toku, které lze přidat).
// Upravíme reziduální graf:
// Snížíme kapacitu hran po směru toku.
// Přidáme zpětné hrany s kapacitou odpovídající již poslanému toku.
// Krok 2-4 opakujeme, dokud neexistuje žádná další augmentační cesta.

var usedFlows = new Dictionary<Edge, int>(setting.EdgeCount);
var maxFlow = 0;
while(true)
{
    // 1. Find augmentation path (available path from source to sink)
    var path = FindAugmentationPath(setting.Graph, setting.Source, setting.Sink, usedFlows)!;

    if(path is null)
    {
        // No path found
        break;
    }
    
    // 2. Find bottleneck (minimum capacity on the path)
    Debug.Assert(path.All(e => e.Capacity - usedFlows.GetValueOrDefault(e, 0) > 0), "Zero residual capacity detected in augmenting path!");
    var minCapacity = path.Min(e =>
        e.Capacity - usedFlows.GetValueOrDefault(e, 0) // Capacity - UsedFlow
    );

    // 3. Augment flow (update used flows)
    foreach (var edge in path)
    {
        if (!usedFlows.ContainsKey(edge))
        {
            usedFlows[edge] = 0;
        }
        
        usedFlows[edge] += minCapacity;
    }

    // 4. Update max flow
    maxFlow += minCapacity;

    // 5. Repeat
}

Console.WriteLine(maxFlow);

Edge[]? FindAugmentationPath(
    Edge[] Graph,
    int source,
    int sink,
    Dictionary<Edge, int> usedFlows)
{
    // 1. Find path
    // Bfs search for path (IGNORE FOR NOW: including residual paths)
    // Look at node
    // Find its neighbors
    // Add all which have flow capacity `(Capacity - UsedFlow) > 0` to queue
    // If we find sink, we are done

    // This will result in finding the shortest augmenting path (in terms of edge count)

    var queue = new Queue<int>();
    var visited = new HashSet<int>();
    var shortestPathNodes = new Dictionary<int, int>(); // Key is to, value is from

    queue.Enqueue(source);
    visited.Add(source);

    while(queue.TryDequeue(out var node))
    {
        // From contains "node", and to contains "neighbor"
        var nodeToNeighborEdges = Graph
            .Where(e => 
                e.From == node  // Find edges from the node
                && !visited.Contains(e.To)
                && e.Capacity - usedFlows.GetValueOrDefault(e, 0) > 0) // Find edges with remaining capacity
            .ToArray(); 

        foreach (var edge in nodeToNeighborEdges)
        {
            if (shortestPathNodes.ContainsKey(edge.To))
                continue;
            
            Debug.Assert(!visited.Contains(edge.To), "Neighbor already visited");
            Debug.Assert(edge.Capacity - usedFlows.GetValueOrDefault(edge, 0) > 0, "Zero residual capacity detected in augmenting path!");

            shortestPathNodes[edge.To] = edge.From;
            visited.Add(edge.To);
            queue.Enqueue(edge.To);
            
            if (edge.To == sink)
            {
                // We found the sink
                return ReconstructEdgePath(
                    ReconstructPath(shortestPathNodes),
                    Graph
                );
            }
        }
    }

    // No path found
    return null;

    ReadOnlySpan<int> ReconstructPath(Dictionary<int, int> parent)
    {
        var reversePath = new List<int>(Graph.Length);
        // Go backwards from sink to source
        var current = sink;
        while (true)
        {
            //reversePath[pathIndex] = current;
            reversePath.Add(current);

            var previous = parent.GetValueOrDefault(current, -1);
            if (previous == -1)
            {
                // No path found
                break;
            }

            current = previous;
        }

        var span = CollectionsMarshal.AsSpan(reversePath);
        span.Reverse();
        return span;
    }

    Edge[] ReconstructEdgePath(ReadOnlySpan<int> path, Edge[] graph)
    {
        var edgePath = new Edge[path.Length - 1];
        for (int i = 0; i < path.Length - 1; i++)
        {
            var from = path[i];
            var to = path[i + 1];
            var edge = graph.First(e => e.From == from && e.To == to);
            edgePath[i] = edge;
        }

        return edgePath;
    }
}

TestSetting Parse(string path)
{
    // Input in format:
    //NumVerts NumEdges
    //Start Target (sink)
    //from1 to1 capacity1
    //from2 to2 capacity2
    //...
    //from_M to_M capacity_M

    var lines = File.ReadAllLines(path);
    var firstLine = lines[0].Split(' ');
    int vertexCount = int.Parse(firstLine[0]);
    int edgeCount = int.Parse(firstLine[1]);

    var secondLine = lines[1].Split(' ');
    int source = int.Parse(secondLine[0]);
    int sink = int.Parse(secondLine[1]);

    var edges = new List<Edge>(edgeCount);
    for (int i = 0; i < edgeCount; i++)
    {
        var edgeLine = lines[i + 2].Split(' ');
        int from = int.Parse(edgeLine[0]);
        int to = int.Parse(edgeLine[1]);
        int capacity = int.Parse(edgeLine[2]);

        var edge = new Edge(from, to, capacity);
        var existingEdge = edges.FirstOrDefault(e => e.From == edge.From && e.To == edge.To);
        if (existingEdge != default)
        {
            // Merge edges
            var existingEdgeI = edges.IndexOf(existingEdge);
            edges[existingEdgeI] = new Edge(existingEdge.From, existingEdge.To, existingEdge.Capacity + edge.Capacity);
        }
        else
        {
            edges.Add(edge);
        }
    }

    return new TestSetting(vertexCount, edgeCount, source, sink, edges.ToArray());
}

readonly record struct TestSetting(int VertexCount, int EdgeCount, int Source, int Sink, Edge[] Graph);
readonly record struct Edge(int From, int To, int Capacity)
{
    public int GetResidualCapacity(int usedFlow) => Capacity - usedFlow;
}
