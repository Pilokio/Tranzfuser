using UnityEngine;

public class CameraFov : MonoBehaviour
{
    private Camera playerCamera;
    private float targetFov;
    private float fov;
    private const float NORMAL_FOV = 60f;
    private const float HOOKSHOT_FOV = 100f;

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

    public void UseGrappleFOV()
    {
        this.targetFov = HOOKSHOT_FOV;

    }

    public void UseNormalFOV()
    {
        this.targetFov = NORMAL_FOV;
    }
}
