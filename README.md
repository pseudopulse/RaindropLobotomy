# Raindrop Lobotomy

Adds content from Lobotomy Corporation.

## Additions

Survivors:
- A Sweeper

Survivor Variants:
- Bandit :: Magic Bullet
- Void Fiend :: Mimicry
- Blade Lineage Mercenary
- Commando :: Solemn Lament
- MULT :: Grinder MK. 5-2
- False Son :: Justitia

Items:
- Clockwork Spring (Steam Machine ego gift)

Enemies:
- Fragment of the Universe
- Steam Transport Machine
- Singing Machine
- 1.76 MHz

Skills:
- Yield My Flesh (Mercenary Special)
- Solemn Lament (Commando Primary)

Ordeals:
- Process of Understanding (Noon)
- Helix of the End (Midnight)
- The Sweepers (Noon)

Ordeals will appear after spending too long on the same stage. Can be configured.

## Content Screenshots
<details><summary>Survivor Previews</summary>

![Justitia](https://i.postimg.cc/dtpdGSdP/06-21-17-screenshot.png)
![Grinder MK52](https://i.postimg.cc/sXLKt4Yh/12-41-32-screenshot.png)
![Solemn Lament](https://i.postimg.cc/sgVDPJS3/03-51-41-screenshot.png)
![Blade Lineage Mercenary](https://i.postimg.cc/hjpC10Hd/07-41-03-screenshot.png)
![Sweeper](https://i.postimg.cc/7Lkn99s5/12-02-12-screenshot.png)
![Mimicry](https://i.postimg.cc/fWr7pDqn/12-02-05-screenshot.png)
![Magic Bullet](https://i.postimg.cc/hv69d1QP/12-02-10-screenshot.png)

</details>

## Credits
pseudopulse - programming, models, animations

smxrez - Solemn Lament gun replacement model, playtesting

monsterskinman - playtesting, ideas

project moon - making the amazing series where all of the visual designs, sounds, and sprites here come from

## Changelog
# 1.5.0
- survivor variant: justitia false son
- fixed custom sfx not working (it broke in-between me working on the 1.4.4 fix and the dlc bugfix patch releasing)

# 1.4.4
- updated for SOTS

# 1.4.3
- having stacks of Charge grants +3.5% movement speed per stack
- you can no longer re-apply seal to an enemy who already has seal
- the duration of seal is now half as long on bosses
- To Claim Their Bones now checks if it would fling you out of the map and downslams instead
- Rest now only does an upward launch if you cancel it via jump instead of shift
- Acupuncture now casts in 0.25s instead of 0.5s, distance traveled remains the same (faster more responsive dash)
# 1.4.2
- now no longer spawns an AbnormalityDirector if no abnos are enabled, preventing shitloads of error spam
# 1.4.1
- grinder mk5-2 now does an upwards leap after leaving rest mode
- landing a hit on Terminate now halts your fall
- slowed down the static of 1.76 MHz
- made the static of 1.76 MHz grayscale
- made Disable Limiter not target destroyable props like barrels or sulfur pods
# 1.4.0
- survivor variant: grinder mk5-2 mult
- enemy: 1.76 MHz
- added configs for enabling/disabling enemies
# 1.3.2
- buffed magic bullet damage coefficients
- bullet of despair now fires to the cursor
- flooding bullets gives more armor
- added a config option to replace solemn lament's ding sound with the crit sound effect
- added a config option to replace magic bullet's gunshot and portal open with vanilla sounds
- firing magic bullet now only consumes ammo if it auto-aimed onto an enemy (manually aimed shots dont count towards the 7)
- made configs generate on mod load instead of the first time they get accessed
- readme update 2: electric boogaloo
# 1.3.1
- updated desc
# 1.3.0
- added new survivor variant: solemn lament commando
- fixed bl merc throwing a nullref when a forgotten relics lightning rod landed a crit (so true??)
# 1.2.3
- fixed Unrelenting and HiddenInvincibility granted by Yield My Flesh / To Claim Their Bones being really inconsistent (sometimes not being granted, sometimes never getting removed)
# 1.2.2
- Acupuncture now dashes to where you're aiming instead of directly forwards
# 1.2.1
- fixed BL merc shitting nullrefs every frame if a living body had no inventory assigned
- undocumented from 1.2.0: blind vermin can no longer be shelled
# 1.2.0
- added new survivor variant: blade lineage mercenary
- reduced the base health of Process of Understanding to 740
# 1.1.4
- we love ability stealing
# 1.1.3
- shelling an enemy with Wear Shell or Goodbye now incurs a 2 second period where you cannot shell an enemy weaker than your current shell. this ensures you will always shell the strongest available during these skills.
# 1.1.2
- most stuff is networked now
- sweeper's Scatter Fuel now has backwards recoil and no longer locks movement (can be configured off)
- ally sweeper names are longer
- g?ood?bye no longer breaks on kill occassionally
- g?ood?bye properly turns enemies into a fine, red.. uhhh... cloud..?
- magic bullet bandit now has proper footsteps and ragdolls on death
- magic bullet bandit's ignition hitbox is now identical to regular bandit's slash
- mimicry now ragdolls on death
# 1.1.1
- fixed mithrix breaking sometimes
- fixed ordeals making the hud invisible in multiplayer
- changed how ordeals are picked, now preset for the 5 stages in a loop instead of repeating midnight after s5
- moved ordeals to the objectives tab (they are not required to leave the stage!!)
# 1.1.0
- fixed Beetle Guards sunder being shelled when it shouldnt (it doesnt work)
- fixed bug where equipping merc's beheaded skin after previously equipping solemn lament would break rendering for the rest of the session
- made flooding bullets have 3 charges and consume 1 per shot
- flooding bullets can now be canceled early, only consuming charges equiv to 1 shot
- lowered sfx volume across the board
- made sfx properly follow the in-game sound volume slider
# 1.0.4
- fixed silent advance creating an infinitely looping sound effect
- fixed mb bandit skill descs
- fixed mimicry specials canceling themselves
- fixed mimicry not prioritizing specific shell skills when stealing
- blacklisted alot of modded enemies who dont have functional skills for mimicry to steal
- bullet of despair does less self damage (same damage to enemies)
- h?ell?o has standard falloff instead of buckshot now
- flooding bullets now grants an armor boost during the state
# 1.0.3
- more bugfixes wooooo!!!!!1
# 1.0.2
- fixed a bug with weapon replacements that broke all skins
# 1.0.1
- readme update
# 1.0.0
- release