namespace OpenDental.Chart;

public record struct Vertex3
{
    public float X;
    public float Y;
    public float Z;

    public Vertex3()
    {
    }

    public Vertex3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public float[] GetFloatArray()
    {
        var array = new float[3];
            
        array[0] = X;
        array[1] = Y;
        array[2] = Z;
            
        return array;
    }

    public override string ToString()
    {
        return X + "," + Y + "," + Z;
    }

    public Vertex3 Copy()
    {
        return new Vertex3(X, Y, Z);
    }
}