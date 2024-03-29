﻿using System;
using System.Collections;
using System.Threading.Tasks;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine;

namespace GeoTetra.Partak
{
    public class PatternTexture : SubscribableBehaviour
    {
        [SerializeField] private PartakStateRef _partakState;
        [SerializeField] private Material _applyMaterial;
        private const int TextureSize = 256;
        private const int Divisions = 8;
        private const int SquareSize = TextureSize / Divisions;
        private readonly Color[] PixelColors = new Color[SquareSize * SquareSize];
        private Texture2D _texture2D;
        private Coroutine _updateCoroutine;

        protected override async Task StartAsync()
        {
            _texture2D = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false, false);
            _applyMaterial.mainTexture = _texture2D;
            
            await _partakState.Cache(this);
            
            foreach (PartakState.PlayerState playerState in _partakState.Ref.PlayerStates)
            {
                playerState.ColorChanged += UpdateTexture;
            }
            StartCoroutine(SetTexture());

            await base.StartAsync();
        }

        protected override void OnDestroy()
        {
            foreach (PartakState.PlayerState playerState in _partakState.Ref.PlayerStates)
            {
                playerState.ColorChanged -= UpdateTexture;
            }
            base.OnDestroy();
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
                        SetPixelColors(_partakState.Ref.PlayerStates[playerIndex].PlayerColor);
                        playerIndex++;
                        if (playerIndex == _partakState.Ref.PlayerCount())
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