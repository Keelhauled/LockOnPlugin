using UnityEngine;
using UnityEngine.Events;

namespace LockOnPluginUtilities
{
    public class Hotkey
    {
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

        private bool GetModifiers() => Input.GetKey("left shift") || Input.GetKey("left alt") || Input.GetKey("left ctrl");
        private bool ShouldProc() => timeHeld >= procTime;
        private void AddTime() => timeHeld += Time.deltaTime;
        private void ResetTime() => timeHeld = 0.0f;

        public void KeyHoldAction(UnityAction action)
        {
            if(enabled && procTime > 0.0f && Input.GetKey(key) && !GetModifiers())
            {
                AddTime();
                if(ShouldProc() && released)
                {
                    released = false;
                    action();
                }
            }
        }

        public void KeyUpAction(UnityAction action)
        {
            if(enabled && Input.GetKeyUp(key) && !GetModifiers())
            {
                if(released)
                {
                    action();
                }

                ResetTime();
                released = true;
            }
        }

        public void KeyDownAction(UnityAction action)
        {
            if(enabled && Input.GetKeyDown(key) && !GetModifiers())
            {
                action();
                released = false;
            }
        }
    }
}