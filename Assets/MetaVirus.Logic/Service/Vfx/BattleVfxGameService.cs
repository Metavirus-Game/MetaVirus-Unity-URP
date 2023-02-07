using System.Collections.Generic;
using System.Threading.Tasks;
using cfg.battle;
using cfg.common;
using cfg.skill;
using GameEngine.Base;
using GameEngine.Utils;
using MetaVirus.Logic.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace MetaVirus.Logic.Service.Vfx
{
    public class BattleVfxGameService : BaseService
    {
        private GameObject _vfxRoot;
        private GameObject _vfxInstRoot;

        private readonly Dictionary<int, VfxInfo> _loadedVfxes = new();
        private readonly Dictionary<GameObject, VfxInstInfo> _instedVfxes = new();
        private GameDataService _gameDataService;

        private readonly List<VfxInstInfo> _vfxInstPool = new();

        private readonly List<GameObject> _assetObjects = new();

        public override void PostConstruct()
        {
            _vfxRoot = new GameObject("VfxRoot");
            DontDestroyOnLoad(_vfxRoot);
            _vfxRoot.SetActive(false);

            _vfxInstRoot = new GameObject("VfxInstRoot")
            {
                transform =
                {
                    position = Vector3.zero,
                    rotation = Quaternion.identity,
                    localScale = Vector3.one
                }
            };
            DontDestroyOnLoad(_vfxInstRoot);
        }

        public override void ServiceReady()
        {
            _gameDataService = GetService<GameDataService>();
        }

        private void LoadVfx(int vfxId, VFXData vfxData, GameObject resObj, GameObject criObj)
        {
            if (resObj == null)
            {
                return;
            }

            var vfxInfo = new VfxInfo
            {
                VfxId = vfxId,
                VFXData = vfxData,
                VfxObject = resObj,
                CriVfxObject = criObj,
            };

            _loadedVfxes[vfxId] = vfxInfo;

            resObj.SetActive(false);
            resObj.transform.SetParent(_vfxRoot.transform, false);
            if (criObj != null)
            {
                criObj.SetActive(false);
                criObj.transform.SetParent(_vfxRoot.transform, false);
            }
        }

        public async Task AsyncLoadVfxes(int[] vfxIds, TaskProgressHandler handler = null)
        {
            handler?.ReportProgress(0);
            var progress = 0;
            var p = (int)(100f / vfxIds.Length);
            foreach (var vfxId in vfxIds)
            {
                if (vfxId == 0 || _loadedVfxes.ContainsKey(vfxId))
                {
                    continue;
                }

                var vfxData = _gameDataService.GetVfxData(vfxId);
                if (vfxData == null || string.IsNullOrEmpty(vfxData.AssetName))
                {
                    continue;
                }

                var go = await Addressables.LoadAssetAsync<GameObject>(vfxData.AssetName).Task;
                go.SetActive(false);
                _assetObjects.Add(go);

                var layerHidder = LayerMask.NameToLayer("Hidden");

                var resObj = await Addressables.InstantiateAsync(vfxData.AssetName).Task;
                resObj.SetLayerAll(layerHidder);
                resObj.SetActive(true);
                GameObject criResObj = null;

                if (!string.IsNullOrEmpty(vfxData.CriAssetName))
                {
                    var go1 = await Addressables.LoadAssetAsync<GameObject>(vfxData.CriAssetName).Task;
                    go1.SetActive(false);
                    _assetObjects.Add(go1);
                    criResObj = await Addressables.InstantiateAsync(vfxData.CriAssetName).Task;
                    criResObj.SetActive(true);
                    criResObj.SetLayerAll(layerHidder);
                }

                LoadVfx(vfxId, vfxData, resObj, criResObj);
                progress += p;
                handler?.ReportProgress(progress);
            }

            handler?.ReportProgress(100);

            // await Task.Delay(1000);
            //
            // foreach (var vfxes in _loadedVfxes.Values)
            // {
            //     var obj = vfxes.VfxObject;
            //     var criObj = vfxes.CriVfxObject;
            //     obj.SetActive(false);
            //     obj.transform.SetParent(_vfxRoot.transform, false);
            //     if (criObj != null)
            //     {
            //         criObj.SetActive(false);
            //         criObj.transform.SetParent(_vfxRoot.transform, false);
            //     }
            // }
        }

        public void ReleaseVfxes()
        {
            foreach (var vfx in _instedVfxes.Values)
            {
                Destroy(vfx.VfxObj);
            }

            foreach (var vfx in _vfxInstPool)
            {
                Destroy(vfx.VfxObj);
            }

            foreach (var vfx in _loadedVfxes.Values)
            {
                Addressables.ReleaseInstance(vfx.VfxObject);
                if (vfx.CriVfxObject != null)
                {
                    Addressables.ReleaseInstance(vfx.CriVfxObject);
                }
            }

            foreach (var ao in _assetObjects)
            {
                Addressables.Release(ao);
            }

            _vfxInstPool.Clear();
            _assetObjects.Clear();
            _loadedVfxes.Clear();
            _instedVfxes.Clear();
        }

        public bool IsVfxLoaded(int vfxId)
        {
            return vfxId != 0 && _loadedVfxes.ContainsKey(vfxId);
        }

        public int GetSkillHealVfxId(BattleSkillData skillData)
        {
            var hitVfxId = skillData.CastAction.HitVfx;

            if (hitVfxId > 0 && !_loadedVfxes.ContainsKey(hitVfxId))
            {
                //没有找到对应的特效，归0，根据技能属性选择
                hitVfxId = 0;
            }

            if (hitVfxId != 0) return hitVfxId;
            return _gameDataService.CommonConfig.VfxHealHitDefault;
        }


        /// <summary>
        /// 返回指定类型的投射物特效，是否用投射物的命中特效替换技能指定的命中特效
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsReplaceSkillHitVfx(ProjectileType type)
        {
            return type != ProjectileType.Multi_Bomb && type != ProjectileType.Multi_Summon;
        }

        /// <summary>
        /// 返回指定类型的投射物是否为单体类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsSingleProjectile(ProjectileType type)
        {
            return type is ProjectileType.Single_Bomb or ProjectileType.Single_Bullet or ProjectileType.Single_Summon
                or ProjectileType.Single_Target;
        }

        public int GetSkillHitVfxId(BattleSkillData skillData)
        {
            var hitVfxId = skillData.CastAction.HitVfx;
            if (IsReplaceSkillHitVfx(skillData.CastAction.Projectile_Ref.Type)
                && skillData.CastAction.Projectile_Ref.HitVfx != 0)
            {
                //动画带有投射物，不是Summon类的，且投射物配置了命中特效
                //Summon和Bomb类型的，会使用技能本身设定的命中特效
                hitVfxId = skillData.CastAction.Projectile_Ref.HitVfx;
            }

            if (hitVfxId > 0 && !_loadedVfxes.ContainsKey(hitVfxId))
            {
                //没有找到对应的特效，归0，根据技能属性选择
                hitVfxId = 0;
            }

            if (hitVfxId != 0) return hitVfxId;

            hitVfxId = skillData.AtkAttribute == AtkAttribute.Physical
                ? _gameDataService.CommonConfig.VfxMeleeHitDefault
                : _gameDataService.CommonConfig.VfxMagicHitDefaut;
            return hitVfxId;
        }

        private void SetVfxObjLayer(GameObject vfxObj, VFXData vfxData)
        {
            if (vfxData.Type == VfxType.HitVfx)
            {
                //命中特效提到最上层
                vfxObj.SetLayerAll(LayerMask.NameToLayer("BattleVfx"));
            }
            else
            {
                vfxObj.SetLayerAll(LayerMask.NameToLayer("Default"));
            }
        }


        private GameObject InstanceVfxGameObject(int vfxId, bool isCri = false, bool autoDestroy = true)
        {
            _loadedVfxes.TryGetValue(vfxId, out var vfx);
            if (vfx.VfxId != vfxId)
            {
                return null;
            }

            GameObject vfxObj = null;
            foreach (var vfxInstInfo in _vfxInstPool)
            {
                if (vfxInstInfo.VfxId != vfxId) continue;

                //池子里有对应id的特效
                _vfxInstPool.Remove(vfxInstInfo);
                vfxObj = vfxInstInfo.VfxObj;
                break;
            }

            if (vfxObj == null)
            {
                vfxObj = vfx.GetVfxObject(isCri);
                vfxObj = Instantiate(vfxObj, _vfxInstRoot.transform, false);
            }

            SetVfxObjLayer(vfxObj, vfx.VFXData);

            var l = vfxObj.GetComponent<VfxParticleSystemManager>();
            if (l == null)
            {
                l = vfxObj.AddComponent<VfxParticleSystemManager>();
            }

            l.OnSpawn();
            if (autoDestroy)
            {
                //自动销毁的特效，不能含有loop的粒子系统
                l.UnloopAllVfx();

                l.OnStopped = go =>
                {
                    l.RestoreLoopVfx();
                    RecycleVfx(go);
                    l.OnStopped = null;
                };
            }
            else
            {
                l.OnStopped = null;
            }


            return vfxObj;
        }

        private void RecycleVfx(GameObject vfxObj, UnityAction onReleased = null)
        {
            vfxObj.SetActive(false);
            if (_instedVfxes.TryGetValue(vfxObj, out var vfxInstInfo))
            {
                _instedVfxes.Remove(vfxObj);
                _vfxInstPool.Add(vfxInstInfo);
            }

            onReleased?.Invoke();
        }

        /// <summary>
        /// 释放特效实例，放入对象池，供下次使用
        /// </summary>
        /// <param name="vfxObj"></param>
        /// <param name="onReleased"></param>
        public void ReleaseVfxInst(GameObject vfxObj, UnityAction onReleased = null)
        {
            if (vfxObj == null) return;
            if (_instedVfxes.TryGetValue(vfxObj, out var info))
            {
                if (info.AutoDestory)
                {
                    return;
                }

                var l = vfxObj.GetComponent<VfxParticleSystemManager>();
                if (l.IsLoopVfx)
                {
                    //带有循环的粒子效果，将循环设置为false，并等待最后一次播放完毕
                    l.UnloopAllVfx();
                    l.OnStopped = go =>
                    {
                        RecycleVfx(go, onReleased);
                        l.RestoreLoopVfx();
                    };
                }
                else
                {
                    //非循环的粒子效果，直接回收
                    RecycleVfx(vfxObj, onReleased);
                }
            }
        }

        public GameObject InstanceVfx(int vfxId, Vector3 position, Quaternion rotation, bool isCri = false,
            bool autoDestroy = true)
        {
            var vfxObj = InstanceVfxGameObject(vfxId, isCri, autoDestroy);
            if (vfxObj == null)
            {
                return new GameObject("VfxNotFound: " + vfxId);
            }

            var info = new VfxInstInfo
            {
                VfxId = vfxId,
                VfxObj = vfxObj,
                BindMethod = VfxBindMethod.Position,
                BindObject = null,
                BindPosition = position,
                BindRotation = rotation,
                AutoDestory = autoDestroy
            };

            vfxObj.transform.position = position;
            vfxObj.transform.rotation = rotation;
            vfxObj.SetActive(true);
            _instedVfxes[vfxObj] = info;

            return info.VfxObj;
        }

        /// <summary>
        /// 实例化一个指定id的特效
        /// </summary>
        /// <param name="vfxId">要实例化的特效id</param>
        /// <param name="bindObj">绑定的GameObject</param>
        /// <param name="isCri">是否获取暴击特效</param>
        /// <param name="autoDestroy">动画播放完自动删除(Loop模式的特效不会自动删除)</param>
        /// <returns></returns>
        public GameObject InstanceVfx(int vfxId, GameObject bindObj, bool isCri = false, bool autoDestroy = true)
        {
            var vfxObj = InstanceVfxGameObject(vfxId, isCri, autoDestroy);
            if (vfxObj == null)
            {
                return new GameObject("VfxNotFound: " + vfxId);
            }

            if (bindObj != null)
            {
                vfxObj.transform.position = bindObj.transform.position;
                vfxObj.transform.rotation = bindObj.transform.rotation;
            }

            var info = new VfxInstInfo
            {
                VfxId = vfxId,
                VfxObj = vfxObj,
                BindObject = bindObj,
                BindMethod = VfxBindMethod.GameObject,
                AutoDestory = autoDestroy
            };

            vfxObj.SetActive(true);
            _instedVfxes[vfxObj] = info;

            return info.VfxObj;
        }

        public override void OnUpdate(float elapseTime, float realElapseTime)
        {
            foreach (var vfx in _instedVfxes.Values)
            {
                if (vfx.BindMethod == VfxBindMethod.GameObject && vfx.BindObject != null)
                {
                    vfx.VfxObj.transform.position = vfx.BindObject.transform.position;
                }
            }
        }

        private enum VfxBindMethod
        {
            Position,
            GameObject
        }

        private struct VfxInstInfo
        {
            public int VfxId;
            public VfxBindMethod BindMethod;
            public GameObject VfxObj;
            public GameObject BindObject;
            public Vector3 BindPosition;
            public Quaternion BindRotation;
            public bool AutoDestory;
        }

        private struct VfxInfo
        {
            public int VfxId;
            public VFXData VFXData;
            public GameObject VfxObject;
            public GameObject CriVfxObject;

            public GameObject GetVfxObject(bool isCri)
            {
                if (isCri && CriVfxObject != null)
                {
                    return CriVfxObject;
                }

                return VfxObject;
            }
        }
    }
}