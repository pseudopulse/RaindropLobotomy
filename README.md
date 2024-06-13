# Raindrop Lobotomy

Adds content from Lobotomy Corporation.

## Additions

Survivors:
- A Sweeper

Survivor Variants:
- Bandit :: Magic Bullet
- Void Fiend :: Mimicry
- Blade Lineage Mercenary

Items:
- Clockwork Spring (Steam Machine ego gift)

Enemies:
- Fragment of the Universe
- Steam Transport Machine
- Singing Machine

Skills:
- Yield My Flesh (Mercenary Special)
- Solemn Lament (Commando Primary)

Ordeals:
- Process of Understanding (Noon)
- Helix of the End (Midnight)
- The Sweepers (Noon)

Ordeals will appear after spending too long on the same stage. Can be configured.

![Blade Lineage Mercenary](https://i.postimg.cc/hjpC10Hd/07-41-03-screenshot.png)
![Sweeper](https://i.postimg.cc/7Lkn99s5/12-02-12-screenshot.png)
![Mimicry](https://i.postimg.cc/fWr7pDqn/12-02-05-screenshot.png)
![Magic Bullet](https://i.postimg.cc/hv69d1QP/12-02-10-screenshot.png)

## Changelog
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

## Credits
pseudopulse - programming, models, animations

smxrez - Solemn Lament gun replacement model, playtesting

monsterskinman - playtesting, ideas