# ChillPatcherLite
[简体中文](README.md) | [English](README_EN.md) | [日本語](README_JA.md)

[![License: GPL-3.0](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
[![.NET Framework 4.7.2](https://img.shields.io/badge/.NET%20Framework-4.7.2-blue.svg)](https://dotnet.microsoft.com/download/dotnet-framework/net472)
[![BepInEx](https://img.shields.io/badge/BepInEx-Plugin-green.svg)](https://github.com/BepInEx/BepInEx)

A BepInEx plugin for *Chill with You : Lo-Fi Story*, split out from [ChillPatcher](https://github.com/BeyondtheApex/ChillPatcher): **rearranges the UI layout and enlarges the music cover**

---

[![Chill with You](imgs/header_schinese.jpg)](https://store.steampowered.com/app/3548580/)

> *Chill with You : Lo-Fi Story* is an audio novel game where you work together with Satone, a girl who loves writing stories. You can customize original tracks from artists, ambient sounds and scenery to create a space for focused work. As your relationship with Satone deepens, you may discover something special between you.

---

## Demo

![Demo](imgs/overview.png)

The two UI features are ported from [ChillPatcher](https://github.com/BeyondtheApex/ChillPatcher) **V1.3.4.1**. The author's rearranged UI looks really nice, but the full mod eventually became huge and complex, so I split out my favorite part and made this lightweight version.

For music playback I use [Chill Music Information Sync](https://github.com/Cainongw/ChillMusicInformationSyncMod) to sync NetEase Cloud Music, which feels more convenient.

## Installation

### Requirements

- *Chill with You : Lo-Fi Story*
- [BepInEx 5.x](https://github.com/BepInEx/BepInEx/releases) (do not use 6.0)

### Steps

1. **Install BepInEx**
   - Download BepInEx from the link above.
   - Extract it into the game root folder.
   - Launch the game once so BepInEx creates its folders (you should see `[Game Root]/BepInEx/plugins/`).

2. **Install the mod**
   - Download the latest `ChillPatcherLite.dll` from Release.
   - Put `ChillPatcherLite.dll` into `BepInEx/plugins/`.
   - Make sure your folder structure looks like this:

```
[Game Root]/
└── BepInEx/
    └── plugins/
            └── ChillPatcherLite.dll
```

## Other mods

If you are interested in other mods for this game, see: [awesome-chillwithyou](https://github.com/clsty/awesome-chillwithyou)

## License & Credits

- Ported from [ChillPatcher](https://github.com/BeyondtheApex/ChillPatcher) V1.3.4.1 by [BeyondtheApex](https://github.com/BeyondtheApex).
- The original project and this derived work are licensed under **GPL-3.0**. Please keep the copyright and license notices.
- This plugin is for learning and communication only. Please support the official game.
