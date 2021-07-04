using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;

namespace UnityEngine.InputSystem.OnScreen
{
    /// <summary>
    /// A button that is visually represented on-screen and triggered by touch or other pointer
    /// input.
    /// </summary>
    //[AddComponentMenu("Input/On-Screen Button")]
    public class OnScreenDrag : OnScreenControl,IDragHandler
    {
       // Vector2 dragPoint_before;

        public void OnDrag(PointerEventData eventData)
        {
           // Vector2 delta = eventData.position - dragPoint_before;
            SendValueToControl(eventData.delta);
            //dragPoint_before = eventData.position;
        }

        [InputControl(layout = "Vector2")]
        [SerializeField]
        private string m_ControlPath;

        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }
    }
}
