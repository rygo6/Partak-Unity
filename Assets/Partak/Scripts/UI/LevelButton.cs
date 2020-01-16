using System;
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
        public bool ShowingLevel { get; set; }
        
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

        public async Task DownloadAndDisplayLevelAsync(PartakDatabase partakDatabase, Document document, CancellationToken cancellationToken)
        {
            if (_image.texture == null || !(_image.texture is Texture2D)) _image.texture = new Texture2D(0,0, TextureFormat.RGBA32, 0, false);
            await partakDatabase.DownloadLevelPreview(document, Image.texture as Texture2D, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            Button.interactable = true;
            Text.text = "Download";
            Text.color = new Color(1, 1, 1, .1f);
            _image.color = Color.white;
        }

        public bool IsIndex(int index0, int index1)
        {
            return index0 == Index0 && index1 == Index1;
        }

        public void ShowRating(bool state)
        {
            _thumbsDownIcon.gameObject.SetActive(state);
            _thumbsDownText.gameObject.SetActive(state);
            _thumbsUpIcon.gameObject.SetActive(state);
            _thumbsUpText.gameObject.SetActive(state);
        }

        public void SetEmpty()
        {
            Index0 = -1;
            Index1 = -1;
            Text.text = "";
            ShowRating(false);
            Button.interactable = false;
        }
    }
}