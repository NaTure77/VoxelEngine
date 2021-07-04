using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CubeWorld
{
    public class Controller : MonoBehaviour
    {
        public Texture2D cloudTexture;
        public Texture2D mapTexture;
		public Material mat;

		public Vector3 pos;
		public Vector3 rot;
		public float moveSpeed = 2f;

        float sensitivityX = 4F;
        float sensitivityY = 4F;



        readonly float PI = Mathf.PI / 180f;
        Vector4[] map = null;

		Vector3Int mapSize = new Vector3Int(4, 4, 4);
        void Start()
        {
            Application.targetFrameRate = 60;
			map = new Vector4[mapSize.x * mapSize.y * mapSize.z];
			DrawMap(mapTexture);

			mat.SetVectorArray("_Map", map);
            pos = mat.GetVector("_Position");
            rot = mat.GetVector("_Rotation");
        }
        public void DrawMap(Texture2D img)
		{
			int heightInPixels = mapSize.z > img.height ? img.height : mapSize.z;
			int widthInPixels = mapSize.x > img.width ? img.width : mapSize.x;
			int y0 = mapSize.y / 2;

			int yVal = mapSize.x * mapSize.z;
			int zVal = mapSize.x;
			for (int z = 0; z < heightInPixels; z++)
				for (int x = 0; x < widthInPixels; x++)
				{
					int idx = x + y0 * yVal + z * zVal;
					map[idx] = img.GetPixel(x, z);
					map[idx].w = 1;
				}
		}

        bool keyW = false;
        bool keyA = false;
        bool keyS = false;
        bool keyD = false;
        bool keyV = false;
        bool keyC = false;
        private void Update()
        {
            Rotate();

            if (Input.GetKeyDown(KeyCode.W)) keyW = true;
            if (Input.GetKeyDown(KeyCode.A)) keyA = true;
            if (Input.GetKeyDown(KeyCode.S)) keyS = true;
            if (Input.GetKeyDown(KeyCode.D)) keyD = true;
            if (Input.GetKeyDown(KeyCode.V)) keyV = true;
            if (Input.GetKeyDown(KeyCode.C)) keyC = true;

            if (Input.GetKeyUp(KeyCode.W)) keyW = false;
            if (Input.GetKeyUp(KeyCode.A)) keyA = false;
            if (Input.GetKeyUp(KeyCode.S)) keyS = false;
            if (Input.GetKeyUp(KeyCode.D)) keyD = false;
            if (Input.GetKeyUp(KeyCode.V)) keyV = false;
            if (Input.GetKeyUp(KeyCode.C)) keyC = false;

            if (keyW) Move(Vector3.forward);
            if (keyA) Move(Vector3.left);
            if (keyS) Move(Vector3.back);
            if (keyD) Move(Vector3.right);
            if (keyV) Move(Vector3.up);
            if (keyC) Move(Vector3.down);
        }
        void Rotate()
        {
            rot.x -= Input.GetAxis("Mouse Y") * sensitivityY;
            rot.y -= Input.GetAxis("Mouse X") * sensitivityX;
            rot.x = Mathf.Clamp(rot.x, -90, 90);
            mat.SetVector("_Rotation", rot * PI);
        }
        void Move(Vector3 d)
        {
            d *= moveSpeed * Time.deltaTime;
            float sin = Mathf.Sin(rot.y * PI);
            float cos = Mathf.Cos(rot.y * PI);
            pos += new Vector3(d.x * cos - d.z * sin, d.y, d.x * sin + d.z * cos);
            mat.SetVector("_Position", pos);
        }
	}
}