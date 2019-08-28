﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GeoTetra.GTUI
{
    public class ModalSelectionUI : BaseUI
    {
        [SerializeField] private GameObject _rootItem;

        private readonly List<GameObject> ItemList = new List<GameObject>();
        private Action[] _actions;

        protected override void Awake()
        {
            base.Awake();
            _rootItem.GetComponent<Button>().onClick.AddListener(() => { Close(0); });
            ItemList.Add(_rootItem);
        }

        public void Close(int index)
        {
            _actions[index]();
            CurrentlyRenderedBy.CloseModal();
        }

        public void Init(string[] messages, Action[] actions, int focusIndex)
        {
            for (int i = 0; i < messages.Length; ++i)
            {
                if (i >= ItemList.Count)
                {
                    ItemList.Add(Instantiate(_rootItem));
                    ItemList[i].GetComponent<RectTransform>().SetParent(_rootItem.transform.parent);
                    ItemList[i].GetComponent<RectTransform>().localScale = Vector3.one;
                    int index = i;
                    ItemList[i].GetComponent<Button>().onClick.AddListener(() => { Close(index); });

                    Navigation navDown = ItemList[i - 1].GetComponent<Button>().navigation;
                    navDown.selectOnDown = ItemList[i].GetComponent<Button>();
                    ItemList[i - 1].GetComponent<Button>().navigation = navDown;

                    Navigation navUp = ItemList[i].GetComponent<Button>().navigation;
                    navUp.selectOnDown = null;
                    navUp.selectOnUp = ItemList[i - 1].GetComponent<Button>();
                    ItemList[i].GetComponent<Button>().navigation = navUp;
                }

                ItemList[i].GetComponentInChildren<Text>().text = messages[i];
            }

            _actions = actions;
        }
    }
}