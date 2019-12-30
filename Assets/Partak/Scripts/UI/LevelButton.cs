using System;
using UnityEngine;
using UnityEngine.UI;

namespace GeoTetra.Partak
{
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private RawImage _image;
        [SerializeField] private Button _button;
        [SerializeField] private Text _text;

        public int Index { get; private set; }
        
        public event Action<LevelButton> ButtonClicked;
        
        public bool Level { get; set; }
        
        public Button Button => _button;
        public Text Text => _text;
        public RawImage Image => _image;

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        public void Initialize(int index)
        {
            Index = index;
        }

        private void OnButtonClick()
        {
            ButtonClicked?.Invoke(this);
        }

//        public void SetLevel(LevelUI.Level level)
//        {
//            Level = level;
//            _text.text = "";
//            _image.texture = level?.PreviewImage;
//            _button.interactable = level != null;
//        }
    }
}