using System;
using System.Reflection;
using UnityEngine;

namespace DesertTreasureHunt
{
    public static class DesertInput
    {
        private static bool isInitialized = false;
        private static bool hasInputSystem = false;
        private static PropertyInfo keyboardCurrentProp;
        private static PropertyInfo mouseCurrentProp;

        private static void Init()
        {
            if (isInitialized) return;
            isInitialized = true;

            try
            {
                Type kbType = Type.GetType("UnityEngine.InputSystem.Keyboard, Unity.InputSystem");
                if (kbType != null)
                {
                    keyboardCurrentProp = kbType.GetProperty("current", BindingFlags.Public | BindingFlags.Static);
                    Type mouseType = Type.GetType("UnityEngine.InputSystem.Mouse, Unity.InputSystem");
                    if (mouseType != null)
                    {
                        mouseCurrentProp = mouseType.GetProperty("current", BindingFlags.Public | BindingFlags.Static);
                    }
                    hasInputSystem = (keyboardCurrentProp != null);
                }
            }
            catch { }
        }

        public static bool GetLeft()
        {
            Init();
            if (CheckLegacyKey(KeyCode.LeftArrow) || CheckLegacyKey(KeyCode.A)) return true;
            return CheckNewKey("leftArrowKey") || CheckNewKey("aKey");
        }

        public static bool GetRight()
        {
            Init();
            if (CheckLegacyKey(KeyCode.RightArrow) || CheckLegacyKey(KeyCode.D)) return true;
            return CheckNewKey("rightArrowKey") || CheckNewKey("dKey");
        }

        public static bool GetJump()
        {
            Init();
            if (CheckLegacyKey(KeyCode.UpArrow) || CheckLegacyKey(KeyCode.W) || CheckLegacyKey(KeyCode.Space)) return true;
            return CheckNewKey("upArrowKey") || CheckNewKey("wKey") || CheckNewKey("spaceKey");
        }

        public static bool GetSlide()
        {
            Init();
            if (CheckLegacyKey(KeyCode.DownArrow) || CheckLegacyKey(KeyCode.S)) return true;
            return CheckNewKey("downArrowKey") || CheckNewKey("sKey");
        }

        public static bool GetPause()
        {
            Init();
            if (CheckLegacyKey(KeyCode.Escape) || CheckLegacyKey(KeyCode.P)) return true;
            return CheckNewKey("escapeKey") || CheckNewKey("pKey");
        }

        private static bool CheckLegacyKey(KeyCode code)
        {
            try
            {
                return Input.GetKeyDown(code);
            }
            catch
            {
                return false;
            }
        }

        private static bool CheckNewKey(string propName)
        {
            if (!hasInputSystem || keyboardCurrentProp == null) return false;
            try
            {
                object kb = keyboardCurrentProp.GetValue(null);
                if (kb == null) return false;
                PropertyInfo keyProp = kb.GetType().GetProperty(propName);
                if (keyProp == null) return false;
                object keyObj = keyProp.GetValue(kb);
                if (keyObj == null) return false;
                PropertyInfo pressedProp = keyObj.GetType().GetProperty("wasPressedThisFrame");
                if (pressedProp != null)
                {
                    return (bool)pressedProp.GetValue(keyObj);
                }
            }
            catch { }
            return false;
        }

        public static bool GetPointerDown(out Vector2 pos)
        {
            pos = Vector2.zero;
            Init();
            try
            {
                if (Input.GetMouseButtonDown(0))
                {
                    pos = Input.mousePosition;
                    return true;
                }
            }
            catch { }

            if (hasInputSystem && mouseCurrentProp != null)
            {
                try
                {
                    object mouse = mouseCurrentProp.GetValue(null);
                    if (mouse != null)
                    {
                        PropertyInfo btnProp = mouse.GetType().GetProperty("leftButton");
                        if (btnProp != null)
                        {
                            object btn = btnProp.GetValue(mouse);
                            PropertyInfo pressedProp = btn.GetType().GetProperty("wasPressedThisFrame");
                            if (pressedProp != null && (bool)pressedProp.GetValue(btn))
                            {
                                PropertyInfo posProp = mouse.GetType().GetProperty("position");
                                object posControl = posProp.GetValue(mouse);
                                MethodInfo readMethod = posControl.GetType().GetMethod("ReadValue");
                                pos = (Vector2)readMethod.Invoke(posControl, null);
                                return true;
                            }
                        }
                    }
                }
                catch { }
            }
            return false;
        }

        public static bool GetPointerUp(out Vector2 pos)
        {
            pos = Vector2.zero;
            Init();
            try
            {
                if (Input.GetMouseButtonUp(0))
                {
                    pos = Input.mousePosition;
                    return true;
                }
            }
            catch { }

            if (hasInputSystem && mouseCurrentProp != null)
            {
                try
                {
                    object mouse = mouseCurrentProp.GetValue(null);
                    if (mouse != null)
                    {
                        PropertyInfo btnProp = mouse.GetType().GetProperty("leftButton");
                        if (btnProp != null)
                        {
                            object btn = btnProp.GetValue(mouse);
                            PropertyInfo relProp = btn.GetType().GetProperty("wasReleasedThisFrame");
                            if (relProp != null && (bool)relProp.GetValue(btn))
                            {
                                PropertyInfo posProp = mouse.GetType().GetProperty("position");
                                object posControl = posProp.GetValue(mouse);
                                MethodInfo readMethod = posControl.GetType().GetMethod("ReadValue");
                                pos = (Vector2)readMethod.Invoke(posControl, null);
                                return true;
                            }
                        }
                    }
                }
                catch { }
            }
            return false;
        }
    }
}
