using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Partak {
public class PopupSelectionUI : MonoBehaviour {

	readonly List<GameObject> ItemList = new List<GameObject>();
	[SerializeField] GameObject _rootItem;
	Canvas _canvas;
	Camera _camera;
	Action[] _actions;

	void Awake() {
		_rootItem.GetComponent<Button>().onClick.AddListener(() => {
			Close(0);
		});
		ItemList.Add(_rootItem);
		_canvas = GetComponentInChildren<Canvas>();
		_camera = GetComponentInChildren<Camera>();
		_canvas.enabled = false;
		_camera.gameObject.SetActive(false);
	}

	public void Close(int index) {
		_actions[index]();
		_canvas.enabled = false;
		_camera.gameObject.SetActive(false);
	}

	public void Show(string[] messages, Action[] actions, int focusIndex) {
		for (int i = 0; i < messages.Length; ++i) {
			if (i < ItemList.Count) {

			} else {
				ItemList.Add(Instantiate(_rootItem));
				ItemList[i].GetComponent<RectTransform>().SetParent(_rootItem.transform.parent);
				ItemList[i].GetComponent<RectTransform>().localScale = Vector3.one;
				int index = i;
				ItemList[i].GetComponent<Button>().onClick.AddListener(() => {
					Close(index);
				});

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
		_canvas.enabled = true;
		_camera.gameObject.SetActive(true);
//		StartCoroutine(ShowCoroutine(focusIndex));
	}

//	IEnumerator ShowCoroutine(int focusIndex) {
//		yield return null;
//		yield return new WaitForSeconds(.4f);
//		_camera.gameObject.SetActive(true);
//		Persistent.Get<EventSystem>().SetSelectedGameObject(null);
//		yield return new WaitForSeconds(.4f);
//		ItemList[focusIndex].GetComponent<Button>().Select();
//	}
}
}