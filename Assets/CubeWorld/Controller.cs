using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace VirtualCam
{
	class Controller : MonoBehaviour
	{
		private World world;
		private Camera cam;
		private XYZ_d Position;
		XYZ_d scalaVector = new XYZ_d();
		XYZ halfBodySize = new XYZ(5,5,10);
		double speed = 80;
		double PI = Math.PI / 180d;
        public static Controller instance;
        XYZ_d Vector = new XYZ_d();
        XYZ_d gravity = new XYZ_d();
        bool isJumpable = true;

        public EventTrigger UpArrowButton = null;
        public EventTrigger DownArrowButton = null;
        public EventTrigger LeftArrowButton = null;
        public EventTrigger RightArrowButton = null;
        public Button AddBlockButton = null;
        public Button SubBlockButton = null;
        public Button GridButton = null;
        public Button JumpButton = null;
        private void Awake()
        {
            instance = this;
        }
        public void Init(World w, Camera c)
		{
            instance = this;
			world = w; 
			cam = c;
			Position = cam.GetPosition();
			render = world.renderer_block;
            InitJoystic();

        }

        void InitJoystic()
        {
            EventTrigger.Entry entry_PointerDown = new EventTrigger.Entry();
            EventTrigger.Entry entry_PointerUp = new EventTrigger.Entry();
            entry_PointerDown.eventID = EventTriggerType.PointerDown;
            entry_PointerDown.callback.AddListener((data) =>{ keyW = true;});
            entry_PointerUp.eventID = EventTriggerType.PointerUp;
            entry_PointerUp.callback.AddListener((data) => { keyW = false; });
            UpArrowButton.triggers.Add(entry_PointerDown);
            UpArrowButton.triggers.Add(entry_PointerUp);

            entry_PointerDown = new EventTrigger.Entry();
            entry_PointerUp = new EventTrigger.Entry();
            entry_PointerDown.eventID = EventTriggerType.PointerDown;
            entry_PointerDown.callback.AddListener((data) => { keyS = true; });
            entry_PointerUp.eventID = EventTriggerType.PointerUp;
            entry_PointerUp.callback.AddListener((data) => { keyS = false; });
            DownArrowButton.triggers.Add(entry_PointerDown);
            DownArrowButton.triggers.Add(entry_PointerUp);

            entry_PointerDown = new EventTrigger.Entry();
            entry_PointerUp = new EventTrigger.Entry();
            entry_PointerDown.eventID = EventTriggerType.PointerDown;
            entry_PointerDown.callback.AddListener((data) => { keyA = true; });
            entry_PointerUp.eventID = EventTriggerType.PointerUp;
            entry_PointerUp.callback.AddListener((data) => { keyA = false; });
            LeftArrowButton.triggers.Add(entry_PointerDown);
            LeftArrowButton.triggers.Add(entry_PointerUp);

            entry_PointerDown = new EventTrigger.Entry();
            entry_PointerUp = new EventTrigger.Entry();
            entry_PointerDown.eventID = EventTriggerType.PointerDown;
            entry_PointerDown.callback.AddListener((data) => { keyD = true; });
            entry_PointerUp.eventID = EventTriggerType.PointerUp;
            entry_PointerUp.callback.AddListener((data) => { keyD = false; });
            RightArrowButton.triggers.Add(entry_PointerDown);
            RightArrowButton.triggers.Add(entry_PointerUp);


            AddBlockButton.onClick.AddListener(AddBlock);
            SubBlockButton.onClick.AddListener(DeleteBlock);
            GridButton.onClick.AddListener(()=> { cam.gridEnabled = !cam.gridEnabled; });
            JumpButton.onClick.AddListener(() => { if (isJumpable) { Vector.z = -80; isJumpable = false; } });
        }
        float h;
        float v;
        //readonly string flag_Horizontal = "Horizontal";
        //readonly string flag_Vertical = "Vertical";

        bool keyW = false;
        bool keyA = false;
        bool keyS = false;
        bool keyD = false;
        void Update()
        {  
            if (Input.GetKeyDown(KeyCode.W)) keyW = true;
            if (Input.GetKeyDown(KeyCode.A)) keyA = true;
            if (Input.GetKeyDown(KeyCode.S)) keyS = true;
            if (Input.GetKeyDown(KeyCode.D)) keyD = true;

            if (Input.GetKeyUp(KeyCode.W)) keyW = false;
            if (Input.GetKeyUp(KeyCode.A)) keyA = false;
            if (Input.GetKeyUp(KeyCode.S)) keyS = false;
            if (Input.GetKeyUp(KeyCode.D)) keyD = false;
            if (keyW) Move(0, 1, 0);
            if (keyA) Move(-1, 0, 0);
            if (keyS) Move(0, -1, 0);
            if (keyD) Move(1, 0, 0);

            if (Input.GetKeyDown(KeyCode.E)) Shoot(new XYZ_b(255, 0, 0));
            if(Input.GetKeyDown(KeyCode.R)) Shoot(new XYZ_b(0, 255, 0));
            if(Input.GetKeyDown(KeyCode.T)) Shoot(new XYZ_b(0, 0, 255));
            if(Input.GetKeyDown(KeyCode.Q))
            {
                double boostSpeed = 50;
                Vector.Add(cam.rayDelta.x * boostSpeed, cam.rayDelta.y * boostSpeed, cam.rayDelta.z * boostSpeed);
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                cam.gridEnabled = !cam.gridEnabled;
            }
            if (Input.GetKeyDown(KeyCode.Space)) AddBlock();
            if (Input.GetKeyDown(KeyCode.X)) DeleteBlock();
            if (Input.GetKeyDown(KeyCode.V)) if (isJumpable) { Vector.z = -80; isJumpable = false; }

            // Block block = world.GetBlock(cam.addFrameIndex);
            //Debug.Log(block.color.x + "," + block.color.y + "," + block.color.z);
            //Debug.Log(cam.addFrameIndex.x + "," + cam.addFrameIndex.y + "," + cam.addFrameIndex.z);
            XYZ_d p = cam.GetPosition();
           // Debug.Log(p.x + "," + p.y + "," + p.z);

        }
		void RegistKey()
		{
            
            /* InputManager.Regist(Keys.D2, new Func(() => { color = (byte)15; }), false);
            InputManager.Regist(Keys.D3, new Func(() => { color = (byte)14; }), false);
            InputManager.Regist(Keys.D4, new Func(() => { color = (byte)4; }), false);
            InputManager.Regist(Keys.D5, new Func(() => { color = (byte)5; }), false);
            InputManager.Regist(Keys.D6, new Func(() => { color = (byte)6; }), false);
            InputManager.Regist(Keys.D7, new Func(() => { color = (byte)7; }), false);
            InputManager.Regist(Keys.D8, new Func(() => { code = (byte)14; }), false);
            InputManager.Regist(Keys.D9, new Func(() => { code = (byte)15; }), false);
            InputManager.Regist(Keys.D0, new Func(() => { code = (byte)1; }), false); */



            /*InputManager.Regist(Keys.D0, new Func(() => { renderer = world.renderer_block; }), false);
            InputManager.Regist(Keys.D1, new Func(() => { renderer = world.renderer_air; }), false);
            InputManager.Regist(Keys.D2, new Func(() => { renderer = world.renderer_mirror; }), false);
            InputManager.Regist(Keys.Space, new Func(() => { AddBlock(); }), false);
            InputManager.Regist(Keys.X, new Func(() => { DeleteBlock(); }), false);*/

        }
		
		byte color = 6;
		public Func<XYZ_d, XYZ, XYZ, int, bool> render;
		public void AddBlock()
		{
            XYZ pos = cam.PositionIndex;
            if (world.IsInFrame(cam.addFrameIndex) && 
                !cam.addFrameIndex.Equal(pos) && !cam.addFrameIndex.Equal(pos.x,pos.y,pos.z+1))
			{
                //world.SetBlock(camera.addFrameIndex, code);
                world.SetBlock(cam.addFrameIndex,true);
				world.SetRender(cam.addFrameIndex, render);
                world.SetColor(cam.addFrameIndex,new XYZ_b((byte)(color * 25)));
			}
		}
		
		public void DeleteBlock()
		{
			if(world.IsInFrame(cam.deleteFrameIndex))
			{
				world.SetBlock(cam.deleteFrameIndex,false);
				world.SetColor(cam.deleteFrameIndex,0,0,0);
				world.SetRender(cam.deleteFrameIndex,world.renderer_air);
			}
		}

		public void Move(XYZ_d vector){Move(vector.x,vector.y,vector.z);}
		public void Move(double x, double y, double z)
		{
			Spin_matrix_z(x,y,cam.GetCursorPos().x,scalaVector);
			scalaVector.Mul(speed);
            //scalaVector.Set(camera.rayDelta).Mul(y,y,y).Add(camera.basisX.x * x,camera.basisX.y * x,0).Mul(speed);
			
			if (!Check_Wall(scalaVector))
            {
                Position.Add(scalaVector);
                world.ConvertToInfinity(Position);
            }
            else
            {
                XYZ_d pos = new XYZ_d(Position).Add(scalaVector);
                XYZ index = new XYZ();
                world.GetFrameIndex(pos, index);
                if (!world.isFrameEnabled(index))
                {
                    Vector.z = -40;
                }
            }
		}
		
        bool Check_Wall(XYZ_d p)
        {
            XYZ_d pos = new XYZ_d(Position).Add(p);
            pos.z += world.frameLength;

            bool a = world.isFrameEnabled(pos);
            pos.z -= world.frameLength;
            bool b = world.isFrameEnabled(pos);
            return a || b;
        }
        public void CrushFrame(int scale)
        {
            XYZ temp = Vector.ToXYZ();
            temp.Div((int)(temp.Length() / (scale / 4 * 3)));
            XYZ brokePos = new XYZ(cam.PositionIndex).Sub(temp);
            XYZ lightPos = new XYZ(cam.PositionIndex);
            int maxScale = 60;
            if (scale > maxScale) scale = maxScale;
            int lightScale = scale + 2;
            XYZ color = new XYZ(255 * scale / 30, 0, 0);
            XYZ gap = new XYZ();

            for(int i = -lightScale; i < lightScale; i++)
            for(int j = -lightScale; j < lightScale; j++)
            for(int k = -lightScale; k < lightScale; k++)
            {
                temp.Set(brokePos).Add(i, j, k);
                world.ConvertToInfinity(temp);
                //if (!world.IsInFrame(temp)) continue;
                if (Math.Sqrt(i * i + j * j + k * k) < scale)
                {
                    world.SetFrame(temp, false);
                }
                //double distance = lightPos.Distance(temp);
                //if (distance < scale)
                //{
                //    XYZ_b c = world.GetColor(temp);
                //    if (world.isFrameEnabled(temp))
                //    {
                //        gap.x = (int)color.x - (int)c.x;
                //        gap.y = (int)color.y - (int)c.y;
                //        gap.z = (int)color.z - (int)c.z;
                //        gap.Mul((int)(maxScale - distance)).Div(maxScale);
                //        c.x = (byte)(c.x + gap.x);
                //        c.y = (byte)(c.y + gap.y);
                //        c.z = (byte)(c.z + gap.z);
                //    }
                //}
            }
        }
        
        public void Falling()
        {
            Vector.z += 4;
            if(!Check_Wall(Vector))
            {
                Position.Add(Vector);
                world.ConvertToInfinity(Position);
            }
            else if(Vector.Length() != 0)
            {
                XYZ index = new XYZ();
                world.GetFrameIndex(new XYZ_d(Position).Add(Vector), index);
                
                double vLength = Vector.Length();
                if (vLength > 90)
                {
                   // if(world.IsInFrame(index))
                     //   CrushFrame((int)(vLength / 4));
                    Vector.Div(4);
                    return;
                }
                else isJumpable = true;

                world.ConvertIndexToPosition(index);
                if (Vector.z > 0)
                {
                    Position.z = index.z - world.frameLength / 2 - 1;
                   
                }
                else
                {
                    Position.z = index.z + world.frameLength / 2 + 4;
                    isJumpable = false;
                }
                Vector.Set(0);

            }
        }

        void Shoot(XYZ_b color)
        {
            XYZ_d delta = new XYZ_d(cam.rayDelta);
            XYZ_d lpos = new XYZ_d(delta).Mul(world.frameLength * 2).Add(Position);
            XYZ frameIndex = new XYZ();
            world.GetFrameIndex(lpos, frameIndex);
            world.ConvertToFramePos(frameIndex,lpos );
            XYZ frameIndex2 = new XYZ(frameIndex);
			int nextDir = 0;
            XYZ_d target = new XYZ_d();
            XYZ_d deltaSign = new XYZ_d();
			XYZ_d maxNumOfDelta = new XYZ_d(world.frameLength);
            if (delta.x != 0) deltaSign.x = (Math.Abs(delta.x) / delta.x);
            if (delta.y != 0) deltaSign.y = (Math.Abs(delta.y) / delta.y);
            if (delta.z != 0) deltaSign.z = (Math.Abs(delta.z) / delta.z);
			
			maxNumOfDelta.Mul(deltaSign).Div(delta);
			target.Set(world.halfFrameLength).Mul(deltaSign);//delta���� �������� �̵��� ���˰����� ����� ���ϱ�.
            target.Sub(lpos).Div(delta);//�����κ��� ������ġ�� �Ÿ��� ���ϰ� delta�� ������. deltasign���� �ѹ� ���߾��⶧���� x,y,z�� ���ο� ���� ��Ȯ�� �񱳰��� �����Եȴ�.
            Task.Factory.StartNew(() =>
            {
                while (!world.isFrameEnabled(frameIndex))
                {
                    world.ConvertToInfinity(frameIndex);
                    world.SetFrame(frameIndex2, false);
                    world.SetFrame(frameIndex, true);
                    world.SetColor(frameIndex, color);
                  
                    if (target.x < target.y)
						if(target.x < target.z) nextDir = 0;
						else nextDir = 2;
					else
						if(target.y < target.z) nextDir = 1;
						else nextDir = 2;
					target.element[nextDir] += maxNumOfDelta.element[nextDir];
					frameIndex2.Set(frameIndex);
					frameIndex.element[nextDir] += (int)deltaSign.element[nextDir];
                    Thread.Sleep(10);
                }
				
                world.SetFrame(frameIndex2, false);
                XYZ temp = new XYZ();
                int scale = 10;
                double distance = 0;
                for (int i = -scale; i < scale; i++)
                    for (int j = -scale; j < scale; j++)
                        for (int k = -scale; k < scale; k++)
                        {
                            temp.Set(frameIndex).Add(i, j, k);
                            distance = frameIndex.Distance(temp);
                            if (distance < scale)
                            {
								world.ConvertToInfinity(temp);
                                XYZ_b c = world.GetColor(temp);
                                //gap.x = (int)color.x - (int)c.x;
                                //gap.y = (int)color.y - (int)c.y;
                                //gap.z = (int)color.z - (int)c.z;
                                //gap.Mul((int)(scale - distance)).Div(scale);
                                //c.x = (byte)(c.x + gap.x);
                                //c.y = (byte)(c.y + gap.y);
                                //c.z = (byte)(c.z + gap.z);
                                temp.x = (int)color.x * (int)(scale - distance) / scale;
                                temp.y = (int)color.y * (int)(scale - distance) / scale;
                                temp.z = (int)color.z * (int)(scale - distance) / scale;
                                c.Add((byte)temp.x, (byte)temp.y, (byte)temp.z);
                                // world.SetFrame(temp, false);
                                //world.SetColor(temp, 255, 0, 0);
                            }
                        }
                    });
        }
		void Spin_matrix_z(double x, double y, double d, XYZ_d position)
		{
			double degree = d * PI;
			double sin = Math.Sin(degree);
			double cos = Math.Cos(degree);
	
			position.x = x * cos + y * sin;
			position.y = y * cos + x * (-sin);
		}
		
		/* void Spin_matrix_z(double x, double y, double d, XYZ_d position, XYZ_d point)
		{
			double degree = d * PI;
			double sin = Math.Sin(degree);
			double cos = Math.Cos(degree);
	
			position.x = (x - point.x) * cos + (y - point.y) * sin + point.x;
			position.y = (y - point.y) * cos + (x - point.x) * (-sin) + point.y;
		} */
	}
}