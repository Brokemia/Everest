# Everest - Celeste Mod Loader / Base API

### License: MIT

----

[![Build Status](https://travis-ci.org/EverestAPI/Everest.svg?branch=master)](https://travis-ci.org/EverestAPI/Everest)

Using [MonoMod](https://github.com/0x0ade/MonoMod), an open-source C# modding tool.

**We're in #game_modding on the "Mt. Celeste Climbing Association" Discord server:**

[![Discord invite](github/invite.png)](https://discord.gg/6qjaePQ)

### Everest Installation:
- If you've updated Celeste or switched betas / branches, delete the `orig` directory where Celeste is installed.
- Download [latest `build-XYZ.zip`](https://ams3.digitaloceanspaces.com/lollyde/index.html)
    - Latest build may be unstable, download at own risk.
    - GitHub releases with stable Releases/Milestones (slower to update): [link](https://github.com/EverestAPI/Everest/releases)
- Extract `build-XYZ.zip` to where Celeste is installed. `Celeste.Mod.mm.dll` should be right next to `Celeste.exe`
- Run `MiniInstaller.exe`

### Mod Installation:
- If it's missing, create a `Mods` directory where Celeste is.
- Put the mod `.zip` into the `Mods` directory.
    - For prototyping: Create a subdirectory, pretend it's a `.zip`
- That's it.

### Everest Devbuild Installation:
- Copy files from inside `lib` into Celeste dir.
- Copy built `Celeste.Mod.mm.dll` into Celeste dir.
- Copy `MiniInstaller.exe` into Celeste dir.
- Run `MiniInstaller.exe`

### Mod Development:
- Follow the installation instructions.
- Use RainbowMod as an example mod. It already contains:
    - The required references (`lib/`, `lib-stripped/`) with "Copy Local" set to "False"
    - The mod `metadata.yaml`
