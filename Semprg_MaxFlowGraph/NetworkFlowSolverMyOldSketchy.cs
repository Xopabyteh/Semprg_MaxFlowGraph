// Heavily inspired by https://github.com/hakenr/Programiste.CSharp/tree/master/MaxFlow#readme

using System.Diagnostics;
using System.Runtime.InteropServices;

/// <summary>
/// This solver is sketch, because it does not handle residual paths.
/// But due to nature of the BFS, it seems work anyway
/// </summary>
readonly struct NetworkFlowSolverMyOldSketchy
{
    public int SolveForMaxFlow(TestSetting setting)
    {
        var usedFlows = new Dictionary<TestSettingEdge, int>(setting.EdgeCount);
        var maxFlow = 0;
        while (true)
        {
            // 1. Find augmentation path (available path from source to sink)
            var path = FindAugmentationPath(setting.Edges, setting.Source, setting.Sink, usedFlows)!;

            if (path is null)
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

        return maxFlow;
    }

    TestSettingEdge[]? FindAugmentationPath(
        TestSettingEdge[] Edges,
        int source,
        int sink,
        Dictionary<TestSettingEdge, int> usedFlows)
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
        var shortestPathNodes = new Dictionary<int, int>(); // Key is "to", value is "from"

        queue.Enqueue(source);
        visited.Add(source);

        while (queue.TryDequeue(out var node))
        {
            // `From` contains "node", and `To` contains "neighbor"
            var nodeToNeighborTestSettingEdges = Edges
                .Where(e =>
                    e.From == node  // Find edges from the node
                    && !visited.Contains(e.To)
                    && e.Capacity - usedFlows.GetValueOrDefault(e, 0) > 0) // Find edges with remaining capacity
                .ToArray();

            foreach (var edge in nodeToNeighborTestSettingEdges)
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
                        ReconstructPath(shortestPathNodes, Edges.Length, sink),
                        Edges
                    );
                }
            }
        }

        // No path found
        return null;
    }

    ReadOnlySpan<int> ReconstructPath(Dictionary<int, int> parent, int edgeCount, int sink)
    {
        var reversePath = new List<int>(edgeCount);
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

    TestSettingEdge[] ReconstructEdgePath(ReadOnlySpan<int> path, TestSettingEdge[] graph)
    {
        var edgePath = new TestSettingEdge[path.Length - 1];
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