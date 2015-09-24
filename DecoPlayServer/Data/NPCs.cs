using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DecoPlayServer
{
    public class Mob
    {
        public uint ID = 0;
        public ushort Type = 0;
        public float Angle = 0;
        public int RangeIndex = 0;
        public Point Pos = new Point( );

        public Mob(ushort Type, Point Pos, float Angle, int RangeIndex)
        {
            this.Type = Type;
            this.Pos = Pos;
            this.Angle = Angle;
            this.RangeIndex = RangeIndex;
        }
    }

    public class NPC
    {
        public uint ID = 0;
        public ushort Type = 0;
        public float Angle = 0;
        public Point Pos = new Point( );

        public NPC(ushort Type, Point Pos, float Angle)
        {
            this.Type = Type;
            this.Pos = Pos;
            this.Angle = Angle;
        }
    }

    class Mobs
    {
        static Random Ran = new Random( );
        public static Thread SpawnThread = new Thread(new ThreadStart(SpawnFunc));

        public static void SpawnFunc( )
        {
            for (int i = 0; i < Maps.MapsData.Count; i++)
            {
                int b = 0;
                foreach (MobRange x in Maps.MapsData[i].MobRanges)
                {
                    for (int a = x.CurMobCount; a < x.MaxMobCount; a++)
                    {
                        Mob New = new Mob(x.Mobs[Ran.Next(0, x.Mobs.Count - 1)], MathCls.RPointInPolygon(x.RangePolygon),Ran.Next(0,360), b);
                        New.ID = Maps.MapsData[i].NextID;
                        Maps.MapsData[i].NewMob(New);
                        x.CurMobCount++;
                    }

                    if(x.HeadMob != 0)
                    {
                        Mob New = new Mob(x.HeadMob, MathCls.RPointInPolygon(x.RangePolygon), Ran.Next(0, 360), b);
                        New.ID = Maps.MapsData[i].NextID;
                        Maps.MapsData[i].NewMob(New);
                        x.CurMobCount++;
                    }
                    b++;
                }
            }
            Thread.Sleep(10000);
        }
    }

    class NPCs
    {
        public static NPC IsNPC(Player player, Point Pos)
        {
            int PlayerMapIndex = Maps.MapsData.Find(player.CharData.Map);
            if (PlayerMapIndex == -1)
                return null;
            Map PlayerMap = Maps.MapsData[PlayerMapIndex];
            foreach (NPC x in PlayerMap.NPCs)
            {
                if (x.Pos == Pos)
                    return x;
            }
            return null;
        }
    }
}
