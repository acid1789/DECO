using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecoPlayServer
{
    class MathCls
    {
        static Random Ran = new Random( );

        public static Point RPointInTriangle(PointF A, PointF B, PointF C)
        {
            double a = Ran.NextDouble( );
            double b = Ran.NextDouble( );

            if (a + b > 1)
            {
                a = 1 - a;
                b = 1 - b;
            }

            double c = 1 - a - b;

            double rndX = (a * A.X) + (b * B.X) + (c * C.X);
            double rndY = (a * A.Y) + (b * B.Y) + (c * C.Y);

            return new Point((int)rndX, (int)rndY);
        }

        public static Point RPointInPolygon(Polygon RangePolygon)
        {
            List<PointF[]> Triangles = Triangulation2D.Triangulate(RangePolygon);
            List<double> Areas = new List<double>();
            foreach (PointF[] x in Triangles)
                Areas.Add(TriangleArea(x));
            int Index = Ran.Next(0, Triangles.Count - 1);
            PointF[] Triangle = RandomWProb(Triangles, Areas);
            return RPointInTriangle(Triangle[0], Triangle[1], Triangle[2]);
        }

        public static double Distance(PointF P1, PointF P2)
        {
            return Math.Sqrt(Math.Pow(P1.X - P2.X, 2) + Math.Pow(P1.Y - P2.Y, 2));
        }

        public static double TriangleArea(PointF[] Triangle)
        {
            double valueA = Distance(Triangle[0], Triangle[1]);
            double valueB = Distance(Triangle[1], Triangle[2]);
            double valueC = Distance(Triangle[2], Triangle[0]);

            double i = (valueA + valueB + valueC) / 2;
            double sss = Math.Sqrt(i * (i - valueA) * (i - valueB) * (i - valueC));
            return Math.Round(Math.Sqrt(i * (i - valueA) * (i - valueB) * (i - valueC)), 1);
        }

        public static T RandomWProb<T>(List<T> Objects, List<double> Probs)
        {
            double All = 0;
            double Temp = 0;

            foreach (double x in Probs)
                All += x;

            for (int i = 0; i < Probs.Count;i++ )
                Probs[i] = Probs[i] / All;

            double Num = Ran.NextDouble( );

            for (int i = 0; i < Probs.Count; i++)
            {
                Temp += Probs[i];
                if (Num < Temp)
                    return Objects[i];
            }

            return Objects[0];
        }
    }
}
