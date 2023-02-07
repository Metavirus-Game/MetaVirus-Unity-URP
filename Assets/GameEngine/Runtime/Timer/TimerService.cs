using System;
using System.Collections.Generic;
using GameEngine.Base;
using UnityEngine;

namespace GameEngine.Timer
{
    public class TimerService : BaseService
    {
        private class Timer
        {
            public readonly int ID;
            private bool _isActive;

            private readonly float _rate;
            private readonly int _ticks;
            private int _ticksElapsed;
            private float _last;
            private readonly Action _callBack;

            private readonly TimerService _timerService;

            public Timer(int id, float rate, int ticks, Action callback, TimerService timerService)
            {
                ID = id;
                _rate = rate < 0 ? 0 : rate;
                _ticks = ticks < 0 ? 0 : ticks;
                _callBack = callback;
                _last = 0;
                _ticksElapsed = 0;
                _isActive = true;
                _timerService = timerService;
            }

            public void Tick(float deltaTime)
            {
                _last += deltaTime;

                if (_isActive && _last >= _rate)
                {
                    _last = 0;
                    _ticksElapsed++;
                    _callBack.Invoke();

                    if (_ticks > 0 && _ticks == _ticksElapsed)
                    {
                        _isActive = false;
                        _timerService.RemoveTimer(ID);
                    }
                }
            }
        }


        // Timer objects
        private List<Timer> _timers;

        // Timer removal queue
        private List<int> _removalPending;

        private int _idCounter;

        public override void PostConstruct()
        {
            _timers = new List<Timer>();
            _removalPending = new List<int>();
        }

        /// <summary>
        /// 延迟指定时间调用callback
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public int InvokeDelay(float delay, Action callback)
        {
            return AddTimer(delay, 1, callback);
        }

        /// <summary>
        /// Creates new timer
        /// </summary>
        /// <param name="rate">Tick rate</param>
        /// <param name="callBack">Callback method</param>
        /// <returns>Timer Id</returns>
        public int AddTimer(float rate, Action callBack)
        {
            return AddTimer(rate, 0, callBack);
        }

        /// <summary>
        /// Creates new timer
        /// </summary>
        /// <param name="rate">Tick rate</param>
        /// <param name="ticks">Number of ticks before timer removal</param>
        /// <param name="callBack">Callback method</param>
        /// <returns>Timer id</returns>
        public int AddTimer(float rate, int ticks, Action callBack)
        {
            var newTimer = new Timer(++_idCounter, rate, ticks, callBack, this);
            _timers.Add(newTimer);
            return newTimer.ID;
        }

        /// <summary>
        /// Removes timer
        /// </summary>
        public void RemoveTimer(int timerId)
        {
            _removalPending.Add(timerId);
        }

        /// <summary>
        /// Timer removal queue handler
        /// </summary>
        void Remove()
        {
            if (_removalPending.Count > 0)
            {
                foreach (var id in _removalPending)
                    for (var i = 0; i < _timers.Count; i++)
                        if (_timers[i].ID == id)
                        {
                            _timers.RemoveAt(i);
                            break;
                        }

                _removalPending.Clear();
            }
        }

        private void Tick(float deltaTime)
        {
            foreach (var t in _timers)
            {
                t.Tick(deltaTime);
            }
        }

        public override void OnUpdate(float elapseTime, float realElapseTime)
        {
            if (!Ready)
            {
                return;
            }

            Remove();
            Tick(elapseTime);
        }
    }
}