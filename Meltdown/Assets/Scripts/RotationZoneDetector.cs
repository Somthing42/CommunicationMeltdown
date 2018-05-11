using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationZoneDetector : MonoBehaviour
{
    public RotationZone RotZone;

    private void OnTriggerEnter(Collider other)
    {
        print("Working: OnTriggerEnter " + other.name);
        if (other.tag == "RotationZone")
        {
            RotZone = other.gameObject.GetComponent<RotationZone>();
        }

    }

    private void OnTriggerStay(Collider other)
    {
        print("Working: OnTriggerStay " + other.name);
        if (other.tag == "RotationZone")
        {
            RotZone = other.gameObject.GetComponent<RotationZone>();
        }

    }
}
