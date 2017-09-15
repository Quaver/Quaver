// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using UnityEngine;
using System.Collections;

/// MouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation

/// To make an FPS style character:
/// - Create a capsule.
/// - Add a rigid body to the capsule
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSWalker script to the capsule

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)
[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour
{
    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = -60F;
    public float maximumY = 60F;

    private float _rotationX = 0F;
    private float _rotationY = 0F;

    private Quaternion _originalRotation;

    private void Update()
    {
        if (axes == RotationAxes.MouseXAndY)
        {
            // Read the mouse input axis
            _rotationX += Input.GetAxis("Mouse X") * sensitivityX;
            _rotationY += Input.GetAxis("Mouse Y") * sensitivityY;

            _rotationX = ClampAngle(_rotationX, minimumX, maximumX);
            _rotationY = ClampAngle(_rotationY, minimumY, maximumY);

            Quaternion xQuaternion = Quaternion.AngleAxis(_rotationX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(_rotationY, -Vector3.right);

            transform.localRotation = _originalRotation * xQuaternion * yQuaternion;
        }
        else if (axes == RotationAxes.MouseX)
        {
            _rotationX += Input.GetAxis("Mouse X") * sensitivityX;
            _rotationX = ClampAngle(_rotationX, minimumX, maximumX);

            Quaternion xQuaternion = Quaternion.AngleAxis(_rotationX, Vector3.up);
            transform.localRotation = _originalRotation * xQuaternion;
        }
        else
        {
            _rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            _rotationY = ClampAngle(_rotationY, minimumY, maximumY);

            Quaternion yQuaternion = Quaternion.AngleAxis(-_rotationY, Vector3.right);
            transform.localRotation = _originalRotation * yQuaternion;
        }
    }

    private void Start()
    {
        // Make the rigid body not change rotation
        var rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null)
            rigidbody.freezeRotation = true;
        _originalRotation = transform.localRotation;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}