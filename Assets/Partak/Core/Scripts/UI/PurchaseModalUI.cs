﻿using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using GeoTetra.GTUI;
using UnityEngine.AddressableAssets;
using UnityEngine.Purchasing;

namespace GeoTetra.Partak
 {
	 public class PurchaseModalUI : ModalUI
    {
	    [SerializeField] private Button _purchaseButton;
        [SerializeField] private IAPButton _iapButton;
        [SerializeField] private AssetReference _mainUI;
        
        private Action _action;
 		
 		protected override void Awake()
 		{
 			base.Awake();
 			_purchaseButton.onClick.AddListener(PurchaseClicked);
            _iapButton.onPurchaseComplete.AddListener(PurchaseComplete);
            _iapButton.onPurchaseFailed.AddListener(PurchaseFail);
 		}
 
 		private void PurchaseClicked()
 		{
	        Group.blocksRaycasts = true;
        }

        private void PurchaseComplete(Product product)
        {
	        base.Close();
	        CurrentlyRenderedBy.Flush();
	        CurrentlyRenderedBy.InstantiateAndDisplayStackUI(_mainUI);
	        CurrentlyRenderedBy.DisplayMessageModal("Full Version Activated.");
        }

        private void PurchaseFail(Product product, PurchaseFailureReason reason)
        {
	        base.Close();
	        CurrentlyRenderedBy.DisplayMessageModal("Failure: " + reason.ToString());
        }
    }
 }