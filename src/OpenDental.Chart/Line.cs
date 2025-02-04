using System.Collections.Generic;

namespace OpenDental.Chart;

public class LineSimple
{
    public readonly List<Vertex3> Vertices;

    public LineSimple()
    {
        Vertices = [];
    }

    public LineSimple(params float[] coords)
    {
        Vertices = [];
        var vertex = new Vertex3();
        for (var i = 0; i < coords.Length; i++)
        {
            vertex.X = coords[i];
            i++;
            vertex.Y = coords[i];
            i++;
            vertex.Z = coords[i];
            Vertices.Add(vertex);
            vertex = new Vertex3();
        }
    }
}