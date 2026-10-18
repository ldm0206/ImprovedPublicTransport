# Improved Public Transport 2

《城市：天际线》（Cities: Skylines）模组：按线路精确控制公共交通的车辆数量与车型。

基于 [bloodypenguin/ImprovedPublicTransport](https://github.com/bloodypenguin/ImprovedPublicTransport) 修改，修复了与当前游戏版本不兼容的问题。

## 安装

1. **先关闭游戏**
2. 打开 PowerShell，粘贴运行以下命令（自动下载最新版并解压到 Mods 目录）：

```powershell
$dest = "$env:LOCALAPPDATA\Colossal Order\Cities_Skylines\Addons\Mods"
New-Item -ItemType Directory -Force $dest | Out-Null
Invoke-WebRequest "https://github.com/ldm0206/ImprovedPublicTransport/releases/latest/download/ImprovedPublicTransport2.zip" -OutFile "$env:TEMP\ImprovedPublicTransport2.zip"
Expand-Archive "$env:TEMP\ImprovedPublicTransport2.zip" -DestinationPath $dest -Force
```

3. 启动游戏 → **内容管理（Content Manager）→ 模组（Mods）** → 找到 **Improved Public Transport 2** 并启用

<details>
<summary>手动安装</summary>

从 [Releases](https://github.com/ldm0206/ImprovedPublicTransport/releases/latest) 下载 `ImprovedPublicTransport2.zip`，解压后得到一个 `ImprovedPublicTransport2` 文件夹，将其整个放到：

```
%LOCALAPPDATA%\Colossal Order\Cities_Skylines\Addons\Mods\
```

最终路径形如 `...\Addons\Mods\ImprovedPublicTransport2\ImprovedPublicTransport2.dll`。

</details>

### 注意事项

- **不要与创意工坊中的原版 Improved Public Transport 2 同时启用**——两者使用相同的 Harmony ID，会冲突
- Harmony 依赖由 `CitiesHarmony.API` 在启动时自动安装，无需手动订阅

## 更新

重新运行上面的安装命令即可覆盖更新。

## 构建

项目通过 GitHub Actions 自动编译：每次推送到 `master` 都会编译、打包并发布新的 Release（标签格式 `v<版本>-<构建号>`），PR 只做编译验证。

本地构建需要：

- Visual Studio 2022（MSBuild，需支持 C# 9.0）与 NuGet
- 游戏程序集放在仓库 `Managed/` 目录（Cities: Skylines 1.21.x 的 `Assembly-CSharp.dll`、`ColossalManaged.dll`、`ICities.dll`、`UnityEngine.dll`）

```powershell
git submodule update --init
nuget restore ImprovedPublicTransport.sln
msbuild ImprovedPublicTransport.sln /p:Configuration=Release
```

编译后会自动复制到游戏 Mods 目录；加 `/p:SkipDeploy=true` 可跳过。

## 许可

原模组作者：[bloodypenguin](https://github.com/bloodypenguin)。
