using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace HenryLab
{
    [Serializable]
    public class BaseAction
    {
        [SerializeField] private string _name = "BaseAction";
        protected Action _callback = null;

        public BaseAction(Action callback = null)
        {
            _callback = callback;
        }

        public string Name { get => _name; set => _name = value; }
        public bool Succeeded { get; protected set; } = true;
        public string FailureReason { get; protected set; } = "";

        public virtual async Task Execute()
        {
            Succeeded = true;
            FailureReason = "";
            Debug.Log(new RichText().Add("Action: ").Add(Name, color: new Color(0f, 0.5f, 1f)));
            await Task.Yield();
            _callback?.Invoke();
        }

        protected void MarkFailed(string reason)
        {
            Succeeded = false;
            FailureReason = reason;
        }

        protected void SafeInvokeEvents(IEnumerable<UnityEvent> events)
        {
            foreach(var unityEvent in events)
            {
                try
                {
                    unityEvent?.Invoke();
                }
                catch(Exception ex)
                {
                    Debug.LogError(ex);
                }
            }
        }
    }
}