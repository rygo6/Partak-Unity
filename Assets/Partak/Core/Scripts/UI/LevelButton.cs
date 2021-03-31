using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using GeoTetra.GTBackend;
using GeoTetra.GTCommon.Components;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace GeoTetra.Partak
{
    public class LevelButton : SubscribableBehaviour
    {
        [SerializeField] private RawImage _image;
        [SerializeField] private Button _button;
        [SerializeField] private Text _text;

        [SerializeField] private Image _thumbsUpIcon;
        [SerializeField] private Text _thumbsUpText;
        
        [SerializeField] private Image _thumbsDownIcon;
        [SerializeField] private Text _thumbsDownText;

        public event Action<LevelButton> ButtonClicked;

        public int Index0 { get; set; } = -1;
        public int Index1 { get; set; } = -1;
        
//        public bool ShowingLevel { get; set; }
        public LocalLevelDatum LevelDatum { get; set; }
        
        public Button Button => _button;
        public Text Text => _text;
        public RawImage Image => _image;
        public Text ThumbsUpText => _thumbsUpText;
        public Text ThumbsDownText => _thumbsDownText;

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClick);
            ShowRating(false);
        }

        private void OnButtonClick()
        {
            ButtonClicked?.Invoke(this);
        }
        
        public void LoadTextureFromDisk(string path)
        {
            if (_image.texture  == null) _image.texture = new Texture2D(0,0, TextureFormat.RGBA32, 0, false);
            byte[] imageBytes = System.IO.File.ReadAllBytes(path);
            Texture2D textured2d = (Texture2D)_image.texture;
            textured2d.LoadImage(imageBytes, true);;
            SizeImageFromRatio();
        }

        public void SizeImageFromRatio()
        {
            if (_image.texture == null || _image.texture.width == 0 || _image.texture.height == 0) return;
            
            _image.SetNativeSize();
            float scaleSize = 1f;
            float frameRatio = RectTransform.rect.width / RectTransform.rect.height;
            float imageRatio = (float)_image.texture.width / (float)_image.texture.height;

            if (frameRatio > imageRatio)
            {
                scaleSize = RectTransform.rect.height / _image.texture.height;
            }
            else
            {
                scaleSize = RectTransform.rect.width / _image.texture.width;
            }

            _image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _image.rectTransform.rect.width * scaleSize);
            _image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _image.rectTransform.rect.height * scaleSize);
        }

        public bool IsIndex(int index0, int index1)
        {
            return index0 == Index0 && index1 == Index1;
        }

        public void ShowRating(bool state)
        {
            _thumbsUpIcon.gameObject.SetActive(state);
            _thumbsUpText.gameObject.SetActive(state);
            _thumbsDownIcon.gameObject.SetActive(state);
            _thumbsDownText.gameObject.SetActive(state);
        }

        public void SetEmpty()
        {
            Index0 = -1;
            Index1 = -1;
            Text.text = "";
            _image.texture = null;
            _image.color = Color.clear;
            LevelDatum = null;
            ShowRating(false);
            Button.interactable = false;
        }

        public int TotalIndex(int collumnCount)
        {
            return (Index0 * collumnCount) + Index1;
        }
    }
}