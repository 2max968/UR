using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
            var tcpOffset = new Vector3((float)urListener.CartesianInfo.TCPOffsetX,
                (float)urListener.CartesianInfo.TCPOffsetY,
                -(float)urListener.CartesianInfo.TCPOffsetZ);
            var tcpRotation = Quaternion.Euler((float)urListener.CartesianInfo.TCPOffsetRx * 180f / Mathf.PI,
                (float)urListener.CartesianInfo.TCPOffsetRy * 180f / Mathf.PI,
                (float)urListener.CartesianInfo.TCPOffsetRz * 180f / Mathf.PI);
            TCP.localPosition = tcpOffset * 1000;
            TCP.localRotation = tcpRotation;
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
            if (GUILayout.Button("Diconnect"))
            {
                urListener.Close();
            }
            
            if(GUILayout.Button("Home"))
            {
                string cmd = "def myProg()\nmovej([0,1.57,-1.57,3.14,-1.57,1.57], a=1.4, v=1.05, t=0, r=0)\nmovej([1.57,1.57,-1.57,3.14,-1.57,1.57], a=1.4, v=1.05, t=0, r=0)\nend";
                urListener.SendCommand(cmd);
            }
            
            if(GUILayout.Button("Move Test"))
            {
                var points = new[]
                {
                    new Vector3(0,0,0),
                    new Vector3(0,0,0.1f),
                    new Vector3(0, 0.1f,0.1f)
                };
                var program = CreatePath(PrintBed.localToWorldMatrix, points);
                SendProgram(program);
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
    
    public void SendProgram(IEnumerable<string> program)
    {
        var list = Enumerable.Concat(Enumerable.Concat(Enumerable.Repeat("def program():", 1), program), Enumerable.Repeat("end", 1));
        urListener.SendCommand(string.Join('\n', list));
    }

    public static string[] CreatePath(Matrix4x4 transform, IEnumerable<Vector3> points, float v = 0.3f, float r = 0.02f)
    {
        string commandl = $"movel([{{0}}, {{1}}, {{2}}, {{3}}, {{4}}, {{5}}], a=1.4, v={v}, t=0, r={r})";
        var transformedPoints = points.Select(p=>transform.MultiplyPoint(p));
        return transformedPoints.Select(p => string.Format(commandl, p.x, p.y, p.z, 0, Math.PI, 0)).ToArray();
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