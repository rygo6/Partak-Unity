using UnityEngine;
using System.Collections;

public class FrustumExpand : MonoBehaviour {

	float _moveBackZ = -20;
	Camera _camera;

	void Start() {
		_camera = GetComponent<Camera>();
	}
 
	void PreRenderAdjustFOV(Camera camera) {
		if (camera == _camera) {
			camera.fieldOfView = 45;
			transform.Translate(new Vector3(0, 0, -_moveBackZ), Space.Self);
		}
	}
 
	public void PreCullAdjustFOV(Camera camera) {
		if (camera == _camera) {
			camera.ResetWorldToCameraMatrix();
        	camera.ResetProjectionMatrix();
			camera.fieldOfView = 179;
			transform.Translate(new Vector3(0, 0, _moveBackZ), Space.Self);
		}
	}

	void OnEnable() {
		Camera.onPreCull += PreCullAdjustFOV;
		Camera.onPreRender += PreRenderAdjustFOV;
	}

	void OnDisable() {
		Camera.onPreCull -= PreCullAdjustFOV;
		Camera.onPreRender -= PreRenderAdjustFOV;
	}
}
