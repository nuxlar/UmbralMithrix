# Umbral Mithrix

Phase info:

- Phase 1 Mithrix now jumps on you for a Crushing Leap, run. Bash, Slam, and Sprint spawn a Glass Clone on each survivor.
- Phase 2 (skippable) An umbral clone spawns in after Crushing Leap to hunt.
- Phase 3 The shadow splits into 2.
- Phase 4 Survival or Umbral Wurms (configurable)
  - Survival
    - Mithrix traps all players within the Prison Realm, anyone left outside is reduced to nothing within seconds
    - Mithrix has 2 new attacks, Gate Of Babylon and Might Of Babylon
    - Gate Of Babylon: Mithrix materializes a vast arsenal of weaponry, launching a storm of blades
    - Might Of Babylon: Mithrix manipulates a large area of the arena into a sea of blades
    - Mithrix summons random lower damage pizzas to track the players
  - Umbral Wurms
    - 1 Wurm has a devastating laser attack
    - The other launches an orb barrage
    - Basically scuffed Gilded Wurms from RoR1
- Phase 5 Congrats! look up and enjoy the fireworks

## How to Activate

There's an Obelisk where you first spawn on the moon. Activate the Obelisk if you want the challenge of Umbral Mithrix or don't for the vanilla Mithrix experience.

![moon obelisk](https://cdn.discordapp.com/attachments/1011187282788765816/1018116657077571595/unknown.png)

## Config Info

The mod is highly configurable. You can edit config values in-game and during runs through Settings -> Mod Options -> UmbralMithrix. Stats are calculated at the start of each phase so if you're blasting through Phase 1 you can make the other phases harder. If the in-game values are too limiting you can edit the modman config to input custom values.

![mod options](https://i.ibb.co/q9fC9jj/Screenshot-2022-09-12-165657.png)

## Future Plans
- Probably nothing OR
- Refactor code to be more organized
- Network the P4 visuals (wonky in MP)
- Rework P4 again
- Rework the Wurms

## Changelog

**1.8.8**

- Fixes the not-such-an-edge-case edge case of being near the walls and the fucker jumping off the moon

**1.8.7**

- Fixes the whole mod not working

**1.8.6**

- Fixes throne spawn not working after the first time
- Reworks Crushing Leap to stay on the ground instead of the current wonkiness and stop moving 1 second before the config value
- KNOWN ISSUE: Crushing Leap can make him jump out of the arena just dont sprint into the arena edges while he jumps
- Adds Risk of Options as a dependency, might be a weird issue, adding it just in case
- Deletes the scuffed P3 HP sharing

**1.8.5**

- Added unused throne spawn state
- Dependency updates
- Reduces Glass Mithrix's damage to half of Mithrix's damage (felt unfair for glass clones to have full damage)
- Added "new" arena (don't really care about vanilla anymore so the arena changes are always active)
  - More outer pillars
  - Outer arches
  - No inner arena boulders
  - No inner arena ramps

**1.8.3**

- R2API fun

**1.8.2**

- Fixes Phase 3 Voidling being invisible after Mithrix

**1.8.1**

- Fixing P4 infinity pizza (idk how it broke itself over the weekend)
- Reduces P3 pizza spawn range
- Reduces P4 Prison Realm radius
- Increased tracking duration for MightOfBabylon
- Updates README with P4 info

**1.8.0 (Cheers to Race and Cap for the Survival P4 idea and helping test/balance/brainstorm!)**

- Fixing some vanilla mithy bugs
- Changes Crushing Leap jump sound to be louder 
- Changes Crushing Leap indicator to be more clear when Mithy is about to land
- Reworks ItemSteal Phase 4
  - Survive...
  - Mithrix is immune with his HP dwindling (1 min)
  - Random P3 pizzas spawning
  - Mithrix creates a shrinking orb that forces players to get close or perish
  - Mithrix has 2 new attacks
    - Gate of Babylon: Mithrix materializes and launches his vast arsenal
    - Might of Babylon: Mithrix manipulates the arena creating a sea of blades

<details>
<summary>1.7.1 and below</summary>
<br>

**1.7.1**

- Fixes Wurm Laser radius

**1.7.0**

- Phase 4 Wurms!
- LaserWurm
  - Fires a devastating laser
  - Launches magma balls
  - Base CD of 24 secs (configurable)
- OrbWurm
  - Fires an orb wave attack (6 orbs) (configurable)
  - Launches lightening balls
  - Base CD of 12 secs (configurable)

**1.6.7**

- bugfix electric bugaloo

**1.6.6**

- bugfix

**1.6.5**

- Adds an alt Phase 4 for funsies
- Removes the old doppelganger phase 4
- The config is on by default, turn it off for the "Vanilla" item stealing Phase 4

**1.6.1**

- Fixes vanilla mithrix not being vanilla after activating umbral on a previous run
- Removed pizza lines from WeaponSlam
- Removed shockwave on SprintBash from clones (P2 Umbra and Phase 3 Umbras)
- Halves WeaponSlam orbs in P2 when clone spawns
- Extends tracking pizza's random range by 25

**1.6.0**

- **DELETE YOUR UMBRALMITHRIX CONFIG**
- Part of the big 2.0 update figured I should release in pieces or it'd take forever
- Several config changes
- Fixes Vanilla Mithrix not having vanilla dash
- Mithrix fires a super shard for every stack of freeze applied
- Glass clones are only in Phases 1 & 2
- A glass clone spawns on each player every 8 seconds mithrix sprints
- Reduces WeaponSlam glass clones from 2 to 1
- Adds 1.5 seconds to Ult duration for slower pizzas
- Adds Umbral Evolution
  - Mithrix seems to be normal but the umbral effect returns while a clone (glass or shadow) is present
  - By Phase 3 he is consumed by his shadow and becomes Umbral Mithrix, The Collective
- Phase 1
  - SprintBash releases a super shard
  - WeaponSlam releases orbs
- Phase 2
  - SprintBash releases a P3 WeaponSlam wave
  - WeaponSlam releases stationary pizza lines forwards
  - Crushing Leap spawns Mithrix's shadow to hunt
  - Pizza is under Mithrix (non-tracking)
- Phase 3
  - Clones spawn farther apart
  - Shared HP bar (BETA) (toggleable - off by default)
    - Whatever damage taken on 1 clone is replicated on the other
  - Pizza
    - Tracking near a random player
    - Each one has -2 lines
- Phase 4
  - Removes extra projectiles
  - Spawns a tracking pizza on FistSlam

**1.5.1**

- repenting for my transgressions
- and reverting 1.5.0

**1.5.0**

- **DELETE YOUR UMBRALMITHRIX CONFIG**
- Glass clones are only in Phases 1 & 2
- Glass clones spawn near each player(s) instead of near mithrix
- A glass clone is triggered every 8 seconds mithrix sprints
- Reduces WeaponSlam glass clones from 2 to 1
- Adds a super shard fire when frozen
- Phase 2 Changes
  - No more Lunar Devastation not even as a config
  - After Crushing Leap a clone spawns and does the Pizza
  - Pizza spawns a half wheel in a range near each player
- Phase 3 Changes
  - Pizza spawns a quarter wheel in a range near each player
- Phase 4 Changes
  - Half Pizza spawns near you on FistSlam
- Config Changes
  - Increases Pizza lines (2)
  - Increases Crushing Leap air time (0.5s)
  - Increases Base HP (100)
  - Increases Level Damage (0.25)
  - Increases Move Speed (1)
  - Increases Turn Speed (190)
  - Increases Acceleration (200)
  - Increases CD
    - WeaponSlam (1s)
    - SprintBash (0.5s)
    - Dash (0.5s)
  - Adds Loop Scaling (First "Loop" is still 0)
  - Adds Player Scaling


**1.4.4**

- Removes clone death animation/noise (sound cue for crushing leap for when things are chaotic)

**1.4.3**

- Fixes Vanilla pizza being 6 slices instead of 8

**1.4.2**

- Fixes bug where chimera insta-die before the fight

**1.4.1**

- Adds config for phase 2 clones after crushing leap (for extra insanity)
- Adds config for phase 2 Lunar Devastation (didn't think ppl liked this attack)

**1.4.0**

- DELETE YOUR UMBRALMITHRIX CONFIG IF UPDATING
- Adds config to toggle umbra effect
- Changes some config values (removes some, edits others)
  - reduces CrushingLeap by 1s
  - increased pizza waves by 1
  - reduces pizza duration to 6 secs
- Reduces Doppel Phase 4 Mithrix HP (No Loop 0, Loop 5x)
- "Reworks" Phase 2
  - Full Pizza (x2 the config)
  - Removed "new" pizza (shockwave)


**1.3.0**
 
- Fixes Blacklist not being applied for "Vanilla" Phase 4
- Adds large Flame Pillar and HammerSlam shockwave to Phase 2
- Halves the Flame Pillar size for Phase 3 
- Removes Phase 3 HammerSlam clones
- Adds config to skip Phase 2 for a more casual fight
- Removes Lunar Chimera from Phases 2 and 3 (in a hacky way)

**1.2.3**

- Fixed "Vanilla" phase 4 having an HP boost
- Doubles config value for phase 4 super shard CD (change to 4 if you're updating)
- Extends clone spawn distance so they can spawn at the edges of the arena
- Extends HammerSlam clone duration to match sprint bash (4s)
- Halves Phase 3 HammerSlam clones to 1
- Halves Phase 2 CrushingLeap clones' duration

**1.2.2**

- Replaces Skyleap with CrushingLeap
- Adds config for CrushingLeap aim duration (configurable mid fight)

**1.2.1**

- Changes README
- Switches Phase 2 Ult wave to the Hammer Slam wave
- Adds SuperShardWeight to config (how many shards are in 1 super shard)
- Adds Phase 2 Ult Super Shard Interval to config (how often it fires)
- Reduces default Super Shard Weight from 12 -> 6 so they're not an insta kill
- Added Github link to page

**1.2.0**

- Makes Phase 4 Vengeance event a config option
- Replaced mountain shrine with obelisk next to where you spawn on the moon
- Removed blink (messes with hammerslam)
- Adds extra clone to HammerSlam
- Adds Super Shards to Phase 2 Ult

**1.1.3**

- Fixes vanilla mithrix not working after hitting the shrine on a previous run (both regular and phase 4)
- Switches Imp Blink with Huntress MiniBlink since u can cheese him by standing at the edge of the map
- Reduces Phase 4 HP buff (3x instead of 5x)
- Updates Vanilla description values in config
- Removes damage config affecting phase 4 mithrix

**1.1.2**

- Fixes umbras getting blacklist items
- Fixes dios making Mithrix invulnerable for 20 secs

**1.1.1**

- Makes README more clear
- Reduces Phase 3 HP percentage
- Makes new item for Umbra-fication so it doesn't conflict with other vengeance mods (Cheers to Moffein)
- Fixes Phase 4 buggin out sometimes
- Fixes UmbralMithrix staying activated after hitting the moon shrine

**1.1.0**

- **DELETE YOUR UMBRALMITHRIX CONFIG**
- Fixes README
- Adds Mountain Shrine on the moon to activate Umbral Mithrix (Cheers to Race and Cap)
- Replaces Mithrix's dash with an extended imp dash (can also dash in mid air now)
- Removes Projectiles from Bash, Slam (Keeps orbs), Dash
- Adds deteriorating glass clone to attacks, Slam and Bash (last 4 seconds)
- Glass Clones release a super shard when they Bash
- Fixes Phase 2 HP being too low
- Adds 2 glass clones on Phase 2 skyleap
- Blacklists some items from Phase 4 doppels (Spare Drone Parts, Empathy Cores, N'kuhana's Opinion, Razorwire, Tesla Coil)
- Removes faster escape flame lines for less lag (in case using voidling escape or something)

**1.0.1**

- 1 Glass Mithrix's for Phase 2 (66% HP Each Mithrix) (if > 2 players 1 more spawns and each has 100% HP)
- Changes extra Mithrix Phase 3 HP (66% HP Each) (if > 2 players 100% Each)
- Reduces Phase 4 invulnerability time to 20 sec base
- Reduces Phase 4 steal time to 0.75 sec

**1.0.0**

- Reduces some config stats to account for the clones
- Makes Mithrix an Umbra
- Adds deteriorating clone on skyleap for phase 1 and 3 (if > 2 players)
- Adds 2 Glass Mithrix's for Phase 2 (33% HP Each Mithrix) (if > 2 players 2 more spawn for a total of 5)
- Adds 1 Mithrix (2 total) for Phase 3 (66% HP Each)
- Changes dash and bash sound
- Reworks Phase 4
  - Mithrix's Shadow is invulnerable and immobile for 30 secs (less time based on your loops)
  - The Shadow has 5x HP (10x if looping)
  - Umbras of all players spawn at the center of the arena
  - The Shadow will start stealing 1 stack of items every 1 seconds after it's shield falls (15 sec max - decreased based on loops)
</details>