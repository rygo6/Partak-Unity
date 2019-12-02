using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Partak
{
    public class ColorScroll : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [SerializeField] private ServiceReference _gameState;
        [SerializeField] private Material[] setMaterialColors;

        private RawImage _rawImage;
        private Texture2D _texture;

        public void OnBeginDrag(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
            Scroll(eventData.delta.x / -1000f);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            PlayerPrefs.SetFloat("ColorScrollX", _rawImage.uvRect.x);
        }

        private void Start()
        {
            _rawImage = GetComponent<RawImage>();
            _texture = (Texture2D) _rawImage.texture;
            var newRect = _rawImage.uvRect;
            newRect.x = PlayerPrefs.GetFloat("ColorScrollX", -.125f);
            _rawImage.uvRect = newRect;
            SetColors();
        }

        public void StartScrollRight()
        {
            InvokeRepeating(nameof(ScrollRight), 0f, 0.02f);
        }

        public void StartScrollLeft()
        {
            InvokeRepeating(nameof(ScrollLeft), 0f, 0.02f);
        }

        public void StopScroll()
        {
            CancelInvoke(nameof(ScrollRight));
            CancelInvoke(nameof(ScrollLeft));
        }

        public void ScrollRight()
        {
            Scroll(-0.002f);
        }

        public void ScrollLeft()
        {
            Scroll(0.002f);
        }

        private void Scroll(float amount)
        {
            var newRect = _rawImage.uvRect;
            newRect.x += amount;
            _rawImage.uvRect = newRect;
            SetColors();
        }

        private void SetColors()
        {
            var u = 0.125f + _rawImage.uvRect.x;
            for (var i = 0; i < _gameState.Service<GameState>().PlayerCount(); ++i)
            {
                _gameState.Service<GameState>().PlayerStates[i].PlayerColor = _texture.GetPixelBilinear(u, .5f);
                setMaterialColors[i].color = _gameState.Service<GameState>().PlayerStates[i].PlayerColor;
                u += .25f;
            }
        }
    }
}