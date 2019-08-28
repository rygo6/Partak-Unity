﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GeoTetra.GTUI
{
    public class BaseUI : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;

        [SerializeField] private Canvas _canvas;

        [SerializeField] private CanvasGroup _group;

        public RectTransform Root => _root;

        public Canvas Canvas => _canvas;

        public CanvasGroup Group => _group;

        public UIRenderer CurrentlyRenderedBy { get; set; }

        private void Reset()
        {
            _root = transform.Find("Root") as RectTransform;
            _canvas = GetComponentInChildren<Canvas>();
            _group = GetComponentInChildren<CanvasGroup>();
        }

        protected virtual void Awake()
        {
            gameObject.SetActive(false);
        }
    }
}