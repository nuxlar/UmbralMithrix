﻿using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptions.OptionConfigs;

namespace UmbralMithrix
{
  internal class ModConfig
  {
    public static ConfigEntry<bool> doppelPhase4;
    public static ConfigEntry<bool> phase2Skip;
    public static ConfigEntry<bool> debuffResistance;

    public static ConfigEntry<float> phase1BaseHPScaling;
    public static ConfigEntry<float> phase2BaseHPScaling;
    public static ConfigEntry<float> phase3BaseHPScaling;
    public static ConfigEntry<float> phase4BaseHPScaling;
    public static ConfigEntry<float> phase1BaseMobilityScaling;
    public static ConfigEntry<float> phase2BaseMobilityScaling;
    public static ConfigEntry<float> phase3BaseMobilityScaling;

    public static ConfigEntry<float> phase1LoopHPScaling;
    public static ConfigEntry<float> phase2LoopHPScaling;
    public static ConfigEntry<float> phase3LoopHPScaling;
    public static ConfigEntry<float> phase4LoopHPScaling;
    public static ConfigEntry<float> phase1LoopMobilityScaling;
    public static ConfigEntry<float> phase2LoopMobilityScaling;
    public static ConfigEntry<float> phase3LoopMobilityScaling;

    public static ConfigEntry<float> phase1PlayerHPScaling;
    public static ConfigEntry<float> phase1PlayerMobilityScaling;
    public static ConfigEntry<float> phase2PlayerHPScaling;
    public static ConfigEntry<float> phase2PlayerMobilityScaling;
    public static ConfigEntry<float> phase3PlayerHPScaling;
    public static ConfigEntry<float> phase3PlayerMobilityScaling;
    public static ConfigEntry<float> phase4PlayerHPScaling;

    public static ConfigEntry<float> basehealth;
    public static ConfigEntry<float> levelhealth;
    public static ConfigEntry<float> basedamage;
    public static ConfigEntry<float> leveldamage;
    public static ConfigEntry<float> basearmor;
    public static ConfigEntry<float> baseattackspeed;

    public static ConfigEntry<float> basespeed;
    public static ConfigEntry<float> mass;
    public static ConfigEntry<float> turningspeed;
    public static ConfigEntry<float> jumpingpower;
    public static ConfigEntry<float> acceleration;
    public static ConfigEntry<int> jumpcount;
    public static ConfigEntry<float> aircontrol;

    public static ConfigEntry<int> PrimStocks;
    public static ConfigEntry<int> SecStocks;
    public static ConfigEntry<int> UtilStocks;
    public static ConfigEntry<float> PrimCD;
    public static ConfigEntry<float> SecCD;
    public static ConfigEntry<float> UtilCD;
    public static ConfigEntry<float> SpecialCD;

    public static ConfigEntry<int> SuperShardWeight;
    public static ConfigEntry<float> CrushingLeap;
    public static ConfigEntry<int> SuperShardCount;
    public static ConfigEntry<float> SuperShardCD;
    public static ConfigEntry<int> WurmOrbs;
    public static ConfigEntry<float> WurmLaserCD;
    public static ConfigEntry<float> WurmOrbCD;


    public static ConfigEntry<int> SlamOrbProjectileCount;
    public static ConfigEntry<int> SlamProjectileCount;
    public static ConfigEntry<int> BashProjectileCount;
    public static ConfigEntry<int> DashProjectileCount;
    public static ConfigEntry<int> SkyleapProjectileCount;
    public static ConfigEntry<int> LunarShardAdd;
    public static ConfigEntry<int> UltimateWaves;
    public static ConfigEntry<int> UltimateCount;
    public static ConfigEntry<float> UltimateDuration;
    public static ConfigEntry<float> JumpRecast;
    public static ConfigEntry<float> JumpPause;
    public static ConfigEntry<int> JumpWaveCount;
    public static ConfigEntry<float> ShardHoming;
    public static ConfigEntry<float> ShardRange;
    public static ConfigEntry<float> ShardCone;


    public static void InitConfig(ConfigFile config)
    {
      doppelPhase4 = config.Bind("General", "Phase 4 Wurm Fight", false, "Toggle the alternate Phase 4 fight");
      phase2Skip = config.Bind("General", "Phase 2 Skip", false, "Skip The Reworked Phase 2 for a more casual fight");
      debuffResistance = config.Bind("General", "Freeze/Nullify Immune", false, "Toggle the debuff resistance for loop 1, will not turn off for loops 2 and up");

      phase1BaseHPScaling = config.Bind("Scaling First Loop", "P1 HP", 0f, "HP boost percentage for Phase 1 FIRST LOOP (5 stages)");
      phase2BaseHPScaling = config.Bind("Scaling First Loop", "P2 HP", 0f, "HP boost percentage for Phase 2 FIRST LOOP");
      phase3BaseHPScaling = config.Bind("Scaling First Loop", "P3 HP", 0f, "HP boost percentage for Phase 3 FIRST LOOP");
      phase4BaseHPScaling = config.Bind("Scaling First Loop", "P4 HP", 0f, "HP boost percentage for Phase 4 FIRST LOOP");
      phase1BaseMobilityScaling = config.Bind("Scaling First Loop", "P1 Mobility", 0f, "Mobility boost percentage for Phase 1 FIRST LOOP");
      phase2BaseMobilityScaling = config.Bind("Scaling First Loop", "P2 Mobility", 0f, "Mobility boost percentage for Phase 2 FIRST LOOP");
      phase3BaseMobilityScaling = config.Bind("Scaling First Loop", "P3 Mobility", 0f, "Mobility boost percentage for Phase 3 FIRST LOOP");

      phase1LoopHPScaling = config.Bind("Scaling Per Loop", "P1 Loop HP", 0.2f, "HP boost percentage for Phase 1 PER LOOP (every 5 stages)");
      phase2LoopHPScaling = config.Bind("Scaling Per Loop", "P2 Loop HP", 0.3f, "HP boost percentage for Phase 2 PER LOOP");
      phase3LoopHPScaling = config.Bind("Scaling Per Loop", "P3 Loop HP", 0.4f, "HP boost percentage for Phase 3 PER LOOP");
      phase4LoopHPScaling = config.Bind("Scaling Per Loop", "P4 Loop HP", 0.5f, "HP boost percentage for Phase 4 PER LOOP");
      phase1LoopMobilityScaling = config.Bind("Scaling Per Loop", "P1 Loop Mobility", 0.15f, "Mobility (movementspd, acceleration, turningspd) boost percentage for Phase 1 PER LOOP");
      phase2LoopMobilityScaling = config.Bind("Scaling Per Loop", "P2 Loop Mobility", 0.25f, "Mobility boost percentage for Phase 2 PER LOOP");
      phase3LoopMobilityScaling = config.Bind("Scaling Per Loop", "P3 Loop Mobility", 0.35f, "Mobility boost percentage for Phase 3 PER LOOP");

      phase1PlayerHPScaling = config.Bind("Player Scaling", "P1 HP Scaling", 0.0125f, "HP boost percentage for Phase 1 PER PLAYER");
      phase2PlayerHPScaling = config.Bind("Player Scaling", "P2 HP Scaling", 0.025f, "HP boost percentage for Phase 2 PER PLAYER");
      phase3PlayerHPScaling = config.Bind("Player Scaling", "P3 HP Scaling", 0.05f, "HP boost percentage for Phase 3 PER PLAYER");
      phase4PlayerHPScaling = config.Bind("Player Scaling", "P4 HP Scaling", 0.075f, "HP boost percentage for Phase 4 PER PLAYER");
      phase1PlayerMobilityScaling = config.Bind("Player Scaling", "P1 Mobility Scaling", 0.0125f, "Mobility boost percentage for Phase 1 PER PLAYER");
      phase2PlayerMobilityScaling = config.Bind("Player Scaling", "P2 Mobility Scaling", 0.025f, "Mobility boost percentage for Phase 2 PER PLAYER");
      phase3PlayerMobilityScaling = config.Bind("Player Scaling", "P3 Mobility Scaling", 0.05f, "Mobility boost percentage for Phase 3 PER PLAYER");

      basehealth = config.Bind("Stats", "Base Health", 1100f, "Vanilla: 1000");
      levelhealth = config.Bind("Stats", "Level Health", 325f, "Health gained per level. Vanilla: 300");
      basedamage = config.Bind("Stats", "Base Damage", 15f, "Vanilla: 16");
      leveldamage = config.Bind("Stats", "Level Damage", 3f, "Damage gained per level. Vanilla: 3.2");
      basearmor = config.Bind("Stats", "Base Armor", 30f, "Vanilla: 20");
      baseattackspeed = config.Bind("Stats", "Base Attack Speed", 1.25f, "Vanilla: 1");

      basespeed = config.Bind("Movement", "Base Move Speed", 15f, "Vanilla: 15");
      mass = config.Bind("Movement", "Mass", 5000f, "Recommended to increase if you increase his movement speed. Vanilla: 900");
      turningspeed = config.Bind("Movement", "Turn Speed", 450f, "Vanilla: 270");
      jumpingpower = config.Bind("Movement", "Moon Shoes", 75f, "How high Mithrix jumps. Vanilla: 25");
      acceleration = config.Bind("Movement", "Acceleration", 300f, "Vanilla: 45");
      jumpcount = config.Bind("Movement", "Jump Count", 3, "Probably doesn't do anything. Vanilla: 0");
      aircontrol = config.Bind("Movement", "Air Control", 1.5f, "Vanilla: 0.25");

      PrimStocks = config.Bind("Skills", "Primary Stocks", 1, "Max Stocks for Mithrix's Weapon Slam. Vanilla: 1");
      SecStocks = config.Bind("Skills", "Secondary Stocks", 1, "Max Stocks for Mithrix's Dash Attack. Vanilla: 1");
      UtilStocks = config.Bind("Skills", "Util Stocks", 4, "Max Stocks for Mithrix's Dash. Vanilla: 2");
      PrimCD = config.Bind("Skills", "Primary Cooldown", 4f, "Cooldown for Mithrix's Weapon Slam. Vanilla: 4");
      SecCD = config.Bind("Skills", "Secondary Cooldown", 4.5f, "Cooldown for Mithrix's Dash Attack. Vanilla: 5");
      UtilCD = config.Bind("Skills", "Util Cooldown", 2.5f, "Cooldown for Mithrix's Dash. Vanilla: 3");
      SpecialCD = config.Bind("Skills", "Special Cooldown", 30f, "Cooldown for Mithrix's Jump Attack. Vanilla: 30");

      SuperShardWeight = config.Bind("New Skills", "Super Shards", 6, "How many shards are in 1 super shard. Vanilla: N/A");
      CrushingLeap = config.Bind("New Skills", "Crushing Leap", 2f, "How long Mithrix stays in the air during the crushing leap. Vanilla: N/A");
      SuperShardCount = config.Bind("New Skills", "Super Shard Stocks", 3, "How many super shards Mithrix can fire before CD. Vanilla: N/A");
      SuperShardCD = config.Bind("New Skills", "Super Shard CD", 4f, "How long it takes for Mithrix to fire super shards again. Vanilla: N/A");
      WurmOrbs = config.Bind("New Skills", "Wurm Orbs", 6, "How many orbs are fired per wave by the Umbral Wurm. Vanilla: N/A");
      WurmLaserCD = config.Bind("New Skills", "Wurm Laser CD", 24f, "Cooldown for the Umbral Wurm's Laser attack. Vanilla: N/A");
      WurmOrbCD = config.Bind("New Skills", "Wurm Orb CD", 12f, "Cooldown for the Umbral Wurm's Orb attack. Vanilla: N/A");

      SlamOrbProjectileCount = config.Bind("Skill Mods", "Orb Projectile Count", 6, "Orbs fired by weapon slam in a circle. Vanilla: N/A");
      LunarShardAdd = config.Bind("Skill Mods", "Shard Add Count", 2, "Bonus shards added to each shot of lunar shards. Vanilla: N/A");
      UltimateWaves = config.Bind("Skill Mods", "P3 Ult Lines", 10, "Total lines in ultimate per burst. Vanilla: 4");
      UltimateCount = config.Bind("Skill Mods", "P3 Ult Bursts", 6, "Total times the ultimate fires. Vanilla: 4");
      UltimateDuration = config.Bind("Skill Mods", "P3 Ult Duration", 7.5f, "How long ultimate lasts. Vanilla: 8");
      JumpRecast = config.Bind("Skill Mods", "Recast Chance", 0f, "Chance Mithrix has to recast his jump skill. USE WITH CAUTION. Vanilla: 0");
      JumpPause = config.Bind("Skill Mods", "Jump Delay", 1f, "How long Mithrix spends in the air when using his jump special. Vanilla: 3");
      JumpWaveCount = config.Bind("Skill Mods", "Jump Wave Count", 16, "Shockwave count when Mithrix lands after a jump. Vanilla: 12");
      ShardHoming = config.Bind("Skill Mods", "Shard Homing", 25f, "How strongly lunar shards home in to targets. Vanilla: 20");
      ShardRange = config.Bind("Skill Mods", "Shard Range", 100f, "Range (distance) in which shards look for targets. Vanilla: 80");
      ShardCone = config.Bind("Skill Mods", "Shard Cone", 120f, "Cone (Angle) in which shards look for targets. Vanilla: 90");

      // Risk Of Options Setup
      // General
      ModSettingsManager.AddOption(new CheckBoxOption(doppelPhase4));
      ModSettingsManager.AddOption(new CheckBoxOption(phase2Skip));
      ModSettingsManager.AddOption(new CheckBoxOption(debuffResistance));
      // Scaling
      ModSettingsManager.AddOption(new StepSliderOption(phase1BaseHPScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.025f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase2BaseHPScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.025f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase3BaseHPScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.025f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase4BaseHPScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.025f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase1BaseMobilityScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.025f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase2BaseMobilityScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.025f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase3BaseMobilityScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.025f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase1LoopHPScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.025f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase2LoopHPScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.025f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase3LoopHPScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.025f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase4LoopHPScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.025f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase1LoopMobilityScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.05f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase2LoopMobilityScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.05f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase3LoopMobilityScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.05f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase1PlayerHPScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.0125f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase2PlayerHPScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.0125f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase3PlayerHPScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.0125f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase1PlayerMobilityScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.0125f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase2PlayerMobilityScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.0125f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase3PlayerMobilityScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.0125f }));
      ModSettingsManager.AddOption(new StepSliderOption(phase4PlayerHPScaling, new StepSliderConfig() { min = 0, max = 1, increment = 0.0125f }));
      // Stats
      ModSettingsManager.AddOption(new StepSliderOption(basehealth, new StepSliderConfig() { min = 500, max = 2500, increment = 50f }));
      ModSettingsManager.AddOption(new StepSliderOption(levelhealth, new StepSliderConfig() { min = 100, max = 500, increment = 25f }));
      ModSettingsManager.AddOption(new StepSliderOption(basedamage, new StepSliderConfig() { min = 10, max = 30, increment = 1f }));
      ModSettingsManager.AddOption(new StepSliderOption(leveldamage, new StepSliderConfig() { min = 1, max = 6.4f, increment = 0.25f }));
      ModSettingsManager.AddOption(new StepSliderOption(basearmor, new StepSliderConfig() { min = 5, max = 50, increment = 5f }));
      ModSettingsManager.AddOption(new StepSliderOption(baseattackspeed, new StepSliderConfig() { min = 0.25f, max = 3, increment = 0.25f }));
      // Movement
      ModSettingsManager.AddOption(new StepSliderOption(basespeed, new StepSliderConfig() { min = 10, max = 30, increment = 1f }));
      ModSettingsManager.AddOption(new StepSliderOption(mass, new StepSliderConfig() { min = 900, max = 10000, increment = 100f }));
      ModSettingsManager.AddOption(new StepSliderOption(turningspeed, new StepSliderConfig() { min = 200, max = 1000, increment = 10f }));
      ModSettingsManager.AddOption(new StepSliderOption(jumpingpower, new StepSliderConfig() { min = 25, max = 100, increment = 5f }));
      ModSettingsManager.AddOption(new StepSliderOption(acceleration, new StepSliderConfig() { min = 45, max = 500, increment = 5f }));
      ModSettingsManager.AddOption(new IntSliderOption(jumpcount, new IntSliderConfig() { min = 1, max = 5 }));
      ModSettingsManager.AddOption(new StepSliderOption(aircontrol, new StepSliderConfig() { min = 0.25f, max = 3, increment = 0.25f }));
      // Skills
      ModSettingsManager.AddOption(new IntSliderOption(PrimStocks, new IntSliderConfig() { min = 1, max = 5 }));
      ModSettingsManager.AddOption(new IntSliderOption(SecStocks, new IntSliderConfig() { min = 1, max = 5 }));
      ModSettingsManager.AddOption(new IntSliderOption(UtilStocks, new IntSliderConfig() { min = 1, max = 5 }));
      ModSettingsManager.AddOption(new StepSliderOption(PrimCD, new StepSliderConfig() { min = 1, max = 5, increment = 0.25f }));
      ModSettingsManager.AddOption(new StepSliderOption(SecCD, new StepSliderConfig() { min = 1, max = 5, increment = 0.25f }));
      ModSettingsManager.AddOption(new StepSliderOption(UtilCD, new StepSliderConfig() { min = 1, max = 5, increment = 0.25f }));
      ModSettingsManager.AddOption(new StepSliderOption(SpecialCD, new StepSliderConfig() { min = 10, max = 50, increment = 1f }));
      // New Skills
      ModSettingsManager.AddOption(new IntSliderOption(SuperShardWeight, new IntSliderConfig() { min = 3, max = 12 }));
      ModSettingsManager.AddOption(new StepSliderOption(CrushingLeap, new StepSliderConfig() { min = 0.1f, max = 6, increment = 0.1f }));
      ModSettingsManager.AddOption(new IntSliderOption(SuperShardCount, new IntSliderConfig() { min = 1, max = 12 }));
      ModSettingsManager.AddOption(new StepSliderOption(SuperShardCD, new StepSliderConfig() { min = 1, max = 5, increment = 0.25f }));
      ModSettingsManager.AddOption(new IntSliderOption(WurmOrbs, new IntSliderConfig() { min = 1, max = 12 }));
      ModSettingsManager.AddOption(new StepSliderOption(WurmLaserCD, new StepSliderConfig() { min = 1, max = 30, increment = 1 }));
      ModSettingsManager.AddOption(new StepSliderOption(WurmOrbCD, new StepSliderConfig() { min = 1, max = 30, increment = 1 }));
      // Skill Mods
      ModSettingsManager.AddOption(new IntSliderOption(SlamOrbProjectileCount, new IntSliderConfig() { min = 0, max = 16 }));
      ModSettingsManager.AddOption(new IntSliderOption(LunarShardAdd, new IntSliderConfig() { min = 1, max = 5 }));
      ModSettingsManager.AddOption(new IntSliderOption(UltimateWaves, new IntSliderConfig() { min = 4, max = 18 }));
      ModSettingsManager.AddOption(new StepSliderOption(UltimateDuration, new StepSliderConfig() { min = 5, max = 10, increment = 0.25f }));
      ModSettingsManager.AddOption(new StepSliderOption(JumpRecast, new StepSliderConfig() { min = 0, max = 1, increment = 0.025f }));
      ModSettingsManager.AddOption(new StepSliderOption(JumpPause, new StepSliderConfig() { min = 0.2f, max = 3, increment = 0.1f }));
      ModSettingsManager.AddOption(new IntSliderOption(JumpWaveCount, new IntSliderConfig() { min = 12, max = 24 }));
      ModSettingsManager.AddOption(new StepSliderOption(ShardHoming, new StepSliderConfig() { min = 10f, max = 60, increment = 5f }));
      ModSettingsManager.AddOption(new StepSliderOption(ShardRange, new StepSliderConfig() { min = 80, max = 160, increment = 10f }));
      ModSettingsManager.AddOption(new StepSliderOption(ShardCone, new StepSliderConfig() { min = 90f, max = 180, increment = 10f }));

      ModSettingsManager.SetModDescription("Moon man with shadowws");
    }
  }
}
