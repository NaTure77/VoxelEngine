using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
namespace VirtualCam
{
	class Camera
	{
		private World world;
		
		public XYZ camSize;
		public XYZ camPoint;
		private XYZ_d Position;
		private XYZ_d position;
		public XYZ PositionIndex;
		
		private XYZ_d[,] perspArray;		
		private XYZ_d[,] deltaArray;
		private XYZ_d[,] perspBasisX;
		private XYZ_d[,] perspBasisZ;
		private double sensitivity = 0.2d;
		XY<double> cursor = new XY<double>(0,0);
		public bool gridEnabled = false;
		private double PI = Math.PI / 180d;
		public XYZ_d rayDelta = new XYZ_d();
		public XYZ deleteFrameIndex;
		public XYZ addFrameIndex = new XYZ();
		public XYZ_d basisX;
		private XYZ_d basisZ;
		public XYZ_d basisY;

        private XYZ[,] frameBoard;
		private XYZ[,] frameInk;
		private XYZ[,] deltaSignBoard;
        private XYZ_d[,] lposBoard;
       
		private int[,] lposDirectBoard;
        private XYZ_b[,] finalBoard;
        public Camera(XYZ cs, XYZ_d cPos, World w)
		{
			camSize = cs;
			Position = cPos;
			PositionIndex = new XYZ();
			world = w;
			camPoint = new XYZ(camSize).Div(2);
			position = new XYZ_d(Position);
			
			perspArray = new XYZ_d[camSize.x,camSize.z];
			deltaArray = new XYZ_d[camSize.x,camSize.z];
			perspBasisX = new XYZ_d[camSize.x,camSize.z];
			perspBasisZ = new XYZ_d[camSize.x,camSize.z];

            frameBoard = new XYZ[camSize.x, camSize.z];
			frameInk = new XYZ[camSize.x, camSize.z];
			deltaSignBoard = new XYZ[camSize.x, camSize.z];
            lposBoard = new XYZ_d[camSize.x, camSize.z];
            
            lposDirectBoard = new int[camSize.x, camSize.z];
            finalBoard = new XYZ_b[camSize.x, camSize.z];
            for (int i = 0; i < camSize.x; i++)
			for (int j = 0; j < camSize.z; j++)
			{
				perspArray[i, j] = new XYZ_d();
				deltaArray[i, j] = new XYZ_d();
				perspBasisX[i, j] = new XYZ_d();
				perspBasisZ[i, j] = new XYZ_d();
				finalBoard[i, j] = new XYZ_b();
				lposBoard[i, j] = new XYZ_d();
				frameBoard[i, j] = new XYZ();
				frameInk[i, j] = new XYZ();
				deltaSignBoard[i, j] = new XYZ();
			}
			deleteFrameIndex = frameBoard[camPoint.x, camPoint.z];
			
            SetPerspArray();
			
            basisX = new XYZ_d(1,0,0);
			basisY = new XYZ_d(0,camSize.y - 1,0);
			basisZ = new XYZ_d(0,0,1);
			//Cursor.lockState = CursorLockMode.Locked;
		}
        public void SetPerspArray()
        {
            double fov = 10 / 3;
            double ratX = 240f / camSize.x;
            double ratZ = 144f / camSize.z;
            for (int i = 0; i < camSize.x; i++)
                for (int j = 0; j < camSize.z; j++)
                {
                    perspArray[i, j].x = (i - camPoint.x)/*  * (camSize.y - 1) */ * fov * ratX;
                    perspArray[i, j].y = camSize.y - 1;
                    perspArray[i, j].z = (j - camPoint.z) /* * (camSize.y - 1)  */* fov * ratZ;
                }
        }
		public void Resize(int x, int y, int z)
		{
			camSize.Set(x,y,z); 
			camPoint.Set(camSize).Div(2);
			SetPerspArray(); 
			deleteFrameIndex = frameBoard[camPoint.x,camPoint.z];
		}
		public XYZ_d GetPosition(){return Position;}
		public XY<double> GetCursorPos(){return cursor;}
        
		
		
		Action<int, int, int> checkFrame;
		
		Vector2 currentCursorPos = new Vector2();
		public void Spin_XZAxis5()
		{
			
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
			cursor.x += (Input.mousePosition.x - currentCursorPos.x) * sensitivity;
			cursor.y -= (Input.mousePosition.y - currentCursorPos.y) * sensitivity;
			currentCursorPos = Input.mousePosition;
#elif UNITY_ANDROID
			if (Input.touchCount > 0)
			Vector2 input = input = Input.GetTouch(0).deltaPosition;
			cursor.x += input.x * sensitivity;
			cursor.y -= input.y * sensitivity;
			input = Vector2.zero;
#endif
			cursor.x %= 360;
			cursor.y %= 360;
			cursor.y = cursor.y > 90 ? 90 :
					   (cursor.y < -90 ? -90 : cursor.y);

			position.Set(Position);
			double degreeX = cursor.y * -PI;
			double degreeY = cursor.x * PI;
			double sinX = Math.Sin(degreeX);
			double sinY = Math.Sin(degreeY);
			double cosX = Math.Cos(degreeX);
			double cosY = Math.Cos(degreeY);

            basisX.Set(cosY, -sinY, 0);
            basisY.Set(cosX * sinY, cosX * cosY, -sinX).Mul(300);
            basisZ.Set(sinX * sinY, sinX * cosY, cosX);

            world.GetFrameIndex(position,PositionIndex);//현재 위치를 박스 단위 위치로 변환
			world.ConvertToInfinity(PositionIndex);
            world.ConvertToFramePos(PositionIndex,position);// 현재 위치를 현재 박스의 로컬 좌표로 전환
			
			//for(int k = 0; k < camSize.z; k++)
			Parallel.For(0,camSize.z,(int k) =>
			{
                Parallel.For(0,camSize.x,(int i) =>
                //for(int i = 0; i < camSize.x; i++)
				{
                    finalBoard[i, k].Set(0);//XYZ_b
                    lposBoard[i, k].Set(0);//XYZ_d
                    frameBoard[i, k].x = -1;;//XYZ
                  
					perspBasisX[i,k].Set(basisX).Mul(perspArray[i,k].x);
					perspBasisZ[i,k].Set(basisZ).Mul(perspArray[i,k].z);
					XYZ_d delta = deltaArray[i,k].Set(perspBasisX[i, k]).Add(basisY).Add(perspBasisZ[i, k]).Div(300);
                    XYZ frameIndex = frameInk[i,k].Set(PositionIndex);

					XYZ_d target = perspBasisX[i,k]; //new XYZ_d();
					XYZ deltaSign = deltaSignBoard[i,k].Set(Math.Sign(delta.x),Math.Sign(delta.y),Math.Sign(delta.z));
					XYZ_d maxNumOfDelta = perspBasisZ[i,k].Set(world.frameLength).Mul(deltaSign).Div(delta); // 각 축에 대해 한 칸 이동시 delta요소들이 몇개가 필요한지.

                    //현위치에서 delta벡터방향으로 이동할 경우 가장 먼저 만나는 경계면 구하기.
                    target.Set(world.halfFrameLength);
                    target.Mul(deltaSign);//delta벡터 방향으로 이동시 접촉가능한 경계면들 구하기.
                    target.Sub(position).Div(delta);//경계면들로부터 현재위치의 거리를 구하고 delta로 나누기. deltasign으로 한번 곱했었기때문에 x,y,z축 서로에 대한 정확한 비교값이 나오게된다.
													// 시작 값.
					Block block;
					int nextDir = 0; // x = 0, y = 1, z = 2
					int j = 0;
                    for (j = 0; j < camSize.y; j++)
                    {
                        block = world.Map[frameIndex.x,frameIndex.y,frameIndex.z];//world.GetBlock(frameIndex);
						finalBoard[i, k].Add(block.color);
						if (frameBoard[i, k].x == -1 && block.touchable)
						{
							frameBoard[i, k].Set(frameIndex);
							target.element[nextDir] -= maxNumOfDelta.element[nextDir];
							target.Sub(target.element[nextDir]);
							target.element[nextDir] = maxNumOfDelta.element[nextDir];			
							lposBoard[i, k].Set(maxNumOfDelta).Mul(0.5).Sub(target).Mul(delta);
							lposDirectBoard[i,k] = nextDir;
						}
						
						//계속 탐색 or 스톱
						if(!block.OnRendered(delta,deltaSign,frameIndex,nextDir)) break;
						
						//다음 위치 탐색(최빈출)
                        if (target.x < target.y)
                            if(target.x < target.z) nextDir = 0;
                            else nextDir = 2;
                        else
                            if(target.y < target.z) nextDir = 1;
                            else nextDir = 2;	
						target.element[nextDir] += maxNumOfDelta.element[nextDir];
						frameIndex.element[nextDir] += deltaSign.element[nextDir];
						world.ConvertToInfinity(frameIndex);
						//가장가까운 경계면에 해당하는 블록으로 이동.
					}
                   // if (j == camSize.y)finalBoard[i, k].Add(128, 255, 255);
				});
			});
            rayDelta.Set(deltaArray[camPoint.x,camPoint.z]);

			//Debug.Log(finalBoard[camPoint.x, camPoint.z].x + "," + finalBoard[camPoint.x, camPoint.z].y + "," + finalBoard[camPoint.x, camPoint.z].z);
        }
        
        public XYZ_b[,] MakeImage()
        {
			int pointDir = lposDirectBoard[camPoint.x, camPoint.z];
            if (deleteFrameIndex.x != -1)
            {
                addFrameIndex.Set(deleteFrameIndex);
			
				if(basisY.element[pointDir] > 0)
					addFrameIndex.element[pointDir] -=1;
				
				else addFrameIndex.element[pointDir] +=1;
                
				world.ConvertToInfinity(addFrameIndex);
            }
            if(gridEnabled)
			{
				XYZ vector = new XYZ(addFrameIndex).Sub(deleteFrameIndex);
				
				int inSideArea = world.halfFrameLength - 4;
				//Parallel.For(0, camSize.z, (int k) =>
				for(int k = 0; k < camSize.z; k++)
				{
					//Parallel.For(0, camSize.x, (int i) =>
					for (int i = 0; i < camSize.x; i++)
					{                  
						if (frameBoard[i, k].x != -1)
						{
							Block block = world.GetBlock(frameBoard[i, k]);
							XYZ_b inv_color = new XYZ_b(255).Sub(block.color);
							if (frameBoard[i,k].Equal(deleteFrameIndex) && lposDirectBoard[i,k] == pointDir)//쳐다보는 블록면 강조
							{
								finalBoard[i, k].Set(inv_color);
							}
							//if (block.code != 14)// 경계선표시.
							{
								bool Xaxis = Math.Abs(lposBoard[i, k].x) > inSideArea; //경계선 테두리 크기 1
								bool Yaxis = Math.Abs(lposBoard[i, k].y) > inSideArea;
								bool Zaxis = Math.Abs(lposBoard[i, k].z) > inSideArea;
								if (Xaxis && Yaxis || Yaxis && Zaxis || Zaxis && Xaxis) { finalBoard[i, k].Set(inv_color); }
								//else if () { finalBoard[i, k].Set(inv_color); }
								//else if () { finalBoard[i, k].Set(inv_color); }
								//else finalBoard[i, k].Set(block.color);
							}
						}
					}//);
				}//);
			}
            for (int i = camPoint.z - 3; i < camPoint.z + 3; i++)
                for (int j = camPoint.x - 3; j < camPoint.x + 3; j++)
                    if (Math.Pow(i - camPoint.z, 2) + Math.Pow(j - camPoint.x, 2) < 8 &&
                       Math.Pow(i - camPoint.z, 2) + Math.Pow(j - camPoint.x, 2) > 4) finalBoard[j, i].Set(255);
            return finalBoard;
        }
			//basisX.Set(1,0,0);
			//basisY.Set(0,camSize.y - 1,0);
			//basisZ.Set(0,0,1);
            //Spin_matrix_x(basisX.y,basisX.z,sinX,cosX,basisX);
            //Spin_matrix_z(basisX.x,basisX.y,sinY,cosY,basisX);
            //Spin_matrix_x(basisY.y,basisY.z,sinX,cosX,basisY);
            //Spin_matrix_z(basisY.x,basisY.y,sinY,cosY,basisY);
            //Spin_matrix_x(basisZ.y,basisZ.z,sinX,cosX,basisZ);
            //Spin_matrix_z(basisZ.x,basisZ.y,sinY,cosY,basisZ);
		/*void Spin_matrix_x(double y, double z, double sin, double cos, XYZ_d position)
		{
			position.y = y * cos + z * sin;
			position.z = z * cos + y * sin * -1;
		}
		void Spin_matrix_z(double x, double y,double sin, double cos, XYZ_d position)
		{
			position.x = x * cos + y * sin;
			position.y = y * cos + x * sin * -1;
		}*/
		
		
	}
}