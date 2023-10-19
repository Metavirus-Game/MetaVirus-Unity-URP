using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MetaVirus.Logic.Service.Battle.Scene
{
    public class BattleCameraControlPoint : MonoBehaviour
    {
        [Header("控制点的移动半径，填0表示不移动")] public float randomMoveRadius = 1;

        [Header("控制点的随机时间间隔，每隔指定时间随机一个位置")] public float randomMoveInterval = 0.5f;

        private Transform _originTransform;

        private Coroutine _randomMove;

        public bool IsRandomMoveOpen => randomMoveInterval > 0 && randomMoveRadius > 0;

        private void Start()
        {
            _originTransform = new GameObject(gameObject.name + "_Original").transform;
            _originTransform.position = transform.position;
            _originTransform.rotation = transform.rotation;
            _originTransform.localScale = transform.localScale;
            _randomMove = StartCoroutine(RandomMove());
        }

        private void OnDisable()
        {
            StopCoroutine(_randomMove);
        }


        private IEnumerator RandomMove()
        {
            var open = IsRandomMoveOpen;
            if (!open) yield break;
            while (true)
            {
                yield return new WaitForSeconds(randomMoveInterval);

                var rndX = Random.Range(-randomMoveRadius, randomMoveInterval);
                var rndZ = Random.Range(-randomMoveRadius, randomMoveInterval);

                var newPos = new Vector3(rndX, 0, rndZ);
                newPos = _originTransform.TransformPoint(newPos);

                transform.position = newPos;
            }
        }


#if UNITY_EDITOR

        [Header("Gizmos")] public Color gizmosColor = Color.yellow;

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmosColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            var size = randomMoveRadius * 2;
            Gizmos.DrawCube(Vector3.zero, new Vector3(size, 0, size));
        }

#endif
    }
}