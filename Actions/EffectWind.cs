using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions
{
    // ReSharper disable once UnusedMember.Global
    public class EffectWind : Effect
    {
        public override string Code { get; } = "wind";

        public WindController Wind;
        private WindController.Patterns _old_pattern;

        public override EffectType Type { get; } = EffectType.Timed;

        public override TimeSpan Duration { get; } = TimeSpan.FromSeconds(30);

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!Active || (!(Engine.Scene is Level level)) || level.Entities.Contains(Wind) || level.Entities.GetToAdd().Contains(Wind)) { return; }

            WindController controller = level.Entities.FindFirst<WindController>();

            if (controller == null)
            {
                level.Foreground.Backdrops.Add(new WindSnowFG { Alpha = 0f });
                Wind = controller = new WindController(WindController.Patterns.Alternating);
                controller.SetStartPattern();
                level.Add(controller);
            }
            else
            {
                Wind = controller;
                controller.SetPattern(WindController.Patterns.Alternating);
            }
        }

        public override void End()
        {
            base.End();
            if (Wind == null) { return; }

            Wind.SetPattern(WindController.Patterns.None);
            if (Engine.Scene is Level level) { level.Remove(Wind); }
            Wind = null;
        }
    }
}