# Assets（核心资源目录）

本目录用于存放 **官方 core** 的基准资源（贴图/音效/数据等），并作为 Mod 资源覆盖的默认候选集。

## 命名与目录（v1.0.0 约束）
- 只允许小写字母/数字/下划线（`a-z0-9_`），禁止空格与中文文件名。
- 目录/文件命名必须可推导 `ResourceKey`（用于 Mod 覆盖与审计）。
  - 示例：`assets/items/icons/icon_wood.png` → `texture:items/icons/icon_wood`

## 校验工具
- 运行校验：`dotnet run --project src/Arcadia.AssetTool/Arcadia.AssetTool.csproj -- validate --root assets`
- 生成清单：`dotnet run --project src/Arcadia.AssetTool/Arcadia.AssetTool.csproj -- manifest --root assets --out .tmp/resource_manifest.json`

