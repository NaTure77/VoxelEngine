using System;
using System.Threading;
using System.Threading.Tasks;

namespace VirtualCam
{
	class XY<T>
	{
		public T x, y;
		public XY(T x, T y)
		{
			this.x = x; this.y = y;
		}
	}
	class XYZ
	{
		public int x{get{return element[0];} set{element[0] = value;}}
		public int y{get{return element[1];} set{element[1] = value;}}
		public int z{get{return element[2];} set{element[2] = value;}}
		public int[] element = {0,0,0};
		public XYZ(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
		public XYZ(XYZ d) : this(d.x,d.y,d.z){}
		public XYZ(int d) : this(d,d,d){}
		public XYZ() : this(0){}
		
		public XYZ Add(int x, int y, int z){this.x += x; this.y += y; this.z += z; return this;}
		public XYZ Add(XYZ d){return Add(d.x,d.y,d.z);}
		public XYZ Add(int d){return Add(d,d,d);}
		
		public XYZ Sub(int x, int y, int z){this.x -= x; this.y -= y; this.z -= z; return this;}
		public XYZ Sub(XYZ d){return Sub(d.x,d.y,d.z);}
		public XYZ Sub(int d){return Sub(d,d,d);}
		
		public XYZ Mul(int x, int y, int z){this.x *= x; this.y *= y; this.z *= z; return this;}
		public XYZ Mul(XYZ d){return Mul(d.x,d.y,d.z);}
		public XYZ Mul(int d){return Mul(d,d,d);}
		
		public XYZ Div(int x, int y, int z){this.x /= x; this.y /= y; this.z /= z; return this;}
		public XYZ Div(XYZ d){return Div(d.x,d.y,d.z);}
		public XYZ Div(int d){return Div(d,d,d);}

        public XYZ Rem(int x, int y, int z) { this.x %= x; this.y %= y; this.z %= z; return this; }
        public XYZ Rem(XYZ d) { return Rem(d.x, d.y, d.z); }

        public XYZ Set(int d){return Set(d,d,d);}
		public XYZ Set(XYZ d){return Set(d.x,d.y,d.z);}
		public XYZ Set(int x, int y ,int z){this.x = x; this.y = y; this.z = z; return this;}
		
		public double Length(){return Math.Sqrt(Math.Pow(x,2) + Math.Pow(y,2) + Math.Pow(z,2));}
		public double Distance(XYZ d){return Math.Sqrt(Math.Pow(x - d.x,2) + Math.Pow(y - d.y,2) + Math.Pow(z - d.z,2));}
		
		public bool Equal(XYZ p){return p.x == x && p.y == y && p.z == z;}
		public bool Equal(int x, int y, int z){return this.x == x && this.y == y && this.z == z;}
		
	}
	class XYZ_d
	{
		//public static XYZ_d ZERO = new XYZ_d(0,0,0);
		public double x{get{return element[0];} set{element[0] = value;}}
		public double y{get{return element[1];} set{element[1] = value;}}
		public double z{get{return element[2];} set{element[2] = value;}}
		public double[] element = {0,0,0};
		//public int iX{get{return (int)Math.Round(x,0);}}
		//public int iY{get{return (int)Math.Round(y,0);}}
		//public int iZ{get{return (int)Math.Round(z,0);}}

        public XYZ ToXYZ() 
        { 
            return new XYZ(
                (int)Math.Round(x, 0),
                (int)Math.Round(y, 0),
                (int)Math.Round(z, 0)); 
        }
		public XYZ_d(double x, double y, double z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
		public XYZ_d(XYZ_d d) : this(d.x,d.y,d.z){}
		public XYZ_d(XYZ d) : this(d.x,d.y,d.z){}
		public XYZ_d(double d) : this(d,d,d){}
		public XYZ_d() : this(0){}
		
		public XYZ_d Add(double x, double y, double z){this.x += x; this.y += y; this.z += z; return this;}
		public XYZ_d Add(XYZ_d d){return Add(d.x,d.y,d.z);}
		public XYZ_d Add(XYZ d){return Add(d.x,d.y,d.z);}
		public XYZ_d Add(double d){return Add(d,d,d);}
		
		public XYZ_d Sub(double x, double y, double z){this.x -= x; this.y -= y; this.z -= z; return this;}
		public XYZ_d Sub(XYZ_d d){return Sub(d.x,d.y,d.z);}
		public XYZ_d Sub(XYZ d){return Sub(d.x,d.y,d.z);}
		public XYZ_d Sub(double d){return Sub(d,d,d);}
		
		public XYZ_d Mul(double x, double y, double z){this.x *= x; this.y *= y; this.z *= z; return this;}
		public XYZ_d Mul(XYZ_d d){return Mul(d.x,d.y,d.z);}
		public XYZ_d Mul(XYZ d){return Mul(d.x,d.y,d.z);}
		public XYZ_d Mul(double d){return Mul(d,d,d);}
		
		//public XYZ_d Remain(double d){this.x %= d; this.y %= d; this.z %= d; return this;}
		public XYZ_d Div(double x, double y, double z){this.x /= x; this.y /= y; this.z /= z; return this;}
		public XYZ_d Div(XYZ_d d){return Div(d.x,d.y,d.z);}
		public XYZ_d Div(double d){return Div(d,d,d);}

        public XYZ_d Rem(int x, int y, int z) { this.x %= x; this.y %= y; this.z %= z; return this; }
        public XYZ_d Rem(double x, double y, double z) { this.x %= x; this.y %= y; this.z %= z; return this; }
        public XYZ_d Rem(XYZ d) { return Rem(d.x, d.y, d.z); }
        public XYZ_d Rem(XYZ_d d) { return Rem(d.x, d.y, d.z); }

        public XYZ_d Set(double d){return Set(d,d,d);}
		public XYZ_d Set(XYZ_d d){return Set(d.x,d.y,d.z);}
		public XYZ_d Set(XYZ d){return Set(d.x,d.y,d.z);}
		public XYZ_d Set(double x, double y ,double z){this.x = x; this.y = y; this.z = z; return this;}
		
		public double Distance(XYZ_d d){return Math.Sqrt(Math.Pow(x - d.x,2) + Math.Pow(y - d.y,2) + Math.Pow(z - d.z,2));}
		public double Distance(XYZ d){return Math.Sqrt(Math.Pow(x - d.x,2) + Math.Pow(y - d.y,2) + Math.Pow(z - d.z,2));}
		public double Length(){return Math.Sqrt(Math.Pow(x,2) + Math.Pow(y,2) + Math.Pow(z,2));}
	}
    class XYZ_b
    {
        public byte x, y, z;
        public XYZ_b(byte x, byte y, byte z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public XYZ_b(XYZ_b d) : this(d.x, d.y, d.z) { }
        public XYZ_b(byte d) : this(d, d, d) { }
        public XYZ_b() : this(0) { }

        public XYZ_b Add(byte x, byte y, byte z)
        {
            if (this.x + x >= 256) this.x = 255;
            else this.x += x;
            if (this.y + y >= 256) this.y = 255;
            else this.y += y;
            if (this.z + z >= 256) this.z = 255;
             else this.z += z; 
            return this; 
        }
        public XYZ_b Add(XYZ_b d) { return Add(d.x, d.y, d.z); }
        public XYZ_b Add(byte d) { return Add(d, d, d); }

        public XYZ_b Sub(byte x, byte y, byte z) { this.x -= x; this.y -= y; this.z -= z; return this; }
        public XYZ_b Sub(XYZ_b d) { return Sub(d.x, d.y, d.z); }
        public XYZ_b Sub(byte d) { return Sub(d, d, d); }

        public XYZ_b Mul(byte x, byte y, byte z) { this.x *= x; this.y *= y; this.z *= z; return this; }
        public XYZ_b Mul(XYZ_b d) { return Mul(d.x, d.y, d.z); }
        public XYZ_b Mul(byte d) { return Mul(d, d, d); }

        public XYZ_b Div(byte x, byte y, byte z) { this.x /= x; this.y /= y; this.z /= z; return this; }
        public XYZ_b Div(XYZ_b d) { return Div(d.x, d.y, d.z); }
        public XYZ_b Div(byte d) { return Div(d, d, d); }

        public XYZ_b Set(byte d) { return Set(d, d, d); }
        public XYZ_b Set(XYZ_b d) { return Set(d.x, d.y, d.z); }
        public XYZ_b Set(byte x, byte y, byte z) { this.x = x; this.y = y; this.z = z; return this; }

        public double Length() { return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2)); }
        public double Distance(XYZ_b d) { return Math.Sqrt(Math.Pow(x - d.x, 2) + Math.Pow(y - d.y, 2) + Math.Pow(z - d.z, 2)); }

        public bool Equal(XYZ_b p) { return p.x == x && p.y == y && p.z == z; }

    }
}