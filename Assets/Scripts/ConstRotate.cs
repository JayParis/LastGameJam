using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstRotate : MonoBehaviour
{
    public Vector3 axis;

    void Start()
    {
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        transform.Rotate(axis * Time.deltaTime);
    }
}
