using System;
using UnityEngine;
using UnityEngine.UI;

namespace Partak
{
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private RawImage _image;
        [SerializeField] private Button _button;
        [SerializeField] private Text _text;

        public event Action<LevelButton> ButtonClicked;
        
        public LevelUI.Level Level { get; set; }
        
        public Button Button => _button;
        public Text Text => _text;
        public RawImage Image => _image;

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            ButtonClicked?.Invoke(this);
        }

        public void SetLevel(LevelUI.Level level)
        {
            Level = level;
            _text.text = "";
            _image.texture = level?.PreviewImage;
            _button.interactable = level != null;
        }
    }
}