using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectHypeTrain : Effect
{
    public override string Code { get; } = "hypeTrain";

    public override EffectType Type { get; } = EffectType.Timed;

    public override TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(10);

    private Texture2D m_trainTexture;

    public override void Start()
    {
        base.Start();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!Active || (Engine.Scene is not Level level) || (Player == null)) { return; }


    }
}