// Heavily inspired by:
// https://github.com/hakenr/Programiste.CSharp/tree/master/MaxFlow#readme
// and "Edmonds Karp Algorithm | Source Code" - https://www.youtube.com/watch?v=OViaWp9Q-Oc

var setting = Parse("input2.txt");

var solverMy = new NetworkFlowSolverMyOldSketchy();
var solverYT = new NetworkFlowSolverEdmondsKarpYTInspired(setting.VertexCount, setting.Source, setting.Sink, setting.Edges);

Console.WriteLine($"My algorithm max flow is: {solverMy.SolveForMaxFlow(setting)}");
Console.WriteLine($"Youtbe algorithm max flow is: {solverYT.SolveForMaxFlow()}");

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

    var edges = new List<TestSettingEdge>(edgeCount);
    for (int i = 0; i < edgeCount; i++)
    {
        var edgeLine = lines[i + 2].Split(' ');
        int from = int.Parse(edgeLine[0]);
        int to = int.Parse(edgeLine[1]);
        int capacity = int.Parse(edgeLine[2]);

        var edge = new TestSettingEdge(from, to, capacity);
        var existingEdge = edges.FirstOrDefault(e => e.From == edge.From && e.To == edge.To);
        if (existingEdge != default)
        {
            // Merge edges
            var existingEdgeI = edges.IndexOf(existingEdge);
            edges[existingEdgeI] = new TestSettingEdge(existingEdge.From, existingEdge.To, existingEdge.Capacity + edge.Capacity);
        }
        else
        {
            edges.Add(edge);
        }
    }

    return new TestSetting(vertexCount, edgeCount, source, sink, edges.ToArray());
}

readonly record struct TestSetting(int VertexCount, int EdgeCount, int Source, int Sink, TestSettingEdge[] Edges);
readonly record struct TestSettingEdge(int From, int To, int Capacity)
{
    public int GetResidualCapacity(int usedFlow) => Capacity - usedFlow;
}
