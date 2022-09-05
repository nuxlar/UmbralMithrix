using BepInEx;
using RoR2;
using RoR2.Skills;
using RoR2.Projectile;
using EntityStates.BrotherMonster;
using EntityStates.BrotherMonster.Weapon;
using R2API;
using R2API.Utils;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace UmbralMithrix
{
  [BepInPlugin("com.Nuxlar.UmbralMithrix", "UmbralMithrix", "1.0.0")]
  [BepInDependency("com.bepis.r2api")]
  [BepInDependency("com.rune580.riskofoptions")]
  [R2APISubmoduleDependency(new string[]
    {
        "LanguageAPI",
        "PrefabAPI",
        "ContentAddition"
    })]

  public class MithrixTheAccursed : BaseUnityPlugin
  {
    bool hasfired;
    int phaseCounter = 0;
    float elapsed = 0;
    bool doppelEventHasTriggered = false;
    GameObject Mithrix = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherBody.prefab").WaitForCompletion();
    GameObject MithrixHurt = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherHurtBody.prefab").WaitForCompletion();
    GameObject BrotherHaunt = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BrotherHaunt/BrotherHauntBody.prefab").WaitForCompletion();
    SpawnCard MithrixCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/Brother/cscBrother.asset").WaitForCompletion();
    SpawnCard MithrixHurtCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/Brother/cscBrotherHurt.asset").WaitForCompletion();
    GameObject MithrixGlass = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/BrotherGlass/BrotherGlassBody.prefab").WaitForCompletion();
    SpawnCard MithrixGlassCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Junk/BrotherGlass/cscBrotherGlass.asset").WaitForCompletion();
    static GameObject exploderProjectile = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarExploder/LunarExploderShardProjectile.prefab").WaitForCompletion();
    static GameObject golemProjectile = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarGolem/LunarGolemTwinShotProjectile.prefab").WaitForCompletion();

    public void Awake()
    {
      ModConfig.InitConfig(Config);
      AddContent();
      On.RoR2.Run.Start += OnRunStart;
      On.RoR2.Artifacts.DoppelgangerInvasionManager.CreateDoppelganger += CreateDoppelganger;
      On.RoR2.CharacterMaster.OnBodyStart += CharacterMasterOnBodyStart;
      On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.OnEnter += BrotherEncounterPhaseBaseStateOnEnter;
      On.EntityStates.Missions.BrotherEncounter.Phase1.OnEnter += Phase1OnEnter;
      On.EntityStates.Missions.BrotherEncounter.Phase2.OnEnter += Phase2OnEnter;
      On.EntityStates.Missions.BrotherEncounter.Phase3.OnEnter += Phase3OnEnter;
      On.EntityStates.Missions.BrotherEncounter.Phase4.OnEnter += Phase4OnEnter;
      On.EntityStates.BrotherMonster.ExitSkyLeap.OnEnter += ExitSkyLeapOnEnter;
      On.EntityStates.FrozenState.OnEnter += FrozenStateOnEnter;
      On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += AddTimedBuff_BuffDef_float;
      On.EntityStates.BrotherMonster.SprintBash.OnEnter += SprintBashOnEnter;
      On.EntityStates.BrotherMonster.WeaponSlam.OnEnter += WeaponSlamOnEnter;
      On.EntityStates.BrotherMonster.WeaponSlam.OnEnter += CleanupPillar;
      On.EntityStates.BrotherMonster.WeaponSlam.FixedUpdate += WeaponSlamFixedUpdate;
      On.EntityStates.BrotherMonster.Weapon.FireLunarShards.OnEnter += FireLunarShardsOnEnter;
      On.EntityStates.BrotherMonster.FistSlam.OnEnter += FistSlamOnEnter;
      On.EntityStates.BrotherMonster.FistSlam.FixedUpdate += FistSlamFixedUpdate;
      On.EntityStates.BrotherMonster.SpellChannelEnterState.OnEnter += SpellChannelEnterStateOnEnter;
      On.EntityStates.BrotherMonster.SpellChannelState.OnEnter += SpellChannelStateOnEnter;
      On.EntityStates.BrotherMonster.SpellChannelState.OnExit += SpellChannelStateOnExit;
      On.EntityStates.BrotherMonster.SpellChannelExitState.OnEnter += SpellChannelExitStateOnEnter;
      On.EntityStates.BrotherMonster.StaggerEnter.OnEnter += StaggerEnterOnEnter;
      On.EntityStates.BrotherMonster.StaggerExit.OnEnter += StaggerExitOnEnter;
      On.EntityStates.BrotherMonster.StaggerLoop.OnEnter += StaggerLoopOnEnter;
      On.EntityStates.BrotherMonster.TrueDeathState.OnEnter += TrueDeathStateOnEnter;
      On.EntityStates.BrotherHaunt.FireRandomProjectiles.OnEnter += FireRandomProjectiles;
    }

    private void AdjustBaseStats()
    {
      Logger.LogMessage("Adjusting Phase 1 Stats");
      int playerCount = PlayerCharacterMasterController.instances.Count;
      float hpMultiplier;
      float mobilityMultiplier;
      if (Run.instance.loopClearCount == 1)
      {
        hpMultiplier = (ModConfig.phase1BaseHPScaling.Value * Run.instance.loopClearCount) + (ModConfig.phase1PlayerHPScaling.Value * playerCount);
        mobilityMultiplier = (ModConfig.phase1BaseMobilityScaling.Value * Run.instance.loopClearCount) + (ModConfig.phase1PlayerMobilityScaling.Value * playerCount);
      }
      else
      {
        hpMultiplier = (ModConfig.phase1LoopHPScaling.Value * Run.instance.loopClearCount) + (ModConfig.phase1PlayerHPScaling.Value * playerCount);
        mobilityMultiplier = (ModConfig.phase1LoopMobilityScaling.Value * Run.instance.loopClearCount) + (ModConfig.phase1PlayerMobilityScaling.Value * playerCount);
      }
      CharacterBody MithrixBody = Mithrix.GetComponent<CharacterBody>();
      CharacterDirection MithrixDirection = Mithrix.GetComponent<CharacterDirection>();
      CharacterMotor MithrixMotor = Mithrix.GetComponent<CharacterMotor>();

      MithrixMotor.mass = ModConfig.mass.Value;
      MithrixMotor.airControl = ModConfig.aircontrol.Value;
      MithrixMotor.jumpCount = ModConfig.jumpcount.Value;

      MithrixBody.baseMaxHealth = (ModConfig.basehealth.Value + (ModConfig.basehealth.Value * hpMultiplier)) / 10;
      MithrixBody.levelMaxHealth = (ModConfig.levelhealth.Value + (ModConfig.levelhealth.Value * hpMultiplier)) / 10;
      MithrixBody.baseDamage = (ModConfig.basedamage.Value) * 100 / 4;
      MithrixBody.levelDamage = (ModConfig.leveldamage.Value) * 100 / 4;

      MithrixBody.baseAttackSpeed = ModConfig.baseattackspeed.Value;
      MithrixBody.baseMoveSpeed = ModConfig.basespeed.Value + (ModConfig.basespeed.Value * mobilityMultiplier);
      MithrixBody.baseAcceleration = ModConfig.acceleration.Value + (ModConfig.acceleration.Value * mobilityMultiplier);
      MithrixBody.baseJumpPower = ModConfig.jumpingpower.Value + (ModConfig.jumpingpower.Value * mobilityMultiplier);
      MithrixDirection.turnSpeed = ModConfig.turningspeed.Value + (ModConfig.turningspeed.Value * mobilityMultiplier);

      MithrixBody.baseArmor = ModConfig.basearmor.Value;

      ProjectileSteerTowardTarget component = FireLunarShards.projectilePrefab.GetComponent<ProjectileSteerTowardTarget>();
      component.rotationSpeed = ModConfig.ShardHoming.Value;
      ProjectileDirectionalTargetFinder component2 = FireLunarShards.projectilePrefab.GetComponent<ProjectileDirectionalTargetFinder>();
      component2.lookRange = ModConfig.ShardRange.Value;
      component2.lookCone = ModConfig.ShardCone.Value;
      component2.allowTargetLoss = true;

      WeaponSlam.duration = (3.5f / ModConfig.baseattackspeed.Value);
      HoldSkyLeap.duration = ModConfig.JumpPause.Value;
      ExitSkyLeap.waveProjectileCount = ModConfig.JumpWaveCount.Value;
      ExitSkyLeap.recastChance = ModConfig.JumpRecast.Value;
      UltChannelState.waveProjectileCount = ModConfig.UltimateWaves.Value;
      UltChannelState.maxDuration = ModConfig.UltimateDuration.Value;
      UltChannelState.totalWaves = ModConfig.UltimateCount.Value;
    }

    private void AdjustBaseSkills()
    {
      SkillLocator SklLocate = Mithrix.GetComponent<SkillLocator>();
      SkillFamily Hammer = SklLocate.primary.skillFamily;
      SkillDef HammerChange = Hammer.variants[0].skillDef;
      HammerChange.baseRechargeInterval = ModConfig.PrimCD.Value;
      HammerChange.baseMaxStock = ModConfig.PrimStocks.Value;

      SkillFamily Bash = SklLocate.secondary.skillFamily;
      SkillDef BashChange = Bash.variants[0].skillDef;
      BashChange.baseRechargeInterval = ModConfig.SecCD.Value;
      BashChange.baseMaxStock = ModConfig.SecStocks.Value;

      SkillFamily Dash = SklLocate.utility.skillFamily;
      SkillDef DashChange = Dash.variants[0].skillDef;
      DashChange.baseRechargeInterval = ModConfig.UtilCD.Value;
      DashChange.baseMaxStock = ModConfig.UtilStocks.Value;
      DashChange.activationState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.ImpMonster.BlinkState));

      SkillFamily Ult = SklLocate.special.skillFamily;
      SkillDef UltChange = Ult.variants[0].skillDef;
      UltChange.baseRechargeInterval = ModConfig.SpecialCD.Value;
      UltChange.baseMaxStock = 5;
    }

    private void AdjustPhase2Stats()
    {
      Logger.LogMessage("Adjusting Phase 2 Stats");
      int playerCount = PlayerCharacterMasterController.instances.Count;
      float hpMultiplier;
      float mobilityMultiplier;
      if (Run.instance.loopClearCount == 1)
      {
        hpMultiplier = (ModConfig.phase2BaseHPScaling.Value * Run.instance.loopClearCount) + (ModConfig.phase2PlayerHPScaling.Value * playerCount);
        mobilityMultiplier = (ModConfig.phase2BaseMobilityScaling.Value * Run.instance.loopClearCount) + (ModConfig.phase2PlayerMobilityScaling.Value * playerCount);
      }
      else
      {
        hpMultiplier = (ModConfig.phase2LoopHPScaling.Value * Run.instance.loopClearCount) + (ModConfig.phase2PlayerHPScaling.Value * playerCount);
        mobilityMultiplier = (ModConfig.phase2LoopMobilityScaling.Value * Run.instance.loopClearCount) + (ModConfig.phase2PlayerMobilityScaling.Value * playerCount);
      }
      CharacterBody MithrixBody = Mithrix.GetComponent<CharacterBody>();
      CharacterBody MithrixGlassBody = MithrixGlass.GetComponent<CharacterBody>();
      CharacterDirection MithrixDirection = Mithrix.GetComponent<CharacterDirection>();

      MithrixBody.baseMaxHealth = playerCount > 2 ? ((ModConfig.basehealth.Value + (ModConfig.basehealth.Value * hpMultiplier)) / 10) / 1.5f : (ModConfig.basehealth.Value + (ModConfig.basehealth.Value * hpMultiplier)) / 10;
      MithrixBody.levelMaxHealth = playerCount > 2 ? ((ModConfig.levelhealth.Value + (ModConfig.levelhealth.Value * hpMultiplier)) / 10) / 1.5f : (ModConfig.levelhealth.Value + (ModConfig.levelhealth.Value * hpMultiplier)) / 10;

      MithrixGlassBody.baseMaxHealth = (ModConfig.basehealth.Value + (ModConfig.basehealth.Value * hpMultiplier)) / 10;
      MithrixGlassBody.levelMaxHealth = (ModConfig.levelhealth.Value + (ModConfig.levelhealth.Value * hpMultiplier)) / 10;
      MithrixGlassBody.baseDamage = (ModConfig.basedamage.Value) * 100 / 4;
      MithrixGlassBody.levelDamage = (ModConfig.leveldamage.Value) * 100 / 4;

      MithrixBody.baseMoveSpeed = ModConfig.basespeed.Value + (ModConfig.basespeed.Value * mobilityMultiplier);
      MithrixBody.baseAcceleration = ModConfig.acceleration.Value + (ModConfig.acceleration.Value * mobilityMultiplier);
      MithrixBody.baseJumpPower = ModConfig.jumpingpower.Value + (ModConfig.jumpingpower.Value * mobilityMultiplier);
      MithrixDirection.turnSpeed = ModConfig.turningspeed.Value + (ModConfig.turningspeed.Value * mobilityMultiplier);

      WeaponSlam.duration = (3.5f / ModConfig.baseattackspeed.Value);
    }

    private void AdjustPhase3Stats()
    {
      Logger.LogMessage("Adjusting Phase 3 Stats");
      int playerCount = PlayerCharacterMasterController.instances.Count;
      float hpMultiplier;
      float mobilityMultiplier;
      if (Run.instance.loopClearCount == 1)
      {
        hpMultiplier = (ModConfig.phase3BaseHPScaling.Value * Run.instance.loopClearCount) + (ModConfig.phase3PlayerHPScaling.Value * playerCount);
        mobilityMultiplier = (ModConfig.phase3BaseMobilityScaling.Value * Run.instance.loopClearCount) + (ModConfig.phase3PlayerMobilityScaling.Value * playerCount);
      }
      else
      {
        hpMultiplier = (ModConfig.phase3LoopHPScaling.Value * Run.instance.loopClearCount) + (ModConfig.phase3PlayerHPScaling.Value * playerCount);
        mobilityMultiplier = (ModConfig.phase3LoopMobilityScaling.Value * Run.instance.loopClearCount) + (ModConfig.phase3PlayerMobilityScaling.Value * playerCount);
      }
      CharacterBody MithrixBody = Mithrix.GetComponent<CharacterBody>();
      CharacterDirection MithrixDirection = Mithrix.GetComponent<CharacterDirection>();

      MithrixBody.baseMaxHealth = playerCount > 2 ? ((ModConfig.basehealth.Value + (ModConfig.basehealth.Value * hpMultiplier)) / 10) / 1.5f : (ModConfig.basehealth.Value + (ModConfig.basehealth.Value * hpMultiplier)) / 10;
      MithrixBody.levelMaxHealth = playerCount > 2 ? ((ModConfig.levelhealth.Value + (ModConfig.levelhealth.Value * hpMultiplier)) / 10) / 1.5f : (ModConfig.levelhealth.Value + (ModConfig.levelhealth.Value * hpMultiplier)) / 10;

      MithrixBody.baseMoveSpeed = ModConfig.basespeed.Value + (ModConfig.basespeed.Value * mobilityMultiplier);
      MithrixBody.baseAcceleration = ModConfig.acceleration.Value + (ModConfig.acceleration.Value * mobilityMultiplier);
      MithrixBody.baseJumpPower = ModConfig.jumpingpower.Value + (ModConfig.jumpingpower.Value * mobilityMultiplier);
      MithrixDirection.turnSpeed = ModConfig.turningspeed.Value + (ModConfig.turningspeed.Value * mobilityMultiplier);

      WeaponSlam.duration = (3.5f / ModConfig.baseattackspeed.Value);
    }

    private void AdjustPhase4Stats()
    {
      Logger.LogMessage("Adjusting Phase 4 Stats");
      int playerCount = PlayerCharacterMasterController.instances.Count;
      float hpMultiplier;
      if (Run.instance.loopClearCount == 1)
        hpMultiplier = (ModConfig.phase4BaseHPScaling.Value * Run.instance.loopClearCount) + (ModConfig.phase4PlayerHPScaling.Value * playerCount);
      else
        hpMultiplier = (ModConfig.phase4LoopHPScaling.Value * Run.instance.loopClearCount) + (ModConfig.phase4PlayerHPScaling.Value * playerCount);
      CharacterBody MithrixHurtBody = MithrixHurt.GetComponent<CharacterBody>();
      MithrixHurtBody.baseMaxHealth = Run.instance.loopClearCount > 1 ? ModConfig.basehealth.Value + (ModConfig.basehealth.Value * hpMultiplier) : (ModConfig.basehealth.Value + (ModConfig.basehealth.Value * hpMultiplier)) / 5;
      MithrixHurtBody.levelMaxHealth = Run.instance.loopClearCount > 1 ? ModConfig.levelhealth.Value + (ModConfig.levelhealth.Value * hpMultiplier) : (ModConfig.levelhealth.Value + (ModConfig.levelhealth.Value * hpMultiplier)) / 5;
      MithrixHurtBody.baseDamage = (ModConfig.basedamage.Value) * 100 / 4;
      MithrixHurtBody.levelDamage = (ModConfig.leveldamage.Value) * 100 / 4;

      MithrixHurtBody.baseArmor = ModConfig.basearmor.Value;
      SkillLocator skillLocator = MithrixHurt.GetComponent<SkillLocator>();
      SkillFamily fireLunarShardsHurt = skillLocator.primary.skillFamily;
      SkillDef fireLunarShardsHurtSkillDef = fireLunarShardsHurt.variants[0].skillDef;
      fireLunarShardsHurtSkillDef.baseRechargeInterval = ModConfig.SuperShardCD.Value;
      fireLunarShardsHurtSkillDef.baseMaxStock = ModConfig.SuperShardCount.Value;
    }
    private void OnRunStart(On.RoR2.Run.orig_Start orig, Run self)
    {
      Logger.LogMessage("Accursing the King of Nothing");
      AdjustBaseSkills();
      AdjustBaseStats();
      orig(self);
    }

    private void AddContent()
    {
      // Add our new EntityStates to the game
      ContentAddition.AddEntityState<LunarDevastationEnter>(out _);
      ContentAddition.AddEntityState<LunarDevastationChannel>(out _);
      ContentAddition.AddEntityState<LunarDevastationExit>(out _);

      /** For Debugging
      SurvivorDef mitchell = ScriptableObject.CreateInstance<SurvivorDef>();
      mitchell.bodyPrefab = MithrixHurt;
      mitchell.descriptionToken = "MITHRIX_DESCRIPTION";
      mitchell.displayPrefab = MithrixHurt;
      mitchell.primaryColor = new Color(0.5f, 0.5f, 0.5f);
      mitchell.displayNameToken = "Mitchell";
      mitchell.desiredSortPosition = 99f;
      ContentAddition.AddSurvivorDef(mitchell);
      **/
    }
    // Prevent freezing from affecting Mithrix after 10 stages or if the config is enabled
    private void FrozenStateOnEnter(On.EntityStates.FrozenState.orig_OnEnter orig, EntityStates.FrozenState self)
    {
      if ((self.characterBody.name == "BrotherBody(Clone)" || self.characterBody.name == "BrotherHurtBody(Clone)") && (Run.instance.loopClearCount >= 2 || ModConfig.debuffResistance.Value))
        return;
      orig(self);
    }
    // Prevent tentabauble from affecting Mithrix after 10 stages or if the config is enabled
    private void AddTimedBuff_BuffDef_float(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
    {
      if ((self.name == "BrotherBody(Clone)" || self.name == "BrotherHurtBody(Clone)") && buffDef == RoR2Content.Buffs.Nullified && (Run.instance.loopClearCount >= 2 || ModConfig.debuffResistance.Value))
        return;
      orig(self, buffDef, duration);
    }
    // Change doppel spawn place to the center of the arena if it's Phase 2
    private void CreateDoppelganger(On.RoR2.Artifacts.DoppelgangerInvasionManager.orig_CreateDoppelganger orig, CharacterMaster srcCharacterMaster, Xoroshiro128Plus rng)
    {
      SpawnCard spawnCard = RoR2.Artifacts.DoppelgangerSpawnCard.FromMaster(srcCharacterMaster);
      if (!(bool)spawnCard)
        return;
      Transform transform;
      DirectorCore.MonsterSpawnDistance input;
      if ((bool)TeleporterInteraction.instance)
      {
        transform = TeleporterInteraction.instance.transform;
        input = DirectorCore.MonsterSpawnDistance.Close;
      }
      else if (phaseCounter == 3)
      {
        transform = Mithrix.transform;
        input = DirectorCore.MonsterSpawnDistance.Close;
      }
      else
      {
        transform = srcCharacterMaster.GetBody().coreTransform;
        input = DirectorCore.MonsterSpawnDistance.Far;
      }
      DirectorPlacementRule placementRule = new DirectorPlacementRule()
      {
        spawnOnTarget = transform,
        placementMode = DirectorPlacementRule.PlacementMode.NearestNode
      };
      DirectorCore.GetMonsterSpawnDistance(input, out placementRule.minDistance, out placementRule.maxDistance);
      DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, placementRule, rng);
      directorSpawnRequest.teamIndexOverride = new TeamIndex?(TeamIndex.Monster);
      directorSpawnRequest.ignoreTeamMemberLimit = true;
      CombatSquad combatSquad = null;
      directorSpawnRequest.onSpawnedServer += (result =>
      {
        if (!(bool)combatSquad)
          combatSquad = Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/Encounters/ShadowCloneEncounter")).GetComponent<CombatSquad>();
        combatSquad.AddMember(result.spawnedInstance.GetComponent<CharacterMaster>());
      });
      DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
      if ((bool)combatSquad)
        NetworkServer.Spawn(combatSquad.gameObject);
      Destroy(spawnCard);
    }
    private void CharacterMasterOnBodyStart(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body)
    {
      orig(self, body);
      // Make Mithrix an Umbra
      if (body.name == "BrotherBody(Clone)" || body.name == "BrotherHurtBody(Clone)" || body.name == "BrotherGlassBody(Clone)")
        self.inventory.GiveItemString(RoR2Content.Items.InvadingDoppelganger.name);
      if (self.name == "BrotherHurtMaster(Clone)")
      {
        body.AddBuff(RoR2Content.Buffs.Immune);
        RoR2.Artifacts.DoppelgangerInvasionManager.PerformInvasion(RoR2Application.rng);
        doppelEventHasTriggered = true;
        Task.Delay(20000 / Run.instance.loopClearCount).ContinueWith(o => { body.RemoveBuff(RoR2Content.Buffs.Immune); });
      }
    }
    // Phase 2 change to encounter spawns (Mithrix instead of Chimera)
    private void BrotherEncounterPhaseBaseStateOnEnter(On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState self)
    {
      phaseCounter++;
      // BrotherEncounterBaseState OnEnter
      self.childLocator = self.GetComponent<ChildLocator>();
      Transform child1 = self.childLocator.FindChild("ArenaWalls");
      Transform child2 = self.childLocator.FindChild("ArenaNodes");
      if ((bool)child1)
        child1.gameObject.SetActive(self.shouldEnableArenaWalls);
      if (!(bool)child2)
        return;
      child2.gameObject.SetActive(self.shouldEnableArenaNodes);
      // BrotherEncounterBaseState OnEnter
      if ((bool)PhaseCounter.instance)
      {
        phaseCounter = PhaseCounter.instance.phase;
        PhaseCounter.instance.GoToNextPhase();
      }
      if ((bool)self.childLocator)
      {
        self.phaseControllerObject = self.childLocator.FindChild(self.phaseControllerChildString).gameObject;
        if ((bool)self.phaseControllerObject)
        {
          self.phaseScriptedCombatEncounter = self.phaseControllerObject.GetComponent<ScriptedCombatEncounter>();
          self.phaseBossGroup = self.phaseControllerObject.GetComponent<BossGroup>();
          self.phaseControllerSubObjectContainer = self.phaseControllerObject.transform.Find("PhaseObjects").gameObject;
          self.phaseControllerSubObjectContainer.SetActive(true);
        }
        GameObject gameObject = self.childLocator.FindChild("AllPhases").gameObject;
        if ((bool)gameObject)
          gameObject.SetActive(true);
      }
      self.healthBarShowTime = Run.FixedTimeStamp.now + self.healthBarShowDelay;
      if ((bool)DirectorCore.instance)
      {
        foreach (Behaviour component in DirectorCore.instance.GetComponents<CombatDirector>())
          component.enabled = false;
      }
      if (!NetworkServer.active || self.phaseScriptedCombatEncounter == null)
        return;
      // Make Mithrix spawn for phase 2
      if (phaseCounter == 1)
      {
        Mithrix.transform.position = new Vector3(-88.5f, 491.5f, -0.3f);
        Mithrix.transform.rotation = Quaternion.identity;
        Transform explicitSpawnPosition = Mithrix.transform;
        ScriptedCombatEncounter.SpawnInfo spawnInfoMithrix = new ScriptedCombatEncounter.SpawnInfo
        {
          explicitSpawnPosition = explicitSpawnPosition,
          spawnCard = MithrixCard,
        };
        self.phaseScriptedCombatEncounter.spawns = new ScriptedCombatEncounter.SpawnInfo[] { spawnInfoMithrix };
      }
      if (phaseCounter == 2)
      {
        Mithrix.transform.position = new Vector3(-88.5f, 491.5f, -0.3f);
        Mithrix.transform.rotation = Quaternion.identity;
        Transform explicitSpawnPosition = Mithrix.transform;
        ScriptedCombatEncounter.SpawnInfo spawnInfoMithrix = new ScriptedCombatEncounter.SpawnInfo
        {
          explicitSpawnPosition = explicitSpawnPosition,
          spawnCard = MithrixCard,
        };
        self.phaseScriptedCombatEncounter.spawns = new ScriptedCombatEncounter.SpawnInfo[] { spawnInfoMithrix, spawnInfoMithrix };
      }
      self.phaseScriptedCombatEncounter.combatSquad.onMemberAddedServer += new Action<CharacterMaster>(self.OnMemberAddedServer);
    }

    private void Phase1OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase1.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.Phase1 self)
    {
      doppelEventHasTriggered = false;
      AdjustBaseStats();
      orig(self);
    }

    private void Phase2OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase2.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.Phase2 self)
    {
      self.KillAllMonsters();
      AdjustPhase2Stats();
      orig(self);
    }

    private void Phase3OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase3.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.Phase3 self)
    {
      self.KillAllMonsters();
      AdjustPhase3Stats();
      orig(self);
    }

    private void Phase4OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase4.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.Phase4 self)
    {
      AdjustPhase4Stats();
      orig(self);
    }
    // Make Brother Haunt fire more projectiles after the fight
    private void FireRandomProjectiles(On.EntityStates.BrotherHaunt.FireRandomProjectiles.orig_OnEnter orig, EntityStates.BrotherHaunt.FireRandomProjectiles self)
    {
      EntityStates.BrotherHaunt.FireRandomProjectiles.maximumCharges = 150;
      EntityStates.BrotherHaunt.FireRandomProjectiles.chargeRechargeDuration = 0.08f;
      EntityStates.BrotherHaunt.FireRandomProjectiles.chanceToFirePerSecond = 0.5f;
      EntityStates.BrotherHaunt.FireRandomProjectiles.damageCoefficient = 15f;
      orig(self);
    }

    private void ExitSkyLeapOnEnter(On.EntityStates.BrotherMonster.ExitSkyLeap.orig_OnEnter orig, ExitSkyLeap self)
    {
      // EntityStates BaseState OnEnter
      if (!(bool)self.characterBody)
        return;
      self.attackSpeedStat = self.characterBody.attackSpeed;
      self.damageStat = self.characterBody.damage;
      self.critStat = self.characterBody.crit;
      self.moveSpeedStat = self.characterBody.moveSpeed;
      // EntityStates BaseState OnEnter
      self.duration = ExitSkyLeap.baseDuration / self.attackSpeedStat;
      int num = (int)Util.PlaySound(ExitSkyLeap.soundString, self.gameObject);
      self.PlayAnimation("Body", nameof(ExitSkyLeap), "SkyLeap.playbackRate", self.duration);
      self.PlayAnimation("FullBody Override", "BufferEmpty");
      self.characterBody.AddTimedBuff(RoR2Content.Buffs.ArmorBoost, ExitSkyLeap.baseDuration);
      AimAnimator aimAnimator = self.GetAimAnimator();
      if ((bool)aimAnimator)
        aimAnimator.enabled = true;
      if (self.isAuthority)
      {
        self.FireRingAuthority();
        // custom Ring Authority
        float num1 = 360f / ExitSkyLeap.waveProjectileCount;
        Vector3 point = Vector3.ProjectOnPlane(self.inputBank.aimDirection, Vector3.up);
        Vector3 point2 = Vector3.ProjectOnPlane(self.inputBank.aimDirection, Vector3.forward);
        Vector3 corePosition = self.characterBody.corePosition;
        for (int index = 0; index < ExitSkyLeap.waveProjectileCount; ++index)
        {
          Vector3 forward3 = Quaternion.AngleAxis(num1 * index, Vector3.up) * point;
          ProjectileManager.instance.FireProjectile(FistSlam.waveProjectilePrefab, corePosition, Util.QuaternionSafeLookRotation(forward3), self.gameObject, self.characterBody.damage * FistSlam.waveProjectileDamageCoefficient, FistSlam.waveProjectileForce, Util.CheckRoll(self.characterBody.crit, self.characterBody.master));
        }
      }
      if (phaseCounter == 1 || phaseCounter == 2)
      {
        for (int index = 0; index < 2; ++index)
        {
          DirectorPlacementRule placementRule = new DirectorPlacementRule();
          placementRule.placementMode = DirectorPlacementRule.PlacementMode.Approximate;
          placementRule.minDistance = 3f;
          placementRule.maxDistance = 20f;
          placementRule.spawnOnTarget = self.gameObject.transform;
          Xoroshiro128Plus rng = RoR2Application.rng;
          DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(MithrixGlassCard, placementRule, rng);
          directorSpawnRequest.summonerBodyObject = self.gameObject;
          directorSpawnRequest.onSpawnedServer += (Action<SpawnCard.SpawnResult>)(spawnResult => spawnResult.spawnedInstance.GetComponent<Inventory>().GiveItem(RoR2Content.Items.HealthDecay, ExitSkyLeap.cloneDuration));
          DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
        }
      }
      if (!(bool)PhaseCounter.instance)
        return;
      if ((double)UnityEngine.Random.value < ExitSkyLeap.recastChance)
        self.recast = true;
      if (PhaseCounter.instance.phase == 1)
        return;
      GenericSkill genericSkill = (bool)self.skillLocator ? self.skillLocator.special : null;
      if (!(bool)genericSkill)
        return;
      if (PhaseCounter.instance.phase == 2)
        UltChannelState.replacementSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(LunarDevastationEnter));
      else
        UltChannelState.replacementSkillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(UltEnterState));
      genericSkill.SetSkillOverride(self.outer, UltChannelState.replacementSkillDef, GenericSkill.SkillOverridePriority.Contextual);
    }

    // Adds more projectiles to SprintBash in a cone shape
    private void SprintBashOnEnter(On.EntityStates.BrotherMonster.SprintBash.orig_OnEnter orig, SprintBash self)
    {
      if (self.isAuthority)
      {
        if (ModConfig.BashProjectileCount.Value > 0)
        {
          Util.PlaySound(EntityStates.LunarGolem.FireTwinShots.attackSoundString, self.gameObject);
          float num = 360f / ModConfig.BashProjectileCount.Value;
          Ray aimRay = self.GetAimRay();
          Vector3 point = Vector3.ProjectOnPlane(self.inputBank.aimDirection, Vector3.up);
          // Vector3 bodyPosition = self.characterBody.transform.position;
          for (int i = 0; i < ModConfig.BashProjectileCount.Value; i++)
          {
            Vector3 cone = Quaternion.AngleAxis(num * i, Vector3.forward) * point;
            ProjectileManager.instance.FireProjectile(FireLunarShards.projectilePrefab, self.characterBody.transform.position, Util.QuaternionSafeLookRotation(cone), self.gameObject, self.characterBody.damage * 0.1f / 12f, 0f, Util.CheckRoll(self.characterBody.crit, self.characterBody.master), DamageColorIndex.Default, null, -1f);
          }
        }
        if (self.characterBody.name == "BrotherBody(Clone)")
        {
          int playerCount = PlayerCharacterMasterController.instances.Count;
          if (playerCount > 2)
            playerCount = 2;
          for (int i = 0; i < playerCount; i++)
          {
            DirectorPlacementRule placementRule = new DirectorPlacementRule();
            placementRule.placementMode = DirectorPlacementRule.PlacementMode.Approximate;
            placementRule.minDistance = 3f;
            placementRule.maxDistance = 20f;
            placementRule.spawnOnTarget = self.gameObject.transform;
            Xoroshiro128Plus rng = RoR2Application.rng;
            DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(MithrixGlassCard, placementRule, rng);
            directorSpawnRequest.summonerBodyObject = self.gameObject;
            directorSpawnRequest.onSpawnedServer += (Action<SpawnCard.SpawnResult>)(spawnResult => spawnResult.spawnedInstance.GetComponent<Inventory>().GiveItem(RoR2Content.Items.HealthDecay, 4));
            DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
          }
        }
      }
      orig(self);
    }

    private void WeaponSlamOnEnter(On.EntityStates.BrotherMonster.WeaponSlam.orig_OnEnter orig, WeaponSlam self)
    {
      GameObject projectilePrefab = WeaponSlam.pillarProjectilePrefab;
      projectilePrefab.transform.localScale = new Vector3(4f, 4f, 4f);
      projectilePrefab.GetComponent<ProjectileController>().ghostPrefab.transform.localScale = new Vector3(4f, 4f, 4f);
      hasfired = false;
      int playerCount = PlayerCharacterMasterController.instances.Count;
      if (playerCount > 3)
        playerCount = 3;
      for (int i = 0; i < playerCount; i++)
      {
        DirectorPlacementRule placementRule = new DirectorPlacementRule();
        placementRule.placementMode = DirectorPlacementRule.PlacementMode.Approximate;
        placementRule.minDistance = 3f;
        placementRule.maxDistance = 20f;
        placementRule.spawnOnTarget = self.gameObject.transform;
        Xoroshiro128Plus rng = RoR2Application.rng;
        DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(MithrixGlassCard, placementRule, rng);
        directorSpawnRequest.summonerBodyObject = self.gameObject;
        directorSpawnRequest.onSpawnedServer += (Action<SpawnCard.SpawnResult>)(spawnResult => spawnResult.spawnedInstance.GetComponent<Inventory>().GiveItem(RoR2Content.Items.HealthDecay, 4));
        DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
      }
      orig(self);
    }
    private void WeaponSlamFixedUpdate(On.EntityStates.BrotherMonster.WeaponSlam.orig_FixedUpdate orig, WeaponSlam self)
    {
      if (self.isAuthority)
      {
        if (self.hasDoneBlastAttack)
        {
          Logger.LogDebug("blast attack done");
          if (self.modelTransform)
          {
            if (hasfired == false)
            {
              hasfired = true;
              Logger.LogDebug("modeltransformed");
              int orbCount = ModConfig.SlamOrbProjectileCount.Value;
              int projectileCount = ModConfig.SlamProjectileCount.Value;
              if (phaseCounter == 0)
              {
                orbCount = ModConfig.SlamOrbProjectileCount.Value / 2;
                projectileCount = ModConfig.SlamProjectileCount.Value / 2;
              }
              float num = 360f / orbCount;
              float num2 = 360f / projectileCount;
              Vector3 point = Vector3.ProjectOnPlane(self.inputBank.aimDirection, Vector3.up);
              Transform transform2 = self.FindModelChild(WeaponSlam.muzzleString);
              Vector3 position = transform2.position;
              Vector3 bodyPosition = self.characterBody.transform.position;
              for (int i = 0; i < orbCount; i++)
              {
                Vector3 forward = Quaternion.AngleAxis(num * i, Vector3.up) * point;
                ProjectileManager.instance.FireProjectile(FistSlam.waveProjectilePrefab, position, Util.QuaternionSafeLookRotation(forward), self.gameObject, self.characterBody.damage * FistSlam.waveProjectileDamageCoefficient, FistSlam.waveProjectileForce, Util.CheckRoll(self.characterBody.crit, self.characterBody.master), DamageColorIndex.Default, null, -1f);
              }
            }
          }
        }
      }
      orig(self);
    }

    private void FireLunarShardsOnEnter(On.EntityStates.BrotherMonster.Weapon.FireLunarShards.orig_OnEnter orig, FireLunarShards self)
    {
      if (self is FireLunarShardsHurt)
      {
        self.duration = FireLunarShards.baseDuration / self.attackSpeedStat;
        if (self.isAuthority)
        {
          Ray aimRay = self.GetAimRay();
          Transform modelChild = self.FindModelChild(FireLunarShards.muzzleString);
          if ((bool)(UnityEngine.Object)modelChild)
            aimRay.origin = modelChild.position;
          aimRay.direction = Util.ApplySpread(aimRay.direction, 0.0f, self.maxSpread, self.spreadYawScale, self.spreadPitchScale);
          for (int i = 0; i < 12; i++)
          {
            int num = (int)Util.PlaySound(FireLunarShards.fireSound, self.gameObject);
            ProjectileManager.instance.FireProjectile(new FireProjectileInfo()
            {
              position = aimRay.origin,
              rotation = Quaternion.LookRotation(aimRay.direction),
              crit = self.characterBody.RollCrit(),
              damage = self.characterBody.damage * self.damageCoefficient,
              damageColorIndex = DamageColorIndex.Default,
              owner = self.gameObject,
              procChainMask = new ProcChainMask(),
              force = 0.0f,
              useFuseOverride = false,
              useSpeedOverride = false,
              target = (GameObject)null,
              projectilePrefab = FireLunarShards.projectilePrefab
            });
          }
        }
        self.PlayAnimation("Gesture, Additive", nameof(FireLunarShards));
        self.PlayAnimation("Gesture, Override", nameof(FireLunarShards));
        self.AddRecoil(-0.4f * FireLunarShards.recoilAmplitude, -0.8f * FireLunarShards.recoilAmplitude, -0.3f * FireLunarShards.recoilAmplitude, 0.3f * FireLunarShards.recoilAmplitude);
        self.characterBody.AddSpreadBloom(FireLunarShards.spreadBloomValue);
        EffectManager.SimpleMuzzleFlash(FireLunarShards.muzzleFlashEffectPrefab, self.gameObject, FireLunarShards.muzzleString, false);
      }
      else
      {
        if (!(self is FireLunarShardsHurt))
        {
          if (self.isAuthority)
          {
            Ray aimRay = self.GetAimRay();
            Transform transform = self.FindModelChild(FireLunarShards.muzzleString);
            if (transform)
            {
              aimRay.origin = transform.position;
            }
            FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
            fireProjectileInfo.position = aimRay.origin;
            fireProjectileInfo.rotation = Quaternion.LookRotation(aimRay.direction);
            fireProjectileInfo.crit = self.characterBody.RollCrit();
            fireProjectileInfo.damage = self.characterBody.damage * self.damageCoefficient;
            fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
            fireProjectileInfo.owner = self.gameObject;
            fireProjectileInfo.procChainMask = default(ProcChainMask);
            fireProjectileInfo.force = 0f;
            fireProjectileInfo.useFuseOverride = false;
            fireProjectileInfo.useSpeedOverride = false;
            fireProjectileInfo.target = null;
            fireProjectileInfo.projectilePrefab = FireLunarShards.projectilePrefab;

            for (int i = 0; i < ModConfig.LunarShardAdd.Value; i++)
            {
              ProjectileManager.instance.FireProjectile(fireProjectileInfo);
              aimRay.direction = Util.ApplySpread(aimRay.direction, 0f, self.maxSpread * (1f + 0.45f * i), self.spreadYawScale * (1f + 0.45f * i), self.spreadPitchScale * (1f + 0.45f * i), 0f, 0f);
              fireProjectileInfo.rotation = Quaternion.LookRotation(aimRay.direction);
            }
          }
        }
        orig(self);
      }
    }
    private static void FistSlamOnEnter(On.EntityStates.BrotherMonster.FistSlam.orig_OnEnter orig, FistSlam self)
    {
      FistSlam.waveProjectileDamageCoefficient = 2.3f;
      FistSlam.healthCostFraction = 0.0f;
      FistSlam.waveProjectileCount = 20;
      FistSlam.baseDuration = 3.5f;
      orig(self);
    }
    private static void FistSlamFixedUpdate(On.EntityStates.BrotherMonster.FistSlam.orig_FixedUpdate orig, FistSlam self)
    {
      if ((bool)(UnityEngine.Object)self.modelAnimator && (double)self.modelAnimator.GetFloat("fist.hitBoxActive") > 0.5 && !self.hasAttacked)
      {
        if (self.isAuthority)
        {
          Ray aimRay = self.GetAimRay();
          float num = 360f / (float)FistSlam.waveProjectileCount;
          Vector3 vector3 = Vector3.ProjectOnPlane(self.inputBank.aimDirection, Vector3.up);
          Vector3 footPosition = self.characterBody.footPosition;
          Vector3 corePosition = self.characterBody.corePosition;
          for (int index = 0; index < FistSlam.waveProjectileCount; ++index)
          {
            Vector3 forward = Quaternion.AngleAxis(num * (float)index, Vector3.up) * vector3;
            ProjectileManager.instance.FireProjectile(golemProjectile, corePosition, Util.QuaternionSafeLookRotation(forward), self.gameObject, (self.characterBody.damage * FistSlam.waveProjectileDamageCoefficient) / 4, 0f, Util.CheckRoll(self.characterBody.crit, self.characterBody.master), DamageColorIndex.Default, null, -1f);
            ProjectileManager.instance.FireProjectile(exploderProjectile, corePosition, Util.QuaternionSafeLookRotation(forward), self.gameObject, (self.characterBody.damage * FistSlam.waveProjectileDamageCoefficient) / 10, 0f, Util.CheckRoll(self.characterBody.crit, self.characterBody.master), DamageColorIndex.Default, null, -1f);
            ProjectileManager.instance.FireProjectile(FireLunarShards.projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), self.gameObject, self.characterBody.damage * 0.1f / 12f, 0f, Util.CheckRoll(self.characterBody.crit, self.characterBody.master), DamageColorIndex.Default, null, -1f);
          }
        }
      }
      orig(self);
    }

    private static void SpellChannelEnterStateOnEnter(On.EntityStates.BrotherMonster.SpellChannelEnterState.orig_OnEnter orig, SpellChannelEnterState self)
    {
      SpellChannelEnterState.duration = 20 / Run.instance.loopClearCount;
      orig(self);
    }

    private static void SpellChannelStateOnEnter(On.EntityStates.BrotherMonster.SpellChannelState.orig_OnEnter orig, SpellChannelState self)
    {
      SpellChannelState.stealInterval = 0.75f / Run.instance.loopClearCount;
      SpellChannelState.delayBeforeBeginningSteal = 0.0f;
      SpellChannelState.maxDuration = 15f / Run.instance.loopClearCount;
      self.PlayAnimation("Body", "SpellChannel");
      int num = (int)Util.PlaySound("Play_moonBrother_phase4_itemSuck_start", self.gameObject);
      self.spellChannelChildTransform = self.FindModelChild("SpellChannel");
      if ((bool)(UnityEngine.Object)self.spellChannelChildTransform)
        self.channelEffectInstance = UnityEngine.Object.Instantiate<GameObject>(SpellChannelState.channelEffectPrefab, self.spellChannelChildTransform.position, Quaternion.identity, self.spellChannelChildTransform);
    }
    private void SpellChannelStateOnExit(On.EntityStates.BrotherMonster.SpellChannelState.orig_OnExit orig, SpellChannelState self)
    {
      orig(self);
      // Spawn in BrotherHaunt (Random Flame Lines)
      GameObject brotherHauntGO = Instantiate(BrotherHaunt);
      brotherHauntGO.GetComponent<TeamComponent>().teamIndex = (TeamIndex)2;
      NetworkServer.Spawn(brotherHauntGO);
    }

    private static void SpellChannelExitStateOnEnter(On.EntityStates.BrotherMonster.SpellChannelExitState.orig_OnEnter orig, SpellChannelExitState self)
    {
      SpellChannelExitState.lendInterval = 0.04f;
      SpellChannelExitState.duration = 2.5f;
      orig(self);
    }

    private static void StaggerEnterOnEnter(On.EntityStates.BrotherMonster.StaggerEnter.orig_OnEnter orig, StaggerEnter self)
    {
      self.duration = 0.0f;
      orig(self);
    }

    private static void StaggerExitOnEnter(On.EntityStates.BrotherMonster.StaggerExit.orig_OnEnter orig, StaggerExit self)
    {
      self.duration = 0.0f;
      orig(self);
    }

    private static void StaggerLoopOnEnter(On.EntityStates.BrotherMonster.StaggerLoop.orig_OnEnter orig, StaggerLoop self)
    {
      self.duration = 0.0f;
      orig(self);
    }

    private void TrueDeathStateOnEnter(On.EntityStates.BrotherMonster.TrueDeathState.orig_OnEnter orig, TrueDeathState self)
    {
      TrueDeathState.dissolveDuration = 3f;
      // Kill BrotherHaunt once MithrixHurt dies
      GameObject.Find("BrotherHauntBody(Clone)").GetComponent<HealthComponent>().Suicide();
      orig(self);
    }

    private static void CleanupPillar(On.EntityStates.BrotherMonster.WeaponSlam.orig_OnEnter orig, EntityStates.BrotherMonster.WeaponSlam self)
    {
      GameObject projectilePrefab = EntityStates.BrotherMonster.WeaponSlam.pillarProjectilePrefab;
      projectilePrefab.transform.localScale = new Vector3(1f, 1f, 1f);
      projectilePrefab.GetComponent<ProjectileController>().ghostPrefab.transform.localScale = new Vector3(1f, 1f, 1f);
      orig.Invoke(self);
    }
  }
}