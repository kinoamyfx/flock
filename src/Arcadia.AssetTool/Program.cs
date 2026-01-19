using System.Text.Json;
using Arcadia.Mdk.Assets;

internal static class Program
{
    public static int Main(string[] args)
    {
        // Why: 不依赖 Godot 的 CLI 校验工具，先把“命名/目录/Key 映射”这种最硬规则自动化，避免画风与资源在迭代中漂移。
        // Context: v1.0.0 选择高像素密度 Pixel；资产量大且 Mod 允许覆盖资源，必须让 Key 稳定可追溯。
        // Attention: 本工具输出尽量短；CI/脚本可用 `--json` 获取结构化结果。
        if (args.Length == 0 || args[0] is "-h" or "--help")
        {
            PrintHelp();
            return 0;
        }

        var cmd = args[0].ToLowerInvariant();
        var assetsRoot = GetArgValue(args, "--root") ?? "assets";
        var json = args.Any(x => x is "--json");

        if (cmd == "validate")
        {
            var validator = new AssetValidator(assetsRoot);
            var result = validator.Validate();
            if (json)
            {
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                Console.WriteLine($"Asset validation: {(result.IsOk ? "OK" : "FAILED")}, Issues={result.Issues.Count}");
                foreach (var issue in result.Issues.Take(50))
                {
                    Console.WriteLine($"{issue.Code}|{issue.Path}|{issue.Message}");
                }

                if (result.Issues.Count > 50)
                {
                    Console.WriteLine($"... truncated (showing 50/{result.Issues.Count})");
                }
            }

            return result.IsOk ? 0 : 2;
        }

        if (cmd == "manifest")
        {
            var outFile = GetArgValue(args, "--out") ?? ".tmp/resource_manifest.json";
            Directory.CreateDirectory(Path.GetDirectoryName(outFile) ?? ".");

            var validator = new AssetValidator(assetsRoot);
            var result = validator.Validate();
            if (!result.IsOk)
            {
                Console.Error.WriteLine("Cannot generate manifest: asset validation failed.");
                return 2;
            }

            var files = Directory.EnumerateFiles(assetsRoot, "*", SearchOption.AllDirectories)
                .Select(x => Path.GetFullPath(x))
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();

            var entries = new List<object>(files.Length);
            foreach (var file in files)
            {
                if (!AssetPathMapper.TryMapToResourceKey(assetsRoot, file, out var key, out _))
                {
                    continue;
                }

                entries.Add(new
                {
                    file = Path.GetRelativePath(assetsRoot, file).Replace('\\', '/'),
                    key = key.ToString()
                });
            }

            var payload = new
            {
                generatedAtUtc = DateTimeOffset.UtcNow,
                root = assetsRoot,
                count = entries.Count,
                entries
            };

            File.WriteAllText(outFile, JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Manifest written: {outFile} (count={entries.Count})");
            return 0;
        }

        Console.Error.WriteLine($"Unknown command: {cmd}");
        PrintHelp();
        return 1;
    }

    private static string? GetArgValue(string[] args, string key)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Arcadia.AssetTool");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  validate --root <assetsDir> [--json]");
        Console.WriteLine("  manifest --root <assetsDir> --out <file>");
    }
}
