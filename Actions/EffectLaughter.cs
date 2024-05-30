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
        if (!Active || Engine.Scene is not Level level || (Player == null)) { return; }

        if (level.Entities.Contains(Laughter) || level.Entities.GetToAdd().Contains(Laughter))
        {
            autoTriggerLaughOrigin.SetValue(Laughter, Laughter.Position = Player.Position);
        }
        else
        {
            Laughter ??= new(Player.Position, string.Empty, true, Player.Position);
            level.Add(Laughter);
            Laughter.Enabled = true;
        }
    }

    public override void End()
    {
        base.End();
        if ((Laughter == null) || (Engine.Scene is not Level level)) { return; }

        level.Remove(Laughter);
        Laughter = null;
    }
}