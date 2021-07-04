using System;

namespace VirtualCam
{    
	class Block
    {
        public XYZ_b color;
        public bool touchable;
		public int lightLevel = 1;
		
		public Func<XYZ_d, XYZ, XYZ, int, bool> OnRendered;
        public Block(bool t, XYZ_b c, Func<XYZ_d, XYZ, XYZ, int, bool> renderer) 
		{
			touchable = t; color = c; OnRendered = renderer;
		}
    }
}