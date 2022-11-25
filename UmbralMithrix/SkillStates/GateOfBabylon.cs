using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using EntityStates;
using EntityStates.VagrantMonster.Weapon;
using EntityStates.BrotherMonster;
using EntityStates.BrotherMonster.Weapon;
using System.Collections.Generic;

namespace UmbralMithrix
{
  public class GateOfBabylon : BaseState
  {
    private float stopwatch;
    private float missileStopwatch;
    public static float baseDuration = 4;
    public static string muzzleString = FireLunarShards.muzzleString;
    public static float missileSpawnFrequency = 12;
    public static float missileSpawnDelay = 0;
    public static float damageCoefficient;
    public static float maxSpread = 1;
    public static GameObject projectilePrefab;
    public static GameObject muzzleflashPrefab;
    private ChildLocator childLocator;
    static System.Random rand = new();
    public override void OnEnter()
    {
      base.OnEnter();
      this.missileStopwatch -= missileSpawnDelay;
      if ((bool)(Object)this.sfxLocator && this.sfxLocator.barkSound != "")
      {
        int num1 = (int)Util.PlaySound(this.sfxLocator.barkSound, this.gameObject);
      }
      Transform modelTransform = this.GetModelTransform();
      if (!(bool)(Object)modelTransform)
        return;
      this.childLocator = modelTransform.GetComponent<ChildLocator>();
      if (!(bool)(Object)this.childLocator)
        return;
      int num2 = (bool)(Object)this.childLocator.FindChild(muzzleString) ? 1 : 0;
    }
    private void FireBlob(Ray projectileRay, float bonusPitch, float bonusYaw)
    {
      this.PlayAnimation("Gesture, Additive", nameof(FireLunarShards));
      this.PlayAnimation("Gesture, Override", nameof(FireLunarShards));
      int randIdx = rand.Next(UmbralMithrix.weaponsList.Count);
      UmbralMithrix.noblePhantasmGhost.GetComponentInChildren<MeshFilter>().sharedMesh = UmbralMithrix.weaponsList[randIdx].GetComponent<MeshFilter>().sharedMesh;
      FireProjectileInfo noblePhantasm = new FireProjectileInfo()
      {
        position = projectileRay.origin,
        rotation = Util.QuaternionSafeLookRotation(projectileRay.direction),
        crit = Util.CheckRoll(this.critStat, this.characterBody.master),
        damage = this.damageStat * FistSlam.waveProjectileDamageCoefficient,
        owner = this.gameObject,
        force = EntityStates.BrotherMonster.FistSlam.waveProjectileForce,
        speedOverride = 100,
        projectilePrefab = UmbralMithrix.noblePhantasm
      };
      ProjectileManager.instance.FireProjectile(noblePhantasm);
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      this.stopwatch += Time.fixedDeltaTime;
      this.missileStopwatch += Time.fixedDeltaTime;
      if ((double)this.missileStopwatch >= 1.0 / (double)missileSpawnFrequency)
      {
        this.missileStopwatch -= 1f / missileSpawnFrequency;
        Transform child = this.childLocator.FindChild(muzzleString);
        if ((bool)(Object)child)
        {
          Ray aimRay = this.GetAimRay();
          float maxDistance = 1000f;
          float randX = UnityEngine.Random.Range(-25f, 25f);
          float randY = UnityEngine.Random.Range(0f, 15f);
          float randZ = UnityEngine.Random.Range(-25f, 25f);
          Vector3 randVector = new Vector3(randX, randY, randZ);
          Vector3 position = new Vector3(child.position.x, child.position.y, child.position.z) + randVector;
          aimRay.origin = position;
          RaycastHit hitInfo;
          {
            if (Physics.Raycast(aimRay, out hitInfo, maxDistance, (int)LayerIndex.world.mask))
              aimRay.direction = hitInfo.point - aimRay.origin;
            this.FireBlob(aimRay, 0.0f, 0.0f);
          }
        }
        if ((double)this.stopwatch < (double)baseDuration || !this.isAuthority)
          return;
        this.outer.SetNextStateToMain();
      }
    }
  }
}