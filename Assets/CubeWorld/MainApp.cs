using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEngine;

namespace VirtualCam
{
	class MainApp : MonoBehaviour
	{
        void Start()
        {
            //XYZ camSize = new XYZ(800, 300, 480);
            XYZ camSize = new XYZ(480, 100, 240);
            World world = World.instance;
            
            world.Init(new XYZ(256, 256, 256));

           
            Camera camera = new Camera(camSize, new XYZ_d(33,24,124).Mul(world.frameLength), world);

            Viewer.instance.Init(camera, camSize);
            Controller.instance.Init(world, camera);

            XYZ t = new XYZ(280, 280, 147);
            world.MakeMirror(t);
            world.GetFrameIndex(new XYZ_d(100, 100, 120).Mul(world.frameLength), t);

            //world.MakeSphere(t,30,14, new XYZ_b(10));
            world.MakePenetration(t);
            t.Add(100, 100, -50);
            //world.MakeSphere(t,30,1, new XYZ_b(100));
            world.MakeCone(new XYZ_d(33,24, 128), 35, 60);

        }
	}
}