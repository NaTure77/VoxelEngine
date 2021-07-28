using Dummiesman;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using ZeroFormatter;

public class VoxelConverter : MonoBehaviour
{
    public GameObject converterPanel;
    public int scale = 1024;

    Mesh mesh = null;
    Texture2D texture = null;

    public Button SelectVoxelDataButton;
    public Button SelectMeshButton;
    public Button SelectTextureButton;
    public Button RenderVoxelButton;

    public Button SaveButton;
    public Button ResetButton;

    string fileName;
    Octree octree;
    private void Awake()
    {
        ZeroFormatterInitializer.Register();
        SelectVoxelDataButton.onClick.AddListener(ShowVoxelDataList);
        SelectMeshButton.onClick.AddListener(ShowObjList);
        SelectTextureButton.onClick.AddListener(ShowTextureList);
        RenderVoxelButton.onClick.AddListener(RenderVoxel);
        SaveButton.onClick.AddListener(Save);
        ResetButton.onClick.AddListener(ResetSelection);
    }

    private void Start()
    {
        ShowVoxelDataList();
    }
    public void ShowVoxelDataList()
    {
        List<ScrollData> list = ScrollUI.MakeDataList(new[] { "*.va" }, (filePath) =>
        {
            VoxelArray voxelArr = FileManager<VoxelArray>.LoadFile_ZF(filePath);
            octree = OctreeGenerator.Generate(voxelArr);
            List<DataNode> octreeData = octree.ConvertToDataNode();
            Controller3.instance.StartRendering(voxelArr.scale, octreeData);
            converterPanel.SetActive(false);
            DestroyImmediate(mesh);
            DestroyImmediate(texture);
            Resources.UnloadUnusedAssets();
            mesh = null;
            texture = null;
            //octree = null;
            GC.Collect();
        });
        ScrollUI.instance.EnableUI(list);
    }
    public void ShowObjList()
    {
        List<ScrollData> list = ScrollUI.MakeDataList(new[] { "*.obj" }, (filePath) =>
        {
            DestroyImmediate(mesh);
            Resources.UnloadUnusedAssets();
            mesh = null;
            octree = null;
            GC.Collect();
            
            ImportMesh(filePath);
        });
        ScrollUI.instance.EnableUI(list);
    }
    public void ShowTextureList()
    {
        List<ScrollData> list = ScrollUI.MakeDataList(new[] {"*.jpg", "*.png"}, (filePath) =>
        {
            DestroyImmediate(texture);
            Resources.UnloadUnusedAssets();
            texture = null;
            octree = null;
            GC.Collect();
            
            ImportTexture(filePath);
        });
        ScrollUI.instance.EnableUI(list);
    }
    public void Save()
    {
        if (octree == null) return;
        FileManager<VoxelArray>.SaveFile_ZF(fileName, octree.ConvertToVoxelArray());
    }

    public Texture2D tex;

    Vector2Int Dim3ToDim2(Vector3 pos)
    {
        int cellNum_col = (int)pos.y / 16;
        int cellNum_row = (int)pos.y % 16;

        int x = (int)pos.x + cellNum_row * 256;
        int y = (int)pos.z + cellNum_col * 256;

        return new Vector2Int(x, y);
    }

    public void SaveToImage()
    {
        Debug.Log("!!!");
        if (octree == null) return;
        VoxelArray va = octree.ConvertToVoxelArray();

        tex = new Texture2D(4096, 4096);

        Color nothing = new Color(0, 0, 0, 0);
        Color[] fillColorArr = tex.GetPixels();
        for (int i = 0; i < fillColorArr.Length; i++) fillColorArr[i] = nothing;
        tex.SetPixels(fillColorArr);
        for (int i = 0; i < va.data.Count; i++)
        {
            VoxelData data = va.data[i];
            Color color = new Color(data.r / 256f, data.g / 256f, data.b / 256f);
            //Vector2Int pos2D = new Vector2Int(data.index / 4096, data.index % 4096);
            Vector2Int pos2D = Dim3ToDim2(va.IdxToPos(data.index));
            tex.SetPixel(pos2D.x, pos2D.y, color);
        }
        tex.Apply();

        byte[] byteArr = tex.EncodeToPNG();
        File.WriteAllBytes(Application.persistentDataPath + "/mapImage1.png", byteArr);
        Debug.Log("!!");
    }

    public void MakeVideoImage()
    {
        VoxelArray va = octree.ConvertToVoxelArray();
        Debug.Log("asdf");
        Color nothing = new Color(0, 0, 0, 0);
        Color[] fillColorArr = new Color[4096 * 4096];
        for (int i = 0; i < fillColorArr.Length; i++) fillColorArr[i] = nothing;

        tex = new Texture2D(4096, 4096);

        int center = 127;
        DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath + "/" + fileName);
        if (!di.Exists) di.Create();
        for (int i = 0; i < 360; i++)
        {
            tex.SetPixels(fillColorArr);
            float sin = Mathf.Sin(i * Mathf.PI / 180f);
            float cos = Mathf.Cos(i * Mathf.PI / 180f);

            for (int j = 0; j < va.data.Count; j++)
            {
                VoxelData data = va.data[j];
                //float brightness = (data.r + data.g + data.b) / (3 * 256f);
                //Color color = new Color(brightness, brightness, brightness);
                Color color = new Color(data.r / 256f, data.g / 256f, data.b / 256f);
                Vector3 pos = va.IdxToPos(data.index);
                Vector3 pos_new = new Vector3();
                pos_new.y = pos.y;
                pos_new.x = Mathf.Round((pos.x - center) * cos - (pos.z - center) * sin + center);
                pos_new.z = Mathf.Round((pos.x - center) * (sin) + (pos.z - center) * cos + center);

                if (pos_new.x < 0 || pos_new.x >= 256 || pos_new.z < 0 || pos_new.z >= 256) continue;
                //int idx = va.PosToIdx(pos_new);
                //tex.SetPixel(idx / 4096, idx % 4096, color);
                Vector2Int pos2D = Dim3ToDim2(pos_new);
                tex.SetPixel(pos2D.x, pos2D.y, color);
            }
            tex.Apply();
            byte[] byteArr = tex.EncodeToPNG();
            
            File.WriteAllBytes(Application.persistentDataPath + "/" + fileName + "/mapImage" + i.ToString() + ".png", byteArr);
        }
    }
    public void ResetSelection()
    {
        mesh = null;
        texture = null;
    }
    public void RenderVoxel()
    {
        if (mesh == null) return;
        octree = OctreeGenerator.Generate(mesh, texture, scale);
        Controller3.instance.StartRendering(scale, octree.ConvertToDataNode());
        converterPanel.SetActive(false);
    }
    public void ImportMesh(string path)
    {
        fileName = Path.GetFileNameWithoutExtension(path);
        mesh = new OBJLoader().LoadMesh(path);
    }
    public void ImportTexture(string path)
    {
        byte[] byteTexture = File.ReadAllBytes(path);
        if(byteTexture.Length > 0)
        {
            texture = new Texture2D(0, 0);
            texture.LoadImage(byteTexture);
        }
    }



}
