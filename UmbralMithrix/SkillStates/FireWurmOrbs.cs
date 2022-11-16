using RoR2;
using RoR2.Projectile;
using UnityEngine;
using EntityStates;
using EntityStates.VagrantMonster.Weapon;
using EntityStates.BrotherMonster;

namespace UmbralMithrix
{
  public class FireWurmOrbs : BaseState
  {
    private float stopwatch;
    private float missileStopwatch;
    public static float baseDuration;
    public static string muzzleString = "MouthMuzzle";
    public static float missileSpawnFrequency;
    public static float missileSpawnDelay;
    public static float damageCoefficient;
    public static float maxSpread;
    public static GameObject projectilePrefab;
    public static GameObject muzzleflashPrefab;
    private ChildLocator childLocator;

    public override void OnEnter()
    {
      base.OnEnter();
      this.missileStopwatch -= JellyBarrage.missileSpawnDelay;
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
      int num2 = (bool)(Object)this.childLocator.FindChild("MouthMuzzle") ? 1 : 0;
    }

    private void FireBlob(Ray projectileRay, float bonusPitch, float bonusYaw)
    {
      int num = 120 / (int)ModConfig.WurmOrbs.Value;
      Vector3 xAxis = Vector3.ProjectOnPlane(this.characterDirection.forward, Vector3.up);
      // projectileRay.direction = Util.ApplySpread(projectileRay.direction, 0.0f, JellyBarrage.maxSpread, 1f, 1f, bonusYaw, bonusPitch);
      for (int i = 0; i < (int)ModConfig.WurmOrbs.Value; i++)
      {
        Vector3 forward = Quaternion.AngleAxis(num * i, Vector3.up) * xAxis;
        ProjectileManager.instance.FireProjectile(JellyBarrage.projectilePrefab, projectileRay.origin, Util.QuaternionSafeLookRotation(forward), this.gameObject, this.damageStat * FistSlam.waveProjectileDamageCoefficient, 0.0f, Util.CheckRoll(this.critStat, this.characterBody.master));
      }
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      this.stopwatch += Time.fixedDeltaTime;
      this.missileStopwatch += Time.fixedDeltaTime;
      if ((double)this.missileStopwatch >= 1.0 / (double)JellyBarrage.missileSpawnFrequency)
      {
        this.missileStopwatch -= 1f / JellyBarrage.missileSpawnFrequency;
        Transform child = this.childLocator.FindChild("MouthMuzzle");
        if ((bool)(Object)child)
        {
          Ray projectileRay = new Ray();
          projectileRay.origin = child.position;
          projectileRay.direction = this.GetAimRay().direction;
          float maxDistance = 1000f;
          RaycastHit hitInfo;
          if (Physics.Raycast(this.GetAimRay(), out hitInfo, maxDistance, (int)LayerIndex.world.mask))
            projectileRay.direction = hitInfo.point - child.position;
          this.FireBlob(projectileRay, 0.0f, 0.0f);
        }
      }
      if ((double)this.stopwatch < (double)JellyBarrage.baseDuration || !this.isAuthority)
        return;
      this.outer.SetNextStateToMain();
    }
  }
}