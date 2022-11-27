using EntityStates.VoidRaidCrab;
using RoR2;
using RoR2.Audio;
using System;
using UnityEngine;
using EntityStates;

namespace UmbralMithrix
{
  public class FireWurmLaser : BaseSkillState
  {
    public static AnimationCurve revolutionsCurve;
    public static GameObject beamVfxPrefab;
    public static float beamRadius = 16f;
    public static float beamMaxDistance = 400f;
    public static float beamDpsCoefficient = 1f;
    public static float beamTickFrequency = 4f;
    public float baseDuration = 5;
    public static GameObject beamImpactEffectPrefab = SpinBeamAttack.beamImpactEffectPrefab;
    public static LoopSoundDef loopSound = SpinBeamAttack.loopSound;
    public static string enterSoundString = SpinBeamAttack.enterSoundString;
    private float beamTickTimer;
    private LoopSoundManager.SoundLoopPtr loopPtr;
    public AnimationCurve headForwardYCurve = new BaseSpinBeamAttackState().headForwardYCurve;
    private Transform headTransform;
    private Transform muzzleTransform;
    protected float duration { get; private set; }
    protected GameObject beamVfxInstance { get; private set; }

    public override void OnEnter()
    {
      base.OnEnter();
      this.headTransform = this.FindModelChild("HeadCenter");
      this.muzzleTransform = this.FindModelChild("MouthMuzzle");
      this.duration = this.baseDuration;

      this.CreateBeamVFXInstance(SpinBeamAttack.beamVfxPrefab);
      this.loopPtr = LoopSoundManager.PlaySoundLoopLocal(this.gameObject, SpinBeamAttack.loopSound);
      int num = (int)Util.PlaySound(SpinBeamAttack.enterSoundString, this.gameObject);
    }

    public override void OnExit()
    {
      LoopSoundManager.StopSoundLoopLocal(this.loopPtr);
      this.DestroyBeamVFXInstance();
      base.OnExit();
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if ((double)this.fixedAge >= (double)this.duration && this.isAuthority)
        this.outer.SetNextStateToMain();
      if (this.isAuthority)
      {
        if ((double)this.beamTickTimer <= 0.0)
        {
          this.beamTickTimer += 1f / SpinBeamAttack.beamTickFrequency;
          this.FireBeamBulletAuthority();
        }
        this.beamTickTimer -= Time.fixedDeltaTime;
      }
    }

    private void FireBeamBulletAuthority()
    {
      Ray beamRay = this.GetBeamRay();
      new BulletAttack()
      {
        muzzleName = "MouthMuzzle",
        origin = beamRay.origin,
        aimVector = beamRay.direction,
        minSpread = 0.0f,
        maxSpread = 0.0f,
        maxDistance = 400f,
        hitMask = LayerIndex.CommonMasks.bullet,
        stopperMask = ((LayerMask)0),
        bulletCount = 1U,
        radius = beamRadius,
        smartCollision = false,
        queryTriggerInteraction = QueryTriggerInteraction.Ignore,
        procCoefficient = 1f,
        procChainMask = new ProcChainMask(),
        owner = this.gameObject,
        weapon = this.gameObject,
        damage = (SpinBeamAttack.beamDpsCoefficient * this.damageStat / SpinBeamAttack.beamTickFrequency),
        damageColorIndex = DamageColorIndex.Default,
        damageType = DamageType.Generic,
        falloffModel = BulletAttack.FalloffModel.None,
        force = 0.0f,
        hitEffectPrefab = SpinBeamAttack.beamImpactEffectPrefab,
        tracerEffectPrefab = ((GameObject)null),
        isCrit = false,
        HitEffectNormal = false
      }.Fire();
    }

    protected Ray GetBeamRay()
    {
      Vector3 forward1 = this.muzzleTransform.forward;
      Vector3 forward2 = this.headTransform.forward;
      return new Ray(this.muzzleTransform.position, forward2);
    }

    protected void CreateBeamVFXInstance(GameObject beamVfxPrefab)
    {
      this.beamVfxInstance = this.beamVfxInstance == null ? UnityEngine.Object.Instantiate<GameObject>(beamVfxPrefab) : throw new InvalidOperationException();
      this.beamVfxInstance.transform.SetParent(this.headTransform, true);
      this.UpdateBeamTransforms();
      RoR2Application.onLateUpdate += new Action(this.UpdateBeamTransformsInLateUpdate);
    }

    protected void DestroyBeamVFXInstance()
    {
      if (this.beamVfxInstance == null)
        return;
      RoR2Application.onLateUpdate -= new Action(this.UpdateBeamTransformsInLateUpdate);
      VfxKillBehavior.KillVfxObject(this.beamVfxInstance);
      this.beamVfxInstance = (GameObject)null;
    }

    private void UpdateBeamTransformsInLateUpdate()
    {
      try
      {
        this.UpdateBeamTransforms();
      }
      catch (Exception ex)
      {
        Debug.LogException(ex);
      }
    }

    private void UpdateBeamTransforms()
    {
      Ray beamRay = this.GetBeamRay();
      this.beamVfxInstance.transform.SetPositionAndRotation(beamRay.origin, Quaternion.LookRotation(beamRay.direction));
    }
  }
}
