using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSR_visual
{
    class Const
    {
        //Visualizzazione/interfaccia
        public static int ScreenWidth = 1024;
        public static int ScreenHeigh = 700;
        public static int ControlsAreaHeigh = 200;
        public static int Zoom = 10;

        //Fisica
        public static float Gravity = 10;
        public static float FloorWidth = 500;
        public static float FloorHeigh = 1;
        public static float FloorYPosition = 40;
        public static float FloorXPosition = 0;
        public static float FloorDensity = 0.1f;
        public static float FloorRestitution = 0.2f;
        public static float FloorFriction = 0.3f;

        //Parti
        public static float MaxForcePerAreaUnit = 5.0f;
        public static float PartDensity = 0.1f;
        public static float MaxMotorTorquePerAreaUnit = 30.0f;
        public static float MaxSpeedPerLenghtUnit = 10.0f;

        public static float Epsilon = 0.07f;
    }
}
