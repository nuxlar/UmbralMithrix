using RoR2;
using RoR2.Projectile;
using UnityEngine;
using EntityStates;
using EntityStates.VagrantMonster.Weapon;

namespace UmbralMithrix
{
  public class FireWurmOrbs : BaseState
  {
    private float stopwatch;
    private float missileStopwatch;
    public static float stormDuration;
    public static float stormToIdleTransitionDuration;
    public static string stormPointChildString;
    public static float missileSpawnFrequency;
    public static float missileSpawnDelay;
    public static int missileTurretCount;
    public static float missileTurretYawFrequency;
    public static float missileTurretPitchFrequency;
    public static float missileTurretPitchMagnitude;
    public static float missileSpeed;
    public static float damageCoefficient;
    public static GameObject projectilePrefab;
    public static GameObject effectPrefab;
    private bool beginExitTransition;
    private ChildLocator childLocator;

    public override void OnEnter()
    {
      base.OnEnter();
      this.missileStopwatch -= JellyStorm.missileSpawnDelay;
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

    private void FireBlob(Ray aimRay, float bonusPitch, float bonusYaw, float speed)
    {
      int num = 120 / 6;
      Vector3 xAxis = Vector3.ProjectOnPlane(this.characterDirection.forward, Vector3.up);
      // Vector3 forward = Util.ApplySpread(aimRay.direction, 0.0f, 0.0f, 1f, 1f, bonusYaw, bonusPitch);
      for (int i = 0; i < 6; i++)
      {
        Vector3 forward = Quaternion.AngleAxis(num * i, Vector3.up) * xAxis;
        ProjectileManager.instance.FireProjectile(JellyStorm.projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(forward), this.gameObject, this.damageStat * JellyStorm.damageCoefficient, 0.0f, Util.CheckRoll(this.critStat, this.characterBody.master));
      }
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      this.stopwatch += Time.fixedDeltaTime;
      this.missileStopwatch += Time.fixedDeltaTime;
      if ((double)this.missileStopwatch >= 1.0 / (double)JellyStorm.missileSpawnFrequency && !this.beginExitTransition)
      {
        this.missileStopwatch -= 1f / JellyStorm.missileSpawnFrequency;
        Transform child = this.childLocator.FindChild(JellyStorm.stormPointChildString);
        if ((bool)(Object)child)
        {
          for (int index = 0; index < JellyStorm.missileTurretCount; ++index)
          {
            float bonusYaw = (float)(360.0 / (double)JellyStorm.missileTurretCount * (double)index + 360.0 * (double)JellyStorm.missileTurretYawFrequency * (double)this.stopwatch);
            this.FireBlob(new Ray()
            {
              origin = child.position,
              direction = child.transform.forward
            }, Mathf.Sin(6.283185f * JellyStorm.missileTurretPitchFrequency * this.stopwatch) * JellyStorm.missileTurretPitchMagnitude, bonusYaw, JellyStorm.missileSpeed);
          }
        }
      }
      if ((double)this.stopwatch < (double)JellyStorm.stormDuration || !this.isAuthority)
        return;
      this.outer.SetNextStateToMain();
    }
  }
}