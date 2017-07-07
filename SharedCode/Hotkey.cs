using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LockOnPluginUtilities
{
    public class Hotkey
    {
        public static bool allowHotkeys = true;

        private string key = "";
        private float procTime = 0.0f;
        private float timeHeld = 0.0f;
        private bool released = true;
        private bool enabled = true;

        public Hotkey(string newKey, float newProcTime = 0.0f)
        {
            key = newKey.ToLower();

            if(key.Length < 1 || key == "false")
                enabled = false;

            if(newProcTime > 0.0f)
                procTime = newProcTime;
        }

        public void KeyDownAction(UnityAction action)
        {
            if(ResetIfShould()) return;

            if(enabled && Input.GetKeyDown(key) && !GetModifiers())
            {
                action();
                released = false;
            }
        }

        // this always needs at least KeyUpAction(null) after it
        public void KeyHoldAction(UnityAction action)
        {
            if(ResetIfShould()) return;

            if(enabled && procTime > 0.0f && Input.GetKey(key) && !GetModifiers())
            {
                timeHeld += Time.deltaTime;
                if(timeHeld >= procTime && released)
                {
                    action();
                    released = false;
                }
            }
        }

        public void KeyUpAction(UnityAction action)
        {
            if(ResetIfShould()) return;

            if(enabled && Input.GetKeyUp(key) && !GetModifiers())
            {
                if(released)
                {
                    action();
                }

                timeHeld = 0.0f;
                released = true;
            }
        }

        private bool GetModifiers()
        {
            return Input.GetKey("left shift") || Input.GetKey("left alt") || Input.GetKey("left ctrl");
        }

        private bool ResetIfShould()
        {
            bool shouldReset = false;

            if(!allowHotkeys)
            {
                shouldReset = true;
            }

            // this requires UnityEngine.UI.Translation.dll for some reason
            foreach(InputField inputField in GameObject.FindObjectsOfType<InputField>())
            {
                if(inputField.isFocused)
                {
                    shouldReset = true;
                    break;
                }
            }

            if(GUIUtility.keyboardControl > 0)
            {
                shouldReset = true;
            }

            // this only works in neo
            //if(Singleton<Studio.Studio>.Instance.isInputNow)
            //{
            //    shouldReset = true;
            //}

            if(shouldReset)
            {
                timeHeld = 0.0f;
                released = true;
                return true;
            }

            return false;
        }
    }
}