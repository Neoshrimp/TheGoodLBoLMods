### Rng Fix

*v2.0.0 Final major release(?)*

*v1.1.0 Limitation removed*

*v1.0.0 big changes, no time to log*

*v0.9 Oni sampling methods*

Vanilla seeding is a clusterfuck. Encounters are path dependent, past choices affect future rolls, battle rngs are not isolated. Needless to say using the same seed will result in very different experience.


~~This mod roughly tries to ensure that n'th node of the same type will have the same rng state no matter where or when encountered. I.e. first 1-2 regular battle will always be the fixed for the same seed, including deck shuffle state and random enemy moves.~~

Probably the most algorithmically involved mod I've ever coded. Blog post about core ideas which make it work soon (perhaps).

What this fixes:
- Pathing no longer affects encounters
- Isolates battle and adventure rngs. Card discovery, shuffles etc. will not affect future encounters.
- More consistent adventure queue.
- Rare card rewards are unaffected by amount card rewards seen previously.
- Consistent money rewards.
- Random boss select is fixed by seed.
- Much other rng dependence on player actions removed.


What this does **not** fix:
- Should fix everything **vanilla**.
- Modded entities will be assigned entity unique rngs when using vanilla getters. 
- Ensuring consistent sampling method is up to the modder, however.


---
*Change log*

`2.2.3` Fix some unintended enemy behavior changes, like all kedamas attacking on turn 1.

`2.2.1` Fix rng state not being properly set for first adventures.

`2.2.0` More consistent Shining Exhibit sampling, some fixes.

`2.1.0` 
- Make card and other entity sampling method fairer to the weights.
- Fix some exhibits not having rng assigned. Requires starting a new run for fix to take effect.
- Change logs location to `rngFix_logs/` in LBoL.exe folder.
- Minor fallback and consistency upgrades.

`2.0.0` Greatly reduces manipulation possible via save/loading:
- Consistent deck shuffling, independent from discard order.
- Consistent card discovery, independent from play order.
- Consistent random targeting.
- Somewhat consistent random insertion into the draw pile.
- Consistent enemy moves, damage, P and score drops.
- Other miscellaneous improvements and fixes.

`1.1.2` Logging softlock fix.

`1.1.1` Fix softlock when restarting on reward screen. Fix no samples being found when majority of weights are very low (<0.01). Separate card logs for rewards and discovery.

`1.1.0` The majority out of battle rng inconsistencies were dealt with.
- Decouple total potential pool from affecting rolls. Some absolute number constraints were introduced as the consequence (more details to follow).
- Fix heavy bias towards higher weights.
- (Experimental) Elite card rewards use regular cardRng queue instead of separate one.
- Extra card rewards from Blank card and Omikuji are using separate rng. Cheers, zekses.


`1.0.1` Main branch debut hardlock bug fix.

`1.0.0` Big update:
* Consistent card rewards even with different mana bases.
* Event queue which makes sense.
* Logging. Logs run information at `%APPDATA%\..\LocalLow\AliothStudio\LBoL\RngFix` Enabled by default for this Seed of the Week cycle.
* Other fixes and improvements.

`0.9.0` Partly addresses the issues the mod hasn't addressed before:
* Exhibits seen should be very consistent. 
* Elites have separate rng for card rewards.
* Experimental encountered event queue.

`0.8.2` Fix bug when restarting on battle reward screen.

`0.8.1` Make supply station use separate rng for rolling exhibit.

`0.8.0` Initial release.
