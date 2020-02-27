using System;
using System.Collections;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine;

namespace GeoTetra.Partak
{
    public class CameraOrbit : MonoBehaviour
    {
        [SerializeField] private CellParticleStore _cellParticleStore;
        [SerializeField] private float _rotateMultiplier = 16f;
        [SerializeField] private float _tweenMainCameraDivider = 8f;
        [SerializeField] private Transform _childCameraTransform;
        [SerializeField] private Camera _mainCamera;

        private void Start()
        {
            _cellParticleStore.WinEvent += PlayerWin;
        }

        public void SetSize(Vector2Int newSize)
        {
            Vector3 cameraPos = new Vector3((newSize.x / 2f)/ 10f, 0, (newSize.y / 2f)/ 10f);
        }
        
        private void PlayerWin()
        {
            StartCoroutine(MainCameraTweenCoroutine());
            StartCoroutine(OrbitCoroutine());
        }

        private IEnumerator MainCameraTweenCoroutine()
        {
            Vector3 startPos = _mainCamera.transform.position;
            Quaternion startRot = _mainCamera.transform.rotation;

            float time = 0.0f;
            while (time < 1.0f)
            {
                time += Time.deltaTime / _tweenMainCameraDivider;
                _mainCamera.transform.position = Vector3.Lerp(startPos, _childCameraTransform.position, time);
                _mainCamera.transform.rotation = Quaternion.Slerp(startRot, _childCameraTransform.rotation, time);
                yield return null;
            }

            while (true)
            {
                _mainCamera.transform.position = _childCameraTransform.position;
                _mainCamera.transform.rotation = _childCameraTransform.rotation;
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