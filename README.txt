README - DoomRoom
#######
Copyright (C) 2012 Eddie Cameron

Released under WTFPL:
DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
                   Version 2, December 2004

Everyone is permitted to copy and distribute verbatim or modified
copies of this license document, and changing it is allowed as long
as the name is changed.

           DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
  TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION

 0. You just DO WHAT THE FUCK YOU WANT TO.
#######

Doom Room:
A basic Doom-like level generator. Has Unity3D in mind, but the meat of the program (CellPathfinder.cs and CellMaster.cs) should be easily transferred to other engines/languages.

- Cell.cs : A simple data structure for a cell in a grid-based level layout. Just holds whether the cell is open to the North, South, East and West
- CellMaster.cs : Holds the state of the cells in a level. Generates a layout to fill a square of a given size.
- CellPathfinder.cs : Uses an A* pathfinding system to find the shortest path between any two cells in a level.
- WorldBuilder.cs : Pretty Unity specific, instantiates walls to match a layout generated/held in CellMaster

For more info, see ( http://grapefruitgames.com/2013/03/11/generating-doo…evels-in-unity/ ‎) for details, with an example Unity3D project hosted in the Github repo ( https://github.com/EddieCameron/DoomRoom )