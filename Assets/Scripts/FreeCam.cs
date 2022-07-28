using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCam : MonoBehaviour
{
    [SerializeField]
    private float _PositionSpeed = 0.01f;

    [SerializeField]
    private float _RotationSpeed = 0.0005f;

    private void Update()
    {
        if (Input.GetAxis("Fire2") != 0)
        {
            float horizontal = Input.GetAxis("Horizontal") * _PositionSpeed;
            transform.localPosition += transform.right * horizontal;

            float vertical = Input.GetAxis("Vertical") * _PositionSpeed;
            transform.localPosition += transform.forward * vertical;

            float transformZ = Input.GetAxis("z") * _PositionSpeed;
            transform.localPosition += transform.up * transformZ;

            float mouseX = Input.GetAxis("Mouse X") * _RotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * _RotationSpeed;

            float rotationX = transform.localEulerAngles.y + mouseX;
            float rotationY = transform.localEulerAngles.x - mouseY;

            transform.localRotation = Quaternion.Euler(rotationY, rotationX, transform.localEulerAngles.z);
        }
    }
}
