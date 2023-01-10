using EntityStates.Huntress;
using KinematicCharacterController;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using EntityStates;
using System.Linq;

namespace UmbralMithrix
{
  public class AimCrushingLeap : BaseSkillState
  {
    public static GameObject areaIndicatorPrefab = ArrowRain.areaIndicatorPrefab;
    public float maxDuration = ModConfig.CrushingLeap.Value;
    public float stopwatch;
    private CharacterModel characterModel;
    private HurtBoxGroup hurtboxGroup;
    private GameObject areaIndicatorInstance;
    static Material tpMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Teleporters/matTeleporterRangeIndicator.mat").WaitForCompletion();
    static Material awShellExpolsionMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/artifactworld/matArtifactShellExplosionIndicator.mat").WaitForCompletion();

    public override void OnEnter()
    {
      base.OnEnter();
      Transform modelTransform = this.GetModelTransform();
      if ((bool)(Object)modelTransform)
      {
        this.characterModel = modelTransform.GetComponent<CharacterModel>();
        this.hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
      }
      if ((bool)(Object)this.characterModel)
        ++this.characterModel.invisibilityCount;
      if ((bool)(Object)this.hurtboxGroup)
        ++this.hurtboxGroup.hurtBoxesDeactivatorCounter;
      this.characterBody.AddBuff((BuffIndex)3);
      int num = (int)Util.PlaySound("Play_voidRaid_snipe_shoot_final", this.gameObject);
      this.gameObject.layer = LayerIndex.fakeActor.intVal;
      ((BaseCharacterController)this.characterMotor).Motor.RebuildCollidableLayers();
      this.characterMotor.velocity = Vector3.zero;
      ((BaseCharacterController)this.characterMotor).Motor.SetPosition(new Vector3(((Component)this.characterMotor).transform.position.x, ((Component)this.characterMotor).transform.position.y + 25f, ((Component)this.characterMotor).transform.position.z), true);
      if (!(bool)(Object)AimCrushingLeap.areaIndicatorPrefab)
        return;
      this.areaIndicatorInstance = Object.Instantiate<GameObject>(AimCrushingLeap.areaIndicatorPrefab);
      this.areaIndicatorInstance.transform.localScale = new Vector3(ArrowRain.arrowRainRadius, ArrowRain.arrowRainRadius, ArrowRain.arrowRainRadius);
      this.areaIndicatorInstance.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material = tpMat;
    }

    private void UpdateAreaIndicator()
    {
      if (!(bool)(Object)this.areaIndicatorInstance)
        return;
      if (this.characterBody.master.aiComponents.Length == 0 || this.characterBody.master.aiComponents[0].currentEnemy == null)
        return;
      Vector3 lastEnemyPosition = this.characterBody.master.aiComponents[0].currentEnemy.lastKnownBullseyePosition.Value;
      this.areaIndicatorInstance.transform.position = lastEnemyPosition;
      BullseyeSearch bullseyeSearch = new BullseyeSearch();
      bullseyeSearch.viewer = this.characterBody;
      bullseyeSearch.searchOrigin = this.characterBody.corePosition;
      bullseyeSearch.searchDirection = this.characterBody.corePosition;
      bullseyeSearch.maxDistanceFilter = 2000;
      bullseyeSearch.teamMaskFilter = TeamMask.GetEnemyTeams(this.GetTeam());
      bullseyeSearch.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
      bullseyeSearch.RefreshCandidates();
      var target = bullseyeSearch.GetResults().FirstOrDefault<HurtBox>();
      if (!(bool)(Object)target)
        return;
      RaycastHit hitInfo;
      if (!Physics.Raycast(new Ray(target.transform.position, Vector3.down), out hitInfo, 500f, (int)LayerIndex.world.mask, QueryTriggerInteraction.UseGlobal))
        return;
      this.areaIndicatorInstance.transform.position = hitInfo.point;
    }


    public void HandleFollowupAttack()
    {
      this.characterMotor.Motor.SetPosition(new Vector3(this.areaIndicatorInstance.transform.position.x, this.areaIndicatorInstance.transform.position.y + 1f, this.areaIndicatorInstance.transform.position.z), true);
      this.outer.SetNextState(new ExitCrushingLeap());
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      this.stopwatch += Time.fixedDeltaTime;
      if (this.stopwatch < (this.maxDuration - 1f))
        this.UpdateAreaIndicator();
      if (this.stopwatch >= (this.maxDuration - 1f))
        this.areaIndicatorInstance.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material = awShellExpolsionMat;
      if (!this.isAuthority || !(bool)(Object)this.inputBank || (double)this.fixedAge < (double)this.maxDuration)
        return;
      this.HandleFollowupAttack();
    }

    public override void OnExit()
    {
      if ((bool)(Object)this.characterModel)
        --this.characterModel.invisibilityCount;
      if ((bool)(Object)this.hurtboxGroup)
        --this.hurtboxGroup.hurtBoxesDeactivatorCounter;
      this.characterBody.RemoveBuff((BuffIndex)3);
      this.gameObject.layer = LayerIndex.defaultLayer.intVal;
      this.characterMotor.Motor.RebuildCollidableLayers();
      if ((bool)(Object)this.areaIndicatorInstance)
        EntityState.Destroy((Object)this.areaIndicatorInstance.gameObject);
      base.OnExit();
    }
  }
}
