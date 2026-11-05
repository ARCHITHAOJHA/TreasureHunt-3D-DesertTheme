using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DesertTreasureHunt
{
    public static class EventSystemHelper
    {
        public static void EnsureCorrectInputModule(GameObject esObj)
        {
            if (esObj == null) return;

            EventSystem es = esObj.GetComponent<EventSystem>();
            if (es == null) es = esObj.AddComponent<EventSystem>();

            Type inputSystemUIModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemUIModuleType != null)
            {
                // If New Input System is present, StandaloneInputModule will ignore mouse clicks.
                // We remove it and attach InputSystemUIInputModule instead!
                StandaloneInputModule oldMod = esObj.GetComponent<StandaloneInputModule>();
                if (oldMod != null)
                {
                    UnityEngine.Object.DestroyImmediate(oldMod);
                }

                if (esObj.GetComponent(inputSystemUIModuleType) == null)
                {
                    esObj.AddComponent(inputSystemUIModuleType);
                }
            }
            else
            {
                if (esObj.GetComponent<StandaloneInputModule>() == null)
                {
                    esObj.AddComponent<StandaloneInputModule>();
                }
            }
        }
    }
}
