using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
namespace VirtualCam
{
	class World : MonoBehaviour
	{
		//public Texture2D cloudTexture;
		public Texture2D mapTexture = null;
		//public Texture2D buildingTexture;
		private XYZ worldSize;
		public int frameLength = 90;
		public int halfFrameLength = 45;
		public XYZ frameSize = new XYZ();
		public XYZ_d realSize = new XYZ_d();
		public Block[,,] Map;
		public Func<XYZ_d, XYZ, XYZ, int, bool> renderer_block = (XYZ_d delta, XYZ deltaSign, XYZ frameIndex, int nextDir)=>{return false;};
		public Func<XYZ_d, XYZ, XYZ, int, bool> renderer_air = (XYZ_d delta, XYZ deltaSign, XYZ frameIndex, int nextDir)=>{return true;};
		public Func<XYZ_d, XYZ, XYZ, int, bool> renderer_mirror = (XYZ_d delta, XYZ deltaSign, XYZ frameIndex, int nextDir)=>
		{
			delta.element[nextDir] *= -1;
			deltaSign.element[nextDir] *= -1;
			frameIndex.element[nextDir] += deltaSign.element[nextDir];
			return true;
		};

		public static World instance;

		private void Awake()
		{
			instance = this;
		}

		public Func<XYZ_d, XYZ, XYZ, int, bool> renderer_penetration = (XYZ_d delta, XYZ deltaSign, XYZ frameIndex, int nextDir)=>
		{
			//delta.element[nextDir] *= -1;
			//deltaSign.element[nextDir] *= -1;
			frameIndex.element[nextDir] += deltaSign.element[nextDir] * 10;
			return true;
		};
        public void ConvertToInfinity(XYZ result)
        {
            result.Rem(frameSize);
            if (result.x < 0) result.x += frameSize.x;
            if (result.y < 0) result.y += frameSize.y;
            if (result.z < 0) result.z += frameSize.z;
        }
        public void ConvertToInfinity(XYZ_d result)
        {

            result.Rem(realSize);
            if (result.x < 0) result.x = realSize.x + result.x;
            if (result.y < 0) result.y = realSize.y + result.y;
            if (result.z < 0) result.z = realSize.z + result.z;
        }
        public bool IsExistPixelInFrame(XYZ p)
		{
			//return Map[p.x,p.y,p.z] != 0;
			//return Map[p.x,p.y,p.z].code != 0;
			return Map[p.x,p.y,p.z].touchable;
		}
		public void GetFrameIndex(XYZ_d p, XYZ i)
		{
            i.Set(p.ToXYZ().Div(frameLength));
            //ConvertToInfinity(i);

        }
		public void GetFrameIndex(XYZ_d p, XYZ_d i)
		{
            i.Set(p.ToXYZ().Div(frameLength));
        }
		public void ConvertIndexToPosition(XYZ i)
		{
            ConvertToInfinity(i);
            i.Mul(frameLength).Add(frameLength/2);
		}
		public void ConvertIndexToPosition(XYZ_d i)
		{
            i.Mul(frameLength).Add(frameLength/2);
		}
		public bool isFrameEnabled(XYZ_d p)
		{
			XYZ temp = new XYZ();
			GetFrameIndex(p,temp);
            ConvertToInfinity(temp);
            //return !IsInFrame(temp) || Map[temp.x,temp.y,temp.z] != 0;
            //return /*!IsInFrame(temp) || */Map[temp.x,temp.y,temp.z].code != 0/* && Map[temp.x, temp.y, temp.z].code != 15*/;
            return /*!IsInFrame(temp) || */Map[temp.x,temp.y,temp.z].touchable;/* && Map[temp.x, temp.y, temp.z].code != 15*/;
		}
		
		public bool isFrameEnabled(XYZ p)
		{
            ConvertToInfinity(p);
            //return !IsInFrame(p) || Map[p.x,p.y,p.z] != 0;
           // return /*!IsInFrame(p) ||*/ Map[p.x,p.y,p.z].code != 0;
            return /*!IsInFrame(p) ||*/ Map[p.x,p.y,p.z].touchable;
		}
		public bool IsInFrame(int x, int y, int z)
		{
			return (x >= 0 && y >= 0 && z >= 0 && 
					x <frameSize.x && y < frameSize.y && z < frameSize.z);
		}
		public bool IsInFrame(XYZ p)
		{
			return (p.x >= 0 && p.y >= 0 && p.z >= 0 && 
					p.x <frameSize.x && p.y < frameSize.y && p.z < frameSize.z);
		}
		public void SetFrame(XYZ i, bool b)
		{
            ConvertToInfinity(i);
            //Map[i.x, i.y, i.z].code = b ? (byte)1 : (byte)0;
            Map[i.x, i.y, i.z].touchable = b;
		}
		//public byte GetColor(int x, int y, int z){return Map[x,y,z];}
		//public byte GetColor(XYZ p){return Map[p.x,p.y,p.z];}
		//public void SetColor(int x, int y, int z, byte b){if(IsInFrame(x,y,z)) Map[x,y,z] = b;}
		//public void SetColor(XYZ p, byte b){SetColor(p.x,p.y,p.z,b);}
		
		//public void AddColor(XYZ p, byte b){ AddColor(p.x,p.y,p.z,b);}
		//public void AddColor(int x, int y, int z, byte b)
		//{
		//	if(IsInFrame(x,y,z))
		//	{
		//		Map[x,y,z] = Map[x,y,z] + b > 13 ? (byte)13 :
		//								  Map[x,y,z] + b < 0 ? (byte)0 : (byte)(Map[x,y,z] + b);
		//	}
		//}


        public Block GetBlock(int x, int y, int z) { return Map[x, y, z]; }
        public Block GetBlock(XYZ p) { return Map[p.x, p.y, p.z]; }

        public void SetBlock(XYZ i, bool b) {SetBlock(i.x, i.y, i.z, b);}
        public void SetBlock(int x, int y, int z, bool b)
        {
            if (IsInFrame(x, y, z))
            {
                //Map[x, y, z].code = b;
                Map[x, y, z].touchable = b;
            }
        }

        public XYZ_b GetColor(int x, int y, int z) { return Map[x, y, z].color; }
        public XYZ_b GetColor(XYZ p) { return Map[p.x, p.y, p.z].color; }

        public void SetColor(int x, int y, int z, XYZ_b c) { if (IsInFrame(x, y, z)) Map[x, y, z].color.Set(c); }
        public void SetColor(XYZ p, XYZ_b c) { SetColor(p.x, p.y, p.z, c); }
        public void SetColor(XYZ p, byte r, byte g, byte b) { if (IsInFrame(p)) Map[p.x, p.y, p.z].color.Set(r,g,b); }

        public void AddColor(XYZ p, XYZ_b c) { AddColor(p.x, p.y, p.z, c); }
        public void AddColor(int x, int y, int z, XYZ_b c)
        {
            if (IsInFrame(x, y, z))
            {
                Map[x, y, z].color.Add(c);
            }
        }
		public void SetRender(XYZ p, Func<XYZ_d, XYZ, XYZ, int, bool> rend)
		{
			if (IsInFrame(p)) Map[p.x, p.y, p.z].OnRendered = rend; 
		}
        public void ConvertToFramePos(XYZ index,XYZ_d p)
		{
			int x = index.x * frameLength + frameLength/2;
			int y = index.y * frameLength + frameLength/2;
			int z = index.z * frameLength + frameLength/2;
			p.Sub(x,y,z);
		}

		public void DrawMap(Texture2D img)
		{
			int heightInPixels = worldSize.y > img.height ? img.height : worldSize.y;
			int widthInPixels = worldSize.x > img.width ? img.width : worldSize.x;

			XYZ_b[,] byteImg = new XYZ_b[img.width, img.height];
			for(int i = 0; i < byteImg.GetLength(0); i++)
			{
				for (int j = 0; j < byteImg.GetLength(1); j++)
				{
					Color color = img.GetPixel(i,j);
					byteImg[i, j] = new XYZ_b((byte)(color.r * 255), (byte)(color.g * 255), (byte)(color.b * 255));
				}
			}
			Parallel.For(0, heightInPixels, (int y) =>
				//for (int y = 0; y < widthInPixels; y++)
				Parallel.For(0, widthInPixels, (int x) =>
				//for (int x = 0; x < heightInPixels; x++)
				{
					int z0 = frameSize.z / 2;
					Map[x, y, z0].touchable = true;
					Map[x, y, z0].OnRendered = renderer_block;
					Map[x, y, z0].color.Set(byteImg[x,y]);

					/*if(!(Math.Abs(Map[x,y,z].color.x - 35) < 10 && Math.Abs(Map[x, y, z].color.y - 31) < 10 && Math.Abs(Map[x, y, z].color.z - 32) < 10))
					{
						Map[x, y, z].code = 15;
					}*/
					if (!(Math.Abs(Map[x, y, z0].color.x - 5) < 20 && Math.Abs(Map[x, y, z0].color.y - 30) < 20 && Math.Abs(Map[x, y, z0].color.z - 40) < 20))
					{
						Map[x, y, z0].OnRendered = renderer_air;
						//Map[x, y, z].code = 15;
					}
					for (int z = z0; z < frameSize.z; z++)
					{
						Map[x, y, z].touchable = Map[x, y, z0].touchable;
						Map[x, y, z].OnRendered = Map[x, y, z0].OnRendered;
						Map[x, y, z].color.Set(Map[x, y, z0].color);
					}
					/* if ((int)currentLine[x * bytesPerPixel] +
										(int)currentLine[x * bytesPerPixel + 1] +
										 (int)currentLine[x * bytesPerPixel + 2] < 150)
					 {
						 Map[x, y, z].color.Div(2);
						 Map[x, y, z].color.x = 70;
						 //if (z - frameSize.z / 2 < 3) Map[x, y, z].code = 0;
						 //else
							 Map[x, y, z].code = 15;
					 }*/

				})
			);
		}
		
		public void DrawBuilding(Texture2D img, XYZ startPos)
        {
			for (int x = 0; x < frameSize.x; x++)
				for (int y = 0; y < frameSize.z; y++)
				{
					if (img.GetPixel(x,y).b == 200)
					{
						Map[x, y, startPos.z].touchable = true;
						Map[x, y, startPos.z].OnRendered = renderer_block; ;
						Map[x, y, startPos.z].color.Set(100, 100, 100);

						Map[x, y, startPos.z - 10].touchable = true;
						Map[x, y, startPos.z - 10].OnRendered = renderer_block;
						Map[x, y, startPos.z - 10].color.Set(100, 100, 100);
					}
				}

			for (int x = 0; x < frameSize.x; x++)
				for (int y = 0; y < frameSize.z; y++)
				{
					int z0 = startPos.z - 1;
					if (img.GetPixel(x, y).b < 10)
					{
						Map[x, y, z0].touchable = true;
						Map[x, y, z0].OnRendered = renderer_block;
						Map[x, y, z0].color.Set(100, 100, 100);
						for (int z = z0 - 1; z >= startPos.z - 9; z--)
						{
							Map[x, y, z].touchable = true;
							Map[x, y, z].OnRendered = renderer_block;
							Map[x, y, z].color.Set(100, 100, 200);
						}
					}
				}
        }
        public void DrawCloud(Texture2D img, int height)
        {
			for (int x = 0; x < frameSize.x; x++)
				for (int y = 0; y < frameSize.z; y++)
				{
					if (img.GetPixel(x, y).b > 0)
					{
						//Map[x, y, height].code = (byte)15;
						Map[x, y, height].touchable = true;
						// Map[x, y, height].renderer = renderer_air;
						Map[x, y, height].color.Set(150);
					}
				}
        }
        public void Init(XYZ worldSize)
        {
            this.worldSize = worldSize;
            frameSize.Set(worldSize);
            realSize.Set(worldSize).Mul(frameLength);
            //Map = new byte[frameSize.x,frameSize.y,frameSize.z];
            Map = new Block[frameSize.x, frameSize.y, frameSize.z];
			for (int i = 0; i < frameSize.x; i++)
                for (int j = 0; j < frameSize.y; j++)
                    for (int k = 0; k < frameSize.z; k++)
                    {
                        Map[i, j, k] = new Block(false, new XYZ_b(),renderer_air);
                    }
            DrawMap(mapTexture);
           // DrawCloud(cloudTexture, frameSize.z / 3);
			
			//DrawBuilding(buildingTexture, new XYZ(280,280,100));
            //Random rand = new Random();
            //for (int i = 0; i < frameSize.x; i++)
            //    for (int j = 0; j < frameSize.y; j++)
            //        for (int k = frameSize.z / 2; k < frameSize.z; k++)
            //        {
            //            //Map[i,j,k] = (byte)((i * j * k / 11) % 6 + 1);
            //            //Map[i,j,k].color.Set
            //            //            ((byte)128,
            //            //           (byte)128,(byte)rand.Next(100, 255));
            //            // Map[i, j, k].color.Set((byte)(Math.Sin(i) * 200 + 100), (byte)(Math.Sin(j) *200 + 100), (byte)(Math.Sin(k) * 200 + 100));
            //            // Map[i, j, k].color.Add((byte)(Math.Cos(i) * 200 + 100), (byte)(Math.Cos(j) *200 + 100), (byte)(Math.Cos(k) * 200 + 100));
            //            //Map[i,j,k].color.Set((byte)((i * j * k) % 256), (byte)((i * j * k) % 256), 255);


            //            Map[i, j, k].code = 1;
            //            //Map[i, j, k].color.Set((byte)rand.Next(100, 255));
            //            Map[i, j, k].color.Set(0);
            //        }

        }
        public void MakeSphere(XYZ pos, int radius, byte code, XYZ_b color)
        {
			Func<XYZ_d, XYZ, XYZ, int, bool> renderer = (XYZ_d delta, XYZ deltaSign, XYZ frameIndex, int nextDir)=>{return false;};
            
            XYZ temp = new XYZ();
            double distance = 0;
            for (int i = -radius; i < radius; i++)
                for (int j = -radius; j < radius; j++)
                    for (int k = -radius; k < radius; k++)
                    {
                        temp.Set(pos).Add(i, j, k);
                        distance = pos.Distance(temp);
                        if (distance < radius && distance > radius - 1)
                        {
                            if(IsInFrame(temp))
                            {
                               // Map[temp.x, temp.y, temp.z].code = code;
                                Map[temp.x, temp.y, temp.z].color.Set(color);
                                Map[temp.x, temp.y, temp.z].OnRendered = renderer_block;
                            }
                        }

                    }
        }
		public void MakeMirror(XYZ pos)
		{
			XYZ temp = new XYZ(pos);
			Block block;
 			for (int w = -1; w < 21; w++)
            {
                for (int h = -1; h < 11; h++)
                {
					temp.Set(pos.x + w, pos.y - 5, frameSize.z / 2 - h).Rem(worldSize);
					//Map[pos.x + w, pos.y-5, frameSize.z / 2 - h].code = (byte)1;
					block = GetBlock(temp);
					block.touchable = true;
					block.OnRendered = renderer_block;
					block.color.Set(100, 100, 255);
					//Map[pos.x + w, pos.y-5, frameSize.z / 2 - h].touchable = true;
                    //Map[pos.x + w, pos.y-5, frameSize.z / 2 - h].OnRendered = renderer_block;
                    //Map[pos.x + w, pos.y - 5, frameSize.z / 2 - h].color.Set(100,100,255);
                }
            }
            for (int w = -1; w < 21; w++)
            {
                for (int h = -1; h < 11; h++)
                {
					temp.Set(pos.x + w, pos.y + 5, frameSize.z / 2 - h).Rem(worldSize);
					block = GetBlock(temp);
					block.touchable = true;
					block.OnRendered = renderer_block;
					block.color.Set(255, 100, 100);
					//Map[pos.x + w, pos.y + 5, frameSize.z / 2 - h].code = (byte)1;
					//Map[pos.x + w, pos.y + 5, frameSize.z / 2 - h].touchable = true;
					//Map[pos.x + w, pos.y + 5, frameSize.z / 2 - h].OnRendered = renderer_block;
					//Map[pos.x + w, pos.y + 5, frameSize.z / 2 - h].color.Set(255,100,100);
				}
            }

            for (int w = 0; w < 20; w++)
            {
                for (int h = 0; h < 10; h++)
                {
					temp.Set(pos.x + w, pos.y - 5, frameSize.z / 2 - h).Rem(worldSize);
					block = GetBlock(temp);
					block.touchable = true;
					block.OnRendered = renderer_mirror;
					block.color.Set(0, 0, 0);
					//Map[pos.x + w, pos.y - 5, frameSize.z / 2 - h].code = (byte)14;
					//Map[pos.x + w, pos.y - 5, frameSize.z / 2 - h].touchable = true;
					// Map[pos.x + w, pos.y - 5, frameSize.z / 2 - h].OnRendered = renderer_mirror;
					// Map[pos.x + w, pos.y - 5, frameSize.z / 2 - h].color.Set(0, 0, 0);
				}
            }
            for (int w = 0; w < 20; w++)
            {
                for (int h = 0; h < 10; h++)
                {
					temp.Set(pos.x + w, pos.y + 5, frameSize.z / 2 - h).Rem(worldSize);
					block = GetBlock(temp);
					block.touchable = true;
					block.OnRendered = renderer_mirror;
					block.color.Set(0, 0, 0);
					//Map[pos.x + w, pos.y + 5, frameSize.z / 2 - h].code = (byte)14;
					//Map[pos.x + w, pos.y + 5, frameSize.z / 2 - h].touchable = true;
					// Map[pos.x + w, pos.y + 5, frameSize.z / 2 - h].OnRendered = renderer_mirror;
					// Map[pos.x + w, pos.y + 5, frameSize.z / 2 - h].color.Set(0, 0, 0);
				}
            }
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    for (int k = 0; k < 4; k++)
                    {
						temp.Set(pos.x + 10 - i, pos.y - j, frameSize.z / 2 - k).Rem(worldSize);
						block = GetBlock(temp);
						block.touchable = true;
						block.OnRendered = renderer_block;
						block.color.Set(80,80,80);
						// Map[pos.x + 10 - i, pos.y - j, frameSize.z / 2 - k].code = (byte)1;
						//Map[pos.x + 10 - i, pos.y - j, frameSize.z / 2 - k].touchable = true;
						//Map[pos.x + 10 - i, pos.y - j, frameSize.z / 2 - k].OnRendered = renderer_block;
						//Map[pos.x + 10 - i, pos.y - j, frameSize.z / 2 - k].color.Set((byte)0);
					}
        }
		
		public void MakeCone(XYZ_d pos, int radius, int height)
		{
			XYZ frameIndex = new XYZ();
			XYZ_d temp = new XYZ_d();
			XYZ_d temp2 = new XYZ_d();
			XYZ_d temp3 = new XYZ_d();
            double distance = 0;
			pos.z += height;
			XYZ_b color = new XYZ_b(1,1,1);
			XYZ_b color2 = new XYZ_b(color).Mul(30);
			
			double degreeX = 30 * -Math.PI / 180d;
			double degreeY = 0 * Math.PI / 180d;
			double sinX = Math.Sin(degreeX);
			double sinY = Math.Sin(degreeY);
			double cosX = Math.Cos(degreeX);
			double cosY = Math.Cos(degreeY);
			XYZ_d basisX = new XYZ_d(cosY, -sinY, 0);
			XYZ_d basisY = new XYZ_d(cosX * sinY, cosX * cosY, -sinX);
			XYZ_d basisZ = new XYZ_d(sinX * sinY, sinX * cosY, cosX);
			
			for (int i = -radius -1; i < radius + 1; i++)
                for (int j = -radius - 1; j < radius + 1; j++)
					for (int k = -height - 1; k <  - height/2 + 1; k++)
					{
						temp.Set(pos).Add(i, j, k);
						frameIndex = temp.ToXYZ();
						ConvertToInfinity(frameIndex);
						SetBlock(frameIndex,true);
						 SetColor(frameIndex,0,0,0);
						 SetRender(frameIndex,renderer_block);
						//SetColor(frameIndex,1,0,0);
					}
					
			 for (int i = -radius; i < radius; i++)
                for (int j = -radius; j < radius; j++)
					for (int k = -height; k <  - height/2 ; k++)
					{
						temp.Set(pos).Add(i, j, k);
						frameIndex = temp.ToXYZ();
						ConvertToInfinity(frameIndex);
						SetBlock(frameIndex,false);
						SetRender(frameIndex,renderer_air);
						//SetColor(frameIndex,1,0,0);
					}
			//frameIndex = pos.ToXYZ();
			//ConvertToInfinity(frameIndex);
			//frameIndex.z -= height - 10;
			
			 temp.Set(pos).Sub(0,10,height-10);
			for(int i = -2; i <= 2; i++)
			{
				for(int j = -2; j <= 2; j++)
				{
					for(int k = -2; k <= 2; k++)
					{
						temp2.Set(temp).Add(i,j,k);
						frameIndex = temp2.ToXYZ();
						ConvertToInfinity(frameIndex);
						SetBlock(frameIndex,true);
						SetRender(frameIndex,renderer_block);
					}
				}
			} 
			
            for (int i = -radius; i < radius; i++)
                for (int j = -radius; j < radius; j++)
				{
					
					temp.Set(pos).Add(i, j, 0);
					distance = pos.Distance(temp);
					if (distance < radius)
					{
						frameIndex = temp.ToXYZ();
						ConvertToInfinity(frameIndex);
						//SetColor(frameIndex,color);
						
						temp2.Set(temp).Sub(pos).Add(0,0,height).Div(height);
						temp2.Set(basisX.x * temp2.x + basisY.x * temp2.y + basisZ.x * temp2.z,
								  basisX.y * temp2.x + basisY.y * temp2.y + basisZ.y * temp2.z,
								  basisX.z * temp2.x + basisY.z * temp2.y + basisZ.z * temp2.z);
						temp2.Div(temp2.Length());
						//temp2.Set(temp3);
						temp.Set(pos).Add(0,0,-height);
						
						/*******************************************************************************/
						 
						/*  frameIndex = temp.ToXYZ();
						XYZ deltaSign = new XYZ(Math.Sign(temp2.x),Math.Sign(temp2.y),Math.Sign(temp2.z));
						 int nextDir = 0;
						 XYZ_d maxNumOfDelta = new XYZ_d(frameLength).Mul(deltaSign).Div(temp2); 
						 XYZ_d target = new XYZ_d(halfFrameLength);					
						 target.Mul(deltaSign);//delta벡터 방향으로 이동시 접촉가능한 경계면들 구하기.
						 target.Div(temp2);
						  */
						//XYZ gap = new XYZ(frameIndex);
						
						//color.Set(2,2,2);
						//byte code = 0;
						for (int k = 0; k < height * 1.5f; k++)
						{
							temp.Add(temp2);
							frameIndex = temp.ToXYZ();
							/* if (target.x < target.y)
								if(target.x < target.z) nextDir = 0;
								else nextDir = 2;
							else
								if(target.y < target.z) nextDir = 1;
								else nextDir = 2; */
							
							//target.element[nextDir] += maxNumOfDelta.element[nextDir];
							//frameIndex.element[nextDir] += deltaSign.element[nextDir];
							ConvertToInfinity(frameIndex);
							if(isFrameEnabled(frameIndex)) 
							{
								AddColor(frameIndex,color2);
								break;
							}
							else AddColor(frameIndex,color);
							//SetBlock(frameIndex,code);
							
							//gap.Set(frameIndex);
							
							//Console.WriteLine(nextDir);
						}
					}
				}
                    
		}
		
		
		public void MakePenetration(XYZ pos)
		{
			for (int w = -1; w < 21; w++)
            {
                for (int h = -1; h < 11; h++)
                {
					for(int l = -10; l < 10; l++)
					{
						Map[pos.x + w, pos.y + l, frameSize.z / 2 - h].touchable = true;
						Map[pos.x + w, pos.y + l, frameSize.z / 2 - h].OnRendered = renderer_block;
						Map[pos.x + w, pos.y + l, frameSize.z / 2 - h].color.Set(100,100,255);
					}

                }
            }
			for (int w = 0; w < 20; w++)
            {
                for (int h = 0; h < 10; h++)
                {
					for(int l = -10; l < 10; l++)
					{
						Map[pos.x + w, pos.y + l, frameSize.z / 2 - h].touchable = false;
						Map[pos.x + w, pos.y + l, frameSize.z / 2 - h].OnRendered = renderer_air;
						Map[pos.x + w, pos.y + l, frameSize.z / 2 - h].color.Set(0,0,0);
					}

                }
            }
			for (int w = 0; w < 20; w++)
            {
                for (int h = 0; h < 10; h++)
                {
                    //Map[pos.x + w, pos.y-5, frameSize.z / 2 - h].code = (byte)1;
                    Map[pos.x + w, pos.y, frameSize.z / 2 - h].touchable = true;
                    Map[pos.x + w, pos.y, frameSize.z / 2 - h].OnRendered = renderer_penetration;
                    Map[pos.x + w, pos.y, frameSize.z / 2 - h].color.Set(100,100,100);
                }
            }
		}
    }
}