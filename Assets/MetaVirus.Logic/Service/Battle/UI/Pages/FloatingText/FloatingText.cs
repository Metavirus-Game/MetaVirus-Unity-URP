using cfg.battle;
using cfg.buff;
using cfg.common;
using cfg.skill;
using DG.Tweening;
using FairyGUI;
using GameEngine;
using GameEngine.ObjectPool;
using MetaVirus.Logic.Service.Battle.data;
using UnityEngine;
using UnityEngine.Events;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Service.Battle.UI.Pages.FloatingText
{
    public struct FloatingTextData
    {
        public AtkAttribute ValueAtkAttr;
        public AttributeId ValueAttr;
        public SkillCastValueType ValueType;
        public int Value;
        public SkillCastStateType SkillState;

        public static FloatingTextData FromSkillCastInfo(SkillCastDataInfo info)
        {
            return new FloatingTextData
            {
                ValueAtkAttr = info.SkillAttribute,
                ValueAttr = info.EffectAttr,
                ValueType = info.ValueType,
                Value = info.EffectValue,
                SkillState = info.SkillState,
            };
        }

        public static FloatingTextData FromBuffEffect(UnitBuffAttached buff, AttributeId effectAttr, int effectValue)
        {
            var effType = buff.BuffInfo.LevelEffect.EffectType == BuffEffectType.RoundIncrease
                ? SkillCastValueType.Increase
                : SkillCastValueType.Decrease;
            return new FloatingTextData
            {
                ValueAtkAttr = buff.BuffInfo.LevelEffect.EffectAttribute,
                ValueAttr = effectAttr,
                ValueType = effType,
                Value = effectValue,
                SkillState = effectValue == 0 ? SkillCastStateType.Immune : SkillCastStateType.Hit,
            };
        }

        public bool IsMiss => SkillState.HasFlag(SkillCastStateType.Miss);
        public bool IsHit => SkillState.HasFlag(SkillCastStateType.Hit);
        public bool IsImmune => SkillState.HasFlag(SkillCastStateType.Immune);
        public bool IsCri => SkillState.HasFlag(SkillCastStateType.Cri);
        public bool IsAbsorb => IsHit && SkillState.HasFlag(SkillCastStateType.Absorb);
        public bool IsReflect => IsHit && SkillState.HasFlag(SkillCastStateType.Reflect);
    }

    public class FloatingText : IRecyclable
    {
        public int BindEntityId => _entity.Id;

        public float AliveTime { get; private set; }

        public GTextField TextField { get; }

        public GLoader IconLoader { get; }

        private GComponent _container;

        public UnityAction OnComplete;

        private readonly GameDataService _gameDataService;

        public string Text
        {
            get => TextField?.text ?? "";
            set
            {
                if (TextField != null)
                {
                    TextField.text = value;
                }
            }
        }

        public Color Color
        {
            get => TextField.color;
            set => TextField.color = value;
        }

        private FloatingTextData _data;

        private BattleUnitEntity _entity;
        private GComponent _rootComp;
        private static readonly int OutlineFactor = Shader.PropertyToID("_OutlineFactor");
        private static readonly int OutlineWidth = Shader.PropertyToID("_OutlineWidth");
        private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");

        private readonly int _id = 0;
        private static int _idGen = 0;

        private float _startDelay = 0;

        public FloatingText()
        {
            _id = ++_idGen;
            _gameDataService = GameFramework.GetService<GameDataService>();
            TextField = MakeTextField();
            IconLoader = MakeIcon();

            _container = new GComponent
            {
                gameObjectName = "FloatingTextContainer",
                size = Vector2.zero,
                position = Vector3.zero,
                pivotAsAnchor = true,
                visible = false,
            };

            _container.AddChild(TextField);
            _container.AddChild(IconLoader);
            IconLoader.SetXY(-10, 0);
        }

        public void SetInfo(GComponent rootComp, FloatingTextData data, BattleUnitEntity unitEntity, float delay = 0)
        {
            _rootComp = rootComp;
            _entity = unitEntity;
            _data = data;
            Setup();

            _rootComp.AddChild(_container);
            _startDelay = delay;
        }

        private const int FontSizeText = 40;
        private const int FontSizeSmall = 60;
        private const int FontSizeBig = 100;

        private void Setup()
        {
            TextField.autoSize = AutoSizeType.Both;
            Color c;
            if (_data.IsMiss)
            {
                c = Color.magenta;
                c.a = 0;
                var glowClr = c * 2;
                Text = "Miss";
                SetFont(FontSizeText, Color.white, c, glowClr);
                IconLoader.visible = false;
            }
            else
            {
                Text = _data.Value.ToString();
                c = _data.ValueType == SkillCastValueType.Decrease
                    ? _gameDataService.BattleColorHpDec()
                    : _gameDataService.BattleColorHpInc();
                var glowClr = c * 2;
                c.a = 0;
                glowClr.a = 0;

                var textSize = _data.IsCri ? FontSizeBig : FontSizeSmall;

                SetFont(textSize, Color.white, c, glowClr);

                if (_data.ValueAtkAttr > AtkAttribute.Magic)
                {
                    var imgName = AtkAttributeToImageName(_data.ValueAtkAttr);
                    var clrs = AtkAttributeToColor(_data.ValueAtkAttr);

                    if (imgName != null)
                    {
                        IconLoader.url = FairyImageUrl.Common(imgName);
                        IconLoader.color = clrs[0];
                        IconLoader.image.material.SetColor(OutlineColor, clrs[1]);
                        IconLoader.image.material.SetFloat(OutlineFactor, 10);
                        IconLoader.image.material.SetFloat(OutlineWidth, 2);
                        IconLoader.SetSize(textSize, textSize);
                    }

                    IconLoader.visible = imgName != null;
                }
                else
                {
                    IconLoader.visible = false;
                }
            }
        }

        private void SetFont(int size, Color textColor, Color outlineColor, Color glowColor)
        {
            var format = TextField.textFormat;
            format.size = size;
            format.color = textColor;
            format.outlineColor = outlineColor;
            format.glowColor = glowColor;
            TextField.textFormat = format;
        }

        private static Vector2 Parabola(Vector2 start, Vector2 end, float height, float t)
        {
            var mid = Vector2.Lerp(start, end, t);
            var y = 4 * (-height * t * t + height * t);
            return new Vector2(mid.x, y + Mathf.Lerp(start.y, end.y, t));
        }

        private static Vector2 Bezier(Vector2 start, Vector2 end, Vector2 control, float t)
        {
            var p1 = (1 - t) * (1 - t) * start;
            var p2 = 2 * t * (1 - t) * control;
            var p3 = t * t * end;

            return p1 + p2 + p3;
        }

        private void AniShow()
        {
            _container.visible = true;
            DOTween.To(value =>
            {
                var format = TextField.textFormat;
                format.color.a = value;
                format.glowColor.a = value;
                format.outlineColor.a = value;
                TextField.textFormat = format;

                // var c= IconLoader.color;
                // c.a = value;
                // IconLoader.color = c;

                _container.alpha = value;
            }, 0, 1, 0.2f);

            //DOTween.ToAlpha(() => Color, clr => Color = clr, 1, 0.2f);

            var toX = 0; //_data.ValueAtkAttr > AtkAttribute.Magic ? -50 : 20;

            DOTween.To(value => _container.SetScale(value, value), 3f, 1, 0.4f)
                .SetEase(Ease.OutBack);
            DOTween.To(value => { _container.SetXY(toX, -150 * value); }, 0, 1, 0.4f)
                .SetEase(Ease.OutBack);
            DOTween.To(value => { _container.SetXY(toX, -150 - 50 * value); }, 0, 1, 0.8f)
                .SetEase(Ease.OutQuad).SetDelay(0.4f);


            DOTween.To(value =>
            {
                var format = TextField.textFormat;
                format.color.a = value;
                format.glowColor.a = value;
                format.outlineColor.a = value;
                TextField.textFormat = format;
                _container.alpha = value;
            }, 1, 0, 0.4f).SetDelay(0.8f).OnComplete(() => OnComplete?.Invoke());

            // DOTween.ToAlpha(() => Color, clr => Color = clr, 0, 0.4f).SetDelay(0.8f)
            //     .OnComplete(() => OnComplete?.Invoke());
        }

        private void AniShow1()
        {
            DOTween.To(value => TextField.SetScale(value, value), 3f, 1, 0.2f);
            DOTween.ToAlpha(() => Color, clr => Color = clr, 1, 0.2f);
            //DOTween.To(value => TextField.SetXY(0, value), 0, -100, 0.2f);

            var dir = 1; //(int)Mathf.Sign(Random.Range(-1f, 1f));

            var start = Vector2.zero;
            var end = new Vector2(dir * 200 + Random.Range(30, 100), 100);
            var control = new Vector2(dir * 50, -400);

            DOTween.To(value =>
            {
                var p = Bezier(start, end, control, value);
                TextField.SetXY(p.x, p.y);
            }, 0, 1, 1.2f);

            DOTween.ToAlpha(() => Color, clr => Color = clr, 0, 0.4f).SetDelay(0.8f)
                .OnComplete(() => OnComplete?.Invoke());
        }

        private GLoader MakeIcon()
        {
            var iconLoader = new GLoader
            {
                size = new Vector2(TextField.height, TextField.height),
                pivot = new Vector2(1, 0.5f),
                pivotAsAnchor = true,
                fill = FillType.ScaleMatchHeight,
                visible = true
            };
            iconLoader.image.graphics.materialKeywords = new[] { "GLOW", "FloatingText" + _id };
            iconLoader.SetXY(0, 0);
            return iconLoader;
        }

        private GTextField MakeTextField()
        {
            var text = new GTextField
            {
                text = "",
                pivot = new Vector2(0, 0.5f),
                pivotAsAnchor = true,
            };
            var format = text.textFormat;
            format.size = 40;
            format.font = EnJosefinSans;
            format.outline = 0.4f;
            format.faceDilate = 0.1f;
            format.glowPower = 0.75f;
            format.glowInner = 0.3f;
            format.glowOuter = 0.6f;
            //format.shadowOffset = new Vector2(0.3f, 0.3f);
            //format.shadowColor = Color.black;
            //format.underlaySoftness = 0.4f;
            text.textFormat = format;
            text.visible = true;
            return text;
        }

        public void OnUpdate(float elapseTime, float realElapseTime)
        {
            if (_startDelay <= -1)
            {
                AliveTime += elapseTime;
            }
            else
            {
                _startDelay -= elapseTime;
                if (_startDelay < 0)
                {
                    _startDelay = -1;
                    AniShow();
                }
            }
        }


        public void OnSpawn()
        {
            AliveTime = 0;
            _startDelay = -1;
            Text = "";
            _container.visible = false;
        }

        public void OnRecycle()
        {
            AliveTime = 0;
            _startDelay = -1;
            Text = "";
            _container.visible = false;
            _container.RemoveFromParent();
        }

        public void OnDestroy()
        {
            _container.visible = false;
            _container.RemoveFromParent();
            _container.Dispose();
        }
    }
}