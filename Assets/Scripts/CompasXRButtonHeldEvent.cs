using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CompasXR.UI
{
    public class CompasXRButtonHeldEvent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public bool isHeld = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            isHeld = true;
            Debug.Log("Button is being held down");
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isHeld = false;
            Debug.Log("Button released");
        }

        // void Update()
        // {
        //     if (isHeld)
        //     {
        //         // This runs every frame while the button is held
        //         Debug.Log("Still holding...");
        //         // You can run your logic here (e.g., charge power, move object, etc.)
        //     }
        // }
    }
}