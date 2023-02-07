using cfg.common;
using GameEngine.Utils;
using MetaVirus.Logic.Service.Vfx;
using UnityEngine;

namespace MetaVirus.Logic.Service.Battle.Projectile.States
{
    public class ProjectileStateMultiBomb : ProjectileStateBase
    {
        private float _time = 0;
        private Transform _parabolaRoot;
        private const float AngleShot = 30;

        private const float AngleShotRad = AngleShot * Mathf.Deg2Rad;
        // private float G => -Physics.gravity.y;
        // private float _shotSpeed = 0;

        private float _distance = 0;
        private float _duration = 0;

        private Vector2 _start;
        private Vector2 _end;
        private Vector2 _control;

        private Vector3 _startPos;
        private Quaternion _startRot;

        private float _shootAngle = 45;

        public ProjectileStateMultiBomb(BattleProjectileObject projectile) : base(projectile)
        {
        }

        protected override void OnInit()
        {
            var pos = Projectile.Owner.UnitAni.GetVfxBindPos(VfxBindPos.Projectile).position;
            var target = Projectile.CastTargets[0];
            var tarPos = Projectile.Battle.GetFormationCenter(target.BattleUnit.Side).position;
            var dir = tarPos - pos;
            var transform = Projectile.transform;
            transform.position = pos;
            transform.forward = dir;

            _startPos = pos;
            _startRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z).normalized);

            _distance = Vector3.Distance(pos, tarPos);

            _duration = _distance / Projectile.GameDataService.BattleProjectileSpeed(Projectile.Info.Speed);

            var v = Mathf.Tan(_shootAngle * Mathf.Deg2Rad);
            var h = _distance / 2 * v;

            _start = Vector2.zero;
            _end = new Vector2(_distance, tarPos.y - pos.y);
            _control = new Vector2(_distance / 5 * 3, h);
        }

        public override void OnUpdate(float deltaTime)
        {
            var pos = GameEngineUtils.Bezier(_start, _end, _control, _time);
            _time += deltaTime / _duration;
            var nextPos = GameEngineUtils.Bezier(_start, _end, _control, _time);

            var projPos = _startRot * new Vector3(0, pos.y, pos.x);
            var projNextPos = _startRot * new Vector3(0, nextPos.y, nextPos.x);

            projPos += _startPos;
            projNextPos += _startPos;

            var dir = projNextPos - projPos;

            var myTrans = Projectile.transform;
            myTrans.position = projPos;
            myTrans.forward = dir;

            if (_time >= 1 && !Projectile.IsHit)
            {
                var target = Projectile.CastTargets[0];
                Projectile.OnHit();
                Projectile.DestroyProjectileVfx();
                var gameObject = Projectile.gameObject;

                gameObject.transform.forward = Projectile.Owner.Transform.forward;
                var hitVfxObj = Projectile.VFXGameService.InstanceVfx(Projectile.Info.HitVfx,
                    gameObject.transform.position, gameObject.transform.rotation);

                void HitCallback(SkillHitInfo info)
                {
                    if (info.percent > 0)
                    {
                        foreach (var data in Projectile.CastDatas)
                        {
                            target = Projectile.Battle.GetUnitEntity(data.CastData.TargetId);
                            Projectile.OnHitTarget?.Invoke(data, target, info);
                        }
                    }

                    if (!info.IsHitFinished) return;
                    Projectile.OnHitFinished(false);
                    Object.Destroy(_parabolaRoot.gameObject);
                }

                var delay = hitVfxObj.GetComponent<BattleVfxHitDelay>();
                if (delay != null)
                {
                    delay.InvokeHitDelay(HitCallback,
                        () => Projectile.OnFinished());
                }
                else
                {
                    HitCallback(SkillHitInfo.Default);
                    Projectile.OnFinished();
                }
            }
        }

        protected void OnInit_Parabola()
        {
            // var pos = Projectile.Owner.UnitAni.GetVfxBindPos(VfxBindPos.Projectile).position;
            // var target = Projectile.CastTargets[0];
            // var tarPos = Projectile.Battle.GetFormationCenter(target.BattleUnit.Side).position;
            // var dir = tarPos - pos;
            // var transform = Projectile.transform;
            // transform.position = pos;
            // transform.forward = dir;
            // _parabolaRoot = new GameObject
            // {
            //     name = "ParabolaRoot",
            //     transform =
            //     {
            //         position = pos,
            //         forward = dir
            //     }
            // }.transform;
            //
            // _distance = Vector3.Distance(pos, tarPos);
            //
            // //通过距离和角度，计算出飞行速度
            // //抛物线的飞行速度由距离决定，和设置中的速度无关
            // // r = v² * sin2θ / g
            // // v² = r * g / sin2θ
            //
            // var v = Mathf.Sqrt(_distance * G / Mathf.Sin(AngleShotRad * 2));
            // _shotSpeed = v;
        }

        public void OnUpdate_Parabola(float deltaTime)
        {
            //     var t2 = _time * _time;
            //     //计算当前点坐标
            //     //z = v * cosθ * t, y = v * sinθ * t - g * t² * 0.5
            //     var currPos = new Vector3(0,
            //         _shotSpeed * Mathf.Sin(AngleShotRad) * _time - G * t2 * 0.5f,
            //         _shotSpeed * Mathf.Cos(AngleShotRad) * _time
            //     );
            //
            //     _time += deltaTime;
            //     t2 = _time * _time;
            //
            //     var nextPos = new Vector3(0,
            //         _shotSpeed * Mathf.Sin(AngleShotRad) * _time - G * t2 * 0.5f,
            //         _shotSpeed * Mathf.Cos(AngleShotRad) * _time
            //     );
            //
            //     var dir = (nextPos - currPos).normalized;
            //     dir = _parabolaRoot.TransformDirection(dir);
            //
            //     var myTrans = Projectile.transform;
            //     myTrans.forward = dir;
            //     myTrans.position = _parabolaRoot.TransformPoint(currPos);
            //
            //     var target = Projectile.CastTargets[0];
            //     var tarTrans = Projectile.Battle.GetFormationCenter(target.BattleUnit.Side);
            //
            //     var tarPos = tarTrans.InverseTransformPoint(myTrans.position);
            //     if ((tarPos.z <= 0 || tarPos.y <= 0) && !Projectile.IsHit)
            //     {
            //         Projectile.OnHit();
            //         Projectile.DestroyProjectileVfx();
            //         var gameObject = Projectile.gameObject;
            //
            //         gameObject.transform.forward = Projectile.Owner.Transform.forward;
            //         var hitVfxObj = Projectile.VFXGameService.InstanceVfx(Projectile.Info.HitVfx,
            //             gameObject.transform.position, gameObject.transform.rotation);
            //
            //         void HitCallback(SkillHitInfo info)
            //         {
            //             if (info.percent > 0)
            //             {
            //                 foreach (var data in Projectile.CastDatas)
            //                 {
            //                     target = Projectile.Battle.GetUnitEntity(data.CastData.TargetId);
            //                     Projectile.OnHitTarget?.Invoke(data, target, info);
            //                 }
            //             }
            //
            //             if (!info.IsHitFinished) return;
            //             Projectile.OnHitFinished(false);
            //             Object.Destroy(_parabolaRoot.gameObject);
            //         }
            //
            //         var delay = hitVfxObj.GetComponent<BattleVfxHitDelay>();
            //         if (delay != null)
            //         {
            //             delay.InvokeHitDelay(HitCallback,
            //                 () => Projectile.OnFinished());
            //         }
            //         else
            //         {
            //             HitCallback(SkillHitInfo.Default);
            //             Projectile.OnFinished();
            //         }
            //     }
        }
    }
}