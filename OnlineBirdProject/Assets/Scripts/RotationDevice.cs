using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationDevice : MonoBehaviour
{
    private void Update()
    {
        transform.Rotate(Vector3.up, 2 * Time.deltaTime);
    }
}
