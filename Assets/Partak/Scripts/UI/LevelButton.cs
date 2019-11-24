using System;
using UnityEngine;
using UnityEngine.UI;
using GeoTetra.GTUI;

namespace Partak
{
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private Button _button;
        [SerializeField] private Text _text;

        public event Action<LevelButton> ButtonClicked;
        
        public Button Button => _button;
        public Text Text => _text;
        public Image Image => _image;

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            ButtonClicked?.Invoke(this);
        }
    }
}