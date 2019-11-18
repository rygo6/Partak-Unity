using GeoTetra.GTCommon;

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEngine.EventSystems
{
    /// <summary>
    /// This is a special variation of the StandAloneInput module which allows you
    /// to switch the PointEventData stream to a new GameObject mid-drag.
    /// </summary>
    [AddComponentMenu("Event/Switch Standalone Input Module")]
    public class SwitchStandaloneInputModule : StandaloneInputModule
    {
        private static SwitchStandaloneInputModule instance;
        
        /// <summary>
        /// Call mid-drag to switch event stream to passed in GameObject. Call from an
        /// OnDrag callback and pass the PointerEventData from that OnDrag callback to this method.
        /// </summary>
        public static void SwitchToGameObject(GameObject gameObject, PointerEventData data)
        {
            if (instance == null) instance = FindObjectOfType<SwitchStandaloneInputModule>();
            if (instance.input.touchCount == 0 && instance.input.mousePresent)
                instance.DoMouseSwitch(gameObject, data);
            else
                instance.DoTouchSwitch(gameObject, data);
        }

        public void DoTouchSwitch(GameObject gameObject, PointerEventData data)
        {
            Debug.Log("Touch");
            Touch touch = input.GetTouch(data.pointerId);
            bool released;
            bool pressed;
            var pointer = GetTouchPointerEventData(touch, out pressed, out released);
            
            RaycastResult result = new RaycastResult();
            result.gameObject = gameObject;
            pointer.pointerCurrentRaycast = result;
            ProcessTouchEvent(pointer, false, true);

            pointer = GetTouchPointerEventData(touch, out pressed, out released);
            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                pointer.pointerPressRaycast = result;
                pointer.pointerCurrentRaycast = result;
                ProcessTouchEvent(pointer, true, false);
            }
        }
        
        private void ProcessTouchEvent(PointerEventData pointer, bool pressed, bool released)
        {
            ProcessTouchPress(pointer, pressed, released);

            if (!released)
            {
                ProcessMove(pointer);
                ProcessDrag(pointer);
            }
            else
                RemovePointerData(pointer);
        }

        private void DoMouseSwitch(GameObject gameObject, PointerEventData data)
        {
            Debug.Log("Mouse");
            var mouseData = GetMousePointerEventData();

            //send released, PointUp on prior object
            RaycastResult result = new RaycastResult();
            result.gameObject = gameObject;
            mouseData.SetButtonState(PointerEventData.InputButton.Left, PointerEventData.FramePressState.Released, mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData.buttonData);
            ProcessMouseEvent(mouseData);

            if (input.GetMouseButton(0))
            {
                mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData.buttonData.pointerPressRaycast = result;
                mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData.buttonData.pointerCurrentRaycast = result;
                mouseData.SetButtonState(PointerEventData.InputButton.Left, PointerEventData.FramePressState.Pressed, mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData.buttonData);
                ProcessMouseEvent(mouseData);
            }
        }

        private GameObject _currentFocusedGameObject;
        
        /// <summary>
        /// Process all mouse events.
        /// </summary>
        protected void ProcessMouseEvent(MouseState mouseData)
        {
            var leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

            _currentFocusedGameObject = leftButtonData.buttonData.pointerCurrentRaycast.gameObject;

            // Process the first mouse button fully
            ProcessMousePress(leftButtonData);
            ProcessMove(leftButtonData.buttonData);
            ProcessDrag(leftButtonData.buttonData);

            // Now process right / middle clicks
            ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData);
            ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
            ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
            ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);

            if (!Mathf.Approximately(leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f))
            {
                var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
                ExecuteEvents.ExecuteHierarchy(scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
            }
        }
        
        protected new GameObject GetCurrentFocusedGameObject()
        {
            return _currentFocusedGameObject;
        }
    }
}