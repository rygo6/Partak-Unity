using UnityEngine;

public class FrustumExpand : MonoBehaviour
{
    private Camera _camera;

    private readonly float _moveBackZ = -20;

    private void Start()
    {
        _camera = GetComponent<Camera>();
    }

    private void PreRenderAdjustFOV(Camera camera)
    {
        if (camera == _camera)
        {
            camera.fieldOfView = 45;
            transform.Translate(new Vector3(0, 0, -_moveBackZ), Space.Self);
        }
    }

    public void PreCullAdjustFOV(Camera camera)
    {
        if (camera == _camera)
        {
            camera.ResetWorldToCameraMatrix();
            camera.ResetProjectionMatrix();
            camera.fieldOfView = 179;
            transform.Translate(new Vector3(0, 0, _moveBackZ), Space.Self);
        }
    }

    private void OnEnable()
    {
        Camera.onPreCull += PreCullAdjustFOV;
        Camera.onPreRender += PreRenderAdjustFOV;
    }

    private void OnDisable()
    {
        Camera.onPreCull -= PreCullAdjustFOV;
        Camera.onPreRender -= PreRenderAdjustFOV;
    }
}