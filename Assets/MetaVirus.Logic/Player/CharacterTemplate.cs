using System;
using cfg.avatar;
using GameEngine;
using MetaVirus.Logic.Service;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static MetaVirus.Logic.Data.Constants;
using Random = UnityEngine.Random;

namespace MetaVirus.Logic.Player
{
    public class CharacterTemplate : MonoBehaviour
    {
        private GameDataService _gameDataService;
        private Animator _animator;

        [Header("身体类型，Body01 - Body20")] [Range(1, 20)]
        public int body = 1;

        [Header("背部类型，0=无装饰，1-3 为披风装饰，4-6为背包装饰")] [Range(0, 6)]
        public int back = 0;

        [Header("头部类型，1-15为头部，16-20为整体头盔，不能有其他配件")] [Range(1, 20)]
        public int head = 1;

        [Header("头发类型，0=没有头发，没有帽子的情况下生效")] [Range(0, 13)]
        public int hair = 0;

        [Header("眉毛类型，0=没有眉毛，戴帽子和没有头发的情况下生效")] [Range(0, 2)]
        public int eyebrow = 0;

        [Header("帽子类型，=0没有帽子")] [Range(0, 14)] public int hat = 0;

        [Header("眼睛类型")] [Range(1, 12)] public int eye = 1;

        [Header("嘴巴类型")] [Range(1, 12)] public int mouth = 1;
        [Header("头饰")] [Range(0, 33)] public int headwear = 0;

        [Header("左手武器")] [Range(0, 55)] public int leftHand;
        [Header("右手武器")] [Range(0, 55)] public int rightHand;

        public NpcWeaponType WeaponType
        {
            get
            {
                var wt = NpcWeaponType.NoWeapon;

                if (MainHand != null && MainHand.Id != 0 && MainHand.RightHand)
                {
                    if (MainHand.TwoHands)
                    {
                        wt = MainHand.WeaponType switch
                        {
                            AvatarWeaponType.Spear => NpcWeaponType.Spear,
                            AvatarWeaponType.Wand => NpcWeaponType.MagicWand,
                            AvatarWeaponType.TwoHands => NpcWeaponType.TwoHands,
                            _ => wt
                        };
                    }
                    else
                    {
                        if (OffHand.LeftHand)
                        {
                            wt = OffHand.WeaponType switch
                            {
                                AvatarWeaponType.Sword => NpcWeaponType.DoubleSword,
                                AvatarWeaponType.Shield => NpcWeaponType.SwordAndShield,
                                AvatarWeaponType.None => NpcWeaponType.SingleSword,
                                _ => wt
                            };
                        }
                        else
                        {
                            wt = NpcWeaponType.SingleSword;
                        }
                    }
                }

                return wt;
            }
        }

        [Header("模型根节点")] public Transform rootNode;
        [Header("模型背部节点，放置背包、背武器等绑定点")] public Transform backNode;
        [Header("头部绑定节点")] public Transform headNode;

        public Transform leftHandNode;
        public Transform rightHandNode;


        [Header("模型是否自动随机切换Idle时的动作")] public bool randomIdleStateType = true;
        public float randomStateTypeMin = 2;
        public float randomStateTypeMax = 5;

        private float _randomRemainingTime = 0;
        private float _currIdleType = 0;
        private float _tarIdleType = 0;
        private float _idleTypeMoveSpeed = 1;

        [Header("Avatar参数，仅限编辑器使用")] public ulong avatarLongData;


        public AvatarBodyData BodyData { get; private set; }
        public AvatarBackData BackData { get; private set; }
        public AvatarHeadData HeadData { get; private set; }
        public AvatarSenseData EyeData { get; private set; }
        public AvatarSenseData EyebrowData { get; private set; }
        public AvatarSenseData MouthData { get; private set; }
        public AvatarSenseData HatData { get; private set; }
        public AvatarSenseData HairData { get; private set; }
        public AvatarSenseData HeadwearData { get; private set; }

        public AvatarWeaponData OffHand { get; private set; }
        public AvatarWeaponData MainHand { get; private set; }

        // Start is called before the first frame update
        void Start()
        {
            _gameDataService = GameFramework.GetService<GameDataService>();
            _animator = GetComponentInChildren<Animator>();
            RebuildAvatar();
        }

        /// <summary>
        /// 将avatar配置转换为Long型数据
        /// 身体5b, 背部 5b, 头部5b, 头发5b, 眉毛3b, 帽子5b, 眼睛5b, 嘴巴5b, 头饰6b, 左手武器6b, 右手武器6b 
        /// </summary>
        /// <returns></returns>
        public ulong AvatarToLong()
        {
            var longParam = (ulong)body;
            longParam = (longParam << 5) | (uint)back;
            longParam = (longParam << 5) | (uint)head;
            longParam = (longParam << 5) | (uint)hair;
            longParam = (longParam << 3) | (uint)eyebrow;
            longParam = (longParam << 5) | (uint)hat;
            longParam = (longParam << 5) | (uint)eye;
            longParam = (longParam << 5) | (uint)mouth;
            longParam = (longParam << 6) | (uint)headwear;
            longParam = (longParam << 6) | (uint)leftHand;
            longParam = (longParam << 6) | (uint)rightHand;

            return longParam;
        }

        public void ParseFromLongData(ulong longData)
        {
            avatarLongData = longData;
            rightHand = (int)(longData & 0x3f);
            longData >>= 6;

            leftHand = (int)(longData & 0x3f);
            longData >>= 6;

            headwear = (int)(longData & 0x3f);
            longData >>= 6;

            mouth = (int)(longData & 0x1f);
            longData >>= 5;

            eye = (int)(longData & 0x1f);
            longData >>= 5;

            hat = (int)(longData & 0x1f);
            longData >>= 5;

            eyebrow = (int)(longData & 0x07);
            longData >>= 3;

            hair = (int)(longData & 0x1f);
            longData >>= 5;

            head = (int)(longData & 0x1f);
            longData >>= 5;

            back = (int)(longData & 0x1f);
            longData >>= 5;

            body = (int)(longData & 0x1f);

            RebuildAvatar();
        }

        public void RebuildAvatar()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                _gameDataService = GameDataService.EditorInst;
                if (_gameDataService == null || _gameDataService.gameTable == null)
                {
                    EditorUtility.DisplayDialog("Message", "You need run GameEngine/LoadGameData first!", "ok");
                    return;
                }
            }
#endif

            //身体
            BodyData = _gameDataService.gameTable.AvatarBodyDatas.GetOrDefault(body) ??
                       _gameDataService.gameTable.AvatarBodyDatas.Get(1);

            //背部
            BackData = _gameDataService.gameTable.AvatarBackDatas.Get(back);

            //头部
            HeadData = _gameDataService.gameTable.AvatarHeadDatas.GetOrDefault(head) ??
                       _gameDataService.gameTable.AvatarHeadDatas.Get(1);

            //头部，五官及头饰
            EyeData = _gameDataService.GetAvatarSenseData(eye, HeadWearType.Eye);
            EyebrowData = _gameDataService.GetAvatarSenseData(eyebrow, HeadWearType.Eyebrow);
            MouthData = _gameDataService.GetAvatarSenseData(mouth, HeadWearType.Mouth);
            HatData = _gameDataService.GetAvatarSenseData(hat, HeadWearType.Hat);
            HairData = _gameDataService.GetAvatarSenseData(hair, HeadWearType.Hair);
            HeadwearData = _gameDataService.GetAvatarSenseData(headwear, HeadWearType.Headwear);

            //左右手
            OffHand = _gameDataService.gameTable.AvatarWeaponDatas.Get(leftHand);
            MainHand = _gameDataService.gameTable.AvatarWeaponDatas.Get(rightHand);

            //设置身体及背部节点
            for (var i = 0; i < rootNode.childCount; i++)
            {
                var node = rootNode.GetChild(i);
                if (node.name == "root") continue;
                var active = node.name == BodyData.ObjectName;

                if (BackData.Type == 0)
                {
                    //披风装饰
                    active |= node.name == BackData.ObjectName;
                }

                node.gameObject.SetActive(active);
            }

            for (var i = 0; i < backNode.childCount; i++)
            {
                var node = backNode.GetChild(i);
                node.gameObject.SetActive(BackData != null && node.name == BackData.ObjectName);
            }

            //设置头部节点
            for (var i = 0; i < headNode.childCount; i++)
            {
                var node = headNode.GetChild(i);
                var active = node.name == HeadData.ObjectName;

                if (HeadData.AllowHeadWear)
                {
                    //设置眼睛
                    active |= node.name == EyeData.ObjectName;
                    //设置嘴巴
                    active |= node.name == MouthData.ObjectName;


                    if (HatData.PartId > 0)
                    {
                        //有帽子，不显示头发，显示眉毛
                        active |= node.name == EyebrowData.ObjectName;
                        active |= node.name == HatData.ObjectName;
                    }
                    else
                    {
                        //没有帽子，显示头发和头饰，如果没有头发，则显示眉毛
                        active |= node.name == HairData.ObjectName;
                        if (HairData.PartId == 0)
                        {
                            active |= node.name == EyebrowData.ObjectName;
                        }
                    }

                    active |= node.name == HeadwearData.ObjectName;
                }

                node.gameObject.SetActive(active);
            }

            //设置左手武器
            for (var i = 0; i < leftHandNode.childCount; i++)
            {
                var node = leftHandNode.GetChild(i);
                var active = node.name == OffHand.ObjectName && OffHand.LeftHand &&
                             !MainHand.TwoHands &&
                             MainHand.Id != 0 && MainHand.RightHand;
                node.gameObject.SetActive(active);
            }

            //设置右手武器
            for (var i = 0; i < rightHandNode.childCount; i++)
            {
                var node = rightHandNode.GetChild(i);
                var active = node.name == MainHand.ObjectName && MainHand.RightHand;
                node.gameObject.SetActive(active);
            }

            if (_animator != null)
                _animator.SetFloat(AniParamName.Weapon, (int)WeaponType);
        }

        private void OnEnable()
        {
            if (_animator != null)
                _animator.SetFloat(AniParamName.Weapon, (int)WeaponType);
        }


        // Update is called once per frame
        void Update()
        {
            if (randomIdleStateType)
            {
                _randomRemainingTime -= Time.deltaTime;
                if (_randomRemainingTime <= 0)
                {
                    _tarIdleType = Random.Range(0, 2);
                    _idleTypeMoveSpeed = Mathf.Abs(_currIdleType - _tarIdleType) * Time.deltaTime * 2;
                    _randomRemainingTime = Random.Range(randomStateTypeMin, randomStateTypeMax);
                }

                if (_animator != null)
                {
                    _currIdleType = Mathf.MoveTowards(_currIdleType, _tarIdleType, _idleTypeMoveSpeed);
                    _animator.SetFloat(AniParamName.IdleStateType, _currIdleType);
                }
            }
        }
    }
}