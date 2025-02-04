namespace OpenDental.Chart;

public class VertexNormal
{
    public Vertex3 Vertex;
    public Vertex3 Normal;

    public override string ToString()
    {
        return "v:" + Vertex + " n:" + Normal;
    }
}