// Rotation der Lüfter erzeugen

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimRotate : MonoBehaviour
{
    public float RPM = 30;
    public Vector3 RotationAxis = new Vector3(0, 1, 0);

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float angle = Time.deltaTime * 360 * RPM / 60;
        transform.Rotate(RotationAxis, angle, Space.Self);
    }
}
