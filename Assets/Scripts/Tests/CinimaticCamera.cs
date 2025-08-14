using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinimaticCamera : MonoBehaviour
{
    public float changeRate = 0.1f;
    public Vector3 speedVector = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float deltaY = 0;
        float deltaX = 0;
        float deltaZ = 0;
        if (Input.GetKeyDown(KeyCode.M))
        {
            Cursor.visible = false;
        }
        if (Input.GetKey("space"))
        {
            deltaY += Time.deltaTime * changeRate;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            deltaY -= Time.deltaTime * changeRate;
        }

        if (Input.GetKey("w"))
        {
            deltaZ += Time.deltaTime * changeRate;
        }
        else if (Input.GetKey("s"))
        {
            deltaZ -= Time.deltaTime * changeRate;
        }

        if (Input.GetKey("a"))
        {
            deltaX -= Time.deltaTime * changeRate;
        }
        else if (Input.GetKey("d"))
        {
            deltaX += Time.deltaTime * changeRate;
        }

        speedVector = speedVector + new Vector3(deltaX, deltaY, deltaZ);

        transform.position += (transform.forward * speedVector.z + transform.right * speedVector.x + transform.up * speedVector.y) * Time.deltaTime;
    }
}
