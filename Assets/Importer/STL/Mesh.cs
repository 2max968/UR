using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace STLEdit
{
    public class Mesh
    {
        public List<Face> Faces { get; private set; } = new List<Face>();

        public string ToAsciiSTL()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("solid form");
            foreach(Face face in Faces)
            {
                sb.AppendLine(" facet normal " + face.Normal.ToSTLVertex());
                sb.AppendLine("  outer loop");
                sb.AppendLine("   vertex " + face.A.ToSTLVertex());
                sb.AppendLine("   vertex " + face.B.ToSTLVertex());
                sb.AppendLine("   vertex " + face.C.ToSTLVertex());
                sb.AppendLine("  endloop");
                sb.AppendLine(" endfacet");
            }
            sb.AppendLine("endsolid form");
            return sb.ToString();
        }

        public string ToWavefrontObject()
        {
            StringBuilder vertexList = new StringBuilder();
            StringBuilder normalList = new StringBuilder();
            StringBuilder uvsList = new StringBuilder();
            StringBuilder faceList = new StringBuilder();
            int vPointer = 1;
            int vnPointer = 1;
            int vtPointer = 1;

            foreach(Face face in Faces)
            {
                vertexList.AppendLine($"v {face.A.ToObjVertex()}");
                vertexList.AppendLine($"v {face.B.ToObjVertex()}");
                vertexList.AppendLine($"v {face.C.ToObjVertex()}");

                if(face.HasNormals)
                {
                    normalList.AppendLine($"vn {face.NormalA.ToObjVertex()}");
                    normalList.AppendLine($"vn {face.NormalB.ToObjVertex()}");
                    normalList.AppendLine($"vn {face.NormalC.ToObjVertex()}");
                }

                if(face.HasUvs)
                {
                    uvsList.AppendLine($"vt {face.UVA.ToObjVector()}");
                    uvsList.AppendLine($"vt {face.UVB.ToObjVector()}");
                    uvsList.AppendLine($"vt {face.UVC.ToObjVector()}");
                }

                if(face.HasUvs && face.HasNormals)
                    faceList.AppendLine($"f {vPointer + 0}/{vtPointer + 0}/{vnPointer + 0} {vPointer + 1}/{vtPointer + 1}/{vnPointer + 1} {vPointer + 2}/{vtPointer + 2}/{vnPointer + 2}");
                else if(face.HasUvs)
                    faceList.AppendLine($"f {vPointer + 0}/{vtPointer + 0} {vPointer + 1}/{vtPointer + 1} {vPointer + 2}/{vtPointer + 2}");
                else if (face.HasNormals)
                    faceList.AppendLine($"f {vPointer + 0}//{vnPointer + 0} {vPointer + 1}//{vnPointer + 1} {vPointer + 2}//{vnPointer + 2}");
                else
                    faceList.AppendLine($"f {vPointer + 0} {vPointer + 1} {vPointer + 2}");

                vPointer += 3;
                if (face.HasNormals) vnPointer += 3;
                if (face.HasUvs) vtPointer += 3;
            }

            return vertexList.ToString() + Environment.NewLine + normalList.ToString() + Environment.NewLine + uvsList.ToString() + Environment.NewLine + faceList.ToString();
        }

        public static Mesh LoadFromASCIIStl(string content)
        {
            try
            {
                string[] lines = content.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                Mesh mesh = new Mesh();
                Face face = new Face();
                List<Vertex> vertices = new List<Vertex>();
                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith("facet normal"))
                    {
                        string strNormal = trimmedLine.Substring(13);
                        if (Vertex.FromString(strNormal, out Vertex normal))
                        {
                            vertices = new List<Vertex>();
                            face = new Face();
                            face.Normal = normal;
                        }
                    }
                    else if (trimmedLine.StartsWith("vertex"))
                    {
                        string strVertex = trimmedLine.Substring(7);
                        if (Vertex.FromString(strVertex, out Vertex v))
                        {
                            vertices.Add(v);
                        }
                    }
                    else if (trimmedLine.StartsWith("endfacet"))
                    {
                        if (vertices.Count == 3)
                        {
                            face.A = vertices[0];
                            face.B = vertices[1];
                            face.C = vertices[2];
                            mesh.Faces.Add(face);
                        }
                    }
                }
                return mesh;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Mesh LoadFromBinaryStl(byte[] content)
        {
            try
            {
                int ptr = 80;
                uint number = BitConverter.ToUInt32(content, ptr);
                int filesize = (int)(84 + number * 50);
                if (filesize != content.Length)
                    return null;
                ptr += 4;
                GCHandle handle = GCHandle.Alloc(content, GCHandleType.Pinned);
                Mesh mesh = new Mesh();
                for (int i = 0; i < number; i++)
                {
                    Vertex normal = Marshal.PtrToStructure<Vertex>(handle.AddrOfPinnedObject() + ptr);
                    ptr += 3 * 4;
                    Vertex[] vertices = new Vertex[3];
                    for (int j = 0; j < 3; j++)
                    {
                        vertices[j] = Marshal.PtrToStructure<Vertex>(handle.AddrOfPinnedObject() + ptr);
                        ptr += 3 * 4;
                    }
                    Face face = new Face(vertices[0], vertices[1], vertices[2]);
                    face.Normal = normal;
                    mesh.Faces.Add(face);
                    ptr += 2;
                }
                handle.Free();
                return mesh;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Mesh LoadFromStl(string path)
        {
            var stream = File.OpenRead(path);
            List<byte> _data = new List<byte>();
            byte[] buffer = new byte[1024];
            long l;
            while((l = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                for(int i = 0; i < l; i++)
                {
                    _data.Add(buffer[i]);
                }
            }
            stream.Close();

            byte[] data = _data.ToArray();
            Mesh mesh1 = Mesh.LoadFromBinaryStl(data);

            string dataStr = File.ReadAllText(path);
            Mesh mesh2 = Mesh.LoadFromASCIIStl(dataStr);

            if (mesh1 == null)
                return mesh2;
            if (mesh2 == null || mesh1.Faces.Count > mesh2.Faces.Count)
                return mesh1;
            return mesh2;
        }

        public static Mesh Merge(Mesh a, Mesh b)
        {
            Mesh c = new Mesh();
            c.Faces.AddRange(a.Faces);
            c.Faces.AddRange(b.Faces);
            return c;
        }

        public Mesh Clone()
        {
            Mesh mesh = new Mesh();
            mesh.Faces.AddRange(Faces);
            return mesh;
        }

        public void TransformTranslate(float x, float y, float z)
        {
            TransformTranslate(new Vertex(x, y, z));
        }

        public void TransformTranslate(Vertex offset)
        {
            for(int i = 0; i < Faces.Count; i++)
            {
                Face face = Faces[i];
                face.A += offset;
                face.B += offset;
                face.C += offset;
                Faces[i] = face;
            }
        }

        public void Transform(Matrix4x4 matrix)
        {
            for(int i = 0; i < Faces.Count; i++)
            {
                Face face = Faces[i];
                face.A *= matrix;
                face.B *= matrix;
                face.C *= matrix;
                Faces[i] = face;
            }
        }

        public void SaveAscciSTL(string filename)
        {
            File.WriteAllText(filename, ToAsciiSTL());
        }

        public (float xmin, float xmax, float ymin, float ymax, float zmin, float zmax) GetBounds()
        {
            (float xmin, float xmax, float ymin, float ymax, float zmin, float zmax) bounds = (float.MaxValue, float.MinValue, float.MaxValue, float.MinValue, float.MaxValue, float.MinValue);
            foreach(Face face in Faces)
            {
                for(int i = 0; i <3; i++)
                {
                    Vertex v = face[i];
                    bounds.xmin = Math.Min(bounds.xmin, v.X);
                    bounds.ymin = Math.Min(bounds.ymin, v.Y);
                    bounds.zmin = Math.Min(bounds.zmin, v.Z);

                    bounds.xmax = Math.Max(bounds.xmax, v.X);
                    bounds.ymax = Math.Max(bounds.ymax, v.Y);
                    bounds.zmax = Math.Max(bounds.zmax, v.Z);
                }
            }

            return bounds;
        }
    }
}
