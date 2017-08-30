using UnityEngine;
using System.Collections;

/// <summary>
/// Very basic player controls.
/// </summary>
public class WASDMovement : MonoBehaviour
{
    public float speed = 1;
    
    void Update () 
    {
        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            direction += Vector3.forward;
        if (Input.GetKey(KeyCode.S))
            direction += Vector3.back;
        if (Input.GetKey(KeyCode.D))
            direction += Vector3.right;
        if (Input.GetKey(KeyCode.A))
            direction += Vector3.left;
        
        direction = transform.TransformDirection(direction);
        direction.y = 0;
        direction.Normalize();

        transform.position += direction * speed;
    }
}
