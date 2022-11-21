using UnityEngine;

public class ExtrudedChunk : MonoBehaviour
{
    public Vector3[] Trace = new Vector3[0];
    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();
    }
    
    void Update()
    {
        line.positionCount = Trace.Length;
        line.SetPositions(Trace);
        Destroy(this);
    }
}