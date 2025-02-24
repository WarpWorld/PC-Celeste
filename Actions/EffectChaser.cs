using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CrowdControl.Actions;

// ReSharper disable once UnusedMember.Global
public class EffectChaser : Effect
{
    public override string Code { get; } = "chaser";

    public override EffectType Type { get; } = EffectType.Timed;

    public override TimeSpan DefaultDuration { get; } = TimeSpan.FromSeconds(30);

    public BadelineOldsite? Chaser;

    public AudioTrackState? Music;

    public override void Load()
    {
        base.Load();
        On.Celeste.AudioState.Apply += OnAudioStateApply;
    }

    public override void Unload()
    {
        base.Unload();
        On.Celeste.AudioState.Apply -= OnAudioStateApply;
    }

    public override bool IsReady()
    {
        if (!(Player?.Active ?? false)) return false;
        if ((Level == null) || Level.Entities.Contains(Chaser) || Level.Entities.ToAdd.Contains(Chaser)) return false;
        
        return true;
    }

    public override void Start() => Start(true);

    private void Start(bool setTicking)
    {
        base.Start();
        
        Music = Level.Session.Audio.Music.Clone();
        Chaser = new(Player.Position + new Vector2(0f, -8f), 0);
        Level.Add(Chaser);
        if (setTicking) IsTimerTicking = true;
    }

    public override void Update(GameTime gameTime)
    {
        Paused = !Chaser.Collidable;
        base.Update(gameTime);
        if (IsReady()) Start(false);
    }

    public override void End()
    {
        base.End();
        Level? level = CrowdControlHelper.Instance.Level;
        if (level == null || Chaser == null) return;

        Audio.Play("event:/char/badeline/disappear", Chaser.Position);

        level.Displacement.AddBurst(Chaser.Center, 0.5f, 24f, 96f, 0.4f);
        level.Particles.Emit(BadelineOldsite.P_Vanish, 12, Chaser.Center, Vector2.One * 6f);

        level.Remove(Chaser);
        Chaser = null;
    }

    public void OnAudioStateApply(On.Celeste.AudioState.orig_Apply orig, AudioState state)
    {
        // If we're about to play the oldsite chase song while we've got an old state clone,
        // undo the changes by reapplying the clone, then don't apply the changes.
        /*if (Music != null && state.Music.Event == Sfxs.music_oldsite_chase) {
            state.Music = Music;
            Music = null;
            return;
        }*/

        orig(state);
    }

}