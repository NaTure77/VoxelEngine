using System.Collections.Generic;
using UnityEngine;

public static class OctreeGenerator
{

    public static Octree Generate(List<Vector3> positions, int scale)
    {
        Octree octree = new Octree(Vector3Int.one * scale / 2, scale);
        for (int i = 0; i < positions.Count; i++)
        {
            octree.Push(positions[i], Color.cyan);
        }
        octree.SetColorRecursive();
        return octree;
    }
    /// <summary>
    /// Convert VoxelArray(Custom Data Type) To Octree
    /// </summary>
    /// <param name="voxelArr"></param>
    /// <returns></returns>
    public static Octree Generate(VoxelArray voxelArr)
    {
        Octree octree = new Octree(Vector3Int.one * voxelArr.scale / 2, voxelArr.scale);
        Vector3[] colors = voxelArr.ConvertToVectorArray();
        for (int i = 0; i < voxelArr.data.Count; i++)
        {
            Vector3 pos = IdxToPos(voxelArr.scale, voxelArr.data[i].index);
            octree.Push(pos, colors[i]);
        }
        octree.SetColorRecursive();
        return octree;
    }

    /// <summary>
    /// Convert Mesh To Octree
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="texture"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    public static Octree Generate(Mesh mesh, Texture2D texture, int scale)
    {
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = mesh.uv;
        Vector3 minPoint = Vector3.one * float.MaxValue;
        Vector3 maxPoint = Vector3.one * float.MinValue;
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].x < minPoint.x) minPoint.x = vertices[i].x;
            if (vertices[i].y < minPoint.y) minPoint.y = vertices[i].y;
            if (vertices[i].z < minPoint.z) minPoint.z = vertices[i].z;

            if (vertices[i].x > maxPoint.x) maxPoint.x = vertices[i].x;
            if (vertices[i].y > maxPoint.y) maxPoint.y = vertices[i].y;
            if (vertices[i].z > maxPoint.z) maxPoint.z = vertices[i].z;
        }
        Vector3 size = maxPoint - minPoint;
        float longestAxisSize = Mathf.Max(size.x, size.y, size.z);

        float density = scale / longestAxisSize * 0.6f;
        Vector3 coord = ((Vector3.one * scale) / density - (maxPoint + minPoint)) / 2;
        
        Octree octree = new Octree(Vector3Int.one * scale / 2, scale);
        if (texture == null)
        {
            texture = new Texture2D(1, 1);
            texture.SetPixel(1, 1, Color.white);
        }
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] += coord;
            //Vector3Int v = Vector3ToPoint(vertices[i], density);
            Vector3 v = vertices[i] * density;
            //if (v.x >= 0 && v.x < scale && v.y >= 0 && v.y < scale && v.z >= 0 && v.z < scale)
            {
                int x = (int)(uvs[i].x * texture.width);
                int y = (int)(uvs[i].y * texture.height);
                octree.Push(v, texture.GetPixel(x, y));
            }
        }
        octree.SetColorRecursive();
        return octree;
    }

    
    static Vector3 IdxToPos(int mapScale, int idx)
    {
        Vector3 result = new Vector3();
        result.z = idx / (mapScale * mapScale) + 0.5f;
        result.y = (idx % (mapScale * mapScale)) / mapScale + 0.5f;
        result.x = idx % mapScale + 0.5f;
        return result;
    }
}
public class Octree
{
    public Node root;

    public Octree(Vector3Int pos, int scale)
    {
        root = new Node(pos, scale);
    }

    public void Push(Vector3 pos, Vector3 colorData)
    {
        root.SubDivide(pos, colorData);
    }
    public void Push(Vector3 pos, Color c)
    {
        Vector3 v = new Vector3(c.r, c.g, c.b);
        root.SubDivide(pos, v);
    }
    public void SetColorRecursive()
    {
        root.SetColor();
    }
    public VoxelArray ConvertToVoxelArray()
    {
        VoxelArray va = new VoxelArray(root.scale);
        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            Node node = queue.Dequeue();
            if (node.scale == 1)
            {
                //가운데가 아니라 왼쪽 아래 포지셔닝을 해야 정수로 나누어 떨어지므로 인덱싱이 제대로 됨.
                //ex) 1.5,1.5,1.5 -> 1,1,1
                node.position -= Vector3.one * 0.5f; 
                va.Add(node.position,node.colorData);
            }
            else
            {
                for (int i = 0; i < node.subNodes.Length; i++)
                {
                    if (node.subNodes[i] != null)
                    {
                        queue.Enqueue(node.subNodes[i]);
                    }
                }
            }
        }
        return va;
    }
    public List<DataNode> ConvertToDataNode()
    {
        List<DataNode> list = new List<DataNode>();
        Queue<Node> queue = new Queue<Node>();
        uint idx = 0; // 최대 2^30까지 표현해야 함.
        queue.Enqueue(root);

        //데이터 생성
        while (queue.Count > 0)
        {
            Node node = queue.Dequeue();
            DataNode dataNode = new DataNode(node.colorData);
            if (node.subNodes[0] != null)
            {
                queue.Enqueue(node.subNodes[0]);
                dataNode.sn0 = ++idx;
            }
            if (node.subNodes[1] != null)
            {
                queue.Enqueue(node.subNodes[1]);
                dataNode.sn1 = ++idx;
            }
            if (node.subNodes[2] != null)
            {
                queue.Enqueue(node.subNodes[2]);
                dataNode.sn2 = ++idx;
            }
            if (node.subNodes[3] != null)
            {
                queue.Enqueue(node.subNodes[3]);
                dataNode.sn3 = ++idx;
            }
            if (node.subNodes[4] != null)
            {
                queue.Enqueue(node.subNodes[4]);
                dataNode.sn4 = ++idx;
            }
            if (node.subNodes[5] != null)
            {
                queue.Enqueue(node.subNodes[5]);
                dataNode.sn5 = ++idx;
            }
            if (node.subNodes[6] != null)
            {
                queue.Enqueue(node.subNodes[6]);
                dataNode.sn6 = ++idx;
            }
            if (node.subNodes[7] != null)
            {
                queue.Enqueue(node.subNodes[7]);
                dataNode.sn7 = ++idx;
            }
            list.Add(dataNode);
        }
        return list;
    }

    public List<uint> GetRoute(Vector3 pos)
    {
        List<uint> list = new List<uint>();

        Node currentNode = root;
        while (currentNode.scale != 1)
        {
            Vector3 position = currentNode.position;
            uint idx = 0;
            if (position.x <= pos.x) idx |= 4;
            if (position.y <= pos.y) idx |= 2;
            if (position.z <= pos.z) idx |= 1;

            
            list.Add(idx);
            currentNode = currentNode.subNodes[idx];
            if (currentNode == null) return null; 
        }

        //중복 접근 제한
        if (currentNode.colorCount == 0) return null;
        else currentNode.colorCount = 0;
        return list;
    }
}

[System.Serializable]
public struct DataNode
{
#pragma warning disable 649
    public float colorX;
    public float colorY;
    public float colorZ;
    public uint sn0;
    public uint sn1;
    public uint sn2;
    public uint sn3;
    public uint sn4;
    public uint sn5;
    public uint sn6;
    public uint sn7;

    public DataNode(Vector3 color)
    {
        //this.color = color;
        colorX = color.x;
        colorY = color.y;
        colorZ = color.z;
        sn0 =
        sn1 =
        sn2 =
        sn3 =
        sn4 =
        sn5 =
        sn6 =
        sn7 = 0;
    }

    public uint[] GetChildIdxArray()
    {
        uint[] arr =
        {
            sn0,sn1,sn2,sn3,sn4,sn5,sn6,sn7
        };
        return arr;
    }
}
public class Node
{
    public int scale;
    public Vector3 position;
    public Node[] subNodes;
    //000: 왼쪽하단아래
    //001: 왼쪽하단위
    //010: 왼쪽상단아래
    //011: 왼쪽상단위

    //100: 오른쪽하단아래
    //101: 오른쪽하단위
    //110: 오른쪽상단아래
    //111: 오른쪽상단위
    public Vector3 colorData = new Vector3();
    public int colorCount = 0;
    public Node(Vector3 position, int scale)
    {
        this.scale = scale;
        this.position = position;
        subNodes = new Node[8];
    }
    public void SubDivide(Vector3 pos, Vector3 colorData)
    {
        if(scale == 1)
        {
            if(colorCount == 0)
            {
                this.colorData = colorData;
            }
            else
            {
                this.colorData = (this.colorData * colorCount + colorData) / (colorCount + 1);
            }
            colorCount++;
            return;
        }
        int idx = 0;
        if (position.x <= pos.x) idx |= 4;
        if (position.y <= pos.y) idx |= 2;
        if (position.z <= pos.z) idx |= 1;

        if (subNodes[idx] == null)
        {
            int childScale = scale / 2;
            Vector3 childPosRelative = Vector3.one * (childScale) * 0.5f;
            if (position.x > pos.x) childPosRelative.x *= -1;
            if (position.y > pos.y) childPosRelative.y *= -1;
            if (position.z > pos.z) childPosRelative.z *= -1;
            subNodes[idx] = new Node(position + childPosRelative, childScale);
        }
        subNodes[idx].SubDivide(pos,colorData);
    }

    public Vector3 SetColor()
    {
        if (scale == 1) return colorData;

        int cnt = 0;
        for(int i = 0; i < subNodes.Length; i++)
        {
            if (subNodes[i] != null)
            {
                colorData += subNodes[i].SetColor();
                cnt++;
            }
        }
        colorData /= cnt;
        return colorData;
    }
}

