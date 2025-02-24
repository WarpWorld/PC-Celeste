using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectLaughter : Effect
{
    public override string Code { get; } = "laughter";

    public override EffectType Type { get; } = EffectType.Timed;

    public override TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(15);

    public Hahaha? Laughter;
    private readonly FieldInfo autoTriggerLaughOrigin = typeof(Hahaha).GetField("autoTriggerLaughOrigin", BindingFlags.Instance | BindingFlags.NonPublic);

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!Active || Level == null || (Player == null)) { return; }

        if (Level.Entities.Contains(Laughter) || Level.Entities.ToAdd.Contains(Laughter))
        {
            autoTriggerLaughOrigin.SetValue(Laughter, Laughter.Position = Player.Position);
        }
        else
        {
            Laughter ??= new(Player.Position, string.Empty, true, Player.Position);
            Level.Add(Laughter);
            Laughter.Enabled = true;
        }
    }

    public override void End()
    {
        base.End();
        if ((Laughter == null) || (Level == null)) { return; }

        Level.Remove(Laughter);
        Laughter = null;
    }
}