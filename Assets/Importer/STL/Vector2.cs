using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STLEdit
{
    public struct Vector2
    {
        public float X, Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public string ToObjVector()
        {
            string _x = X.ToString("0.000000", CultureInfo.InvariantCulture);
            string _y = Y.ToString("0.000000", CultureInfo.InvariantCulture);
            return $"{_x} {_y}";
        }

        public static Vector2 operator*(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X * b.X, a.Y * b.Y);
        }
    }
}
