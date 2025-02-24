using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ConnectorLib.JSON;
using FMOD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectHypeTrain : Effect
{
    public override string Code { get; } = "event-hype-train";

    public override EffectType Type { get; } = EffectType.Timed;

    public override TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(30);

    private Vector2 m_trainPosition;
    private Vector2 m_trainVelocity;
    private float m_trainDirection;

    private TrainEntity? m_trainEntity;

    private readonly Dictionary<string, HypeTrainSourceDetails.Contribution> m_contributions = new();

    private Rectangle? m_cameraBounds;
    private Vector2? m_trainSize;

    private static Texture2D? m_texTrainFront;
    private static Texture2D? m_texTrainBox;
    private static Texture2D? m_texTrainCoal;
    private static Texture2D? m_texTrainTank;

    private void TryGetTrainTextures()
    {
        m_texTrainFront ??= CrowdControlHelper.Instance.GraphicsDevice.LoadEmbeddedTexture("Content\\HypeTrain\\front2_64.png");
        m_texTrainBox ??= CrowdControlHelper.Instance.GraphicsDevice.LoadEmbeddedTexture("Content\\HypeTrain\\box_64.png");
        m_texTrainCoal ??= CrowdControlHelper.Instance.GraphicsDevice.LoadEmbeddedTexture("Content\\HypeTrain\\coal_64.png");
        m_texTrainTank ??= CrowdControlHelper.Instance.GraphicsDevice.LoadEmbeddedTexture("Content\\HypeTrain\\tank_64.png");
    }

    public override void Load()
    {
        base.Load();
        TryGetTrainTextures();
    }

    public override void Start()
    {
        base.Start();
        if (!Active || (Player == null)) return;

        m_cameraBounds = Engine.Viewport.Bounds;
        
        if (!m_cameraBounds.HasValue) return;
        if (m_texTrainFront == null) return;

        m_contributions.Clear();
        // ReSharper disable RedundantAlwaysMatchSubpattern
        {
            if (CurrentRequest?.sourceDetails is HypeTrainSourceDetails { TopContributions: not null } sourceDetails)
                foreach (HypeTrainSourceDetails.Contribution contribution in sourceDetails.TopContributions)
                    m_contributions[contribution.UserName] = contribution;
        }
        {
            if (CurrentRequest?.sourceDetails is HypeTrainSourceDetails { LastContribution: not null } sourceDetails)
                m_contributions[sourceDetails.LastContribution.UserName] = sourceDetails.LastContribution;
        }
        // ReSharper restore RedundantAlwaysMatchSubpattern

        m_trainSize = CalculateTotalTrainSize();
        if (!m_trainSize.HasValue) return;

        m_trainPosition = new Vector2(m_cameraBounds.Value.Left - (m_texTrainFront.Width * TRAIN_SCALE), m_cameraBounds.Value.Center.Y);
        m_trainVelocity = new Vector2(MathF.Min(80, MathF.Max(320, 32f * ((CurrentRequest?.sourceDetails as HypeTrainSourceDetails)?.Level ?? 1f))), 0f);
        m_trainDirection = MathF.Sign(m_trainVelocity.X);

        m_trainEntity = new(this, m_trainSize.Value, m_trainVelocity);
        Scene.Entities.Add(m_trainEntity);
        //Scene.Entities.UpdateLists();
    }

    public override void End()
    {
        base.End();
        if (m_trainEntity == null) return;
        Scene.Entities.Remove(m_trainEntity);
        //Scene.Entities.UpdateLists();
    }

    private const float SCALING = 4f;
    private static readonly Vector2 SHRINK = new Vector2(4f, 8f) * SCALING;
    private static readonly Vector2 HALF_SHRINK = SHRINK / 2f;

    public class TrainEntity : Solid
    {
        private readonly EffectHypeTrain m_effect;

        private Vector2 m_storedPosition;
        
        public TrainEntity(EffectHypeTrain effect, Vector2 size, Vector2 velocity) : base(effect.Camera.ScreenToCamera(new(
            ((Engine.Viewport.Bounds.Location.ToVector2().X - size.X) + HALF_SHRINK.X) / SCALING,
            (Engine.Viewport.Bounds.Center.ToVector2().Y + HALF_SHRINK.Y) / SCALING
        )), (size.X - SHRINK.X) / SCALING, (size.Y - SHRINK.Y) / SCALING, false)
        {
            System.Diagnostics.Debug.Assert(size.X > size.Y, "Train cannot be taller than it is wide.");
            m_effect = effect;
            Speed = velocity / SCALING;
            AllowStaticMovers = true;
        }


        public override void Added(Scene scene)
        {
            base.Added(scene);
            Position = m_storedPosition;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            m_storedPosition = Position;
        }

        public override void Update()
        {
            base.Update();
            if ((!Active) || (!m_effect.Active) || (m_effect.Player == null)) return;

            Rectangle? cameraBounds = CrowdControlHelper.Instance.Camera?.Viewport.Bounds;
            if (!cameraBounds.HasValue) return;

            m_effect.m_trainPosition += m_effect.m_trainVelocity * Engine.DeltaTime;
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Collider?.Render(camera);
        }

        public override void Render()
        {
            base.Render();
            Collider?.Render(m_effect.Camera);
        }
    }

    private Vector2 CalculateTotalTrainSize()
    {
        Vector2 totalSize;
        Texture2D? tex = m_texTrainFront;
        if (tex == null) return Vector2.Zero;
        
        totalSize.X = (tex.Width * TRAIN_SCALE);
        totalSize.Y = (tex.Height * TRAIN_SCALE);
        
        foreach (var contribution in m_contributions)
        {
            tex = (string.Equals(contribution.Value.Type, "bits") ? m_texTrainCoal : m_texTrainBox)!;
            if (tex == null) continue;
            
            float nextOffsetY;
            if (tex == m_texTrainCoal)
                nextOffsetY = -4f;
            else if (tex == m_texTrainBox)
                nextOffsetY = -4f;
            else if (tex == m_texTrainTank)
                nextOffsetY = -5f;
            else
                nextOffsetY = 0f;

            totalSize.X += (tex.Width * TRAIN_SCALE);
            totalSize.Y = Math.Max(totalSize.Y, (tex.Height * TRAIN_SCALE) + Math.Abs(nextOffsetY));
        }
        
        return totalSize;
    }

    private const float TRAIN_SCALE = 1.5f;

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!Active || (Player == null)) return;

        if (!Scene.Entities.Contains(m_trainEntity))
        {
            Scene.Entities.Add(m_trainEntity);
            //Scene.Entities.UpdateLists();
        }
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        if (!Active || (Player == null)) return;

        Texture2D? tex = m_texTrainFront;
        if (tex == null) return;
        SpriteEffects effects = (m_trainDirection > 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        
        Monocle.Draw.SpriteBatch.Draw(tex, m_trainPosition, null, Color.White, 0f, Vector2.Zero, TRAIN_SCALE, effects, 1f);
        
        float nextOffsetX = m_trainPosition.X + ((m_trainDirection < 0) ? (tex.Width * TRAIN_SCALE) : 0f);
        foreach (var contribution in m_contributions)
        {
            tex = (string.Equals(contribution.Value.Type, "bits") ? m_texTrainCoal : m_texTrainBox)!;
            nextOffsetX += (tex.Width * TRAIN_SCALE) * -m_trainDirection;
            float nextOffsetY;
            if (tex == m_texTrainCoal)
                nextOffsetY = -4f;
            else if (tex == m_texTrainBox)
                nextOffsetY = -4f;
            else if (tex == m_texTrainTank)
                nextOffsetY = -5f;
            else
                nextOffsetY = 0f;

            Monocle.Draw.SpriteBatch.Draw(tex, new Vector2((int)nextOffsetX, (int)(m_trainPosition.Y + nextOffsetY)), null, Color.White, 0f, Vector2.Zero, TRAIN_SCALE, effects, 1f);
            
            string carText = contribution.Value.UserName;
            Vector2 halfText = ActiveFont.Measure(carText) / 8f;
            ActiveFont.DrawOutline(
                carText,
                new Vector2(nextOffsetX + (((tex.Width * TRAIN_SCALE) * m_trainDirection) / 2f) - halfText.X, m_trainPosition.Y + nextOffsetY + ((tex.Height * TRAIN_SCALE) / 2f) - halfText.Y),
                Vector2.Zero,
                Vector2.One * 0.25f,
                Color.White,
                1f,
                Color.Black
            );
        }

        if ((m_trainVelocity.X > 0) && (nextOffsetX > m_cameraBounds.Value.Right))
            TryStop();
        else if ((m_trainVelocity.X < 0) && ((nextOffsetX + (tex.Width * TRAIN_SCALE)) < m_cameraBounds.Value.Left))
            TryStop();
    }
}