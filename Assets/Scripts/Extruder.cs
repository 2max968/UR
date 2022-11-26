using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extruder : MonoBehaviour
{
    public RobotArm Robot;
    private List<Vector3> trace;
    //private LineRenderer line;
    private MeshRenderer wurstR;
    private MeshFilter wurstF;
    private Transform trailObj;
    private Vector3 tmpPosition = new Vector3(float.NaN, float.NaN, float.NaN);

    private bool extruding = false;

    // Start is called before the first frame update
    void Start()
    {
        trace = new List<Vector3>();
        trailObj = transform.Find("Trail");
        wurstR = trailObj.GetComponent<MeshRenderer>();
        wurstF = trailObj.GetComponent<MeshFilter>();
        //line = GetComponentInChildren<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        transform.position = Robot.TCP.position;
        transform.rotation = Robot.TCP.rotation;

        if (Robot.Outputs[0])
        {
            extruding = true;
            if (transform.position != tmpPosition)
            {
                trace.Add(transform.position);
                if (trace.Count >= 2)
                {
                    wurstR.enabled = true;
                    wurstF.mesh = Util.CreateWurst(trace, 0.01f);
                    //line.enabled = true;
                    //line.positionCount = trace.Count;
                    //for (int i = 0; i < trace.Count; i++)
                    //    line.SetPosition(i, trace[i]);
                }
                else
                {
                    wurstR.enabled = false;
                    //line.enabled = false;
                }
            }
        }
        else
        {
            if (extruding)
            {
                trace.Add(transform.position);
                var empty = new GameObject();
                empty.transform.name = "Extruded Chunk";
                /*var lr = empty.AddComponent<LineRenderer>();
                var chunk = empty.AddComponent<ExtrudedChunk>();
                lr.material = line.material;
                lr.widthMultiplier = line.widthMultiplier;
                chunk.Trace = trace.ToArray();*/
                empty.AddComponent<MeshRenderer>().material = wurstR.material;
                empty.AddComponent<MeshFilter>().mesh = Util.CreateWurst(trace, 0.01f);
            }
            //line.enabled = false;
            wurstR.enabled = false;
            trace.Clear();
            extruding = false;
        }
        tmpPosition = transform.position;

        trailObj.position = new Vector3(0, 0, 0);
        trailObj.rotation = Quaternion.identity;
        //trailObj.localScale = trailObj.parent.lossyScale.normalize;
    }
}
