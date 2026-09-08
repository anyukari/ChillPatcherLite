# ChillPatcherLite

从 [BeyondtheApex/ChillPatcher](https://github.com/BeyondtheApex/ChillPatcher) **V1.3.4.1**
中完整移植出来的两个 UI 功能，做成一个独立的轻量 BepInEx 插件：

1. **UI 重排列**（`EnableUIRearrange`）
   - 把主界面图标整理到顶部/左侧，把音乐控制条移到左下角，
     使游戏界面更接近音乐播放器的布局。
2. **大音乐封面**（`EnableAlbumArtDisplay` + UI 重排列的 2 倍按钮）
   - 播放列表切换按钮会显示当前歌曲的方形封面，并放到音乐控制条上方放大 2 倍；
   - 关闭 UI 重排列时，保留原 ChillPatcher 的圆形小封面行为。

> 移植代码仅保留这两个功能。**不包含** RIME 输入法、OneJS/图形化设置、
> 歌词/AI/番茄钟/天气/窗口小组件、Spotify/日历、网络音乐模块加载器等。

## 与 ChillPatcher 的区别

原版 V1.3.4.1 还包含大量额外系统，启动后即使 UI 实例被关闭也仍然会初始化
OneJS、模块加载、输入法等。本插件编译后只有一个 DLL，只挂两个 Harmony 补丁，
代码路径与 V1.3.4.1 的对应补丁保持一致。

## 安装

前置：游戏《Chill with You: Lo-Fi Story》+ BepInEx 5.x。

1. 编译得到 `ChillPatcherLite.dll`（见下方构建）。
2. 复制到：

   ```
   [游戏目录]\BepInEx\plugins\ChillPatcherLite.dll
   ```

3. **不要与原 ChillPatcher 同时使用**。若要替换原 ChillPatcher，请先从
   `BepInEx\plugins` 中移除原 `ChillPatcher` 文件夹和 `ChillPatcher.zip`，
   只保留本插件的 `ChillPatcherLite.dll`。
4. 启动游戏一次后，可在
   `[游戏目录]\BepInEx\config\com.chillpatcherlite.plugin.cfg`
   中调整配置。

### 与 ChillMusicInformationSync 兼容

如果同时安装 [ChillMusicInformationSync](https://github.com/Cainongw/ChillMusicInformationSyncMod)，
本插件会在启动时自动把它的封面注入切到“ChillPatcher 共存模式”，
让同步封面直接显示在大封面按钮上，而不会另外生成一张独立封面。
本插件**不会**冒用原 ChillPatcher 的程序集名或类型名。

## 配置

```ini
[Features]

## 重排列主界面按钮/音乐控制条布局
# Default value: true
EnableUIRearrange = true

## 在播放列表切换按钮上显示当前歌曲封面
# Default value: true
EnableAlbumArtDisplay = true

## 隐藏音乐控制条底部的背景图
# Default value: false
HideBottomBackImage = false
```

键名与 ChillPatcher V1.3.4.1 保持一致，可以直接照搬原配置值。

## 封面来源说明

本轻量版没有 ChillPatcher 的“网络音乐模块系统”，因此：

- 本地歌曲：自动读取歌曲同目录下的 `cover` / `folder` / `front` / `album` /
  `artwork`（jpg/jpeg/png/bmp/gif）图片，找不到时使用内嵌的本地封面；
- 游戏原生曲目：按 原创 / 特别 / 其他 使用内嵌的三张分类封面；
- 其他带 UUID 且无模块提供封面的歌曲：使用内嵌默认封面；
- 如果你依赖网易云/B站/QQ 模块提供实时封面，需要保留原模块系统，这不在本
  轻量版范围内。

## 构建

```powershell
.\build.ps1
```

或手动：

```powershell
dotnet build ChillPatcherLite.csproj -c Release
```

默认游戏路径：

```
E:\SOFT\STEAM\steamapps\common\Chill with You Lo-Fi Story
```

如果路径不同，用 `-GameDir` 指定：

```powershell
.\build.ps1 -GameDir "D:\Steam\steamapps\common\Chill with You Lo-Fi Story"
```

## 许可与致谢

- 移植自 [ChillPatcher](https://github.com/BeyondtheApex/ChillPatcher)
  V1.3.4.1，作者 [BeyondtheApex](https://github.com/BeyondtheApex)。
- 原项目与本文档派生代码按 **GPL-3.0** 授权，请保留版权与许可声明。
- 本插件仅供学习交流，请支持正版游戏。
