# ChillPatcherLite
[简体中文](README.md) | [English](README_EN.md) | [日本語](README_JA.md)

[![License: GPL-3.0](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
[![.NET Framework 4.7.2](https://img.shields.io/badge/.NET%20Framework-4.7.2-blue.svg)](https://dotnet.microsoft.com/download/dotnet-framework/net472)
[![BepInEx](https://img.shields.io/badge/BepInEx-Plugin-green.svg)](https://github.com/BepInEx/BepInEx)

[ChillPatcher](https://github.com/BeyondtheApex/ChillPatcher) から切り出した、ゲーム『Chill with You : Lo-Fi Story』用 BepInEx プラグイン：**UI レイアウトの変更と、音楽ジャケットの拡大**

---

[![Chill with You](imgs/header_schinese.jpg)](https://store.steampowered.com/app/3548580/)

> 『Chill with You : Lo-Fi Story』は、物語を書くのが好きな女の子・サトネと一緒に働く、オーディオノベル形式のゲームです。アーティストのオリジナル楽曲や環境音、風景を自由にカスタマイズして、作業に集中できる空間を作ることができます。サトネとの関係が深まるにつれ、あなたは彼女との特別なつながりに気づくかもしれません。

---

## 動作イメージ

![デモ](imgs/overview.png)

[ChillPatcher](https://github.com/BeyondtheApex/ChillPatcher) **V1.3.4.1** から移植した 2 つの UI 機能です。作者さんが作り直した UI はとても見やすいのですが、後半は MOD の機能が増えすぎて複雑になってしまったので、自分の一番好きな部分だけを切り出して軽量版にしました。

音楽の再生には [Chill Music Information Sync](https://github.com/Cainongw/ChillMusicInformationSyncMod) を使って NetEase Cloud Music を同期しています。その方が便利だと感じたからです。

## インストール

### 前提環境

- ゲーム『Chill with You : Lo-Fi Story』
- [BepInEx 5.x](https://github.com/BepInEx/BepInEx/releases)（6.0 は使用しないでください）

### 手順

1. **BepInEx をインストール**
   - 上のリンクから BepInEx をダウンロードします。
   - ゲームのルートフォルダに解凍します。
   - ゲームを一度起動して BepInEx のフォルダを生成します（`[ゲームルート]/BepInEx/plugins/` が確認できます）。

2. **MOD をインストール**
   - Release から最新版の `ChillPatcherLite.dll` をダウンロードします。
   - `ChillPatcherLite.dll` を `BepInEx/plugins/` に入れます。
   - フォルダ構成が次のようになっていることを確認してください：

```
[ゲームルート]/
└── BepInEx/
    └── plugins/
            └── ChillPatcherLite.dll
```

## 他の MOD

このゲームの他の MOD に興味があれば、こちらもご覧ください: [awesome-chillwithyou](https://github.com/clsty/awesome-chillwithyou)

## ライセンスと謝辞

- [ChillPatcher](https://github.com/BeyondtheApex/ChillPatcher) V1.3.4.1（作者: [BeyondtheApex](https://github.com/BeyondtheApex)）から移植しています。
- 元プロジェクトと本派生作品は **GPL-3.0** でライセンスされています。著作権表示とライセンス表示は保持してください。
- 本プラグインは学習・交流目的のみです。正規版ゲームを応援してください。
