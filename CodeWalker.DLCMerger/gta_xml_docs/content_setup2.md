### How the two files are tied together

| Element                                                         | Where it lives | What it points to / matches                                                                                                                        | Purpose of the link                                                                                    |
| --------------------------------------------------------------- | -------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| **`<datFile>` value** (`content.xml`)                           | **setup2.xml** | The actual **content.xml** file                                                                                                                    | Tells the game which secondary manifest to load first.                                                 |
| **`deviceName`** (`dlc_t72b3m`)                                 | setup2.xml     | Prefix of every file path in **content.xml** (`dlc_t72b3m:/…`)                                                                                     | Couples the “setup” record to the physical DLC directory; if you rename one you must rename the other. |
| **`nameHash`** (`t72b3m`)                                       | setup2.xml     | 1. Start of every *change‑set* name in **content.xml** (`t72b3m_AUTOGEN`, `t72b3m_UNLOCKS_AUTOGEN`)  2. Folder/RPF names such as `t72b3m_mods.rpf` | Keeps every identifier derived from the same short hash so the engine can resolve dependencies.        |
| **`contentChangeSetGroups/Item/ContentChangeSets`**             | setup2.xml     | Exact `changeSetName` entries inside **content.xml**                                                                                               | Ensures the sets listed in the startup group actually exist and can be activated.                      |
| **`type` + `order`** (`EXTRACONTENT_COMPAT_PACK`, `value="32"`) | setup2.xml     | Load slot reserved by the game                                                                                                                     | Must stay unique across all DLC so load ordering and compatibility logic work.                         |

### Constraints you **must** keep intact

1. **One‑to‑one filenames**

   * `setup2.xml`’s `<datFile>` must still spell **content.xml** (or whatever you rename the secondary manifest to).
   * If you rename or move *content.xml*, update the `<datFile>` path accordingly.

2. **Consistent DLC prefix** (`dlc_t72b3m`)

   * Folder name, `deviceName`, and every file path inside **content.xml** must match **exactly** (including case).
   * Changing it means renaming the DLC folder and every reference inside both XMLs.

3. **Stable hash root** (`t72b3m`)

   * `nameHash` in setup2.xml, every `changeSetName`, and file names such as `t72b3m_mods.rpf` must share the same root string.
   * If you alter the hash, change it *everywhere* or the engine will think they are unrelated assets.

4. **Change‑set integrity**

   * Every entry listed under `ContentChangeSets` in setup2.xml **must appear** once in `<contentChangeSets>` inside content.xml, and vice‑versa.
   * Do not add a new change‑set in one file without mirroring it in the other.

5. **Unique `order` value**

   * `order value="32"` decides when this pack loads relative to others.
   * Re‑use of the same number by two packs will make one of them override the other.

6. **Correct pack `type`**

   * Keep `EXTRACONTENT_COMPAT_PACK` unless you know the alternative you need; changing it alters how the game treats dependencies.

7. **Path syntax and wildcards**

   * Leave `%PLATFORM%` exactly as shown; it is substituted at runtime.
   * Preserve the drive‑style prefix (`dlc_t72b3m:/`)—changing the colon or leading slash breaks the virtual file‑system mount.

8. **File‑enable lists stay in sync**

   * Files enumerated under `<filesToEnable>` should also exist in the `<dataFiles>` list and on disk.
   * Deleting or renaming a `.meta`/`.rpf` file requires updating both sections.

If all of these constraints remain satisfied, the two XML manifests will continue to recognise each other and the DLC will mount cleanly in‑game.
