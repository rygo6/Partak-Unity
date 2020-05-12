using System;
using System.Collections;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine;

namespace GeoTetra.Partak
{
    public class PatternTexture : MonoBehaviour
    {
        [SerializeField] private GameStateReference _gameState;
        [SerializeField] private Material _applyMaterial;
        private const int TextureSize = 256;
        private const int Divisions = 8;
        private const int SquareSize = TextureSize / Divisions;
        private readonly Color[] PixelColors = new Color[SquareSize * SquareSize];
        private Texture2D _texture2D;
        private Coroutine _updateCoroutine;

        private void Start()
        {
            _texture2D = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false, false);
            _applyMaterial.mainTexture = _texture2D;
            foreach (GameState.PlayerState playerState in _gameState.Service.PlayerStates)
            {
                playerState.ColorChanged += UpdateTexture;
            }
            StartCoroutine(SetTexture());
        }

        private void OnDestroy()
        {
            foreach (GameState.PlayerState playerState in _gameState.Service.PlayerStates)
            {
                playerState.ColorChanged -= UpdateTexture;
            }
        }

        private void UpdateTexture(Color color)
        {
            if (_updateCoroutine == null)
            {
                _updateCoroutine = StartCoroutine(SetTexture());
            }
        }

        private IEnumerator SetTexture()
        {
            yield return null;
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
                        SetPixelColors(_gameState.Service.PlayerStates[playerIndex].PlayerColor);
                        playerIndex++;
                        if (playerIndex == _gameState.Service.PlayerCount())
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

            _updateCoroutine = null;
        }

        private void SetPixelColors(Color color)
        {
            for (var i = 0; i < PixelColors.Length; ++i) PixelColors[i] = color;
        }
    }
}