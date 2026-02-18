# SaveMyFit  

**SaveMyFit** is a simple Quality of Life mod that saves player costume (color) choices to the save file using [FrogDataLib](https://thunderstore.io/c/yapyap/p/Robyn/FrogDataLib/)  

## Features
 - Outfit selections for all players are saved on a per-savefile basis.  
 - Outfits are matched to both the index and the name of the costume (color).  
	- If a mismatch is detected, the outfit will **not** be applied.
		- This could happen if the number of Colors has changed since the last time this save was played.  
	- Each client's unique playerId is saved with their outfit name and outfit index.  
 - Outfit save information is updated every time the game is saved and any time someone changes their fit (color).  

 ### Source/Bug reports
 - Feel free to browse the source of this mod and report any feedback at https://github.com/darmuh/SaveMyFit