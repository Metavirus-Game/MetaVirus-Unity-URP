using System;
using System.Collections;
using GameEngine.Base;
using UnityEngine;

namespace GameEngine.Event
{
    public class EventService : BaseService
    {
        public delegate void EmptyHandler();

        public delegate void Handler(params object[] args);

        public delegate void Handler<in T>(T arg);

        public delegate void Handler<in T1, in T2>(T1 arg1, T2 arg2);

        private Hashtable _listeners = new Hashtable();

        public void On<T1, T2>(string message, Handler<T1, T2> action)
        {
            if (_listeners[message] is Handler<T1, T2> actions)
            {
                _listeners[message] = actions + action;
            }
            else
            {
                _listeners[message] = action;
            }
        }

        public void Remove<T1, T2>(string message, Handler<T1, T2> action)
        {
            if (_listeners[message] is Handler<T1, T2> actions)
            {
                _listeners[message] = actions - action;
            }
        }

        public void Emit<T1, T2>(string message, T1 arg1, T2 arg2)
        {
            if (_listeners[message] is Handler<T1, T2> actions)
            {
                actions(arg1, arg2);
            }
        }

        public void On<T>(string message, Handler<T> action)
        {
            if (_listeners[message] is Handler<T> actions)
            {
                _listeners[message] = actions + action;
            }
            else
            {
                _listeners[message] = action;
            }
        }

        public void Remove<T>(string message, Handler<T> action)
        {
            if (_listeners[message] is Handler<T> actions)
            {
                _listeners[message] = actions - action;
            }
        }

        public void Emit<T>(string message, T arg)
        {
            if (_listeners[message] is Handler<T> actions)
            {
                try
                {
                    actions(arg);
                }
                catch (Exception e)
                {
                    Debug.LogError($"message:{message} call exception: {e.StackTrace}");
                }
            }
        }

        public void On(string message, Handler action)
        {
            if (_listeners[message] is Handler actions)
            {
                _listeners[message] = actions + action;
            }
            else
            {
                _listeners[message] = action;
            }
        }

        public void Remove(string message, Handler action)
        {
            if (_listeners[message] is Handler actions)
            {
                _listeners[message] = actions - action;
            }
        }

        public void Emit(string message, params object[] args)
        {
            var actions = _listeners[message] as Handler;
            actions?.Invoke(args);
        }

        public void On(string message, EmptyHandler action)
        {
            if (_listeners[message] is EmptyHandler actions)
            {
                _listeners[message] = actions + action;
            }
            else
            {
                _listeners[message] = action;
            }
        }

        public void Emit(string message)
        {
            var actions = _listeners[message] as EmptyHandler;
            actions?.Invoke();
        }

        public void Remove(string message, EmptyHandler action)
        {
            if (_listeners[message] is EmptyHandler actions)
            {
                _listeners[message] = actions - action;
            }
        }

        public void Clear()
        {
            _listeners = new Hashtable();
        }
    }
}