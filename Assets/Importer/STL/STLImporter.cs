using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace UnitySTL
{
    [ScriptedImporter(1, "stl")]
    public class STLImporter : ScriptedImporter
    {
        public NormalMode NormalMode = NormalMode.Flat;
        public float GlobalScaling = 1;
        public UVMode UVMode = UVMode.ProjectXYZ;
        public Vector2 UVTiling = new Vector2(1, 1);
        public OutputType ImportMode = OutputType.Prefab;
        public bool GenerateCollider = false;
        public PivotMode PivotMode = PivotMode.None;
        public Vector3 PivotPosition = new Vector3(.5f, .5f, .5f);

        public override void OnImportAsset(AssetImportContext ctx)
        {
            string name = new FileInfo(assetPath).Name;
            int dot = name.LastIndexOf('.');
            if(dot > 0)
                name = name.Substring(0, name.LastIndexOf('.'));
            var stlMesh = STLEdit.Mesh.LoadFromStl(ctx.assetPath);
            stlMesh.Transform(STLEdit.Matrix4x4.GetScaleMatrix(GlobalScaling, GlobalScaling, GlobalScaling));
            if(NormalMode == NormalMode.Flat)
            {
                STLEdit.MeshOptimizer.CreateNormalsFlat(stlMesh);
            }
            else if(NormalMode == NormalMode.Smooth)
            {
                STLEdit.MeshOptimizer.CreateNormalsSmooth(stlMesh);
            }

            if(UVMode == UVMode.ProjectXYZ)
            {
                STLEdit.MeshOptimizer.CreateUVSimpleXYZ(stlMesh, false, UVTiling.x, UVTiling.y);
            }
            else if (UVMode == UVMode.ProjectXYZNormalized)
            {
                STLEdit.MeshOptimizer.CreateUVSimpleXYZ(stlMesh, true, UVTiling.x, UVTiling.y);
            }
            else if(UVMode == UVMode.PerTriangle)
            {
                STLEdit.MeshOptimizer.CreateUVPerTriangle(stlMesh, UVTiling.x, UVTiling.y);
            }

            if(PivotMode == PivotMode.RelativeToBoundingBox)
            {
                STLEdit.MeshOptimizer.SetPivotRelativeToBounding(stlMesh, new STLEdit.Vertex(PivotPosition.x, PivotPosition.y, PivotPosition.z));
            }

            List<Vector3> vertecies = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            int vPtr = 0;
            foreach(var face in stlMesh.Faces)
            {
                vertecies.Add(face.A.ToVector());
                vertecies.Add(face.B.ToVector());
                vertecies.Add(face.C.ToVector());
                triangles.Add(vPtr++);
                triangles.Add(vPtr++);
                triangles.Add(vPtr++);
                if(face.HasNormals)
                {
                    normals.Add(face.NormalA.ToVector());
                    normals.Add(face.NormalB.ToVector());
                    normals.Add(face.NormalC.ToVector());
                }
                if(face.HasUvs)
                {
                    uvs.Add(face.UVA.ToVector());
                    uvs.Add(face.UVB.ToVector());
                    uvs.Add(face.UVC.ToVector());
                }
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertecies.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uvs.ToArray();

            if (ImportMode == OutputType.Mesh)
            {
                ctx.AddObjectToAsset("mesh", mesh);
            }
            else if (ImportMode == OutputType.Prefab)
            {
                GameObject prefab = new GameObject();
                var meshRenderer = prefab.AddComponent<MeshRenderer>();
                var meshFilter = prefab.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = mesh;
                var material = new Material(Shader.Find("Standard"));
                meshRenderer.material = material;
                if(GenerateCollider)
                {
                    var meshCollider = prefab.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = mesh;
                }
                mesh.name = name;
                ctx.AddObjectToAsset("material", material);
                ctx.AddObjectToAsset("mesh", mesh);
                ctx.AddObjectToAsset("prefab", prefab);
                ctx.SetMainObject(prefab);
            }
        }
    }

    [CustomEditor(typeof(STLImporter))]
    public class STLImportSettings : ScriptedImporterEditor
    {
        const float INCH = 0.0254f;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var importMode = serializedObject.FindProperty("ImportMode");
            var globalScaling = serializedObject.FindProperty("GlobalScaling");
            var normalMode = serializedObject.FindProperty("NormalMode");
            var uvMode = serializedObject.FindProperty("UVMode");
            var tiling = serializedObject.FindProperty("UVTiling");
            var generateCollider = serializedObject.FindProperty("GenerateCollider");
            var pivotMode = serializedObject.FindProperty("PivotMode");
            var pivotPosition = serializedObject.FindProperty("PivotPosition");

            EditorGUILayout.PropertyField(importMode);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(globalScaling);
            ScalingPresets sp = ScalingPresets.Custom;
            float _gs = globalScaling.floatValue;
            if (_gs.Eq(1)) sp = ScalingPresets.Meter;
            else if (_gs.Eq(0.01f)) sp = ScalingPresets.Centimeter;
            else if (_gs.Eq(0.001f)) sp = ScalingPresets.Millimeter;
            else if (_gs.Eq(INCH)) sp = ScalingPresets.Inch;
            ScalingPresets sp2 = (ScalingPresets)EditorGUILayout.EnumPopup(sp);
            if(sp2 != sp && sp2 != ScalingPresets.Custom)
            {
                if (sp2 == ScalingPresets.Meter) _gs = 1;
                else if (sp2 == ScalingPresets.Centimeter) _gs = .01f;
                else if (sp2 == ScalingPresets.Millimeter) _gs = .001f;
                else if (sp2 == ScalingPresets.Inch) _gs = INCH;
                globalScaling.floatValue = _gs;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(normalMode);
            EditorGUILayout.PropertyField(uvMode);
            EditorGUILayout.PropertyField(tiling);
            if((OutputType)System.Enum.GetValues(typeof(OutputType)).GetValue(importMode.enumValueIndex) == OutputType.Prefab)
            {
                EditorGUILayout.PropertyField(generateCollider);
            }
            else
            {
                generateCollider.boolValue = false;
            }

            EditorGUILayout.PropertyField(pivotMode);
            var valPivotMode = (PivotMode)System.Enum.GetValues(typeof(PivotMode)).GetValue(pivotMode.enumValueIndex);
            if (valPivotMode == PivotMode.RelativeToBoundingBox)
                EditorGUILayout.PropertyField(pivotPosition);

            serializedObject.ApplyModifiedProperties();
            ApplyRevertGUI();
        }

        /*protected override void Apply()
        {
            serializedObject.ApplyModifiedProperties();
            base.Apply();
        }

        protected override bool OnApplyRevertGUI()
        {
            bool apply = base.OnApplyRevertGUI();
            Debug.Log("Apply: " + apply);
            return apply;
        }*/
    }

    public enum NormalMode
    {
        Flat,
        Smooth
    }

    public enum UVMode
    {
        ProjectXYZ,
        ProjectXYZNormalized,
        PerTriangle
    }

    public enum OutputType
    {
        Mesh,
        Prefab
    }

    public enum ScalingPresets
    {
        Custom,
        Meter,
        Centimeter,
        Millimeter,
        Inch
    }

    public enum PivotMode
    {
        None,
        RelativeToBoundingBox
    }

    public static class UnityExtensions
    {
        public static Vector3 ToVector(this STLEdit.Vertex v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Vector2 ToVector(this STLEdit.Vector2 v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static bool Eq(this float a, float b)
        {
            float diff = a - b;
            return (diff > -float.Epsilon && diff < float.Epsilon);
        }
    }
}