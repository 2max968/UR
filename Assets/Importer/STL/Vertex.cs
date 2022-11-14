using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STLEdit
{
    public struct Vertex
    {
        public float X, Y, Z;

        public float this[int ind]
        {
            get
            {
                switch(ind)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (ind)
                {
                    case 0:
                        X = value;
                        return;
                    case 1:
                        Y = value;
                        return;
                    case 2:
                        Z = value;
                        return;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        public float Length
        {
            get
            {
                return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
            }
        }

        public Vertex(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vertex operator +(Vertex a, Vertex b)
        {
            return new Vertex(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vertex operator -(Vertex a, Vertex b)
        {
            return new Vertex(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vertex operator *(Vertex a, Vertex b)
        {
            return new Vertex(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vertex operator *(Vertex a, float b)
        {
            return new Vertex(a.X * b, a.Y * b, a.Z * b);
        }

        public static Vertex Cross(Vertex a, Vertex b)
        {
            float x = (a.Y * b.Z) - (a.Z * b.Y);
            float y = (a.Z * b.X) - (a.X * b.Z);
            float z = (a.X * b.Y) - (a.Y * b.X);
            return new Vertex(x, y, z);
        }

        public Vertex GetNormalized()
        {
            float l = Length;
            return new Vertex(X / l, Y / l, Z / l);
        }

        public string ToSTLVertex()
        {
            string _x = X.ToString("e", CultureInfo.InvariantCulture);
            string _y = Y.ToString("e", CultureInfo.InvariantCulture);
            string _z = Z.ToString("e", CultureInfo.InvariantCulture);
            return $"{_x} {_y} {_z}";
        }

        public string ToObjVertex()
        {
            string _x = X.ToString("0.000000", CultureInfo.InvariantCulture);
            string _y = Y.ToString("0.000000", CultureInfo.InvariantCulture);
            string _z = Z.ToString("0.000000", CultureInfo.InvariantCulture);
            return $"{_x} {_y} {_z}";
        }

        public static bool FromString(string text, out Vertex vertex)
        {
            string[] components = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if(components.Length >= 3)
            {
                if(float.TryParse(components[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float x)
                    && float.TryParse(components[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float y)
                    && float.TryParse(components[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float z))
                {
                    vertex = new Vertex(x, y, z);
                    return true;
                }
            }
            vertex = new Vertex();
            return false;
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        public Vector2 XY { get => new Vector2(X, Y); }
        public Vector2 YZ { get => new Vector2(Y, Z); }
        public Vector2 XZ { get => new Vector2(X, Z); }

        public static bool operator ==(Vertex a, Vertex b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(Vertex a, Vertex b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vertex)
                return this == (Vertex)obj;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return (X.GetHashCode() << 2) ^ (Y.GetHashCode() << 1) ^ Z.GetHashCode();
        }
    }
}
