using System.Collections;
using System.IO;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine;
using UnityEngine.UI;

namespace GeoTetra.Partak.UI
{
    public class LevelGallery : MonoBehaviour
    {
        [SerializeField] private ServiceReference _gameState;
        [SerializeField] public Button _leftButton;
        [SerializeField] public Button _rightButton;
        [SerializeField] private float _transitionSpeed = 4f;
        
        private Material _material;

        private void Start()
        {
            _gameState.Service<GameState>().LevelIndex = PlayerPrefs.GetInt("LevelIndex");
            _leftButton.onClick.AddListener(GalleryLeft);
            _rightButton.onClick.AddListener(GalleryRight);
            _material = GetComponent<RawImage>().material;
            
            string imagePath = LevelUtility.LevelImagePath(_gameState.Service<GameState>().GetSelectedLevelId());
            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            Texture2D image = new Texture2D(0,0);
            image.LoadImage(imageBytes);
            
            _material.SetTexture("_Texture1", image);
        }

        private void GalleryRight()
        {
            StartCoroutine(GalleryTransitionCoroutine(1));
        }

        private void GalleryLeft()
        {
            StartCoroutine(GalleryTransitionCoroutine(-1));
        }

        private IEnumerator GalleryTransitionCoroutine(int direction)
        {
            _rightButton.interactable = false;
            _leftButton.interactable = false;
            _gameState.Service<GameState>().LevelIndex += direction;
            
            string imagePath = LevelUtility.LevelImagePath(_gameState.Service<GameState>().GetSelectedLevelId());
            if (!File.Exists(imagePath))
            {
                if (direction > 0)
                {
                    _gameState.Service<GameState>().LevelIndex = 0;
                }
                else if (direction < 0)
                {
                    _gameState.Service<GameState>().LevelIndex = GetMaxLevelIndex() - 1;
                }
                
                imagePath = LevelUtility.LevelImagePath(_gameState.Service<GameState>().GetSelectedLevelId());
            }
            
            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            Texture2D image = new Texture2D(0,0);
            image.LoadImage(imageBytes);

            PlayerPrefs.SetInt("LevelIndex", _gameState.Service<GameState>().LevelIndex);
            _material.SetTexture("_Texture2", image);
            yield return null;
            yield return null;
            Vector2 mainStart = new Vector2(0, 0);
            Vector2 mainEnd = new Vector2(direction, 0);
            Vector2 mainOffset;
            Vector2 texture2Start = new Vector2(mainEnd.x * -1, mainEnd.y);
            Vector2 texture2End = new Vector2(mainStart.x, mainStart.y);
            Vector2 texture2Offset;
            float fade;
            float time = 0.0f;
            while (time < 1.0f)
            {
                time += Time.deltaTime * _transitionSpeed;

                mainOffset = Vector2.Lerp(mainStart, mainEnd, time);
                texture2Offset = Vector2.Lerp(texture2Start, texture2End, time);

                fade = Mathf.Lerp(0, 1, time);

                _material.SetFloat("_Blend", fade);
                _material.SetTextureOffset("_Texture1", mainOffset);
                _material.SetTextureOffset("_Texture2", texture2Offset);

                yield return null;
            }
            
            _material.SetTexture("_Texture1", _material.GetTexture("_Texture2"));
            _material.SetFloat("_Blend", 0);
            _material.SetTextureOffset("_Texture1", mainStart);
            _material.SetTextureOffset("_Texture2", mainEnd);
            _material.SetTexture("_Texture2", null);
            _rightButton.interactable = true;
            _leftButton.interactable = true;
        }

        private int GetMaxLevelIndex()
        {
            return _gameState.Service<GameState>().LevelCatalogDatum.LevelIDs.Count;
        }

        private void OnDestroy()
        {
            _material.SetTexture("_Texture1", null);
            _material.SetTexture("_Texture2", null);
            _material.SetTextureOffset("_Texture1", Vector2.zero);
            _material.SetTextureOffset("_Texture2", Vector2.zero);
        }
    }
}