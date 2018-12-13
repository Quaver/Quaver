# Configuring skin.ini
Sometimes the default configuration for skins may not be good enough and you want to adjust them. By making a `skin.ini` file in the root directory of the skin folder, you are allowed to manipulate some of the values to change the look of the game.

## General ##

The `[General]` section of the config file contains metadata about the skin - who it's by, what it's called, and the version. It's mainly to let people know what your skin is all about.

| Value | Data Type | Notes |
|:-:|:-:|:-:|
| Name | String | The name of the skin |
| Author | String | The creator/author of the skin |
| Version | String | The version number of the skin |

## Keys ##

This section is to manipulate the elements for the Keys game mode including 4K and 7K.

* To start manipulating elements for 4K, create a section in the file titled `[4K]` and have all of your config properties under it.
* To start manipulating elements for 7K, create a section in the file titled `[7K]` and have all of your config properties under it.

| Value | Data Type | Notes |
|:-:|:-:|:-:|
| BgMaskPadding | Integer | ? |
| BgMaskAlpha | Float (0.0-1.0) | The alpha channel/opacity of the [Stage Bg Mask]() |
| HitPosOffsetY | Integer | The offset of the hit position relative to its default location at the edge of the receptors |
| NotePadding | Integer | ? |
| ColumnLightingScale | Float | The height scale of the column lighting to make it bigger or smaller.
| ColumnSize | Integer | The size of each column. Equal size for each column |
| ReceptorPosOffsetY | Integer | The y position of the receptors relative to its default location on the screen |
| ColumnAlignment | Integer (Percentage) | A percentage value of the width of the screen where the stage will be placed |
| ColorObjectsBySnapDistance | Boolean (True or False) | If true, it will look for file names relative to snap distance. See the [HitObjects]() section for more information |
| JudgementHitBurstScale | Float | The scale of the judgement hit bursts to change the size of it |
| ReceptorsOverHitObjects | Boolean (True or False) | If true, the receptors will be over the hitobjects when they fall down and vice versa |
| ColumnColor{1-7} | RGB Color (255,255,255) | The color in which the [Column Lighting]() is tinted in the specified lane |
| FlipNoteImagesOnUpscroll | Boolean (True or False) | If true, the notes will be flipped upside down if upscroll is enabled |
| FlipNoteEndImagesOnUpscroll | Boolean (True or False) | If true, the notes's ends will be flipped upside down if upscroll is enabled |
| HitLightingY | Integer | The Y position of the hit/holdlighting relative to its default position |
| HitLightingWidth | Integer | The width of the hit/holdlighting |
| HitLightingHeight | Integer | The height of the hit/holdlighting |
| ScoreDisplayPosX | Integer | The X position of the score display relative to its default position |
| ScoreDisplayPosY | Integer | The Y position of the score display relative to its default position |
| AccuracyDisplayPosX | Integer | The X position of the accuracy display relative to its default position |
| AccuracyDisplayPosY | Integer | The Y position of the accuracy display relative to its default position |
| KpsDisplayPosX | Integer | The X position of the keys per second display relative to its default position |
| KpsDisplayPosY | Integer | The Y position of the keys per second display relative to its default position |
| ComboPosY | Integer | The Y position of the combo display relative to its default position |
| JudgementBurstPosY | Integer | The Y position of the judgement burst relative to the middle of the screen |
| HealthBarType | `Horizontal` or `Vertical` | The type of health bar your image is. If it is horizontal, specify horizontal and vice versa |
| HealthBarKeysAlignment | `LeftStage`, `RightStage` or `TopLeft` | Where the health bar is positioned in relation to the stage |