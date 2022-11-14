using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STLEdit
{
    public struct Face
    {
        public Vertex A, B, C;
        public Vertex Normal;
        public bool HasNormals;
        public Vertex NormalA, NormalB, NormalC;
        public bool HasUvs;
        public Vector2 UVA, UVB, UVC;

        public Vertex this[int ind]
        {
            get
            {
                switch (ind)
                {
                    case 0: return A;
                    case 1: return B;
                    case 2: return C;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (ind)
                {
                    case 0:
                        A = value;
                        return;
                    case 1:
                        B = value;
                        return;
                    case 2:
                        C = value;
                        return;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        public Face(Vertex a, Vertex b, Vertex c)
        {
            A = a;
            B = b;
            C = c;
            Normal = Vertex.Cross(b - a, c - a).GetNormalized();
            HasNormals = false;
            HasUvs = false;
            NormalA = NormalB = NormalC = new Vertex();
            UVA = UVB = UVC = new Vector2();
        }

        public Vertex GetNormal(int ind)
        {
            switch(ind)
            {
                case 0: return NormalA;
                case 1: return NormalB;
                case 2: return NormalC;
                default: throw new IndexOutOfRangeException();
            }
        }

        public void SetNormal(int ind, Vertex value)
        {
            switch(ind)
            {
                case 0:
                    NormalA = value;
                    return;
                case 1:
                    NormalB = value;
                    return;
                case 2:
                    NormalC = value;
                    return;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public Vertex GetUnnormalizedNormal()
        {
            Vertex ab = B - A;
            Vertex ac = C - A;
            return Vertex.Cross(ab, ac);
        }
    }
}
