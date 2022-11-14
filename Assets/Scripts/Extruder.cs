using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extruder : MonoBehaviour
{
    public RobotArm Robot;
    private List<Vector3> trace;
    private LineRenderer line;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        transform.position = Robot.TCPPosition;
        transform.rotation = Robot.TCPRotation;
    }
}
