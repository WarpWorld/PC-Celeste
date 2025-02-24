using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectWind : Effect
{
    public override string Code { get; } = "wind";

    public WindController Wind;
    private WindController.Patterns _old_pattern;

    public override EffectType Type { get; } = EffectType.Timed;

    public override TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(30);

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!Active || (Level == null) || Level.Entities.Contains(Wind) || Level.Entities.ToAdd.Contains(Wind)) { return; }

        WindController controller = Level.Entities.FindFirst<WindController>();

        if (controller == null)
        {
            Level.Foreground.Backdrops.Add(new WindSnowFG { Alpha = 0f });
            Wind = controller = new(WindController.Patterns.Alternating);
            controller.SetStartPattern();
            Level.Add(controller);
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
        if (Engine.Scene is Level Level) { Level.Remove(Wind); }
        Wind = null;
    }
}