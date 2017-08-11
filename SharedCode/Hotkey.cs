using UnityEngine;
using UnityEngine.Events;

namespace LockOnPlugin
{
    public class Hotkey
    {
        public static bool allowHotkeys = true;
        public static bool inputFieldSelected = false;

        private string key = "";
        private float procTime = 0f;
        private float timeHeld = 0f;
        private bool released = true;
        private bool enabled = true;

        public Hotkey(string newKey, float newProcTime = 0f)
        {
            key = newKey.ToLower();

            if(key.Length < 1 || key == "false")
                enabled = false;

            if(newProcTime > 0f)
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

            if(enabled && procTime > 0f && Input.GetKey(key) && !GetModifiers())
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

                timeHeld = 0f;
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

            if(GUIUtility.keyboardControl > 0)
            {
                shouldReset = true;
            }
            
            if(inputFieldSelected)
            {
                shouldReset = true;
            }

            if(shouldReset)
            {
                timeHeld = 0f;
                released = true;
                return true;
            }

            return false;
        }
    }
}
