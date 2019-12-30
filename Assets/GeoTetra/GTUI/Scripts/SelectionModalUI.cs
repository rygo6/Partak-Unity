using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GeoTetra.GTUI
{
    public class SelectionModalUI : ModalUI
    {
        [SerializeField] private Text _messageText;
        [SerializeField] private GameObject _rootItem;

        private readonly List<GameObject> _itemList = new List<GameObject>();
        private Action[] _actions;

        protected override void Awake()
        {
            base.Awake();
            _rootItem.GetComponent<Button>().onClick.AddListener(() => { OnSelectionClick(0); });
            _itemList.Add(_rootItem);
        }

        private void OnSelectionClick(int index)
        {
            base.Close();
            _actions[index]();
        }

        public void Init(string mainMessage, string[] messages, Action[] actions, int focusIndex)
        {
            if (string.IsNullOrEmpty(mainMessage))
            {
                _messageText.gameObject.SetActive(false);
            }
            else
            {
                _messageText.gameObject.SetActive(true);
                _messageText.text = mainMessage;
            }
            
            for (int i = 0; i < messages.Length; ++i)
            {
                if (i >= _itemList.Count)
                {
                    _itemList.Add(Instantiate(_rootItem));
                    _itemList[i].GetComponent<RectTransform>().SetParent(_rootItem.transform.parent);
                    _itemList[i].GetComponent<RectTransform>().localScale = Vector3.one;
                    int index = i;
                    _itemList[i].GetComponent<Button>().onClick.AddListener(() => { OnSelectionClick(index); });

                    Navigation navDown = _itemList[i - 1].GetComponent<Button>().navigation;
                    navDown.selectOnDown = _itemList[i].GetComponent<Button>();
                    _itemList[i - 1].GetComponent<Button>().navigation = navDown;

                    Navigation navUp = _itemList[i].GetComponent<Button>().navigation;
                    navUp.selectOnDown = null;
                    navUp.selectOnUp = _itemList[i - 1].GetComponent<Button>();
                    _itemList[i].GetComponent<Button>().navigation = navUp;

                    if (i == focusIndex) _focusSelectable = _itemList[i].GetComponent<Button>();
                }

                _itemList[i].gameObject.SetActive(true);
                _itemList[i].GetComponentInChildren<Text>().text = messages[i];
            }

            for (int i = messages.Length; i < _itemList.Count; ++i)
            {
                _itemList[i].gameObject.SetActive(false);
            }

            _actions = actions;
        }
    }
}