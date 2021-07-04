using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VirtualCam
{
	class Viewer : MonoBehaviour
    {
       // int pixelSize;
        XY<int> screenSize;

        public RawImage display = null;

        Texture2D buffer;
        Texture2D bufferTemp;
        Camera cam;
        XYZ camSize;
        public void UpdateBufferSize(XYZ camSize)
        {
            buffer = new Texture2D(camSize.x, camSize.z);
            display.texture = buffer;
        }

       
        int qualityLevel = 0;

        public void SetGreaterQuality()
        {
            if (qualityLevel < 2)
            {
                cam.Resize(camSize.x * 2, camSize.y, camSize.z * 2);
                UpdateBufferSize(camSize);
                qualityLevel++;
            }
        }
       public void SetLowerQuality()
        {
            if (qualityLevel > -2)
            {
                cam.Resize(camSize.x / 2, camSize.y, camSize.z / 2);
                UpdateBufferSize(camSize);
                qualityLevel--;
            }
        }
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape)) isPaused = !isPaused;
            if (Input.GetKeyDown(KeyCode.N)) SetGreaterQuality();
            if (Input.GetKeyDown(KeyCode.M)) SetLowerQuality();

            if (isPaused) return;
            Controller.instance.Falling();
            cam.Spin_XZAxis5();
            Draw(cam.MakeImage());
           // UpdateBuffer();
            //ShowImage();
        }

        public static Viewer instance;

        private void Awake()
        {
            instance = this;
        }
        public void Init(Camera cam, XYZ camSize)
        {
            this.cam = cam;
            screenSize = new XY<int>(camSize.x,camSize.z);
            this.camSize = camSize;
            UpdateBufferSize(camSize);
		    cam.Resize(camSize.x/4,camSize.y,camSize.z/4);
		    UpdateBufferSize(camSize);
            display.texture = buffer;
            //graphics.CompositingMode = CompositingMode.SourceCopy;
        }
        bool isPaused = false;

        public void InitDraw()
        {

        }
        public void ShowImage()
        {
            display.texture = buffer;
            //graphics.DrawImage(_backBuffer, rect);
        }



        public void Draw(XYZ_b[,] data)
        {
            Color color = new Color(0,0,0,1);
            int height = buffer.height;
            int width = buffer.width;
            for (int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    color.r = data[j, i].x / (float)256;
                    color.g = data[j, i].y / (float)256;
                    color.b = data[j, i].z / (float)256;
                    buffer.SetPixel(j, height - i - 1, color);
                }
            }
            buffer.Apply();
            //display.texture = buffer;
            //Debug.Log(buffer.GetPixel(buffer.width/2,buffer.height/2));
            
        }
    }
}