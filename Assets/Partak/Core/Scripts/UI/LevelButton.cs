﻿using System;
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

        public event Action<LevelButton> ButtonClicked;

        public int Index0 { get; set; } = -1;
        public int Index1 { get; set; } = -1;
        
//        public bool ShowingLevel { get; set; }
        public LocalLevelDatum LevelDatum { get; set; }
        
        public Button Button => _button;
        public Text Text => _text;
        public RawImage Image => _image;
        public Text ThumbsUpText => _thumbsUpText;

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
            if (_image.texture  == null) _image.texture  = new Texture2D(0,0, TextureFormat.RGBA32, 0, false);
            byte[] imageBytes = System.IO.File.ReadAllBytes(path);
            Texture2D textured2d = (Texture2D)_image.texture;
            textured2d.LoadImage(imageBytes, true);;
        }

        public bool IsIndex(int index0, int index1)
        {
            return index0 == Index0 && index1 == Index1;
        }

        public void ShowRating(bool state)
        {
            _thumbsUpIcon.gameObject.SetActive(state);
            _thumbsUpText.gameObject.SetActive(state);
        }

        public void SetEmpty()
        {
            Index0 = -1;
            Index1 = -1;
            Text.text = "";
            _image.texture = null;
            _image.color = Color.gray;
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