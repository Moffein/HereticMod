## Heretic

Adds the Heretic as a selectable survivor with stats balanced around the base cast.

Grab all 4 Heresy items in a run to power up and acquire the Mark of Heresy!

We are looking for translators! Check the languages folder on the [GitHub](https://github.com/Moffein/HereticMod/tree/master/language) if you would like to submit a translation for your language.
Current Supported Languages: English, Spanish, Simplified Chinese, Russian

[![](https://i.imgur.com/zX96kAC.jpg)]()
[![](https://i.imgur.com/INtd4VY.png)]()

Squawk button can be assigned in the in-game options menu!

## Changes from Vanilla

*Note: The first stack of all Heresy items will have no effect on the Heretic, as they are replacing her existing skills. This behavior was chosen for the sake of internal consistency (otherwise, there'd need to be a bunch of different cases for how Heresy items work if you spawn as Heretic or if you pick them up and become the Heretic mid-run).*

- Base Stats
	- HP reduced from 440 -> 110
	- Damage reduced from 18 -> 12
	- HP Regen increased from -6 -> 1

- Visions of Heresy's reload scales with Attack Speed instead of Cooldown.
	- Can be toggled in the config.
	- If RiskyMod is installed, the Visions changes from this mod will take precedence.
	
- Mark of Heresy (New Item)
	- Automatically granted when all 4 Heresy items are collected up in a single run.
		- Cannot be cleansed, printed, stolen, or removed.
		- Happens at the same time as the Heretic transformation.
	- Increases HP 4x
	- Increases damage by 50%
	- Reduces your base regen all the way to -6HP/s (value used for Rainstorm) regardless of difficulty.
		- Unaffected by Tonic as well.

## Installation

Drop the Moffein-Heretic folder into \BepInEx\plugins\

## Credits

Code - Moffein

Spanish Translation - Anreol

Simplified Chinese Translation - WockyTheWolf, JunJun_w

Russian Translation - Drochuagon, Lecarde

French Translation - HolographicWings

German Translation - juppytaar

Portuguese Translation - Kauzok