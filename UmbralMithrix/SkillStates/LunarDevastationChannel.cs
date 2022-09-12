using RoR2;
using RoR2.Projectile;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.AddressableAssets;
using EntityStates.BrotherHaunt;
using EntityStates.BrotherMonster;
using EntityStates.BrotherMonster.Weapon;
using UmbralMithrix;
using System.Collections.Generic;

public class LunarDevastationChannel : EntityStates.BaseState
{
  static GameObject golemProjectile = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarGolem/LunarGolemTwinShotProjectile.prefab").WaitForCompletion();
  public int totalWaves = ModConfig.P2UltCount.Value;
  public float maxDuration = ModConfig.P2UltDuration.Value;
  private int wavesFired;
  private int charges;
  private float chargeTimer;
  private float elapsed;
  private GameObject channelEffectInstance;

  public override void OnEnter()
  {
    this.charges = FireRandomProjectiles.initialCharges;
    int num = (int)Util.PlaySound(UltChannelState.enterSoundString, this.gameObject);
    Transform modelChild = this.FindModelChild("MuzzleUlt");
    if ((bool)(Object)modelChild && (bool)(Object)UltChannelState.channelEffectPrefab)
      this.channelEffectInstance = Object.Instantiate<GameObject>(UltChannelState.channelEffectPrefab, modelChild.position, Quaternion.identity, modelChild);
    if (!(bool)(Object)UltChannelState.channelBeginMuzzleflashEffectPrefab)
      return;
    EffectManager.SimpleMuzzleFlash(UltChannelState.channelBeginMuzzleflashEffectPrefab, this.gameObject, "MuzzleUlt", false);
  }

  private void FireWave()
  {
    ++this.wavesFired;
    float num = 360f / ModConfig.P2UltOrbCount.Value;
    Vector3 point = Vector3.ProjectOnPlane(this.inputBank.aimDirection, Vector3.up);
    Vector3 footPosition = this.characterBody.footPosition;
    Vector3 corePosition = this.characterBody.corePosition;
    Util.PlaySound(ExitSkyLeap.soundString, this.gameObject);
    EffectManager.SimpleMuzzleFlash(FistSlam.slamImpactEffect, this.gameObject, FistSlam.muzzleString, false);
    for (int idx = 0; idx < ModConfig.P2UltOrbCount.Value; ++idx)
    {
      Vector3 forward = Quaternion.AngleAxis(num * idx, Vector3.up) * point;
      ProjectileManager.instance.FireProjectile(golemProjectile, corePosition, Quaternion.LookRotation(forward), this.gameObject, (this.characterBody.damage * FistSlam.waveProjectileDamageCoefficient) / 4, 0f, Util.CheckRoll(this.characterBody.crit, this.characterBody.master), DamageColorIndex.Default, null, -1f);
      ProjectileManager.instance.FireProjectile(ExitSkyLeap.waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), this.gameObject, this.characterBody.damage * ExitSkyLeap.waveProjectileDamageCoefficient, ExitSkyLeap.waveProjectileForce, Util.CheckRoll(this.characterBody.crit, this.characterBody.master));
    }

  }

  private void FireRandomFlameLine()
  {
    NodeGraph groundNodes = SceneInfo.instance.groundNodes;
    if (!(bool)(Object)groundNodes)
      return;
    List<NodeGraph.NodeIndex> withFlagConditions = groundNodes.GetActiveNodesForHullMaskWithFlagConditions(HullMask.Golem, NodeFlags.None, NodeFlags.NoCharacterSpawn);
    NodeGraph.NodeIndex nodeIndex = withFlagConditions[Random.Range(0, withFlagConditions.Count)];
    --this.charges;
    Vector3 position;
    groundNodes.GetNodePosition(nodeIndex, out position);
    ProjectileManager.instance.FireProjectile(new FireProjectileInfo()
    {
      projectilePrefab = FireRandomProjectiles.projectilePrefab,
      owner = this.gameObject,
      damage = this.damageStat * 15,
      position = position + Vector3.up * FireRandomProjectiles.projectileVerticalOffset,
      rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360f), 0.0f)
    });
  }

  public override void FixedUpdate()
  {
    base.FixedUpdate();
    if (!this.isAuthority)
      return;
    elapsed += Time.deltaTime;
    if (Mathf.CeilToInt(this.fixedAge / this.maxDuration * (float)this.totalWaves) > this.wavesFired)
      this.FireWave();
    this.chargeTimer -= Time.fixedDeltaTime;
    if ((double)this.chargeTimer <= 0.0)
    {
      this.chargeTimer = 0.06f;
      this.charges = Mathf.Min(this.charges + 1, 150);
    }
    if ((double)Random.value >= (double)0.5 || this.charges <= 0)
      return;
    this.FireRandomFlameLine();
    if (elapsed >= 0.5f)
    {
      Ray aimRay = this.GetAimRay();
      elapsed = elapsed % 0.5f;
      for (int i = 0; i < 12; i++)
      {
        Util.PlaySound(EntityStates.BrotherMonster.Weapon.FireLunarShards.fireSound, this.gameObject);
        ProjectileManager.instance.FireProjectile(FireLunarShards.projectilePrefab, aimRay.origin, Quaternion.LookRotation(aimRay.direction), this.gameObject, this.characterBody.damage * 0.1f / 12f, 0f, Util.CheckRoll(this.characterBody.crit, this.characterBody.master), DamageColorIndex.Default, null, -1f);
      }
    }
    if ((double)this.fixedAge <= (double)this.maxDuration)
      return;
    this.outer.SetNextState(new LunarDevastationExit());
  }

  public override void OnExit()
  {
    int num = (int)Util.PlaySound(UltChannelState.exitSoundString, this.gameObject);
    if ((bool)(Object)this.channelEffectInstance)
      EntityStates.EntityState.Destroy((Object)this.channelEffectInstance);
    base.OnExit();
  }
}