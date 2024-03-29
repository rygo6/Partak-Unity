﻿using System;
using System.Collections;
using System.Threading.Tasks;
using GeoTetra.GTBackend;
using UnityEngine;
using UnityEngine.UI;
using GeoTetra.GTPooling;
using GeoTetra.GTSnapper;
using GeoTetra.GTUI;
using UnityEngine.AddressableAssets;

namespace GeoTetra.Partak
{
    public class LevelCreateUI : StackUI
    {
        [SerializeField] private AnalyticsServiceRef _analyticsService;
        [SerializeField] private PartakStateRef _partakState;
        [SerializeField] private SceneTransitRef _sceneTransit;
        [SerializeField] private ComponentContainerRef _componentContainer;
        [SerializeField] private PartakAWSRef _partakAWS;
        [SerializeField] private AssetReference _mainMenuScene;
        [SerializeField] private AssetReference _newLevelScene;
        [SerializeField] private AssetReference _loadModalUI;
        [SerializeField] private ItemCatalogUI _itemCatalogUI;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _validateButton;

        private string[] _changeSizeMessages;
        private Action[] _changeSizeActions;
        
        private string[] _saveMessages;
        private Action[] _saveActions;

        private LevelConfig _levelConfig;
        private LevelTester _levelTester;
        private string _editingLevelId;

        protected override void Awake()
        {
            base.Awake();
            
            _changeSizeMessages = new[] {"128x128", "192x192", "256x144", "256x256"};
            _changeSizeActions = new Action[]
            {
                () => SizeChanged(new Vector2Int(128,128)), 
                () => SizeChanged(new Vector2Int(192,192)),
                () => SizeChanged(new Vector2Int(256,144)),
                () => SizeChanged(new Vector2Int(256,256))
            };
            
            _saveMessages = new[] {"Yes", "No"};
            _saveActions = new Action[]
            {
                SaveToAWS,
                SaveLevel,
            };
            
            _saveButton.onClick.AddListener(OnClickSave);
            _cancelButton.onClick.AddListener(OnCloseClick);
            _validateButton.onClick.AddListener(OnValidateClick);
        }

        protected override async Task StartAsync()
        {
            await Task.WhenAll(
                _componentContainer.Cache(this),
                _partakState.Cache(this),
                _sceneTransit.Cache(this),
                _partakAWS.Cache(this),
                _analyticsService.Cache(this)
            );
            await base.StartAsync();
        }

        public override async void OnTransitionInFinish()
        {
            base.OnTransitionInFinish();
            _levelConfig = await _componentContainer.AwaitRegister<LevelConfig>();
            _levelTester = await _componentContainer.AwaitRegister<LevelTester>();
            _itemCatalogUI.Initialize();

            _editingLevelId = _partakState.Ref.GetEditingLevelId();
            string levelPath = LevelUtility.LevelPath(_editingLevelId);
            
            if (!System.IO.File.Exists(levelPath))
            {
                CurrentlyRenderedBy.DisplaySelectionModal("Select Size", _changeSizeMessages, _changeSizeActions, 0);
            }
            else
            {
                _levelConfig.Deserialize(_editingLevelId, true);   
            }
        }

        public override void OnTransitionOutStart()
        {
            base.OnTransitionOutStart();
            _itemCatalogUI.Deinitialize();
        }

        private void OnClickSave()
        {
            StartCoroutine(OnClickSaveCoroutine());
        }

        private IEnumerator OnClickSaveCoroutine()
        {
            yield return StartCoroutine(RunTestCoroutine());
            if (_levelTester.Result == LevelTester.TestResult.Success)
            {
                // _levelConfig.Datum.Shared = false;
                // if (_levelConfig.Datum.Shared)
                // {
                //     SerializeLevel();
                //     OnCloseClick();
                // }
                // else
                // {
                    CurrentlyRenderedBy.DisplaySelectionModal("Share Level Online?", _saveMessages, _saveActions, 0);
                // }
            }
            else
            {
                DisplayFailMessages();
            }
        }
        
        private void OnCloseClick()
        {
            OnBackClicked();
            _sceneTransit.Ref.Load(_newLevelScene, _mainMenuScene);
            _analyticsService.Ref.CreateLevelCancelled();
        }

        private void SerializeLevel()
        {
            _levelConfig.Serialize(_editingLevelId, true);
            _partakState.Ref.AddLevelId(_editingLevelId);
        }

        private void SaveLevel()
        {
            SerializeLevel();
            OnCloseClick();
            _analyticsService.Ref.CreateLevelSaved();
        }
        
        private async void SaveToAWS()
        {
            SerializeLevel();

            if (_partakState.Ref.AllowLevelUpload())
            {
                _levelConfig.Datum.Shared = true;
                await _partakAWS.Ref.SaveLevel(_editingLevelId);
                _partakState.Ref.LastUploadedLevelTime = DateTime.Now;
                _analyticsService.Ref.CreateLevelUploaded();
                OnCloseClick();
            }
            else
            {
                CurrentlyRenderedBy.DisplayMessageModal(_partakState.Ref.FullVersion ? "You can only share a level every hour." : "You can only share a level every 24 hours.", OnCloseClick);
            }
        }

        private void OnValidateClick()
        {
            StartCoroutine(OnValidateClickCoroutine());
        }

        private IEnumerator OnValidateClickCoroutine()
        {
            yield return StartCoroutine(RunTestCoroutine());
            if (_levelTester.Result == LevelTester.TestResult.Success)
            {
                CurrentlyRenderedBy.DisplayMessageModal("Test Successful.", null);
            }
            else
            {
                DisplayFailMessages();
            }
        }

        private void DisplayFailMessages()
        {
            if (_levelTester.Result == LevelTester.TestResult.CursorsBlocked)
            {
                CurrentlyRenderedBy.DisplayMessageModal("An object is blocking the spawn position of a particle. Remove any objects beneath a player cursor.", null);
            }
            else if (_levelTester.Result == LevelTester.TestResult.SpawnBlocked)
            {
                CurrentlyRenderedBy.DisplayMessageModal("There is not enough room for all the particles to spawn. Move objects away from where the particles spawn.", null);
            } 
            else if (_levelTester.Result == LevelTester.TestResult.ParticlesBlocked)
            {
                CurrentlyRenderedBy.DisplayMessageModal("All of the particles could not reach each other. Ensure there are open paths for each player's particles to reach every other player's particles.", null);
            }
            else if (_levelTester.Result == LevelTester.TestResult.TooFewObjects)
            {
                CurrentlyRenderedBy.DisplayMessageModal($"Level has too few objects, add more. Minimum 3. Currently {_levelConfig.ItemRoot.ItemCount}", null);
            } 
            else if (_levelTester.Result == LevelTester.TestResult.TooManyObjects)
            {
                CurrentlyRenderedBy.DisplayMessageModal($"Level has too many objects, add more. Maximum 40. Currently {_levelConfig.ItemRoot.ItemCount}", null);
            }
        }
        
        private IEnumerator RunTestCoroutine()
        {
            CurrentlyRenderedBy.InstantiateAndDisplayModalUI(_loadModalUI);
            for (int i = 0; i < 4; ++i)
            {
                yield return StartCoroutine(_levelTester.RunTest(i));
                if (_levelTester.Result != LevelTester.TestResult.Success)
                {
                    break;
                }
            }
            CurrentlyRenderedBy.CloseModal();
        }

        private void SizeChanged(Vector2Int newSize)
        {
            _levelConfig.SetLevelSize(newSize, true);
        }
    }
}