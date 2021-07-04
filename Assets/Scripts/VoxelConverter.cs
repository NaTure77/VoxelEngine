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
            List<DataNode> octreeData = OctreeGenerator.Generate(voxelArr).ConvertToDataNode();
            Controller3.instance.StartRendering(voxelArr.scale, octreeData);
            converterPanel.SetActive(false);
            DestroyImmediate(mesh);
            DestroyImmediate(texture);
            Resources.UnloadUnusedAssets();
            mesh = null;
            texture = null;
            octree = null;
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
