using System.Collections.Generic;
using System.Drawing;
using SharpDX.Direct3D9;

namespace OpenDental.Chart;

public class ToothGroup
{
    public bool Visible;
    public Color PaintColor;
    public ToothGroupType GroupType;
    public List<Face> Faces = [];
    public IndexBuffer FacesDirectX;
    public int NumIndicies;

    public void PrepareForDirectX(Device device)
    {
        CleanupDirectX();

        var triangles = new List<int>();
        foreach (var face in Faces)
        {
            for (var j = 1; j < face.IndexList.Count - 1; j++)
            {
                triangles.Add(face.IndexList[0]);
                triangles.Add(face.IndexList[j]);
                triangles.Add(face.IndexList[j + 1]);
            }
        }

        var uniqueIndicies = new List<int>();
        foreach (var t in triangles)
        {
            if (uniqueIndicies.IndexOf(t) < 0)
            {
                uniqueIndicies.Add(t);
            }
        }


        var optimizedIndices = D3DX.OptimizeFaces(triangles.ToArray(), triangles.Count / 3, uniqueIndicies.Count);
        var optimalTriangles = new List<int>();
        
        foreach (var index in optimizedIndices)
        {
            var firstVertexIndex = 3 * index;
            
            optimalTriangles.Add(triangles[firstVertexIndex]);
            optimalTriangles.Add(triangles[firstVertexIndex + 1]);
            optimalTriangles.Add(triangles[firstVertexIndex + 2]);
        }

        var arrayTriangles = optimalTriangles.ToArray();
        FacesDirectX = new IndexBuffer(device, sizeof(int) * arrayTriangles.Length, Usage.None, Pool.Managed, false);
        var ds = FacesDirectX.Lock(0, 0, LockFlags.None);
        ds.WriteRange(arrayTriangles);
        FacesDirectX.Unlock();
        NumIndicies = arrayTriangles.Length;
    }

    public void DrawDirectX(Device device, ToothGraphic parentGraphic)
    {
        device.Indices = FacesDirectX;
        device.DrawIndexedPrimitive(PrimitiveType.TriangleList, 0, 0, parentGraphic.VertexNormals.Count, 0, NumIndicies / 3);
    }

    public void CleanupDirectX()
    {
        if (FacesDirectX is null)
        {
            return;
        }
        
        FacesDirectX.Dispose();
        FacesDirectX = null;
    }

    public override string ToString()
    {
        return GroupType + ". Faces:" + Faces.Count;
    }
}

public enum ToothGroupType
{
    /// <summary>
    /// This is areas of the tooth that are not included in any fillings at all.
    /// </summary>
    Enamel,

    Cementum,
    M,
    O,
    D,
    B,
    L,
    F,
    I,

    ///<summary>9. class V. In addition to B or F</summary>
    V,

    ///<summary>10. The pulp chamber and post or buildup.  Because our teeth are not yet transparent, these faces need to be translated forward.</summary>
    Buildup,

    ///<summary>11. Only defined on some anterior teeth.  The small bit of enamel on the F that is not included in any filling.</summary>
    EnamelF,

    /// <summary>12. Only defined on some anterior teeth.  The F portion of the D filling.</summary>
    DF,

    /// <summary>13. Only defined on some anterior teeth.  The F portion of the M filling.</summary>
    MF,

    /// <summary>14. Only defined on some anterior teeth.  The F portion of the I filling.</summary>
    IF,

    ///<summary>Only present in the special implant tooth object.</summary>
    Implant,

    ///<summary>Not used. Just a placeholder</summary>
    Canals,

    ///<summary>Used where there are unknown materials present such as 'default'.  Always ignore these groups.</summary>
    None
}