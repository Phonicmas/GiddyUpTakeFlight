# Giddy-Up 2: Take Flight

An addon for [Giddy-Up 2 - Continued](https://steamcommunity.com/sharedfiles/filedetails/?id=3674332861) that lets flying mounts actually fly.

RimWorld 1.7. Safe to add to an existing save.

## Requirements

- [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)
- [Giddy-Up 2 - Continued](https://steamcommunity.com/sharedfiles/filedetails/?id=3674332861)

Load it after both.

## Features

### Flying over terrain

- Riders on a flying mount ignore terrain movement costs. Marsh, mud, sand and shallow water cost nothing extra.
- They can cross deep water. Snow still slows them, and walls, buildings and closed doors still block them.

### Take flight

- A rider on a flying mount gets a **Take flight** button. Pick a spot within range and the mount carries its rider there through the air, over walls and water.
- Afterwards the mount needs to rest for a while before it can take flight again.
- By default both the take-off and landing spot need open sky.

### World flight

- Riders on flying mounts, or a caravan with them, can fly to another tile on the world map.
- The destination options are the same as transport pods: form a caravan, land on one of your maps, visit or attack a settlement, visit a site.
- Every flying animal has a base range in tiles. Lighter loads fly further: at half the mount's carrying capacity the range doubles, up to ×3 by default.
- A mount carrying more than its carrying capacity can't take flight at all.
- After a world flight the mount rests for a while, longer the further it flew.
- Riders who can't fly stay behind as their own caravan.

#### How range works

Load is the rider's mass (including worn gear and inventory) plus anything in the mount's own inventory. Capacity is the mount's carrying capacity.

Range = base range × (capacity ÷ load), never below ×1 and capped at the max multiplier.

Example: a mount with 300 kg capacity and a base range of 8 tiles.

| Load | Multiplier | Range |
|---|---|---|
| 300 kg (100%) | ×1.0 | 8 tiles |
| 200 kg (67%) | ×1.5 | 12 tiles |
| 150 kg (50%) | ×2.0 | 16 tiles |
| 100 kg or less (≤ 33%) | ×3.0 (cap) | 24 tiles |
| 320 kg (107%) | can't fly | – |

A group flies as far as its shortest range.

## Which animals fly

- Animals that can fly in vanilla (the flight system from Odyssey).
- Floating creatures from Vanilla Expanded Framework, such as the Alpha Animals floaters and the Dark Ages phrak.
- Built-in support for these mods (only applied when the mod is active):

| Mod | Animals | Base world range |
|---|---|---|
| Dragons Descent | Common dragons | 10 tiles |
| | Gold, silver and jade dragons | 14 tiles |
| | True dragon | 20 tiles |
| Dragon Genetics | Aviagon | 10 tiles |
| | Dracowolf | 6 tiles |
| | Dragonrat | default |
| Mythic Ages: Megafauna Bestiary | Harpeagle | 8 tiles |

Any mountable animal can be switched on or off in the mod settings.

## Settings

| Setting | Default |
|---|---|
| Flying mounts ignore terrain | On |
| Take flight | On |
| Require open sky | On |
| Take flight range | 30 cells |
| Take flight speed | 18 cells per second |
| Rest after a flight | 2 hours |
| World flight | On |
| Default world range | 5 tiles |
| Max range multiplier for light loads | ×3 |
| World flight speed | 120 tiles per day |
| Rest per tile flown | 1 hour |
| Mountable animals that can fly | per animal |

## For modders

Mark an animal as a flying mount with the `GiddyUpTakeFlight.FlyingMount` mod extension. Every field is optional; leave one out to use the mod setting.

### Directly on your animal's ThingDef

```xml
<ThingDef ParentName="AnimalThingBase">
  <defName>MyGriffin</defName>
  ...
  <modExtensions>
    <li Class="GiddyUpTakeFlight.FlyingMount">
      <canFly>true</canFly>                       <!-- false stops an auto-detected animal from flying -->
      <rangeFactor>1.2</rangeFactor>              <!-- multiplies the Take flight range on the map -->
      <cooldownFactor>0.8</cooldownFactor>        <!-- multiplies the rest time after a flight -->
      <worldFlightRange>12</worldFlightRange>     <!-- world map range in tiles at full load, 0 = no world flight -->
      <maxRangeMultiplier>3</maxRangeMultiplier>  <!-- cap on the light-load range bonus -->
      <worldFlightSpeed>120</worldFlightSpeed>    <!-- tiles per day -->
      <rangeByLoad>                               <!-- replaces the capacity / load formula. x = load / capacity, y = range multiplier -->
        <points>
          <li>(0.25, 3)</li>
          <li>(1, 1)</li>
        </points>
      </rangeByLoad>
    </li>
  </modExtensions>
</ThingDef>
```

### Patching another mod's animal

No dependency needed. The patch only applies if the animal exists.

```xml
<Patch>
  <Operation Class="PatchOperationAddModExtension">
    <success>Always</success>
    <xpath>Defs/ThingDef[defName="SomeModGriffin"]</xpath>
    <value>
      <li Class="GiddyUpTakeFlight.FlyingMount">
        <worldFlightRange>10</worldFlightRange>
      </li>
    </value>
  </Operation>
</Patch>
```

## AI disclosure

The preview image was made with generative AI.

## Credits

- Giddy-Up 2 by Roolo, Owlchemist, dav9670 and Meme Goddess.
- XML and C# by Phonicmas.
