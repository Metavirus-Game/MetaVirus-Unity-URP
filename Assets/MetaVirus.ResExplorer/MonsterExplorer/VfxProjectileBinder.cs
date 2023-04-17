using System;
using GameEngine;
using MetaVirus.Logic.Service.Vfx;
using UnityEngine;

namespace MetaVirus.ResExplorer.MonsterExplorer
{
    public class VfxProjectileBinder : MonoBehaviour
    {
        public float lifetime = 2;
        public float speed = 10;
        public int bindVfxId;

        private BattleVfxGameService _battleVfxGameService;
        private Vector3 _targetPos;
        private GameObject _vfxObject;

        private void Start()
        {
            _battleVfxGameService = GameFramework.GetService<BattleVfxGameService>();
            _targetPos = transform.position + transform.forward * 100;
            _vfxObject = _battleVfxGameService.InstanceVfx(bindVfxId, gameObject, autoDestroy: false);
        }

        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, speed * Time.deltaTime);
            lifetime -= Time.deltaTime;
            if (lifetime < 0)
            {
                Destroy(gameObject);
                _battleVfxGameService.ReleaseVfxInst(_vfxObject);
            }
        }
    }
}