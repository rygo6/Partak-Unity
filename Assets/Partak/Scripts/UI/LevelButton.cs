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
        
        public event Action<LevelButton> ButtonClicked;

        public int Index0 { get; set; } = -1;
        public int Index1 { get; set; } = -1;
        public bool Level { get; set; }
        
        public Button Button => _button;
        public Text Text => _text;
        public RawImage Image => _image;

        private Task _downloadLevelTask;
        private CancellationTokenSource _downloadLevelCancellationTokenSource;

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

//        public void Initialize(int index)
//        {
//            Index = index;
//        }

        private void OnButtonClick()
        {
            ButtonClicked?.Invoke(this);
        }

        public async Task DownloadAndDisplayLevelAsync(PartakDatabase partakDatabase, Document document)
        {
            if (_downloadLevelCancellationTokenSource != null)
            {
                if (!_downloadLevelCancellationTokenSource.IsCancellationRequested)
                {
                    Debug.Log("Cancelling");
                    _downloadLevelCancellationTokenSource.Cancel();
                }
                _downloadLevelCancellationTokenSource.Dispose();
            } 
            _downloadLevelCancellationTokenSource = new CancellationTokenSource();
            if (_image.texture == null || !(_image.texture is Texture2D)) _image.texture = new Texture2D(0,0, TextureFormat.RGBA32, 0, false);
            await partakDatabase.DownloadLevelImage(document, Image.texture as Texture2D, _downloadLevelCancellationTokenSource.Token);
            Button.interactable = true;
            Text.text = "Download";
            Text.color = new Color(1, 1, 1, .1f);
            _image.color = Color.white;
            _downloadLevelCancellationTokenSource = null;
        }

        public bool IsIndex(int index0, int index1)
        {
            return index0 == Index0 && index1 == Index1;
        }

        public void SetEmpty()
        {
            Index0 = -1;
            Index1 = -1;
            Text.text = "";
            Button.interactable = false;
        }
    }
}