# Skinning the Keys game mode (4K and 7K)
The following are the list of elements to customize the Keys game mode. This includes 4K and 7K.

**Note: All elements in this case must be in either a folder titled `4k` or `7k` depending on which game mode you wish the element to be for.**

## Column ##

`/{4k or 7k}/Column/column-lighting.png`

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

`/{4k or 7k}/Lighting/hitlighting.png`

![](img/Column/4K/bar-4k-hitlighting@1x8.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| Yes | MidCenter | - |

**Notes:**

- Spritesheet animation name: `/{4k or 7k}/Lighting/hitlighting@{rows}x{columns}.png`
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

`/{4k or 7k}/Lighting/holdlighting.png`

![](img/Column/4K/bar-4k-holdlighting@1x12.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| Yes | MidCenter | - |

**Notes:**

- Spritesheet animation name: `/{4k or 7k}/Lighting/holdlighting@{rows}x{columns}.png`
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

## Stage ##