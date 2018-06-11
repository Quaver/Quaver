# Skinning the Gameplay Interface
The gameplay interface includes general skin elements that are present in all game modes. It includes elements such as number displays, scoreboards, judgements,
and more.

## Cursor ##

`/Cursor/main-cursor.png`

![](img/Cursor/main-cursor.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| No | MidCenter | None |

**Notes:**

- The mouse cursor that is displayed.
- Hidden during gameplay. Active during menu navigation.

**skin.ini Values:**

- None

## Grades ##

`/Grades/grade-small-a.png`

![](img/Grades/grade-small-a.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| No | - | None |

**Notes:**

- The sprite that is displayed next to the accuracy when having 90-94%.
- Displayed in the results screen.
- Displayed in the song select screen leaderboards.

**skin.ini Values:**

- None

---

`/Grades/grade-small-b.png`

![](img/Grades/grade-small-b.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| No | - | None |

**Notes:**

- The sprite that is displayed next to the accuracy when having 80-89%.
- Displayed in the results screen.
- Displayed in the song select screen leaderboards.

**skin.ini Values:**

- None

---

`/Grades/grade-small-c.png`

![](img/Grades/grade-small-c.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| No | - | None |

**Notes:**

- The sprite that is displayed next to the accuracy when having 70-79%.
- Displayed in the results screen.
- Displayed in the song select screen leaderboards.

**skin.ini Values:**

- None

---

`/Grades/grade-small-d.png`

![](img/Grades/grade-small-d.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| No | - | None |

**Notes:**

- The sprite that is displayed next to the accuracy when having less than 70%.
- Displayed in the results screen.
- Displayed in the song select screen leaderboards.

**skin.ini Values:**

- None

---

`/Grades/grade-small-f.png`

![](img/Grades/grade-small-f.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| No | - | None |

**Notes:**

- The sprite that is displayed next to the accuracy when failing a map.
- Displayed in the results screen.
- Displayed in the song select screen leaderboards.

**skin.ini Values:**

- None

---

`/Grades/grade-small-s.png`

![](img/Grades/grade-small-s.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| No | - | None |

**Notes:**

- The sprite that is displayed next to the accuracy when having 95-98%.
- Displayed in the results screen.
- Displayed in the song select screen leaderboards.

**skin.ini Values:**

- None

---

`/Grades/grade-small-ss.png`

![](img/Grades/grade-small-ss.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| No | - | None |

**Notes:**

- The sprite that is displayed next to the accuracy when having 99%.
- Displayed in the results screen.
- Displayed in the song select screen leaderboards.

**skin.ini Values:**

- None

---

`/Grades/grade-small-x.png`

![](img/Grades/grade-small-x.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| No | - | None |

**Notes:**

- The sprite that is displayed next to the accuracy when having 100% while having perfect judgements.
- Displayed in the results screen.
- Displayed in the song select screen leaderboards.

**skin.ini Values:**

- None

---

`/Grades/grade-small-xx.png`

![](img/Grades/grade-small-xx.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| No | - | None |

**Notes:**

- The sprite that is displayed next to the accuracy when having 100% with all marv judgements.
- Displayed in the results screen.
- Displayed in the song select screen leaderboards.

**skin.ini Values:**

- None

---


## Health Bar ##

`/Health/health-background.png`

![](img/Health/health-background.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| Yes | Depends | 600x40 if horizontal. 40x600 if vertical |

**Notes:**

- Spritesheet animation file name: `/Health/health-background@{rows}x{columns}.png`
- Displayed as the background health bar. This one does not change and only serves as the background of the health gauge.

**skin.ini Values:**

| Name | Possible Values | Notes |
|:-:|:-:|:-:|
| HealthBarType | `Horizontal` or `Vertical` | If your image is horizontal, set it to horizontal. If vertical, set it to vertical. |
| HealthBarKeysAlignment | `RightStage`, `LeftStage`, or `TopLeft` | Determines where to place the health bar in the Keys game mode.

---

`/Health/health-foreground.png`

![](img/Health/health-foreground.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| Yes | Depends | 600x40 if horizontal. 40x600 if vertical |

**Notes:**

- Spritesheet animation file name: `/Health/health-foreground@{rows}x{columns}.png`
- Displayed in the foreground. This is the healthbar that moves according to the current health.

**skin.ini Values:**

| Name | Possible Values | Notes |
|:-:|:-:|:-:|
| HealthBarType | `Horizontal` or `Vertical` | If your image is horizontal, set it to horizontal. If vertical, set it to vertical. |
| HealthBarKeysAlignment | `RightStage`, `LeftStage`, or `TopLeft` | Determines where to place the health bar in the Keys game mode.

## Judgements ##

`/Judgements/judgement-overlay.png`

![](img/Judgements/judgement-overlay.png)

| Animatable | Alignment | Suggested Size |
|:-:|:-:|:-:|
| No | MidRight | 100x100 |

**Notes:**

- Background image that displays the current judgements the player has.
- Automatically colored according to the [JudgeColors skin.ini value]()
- Should be white!

**skin.ini Values:**

| Name | Possible Values | Notes |
|:-:|:-:|:-:|
| JudgeColor{Marv-Miss} | RGB Color (255,255,255) | The overlay is tinted according to these skin.ini values. 

## Numbers ##

## Pause Screen ##

## Scoreboard ##





