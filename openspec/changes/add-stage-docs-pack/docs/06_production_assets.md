# Production｜素材与资产（Manifest + License + Pipeline）

## Outcome
让素材“可追溯、可替换、可验收”，避免混风格与侵权风险。

## Naming（真相源）
- `assets/_docs/readme.md`（命名/目录/ResourceKey 规则）

## Asset Manifest Template（建议每次发包更新）
> 输出路径建议：`.tmp/resource_manifest.json`（工具见 `assets/_docs/readme.md`）。

字段建议：
- `resourceKey`
- `type`（texture/audio/data/font）
- `source`（self-made / generated / pack / purchased）
- `license`（internal / CC0 / CC-BY / commercial / unknown）
- `owner`（责任人/来源链接）
- `replaceable`（true/false）

## License Ledger Template（许可证台账）
| Asset/Pack | Source | License | Proof | Notes |
|---|---|---|---|---|
|  |  |  |  |  |

## Pipeline Verification（最小验收）
- 资源命名校验：`dotnet run --project src/Arcadia.AssetTool/Arcadia.AssetTool.csproj -- validate --root assets`
- 清单生成：`dotnet run --project src/Arcadia.AssetTool/Arcadia.AssetTool.csproj -- manifest --root assets --out .tmp/resource_manifest.json`

## Risks
- “生成素材”版权归属不清：必须在台账里写明条款与 proof。

