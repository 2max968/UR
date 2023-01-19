using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Assets.Scripts;
using UnityEngine;

public class RobotArm : MonoBehaviour
{
    public Transform TCP;
    public Transform[] Transforms;
    public Axis[] RotationAxis;
    public int[] RotationOffsets;
    public Transform PrintBed;

    public float[] Angles = new float[] { 0, 0, 0, 0, 0, 0 };
    private Quaternion[] startRotations;

    private URPackageListener urListener;
    private string ipInput = "192.168.56.101";

    public Vector3 TCPPosition;
    public Quaternion TCPRotation;
    public bool[] Outputs;

    public TextAsset GCode;

    public static readonly Matrix4x4 Robot2Unity = new Matrix4x4(
        new Vector4(1, 0, 0, 0),
        new Vector4(0, 0, 1, 0),
        new Vector4(0, 1, 0, 0),
        new Vector4(0, 0, 0, 1));

    // Start is called before the first frame update
    void Start()
    {
        urListener = new URPackageListener();
        startRotations = new Quaternion[Transforms.Length];
        for (int i = 0; i < Transforms.Length; i++)
            startRotations[i] = Transforms[i].localRotation;
        Outputs = new bool[18];
    }

    private void OnDestroy()
    {
        urListener?.Close();
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < Transforms.Length; i++)
        {
            if(urListener != null && urListener.Connected)
                Angles[i] = (float)urListener.JointData.AsArray[i].q_actual * 180f / MathF.PI;
            
            Transforms[i].localRotation = startRotations[i];
            Transforms[i].Rotate(axisTovector3(RotationAxis[i]), Angles[i] + RotationOffsets[i], Space.Self);
        }
        if(urListener != null && urListener.Connected)
        {
            /*var tcpOffset = new Vector3((float)urListener.CartesianInfo.TCPOffsetX,
                (float)urListener.CartesianInfo.TCPOffsetY,
                -(float)urListener.CartesianInfo.TCPOffsetZ);
            var tcpRotation = Quaternion.Euler((float)urListener.CartesianInfo.TCPOffsetRx * 180f / Mathf.PI,
                (float)urListener.CartesianInfo.TCPOffsetRy * 180f / Mathf.PI,
                (float)urListener.CartesianInfo.TCPOffsetRz * 180f / Mathf.PI);
            TCP.localPosition = tcpOffset * 1000;
            TCP.localRotation = tcpRotation;*/
            Vector4 cartPosition = Robot2Unity * new Vector4((float)urListener.CartesianInfo.X, 
                (float)urListener.CartesianInfo.Y,
                (float)urListener.CartesianInfo.Z, 1);
            Quaternion cartRotation = Quaternion.Euler(new Vector3(
                (float)urListener.CartesianInfo.Rx * 180f / Mathf.PI,
                (float)urListener.CartesianInfo.Ry * 180f / Mathf.PI,
                (float)urListener.CartesianInfo.Rz * 180f / Mathf.PI));
            var rotMat = transform.localToWorldMatrix;
            rotMat.SetColumn(3, new(0, 0, 0, 1));
            var cartForward = rotMat * (cartRotation * Vector3.forward);
            var cartUp = rotMat * (cartRotation * Vector3.up);
            TCP.position =  transform.localToWorldMatrix * cartPosition;
            TCP.rotation = cartRotation;
            
            TCPPosition = new Vector3((float)urListener.CartesianInfo.Y, -(float)urListener.CartesianInfo.Z,
                (float)urListener.CartesianInfo.X);
            TCPRotation = Quaternion.Euler((float)urListener.CartesianInfo.Rx, (float)urListener.CartesianInfo.Ry,
                (float)urListener.CartesianInfo.Rz);

            for (int i = 0; i < Outputs.Length; i++)
            {
                int bits = urListener.MasterboardData.digitalOutputBits;
                bits >>= i;
                bits &= 1;
                Outputs[i] = bits != 0;
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 200, 400));
        if (!urListener.Connected)
        {
            GUILayout.BeginHorizontal();
            ipInput = GUILayout.TextField(ipInput);
            if (GUILayout.Button("Connect"))
            {
                urListener.Connect(ipInput, false);
            }

            GUILayout.EndHorizontal();
        }
        else
        {
            if (GUILayout.Button("Disconnect"))
            {
                urListener.Close();
            }
            
            if(GUILayout.Button("Home"))
            {
                var bedTransform = PrintBed.localToWorldMatrix;
                Vector3 homePoint = bedTransform.GetColumn(3);
                Vector3 homeDown = bedTransform * Vector3.down;
                var homePointRobot = Robot2Unity.inverse * new Vector4(homePoint.x, homePoint.y, homePoint.z, 1);
                string cmd = $"movej(p[{homePointRobot.x.ToString(CultureInfo.InvariantCulture)}, {homePointRobot.y.ToString(CultureInfo.InvariantCulture)}, {homePointRobot.z.ToString(CultureInfo.InvariantCulture)}, 3.14145, 0, 0])";
                SendProgram(new []{cmd}, "home");
            }
            
            if(GUILayout.Button("Move Test"))
            {
                var points = new Vector3[]
                {
                    new(0, 0, 0.05f),
                    new(0.1f, 0.1f,0.001f),
                    new(float.NaN, 1,1),
                    new(-0.1f,0.1f,0.001f),
                    new(-0.1f,-0.1f,0.001f),
                    new(0.1f,-0.1f,0.001f),
                    new(0.1f, 0.1f,0.001f),
                    new(float.NaN, 1, 0),
                    new(0, 0, 0.05f)
                };
                var program = CreatePath(PrintBed, transform, points, 0.3f, 0).ToList();
                SendProgram(program, "move_test");
            }

            if (GUILayout.Button("Blink"))
            {
                List<string> program = new List<string>();
                for (int i = 0; i < 10; i++)
                {
                    program.Add("set_digital_out(0, True)");
                    program.Add("sleep(0.5)");
                    program.Add("set_digital_out(0, False)");
                }
                SendProgram(program);
            }

            if (GUILayout.Button(GCode.name))
            {
                var gcode = GCodeParser.ParseString(GCode.text);
                bool extrusion = false;
                var path = new List<Vector3>();
                path.Add(new(float.NaN, 1, 0));
                foreach (var line in gcode.Lines)
                {
                    if (line.Extrusion > 0 && !extrusion)
                    {
                        extrusion = true;
                        path.Add(new(float.NaN, 1, 1));
                    }
                    if (line.Extrusion == 0 && extrusion)
                    {
                        extrusion = false;
                        path.Add(new(float.NaN, 1, 0));
                    }

                    var lastPoint = path.LastOrDefault(v => float.IsNormal(v.magnitude));
                    var currentPoint = new Vector3((float)line.X / 1000f, (float)line.Y / 1000f, (float)line.Z / 1000f);
                    if(Vector3.Distance(lastPoint, currentPoint) > 0.0001f)
                        path.Add(currentPoint);
                }
                var program = CreatePath(PrintBed, transform, path.ToArray(), 0.05f, 0);
                SendProgram(program, "gcode");
            }
        }
        GUILayout.EndArea();
    }

    static Vector3 axisTovector3(Axis axis)
    {
        switch (axis)
        {
            case Axis.PositiveX: return new Vector3(1, 0, 0);
            case Axis.PositiveY: return new Vector3(0, 1, 0);
            case Axis.PositiveZ: return new Vector3(0, 0, 1);
            case Axis.NegativeX: return new Vector3(-1, 0, 0);
            case Axis.NegativeY: return new Vector3(0, -1, 0);
            case Axis.NegativeZ: return new Vector3(0, 0, -1);
            default: throw new Exception($"Undefined Axis: {axis}");
        }
    }
    
    public void SendProgram(IEnumerable<string> program, string programName = "program")
    {
        var list = Enumerable.Concat(Enumerable.Concat(Enumerable.Repeat($"def {programName}():", 1), program), Enumerable.Repeat("end", 1));
        urListener.SendCommand(string.Join('\n', list));
    }

    public static string[] CreatePath(Transform bed, Transform robotBase, IEnumerable<Vector3> points, float v = 0.3f, float r = 0.02f)
    {
        var robotToBed = robotBase.worldToLocalMatrix * bed.localToWorldMatrix;
        var transform = Robot2Unity.inverse * robotToBed * Robot2Unity;
        string commandl = $"movel(p[{{0}}, {{1}}, {{2}}, {{3}}, {{4}}, {{5}}], a=1.4, v={v.ToString(CultureInfo.InvariantCulture)}, t=0, r={r.ToString(CultureInfo.InvariantCulture)})";
        string command2 = $"movej(p[{{0}}, {{1}}, {{2}}, {{3}}, {{4}}, {{5}}])";
        var transformedPoints = points.Select(p => float.IsNaN(p.x) ? p : transform.MultiplyPoint(p));
        List<string> commands = new List<string>();
        bool moved = false;
        foreach (var p in transformedPoints)
        {
            if (float.IsNaN(p.x) && p.y == 1) // Extrution
            {
                if(p.z == 0)
                    commands.Add("set_digital_out(0, False)");
                else if(p.z == 1)
                    commands.Add("set_digital_out(0, True)");
            }
            else // Move
            {
                if (moved)
                {
                    commands.Add(string.Format(CultureInfo.InvariantCulture, commandl, p.x, p.y, p.z, Math.PI, 0, 0));
                }
                else
                {
                    moved = true;
                    commands.Add(string.Format(CultureInfo.InvariantCulture, command2, p.x, p.y, p.z, Math.PI, 0, 0));
                }
            }
        }

        return commands.ToArray();
    }
}

public enum Axis
{
    PositiveX,
    PositiveY,
    PositiveZ,
    NegativeX,
    NegativeY,
    NegativeZ
}