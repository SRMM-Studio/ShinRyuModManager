> ### **%{color:gold} Version 4.4.2 %** ###
* Fix par repacking breaking itself out of nowhere (the wonders of software development)
---
> ### **%{color:orange} Version 4.4.1 %** ###
* Improve file redirection for VF5 REVO. Some files not being picked up will now appear
---

> ### **%{color:orange} Version 4.4.0 %** ###
* Virtua Fighter 5 R.E.V.O support
* Fix GUI crashes caused by DLL injections from mods or other utilities
---

> ### **%{color:orange} Version 4.3.6 %** ###
* Add YP_GET_FILE_PATH export to YakuzaParless. This will allow script mods to better access files. It will return the true path of a provided file. Example: passing "data/ogre_command.ofc" to it may return "mods/MyMod/ogre_command.ofc" if it has been overriden by any mod, if not, it will return the original value provided.
---

> ### **%{color:orange} Version 4.3.5 %** ###
* Fix UBIK redirection bug for Lost Judgment
---

> ### **%{color:orange} Version 4.3.4 %** ###
* Repack particle/arc_list in new DE games
* Fix repacking that broke specific things like UI mods in Yakuza 0/Kiwami 1
---

> ### **%{color:orange} Version 4.3.3 %** ###
* Shin Ryu Mod Manager is now 64 bits
* Add UBIK (cloth physics) support for Dragon Engine games
* Exe validity check removed
* Fix a bug with Yakuza 5
---

> ### **%{color:orange} Version 4.3.2 %** ###
* Gaiden uses dinput8.dll instead now
* Gamepass detection improvement (biweekly occurence)
---

> ### **%{color:orange} Version 4.3.1 %** ###
* Gamepass fix for Like a Dragon
* Gamepass fix for The Man Who Erased His Name
* Improve Gamepass version detection
* Fix DLLExport functions
* Add YP_GET_NUM_MOD_FILES and YP_GET_MOD_FILE DLLExport functions
---

> ### **%{color:orange} Version 4.3.0 %** ###
* Gamepass fix for Yakuza 3 and 4
* Kiwami 2 Gamepass Support
* Improve Gamepass version detection
* Fix SRMM not starting up at all on some instances on Gamepass
---

> ### **%{color:orange} Version 4.2.0 %** ###
* Removed the CLI executable and merged all functionality into this one (execute with commandline arguments or open while holding 'left ctrl' to enable CLI mode)
* Renamed the executable to %{color:#fcbe03}**ShinRyuModManager**%
* The GUI will no longer close after applying mods
* Added a sound to notify when the MLO is finished rebuilding if done through the GUI
* The RebuildMLO feature now supports all games
* Added USM support for Yakuza 3 and 4
* Added entity support for Lost Judgment, Gaiden and 8
* Fixed an issue that would make the wrong error message appear when saving a mod list with all mods disabled
* Fixed an issue that would make the mod list appear empty when displaying the changelog after an update (this window!)
---

> ### **%{color:orange} Version 4.1.4 %** ###
* Reverted a fix that would cause crashes when changing mod priorities.
---

> ### **%{color:orange} Version 4.1.3 %** ###
* Further improvements to overall system stability and other minor adjustments have been made to enhance the user experience.
---

> ### **%{color:orange} Version 4.1.2 %** ###
* Improve performance
* Remove MLO reloading feature (no modder used it)
---

> ### **%{color:orange} Version 4.1.1 %** ###
* Fix incorrect path encoding when attempting to open a mod-image
---

> ### **%{color:orange} Version 4.1.0 %** ###
* Add a changelog *(this window!)*
* Fix update checks not being performed on a separate thread
* The mod-image now supports a variety of formats (PNG, JPEG, BMP, GIF, TIFF, ICO)
* The mod-image will use high quality scaling when being rendered
* The mod-image file will no longer be blocked while being rendered
---

> ### **%{color:orange} Version 4.0.3 %** ###
* Redirect 2d to 2dpar in Yakuza 0, Kiwami 1
* Repack pausepar pars
* Show current game on title bar (by counter185)
* Automatically update the mods list when folders are added/removed (by counter185)
---

> ### **%{color:orange} Version 4.0.2 %** ###
* Fix support for gamepass that got broken recently
* Add ability to drag and drop to install mods (by counter185)
* Fix memory leak when installing from zip (by counter185)
---

> ### **%{color:orange} Version 4.0.1 %** ###
* Yakuza Kiwami 2 fix
---

> ### **%{color:orange} Version 4.0.0 %** ###
* %{color:#fcbe03}Rebranded into **Shin Ryu Mod Manager** %

* Yakuza 5 ADX/HCA support
* Yakuza 6 GOG support
* Hashes for GOG versions of games
* UI remade
* Updater remade
* Install mods through zip files
* Reload MLO while the game is still running (Left Shift + Left Ctrl + Q)