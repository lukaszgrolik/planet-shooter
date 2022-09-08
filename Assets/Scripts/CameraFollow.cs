using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private bool smoothEnabled = true;
    [SerializeField] private float smoothSpeed = 10f;
    // [HideInInspector] private Vector3 offset = new Vector3(0, 25, -25);

    private Camera cam;
    private Transform target;

    private Vector3 offset;

    public void Setup(Camera cam, Transform target)
    {
        this.target = target;

        SetCamera(cam);
    }

    public void SetCamera(Camera cam)
    {
        this.cam = cam;

        var camT = cam.transform;
        var height = camT.position.z;
        // var side = GetIsometricOffset(camT.eulerAngles.x, height);

        // offset = new Vector3(-side, height, -side);
        offset = new Vector3(0, 0, height);
    }

    public void SetSmoothEnabled(bool value)
    {
        smoothEnabled = value;
    }

    float GetIsometricOffset(float angle, float height)
    {
        return height / Mathf.Tan(angle * Mathf.PI / 180) / Mathf.Sqrt(2);
    }

    void LateUpdate()
    {
        if (!target) return;

        var targetPos = target.position + offset;
        var endPos = Vector3.zero;

        if (smoothEnabled)
        {
            endPos = Vector3.Lerp(cam.transform.position, targetPos, smoothSpeed * Time.deltaTime);
        }
        else
        {
            endPos = targetPos;
        }

        cam.transform.position = endPos;
        // Camera.main.transform.position = desiredPos;
    }
}
