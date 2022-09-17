using EntityStates.BrotherMonster;
using RoR2;
using UnityEngine;
using EntityStates;

namespace UmbralMithrix
{
  public class EnterCrushingLeap : BaseSkillState
  {
    public static float damageCoefficient = 20f;
    public static float projectileDamageCoefficient = 5f;
    public static float projectileCount = 8f;
    public static float projectileForce = 5f;
    public static float baseDuration = 0.5f;
    private float duration;

    public override void OnEnter()
    {
      base.OnEnter();
      int num = (int)Util.PlaySound(EnterSkyLeap.soundString, this.gameObject);
      this.duration = EnterCrushingLeap.baseDuration / this.attackSpeedStat;
      this.PlayAnimation("Body", "EnterSkyLeap", "SkyLeap.playbackRate", this.duration);
      this.PlayAnimation("FullBody Override", "BufferEmpty");
      this.characterDirection.moveVector = this.characterDirection.forward;
      this.characterBody.AddTimedBuff((BuffIndex)1, EnterSkyLeap.baseDuration);
      AimAnimator aimAnimator = this.GetAimAnimator();
      if (!(bool)(Object)aimAnimator)
        return;
      aimAnimator.enabled = true;
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if (!this.isAuthority || (double)((EntityState)this).fixedAge <= (double)this.duration)
        return;
      this.outer.SetNextState(new AimCrushingLeap());
    }
  }
}
