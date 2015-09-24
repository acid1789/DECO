using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecoPlayServer
{
    public class Gate
    {
        public Point GatePos = new Point( );
        public Point DstPos = new Point( );
        public ushort DstMap = 0;
        public int Conditions = 0;

        public override string ToString( )
        {
            return "Gate Pos : " + GatePos.X + ", " + GatePos.Y
                + " - Dst Pos : " + DstPos.X + ", " + DstPos.Y
                + " - Dst Map : " + DstPos;
        }
    }

    class Data
    {
        private enum MapsSize
        {
            _0 = 512,
            _1 = 512,
            _2 = 512,
            _3 = 512,
            _4 = 512,
            _5 = 256,
            _6 = 512,
            _7 = 512,
            _8 = 512,
            _9 = 512,
            _10 = 512,
            _11 = 512,
            _12 = 512,
            _13 = 512,
            _14 = 512,
            _15 = 512,
            _16 = 512,
            _17 = 512,
            _18 = 512,
            _19 = 512,
            _20 = 512,
            _21 = 512,
            _22 = 512,
            _23 = 512,
            _24 = 256,
            _25 = 256,
            _26 = 256,
            _27 = 256,
            _28 = 256,
            _29 = 256,
            _30 = 512,
            _33 = 128,
            _34 = 256,
            _35 = 256,
            _36 = 256,
            _37 = 512,
            _38 = 512,
            _39 = 128,
            _40 = 128,
            _41 = 512,
            _42 = 512,
            _43 = 512,
            _44 = 256,
            _45 = 512,
            _46 = 512,
            _47 = 512,
            _48 = 512,
            _49 = 512,
            _50 = 512,
            _51 = 512,
            _52 = 512,
            _53 = 256,
            _56 = 512,
            _57 = 512,
            _58 = 512,
            _59 = 512,
            _60 = 512,
            _61 = 512,
            _62 = 512,
            _63 = 512,
            _66 = 512,
            _67 = 512,
            _68 = 512,
            _69 = 512,
            _70 = 512,
            _71 = 512,
            _72 = 512,
            _73 = 512,
            _74 = 512,
            _75 = 512,
            _76 = 512,
            _77 = 512,
            _78 = 512,
        }

        

        public static double GetDistance(Point p, Point q)
        {
            double a = p.X - q.X;
            double b = p.Y - q.Y;
            double distance = Math.Sqrt(a * a + b * b);
            return distance;
        }

        public static Gate inGate(ushort Map, Point Pos)
        {
            int MapIndex = Maps.MapsData.Find(Map);
            if (MapIndex == -1)
                return null;
            Map Data = Maps.MapsData[MapIndex];

            foreach (Gate x in Data.Gates)
            {
                if (inGate(Pos, x))
                    return x;
            }
            return null;
        }

        public static bool inGate(Point Pos, Gate TheGate)
        {
            if (GetDistance(TheGate.GatePos, Pos) <= 10)
                return true;
            return false;
        }


        public static int GetGameCoord(int Map, Point Src)
        {
            int Space = (int)(Enum.GetValues(typeof(MapsSize)).GetValue(Enum.GetNames(typeof(MapsSize)).ToList( ).IndexOf("_" + Map)));
            return (Src.Y * Space) + Src.X;
        }

        public static Point GetReallyCoord(int Map, int Src)
        {
            int Space = (int)(Enum.GetValues(typeof(MapsSize)).GetValue(Enum.GetNames(typeof(MapsSize)).ToList( ).IndexOf("_" + Map)));
            return new Point(Src % Space, Src / Space);
        }

        public static int GetStyle(CharGender CharGender, CharNation CharNation, byte CharFace, byte CharHair)
        {
            ushort FaceID = (ushort)(10200 + ((int)CharGender * 100) + CharFace);
            ushort HairID = (ushort)(10000 + ((int)CharGender * 100) + CharHair);
            int x = (byte)~CharNation & 1;
            return
                  ((byte)~CharNation & 1)
                + ((byte)CharGender * 2)
                + (FaceID * 4)
                + (HairID * 0x20000);
        }
        public static CharNation GetCharNation(int appearance)
        {
            int nCharNation = appearance & 1;
            if (nCharNation == 0) return (CharNation)1;
            return (CharNation)0;
        }
        public static CharGender GetCharGender(int appearance)
        {
            int nCharGender = (appearance >> 1) & 1;
            return (CharGender)nCharGender;
        }
        public static byte GetCharHair(int appearance, CharNation CharNation, CharGender CharGender)
        {
            int nCharHair = ((appearance >> 17) & 0x7FFF) - (int)(10000 + ((int)CharGender * 100));
            return (byte)nCharHair;
        }

        public static byte GetCharFace(int appearance, CharNation CharNation, CharGender CharGender)
        {
            int nCharFace = ((appearance >> 2) & 0x7FFF) - (ushort)(10200 + ((int)CharGender * 100));
            return (byte)nCharFace;
        }
    }
}
