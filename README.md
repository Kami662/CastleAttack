# Castle Attack!

A mobile roguelite tower-defense game with the roles reversed: you play the
attacking monster horde, sending waves down a path toward an AI-defended castle.

Unity **6000.6.1f1**, Universal Render Pipeline.

Design documentation lives in the shared Claude project (`game-design-document.md`),
not in this repo.

---

## Setting up a new machine

After cloning, run the setup script from the repo root. **Once, per machine.**

**Windows** (PowerShell):
```powershell
.\setup-dev.ps1
```

**macOS / Linux**:
```bash
./setup-dev.sh
```

That's it. The script finds your Unity install, registers the merge tool, and
initialises Git LFS — then tells you plainly whether each one worked.

### Why this isn't optional

Two pieces of this project's setup **cannot be committed to the repo** and have
to exist on each machine. Both fail *silently* if you skip them — nothing errors,
you just quietly corrupt things:

**1. Unity SmartMerge.** Unity scenes and prefabs are YAML. Git's default
line-based merge mangles them, so two people touching the same scene produces a
conflict that is genuinely painful to untangle by hand. `.gitattributes` tells
git to use a merge driver called `unityyamlmerge`, but the driver's *definition*
lives in `.git/config`, which isn't committed. Skip this and git falls back to
the line-based merge without complaining.

**2. Git LFS.** Art assets are stored as LFS pointers. Without LFS installed,
`git clone` gives you small text files where your textures and models should be,
and Unity will show broken assets with no useful error.

If the script reports anything under "STILL NEEDED", fix it before you commit.

### Manual fallback

If the script can't find Unity, register the merge driver yourself:

```bash
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver '"<PATH-TO>/UnityYAMLMerge.exe" merge -p --force --fallback none %O %B %A %A'
git config merge.unityyamlmerge.recursive binary
git lfs install
```

The tool lives at:
- **Windows** — `C:\Program Files\Unity\Hub\Editor\<version>\Editor\Data\Tools\UnityYAMLMerge.exe`
- **macOS** — `/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/Tools/UnityYAMLMerge`

### Verify it worked

```bash
git config --get merge.unityyamlmerge.driver   # should print a path
git lfs version                                 # should print a version
```

---

## Working conventions

**Don't edit the same scene at the same time as someone else.** SmartMerge makes
conflicts survivable, not pleasant. Say in chat when you're working in
`SampleScene`, or split work into separate scenes and prefabs where you can.

**Prefabs over scene objects.** Changes to a prefab merge far more cleanly than
changes to a scene. If something will be edited by more than one person, make it
a prefab.

**Commit `.meta` files with their assets.** Unity tracks every asset by a GUID
stored in its `.meta` file. Committing one without the other breaks references
for everyone else.

**Never commit `Library/`.** It's a local cache — multiple GB, machine-specific,
regenerated automatically. Already in `.gitignore`.

---

## Art pipeline

**Keep source art out of this repo.** Working files — layered `.psd`, uncompressed
`.blend` with full modifier stacks, raw audio — live in shared cloud storage.
Only export **game-ready** assets here: `.fbx`, compressed textures, final audio.

Source files are large, change constantly, and Unity can't use them directly.
Committing them burns LFS quota for everyone with no benefit. A 200 MB working
`.blend` might export to a 2 MB `.fbx` — that's the one the game needs.

### LFS quota

The formats tracked by LFS are listed in `.gitattributes`. GitHub's free tier is
**1 GB storage and 1 GB bandwidth per month**, and bandwidth counts every clone
and pull by every person. If we outgrow it, a data pack is $5/month for 50 GB.

To add a new format to LFS later:
```bash
git lfs track "*.ext"
git add .gitattributes
```

Note this only affects files committed **from that point on**. Anything already
in history stays in regular git — moving it requires rewriting history, which is
why formats worth tracking should be added *before* the first big file lands.
