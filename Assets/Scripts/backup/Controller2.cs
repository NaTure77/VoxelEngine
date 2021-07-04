using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Linq;

namespace CubeWorld
{
    public class Controller2 : MonoBehaviour
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
        public Vector2 rot_sensitivity = new Vector2(4,4);
        public GameObject touchPad;
        public GameObject controllerPad;
        int KernelID; //커널 ID를 이런식으로 담아낼 수 있음. 커널 ID는 ComputeShader에 있는 함수 이름이라고 할 수 있음.
        ComputeBuffer mapBuffer;
        ComputeBuffer centerPos;
        RenderTexture renderTexture;


        bool addBlockFlag = false;
        bool delBlockFlag = false;
        private struct CSPARAM
        {
            public const string KERNELID = "Kernel1";
            public const string IDXDATA = "idxData";
            public const string RESULT = "Result";
            public const string POSITION = "_Position";
            public const string ROTATION = "_Rotation";
            public const string MAP = "_Map";
            public const string CENTER = "centerPos";

            public const string CENTERBEF = "centerPos_bef";
            public const string MAPSIZE = "mapSize";
            public const string FLAG_DELBLK = "deleteBlock";
            public const string FLAG_ADDBLK = "addBlock";
            public const string FLAG_GRID = "gridEnabled";
            public const string FLAG_LIGHT = "lightEnabled";
            public const string RESOLUTION_X = "ResolutionX";
            public const string RESOLUTION_Y = "ResolutionY";
            public const string SCREEN_CENTER_X = "screenCenterX";
            public const string SCREEN_CENTER_Y = "screenCenterY";
            public const string MAX_STEPS = "MAX_STEPS";

            public const string LIGHTPOS = "lightPos";
            public const string LIGHTIDX = "lightIdx";

            public const int THREAD_NUMBER_X = 8;
            public const int THREAD_NUMBER_Y = 8;
        }

        readonly float PI = Mathf.PI / 180f;

        public static Controller2 instance;

        public InputManager inputManager;
        private void Awake()
        {
            instance = this;
            Application.targetFrameRate = 60;

            inputManager = new InputManager();
            inputManager.Enable();

            pos_temp = pos;
            rot_temp = rot;


            inputManager.Player.Quality.performed += val =>
            {
                if (val.ReadValue<float>() > 0)
                    SetQuality(1);
                else SetQuality(-1);
            };
            inputManager.Player.Mouse1.performed += val => 
            {
                if (val.ReadValue<float>() > 0)
                    delBlockFlag = true;
            };
            inputManager.Player.Fire.performed += val =>
            {
                if (val.ReadValue<float>() > 0)
                    addBlockFlag = true;
            };
            Init();
        }

        void Init()
        {
            KernelID = computeShader.FindKernel(CSPARAM.KERNELID);
            centerPos = new ComputeBuffer(1, sizeof(int));
            computeShader.SetBuffer(KernelID, CSPARAM.CENTER, centerPos);
            computeShader.SetFloat(CSPARAM.MAX_STEPS, distanceLevels[currentDLevel]);
            ShowGrid(false);
            ShowShadow(false);
        }

        public void SetTexture()
        {
            resolution.x = (int)(Screen.currentResolution.width * qualityLevels[currentQLevel]);
            resolution.y = (int)(Screen.currentResolution.height * qualityLevels[currentQLevel]);

            renderTexture?.Release();
            renderTexture = new RenderTexture(resolution.x, resolution.y, 24);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
            rawImage.texture = renderTexture;
            computeShader.SetInt(CSPARAM.RESOLUTION_X, resolution.x);
            computeShader.SetInt(CSPARAM.RESOLUTION_Y, resolution.y);
            computeShader.SetInt(CSPARAM.SCREEN_CENTER_X, resolution.x/2);
            computeShader.SetInt(CSPARAM.SCREEN_CENTER_Y, resolution.y/2);
            computeShader.SetTexture(KernelID, CSPARAM.RESULT, renderTexture);
        }

        IEnumerator mainLoop;
        public void StartRendering(Vector3Int mapSize, Vector4[] mapData)
        {
            if (mainLoop != null)
            {
                StopCoroutine(mainLoop);
                mapBuffer.Release();
            }
            
            mapBuffer =  new ComputeBuffer(mapSize.x * mapSize.y * mapSize.z, sizeof(float) * 4);
            mapBuffer.SetData(mapData);
            computeShader.SetBuffer(KernelID, CSPARAM.MAP, mapBuffer);
            computeShader.SetVector(CSPARAM.MAPSIZE, (Vector3)mapSize);
            SetTexture();

            mainLoop = Loop();
            StartCoroutine(mainLoop);
        }

        void ReleaseBuffers()
        {
            mapBuffer?.Release();
            centerPos?.Release();
        }
        void OnDestroy()
        {
            ReleaseBuffers();
            inputManager.Dispose();
            Destroy(this);
        }

        IEnumerator Loop()
        {
            while(true)
            {
                CheckMouseClick();

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
                DispatchComputeShader();
                
                yield return null;
            }
           
        }

        float[] qualityLevels = {0.0625f, 0.125f,0.25f, 0.5f, 1};
        int currentQLevel = 2;

        public Text qText;
        public void SetQuality(int i)
        {
            currentQLevel += i;
            currentQLevel = (int)Mathf.Clamp(currentQLevel, 0, 4);
            SetTexture();
            qText.text = currentQLevel.ToString();
        }


        int[] distanceLevels = { 50, 100, 200, 300, 400};
        int currentDLevel = 1;
        public Text dText;
        public void SetDistance(int i)
        {
            currentDLevel += i;
            currentDLevel = Mathf.Clamp(currentDLevel, 0, 4);
            computeShader.SetFloat(CSPARAM.MAX_STEPS, distanceLevels[currentDLevel]);
            dText.text = currentDLevel.ToString();
        }
        public void ShowGrid(bool b)
        {
            computeShader.SetBool(CSPARAM.FLAG_GRID, b);
        }

        public void ShowShadow(bool b)
        {
            computeShader.SetVector(CSPARAM.LIGHTPOS, pos);
            computeShader.SetBool(CSPARAM.FLAG_LIGHT, b);
        }

        public void SetGamepadMode(bool b)
        {
            if (b) rot_sensitivity = Vector2.one * 2;
            else rot_sensitivity = Vector2.one * 0.2f;

            touchPad.gameObject.SetActive(!b);
            controllerPad.gameObject.SetActive(b);
        }
        public void CheckMouseClick()
        {
            computeShader.SetBool(CSPARAM.FLAG_DELBLK, delBlockFlag);
            computeShader.SetBool(CSPARAM.FLAG_ADDBLK, addBlockFlag);
            delBlockFlag = false;
            addBlockFlag = false;
        }

        
        private void DispatchComputeShader()
        {
            computeShader.Dispatch(KernelID, Mathf.CeilToInt( 1.0f * resolution.x / CSPARAM.THREAD_NUMBER_X), Mathf.CeilToInt(1.0f * resolution.y / CSPARAM.THREAD_NUMBER_Y), 1);
            //타게팅 된 블록의 인덱스 가져오기.
            int[] centerIdx = { 0 };
            centerPos.GetData(centerIdx);
            computeShader.SetInt(CSPARAM.CENTERBEF,centerIdx[0]);
        }
        void Rotate()
        {
            rot_temp = Vector3.Lerp(rot_temp, rot,0.3f);
            computeShader.SetVector(CSPARAM.ROTATION, rot_temp * PI);
        }

        void Move(Vector3 d)
        {
            d *= moveSpeed * Time.deltaTime;
            float sin = Mathf.Sin(rot_temp.y * PI);
            float cos = Mathf.Cos(rot_temp.y * PI);
            pos += new Vector3(d.x * cos - d.z * sin, d.y, d.x * sin + d.z * cos);

            pos_temp = Vector3.Lerp(pos_temp, pos, 0.1f);
            computeShader.SetVector(CSPARAM.POSITION, pos_temp);
        }
    }
}