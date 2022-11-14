using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STLEdit
{
    public static class MeshOptimizer
    {
        public static void CreateNormalsFlat(Mesh mesh)
        {
            for(int i = 0; i < mesh.Faces.Count; i++)
            {
                Face face = mesh.Faces[i];

                face.HasNormals = true;
                face.NormalA = face.Normal;
                face.NormalB = face.Normal;
                face.NormalC = face.Normal;

                mesh.Faces[i] = face;
            }
        }

        public static bool IsClose(Vertex a, Vertex b)
        {
            double delta = 1e-6;
            return Math.Abs(a.X - b.X) < delta && Math.Abs(a.Y - b.Y) < delta && Math.Abs(a.Z - b.Z) < delta;
        }

        public static void CreateNormalsSmooth(Mesh mesh)
        {
            int cGroupId = 0;
            int[] groups = new int[mesh.Faces.Count * 3];
            for(int i = 0; i < groups.Length; i++)
            {
                if (groups[i] == 0)
                {
                    cGroupId++;
                    groups[i] = cGroupId;
                    Vertex vert = mesh.Faces[i / 3][i % 3];
                    for (int j = i + 1; j < groups.Length; j++)
                    {
                        if (IsClose(vert, mesh.Faces[j / 3][j % 3]))
                        {
                            groups[j] = cGroupId;
                        }
                    }
                }
            }

            Vertex[] groupVertecies = new Vertex[cGroupId];
            for(int i = 0; i < groups.Length; i++)
            {
                groupVertecies[groups[i] - 1] += mesh.Faces[i / 3].GetUnnormalizedNormal();
            }
            /*for (int i = 0; i < groups.Length; i++)
            {
                mesh.Faces[i / 3].HasNormals = true;
                mesh.Faces[i / 3].SetNormal(i % 3, groupVertecies[groups[i] - 1].GetNormalized());
            }*/
            for(int i = 0; i < mesh.Faces.Count; i++)
            {
                Face face = new Face(mesh.Faces[i].A, mesh.Faces[i].B, mesh.Faces[i].C);
                face.HasNormals = true;
                face.NormalA = groupVertecies[groups[i * 3 + 0] - 1].GetNormalized();
                face.NormalB = groupVertecies[groups[i * 3 + 1] - 1].GetNormalized();
                face.NormalC = groupVertecies[groups[i * 3 + 2] - 1].GetNormalized();
                mesh.Faces[i] = face;
            }
        }

        static float _abs(float x)
        {
            if (x < 0) return -x;
            return x;
        }

        public static void CreateUVSimpleXYZ(Mesh mesh, bool normalize, float tilingX = 1, float tilingY = 1)
        {
            float scaleX = tilingX;
            float scaleY = tilingY;

            if (normalize)
            {
                var bounds = mesh.GetBounds();
                float bx = _abs(bounds.xmin - bounds.xmax);
                float by = _abs(bounds.ymin - bounds.ymax);
                float bz = _abs(bounds.zmin - bounds.zmax);
                float bsize = Math.Max(Math.Max(bx, by), bz);
                scaleX /= bsize;
                scaleY /= bsize;
            }

            Vector2 scale = new Vector2(scaleX, scaleY);

            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                Face face = mesh.Faces[i];

                face.HasUvs = true;
                float x = _abs(face.Normal.X);
                float y = _abs(face.Normal.Y);
                float z = _abs(face.Normal.Z);

                if(x > y && x > z)
                {
                    face.UVA = face.A.YZ * scale;
                    face.UVB = face.B.YZ * scale;
                    face.UVC = face.C.YZ * scale;
                }
                else if(y > z && y > x)
                {
                    face.UVA = face.A.XZ * scale;
                    face.UVB = face.B.XZ * scale;
                    face.UVC = face.C.XZ * scale;
                }
                else
                {
                    face.UVA = face.A.XY * scale;
                    face.UVB = face.B.XY * scale;
                    face.UVC = face.C.XY * scale;
                }

                mesh.Faces[i] = face;
            }
        }

        public static void CreateUVPerTriangle(Mesh mesh, float tilingX, float tilingY)
        {
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                Face face = mesh.Faces[i];
                face.HasUvs = true;
                face.UVA = new Vector2(0, 0);
                face.UVB = new Vector2(tilingX, 0);
                face.UVC = new Vector2(tilingX, tilingY);
                mesh.Faces[i] = face;
            }
        }

        public static void SetPivotRelativeToBounding(Mesh mesh, Vertex relPos)
        {
            Vertex min = mesh.Faces[0][0];
            Vertex max = mesh.Faces[0][0];

            for(int i = 0; i < mesh.Faces.Count * 3; i++)
            {
                Vertex vert = mesh.Faces[i / 3][i % 3];
                for(int j = 0; j < 3; j++)
                {
                    if (vert[j] < min[j])
                        min[j] = vert[j];
                    if (vert[j] > max[j])
                        max[j] = vert[j];
                }
            }

            Vertex center = min * relPos + max * (new Vertex(1, 1, 1) - relPos);

            for(int i = 0; i < mesh.Faces.Count; i++)
            {
                Face face = mesh.Faces[i];
                face.A -= center;
                face.B -= center;
                face.C -= center;
                mesh.Faces[i] = face;
            }
        }
    }
}
