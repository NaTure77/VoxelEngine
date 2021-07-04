using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Linq;
using System.Security.Cryptography;
using System.Collections.Generic;
public class Controller3 : MonoBehaviour
{
    public ComputeShader computeShader = null;
    public RawImage rawImage;
    public Vector2Int resolution = new Vector2Int();
    public Vector3 pos;
    public Vector3 pos_temp;

    public Vector3 moveDirection_touch;
    Vector3 moveDirection;
    public Vector3 rot;
    public Vector3 rot_temp;

    public float moveSpeed = 2f;
    public Vector2 rot_sensitivity = new Vector2(4, 4);
    public GameObject touchPad;
    public GameObject controllerPad;

    public Slider slider;

    int KernelID; //커널 ID를 이런식으로 담아낼 수 있음. 커널 ID는 ComputeShader에 있는 함수 이름이라고 할 수 있음.
    ComputeBuffer mapBuffer;
    ComputeBuffer octreeBuffer;
    ComputeBuffer scalesBuffer;
    ComputeBuffer childPosBuffer;
    public RenderTexture renderTexture;

    bool lightMoving = false;
    bool circleShape = false;
   // bool addBlockFlag = false;
   // bool delBlockFlag = false;
    private struct CSPARAM
    {
        public const string KERNELID = "Kernel1";
        public const string RESULT = "Result";
        public const string POSITION = "_Position";
        public const string ROTATION = "_Rotation";
        public const string OCTREE = "octree";
        // public const string CENTER = "centerPos";

        // public const string CENTERBEF = "centerPos_bef";
        public const string MAPSIZE = "mapSize";
        // public const string FLAG_DELBLK = "deleteBlock";
        // public const string FLAG_ADDBLK = "addBlock";
        public const string FLAG_GRID = "gridEnabled";
        public const string FLAG_LIGHT = "lightEnabled";
        public const string FLAG_CIRCLE = "circleShape";
        public const string RESOLUTION_X = "ResolutionX";
        public const string RESOLUTION_Y = "ResolutionY";
        public const string VOXELLVL = "voxelLevel";
        public const string SCALES = "scales";
        public const string CHILDDIR = "childDirection";
        //  public const string SCREEN_CENTER_X = "screenCenterX";
        //  public const string SCREEN_CENTER_Y = "screenCenterY";
        //  public const string MAX_STEPS = "MAX_STEPS";

        public const string LIGHTPOS = "lightPos";
        //  public const string LIGHTIDX = "lightIdx";

        public const int THREAD_NUMBER_X = 8;
        public const int THREAD_NUMBER_Y = 8;
    }

    readonly float PI = Mathf.PI / 180f;

    public static Controller3 instance;

    public InputManager inputManager;
    private void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;
        inputManager = new InputManager();
        inputManager.Enable();
        //inputManager = new InputManager();
        //inputManager.Enable();

        pos_temp = pos;
        rot_temp = rot;

        slider.onValueChanged.AddListener((val) =>
        {
            SetVoxelLevel((int)val);
        });

        inputManager.Player.Quality.performed += val =>
        {
            if (val.ReadValue<float>() > 0)
                SetQuality(1);
            else SetQuality(-1);
        };

        inputManager.Player.VoxelLevel.performed += val =>
        {
            if (val.ReadValue<float>() > 0)
                ++VoxelLevel;
            //AdjVoxelLevel(-1);
            else --VoxelLevel;

            VoxelLevel = (int)Mathf.Clamp(VoxelLevel, 0, Mathf.Log(mapSize, 2) - 1);
            slider.value = VoxelLevel;
            //AdjVoxelLevel(1);
        };
        inputManager.Player.LightMove.performed += val =>
        {
            lightMoving = !lightMoving;
        };

        inputManager.Player.CircleShape.performed += val =>
        {
            circleShape = !circleShape;
            computeShader.SetBool(CSPARAM.FLAG_CIRCLE, circleShape);
        };
        Init();
    }
    void Init()
    {
        KernelID = computeShader.FindKernel(CSPARAM.KERNELID);
        computeShader.SetInt(CSPARAM.VOXELLVL, VoxelLevel);
        ShowGrid(false);
        computeShader.SetBool(CSPARAM.FLAG_LIGHT, false);
        computeShader.SetBool(CSPARAM.FLAG_CIRCLE, false);
    }

    public void SetTexture()
    {
        resolution.x = (int)(Screen.currentResolution.width * qualityLevels[currentQLevel]);
        resolution.y = (int)(Screen.currentResolution.height * qualityLevels[currentQLevel]);

        renderTexture = new RenderTexture(resolution.x, resolution.y, 32);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
        rawImage.texture = renderTexture;
        computeShader.SetInt(CSPARAM.RESOLUTION_X, resolution.x);
        computeShader.SetInt(CSPARAM.RESOLUTION_Y, resolution.y);

        computeShader.SetTexture(KernelID, CSPARAM.RESULT, renderTexture);
    }

    IEnumerator mainLoop;

    int mapSize;
    public void StartRendering(int mapSize, List<DataNode> octree)
    {
        if (mainLoop != null)
        {
            StopCoroutine(mainLoop);
            renderTexture.Release();
            ReleaseBuffers();
        }

        // mapBuffer =  new ComputeBuffer(pointData.Length, sizeof(float) * 3);
        octreeBuffer = new ComputeBuffer(octree.Count, sizeof(int) * 8 + sizeof(float) * 3);
        scalesBuffer = new ComputeBuffer(10, sizeof(int));
        childPosBuffer = new ComputeBuffer(8, sizeof(float) * 3);

        int[] scaleArr = { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024 };
        Vector3[] childPosArr =
        {
                new Vector3(-1,-1,-1),
                new Vector3(-1,-1,1),
                new Vector3(-1,1,-1),
                new Vector3(-1,1,1),
                new Vector3(1,-1,-1),
                new Vector3(1,-1,1),
                new Vector3(1,1,-1),
                new Vector3(1,1,1)
            };
        //mapBuffer.SetData(pointData);
        octreeBuffer.SetData(octree.ToArray());
        scalesBuffer.SetData(scaleArr);
        childPosBuffer.SetData(childPosArr);

        //computeShader.SetBuffer(KernelID, CSPARAM.MAP, mapBuffer);
        computeShader.SetBuffer(KernelID, CSPARAM.OCTREE, octreeBuffer);
        computeShader.SetBuffer(KernelID, CSPARAM.SCALES, scalesBuffer);
        computeShader.SetBuffer(KernelID, CSPARAM.CHILDDIR, childPosBuffer);

        this.mapSize = mapSize;
        computeShader.SetInt(CSPARAM.MAPSIZE, mapSize);
        SetTexture();

        slider.maxValue = Mathf.Log(mapSize, 2) - 1;
        SetVoxelLevel(VoxelLevel);


        if(ImageViewer.instance.shaderEnabled)
        {
            ImageViewer.instance.SetOctreeBuffer(octreeBuffer);
        }
        else
        {
            ImageViewer.instance.ReleaseBuffer();
        }

        mainLoop = Loop();
        StartCoroutine(mainLoop);
    }

    public void StopLoop()
    {
        if (mainLoop != null)
        {
            ImageViewer.instance.shaderEnabled = false;
            StopCoroutine(mainLoop);
        }
    }
    public void StartLoop()
    {
        if (mainLoop != null)
        {
            mainLoop = Loop();
            StartCoroutine(mainLoop);
        }
    }
    void ReleaseBuffers()
    {
        mapBuffer?.Release();
        octreeBuffer?.Release();
        scalesBuffer?.Release();
        childPosBuffer?.Release();
    }
    void OnDestroy()
    {
        ReleaseBuffers();
        inputManager.Dispose();
        Destroy(this);
    }

    IEnumerator Loop()
    {
        while (true)
        {

            Vector2 delta = inputManager.Player.Look.ReadValue<Vector2>();

            rot.x = Mathf.Clamp(rot.x - delta.y * rot_sensitivity.y, -90, 90);
            rot.y = rot.y - delta.x * rot_sensitivity.x;
            Rotate();

            Vector2 v = inputManager.Player.Move.ReadValue<Vector2>();
            moveDirection.x = v.x;
            moveDirection.z = v.y;
            moveDirection.y = inputManager.Player.UPDOWN.ReadValue<float>();
            moveDirection += moveDirection_touch;
            Move(moveDirection);

            if(ImageViewer.instance.shaderEnabled)
            {
                ImageViewer.instance.Dispatch();
            }
            DispatchComputeShader();

            yield return null;
        }

    }

    float[] qualityLevels = { 0.0625f, 0.125f, 0.25f, 0.5f, 1 };
    public int currentQLevel = 2;

    public Text qText;
    public void SetQuality(int i)
    {
        currentQLevel += i;
        currentQLevel = (int)Mathf.Clamp(currentQLevel, 0, 4);
        SetTexture();
        qText.text = currentQLevel.ToString();
    }

    Vector3 lightPos = new Vector3();
    IEnumerator coroutineVar;
    public void ShowShadow(bool b)
    {
        if (!b)
        {
            StopCoroutine(coroutineVar);
        }
        else
        {
            coroutineVar = followLightPosCoroutine();
            StartCoroutine(coroutineVar);
        }
        computeShader.SetBool(CSPARAM.FLAG_LIGHT, b);
    }


    IEnumerator followLightPosCoroutine()
    {
        while (true)
        {
            if (lightMoving)
            {
                lightPos = Vector3.Lerp(lightPos, pos, 0.01f);
                computeShader.SetVector(CSPARAM.LIGHTPOS, lightPos);
            }

            yield return null;
        }
    }

    public int VoxelLevel = 0;
    // public Text sText;

    public void SetVoxelLevel(int v)
    {
        VoxelLevel = (int)Mathf.Clamp(v, 0, Mathf.Log(mapSize, 2) - 1);
        computeShader.SetInt(CSPARAM.VOXELLVL, VoxelLevel);
        // sText.text = VoxelLevel.ToString();
    }
    public void ShowGrid(bool b)
    {
        computeShader.SetBool(CSPARAM.FLAG_GRID, b);
    }
    public void SetGamepadMode(bool b)
    {
        if (b) rot_sensitivity = Vector2.one * 2;
        else rot_sensitivity = Vector2.one * 0.2f;

        touchPad.gameObject.SetActive(!b);
        //controllerPad.gameObject.SetActive(b);
    }

    private void DispatchComputeShader()
    {
        computeShader.Dispatch(KernelID, Mathf.CeilToInt(1.0f * resolution.x / CSPARAM.THREAD_NUMBER_X), Mathf.CeilToInt(1.0f * resolution.y / CSPARAM.THREAD_NUMBER_Y), 1);
        //타게팅 된 블록의 인덱스 가져오기.
    }
    void Rotate()
    {
        rot_temp = Vector3.Lerp(rot_temp, rot, 0.3f);
        computeShader.SetVector(CSPARAM.ROTATION, rot_temp * PI);
    }

    void Move(Vector3 d)
    {
        d *= moveSpeed * Time.deltaTime;
        float sin = Mathf.Sin(rot_temp.y * PI);
        float cos = Mathf.Cos(rot_temp.y * PI);
        pos += new Vector3(d.x * cos - d.z * sin, d.y, d.x * sin + d.z * cos);

        pos.x = Mathf.Clamp(pos.x, 0, mapSize);
        pos.y = Mathf.Clamp(pos.y, 0, mapSize);
        pos.z = Mathf.Clamp(pos.z, 0, mapSize);

        pos_temp = Vector3.Lerp(pos_temp, pos, 0.1f);
        computeShader.SetVector(CSPARAM.POSITION, pos_temp);
    }
}