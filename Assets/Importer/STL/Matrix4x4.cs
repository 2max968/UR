using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STLEdit
{
    public class Matrix4x4
    {
        float[] components;

        public float this[int i]
        {
            get
            {
                return components[i];
            }
            set
            {
                components[i] = value;
            }
        }

        public float this[int row, int column]
        {
            get
            {
                return components[index(row, column)];
            }
            set
            {
                components[index(row, column)] = value;
            }
        }

        public Matrix4x4()
        {
            components = new float[4 * 4];
            for(int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 4; j++)
                {
                    this[i, j] = i == j ? 1 : 0;
                }
            }
        }

        public Matrix4x4(float m11, float m12, float m13, 
            float m21, float m22, float m23, 
            float m31, float m32, float m33, 
            float m41, float m42, float m43)
        {
            components = new float[]
            {
                m11, m12, m13, 0,
                m21, m22, m23, 0,
                m31, m32, m33, 0,
                m41, m42, m43, 1
            };
        }

        int index(int row, int column)
        {
            return row * 4 + column;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < 4; i++)
            {
                sb.Append("|");
                for(int j = 0; j < 4; j++)
                {
                    sb.AppendFormat("{0,10}", this[i, j]);
                }
                sb.AppendLine("|");
            }
            return sb.ToString();
        }

        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
        {
            Matrix4x4 mat = new Matrix4x4();
            for(int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    float sum = 0;
                    for(int i = 0; i < 4; i++)
                    {
                        sum += a[r, i] * b[i, c];
                    }
                    mat[r, c] = sum;
                }
            }
            return mat;
        }

        public static Vertex operator *(Matrix4x4 m, Vertex v)
        {
            return v * m;
        }

        public static Vertex operator *(Vertex v, Matrix4x4 m)
        {
            Vertex sol = new Vertex();
            float[] vec = new float[] { v.X, v.Y, v.Z, 1 };
            for(int i = 0; i < 3; i++)
            {
                float sum = 0;
                for(int j = 0; j < 4; j++)
                {
                    sum += vec[j] * m[j, i];
                }
                sol[i] = sum;
            }
            return sol;
        }

        public static Matrix4x4 GetTranslationMatrix(float x, float y, float z)
        {
            Matrix4x4 mat = new Matrix4x4();
            mat[3, 0] = x;
            mat[3, 1] = y;
            mat[3, 2] = z;
            return mat;
        }

        public Matrix4x4 AtPoint(float x, float y, float z)
        {
            return GetTranslationMatrix(-x, -y, -z) * this * GetTranslationMatrix(x, y, z);
        }

        public static Matrix4x4 GetScaleMatrix(float x, float y, float z)
        {
            Matrix4x4 mat = new Matrix4x4();
            mat[0, 0] = x;
            mat[1, 1] = y;
            mat[2, 2] = z;
            return mat;
        }

        public static Matrix4x4 GetRotationX(float angle)
        {
            float s = (float)Math.Sin(angle);
            float c = (float)Math.Cos(angle);

            return new Matrix4x4(1, 0, 0, 0, s, c, 0, c, -s, 0, 0, 0);
        }

        public static Matrix4x4 GetRotationY(float angle)
        {
            float s = (float)Math.Sin(angle);
            float c = (float)Math.Cos(angle);

            return new Matrix4x4(c, 0, -s, 0, 1, 0, s, 0, c, 0, 0, 0);
        }

        public static Matrix4x4 GetRotationZ(float angle)
        {
            float s = (float)Math.Sin(angle);
            float c = (float)Math.Cos(angle);

            return new Matrix4x4(c, -s, 0, s, c, 0, 0, 0, 1, 0, 0, 0);
        }
    }
}
