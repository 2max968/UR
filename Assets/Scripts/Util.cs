using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static Mesh CreateWurst(IEnumerable<Vector3> points, float radius)
    {
        //Mesh sphere = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
        Mesh sphere = CreateSphere(radius);
        List<Mesh> meshes = new List<Mesh>();
        List<Matrix4x4> matrices = new List<Matrix4x4>();

        Vector3? lastPoint = null;
        foreach(var p in points)
        {
            if (lastPoint is not null && p == lastPoint.Value)
                continue;

            if(lastPoint != null)
            {
                meshes.Add(CreateCyllinder(radius, Vector3.Distance(p, lastPoint.Value)));
                Debug.Assert(p != lastPoint.Value);
                var lookRotation = Quaternion.LookRotation(p - lastPoint.Value);
                var translation = (p + lastPoint.Value) / 2;
                var trans = Matrix4x4.Translate(translation) * Matrix4x4.Rotate(lookRotation);
                matrices.Add(trans);
            }
            lastPoint = p;

            meshes.Add(sphere);
            matrices.Add(Matrix4x4.Translate(p));
        }

        var ci = new CombineInstance[meshes.Count];
        for(int i = 0; i < ci.Length; i++)
        {
            ci[i].mesh = meshes[i];
            ci[i].transform = matrices[i];
        }

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(ci);
        return mesh;
    }

    public static Mesh CreateCyllinder(float rad, float height, int steps = 8)
    {
        float z = height / 2;
        var vertices = new Vector3[steps * 2];
        var normals = new Vector3[steps * 2];
        var triangles = new int[steps * 2 * 3];
        for(int i = 0; i < steps; i++)
        {
            // Generate Vertices
            float a = (float)i / steps * Mathf.PI * 2;
            float x = Mathf.Cos(a) * rad;
            float y = Mathf.Sin(a) * rad;
            vertices[i * 2] = new Vector3(x, y, z);
            vertices[i * 2 + 1] = new Vector3(x, y, -z);
            normals[i * 2] = normals[i*2+1] = new Vector3(x, y, 0).normalized;

            // Generate Triangles
            triangles[i * 3 * 2 + 0] = (i * 2 + 0) % (steps * 2);
            triangles[i * 3 * 2 + 1] = (i * 2 + 1) % (steps * 2);
            triangles[i * 3 * 2 + 2] = (i * 2 + 2) % (steps * 2);

            triangles[i * 3 * 2 + 3] = (i * 2 + 1) % (steps * 2);
            triangles[i * 3 * 2 + 4] = (i * 2 + 3) % (steps * 2);
            triangles[i * 3 * 2 + 5] = (i * 2 + 2) % (steps * 2);
        }

        return new Mesh()
        {
            vertices = vertices,
            triangles = triangles,
            normals = normals
        };
    }

    public static Mesh CreateSphere(float radius, int steps = 8)
    {
        var vertices = new Vector3[steps * steps];
        var normals = new Vector3[steps * steps];
        var triangles = new int[steps * steps * 2 * 3];
        for(int i = 0; i < steps; i++)
        {
            float ai = (float)i / (steps - 1) * Mathf.PI;
            float yFactor = Mathf.Cos(ai);
            float radiusFactor = Mathf.Sin(ai);
            float y = yFactor * radius;
            float cRadius = radius * radiusFactor;
            for(int j = 0; j < steps; j++)
            {
                float aj = (float)j / steps * Mathf.PI * 2;
                float x = Mathf.Cos(aj) * cRadius;
                float z = Mathf.Sin(aj) * cRadius;

                vertices[i * steps + j] = new Vector3(x, y, z);
                normals[i * steps + j] = new Vector3(x, y, z).normalized;

                if (i < steps - 1)
                {
                    var tri = new[]
                    {
                        (i * steps + j) % vertices.Length,
                        (i * steps + j + steps) % vertices.Length,
                        (i * steps + j + 1) % vertices.Length,
                        (i * steps + j + 1 + steps) % vertices.Length
                    };
                    var triInd = (i * steps + j) * 6;
                    triangles[triInd + 0] = tri[0];
                    triangles[triInd + 1] = tri[2];
                    triangles[triInd + 2] = tri[1];
                    triangles[triInd + 3] = tri[1];
                    triangles[triInd + 4] = tri[2];
                    triangles[triInd + 5] = tri[3];
                }
            }
        }

        return new Mesh()
        {
            vertices = vertices,
            normals = normals,
            triangles = triangles
        };
    }
}
