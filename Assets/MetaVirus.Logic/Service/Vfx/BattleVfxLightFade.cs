using System;
using UnityEngine;

namespace MetaVirus.Logic.Service.Vfx
{
    public class BattleVfxLightFade : MonoBehaviour
    {
        [Header("Seconds to dim the light")] public float life = 0.2f;

        public float TimeScale { get; set; } = 1;

        private Light _li;
        private float _initIntensity;

        // Use this for initialization
        void Start()
        {
            if (gameObject.GetComponent<Light>())
            {
                _li = gameObject.GetComponent<Light>();
                _initIntensity = _li.intensity;
            }
            else
                print("No light object found on " + gameObject.name);
        }

        private void OnEnable()
        {
            if (_li != null)
            {
                _li.enabled = true;
                _li.intensity = _initIntensity;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (gameObject.GetComponent<Light>())
            {
                _li.intensity -= _initIntensity * (Time.deltaTime * TimeScale / life);
                if (_li.intensity <= 0)
                    _li.enabled = false;
            }
        }
    }
}