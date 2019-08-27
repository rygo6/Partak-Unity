using UnityEngine;

namespace Partak
{
    public class PatternTexture : MonoBehaviour
    {
        private const int TextureSize = 256;

        private const int Divisions = 8;

        private const int SquareSize = TextureSize / Divisions;

        private readonly Color[] PixelColors = new Color[SquareSize * SquareSize];
        [SerializeField] private Material _applyMaterial;

        private MenuConfig _playerSettings;

        private Color _testColor = Color.black;

        private Texture2D _texture2D;

        private void Start()
        {
            _texture2D = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false, false);
            _applyMaterial.mainTexture = _texture2D;
            _playerSettings = Persistent.Get<MenuConfig>();
            SetTexture();
        }

        private void Update()
        {
            if (!_testColor.RGBEquals(_playerSettings.PlayerColors[0]))
            {
                _testColor = _playerSettings.PlayerColors[0];
                SetTexture();
            }
        }

        private void SetTexture()
        {
//            Debug.Log("SetTexture " + _playerSettings.PlayerColors[0]) ;
            bool xblack = false;
            bool yBlack = false;
            int playerIndex = 0;
            for (int y = 0; y < TextureSize; y += SquareSize)
            {
                for (int x = 0; x < TextureSize; x += SquareSize)
                {
                    if (yBlack)
                    {
                        SetPixelColors(Color.black);
                    }
                    else if (xblack)
                    {
                        SetPixelColors(Color.black);
                        xblack = false;
                    }
                    else
                    {
                        SetPixelColors(_playerSettings.PlayerColors[playerIndex]);
                        playerIndex++;
                        if (playerIndex == _playerSettings.PlayerColors.Length)
                            playerIndex = 0;
                        xblack = true;
                    }

                    _texture2D.SetPixels(x, y, SquareSize, SquareSize, PixelColors);
                }

                if (yBlack)
                {
                    yBlack = false;
                }
                else
                {
                    yBlack = true;
                    if (xblack)
                        xblack = false;
                    else
                        xblack = true;
                }
            }

            _texture2D.Apply();
        }

        private void SetPixelColors(Color color)
        {
            for (var i = 0; i < PixelColors.Length; ++i) PixelColors[i] = color;
        }
    }
}