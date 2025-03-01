// Heavily inspired by:
// and "Edmonds Karp Algorithm | Source Code" - https://www.youtube.com/watch?v=OViaWp9Q-Oc

class NetworkFlowSolverEdmondsKarpYTInspired
{
    // What an interesting way for keeping track of visited nodes :)
    // - node_i is visited if visited[i] == visitedToken
    // - to reset visited nodes, increment visitedToken
    private int visitedToken = 1;
    private int[] visited;

    private int maxFlow;

    /// <summary>
    /// Element at i returns all edges outgoing from that node_i. <br/>
    /// [from_i] -> list of edges where .From is [from_i]
    /// </summary>
    private List<Edge>[] graph;

    private readonly int _source;
    private readonly int _sink;
    private readonly int _vertexCount;

    public NetworkFlowSolverEdmondsKarpYTInspired(int vertexCount, int source, int sink, TestSettingEdge[] tEdges)
    {
        _vertexCount = vertexCount;
        _source = source;
        _sink = sink;
        visited = new int[vertexCount];

        // Create edges
        graph = Enumerable.Range(0, vertexCount)
            .Select(_ => new List<Edge>())
            .ToArray();

        foreach (var tEdge in tEdges)
        {
            var edge = new Edge(tEdge.From, tEdge.To, tEdge.Capacity);
            var residual = new Edge(tEdge.To, tEdge.From, capacity: 0);
            
            edge.Residual = residual;
            residual.Residual = edge;
        
            graph[edge.From].Add(edge);
            graph[residual.From].Add(residual);
        }
    }

    private void MarkNodeVisited(int i)
        => visited[i] = visitedToken;

    private bool IsNodeVisited(int i)
        => visited[i] == visitedToken;
    private void ClearVisited()
        => visitedToken++;

    public int SolveForMaxFlow()
    {
        maxFlow = 0;
        int bottleNeck;

        // Find augmenting path and augment the flow
        // until no more augmenting paths are found
        do
        {
            bottleNeck = FindAugmentingPathBFS();
            maxFlow += bottleNeck;
            ClearVisited();
        } while (bottleNeck != 0);
        
        return maxFlow;
    }


    /// <summary>
    /// Find augmenting path using BFS.
    /// Marks visited nodes and returns the bottleNeck value.
    /// Augments the edges on the path.
    /// </summary>
    private int FindAugmentingPathBFS()
    {
        var queue = new Queue<int>(_vertexCount);

        MarkNodeVisited(_source);
        queue.Enqueue(_source);

        // For rebuilding the path
        // indexing at node_i returns the edge that led to node_i
        var prevPath = new Edge[_vertexCount];
        
        while (queue.TryDequeue(out var node))
        {
            if(node == _sink)
                break;

            // For each edge outgoing from node
            foreach (var edge in graph[node])
            {
                var capacity = edge.RemainingCapacity();
                if (capacity > 0 && !IsNodeVisited(edge.To))
                {
                    MarkNodeVisited(edge.To);
                    prevPath[edge.To] = edge;
                    queue.Enqueue(edge.To);
                }
            }
        }

        // No found edge leading to sink
        if (prevPath[_sink] == null)
            return 0;

        // -> Augmenting path exists
        // Find bottleneck
        var bottleNeck = int.MaxValue;
        for (int i = _sink;;)
        {
            var prevEdge = prevPath[i];
            if (prevEdge == null)
                break;

            bottleNeck = Math.Min(bottleNeck, prevEdge.RemainingCapacity());
            i = prevEdge.From;
        }

        // Augment by bottleneck
        for (int i = _sink;;)
        {
            var prevEdge = prevPath[i];
            if (prevEdge == null)
                break;

            prevEdge.Augment(bottleNeck);
            i = prevEdge.From;
        }

        return bottleNeck;
    }

    public class Edge
    {
        public int From { get; init; }
        public int To { get; init; }
        public int Flow { get; private set; }
        public Edge Residual { get; set; }
        public int Capacity { get; init; }

        public Edge(int from, int to, int capacity)
        {
            From = from;
            To = to;
            Capacity = capacity;
        }

        public bool IsResidual() 
            => Capacity == 0;

        public int RemainingCapacity()
            => Capacity - Flow;

        public void Augment(int bottleNeck)
        {
            Flow += bottleNeck;
            Residual.Flow -= bottleNeck;
        }
    }

}
