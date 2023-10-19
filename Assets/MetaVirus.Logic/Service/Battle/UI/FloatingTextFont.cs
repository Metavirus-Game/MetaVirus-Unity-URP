using FairyGUI;
using TMPro;
using UnityEngine;

namespace MetaVirus.Logic.Service.Battle.UI
{
    public class FloatingTextFont : TMPFont
    {
        private TextFormat _format;

        public override void SetFormat(TextFormat format, float fontSizeScale)
        {
            base.SetFormat(format, fontSizeScale);
            _format = format;
        }

        public override void UpdateGraphics(NGraphics graphics)
        {
            base.UpdateGraphics(graphics);
            var block = graphics.materialPropertyBlock;
            if (_format.glowPower > 0)
            {
                graphics.ToggleKeyword("GLOW_ON", true);
                block.SetFloat(ShaderUtilities.ID_GlowPower, _format.glowPower);
                block.SetFloat(ShaderUtilities.ID_GlowInner, _format.glowInner);
                block.SetFloat(ShaderUtilities.ID_GlowOuter, _format.glowOuter);
                block.SetColor(ShaderUtilities.ID_GlowColor, _format.glowColor);
            }
            else
            {
                graphics.ToggleKeyword("GLOW_ON", false);
                block.SetFloat(ShaderUtilities.ID_GlowPower, 0);
            }
        }
    }
}
