using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFov : MonoBehaviour
{
    private Camera playerCamera;
    private float targetFov;
    private float fov;

    private void Awake()
    {
        playerCamera = GetComponent<Camera>();
        targetFov = playerCamera.fieldOfView;
        fov = targetFov;
    }
    // Update is called once per frame
    void Update()
    {
        float fovSpeed = 4f;
        fov = Mathf.Lerp(fov, targetFov, Time.deltaTime * fovSpeed);
        playerCamera.fieldOfView = fov;
    }

    public void SetCameraFov(float targetFov)
    {
        this.targetFov = targetFov;
    }
}
