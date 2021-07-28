using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeroFormatter;

[System.Serializable]
public class PointArr
{
    public int sizeX;
    public int sizeY;
    public int sizeZ;
    public List<PointData> data;

    public Vector3[] ConvertToVectorArray()
    {
        List<Vector3> list = new List<Vector3>();

        foreach(PointData p in data)
        {
            list.Add(new Vector3(p.r, p.g, p.b) / 256);
        }
        return list.ToArray();
    }
}

[System.Serializable]
public class PointData
{

    public int index;
    public byte r;
    public byte g;
    public byte b;

    public PointData(int idx, Color color)
    {
        index = idx;
        r = (byte)(color.r * 255);
        g = (byte)(color.g * 255);
        b = (byte)(color.b * 255);
    }
}

[ZeroFormattable]
public class VoxelArray
{
    [Index(0)]
    public virtual int scale { get; set; }
    [Index(1)]
    public virtual List<VoxelData> data { get; set; }

    public VoxelArray() { }
    public VoxelArray(int scale) { data = new List<VoxelData>(); this.scale = scale; }
    public Vector3[] ConvertToVectorArray()
    {
        List<Vector3> list = new List<Vector3>();

        foreach (VoxelData p in data)
        {
            list.Add(new Vector3(p.r, p.g, p.b) / 256);
        }
        return list.ToArray();
    }
    public void Add(Vector3 position, Vector3 color)
    {
        int idx = PosToIdx(position);
        data.Add(new VoxelData(idx, color));
    }
    public void Add(Vector3 position, Color color)
    {
        int idx = PosToIdx(position);
        data.Add(new VoxelData(idx, color));
    }
    public int PosToIdx(Vector3 p)
    {
        return (int)((int)p.z * scale * scale + (int)p.y * scale + (int)p.x);
    }
    public Vector3 IdxToPos(int idx)
    {
        Vector3 result = new Vector3();
        result.z = idx / (scale * scale);// + 0.5f;
        result.y = (idx % (scale * scale)) / scale;// + 0.5f;
        result.x = idx % scale;// + 0.5f;
        return result;
    }
}
[ZeroFormattable]
public class VoxelData
{
    [Index(0)]
    public virtual int index { get; set; }
    [Index(1)]
    public virtual byte r { get; set; }
    [Index(2)]
    public virtual byte g { get; set; }
    [Index(3)]
    public virtual byte b { get; set; }

    public VoxelData()
    {

    }
    public VoxelData(int idx, Color color)
    {
        index = idx;
        r = (byte)(color.r * 255);
        g = (byte)(color.g * 255);
        b = (byte)(color.b * 255);
    }

    public VoxelData(int idx, Vector3 color)
    {
        index = idx;
        r = (byte)(color.x * 255);
        g = (byte)(color.y * 255);
        b = (byte)(color.z * 255);
    }
}