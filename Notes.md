## Update Log 02/12/2024
- Collision makes use of Sphere check all the time which lead to agents clipping into other boxy objects, like walls. Updated `CheckIfPositionIsOpen` function to include Box checks for boxy game objects, which now spawns agents without clipping them into walls.
- SphereF and SphereM spawn in a way that clip the environment and each other. Are they tagged appropriately when they're created? Do they have colliders when they are created? Are collisions detected for each new object at once and then they spawn, or for each new object to create, we calculate the collision, spawn it, and repeat?
  - Fixed Spheres overlapping each other, but they still sometimes get spawned fully in walls, partially clipping walls, or partially clipping agents.
- Checked 24 messages of reaching 20 max attempts correspond with 24 spheres overlapping one another. The smaller balls also overlap, but they do not give out a Debug.Log message, indicating that their overlap check is either incorrect, or they are initialised in an invalid way. 
- Wall clipping
  - The simulation is instantiated with walls already clipping other walls. During the simulation, agents move walls in a way that clips them as well. This might be intentional since the selection of wall models was arbitrary and there might be a desire to be able to create shapes by aligning walls that were not pre-made with architectural cohesiveness or other similar constraints in mind. If the eventual aim is to restrict the ability to place a wall in a way that condones proper architectural principles, changes have to be made in the way in which agents can select the place where to put a wall.
    - FIXED: Added an exception to include positions that intersect the floor as the floor was flagging any object spawned as invalid since the floor also has a collider. With this, props no longer clip the walls. 
    - TODO: Decide what to do with objects that fail to be spawned in the max number of attempts. Right now, nothing happens to them, and so that is why they were "getting spawned incorrectly"; their original location was incorrect, the CheckIfOpen function was always returning false cause it intersected the floor, and after max attempts was reached, nothing happened to the object.
  - Currently, agents are capable of placing walls on top of themselves, rendering them invisible and "cheating" at the game.
- MaxAttempts reached for agents, though they seemingly aren't intersecting anything
- Female agents use `Castle Agent` script while Male agents use `Male Agent V2` script.

## Update Log 03/12/2024
### TODO 
- Run sim on Linux server DONE
- Fix wall placement collision
  - Agents ignore the collision when carrying objects
- Refactor code such that Female agents use the same functions/class
- What to do with objects that fail to spawn correctly?


## Update Log 09/12/2024
### Installing mlagents==1.1.0
- `conda activate castles`
- `conda install python=3.10.12`
- `pip install -r requirements.txt`

At this point, you might have issues with loading a DLL file when trying to run `mlagents-learn`
```bash
    raise err
OSError: [WinError 126] The specified module could not be found. Error loading "C:\Users\Ubervelocity\anaconda3\envs\mlagents\lib\site-packages\torch\lib\torch_python.dll" or one of its dependencies.
```

This only happens on Windows 11 apparently. To solve it, you need to reinstall torch
- `pip uninstall torch`
- `pip install torch`

Do the same reinstallation process for any library that fails to load.