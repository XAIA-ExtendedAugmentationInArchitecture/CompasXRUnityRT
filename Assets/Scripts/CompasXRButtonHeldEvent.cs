using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CompasXR.UI
{
    public class CompasXRButtonHeldEvent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public bool isHeld = false;
        private float vibrationInterval = 0.3f; // seconds between buzzes
        private float timer = 0f;

        public bool vibrate = true;
        public bool ignore = false;

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

        void Update()
        {
            if (isHeld && vibrate)
            {
                if (!ignore)
                {
                    timer += Time.deltaTime;

                    if (timer >= vibrationInterval)
                    {
                        Vibrate();
                        timer = 0f;
                    }
                }
            }
        }

        private void Vibrate()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
                    Handheld.Vibrate();  // or your custom Android vibrator
            #endif
            Debug.Log("Vibrate : Buzz!");
        }
    }
}