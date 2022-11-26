using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ModelPreview : MonoBehaviour
{
    public Material Material;
    FileInfo[] files;
    Matrix4x4 file2world
        = Matrix4x4.Scale(new Vector3(.01f, .01f, .01f))
        * new Matrix4x4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, 0, 1)
            );

    public IEnumerator LoadModel(Stream inputStream)
    {
        while (transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
            yield return new WaitForSeconds(0);
        }

        GCodeParser parser = GCodeParser.ParseString(inputStream);

        List<Vector3> points = new List<Vector3>();
        bool extruding = false;
        int counter = 0;
        GCodeLine lastLine = null;
        foreach(var line in parser.Lines)
        {
            if(line.IsMovement)
            {
                if(line.Extrusion > 0)
                {
                    if(!extruding && lastLine is not null)
                    {
                        points.Add(file2world * new Vector3((float)lastLine.X, (float)lastLine.Y, (float)lastLine.Z));
                    }
                    points.Add(file2world * new Vector3((float)line.X, (float)line.Y, (float)line.Z));
                    extruding = true;
                }
                else
                {
                    if(extruding)
                    {
                        var obj = new GameObject();
                        //var lineR = obj.AddComponent<LineRenderer>();
                        //lineR.positionCount = points.Count;
                        //lineR.SetPositions(points.ToArray());
                        obj.transform.name = $"Chunk {counter++}";
                        obj.transform.parent = transform;
                        //lineR.widthMultiplier = 0.01f;
                        //lineR.material = Material;
                        var wurst = Util.CreateWurst(points, 0.001f);
                        obj.AddComponent<MeshFilter>().mesh = wurst;
                        obj.AddComponent<MeshRenderer>().material = Material;
                        points.Clear();
                        yield return new WaitForSeconds(0);
                    }
                    extruding = false;
                }
                lastLine = line;
            }
        }

        /*var go = new GameObject();
        var mesh = Util.CreateSphere(1);
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>().material = Material;*/
    }

    private void Start()
    {
        var dir = new DirectoryInfo(".");
        files = dir.GetFiles("*.gcode", SearchOption.AllDirectories);
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(8, 8, 128, 400));
        foreach(var file in files)
        {
            if(GUILayout.Button(file.Name))
            {
                var stream = File.OpenRead(file.FullName);
                StartCoroutine(LoadModel(stream));
            }
        }
        GUILayout.EndArea();
    }
}
