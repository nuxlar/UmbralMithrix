using EntityStates.BrotherMonster;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using EntityStates;
using System;
using UnityEngine.AddressableAssets;

namespace UmbralMithrix
{
  public class ExitCrushingLeap : BaseSkillState
  {
    private float duration;
    private float baseDuration = 0.5f;
    SpawnCard MithrixGlassCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Junk/BrotherGlass/cscBrotherGlass.asset").WaitForCompletion();

    public override void OnEnter()
    {
      base.OnEnter();
      this.duration = ExitSkyLeap.baseDuration / this.attackSpeedStat;
      int num = (int)Util.PlaySound(ExitSkyLeap.soundString, this.gameObject);
      this.PlayAnimation("Body", nameof(ExitSkyLeap), "SkyLeap.playbackRate", this.duration);
      this.PlayAnimation("FullBody Override", "BufferEmpty");
      this.characterBody.AddTimedBuff(RoR2Content.Buffs.ArmorBoost, ExitSkyLeap.baseDuration);
      AimAnimator aimAnimator = this.GetAimAnimator();
      if ((bool)(UnityEngine.Object)aimAnimator)
        ((Behaviour)aimAnimator).enabled = true;
      float num2 = 360f / (float)ExitSkyLeap.waveProjectileCount;
      Vector3 vector3_1 = Vector3.ProjectOnPlane(this.inputBank.aimDirection, Vector3.up);
      Vector3 footPosition = this.characterBody.footPosition;
      for (int index = 0; index < ExitSkyLeap.waveProjectileCount; ++index)
      {
        Vector3 vector3_2 = Quaternion.AngleAxis(num2 * (float)index, Vector3.up) * vector3_1;
        ProjectileManager.instance.FireProjectile(ExitSkyLeap.waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(vector3_2), this.gameObject, this.characterBody.damage * ExitSkyLeap.waveProjectileDamageCoefficient, ExitSkyLeap.waveProjectileForce, Util.CheckRoll(this.characterBody.crit, this.characterBody.master), (DamageColorIndex)0, (GameObject)null, -1f);
      }
      if (!(bool)PhaseCounter.instance)
        return;
      if (PhaseCounter.instance.phase == 1)
        return;
      if (PhaseCounter.instance.phase == 2)
      {
        for (int index = 0; index < 2; ++index)
        {
          DirectorPlacementRule placementRule = new DirectorPlacementRule();
          placementRule.placementMode = DirectorPlacementRule.PlacementMode.Approximate;
          placementRule.minDistance = 3f;
          placementRule.maxDistance = 20f;
          placementRule.spawnOnTarget = this.gameObject.transform;
          Xoroshiro128Plus rng = RoR2Application.rng;
          DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(MithrixGlassCard, placementRule, rng);
          directorSpawnRequest.summonerBodyObject = this.gameObject;
          directorSpawnRequest.onSpawnedServer += (Action<SpawnCard.SpawnResult>)(spawnResult => spawnResult.spawnedInstance.GetComponent<Inventory>().GiveItem(RoR2Content.Items.HealthDecay, ExitSkyLeap.cloneDuration));
          DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
        }
      }
      GenericSkill genericSkill = (bool)this.skillLocator ? this.skillLocator.special : null;
      if (!(bool)genericSkill)
        return;
      if (PhaseCounter.instance.phase == 2)
        UltChannelState.replacementSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(LunarDevastationEnter));
      else
        UltChannelState.replacementSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(UltEnterState));
      genericSkill.SetSkillOverride(this.outer, UltChannelState.replacementSkillDef, GenericSkill.SkillOverridePriority.Contextual);
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if (!this.isAuthority)
        return;
      if ((double)this.fixedAge <= (double)this.duration)
        return;
      this.outer.SetNextStateToMain();
    }
  }
}