// Der Extruder erzeugt aus der Bewegung des TCPs einen Materialstrang.
// Sobald die Extrusion deaktiviert wird, erzeugt der Extruder ein neues Objekt,
// an welches er den aktuellen Extrusionsstrang anhängt.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extruder : MonoBehaviour
{
    public RobotArm Robot;
    private List<Vector3> trace;
    private MeshRenderer wurstR;
    private MeshFilter wurstF;
    private Transform trailObj;
    private Vector3 tmpPosition = new Vector3(float.NaN, float.NaN, float.NaN);

    private bool extruding = false;

    void Start()
    {
        trace = new List<Vector3>();
        trailObj = transform.Find("Trail");
        wurstR = trailObj.GetComponent<MeshRenderer>();
        wurstF = trailObj.GetComponent<MeshFilter>();
    }

    private void LateUpdate()
    {
        // Position und Rotation des Extruders an den TCP des Roboters anpassen
        transform.position = Robot.TCP.position;
        transform.rotation = Robot.TCP.rotation;

        // Abfragen, ob der erste digidale Ausgang des Roboters aktiviert ist
        if (Robot.Outputs[0])
        {
            extruding = true;
            if (transform.position != tmpPosition)
            {
                // Position des Extruders in die Liste der Punkte aufnehmen und eine Wurst aus diesen Punkten erzeugen
                trace.Add(transform.position);
                if (trace.Count >= 2)
                {
                    wurstR.enabled = true;
                    wurstF.mesh = Util.CreateWurst(trace, 0.001f);
                }
                else
                {
                    wurstR.enabled = false;
                }
            }
        }
        else
        {
            if (extruding)
            {
                
                // Wenn die Extrusion deaktiviert wird, wird eine neues leeres Objekt erzeugt,
                // an dieses Objekt wird die Wurst angehängt
                trace.Add(transform.position);
                var empty = new GameObject();
                empty.transform.name = "Extruded Chunk";
                empty.AddComponent<MeshRenderer>().material = wurstR.material;
                empty.AddComponent<MeshFilter>().mesh = Util.CreateWurst(trace, 0.001f);
            }
            wurstR.enabled = false;
            trace.Clear();
            extruding = false;
        }
        tmpPosition = transform.position;

        trailObj.position = new Vector3(0, 0, 0);
        trailObj.rotation = Quaternion.identity;
    }
}
