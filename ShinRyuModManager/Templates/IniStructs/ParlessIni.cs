namespace ShinRyuModManager.Templates;

public static class ParlessIni
{
    public const int CURRENT_VERSION = 7;
    
    public static List<IniSection> GetParlessSections()
    {
        return
        [
            new IniSection // Parless
            {
                Name = "Parless",
                Comments = ["All values are 0 for false, 1 for true."],
                Keys =
                [
                    new IniKey
                    {
                        Name = "IniVersion",
                        Comments = ["Ini version. Should not be changed manually."],
                        DefaultValue = CURRENT_VERSION
                    },
                    
                    new IniKey
                    {
                        Name = "ParlessEnabled",
                        Comments = ["Global switch for Parless. Set to 0 to disable everything."],
                        DefaultValue = 1
                    },
                    
                    new IniKey
                    {
                        Name = "TempDisabled",
                        Comments =
                        [
                            "Temporarily disables Parless for one run only. Overrides ParlessEnabled.",
                            "This will be set back to 0 whenever the game is launched with it set to 1."
                        ],
                        DefaultValue = 0
                    }
                ]
            },
            new IniSection // Overrides
            {
                Name = "Overrides",
                Comments =
                [
                    "General override order:",
                    "if LooseFilesEnabled is set to 1, files inside \".parless\" paths will override everything.",
                    "if ModsEnabled is set to 1, mod files will override files inside pars."
                ],
                Keys =
                [
                    new IniKey
                    {
                        Name = "LooseFilesEnabled",
                        Comments =
                        [
                            "Allows loading files from \".parless\" paths.",
                            "Files in these paths will override the mod files installed in /mods/",
                            "Example: files in /data/chara.parless/ will override the",
                            "files in /data/chara.par AND files in /chara/ in all mods."
                        ],
                        DefaultValue = 0
                    },
                    
                    new IniKey
                    {
                        Name = "ModsEnabled",
                        Comments =
                        [
                            "Allows loading files from the /mods/ directory.",
                            "Each mod has to be extracted in its own folder, where its contents",
                            "will mirror the game's /data/ directory. Pars should be extracted into folders.",
                            "Example: /mods/ExampleMod/chara/auth/c_am_kiryu/c_am_kiryu.gmd",
                            "will replace the /auth/c_am_kiryu/c_am_kiryu.gmd file inside /data/chara.par"
                        ],
                        DefaultValue = 1
                    },
                    
                    new IniKey
                    {
                        Name = "RebuildMLO",
                        Comments =
                        [
                            "Removes the need to run ShinRyuModManager before launching your game,",
                            "should have little to no effect on the time it takes to launch,",
                            "and should help users avoid mistakenly not rebuilding.",
                            "Optional QOL feature to help you avoid having to re-run the mod manager every time."
                        ],
                        DefaultValue = 1
                    },
                    
                    new IniKey
                    {
                        Name = "Locale",
                        Comments =
                        [
                            "Changes the filepaths of localized pars to match the current locale.",
                            "Only needed if you're running a non-English version of the game.",
                            "English=0, Japanese=1, Chinese=2, Korean=3"
                        ],
                        DefaultValue = 0
                    }
                ]
            },
            new IniSection // RyuModManager
            {
                Name = "RyuModManager",
                Comments = [],
                Keys =
                [
                    new IniKey
                    {
                        Name = "Verbose",
                        Comments = ["Print additional info, including all file paths that get added to the MLO"],
                        DefaultValue = 0
                    },
                    
                    new IniKey
                    {
                        Name = "CheckForUpdates",
                        Comments = ["Check for updates before exiting the program"],
                        DefaultValue = 1
                    },
                    
                    new IniKey
                    {
                        Name = "ShowWarnings",
                        Comments = ["Show warnings whenever a mod was possibly not extracted correctly"],
                        DefaultValue = 1
                    },
                    
                    new IniKey
                    {
                        Name = "LoadExternalModsOnly",
                        Comments =
                        [
                            "Only load mods from the /mods/_externalMods/ directory, and ignore the load order file.",
                            "That directory will be created automatically by external mod managers.",
                            "If this option is disabled, external mods will NOT be loaded, unless \"_externalMods\" is added manually to the load order file.",
                            "If the directory does not exist, then the load order will be used as normal."
                        ],
                        DefaultValue = 1
                    }
                ]
            },
            new IniSection // Logs
            {
                Name = "Logs",
                Comments = [],
                Keys =
                [
                    new IniKey
                    {
                        Name = "LogMods",
                        Comments = ["Write filepaths for mods that get loaded into modOverrides.txt"],
                        DefaultValue = 0
                    },
                    
                    new IniKey
                    {
                        Name = "LogParless",
                        Comments = ["Write filepaths for .parless paths that get loaded into parlessOverrides.txt"],
                        DefaultValue = 0
                    },
                    
                    new IniKey
                    {
                        Name = "LogAll",
                        Comments = ["Write filepaths for every file that gets loaded into allFilepaths.txt"],
                        DefaultValue = 0
                    },
                    
                    new IniKey
                    {
                        Name = "IgnoreNonPaths",
                        Comments = ["Do not log any strings that are not proper filepaths"],
                        DefaultValue = 1
                    }
                ]
            },
            new IniSection // Debug
            {
                Name = "Debug",
                Comments = [],
                Keys =
                [
                    new IniKey
                    {
                        Name = "ConsoleEnabled",
                        Comments = ["Enable the debugging console"],
                        DefaultValue = 0
                    },
                    new IniKey
                    {
                        Name = "ReloadingEnabled",
                        Comments = ["Enable reloading mod files by pressing CTRL+ALT+R"],
                        DefaultValue = 0,
                    },
                    new IniKey
                    {
                        Name = "V5FSArcadeSupport",
                        Comments = ["Enable modding Virtua Fighter 5 Final Showdown on Yakuza games who have it on their arcade."],
                        DefaultValue = 0,
                    },
                ]
            }
        ];
    }
}
