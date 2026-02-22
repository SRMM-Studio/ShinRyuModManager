> All byte values are little-endian

### ModList Format

| Segment        | Value / Range | Length   | Type      | Encoding / Details                                         |
|:---------------|:--------------|:---------|:----------|:-----------------------------------------------------------|
| Signature      | "SRMM_ML"     | 7 bytes  | char[]    | ASCII text, hex: `53 52 4D 4D 5F 4D 4C`                    |
| Version        | 0 - 255       | 1 byte   | uint8     | File format version                                        |
| Active Profile | 0 - 7         | 1 byte   | uint8     | Active profile index (value + 1 = profile, 0 -> Profile 1) |
| Mod Count      | 0 - 65,535    | 2 bytes  | uint16    | Number of mod entries                                      |
| Mod Entries    | Variable      | Variable | `ModInfo` | Repeats `Mod Count` times                                  |


### ModInfo Format

| Segment          | Value / Range      | Length   | Type   | Encoding / Details                                     |
|:-----------------|:-------------------|:---------|:-------|:-------------------------------------------------------|
| Enabled Profiles | `Enabled Profiles` | 1 byte   | uint8  | Bitmask representing profiles where the mod is enabled |
| Name Length      | 0 - 65,535         | 2 bytes  | utin16 | Length of mod name in bytes                            |
| Mod Name         | Variable           | Variable | char[] | UTF8 encoded text, length = `Name Length`              |


### Enabled Profiles Format

| Bit | Mask | Profile   |
|:----|:-----|:----------|
| 0   | 0x01 | Profile 1 |
| 1   | 0x02 | Profile 2 |
| 2   | 0x04 | Profile 3 |
| 3   | 0x08 | Profile 4 |
| 4   | 0x10 | Profile 5 |
| 5   | 0x20 | Profile 6 |
| 6   | 0x40 | Profile 7 |
| 7   | 0x80 | Profile 8 |