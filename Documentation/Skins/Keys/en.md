# Skinning the Keys game mode (4K and 7K)
The following are the list of elements to customize the Keys game mode. This includes 4K and 7K.

**Note: All elements in this case must be in either a folder titled `4k` or `7k` depending on which game mode you wish the element to be for.**

## Column ##

`/4k/Column/column-lighting.png`

`/7k/Column/column-lighting.png`

![](img/Column/4K/bar-4k-column-lighting.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| No | MidCenter | - |

**Notes:**

- The sprite that is displayed that lights up the column when holding down the respective input key.

**skin.ini Values:**

| Name | Possible Values | Notes |
|:-:|:-:|:-:|
| ColumnLightingScale | Float | The y size scale of the column lighting.

## Lighting ##

`/4k/Lighting/hitlighting.png`

`/7k/Lighting/hitlighting.png`

![](img/Lighting/4K/bar-4k-hitlighting@1x8.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| Yes | MidCenter | - |

**Notes:**

- Spritesheet animation name for 4K: `/4k/Lighting/hitlighting@{rows}x{columns}.png`
- Spritesheet animation name for 7K: `/7k/Lighting/hitlighting@{rows}x{columns}.png`
- When hitting an object, an animation will play to give feedback to the user.
- If animation frames are given, it it is played at 180 FPS.
- If no animation frames are given, a default animation is played.

**skin.ini Values:**

| Name | Possible Values | Notes |
|:-:|:-:|:-:|
| HitLightingWidth | Integer | The width of the lighting |
| HitLightingHeight | Integer | The height of the lighting |
| HitLightingY | Integer | The y position of the lighting |

---

`/4k/Lighting/holdlighting.png`

`/7k/Lighting/holdlighting.png`

![](img/Lighting/4K/bar-4k-holdlighting@1x12.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| Yes | MidCenter | - |

**Notes:**

- Spritesheet animation name for 4K: `/4k/Lighting/holdlighting@{rows}x{columns}.png`
- Spritesheet animation name for 7K: `/7k/Lighting/holdlighting@{rows}x{columns}.png`
- When hold a long note, an animation will play to give feedback to the user.
- If animation frames are given, it it is played at 180 FPS.
- If no animation frames are given, a default animation is played.

**skin.ini Values:**

| Name | Possible Values | Notes |
|:-:|:-:|:-:|
| HitLightingWidth | Integer | The width of the lighting |
| HitLightingHeight | Integer | The height of the lighting |
| HitLightingY | Integer | The y position of the lighting |

## Notes ##

### HitObjects ###

### HoldHitObjects ###

### HoldBodies ###

### HoldEnds ###

## Receptors ##

`/{4k or 7k}/Receptors/Up/receptor-up-{1-7}.png`

![](img/Receptors/4K/Up/bar-4k-receptor-up-1.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| No | TopLeft | Square Image (256x256) |

**Notes:**

- The image displayed when the input key is not pressed.

**skin.ini Values:**

| Name | Possible Values | Notes |
|:-:|:-:|:-:|
| ReceptorPosOffsetY | Integer | The y position offset of the receptors relative to the bottom/top of the stage |
| ColumnSize | Integer | Increases the width of the receptors.
| ReceptorsOverHitObjects | True or False | If true, the receptors will be over the hitobjects when they fall down.

---

`/{4k or 7k}/Receptors/Down/receptor-down-{1-7}.png`

![](img/Receptors/4K/Down/bar-4k-receptor-down-1.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| No | TopLeft | Square Image (256x256) |

**Notes:**

- The image displayed when the input key is pressed.

**skin.ini Values:**

| Name | Possible Values | Notes |
|:-:|:-:|:-:|
| ReceptorPosOffsetY | Integer | The y position offset of the receptors relative to the bottom/top of the stage |
| ColumnSize | Integer | Increases the width of the receptors. |
| ReceptorsOverHitObjects | True or False | If true, the receptors will be over the hitobjects when they fall down. |
 
## Stage ##