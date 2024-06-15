### Rng Fix

*v0.9 Oni sampling methods*

Vanilla seeding is a clusterfuck. Encounters are path dependent, past choices affect future rolls, battle rngs are not isolated. Needless to say using the same seed will result in very different experience.

*Mod is still in early release so reports of inconsistencies in reruns or same seed co-op runs are greatly appreciated*

This mod roughly tries to ensure that n'th node of the same type will have the same rng state no matter where or when encountered. I.e. first 1-2 regular battle will always be the fixed for the same seed, including deck shuffle state and random enemy moves. 

What this fixes:
- Pathing no longer affects encounters
- Isolates battle and adventure rngs. Card discovery, shuffles etc. will not affect future encounters.
- More consistent adventure queue.
- Rare card rewards are unaffected by amount card rewards seen previously.
- Consistent money rewards.
- Random boss select is fixed by seed.
- Much other rng dependence on player actions removed.


What this does **not** fix:
- Card rewards are wildly affected by player's manabase (same state, different pool issue)
- Weight shifts in adventure and exhibit pools sometimes produce completely different outcomes.


---
*Change log*

`0.9.0` Partly addresses the issues the mod hasn't addressed before:
* Exhibits seen should be very consistent. 
* Elites have separate rng for card rewards.
* Experimental encountered event queue.

`0.8.2` Fix bug when restarting on battle reward screen.

`0.8.1` Make supply station use separate rng for rolling exhibit.

`0.8.0` Initial release.
