using System.Collections;
using GeoTetra.GTCommon.ScriptableObjects;
using UnityEngine;

namespace Partak
{
    public class CameraOrbit : MonoBehaviour
    {
        [SerializeField] private ComponentContainer _componentContainer;
        [SerializeField] private float _rotateMultiplier = 16f;
        [SerializeField] private float _tweenMainCameraDivider = 8f;
        private Transform childCameraTransform;

        private void Start()
        {
            childCameraTransform = transform.GetComponentsInChildren<Transform>()[1];
            _componentContainer.Get<CellParticleStore>().WinEvent += PlayerWin;
        }

        public void PlayerWin()
        {
            StartCoroutine(MainCameraTweenCoroutine());
            StartCoroutine(OrbitCoroutine());
        }

        private IEnumerator MainCameraTweenCoroutine()
        {
            Transform mainCameraTransform = Camera.main.transform;

            Vector3 startPos = mainCameraTransform.position;
            Quaternion startRot = mainCameraTransform.rotation;

            float time = 0.0f;
            while (time < 1.0f)
            {
                time += Time.deltaTime / _tweenMainCameraDivider;
                mainCameraTransform.position = Vector3.Lerp(startPos, childCameraTransform.position, time);
                mainCameraTransform.rotation = Quaternion.Slerp(startRot, childCameraTransform.rotation, time);
                yield return null;
            }

            while (true)
            {
                mainCameraTransform.position = childCameraTransform.position;
                mainCameraTransform.rotation = childCameraTransform.rotation;
                yield return null;
            }
        }

        private IEnumerator OrbitCoroutine()
        {
            while (true)
            {
                Vector3 newEuler = transform.localEulerAngles;
                newEuler.y += Time.deltaTime * _rotateMultiplier;
                transform.localEulerAngles = newEuler;
                yield return null;
            }
        }
    }
}