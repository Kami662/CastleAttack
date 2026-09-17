# Castle Attack!

A mobile roguelite tower-defense game with the roles reversed: you play the
attacking monster horde, sending waves down a path toward an AI-defended castle.

Unity **6000.6.1f1**, Universal Render Pipeline.

Design documentation lives in the shared Claude project (`game-design-document.md`),
not in this repo.

---

## Setting up a new machine

Clone the repo, then do these two things **before your first merge**.

### 1. Register Unity's YAML merge tool (required, once per machine)

Unity scene and prefab files are YAML. Git's default line-based merge mangles
them, so two people touching the same scene produces a conflict that is
genuinely painful to resolve by hand. Unity ships its own YAML-aware merge tool
that resolves most of these automatically.

`.gitattributes` already tells Git to *use* a driver called `unityyamlmerge`,
but the driver's definition lives in `.git/config`, which is **not committed** —
so every developer has to register it locally.

On Windows, from the repo root:

```bash
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver '"C:/Program Files/Unity/Hub/Editor/6000.6.1f1/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p --force --fallback none %O %B %A %A'
git config merge.unityyamlmerge.recursive binary
```

Adjust the path if your Unity version or install location differs. To check the
tool is actually there:

```bash
ls "/c/Program Files/Unity/Hub/Editor/6000.6.1f1/Editor/Data/Tools/UnityYAMLMerge.exe"
```

On macOS the tool is at
`/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/Tools/UnityYAMLMerge`.

### 2. Check your Unity editor settings

These should already be correct (they're committed in `ProjectSettings/`), but
worth confirming under **Edit → Project Settings → Editor**:

- **Asset Serialization → Mode: Force Text** — required for any of the above to work.
- **Version Control → Mode: Visible Meta Files**

---

## Working conventions

**Don't edit the same scene at the same time as someone else.** SmartMerge makes
conflicts survivable, not pleasant. Say in chat when you're working in
`SampleScene`, or split work into separate scenes and prefabs where you can.

**Prefabs over scene objects.** Changes to a prefab merge far more cleanly than
changes to a scene. If something is going to be edited by more than one person,
make it a prefab.

**Commit `.meta` files with their assets.** Unity tracks every asset by a GUID
stored in its `.meta` file. Committing one without the other breaks references
for everyone else.

**Never commit `Library/`.** It's a local cache — multiple GB, machine-specific,
and regenerated automatically. It's already in `.gitignore`.

---

## Git LFS — do this before the art lands

Right now this repo is ~4.5 MB of almost entirely text, so LFS isn't set up yet.
That changes the moment `.blend`, `.fbx` and texture files start arriving: Git
stores every version of a binary file in full, so a 50 MB model edited ten times
is 500 MB in history, permanently, for everyone who clones.

Adding LFS *before* those files are committed is trivial. Adding it afterwards
means rewriting history. So: set it up as the first step of the art pipeline,
not after.

```bash
git lfs install
git lfs track "*.png" "*.psd" "*.tga" "*.exr" "*.fbx" "*.blend" "*.wav" "*.mp3" "*.ogg"
git add .gitattributes
```

That rewrites the relevant lines in `.gitattributes` from `binary` to
`filter=lfs diff=lfs merge=lfs -text`. Everyone then needs `git lfs install`
on their own machine.
