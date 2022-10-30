using BepInEx;
using RoR2;
using RoR2.Skills;
using RoR2.Projectile;
using EntityStates.BrotherMonster;
using EntityStates.BrotherMonster.Weapon;
using R2API;
using R2API.Utils;
using System;
using System.Collections.Generic;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace UmbralMithrix
{
  [BepInPlugin("com.Nuxlar.UmbralMithrix", "UmbralMithrix", "1.4.4")]
  [BepInDependency("com.bepis.r2api")]
  [BepInDependency("com.rune580.riskofoptions")]
  [R2APISubmoduleDependency(new string[]
    {
        "LanguageAPI",
        "PrefabAPI",
        "ContentAddition",
        "ItemAPI"
    })]

  public class UmbralMithrix : BaseUnityPlugin
  {
    bool hasfired;
    public int phaseCounter = 0;
    float elapsed = 0;
    bool shrineActivated = false;
    bool doppelEventHasTriggered = false;
    HashSet<ItemIndex> doppelBlacklist = new();
    ItemDef UmbralItem;
    GameObject Mithrix = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherBody.prefab").WaitForCompletion();
    SkillDef originalDash;
    GameObject Obelisk = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/mysteryspace/MSObelisk.prefab").WaitForCompletion();

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
      CreateDoppelItem();
      On.RoR2.Run.Start += OnRunStart;
      On.EntityStates.EntityState.Update += ExplodeOnSprint;
      On.RoR2.CharacterBody.OnInventoryChanged += OnInventoryChanged;
      On.EntityStates.Interactables.MSObelisk.ReadyToEndGame.OnEnter += ReadyToEndGameOnEnter;
      On.EntityStates.Interactables.MSObelisk.ReadyToEndGame.FixedUpdate += ReadyToEndGameFixedUpdate;
      On.RoR2.Stage.Start += StageStart;
      On.RoR2.Artifacts.DoppelgangerInvasionManager.CreateDoppelganger += CreateDoppelganger;
      On.RoR2.CharacterMaster.OnBodyStart += CharacterMasterOnBodyStart;
      On.EntityStates.BrotherMonster.SkyLeapDeathState.OnEnter += SkyLeapDeathStateOnEnter;
      On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.OnEnter += BrotherEncounterPhaseBaseStateOnEnter;
      On.EntityStates.Missions.BrotherEncounter.Phase1.OnEnter += Phase1OnEnter;
      On.EntityStates.Missions.BrotherEncounter.Phase2.OnEnter += Phase2OnEnter;
      On.EntityStates.Missions.BrotherEncounter.Phase3.OnEnter += Phase3OnEnter;
      On.EntityStates.Missions.BrotherEncounter.Phase4.OnEnter += Phase4OnEnter;
      On.EntityStates.FrozenState.OnEnter += FrozenStateOnEnter;
      On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += AddTimedBuff_BuffDef_float;
      On.EntityStates.BrotherMonster.SprintBash.OnEnter += SprintBashOnEnter;
      On.EntityStates.BrotherMonster.WeaponSlam.OnEnter += WeaponSlamOnEnter;
      On.EntityStates.BrotherMonster.WeaponSlam.FixedUpdate += WeaponSlamFixedUpdate;
      On.EntityStates.BrotherMonster.WeaponSlam.OnEnter += CleanupPillar;
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
      originalDash = Mithrix.GetComponent<SkillLocator>().utility.skillFamily.variants[0].skillDef;
    }

    private void RevertToVanillaStats()
    {
      CharacterBody MithrixBody = Mithrix.GetComponent<CharacterBody>();
      CharacterBody MithrixHurtBody = MithrixHurt.GetComponent<CharacterBody>();
      CharacterDirection MithrixDirection = Mithrix.GetComponent<CharacterDirection>();
      CharacterMotor MithrixMotor = Mithrix.GetComponent<CharacterMotor>();

      MithrixMotor.mass = 900;
      MithrixMotor.airControl = 0.25f;
      MithrixMotor.jumpCount = 1;

      MithrixBody.baseMaxHealth = 1000;
      MithrixBody.levelMaxHealth = 300;
      MithrixBody.baseDamage = 16;
      MithrixBody.levelDamage = 3.2f;

      // Mithrix Hurt
      MithrixHurtBody.baseMaxHealth = 1400;
      MithrixHurtBody.levelMaxHealth = 420;
      MithrixHurtBody.baseArmor = 20;
      // Mithrix Hurt

      MithrixBody.baseAttackSpeed = 1;
      MithrixBody.baseMoveSpeed = 15;
      MithrixBody.baseAcceleration = 45;
      MithrixBody.baseJumpPower = 25;
      MithrixDirection.turnSpeed = 270;

      MithrixBody.baseArmor = 20;

      ProjectileSteerTowardTarget component = FireLunarShards.projectilePrefab.GetComponent<ProjectileSteerTowardTarget>();
      component.rotationSpeed = 20;
      ProjectileDirectionalTargetFinder component2 = FireLunarShards.projectilePrefab.GetComponent<ProjectileDirectionalTargetFinder>();
      component2.lookRange = 80;
      component2.lookCone = 90;
      component2.allowTargetLoss = true;

      WeaponSlam.duration = 3.5f;
      HoldSkyLeap.duration = 3;
      ExitSkyLeap.waveProjectileCount = 12;
      ExitSkyLeap.recastChance = 0;
      UltChannelState.waveProjectileCount = 8;
      UltChannelState.maxDuration = 8;
      UltChannelState.totalWaves = 4;
    }

    private void RevertToVanillaSkills()
    {
      SkillLocator SklLocate = Mithrix.GetComponent<SkillLocator>();
      SkillLocator skillLocator = MithrixHurt.GetComponent<SkillLocator>();
      // MithrixHurt
      SkillFamily fireLunarShardsHurt = skillLocator.primary.skillFamily;
      SkillDef fireLunarShardsHurtSkillDef = fireLunarShardsHurt.variants[0].skillDef;
      fireLunarShardsHurtSkillDef.baseRechargeInterval = 6;
      fireLunarShardsHurtSkillDef.baseMaxStock = 12;
      // Mithrix
      SkillFamily Hammer = SklLocate.primary.skillFamily;
      SkillDef HammerChange = Hammer.variants[0].skillDef;
      HammerChange.baseRechargeInterval = 4;
      HammerChange.baseMaxStock = 1;

      SkillFamily Bash = SklLocate.secondary.skillFamily;
      SkillDef BashChange = Bash.variants[0].skillDef;
      BashChange.baseRechargeInterval = 5;
      BashChange.baseMaxStock = 1;

      SkillFamily Dash = SklLocate.utility.skillFamily;
      Dash.variants[0].skillDef = originalDash;

      SkillFamily Ult = SklLocate.special.skillFamily;
      SkillDef UltChange = Ult.variants[0].skillDef;
      UltChange.baseRechargeInterval = 30;
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
      CharacterBody MithrixGlassBody = MithrixGlass.GetComponent<CharacterBody>();
      CharacterDirection MithrixDirection = Mithrix.GetComponent<CharacterDirection>();
      CharacterMotor MithrixMotor = Mithrix.GetComponent<CharacterMotor>();

      MithrixMotor.mass = ModConfig.mass.Value;
      MithrixMotor.airControl = ModConfig.aircontrol.Value;
      MithrixMotor.jumpCount = ModConfig.jumpcount.Value;

      MithrixGlassBody.baseDamage = ModConfig.basedamage.Value;
      MithrixGlassBody.levelDamage = ModConfig.leveldamage.Value;

      MithrixBody.baseMaxHealth = ModConfig.basehealth.Value + (ModConfig.basehealth.Value * hpMultiplier);
      MithrixBody.levelMaxHealth = ModConfig.levelhealth.Value + (ModConfig.levelhealth.Value * hpMultiplier);
      MithrixBody.baseDamage = ModConfig.basedamage.Value;
      MithrixBody.levelDamage = ModConfig.leveldamage.Value;

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
      UltChannelState.waveProjectileCount = (int)(ModConfig.UltimateWaves.Value * 2);
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

      // Replace dash with blink (creating new skilldef so it can be done while midair)
      SkillFamily Dash = SklLocate.utility.skillFamily;
      SkillDef DashChange = Dash.variants[0].skillDef;
      DashChange.baseMaxStock = ModConfig.UtilStocks.Value;
      DashChange.baseRechargeInterval = ModConfig.UtilCD.Value;

      SkillFamily Ult = SklLocate.special.skillFamily;
      SkillDef UltChange = Ult.variants[0].skillDef;
      UltChange.baseRechargeInterval = ModConfig.SpecialCD.Value;
      UltChange.activationState = new EntityStates.SerializableEntityStateType(typeof(EnterCrushingLeap));
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
      CharacterDirection MithrixDirection = Mithrix.GetComponent<CharacterDirection>();

      MithrixBody.baseMaxHealth = (ModConfig.basehealth.Value + (ModConfig.basehealth.Value * hpMultiplier)) * 10;
      MithrixBody.levelMaxHealth = (ModConfig.levelhealth.Value + (ModConfig.levelhealth.Value * hpMultiplier)) * 10;

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

      MithrixBody.baseMaxHealth = playerCount > 2 ? ModConfig.basehealth.Value + (ModConfig.basehealth.Value * hpMultiplier) : (ModConfig.basehealth.Value + (ModConfig.basehealth.Value * hpMultiplier)) / 2;
      MithrixBody.levelMaxHealth = playerCount > 2 ? ModConfig.levelhealth.Value + (ModConfig.levelhealth.Value * hpMultiplier) : (ModConfig.levelhealth.Value + (ModConfig.levelhealth.Value * hpMultiplier)) / 2;

      MithrixBody.baseMoveSpeed = ModConfig.basespeed.Value + (ModConfig.basespeed.Value * mobilityMultiplier);
      MithrixBody.baseAcceleration = ModConfig.acceleration.Value + (ModConfig.acceleration.Value * mobilityMultiplier);
      MithrixBody.baseJumpPower = ModConfig.jumpingpower.Value + (ModConfig.jumpingpower.Value * mobilityMultiplier);
      MithrixDirection.turnSpeed = ModConfig.turningspeed.Value + (ModConfig.turningspeed.Value * mobilityMultiplier);

      WeaponSlam.duration = (3.5f / ModConfig.baseattackspeed.Value);
      UltChannelState.waveProjectileCount = ModConfig.UltimateWaves.Value;
      UltChannelState.maxDuration = ModConfig.UltimateDuration.Value;
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

      if (ModConfig.doppelPhase4.Value)
      {
        MithrixHurtBody.baseMaxHealth = Run.instance.loopClearCount > 1 ? (ModConfig.basehealth.Value + (ModConfig.basehealth.Value * hpMultiplier)) * 5 : ModConfig.basehealth.Value + (ModConfig.basehealth.Value * hpMultiplier);
        MithrixHurtBody.levelMaxHealth = Run.instance.loopClearCount > 1 ? (ModConfig.levelhealth.Value + (ModConfig.levelhealth.Value * hpMultiplier)) * 5 : ModConfig.levelhealth.Value + (ModConfig.levelhealth.Value * hpMultiplier);
      }
      else
      {
        MithrixHurtBody.baseMaxHealth = ModConfig.basehealth.Value + (ModConfig.basehealth.Value * hpMultiplier);
        MithrixHurtBody.levelMaxHealth = ModConfig.levelhealth.Value + (ModConfig.levelhealth.Value * hpMultiplier);
      }

      MithrixHurtBody.baseArmor = ModConfig.basearmor.Value;
      SkillLocator skillLocatorM = MithrixHurt.GetComponent<SkillLocator>();
      SkillFamily fireLunarShardsHurt = skillLocatorM.primary.skillFamily;
      SkillDef fireLunarShardsHurtSkillDef = fireLunarShardsHurt.variants[0].skillDef;
      fireLunarShardsHurtSkillDef.baseRechargeInterval = ModConfig.SuperShardCD.Value;
      fireLunarShardsHurtSkillDef.baseMaxStock = ModConfig.SuperShardCount.Value;
    }

    private void CreateBlacklist()
    {
      // N'kuhanas Opinion
      doppelBlacklist.Add(RoR2Content.Items.NovaOnHeal.itemIndex);
      // Tesla Coil
      doppelBlacklist.Add(RoR2Content.Items.ShockNearby.itemIndex);
      // Razorwire
      doppelBlacklist.Add(RoR2Content.Items.Thorns.itemIndex);
      // Empathy Cores
      doppelBlacklist.Add(RoR2Content.Items.RoboBallBuddy.itemIndex);
      // Spare Drone Parts
      doppelBlacklist.Add(DLC1Content.Items.DroneWeapons.itemIndex);
    }

    private void AddContent()
    {
      // Add our new EntityStates to the game
      ContentAddition.AddEntityState<EnterCrushingLeap>(out _);
      ContentAddition.AddEntityState<AimCrushingLeap>(out _);
      ContentAddition.AddEntityState<ExitCrushingLeap>(out _);
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
    private void CreateDoppelItem()
    {
      LanguageAPI.Add("UMBRALMITHRIX_UMBRAL_ITEM", "Origin Bonus");
      LanguageAPI.Add("UMBRALMITHRIX_UMBRAL_PICKUP", "For Mithrix ONLY >:(");
      LanguageAPI.Add("UMBRALMITHRIX_UMBRAL_DESC", "Funny umbra skin");

      LanguageAPI.Add("UMBRALMITHRIX_UMBRAL_SUBTITLENAMETOKEN", "The Collective");
      LanguageAPI.Add("UMBRALMITHRIX_UMBRAL_MODIFIER", "Umbral");

      UmbralItem = ScriptableObject.CreateInstance<ItemDef>();
      UmbralItem.name = "UmbralMithrixUmbralItem";
      UmbralItem.deprecatedTier = ItemTier.NoTier;
      UmbralItem.nameToken = "UMBRALMITHRIX_UMBRAL_NAME";
      UmbralItem.pickupToken = "UMBRALMITHRIX_UMBRAL_PICKUP";
      UmbralItem.descriptionToken = "UMBRALMITHRIX_UMBRAL_DESC";
      // UmbralItem.tags = new ItemTag[] { ItemTag.WorldUnique, ItemTag.BrotherBlacklist, ItemTag.CannotSteal };
      ItemDisplayRule[] idr = new ItemDisplayRule[0];
      //ContentAddition.AddItemDef(UmbralItem);
      ItemAPI.Add(new CustomItem(UmbralItem, idr));

      On.RoR2.CharacterBody.GetSubtitle += (orig, self) =>
      {
        if (self.inventory && self.inventory.GetItemCount(UmbralItem) > 0)
        {
          return Language.GetString("UMBRALMITHRIX_UMBRAL_SUBTITLENAMETOKEN");
        }
        return orig(self);
      };

      On.RoR2.Util.GetBestBodyName += (orig, bodyObject) =>
      {
        string toReturn = orig(bodyObject);
        CharacterBody cb = bodyObject.GetComponent<CharacterBody>();
        if (cb && cb.inventory && cb.inventory.GetItemCount(UmbralItem) > 0)
        {
          toReturn = Language.GetString("UMBRALMITHRIX_UMBRAL_MODIFIER") + " " + toReturn; ;
        }
        return toReturn;
      };


      IL.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += (il) =>
      {
        ILCursor c = new ILCursor(il);
        c.GotoNext(
                     x => x.MatchLdsfld(typeof(RoR2Content.Items), "InvadingDoppelganger")
                    );
        c.Index += 2;
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<int, CharacterBody, int>>((vengeanceCount, self) =>
                {
                  int toReturn = vengeanceCount;
                  if (self.inventory)
                  {
                    toReturn += self.inventory.GetItemCount(UmbralItem);
                  }
                  return toReturn;
                });
      };

      IL.RoR2.CharacterModel.UpdateOverlays += (il) =>
      {
        ILCursor c = new ILCursor(il);
        c.GotoNext(
                   x => x.MatchLdsfld(typeof(RoR2Content.Items), "InvadingDoppelganger")
                  );
        c.Index += 2;
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<int, CharacterModel, int>>((vengeanceCount, self) =>
              {
                int toReturn = vengeanceCount;
                if (self.body && self.body.inventory)
                {
                  toReturn += self.body.inventory.GetItemCount(UmbralItem);
                }
                return toReturn;
              });
      };
    }
    private void ExplodeOnSprint(On.EntityStates.EntityState.orig_Update orig, EntityStates.EntityState self)
    {
      orig(self);
      if (self.characterBody)
      {
        if (self.characterBody.isSprinting && self.characterBody.name == "BrotherBody(Clone)")
        {
          float num1 = 180f / 12;
          Vector3 point = Vector3.ProjectOnPlane(self.inputBank.aimDirection, Vector3.up);
          elapsed += Time.deltaTime;
          if (elapsed >= 8f)
          {
            Util.PlaySound("Play_voidRaid_m1_shoot", self.gameObject);
            EffectManager.SimpleMuzzleFlash(FistSlam.slamImpactEffect, self.gameObject, WeaponSlam.muzzleString, false);
            elapsed = elapsed % 8f;
            for (int i = 0; i < 12; ++i)
            {
              Vector3 baseCircle = Quaternion.AngleAxis(num1 * i, Vector3.forward) * point;
              Vector3 angledCircleA = Quaternion.AngleAxis(num1 * i, new Vector3(0, 1, 1)) * point;
              Vector3 angledCircleB = Quaternion.AngleAxis(num1 * i, new Vector3(0, -1, 1)) * point;
              for (int idx = 0; idx < ModConfig.SuperShardWeight.Value; idx++)
                ProjectileManager.instance.FireProjectile(FireLunarShards.projectilePrefab, self.characterBody.coreTransform.position, Quaternion.LookRotation(baseCircle), self.gameObject, self.characterBody.damage * 0.1f / 12f, 0f, Util.CheckRoll(self.characterBody.crit, self.characterBody.master), DamageColorIndex.Default, null, -1f);
              for (int idx = 0; idx < ModConfig.SuperShardWeight.Value; idx++)
                ProjectileManager.instance.FireProjectile(FireLunarShards.projectilePrefab, self.characterBody.coreTransform.position, Quaternion.LookRotation(angledCircleB), self.gameObject, self.characterBody.damage * 0.1f / 12f, 0f, Util.CheckRoll(self.characterBody.crit, self.characterBody.master), DamageColorIndex.Default, null, -1f);
              for (int idx = 0; idx < ModConfig.SuperShardWeight.Value; idx++)
                ProjectileManager.instance.FireProjectile(FireLunarShards.projectilePrefab, self.characterBody.coreTransform.position, Quaternion.LookRotation(angledCircleA), self.gameObject, self.characterBody.damage * 0.1f / 12f, 0f, Util.CheckRoll(self.characterBody.crit, self.characterBody.master), DamageColorIndex.Default, null, -1f);
            }
          }
        }
      }
    }
    private void OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
    {
      orig(self);
      if (NetworkServer.active && self.inventory && shrineActivated && phaseCounter == 3 && (self.inventory.GetItemCount(UmbralItem) > 0 || self.inventory.GetItemCount(RoR2Content.Items.InvadingDoppelganger) > 0))
      {
        // Remove Blacklisted Items
        foreach (ItemIndex item in doppelBlacklist)
        {
          int itemCount = self.inventory.GetItemCount(item);
          if (itemCount > 0)
            self.inventory.RemoveItem(item, itemCount);
        }
      }
    }
    private void StageStart(On.RoR2.Stage.orig_Start orig, RoR2.Stage self)
    {
      orig(self);
      if (self.sceneDef.cachedName == "moon2")
        SpawnUmbralObelisk();
    }

    private void SpawnUmbralObelisk()
    {
      // 1090.1f, -283.1f, 1138.6f
      GameObject obelisk = Instantiate(Obelisk, new Vector3(1090.1f, -283.1f, 1138.6f), Quaternion.identity);
      obelisk.GetComponent<PurchaseInteraction>().NetworkcontextToken = "Summon The Umbral King?";
      obelisk.name = "UmbralObelisk";
      obelisk.transform.eulerAngles = new Vector3(0.0f, 66f, 0.0f);
      NetworkServer.Spawn(obelisk);
    }

    private void ReadyToEndGameOnEnter(On.EntityStates.Interactables.MSObelisk.ReadyToEndGame.orig_OnEnter orig, EntityStates.Interactables.MSObelisk.ReadyToEndGame self)
    {
      if (self.gameObject.name == "UmbralObelisk")
      {
        self.purchaseInteraction = self.GetComponent<PurchaseInteraction>();
        shrineActivated = true;
        self.purchaseInteraction.Networkavailable = false;
        Chat.SendBroadcastChat(new Chat.SimpleChatMessage() { baseToken = $"<color=#8826dd>The Umbral King awaits...</color>" });
        self.GetComponent<ChildLocator>().FindChild(EntityStates.Interactables.MSObelisk.ReadyToEndGame.chargeupChildString).gameObject.SetActive(true);
        int num = (int)Util.PlaySound(EntityStates.Interactables.MSObelisk.ReadyToEndGame.chargeupSoundString, self.gameObject);
      }
      else
        orig(self);
    }

    private void ReadyToEndGameFixedUpdate(On.EntityStates.Interactables.MSObelisk.ReadyToEndGame.orig_FixedUpdate orig, EntityStates.Interactables.MSObelisk.ReadyToEndGame self)
    {
      if (self.gameObject.name == "UmbralObelisk")
      {
        if ((double)self.fixedAge < (double)EntityStates.Interactables.MSObelisk.ReadyToEndGame.chargeupDuration || self.ready)
          return;
      }
      else
        orig(self);
    }

    private void OnRunStart(On.RoR2.Run.orig_Start orig, Run self)
    {
      shrineActivated = false;
      CreateBlacklist();
      RevertToVanillaStats();
      RevertToVanillaSkills();
      orig(self);
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
      if (ModConfig.doppelPhase4.Value && shrineActivated)
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
      else
        orig(srcCharacterMaster, rng);
    }
    private void CharacterMasterOnBodyStart(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body)
    {
      orig(self, body);
      if (shrineActivated)
      {
        if (self.name == "BrotherGlassMaster(Clone)")
          body.skillLocator.secondary.ExecuteIfReady();
        if (PhaseCounter.instance)
        {
          if ((PhaseCounter.instance.phase == 2 || PhaseCounter.instance.phase == 3) && (body.name == "LunarGolemBody(Clone)" || body.name == "LunarExploderBody(Clone)" || body.name == "LunarWispBody(Clone)"))
            body.healthComponent.Suicide();
        }
        // Make Mithrix an Umbra
        if (ModConfig.umbraToggle.Value && (body.name == "BrotherBody(Clone)" || body.name == "BrotherHurtBody(Clone)" || body.name == "BrotherGlassBody(Clone)"))
          self.inventory.GiveItemString(UmbralItem.name);
        if (self.name == "BrotherHurtMaster(Clone)" && !ModConfig.doppelPhase4.Value)
        {
          body.AddBuff(RoR2Content.Buffs.Immune);
          Task.Delay(3000).ContinueWith(o => { body.RemoveBuff(RoR2Content.Buffs.Immune); });
        }
        if (self.name == "BrotherHurtMaster(Clone)" && ModConfig.doppelPhase4.Value)
        {
          if (self.inventory.GetItemCount(RoR2Content.Items.ExtraLifeConsumed.itemIndex) == 0)
            body.AddBuff(RoR2Content.Buffs.Immune);
          if (!doppelEventHasTriggered)
            RoR2.Artifacts.DoppelgangerInvasionManager.PerformInvasion(RoR2Application.rng);
          doppelEventHasTriggered = true;
          if (Run.instance.loopClearCount != 0)
            Task.Delay(20000 / Run.instance.loopClearCount).ContinueWith(o => { body.RemoveBuff(RoR2Content.Buffs.Immune); });
          else
            Task.Delay(20000).ContinueWith(o => { body.RemoveBuff(RoR2Content.Buffs.Immune); });
        }
      }
    }

    private void SkyLeapDeathStateOnEnter(On.EntityStates.BrotherMonster.SkyLeapDeathState.orig_OnEnter orig, EntityStates.BrotherMonster.SkyLeapDeathState self)
    {
      if (self.characterBody.name == "BrotherGlassBody(Clone)")
      {
        self.DestroyModel();
        if (!NetworkServer.active)
          return;
        self.DestroyBodyAsapServer();
      }
      else
        orig(self);
    }

    // Phase 2 change to encounter spawns (Mithrix instead of Chimera)
    private void BrotherEncounterPhaseBaseStateOnEnter(On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState self)
    {
      if (shrineActivated)
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
      else
        orig(self);
    }

    private void Phase1OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase1.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.Phase1 self)
    {
      doppelEventHasTriggered = false;
      if (shrineActivated)
      {
        Logger.LogMessage("Accursing the King of Nothing");
        AdjustBaseSkills();
        AdjustBaseStats();
      }
      orig(self);
    }

    private void Phase2OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase2.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.Phase2 self)
    {
      // Phase 2 Skip
      if (shrineActivated && ModConfig.phase2Skip.Value)
      {
        orig(self);
        self.PreEncounterBegin();
        self.outer.SetNextState((EntityStates.EntityState)new EntityStates.Missions.BrotherEncounter.Phase3());
      }
      // Regular Umbral Phase 2
      else if (shrineActivated)
      {
        self.KillAllMonsters();
        AdjustPhase2Stats();
        orig(self);
      }
      else
        orig(self);
    }

    private void Phase3OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase3.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.Phase3 self)
    {
      if (shrineActivated)
      {
        self.KillAllMonsters();
        AdjustPhase3Stats();
      }
      orig(self);
    }

    private void Phase4OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase4.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.Phase4 self)
    {
      if (shrineActivated)
        AdjustPhase4Stats();
      orig(self);
    }

    // Adds more projectiles to SprintBash in a cone shape
    private void SprintBashOnEnter(On.EntityStates.BrotherMonster.SprintBash.orig_OnEnter orig, SprintBash self)
    {
      if (shrineActivated)
      {
        if (self.isAuthority)
        {
          if (self.characterBody.name == "BrotherGlassBody(Clone)")
          {
            Ray aimRay = self.GetAimRay();
            for (int i = 0; i < ModConfig.SuperShardWeight.Value; i++)
            {
              Util.PlaySound(EntityStates.BrotherMonster.Weapon.FireLunarShards.fireSound, self.gameObject);
              ProjectileManager.instance.FireProjectile(FireLunarShards.projectilePrefab, aimRay.origin, Quaternion.LookRotation(aimRay.direction), self.gameObject, self.characterBody.damage * 0.1f / 12f, 0f, Util.CheckRoll(self.characterBody.crit, self.characterBody.master), DamageColorIndex.Default, null, -1f);
            }
          }
          if (self.characterBody.name == "BrotherBody(Clone)")
          {
            for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
            {
              DirectorPlacementRule placementRule = new DirectorPlacementRule();
              placementRule.placementMode = DirectorPlacementRule.PlacementMode.Approximate;
              placementRule.minDistance = 3f;
              placementRule.maxDistance = 20f;
              placementRule.spawnOnTarget = PlayerCharacterMasterController.instances[i].master.GetBody().coreTransform;
              Xoroshiro128Plus rng = RoR2Application.rng;
              DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(MithrixGlassCard, placementRule, rng);
              directorSpawnRequest.summonerBodyObject = self.gameObject;
              directorSpawnRequest.onSpawnedServer += (Action<SpawnCard.SpawnResult>)(spawnResult => spawnResult.spawnedInstance.GetComponent<Inventory>().GiveItem(RoR2Content.Items.HealthDecay, 2));
              DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
            }
          }
        }
      }
      orig(self);
    }

    private void WeaponSlamOnEnter(On.EntityStates.BrotherMonster.WeaponSlam.orig_OnEnter orig, WeaponSlam self)
    {
      if (shrineActivated)
      {
        GameObject projectilePrefab = WeaponSlam.pillarProjectilePrefab;
        if (phaseCounter == 2)
        {
          projectilePrefab.transform.localScale = new Vector3(2f, 2f, 2f);
          projectilePrefab.GetComponent<ProjectileController>().ghostPrefab.transform.localScale = new Vector3(2f, 2f, 2f);
        }
        else
        {
          projectilePrefab.transform.localScale = new Vector3(4f, 4f, 4f);
          projectilePrefab.GetComponent<ProjectileController>().ghostPrefab.transform.localScale = new Vector3(4f, 4f, 4f);
        }
        hasfired = false;
        if (phaseCounter != 2)
        {
          for (int idx = 0; idx < 2; idx++)
          {
            for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
            {
              DirectorPlacementRule placementRule = new DirectorPlacementRule();
              placementRule.placementMode = DirectorPlacementRule.PlacementMode.Approximate;
              placementRule.minDistance = 3f;
              placementRule.maxDistance = 20f;
              placementRule.spawnOnTarget = PlayerCharacterMasterController.instances[i].master.GetBody().coreTransform;
              Xoroshiro128Plus rng = RoR2Application.rng;
              DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(MithrixGlassCard, placementRule, rng);
              directorSpawnRequest.summonerBodyObject = self.gameObject;
              directorSpawnRequest.onSpawnedServer += (Action<SpawnCard.SpawnResult>)(spawnResult => spawnResult.spawnedInstance.GetComponent<Inventory>().GiveItem(RoR2Content.Items.HealthDecay, 2));
              DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
            }
          }
        }
      }
      orig(self);
    }

    private void WeaponSlamFixedUpdate(On.EntityStates.BrotherMonster.WeaponSlam.orig_FixedUpdate orig, EntityStates.BrotherMonster.WeaponSlam self)
    {
      if (phaseCounter == 1)
      {
        if (PhaseCounter.instance)
          PhaseCounter.instance.phase = 3;
        orig(self);
        if (PhaseCounter.instance)
          PhaseCounter.instance.phase = 2;
      }
      else
        orig(self);
    }

    private void FireLunarShardsOnEnter(On.EntityStates.BrotherMonster.Weapon.FireLunarShards.orig_OnEnter orig, FireLunarShards self)
    {
      if (shrineActivated)
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
            for (int i = 0; i < ModConfig.SuperShardWeight.Value; i++)
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
      else
        orig(self);
    }
    private void FistSlamOnEnter(On.EntityStates.BrotherMonster.FistSlam.orig_OnEnter orig, FistSlam self)
    {
      if (shrineActivated)
      {
        FistSlam.waveProjectileDamageCoefficient = 2.3f;
        FistSlam.healthCostFraction = 0.0f;
        FistSlam.waveProjectileCount = 20;
        FistSlam.baseDuration = 3.5f;
      }
      orig(self);
    }
    private void FistSlamFixedUpdate(On.EntityStates.BrotherMonster.FistSlam.orig_FixedUpdate orig, FistSlam self)
    {
      if (shrineActivated)
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
      }
      orig(self);
    }

    private void SpellChannelEnterStateOnEnter(On.EntityStates.BrotherMonster.SpellChannelEnterState.orig_OnEnter orig, SpellChannelEnterState self)
    {
      if (shrineActivated && !ModConfig.doppelPhase4.Value)
        SpellChannelEnterState.duration = 2.5f;
      if (shrineActivated && ModConfig.doppelPhase4.Value)
      {
        if (Run.instance.loopClearCount == 0)
          SpellChannelEnterState.duration = 20;
        else
          SpellChannelEnterState.duration = 20 / Run.instance.loopClearCount;
      }
      orig(self);
    }

    private void SpellChannelStateOnEnter(On.EntityStates.BrotherMonster.SpellChannelState.orig_OnEnter orig, SpellChannelState self)
    {
      if (shrineActivated && ModConfig.doppelPhase4.Value)
      {
        int loopClearCount = 1;
        if (Run.instance.loopClearCount != 0)
          loopClearCount = Run.instance.loopClearCount;
        SpellChannelState.stealInterval = 0.75f / loopClearCount;
        SpellChannelState.delayBeforeBeginningSteal = 0.0f;
        SpellChannelState.maxDuration = 15f / loopClearCount;
        self.PlayAnimation("Body", "SpellChannel");
        int num = (int)Util.PlaySound("Play_moonBrother_phase4_itemSuck_start", self.gameObject);
        self.spellChannelChildTransform = self.FindModelChild("SpellChannel");
        if ((bool)(UnityEngine.Object)self.spellChannelChildTransform)
          self.channelEffectInstance = UnityEngine.Object.Instantiate<GameObject>(SpellChannelState.channelEffectPrefab, self.spellChannelChildTransform.position, Quaternion.identity, self.spellChannelChildTransform);
      }
      else
        orig(self);
    }
    private void SpellChannelStateOnExit(On.EntityStates.BrotherMonster.SpellChannelState.orig_OnExit orig, SpellChannelState self)
    {
      orig(self);
      if (shrineActivated)
      {
        // Spawn in BrotherHaunt (Random Flame Lines)
        GameObject brotherHauntGO = Instantiate(BrotherHaunt);
        brotherHauntGO.GetComponent<TeamComponent>().teamIndex = (TeamIndex)2;
        NetworkServer.Spawn(brotherHauntGO);
      }
    }

    private void SpellChannelExitStateOnEnter(On.EntityStates.BrotherMonster.SpellChannelExitState.orig_OnEnter orig, SpellChannelExitState self)
    {
      if (shrineActivated)
      {
        SpellChannelExitState.lendInterval = 0.04f;
        SpellChannelExitState.duration = 2.5f;
      }
      orig(self);
    }

    private void StaggerEnterOnEnter(On.EntityStates.BrotherMonster.StaggerEnter.orig_OnEnter orig, StaggerEnter self)
    {
      if (shrineActivated)
        self.duration = 0.0f;
      orig(self);
    }

    private void StaggerExitOnEnter(On.EntityStates.BrotherMonster.StaggerExit.orig_OnEnter orig, StaggerExit self)
    {
      if (shrineActivated)
        self.duration = 0.0f;
      orig(self);
    }

    private void StaggerLoopOnEnter(On.EntityStates.BrotherMonster.StaggerLoop.orig_OnEnter orig, StaggerLoop self)
    {
      if (shrineActivated)
        self.duration = 0.0f;
      orig(self);
    }

    private void TrueDeathStateOnEnter(On.EntityStates.BrotherMonster.TrueDeathState.orig_OnEnter orig, TrueDeathState self)
    {
      if (shrineActivated)
      {
        TrueDeathState.dissolveDuration = 3f;
        // Kill BrotherHaunt once MithrixHurt dies
        GameObject.Find("BrotherHauntBody(Clone)").GetComponent<HealthComponent>().Suicide();
      }
      orig(self);
    }

    private void CleanupPillar(On.EntityStates.BrotherMonster.WeaponSlam.orig_OnEnter orig, EntityStates.BrotherMonster.WeaponSlam self)
    {
      if (shrineActivated)
      {
        GameObject projectilePrefab = EntityStates.BrotherMonster.WeaponSlam.pillarProjectilePrefab;
        projectilePrefab.transform.localScale = new Vector3(1f, 1f, 1f);
        projectilePrefab.GetComponent<ProjectileController>().ghostPrefab.transform.localScale = new Vector3(1f, 1f, 1f);
      }
      orig(self);
    }
  }
}