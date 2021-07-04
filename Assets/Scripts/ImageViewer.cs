using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using YoutubePlayer;

public class ImageViewer : MonoBehaviour
{
    //public Texture2D image;

    uint[] pixelMap;
    public static ImageViewer instance;

    public ComputeShader computeShader;

    ComputeBuffer imageMapBuffer;
    public RenderTexture renderTexture;
    public VideoPlayer videoPlayer;
    public SimpleYoutubeVideo youtubeVideo;
    int kernelID;

    int width = 0;
    int height = 0;
    public bool useYoutubeVideo = false;
    public bool shaderEnabled = false;
    public void Awake()
    {
        instance = this;
        InitComputeShader();
    }

    void InitComputeShader()
    {
        kernelID = computeShader.FindKernel(CSPARAM.KERNELID);
    }
    private struct CSPARAM
    {
        public const string KERNELID = "CSMain";
        public const string OCTREE = "octree";
        public const string IMAGEMAP = "imageMap";
        public const string WIDTH = "width";
        public const string HEIGHT = "height";
        public const string LEVEL = "level";
        public const string IMAGE = "image";
        public const string RATIO = "ratio";
        public const string VOXELGAP = "voxelGap";
        public const int THREAD_NUMBER_X = 8;
        public const int THREAD_NUMBER_Y = 8;
    }

    int level = 0;
    int gap = 1;
    float sizeRatio = 1;
    public void Dispatch()
    {
        
        //voxelLevel이 바뀔때마다
        level = 10 - Controller3.instance.VoxelLevel;
        gap = (int)Mathf.Pow(2, Controller3.instance.VoxelLevel);
        computeShader.SetInt(CSPARAM.LEVEL, level);
        computeShader.SetInt(CSPARAM.VOXELGAP, gap);
        //computeShader.SetVector(CSPARAM.RATIO, new Vector2(gap,gap));
        // scaledWidth = width / gap;
        // scaledHeight = height / gap;
        computeShader.Dispatch(kernelID, Mathf.CeilToInt(1.0f * width / (CSPARAM.THREAD_NUMBER_X * gap)), Mathf.CeilToInt(1.0f * height / (CSPARAM.THREAD_NUMBER_Y * gap)), 1);

        //computeShader.Dispatch(kernelID, Mathf.CeilToInt(1.0f * width / CSPARAM.THREAD_NUMBER_X), Mathf.CeilToInt(1.0f * height / CSPARAM.THREAD_NUMBER_Y), 1);
    }

    public void SetOctreeBuffer(ComputeBuffer octreeBuffer)
    {
        ReleaseBuffer();
        //renderTexture = new RenderTexture(width,height, 32);
        renderTexture.enableRandomWrite = true;
        //renderTexture.Create();
        
        //Graphics.Blit(image, renderTexture);
        computeShader.SetTexture(kernelID, CSPARAM.IMAGE, renderTexture);
        imageMapBuffer = new ComputeBuffer(pixelMap.Length, sizeof(uint));
        imageMapBuffer.SetData(pixelMap);
        computeShader.SetBuffer(kernelID, CSPARAM.IMAGEMAP, imageMapBuffer);
        computeShader.SetBuffer(kernelID, CSPARAM.OCTREE, octreeBuffer);

        if (youtubeVideo == null) videoPlayer.Play();
    }
    public List<DataNode> MakeImageVoxel(int mapScale)
    {
        shaderEnabled = true;
        width = renderTexture.width;// Mathf.Clamp(image.width, 0, mapScale);
        height = renderTexture.height;//Mathf.Clamp(image.height, 0, mapScale);
        if (width > mapScale || height > mapScale)
        {
            if (width > height)
            {
                height = (mapScale * height) / width;
                width = mapScale;
            }
            else
            {
                height = mapScale;
                width = (mapScale * width) / height;
            }
            sizeRatio = (float)renderTexture.width / width;
        }
        else sizeRatio = 1;
        computeShader.SetFloat(CSPARAM.RATIO, sizeRatio);
        computeShader.SetInt(CSPARAM.WIDTH, width);
        computeShader.SetInt(CSPARAM.HEIGHT, height);
        computeShader.SetInt(CSPARAM.LEVEL, 10 - Controller3.instance.VoxelLevel);
        List<Vector3> shape = MakeCurveDisplayShape(mapScale);
        Octree octree = OctreeGenerator.Generate(shape, mapScale);
        List<DataNode> dataNodes = octree.ConvertToDataNode();

        pixelMap = new uint[width * height * 10];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3 pos = shape[i * height + j];//voxelArr.IdxToPos(voxelArr.data[i * height + j].index);//new Vector3(i + startPoint.x, j + startPoint.y, startPoint.z);
                List<uint> route = octree.GetRoute(pos);
                if (route == null) continue;
                uint dataIdx = 0;
                for (int k = 0; k < route.Count; k++)
                {
                    dataIdx = dataNodes[(int)dataIdx].GetChildIdxArray()[route[k]];
                    pixelMap[j * width * 10 + i * 10 + k] = dataIdx;
                }
                //pixelMap[j * width + i] = dataIdx;
            }
        }

        return dataNodes;
    }

    List<Vector3> MakeCurveDisplayShape(int mapScale)
    {
        List<Vector3> list = new List<Vector3>();
        Vector3 startPoint = (Vector3Int.one * mapScale - new Vector3(width, height, 0)) / 2;
        Vector3 pivot = Vector3.one * mapScale * 0.5f;
        pivot.z += 200;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3 pos = new Vector3(i + startPoint.x, j + startPoint.y, startPoint.z);
                //pos.z = ((pos - pivot).normalized * 200 + pivot).z;
                list.Add(pos);
            }
        }

        float backGroundDepth = startPoint.z + 256;
        for (int i = 0; i < mapScale; i++)
        {
            for (int j = 0; j < mapScale; j++)
            {
                Vector3 pos = new Vector3(i, j, backGroundDepth);
                list.Add(pos);
            }
        }
        //list = list.Distinct().ToList();
        return list;
    }
  
    //1. octree를 탐색하며 리프까지 도달할때의 경로를 배열로 저장.
    //2. 저장한 배열로 DataNode[]를 탐색하여 리프노드의 인덱스를 찾아냄. 
    public void ReleaseBuffer()
    {
        if(videoPlayer != null)
            videoPlayer.Stop();
        if (renderTexture != null)
            renderTexture.Release();
        imageMapBuffer?.Release();
    }

    public void OnDestroy()
    {
        ReleaseBuffer();
    }
}