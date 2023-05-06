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

    public Hahaha Laughter;
    private FieldInfo autoTriggerLaughOrigin = typeof(Hahaha).GetField("autoTriggerLaughOrigin", BindingFlags.Instance | BindingFlags.NonPublic);

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        Player player = Player;
        if (!Active || !(Engine.Scene is Level level) || (player == null)) { return; }

        if (level.Entities.Contains(Laughter) || level.Entities.GetToAdd().Contains(Laughter))
        {
            autoTriggerLaughOrigin.SetValue(Laughter, Laughter.Position = player.Position);
        }
        else
        {
            Laughter ??= new Hahaha(player.Position, string.Empty, true, player.Position);
            level.Add(Laughter);
            Laughter.Enabled = true;
        }
    }

    public override void End()
    {
        base.End();
        if ((Laughter == null) || (!(Engine.Scene is Level level))) { return; }

        level.Remove(Laughter);
        Laughter = null;
    }
}