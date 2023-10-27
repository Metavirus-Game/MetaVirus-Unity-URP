using System.Collections.Generic;
using System.Threading.Tasks;
using cfg.battle;
using cfg.skill;
using GameEngine;
using GameEngine.ObjectPool;
using MetaVirus.Battle.Record;
using MetaVirus.Logic.Service.Battle.data;
using MetaVirus.Logic.Service.Vfx;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Service.Battle.Projectile
{
    public class BattleProjectileManager
    {
        /// <summary>
        /// key = npc_resource_data.id
        /// value = projectile object instance
        /// 战斗中用到的投射物体实例
        /// </summary>
        private readonly Dictionary<int, GameObject> _projectileObjs = new();

        private readonly Transform _projectileRoot;

        private readonly BattleVfxGameService _vfxGameService;

        // private readonly Dictionary<BattleProjectileObject, ProjectileInst> _projectileInsts = new();

        private readonly List<BattleProjectileGroup> _projectileGroups = new();

        private ObjectPool<BattleProjectileObject> _projectilePool;

        public BattleProjectileManager()
        {
            _projectileRoot = new GameObject("BattleProjectileManager").transform;
            _vfxGameService = GameFramework.GetService<BattleVfxGameService>();
            _projectilePool = GameFramework.GetService<ObjectPoolService>().NewObjectPool("BattleProjectilePool",
                newObjFunc:
                () =>
                {
                    var obj = new GameObject($"BattleProjectile");
                    obj.SetActive(false);
                    var ret = obj.AddComponent<BattleProjectileObject>();
                    return ret;
                });

            //Object.DontDestroyOnLoad(_projectileRoot.gameObject);
        }


        public void OnUpdate(float elapseTime, float realElapseTime)
        {
            for (var i = _projectileGroups.Count - 1; i >= 0; i--)
            {
                var group = _projectileGroups[i];
                group.OnUpdate(elapseTime, realElapseTime);
                if (group.IsComplete)
                {
                    ReleaseProjectileGroup(group);
                }
            }
        }

        public async Task AsyncLoadProjectiles(BattleUnitEntity[] entities)
        {
            foreach (var entity in entities)
            {
                var resId = entity.BattleUnit.ResourceId;
                var resData = entity.BattleUnit.ResourceData;

                if (string.IsNullOrEmpty(resData.Projectile) || _projectileObjs.ContainsKey(resId))
                {
                    continue;
                }

                var go = await Addressables.InstantiateAsync(resData.Projectile).Task;
                go.SetActive(false);
                go.transform.SetParent(_projectileRoot);
                _projectileObjs[resId] = go;
            }
        }

        public void DoProjectileAttack(BaseBattleInstance battle, BattleUnitEntity owner,
            ProjectileData info, HitTargetAction onHitTarget,
            params FrameSkillCastDataPb[] targets)
        {
            var group = new BattleProjectileGroup(this, battle, owner, info, onHitTarget, targets);
            _projectileGroups.Add(group);
            group.Start();
        }


        /// <summary>
        /// 返回entity对应的投射物体实例
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="owner"></param>
        /// <param name="info"></param>
        /// <param name="castDatas"></param>
        /// <returns></returns>
        public BattleProjectileObject GetProjectile(BaseBattleInstance battle, BattleUnitEntity owner,
            ProjectileData info, params FrameSkillCastDataPb[] castDatas)
        {
            // GameObject obj = null;
            // var resId = owner.BattleUnit.ResourceId;
            // if (_projectileObjs.ContainsKey(resId))
            // {
            //     //实例化对应的GameObject
            //     obj = Object.Instantiate(_projectileObjs[resId]);
            // }
            // else
            // {
            // obj = new GameObject($"{resId}-Projectile");
            // }

            // obj.SetActive(false);
            //
            // var proj = obj.GetOrAddComponent<BattleProjectileObject>();

            var proj = _projectilePool.Get<BattleProjectileObject>();
            proj.SetProjectileInfo(info, battle, owner, castDatas);

            // var projectileInst = new ProjectileInst
            // {
            //     ResId = resId,
            //     ProjectileObject = proj
            // };
            //
            // if (info.ProjectileVfx > 0)
            // {
            //     var vfxObj = _vfxGameService.InstanceVfx(info.ProjectileVfx, obj, false, false);
            //     projectileInst.VfxObject = vfxObj;
            // }
            //
            // _projectileInsts[proj] = projectileInst;

            return proj;
        }

        // public void ReleaseProjectile(BattleProjectileObject projectileObject)
        // {
        //     if (_projectileInsts.ContainsKey(projectileObject))
        //     {
        //         var inst = _projectileInsts[projectileObject];
        //         _vfxGameService.ReleaseVfxInst(inst.VfxObject);
        //         _projectileInsts.Remove(projectileObject);
        //
        //         Object.Destroy(inst.ProjectileObject.gameObject);
        //     }
        // }

        public void ReleaseProjectileGroup(BattleProjectileGroup group)
        {
            _projectileGroups.Remove(group);
            group.OnDestory();
            _projectilePool.Release(group.Projectiles);
        }

        public void Clear()
        {
            foreach (var obj in _projectileObjs.Values)
            {
                Addressables.ReleaseInstance(obj);
            }

            _projectileObjs.Clear();

            foreach (var group in _projectileGroups)
            {
            }

            Object.Destroy(_projectileRoot.gameObject);
        }
    }
}