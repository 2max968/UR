// Skript zum Steuern einer fliegenden Kamera

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private const int CAMMOVE_BUTTON = 1;
    
    private float x = 0;
    private float y = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Mauscursor sperren, wenn Mausbutton gedrückt ist
        if (Input.GetMouseButtonDown(CAMMOVE_BUTTON))
            Cursor.lockState = CursorLockMode.Locked;
        if (Input.GetMouseButtonUp(CAMMOVE_BUTTON))
            Cursor.lockState = CursorLockMode.None;
        
        // Camera rotieren, wenn Maustaste gedrückt ist und Maus bewegt wird
        if (Input.GetMouseButton(CAMMOVE_BUTTON))
        {
            x += Input.GetAxis("Mouse X") * 2;
            y += Input.GetAxis("Mouse Y") * 2;
            
            transform.rotation = Quaternion.identity;
            transform.Rotate(Vector3.up, x, Space.World);
            transform.Rotate(Vector3.left, y, Space.Self);
        }

        // Kamera mit WASD im Raum bewegen
        float moveSpeed = 0.3f;
        if (Input.GetKey(KeyCode.LeftShift))
            moveSpeed = 1;
        if (Input.GetKey(KeyCode.W))
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S))
            transform.position -= transform.forward * moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.A))
            transform.position -= transform.right * moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D))
            transform.position += transform.right * moveSpeed * Time.deltaTime;
    }
}
