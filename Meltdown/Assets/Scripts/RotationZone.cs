using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationZone : MonoBehaviour
{
    public GameObject pointerPrefab;
    [HideInInspector]
    public GameObject pointerObject;
    [HideInInspector]
    public Transform pointerTransform;

    public enum ZoneType
    {
        AROUND_POINT,
        FORWARD_DIR
    }

    public ZoneType Type;

    public GameObject Orinator;

    public BoxCollider BC;

    private void Start()
    {
        BC = GetComponent<BoxCollider>();
        pointerObject = Instantiate(pointerPrefab);
        pointerTransform = pointerObject.transform;

    }

    public bool InsideZone(Vector3 Point)
    {
        Vector3 ToPoint = Point - BC.bounds.center;

        bool Result = Mathf.Abs(ToPoint.x) < Mathf.Abs(BC.bounds.extents.x) &&
            Mathf.Abs(ToPoint.y) < Mathf.Abs(BC.bounds.extents.y) &&
            Mathf.Abs(ToPoint.z) < Mathf.Abs(BC.bounds.extents.z);

        if (Result) { EnablePointer(Point, ToPoint); };

        return Result;
    }

    private void EnablePointer(Vector3 Point, Vector3 ToPoint)
    {
        pointerObject.SetActive(true);
        pointerTransform.position = Vector3.Lerp(BC.bounds.center, Point, 0.5f);
        pointerTransform.LookAt(Point);
        pointerTransform.localScale = new Vector3(pointerTransform.localScale.x, pointerTransform.localScale.y, ToPoint.magnitude);
    }
}
