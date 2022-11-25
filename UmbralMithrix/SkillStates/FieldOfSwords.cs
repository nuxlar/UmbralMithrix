using RoR2;
using RoR2.Projectile;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using EntityStates;
using EntityStates.TitanMonster;
using EntityStates.BrotherMonster;

namespace UmbralMithrix
{
  public class FieldOfSwords : BaseState
  {
    public static float entryDuration = 1.5f;
    public static float fireDuration = 2f;
    public static float exitDuration = 1f;
    [SerializeField]
    public GameObject chargeEffectPrefab;
    [SerializeField]
    public GameObject fistEffectPrefab;
    [SerializeField]
    public GameObject fireEffectPrefab;
    [SerializeField]
    public GameObject fistProjectilePrefab = UmbralMithrix.noblePhantasm;
    public static float maxDistance = 40f;
    public static float trackingDuration = 0.5f;
    public static float fistDamageCoefficient = 4f;
    public static float fistForce = 2000f;
    public static string attackSoundString;
    public static string muzzleString;
    private Animator modelAnimator;
    private Transform modelTransform;
    private bool hasShownPrediction;
    private bool predictionOk;
    protected Vector3 predictedTargetPosition;
    private AimAnimator aimAnimator;
    private GameObject chargeInstance;
    private float stopwatch;
    private float duration = 3;
    private FireFist.SubState subState;
    private FireFist.Predictor predictor;
    private GameObject predictorDebug;
    static System.Random rand = new();

    public override void OnEnter()
    {
      base.OnEnter();
      this.stopwatch = 0.0f;
      this.modelAnimator = this.GetModelAnimator();
      this.modelTransform = this.GetModelTransform();
      this.subState = FireFist.SubState.Prep;
      int num = (int)Util.PlayAttackSpeedSound(FistSlam.attackSoundString, this.gameObject, this.attackSpeedStat);
      this.PlayCrossfade("FullBody Override", nameof(FistSlam), "FistSlam.playbackRate", this.duration, 0.1f);
      if ((bool)(Object)this.characterDirection)
        this.characterDirection.moveVector = this.characterDirection.forward;
      if ((bool)(Object)this.modelTransform)
      {
        AimAnimator component = this.modelTransform.GetComponent<AimAnimator>();
        if ((bool)(Object)component)
          component.enabled = true;
      }
      Transform modelChild = this.FindModelChild("MuzzleRight");
      if (!(bool)(Object)modelChild || !(bool)(Object)FistSlam.chargeEffectPrefab)
        return;
      this.chargeInstance = Object.Instantiate<GameObject>(FistSlam.chargeEffectPrefab, modelChild.position, modelChild.rotation);
      this.chargeInstance.transform.parent = modelChild;
      ScaleParticleSystemDuration component1 = this.chargeInstance.GetComponent<ScaleParticleSystemDuration>();
      if (!(bool)(Object)component1)
        return;
      if (!NetworkServer.active)
        return;
      BullseyeSearch bullseyeSearch = new BullseyeSearch();
      bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
      if ((bool)(Object)this.teamComponent)
        bullseyeSearch.teamMaskFilter.RemoveTeam(this.teamComponent.teamIndex);
      bullseyeSearch.maxDistanceFilter = FireFist.maxDistance;
      bullseyeSearch.maxAngleFilter = 90f;
      Ray aimRay = this.GetAimRay();
      bullseyeSearch.searchOrigin = aimRay.origin;
      bullseyeSearch.searchDirection = aimRay.direction;
      bullseyeSearch.filterByLoS = false;
      bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
      bullseyeSearch.RefreshCandidates();
      HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault<HurtBox>();
      if (!(bool)(Object)hurtBox)
        return;
      this.predictor = new FireFist.Predictor(this.transform);
      this.predictor.SetTargetTransform(hurtBox.transform);
    }

    public override void OnExit()
    {
      if ((bool)(Object)this.chargeInstance)
        EntityState.Destroy((Object)this.chargeInstance);
      this.PlayAnimation("FullBody Override", "BufferEmpty");
      EntityState.Destroy((Object)this.predictorDebug);
      this.predictorDebug = (GameObject)null;
      base.OnExit();
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      this.stopwatch += Time.fixedDeltaTime;
      switch (this.subState)
      {
        case FireFist.SubState.Prep:
          if (this.predictor != null)
            this.predictor.Update();
          if ((double)this.stopwatch <= (double)FireFist.trackingDuration)
          {
            if (this.predictor != null)
            {
              this.predictionOk = this.predictor.GetPredictedTargetPosition(entryDuration - trackingDuration, out this.predictedTargetPosition);
              if (this.predictionOk && (bool)(Object)this.predictorDebug)
                this.predictorDebug.transform.position = this.predictedTargetPosition;
            }
          }
          else if (!this.hasShownPrediction)
          {
            this.hasShownPrediction = true;
            this.PlacePredictedAttack();
          }
          if ((double)this.stopwatch < (double)entryDuration)
            break;
          this.predictor = (FireFist.Predictor)null;
          this.subState = FireFist.SubState.FireFist;
          this.stopwatch = 0.0f;
          if ((bool)(Object)this.chargeInstance)
            EntityState.Destroy((Object)this.chargeInstance);
          EffectManager.SimpleMuzzleFlash(FistSlam.slamImpactEffect, this.gameObject, FistSlam.muzzleString, false);
          break;
        case FireFist.SubState.FireFist:
          if ((double)this.stopwatch < (double)FireFist.fireDuration)
            break;
          this.subState = FireFist.SubState.Exit;
          this.stopwatch = 0.0f;
          break;
        case FireFist.SubState.Exit:
          if ((double)this.stopwatch < (double)FireFist.exitDuration || !this.isAuthority)
            break;
          this.outer.SetNextStateToMain();
          break;
      }
    }

    protected void PlacePredictedAttack()
    {
      float num3 = UnityEngine.Random.Range(0.0f, 360f);
      for (int index3 = 0; index3 < 8; ++index3)
      {
        int num4 = 0;
        for (int index4 = 0; index4 < 6; ++index4)
        {
          Vector3 vector3 = Quaternion.Euler(0.0f, num3 + 45f * (float)index3, 0.0f) * Vector3.forward;
          Vector3 position = this.predictedTargetPosition + vector3 * FireGoldFist.distanceBetweenFists * (float)index4;
          float maxDistance = 60f;
          RaycastHit hitInfo;
          if (Physics.Raycast(new Ray(position + Vector3.up * (maxDistance / 2f), Vector3.down), out hitInfo, maxDistance, (int)LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            position = hitInfo.point;
          this.PlaceSingleDelayBlast(position, FireGoldFist.delayBetweenFists * (float)num4);
          ++num4;
        }
      }
    }

    protected void PlaceSingleDelayBlast(Vector3 position, float delay)
    {
      if (!this.isAuthority)
        return;
      ProjectileManager.instance.FireProjectile(new FireProjectileInfo()
      {
        projectilePrefab = new FireGoldFist().fistProjectilePrefab,
        position = position,
        rotation = Quaternion.identity,
        owner = this.gameObject,
        damage = this.damageStat * fistDamageCoefficient,
        force = fistForce,
        crit = this.characterBody.RollCrit(),
        fuseOverride = entryDuration - trackingDuration + delay
      });
    }

    private enum SubState
    {
      Prep,
      FireFist,
      Exit,
    }

    private class Predictor
    {
      private Transform bodyTransform;
      private Transform targetTransform;
      private Vector3 targetPosition0;
      private Vector3 targetPosition1;
      private Vector3 targetPosition2;
      private int collectedPositions;

      public Predictor(Transform bodyTransform) => this.bodyTransform = bodyTransform;

      public bool hasTargetTransform => (bool)(Object)this.targetTransform;

      public bool isPredictionReady => this.collectedPositions > 2;

      private void PushTargetPosition(Vector3 newTargetPosition)
      {
        this.targetPosition2 = this.targetPosition1;
        this.targetPosition1 = this.targetPosition0;
        this.targetPosition0 = newTargetPosition;
        ++this.collectedPositions;
      }

      public void SetTargetTransform(Transform newTargetTransform)
      {
        this.targetTransform = newTargetTransform;
        this.targetPosition2 = this.targetPosition1 = this.targetPosition0 = newTargetTransform.position;
        this.collectedPositions = 1;
      }

      public void Update()
      {
        if (!(bool)(Object)this.targetTransform)
          return;
        this.PushTargetPosition(this.targetTransform.position);
      }

      public bool GetPredictedTargetPosition(float time, out Vector3 predictedPosition)
      {
        Vector3 vector3_1 = this.targetPosition1 - this.targetPosition2;
        Vector3 vector3_2 = this.targetPosition0 - this.targetPosition1;
        vector3_1.y = 0.0f;
        vector3_2.y = 0.0f;
        FireFist.Predictor.ExtrapolationType extrapolationType = vector3_1 == Vector3.zero || vector3_2 == Vector3.zero ? FireFist.Predictor.ExtrapolationType.None : ((double)Vector3.Dot(vector3_1.normalized, vector3_2.normalized) <= 0.980000019073486 ? FireFist.Predictor.ExtrapolationType.Polar : FireFist.Predictor.ExtrapolationType.Linear);
        float num1 = 1f / Time.fixedDeltaTime;
        predictedPosition = this.targetPosition0;
        switch (extrapolationType)
        {
          case FireFist.Predictor.ExtrapolationType.Linear:
            predictedPosition = this.targetPosition0 + vector3_2 * (time * num1);
            break;
          case FireFist.Predictor.ExtrapolationType.Polar:
            Vector3 position = this.bodyTransform.position;
            Vector3 vector2Xy1 = (Vector3)Util.Vector3XZToVector2XY(this.targetPosition2 - position);
            Vector3 vector2Xy2 = (Vector3)Util.Vector3XZToVector2XY(this.targetPosition1 - position);
            Vector3 vector2Xy3 = (Vector3)Util.Vector3XZToVector2XY(this.targetPosition0 - position);
            float magnitude1 = vector2Xy1.magnitude;
            float magnitude2 = vector2Xy2.magnitude;
            float magnitude3 = vector2Xy3.magnitude;
            float num2 = Vector2.SignedAngle((Vector2)vector2Xy1, (Vector2)vector2Xy2) * num1;
            float num3 = Vector2.SignedAngle((Vector2)vector2Xy2, (Vector2)vector2Xy3) * num1;
            double num4 = ((double)magnitude2 - (double)magnitude1) * (double)num1;
            float num5 = (magnitude3 - magnitude2) * num1;
            float num6 = (float)(((double)num2 + (double)num3) * 0.5);
            double num7 = (double)num5;
            float num8 = (float)((num4 + num7) * 0.5);
            float num9 = magnitude3 + num8 * time;
            if ((double)num9 < 0.0)
              num9 = 0.0f;
            Vector2 vector2 = Util.RotateVector2((Vector2)vector2Xy3, num6 * time) * (num9 * magnitude3);
            predictedPosition = position;
            predictedPosition.x += vector2.x;
            predictedPosition.z += vector2.y;
            break;
        }
        RaycastHit hitInfo;
        if (!Physics.Raycast(new Ray(predictedPosition + Vector3.up * 1f, Vector3.down), out hitInfo, 200f, (int)LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
          return false;
        predictedPosition = hitInfo.point;
        return true;
      }

      private enum ExtrapolationType
      {
        None,
        Linear,
        Polar,
      }
    }
  }
}
