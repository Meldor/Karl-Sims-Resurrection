using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PoleBalancing
{
    class Const
    {
        //Visualizzazione/interfaccia
        public static int ScreenWidth = 1024;
        public static int ScreenHeigh = 700;
        public static int ControlsAreaHeigh = 200;
        public static int Zoom = 20;

        //Fisica
        public static float Gravity = 10;
        public static float FloorWidth = 500;
        public static float FloorHeigh = 0.5f;
        public static float FloorYPosition = 10;

        //Parti
        public static float MaxCartForce = 4.0f;
        public static float PartDensity = 0.1f;

        public static float Epsilon = 0.07f;
    }
}
