> ### **%{color:gold} Version 4.6.6 %** ###
* Fix modded motion pars freezing Yakuza 3 to Kiwami 1

> ### **%{color:orange} Version 4.6.5 %** ###
* USM support for Yakuza 0, Kiwami 1, and 5
* Fix UBIKs for Gaiden
* Unhardcode coliseum fighter limit for Yakuza 5

> ### **%{color:orange} Version 4.6.4 %** ###
* Fix se.cpk and repacked bgm.cpk mods being broken on Yakuza 0
* Have i told you how much i hate CPK files


> ### **%{color:orange} Version 4.6.3 %** ###
* Experimental: Repack hacts that don't exist on base game for all Dragon Engine games
* Optimize hact repacking behavior for Yakuza 5
* Early preparations for Kiwami 3
* Some path fixes for hact rebuilding

> ### **%{color:orange} Version 4.6.2 %** ###
* Fix broken support for music mods in Yakuza 5
* EXPERIMENTAL: Loose bgm.cpk sound loading (Example: MyMod/stream/system_title_bgm.hca)

> ### **%{color:orange} Version 4.6.1 %** ###
* Fix broken support for mods using the legacy talk hact method in Dragon Engine
* Perhaps we should update our mods so i don't have to consider 5000 edge cases while expanding the manager (wink wink nudge nudge)

> ### **%{color:orange} Version 4.6.0 %** ###
* Fix broken support for mods using the legacy hact/heat action method in Y0/YK1

> ### **%{color:orange} Version 4.5.9 %** ###
* Experimental: CPK repacking for Yakuza 0 and Kiwami 1
* Experimental: Repack hacts that don't exist on base game for 0, Kiwami 1, Like a Dragon, LAD: Gaiden
* Reset all file dates while repacking pars (this might solve some infinite load problems)
* Add exports: YP_GET_GAME_NAME, YP_IS_GOG, YP_IS_XBOX

> ### **%{color:orange} Version 4.5.8 %** ###
* Fix MLO not being rebuilt sometimes

> ### **%{color:orange} Version 4.5.7 %** ###
* Pack HActs in Yakuza 5 that did not exist in the base game (thanks Gibbed.Yakuza0)
* Minor performance improvements
* Basic logging into srmm_log.txt

> ### **%{color:orange} Version 4.5.6 %** ###
* Update Kiwami 2 code
* This fixes some crashes on GOG version too
* Remove unnecessary logs

> ### **%{color:orange} Version 4.5.5 %** ###
* Address "concerns" about library meta location
* Fetch libraries when checking for mod dependencies

> ### **%{color:orange} Version 4.5.4 %** ###
* Can now install .rar and .7z mod files with "Install mod" button too
* Add libraries feature (for script modders that aim to expand game functionality rather than make direct mods)
* Mods can depend on libraries. These will be automatically installed and don't need to be bundled with the mod.
* Examples of these are OgreCommand for Yakuza 3 & 4, and DELib for Yakuza 7 and 8 that makes Like A Brawler mods work

> ### **%{color:orange} Version 4.5.3 %** ###
* Fix broken support for Pirates in Hawaii after the game got updated to 1.10

> ### **%{color:orange} Version 4.5.2 %** ###
* Fix Simplified Chinese file redirection in Pirates in Hawaii

> ### **%{color:orange} Version 4.5.1 %** ###
* Rewrite mod file handling in preparation for new system used in Pirates in Hawaii
* SRMM will now automatically calculate hashes for gmts and beps and move them to appopriate folders in Pirates in Hawaii
* Modders should continue to mod animations just like how they did before Pirates in Hawaii
---

> ### **%{color:orange} Version 4.5.0 %** ###
* Add run game button (@SilvaKabe)
* Fix pirate game path redirection for the first time i hope
---

> ### **%{color:orange} Version 4.4.9 %** ###
* Improve path redirection performance
---

> ### **%{color:orange} Version 4.4.8 %** ###
* Fix CPK binding problems
---

> ### **%{color:orange} Version 4.4.7 %** ###
* Improve path redirection for Pirate Yakuza in Hawaii
---

> ### **%{color:orange} Version 4.4.6 %** ###
* Pirate Yakuza in Hawaii gv_files modding support. Example: mods/MyMod/gv_files/gv_fighter_majima.awb
* Fix path redirection in Pirate Yakuza in Hawaii that affected a small amount of files (mainly chara/dds_hires)
---

> ### **%{color:orange} Version 4.4.5 %** ###
* Pirate Yakuza in Hawaii support
---

> ### **%{color:orange} Version 4.4.4 %** ###
* Disable GUI crash fixes if SRMM is being emulated on Linux, otherwise the application will crash! Bad news for Reshade/Legend fix users on Linux
* Add "ReloadingEnabled" and "V5FSArcadeSupport" to "Debug" section in YakuzaParless.ini
* Now, i don't know why you would do this but, this means you can mod the arcade versions of Virtua Fighter 5 in games like Yakuza 6 if you set V5FSArcadeSupport to 1
---

> ### **%{color:orange} Version 4.4.3 %** ###
* Music modding support for VF5 R.E.V.O.
---

> ### **%{color:orange} Version 4.4.2 %** ###
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