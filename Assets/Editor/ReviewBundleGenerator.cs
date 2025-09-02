#if UNITY_EDITOR
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// This tool is aware of your Mechs namespace/types:
using Mechs;

public class ReviewBundleGenerator : EditorWindow
{
    [MenuItem("Tools/Review Bundle/Create…")]
    private static void Open() => GetWindow<ReviewBundleGenerator>("Review Bundle");

    // Options
    private bool includeAllCs = true;
    private bool includeScenesInBuild = true;
    private bool includeAllScenesToo = false;
    private bool includeProjectSettings = true;
    private bool includePackages = true;
    private bool includeMechAssets = true;
    private bool includeDatabases = true;
    private bool zipWhenDone = true;

    private void OnGUI()
    {
        GUILayout.Label("What to include", EditorStyles.boldLabel);
        includeAllCs = EditorGUILayout.Toggle("All C# under Assets/", includeAllCs);
        includeScenesInBuild = EditorGUILayout.Toggle("Scenes in Build Settings", includeScenesInBuild);
        includeAllScenesToo = EditorGUILayout.Toggle("All Scenes under Assets/", includeAllScenesToo);
        includeProjectSettings = EditorGUILayout.Toggle("ProjectSettings subset", includeProjectSettings);
        includePackages = EditorGUILayout.Toggle("Packages manifest", includePackages);
        includeMechAssets = EditorGUILayout.Toggle("Mech ScriptableObjects", includeMechAssets);
        includeDatabases = EditorGUILayout.Toggle("Component Databases", includeDatabases);
        zipWhenDone = EditorGUILayout.Toggle("Zip when done", zipWhenDone);

        GUILayout.Space(8);
        if (GUILayout.Button("Create Review Bundle"))
            CreateBundle();
    }

    private void CreateBundle()
    {
        var projRoot = Directory.GetParent(Application.dataPath)!.FullName;
        var defaultDir = Path.Combine(projRoot, "ReviewBundles");
        Directory.CreateDirectory(defaultDir);

        var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var folderName = $"ReviewBundle_{PlayerSettings.productName}_{stamp}";
        var targetDirChoose = EditorUtility.SaveFolderPanel("Choose output folder", defaultDir, folderName);
        if (string.IsNullOrEmpty(targetDirChoose)) return;
        var outDir = Path.Combine(targetDirChoose, folderName);
        Directory.CreateDirectory(outDir);

        // Report scaffold
        var report = new StringBuilder();
        report.AppendLine($"# Review Report");
        report.AppendLine($"- Product: {PlayerSettings.productName}");
        report.AppendLine($"- Unity: {Application.unityVersion}");
        report.AppendLine($"- Platform: {EditorUserBuildSettings.activeBuildTarget}");
        report.AppendLine($"- Serialization: {EditorSettings.serializationMode}");
        report.AppendLine();

        // 1) Packages
        if (includePackages)
        {
            var pkgDst = Path.Combine(outDir, "Packages");
            Directory.CreateDirectory(pkgDst);
            CopyIfExists(Path.Combine(projRoot, "Packages/manifest.json"), Path.Combine(pkgDst, "manifest.json"));
            CopyIfExists(Path.Combine(projRoot, "Packages/packages-lock.json"), Path.Combine(pkgDst, "packages-lock.json"));

            var manifestPath = Path.Combine(projRoot, "Packages/manifest.json");
            if (File.Exists(manifestPath))
            {
                report.AppendLine("## Packages (manifest.json excerpt)");
                foreach (var line in File.ReadLines(manifestPath).Take(200))
                    report.AppendLine("    " + line);
                report.AppendLine();
            }
        }

        // 2) ProjectSettings subset
        if (includeProjectSettings)
        {
            var psDst = Path.Combine(outDir, "ProjectSettings");
            Directory.CreateDirectory(psDst);
            string[] psFiles =
            {
                "ProjectSettings.asset",
                "EditorBuildSettings.asset",
                "TagManager.asset",
                "InputManager.asset",
                "GraphicsSettings.asset",
                "QualitySettings.asset",
                "TimeManager.asset",
                "Physics2DSettings.asset",
                "PhysicsManager.asset"
            };
            foreach (var f in psFiles)
                CopyIfExists(Path.Combine(projRoot, "ProjectSettings", f), Path.Combine(psDst, f));
        }

        // 3) All .cs under Assets/
        if (includeAllCs)
        {
            var dst = Path.Combine(outDir, "Assets_Scripts");
            CopyTreeFiltered(Application.dataPath, dst, path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));
        }

        // 4) Scenes
        if (includeScenesInBuild)
        {
            var dst = Path.Combine(outDir, "Scenes_BuildSettings");
            Directory.CreateDirectory(dst);
            foreach (var s in EditorBuildSettings.scenes.Where(s => s.enabled))
            {
                var p = s.path;
                if (!string.IsNullOrEmpty(p))
                    CopyAssetByPath(p, Path.Combine(dst, Path.GetFileName(p)));
            }

            report.AppendLine("## Scenes in Build Settings");
            foreach (var s in EditorBuildSettings.scenes.Where(s => s.enabled))
                report.AppendLine($"- {s.path}");
            report.AppendLine();
        }

        if (includeAllScenesToo)
        {
            var dst = Path.Combine(outDir, "Scenes_All");
            Directory.CreateDirectory(dst);
            foreach (var guid in AssetDatabase.FindAssets("t:Scene"))
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                CopyAssetByPath(p, Path.Combine(dst, Path.GetFileName(p)));
            }
        }

        // 5) Mech-related assets and summary
        if (includeMechAssets)
        {
            var mechDst = Path.Combine(outDir, "MechAssets");
            Directory.CreateDirectory(mechDst);

            // Collect ScriptableObjects
            var comps = LoadAllByType<MechComponentSO>("t:MechComponentSO");
            var passives = LoadAllByType<SpecialEffectSO>("t:PassiveStatModifierSO");
            var actives = LoadAllByType<SpecialEffectSO>("t:ActiveAbilitySO");
            var sets = LoadAllByType<SpecialEffectSO>("t:SetBonusSO");

            // Copy assets (+.meta)
            var allSo = new List<UnityEngine.Object>();
            allSo.AddRange(comps);
            allSo.AddRange(passives);
            allSo.AddRange(actives);
            allSo.AddRange(sets);

            foreach (var a in allSo)
            {
                var p = AssetDatabase.GetAssetPath(a);
                CopyAssetByPath(p, Path.Combine(mechDst, Path.GetFileName(p)));
            }

            // Summary (aware of StatBlock + hardpoints)
            var structural = comps.Where(c => c && c.IsStructural).OrderBy(c => c.Slot).ThenBy(c => c.DisplayName).ToList();
            var weapons = comps.Where(c => c && c.IsWeapon).OrderBy(c => c.DisplayName).ToList();

            report.AppendLine("## Mech Components");
            report.AppendLine("### Structural");
            foreach (var c in structural)
            {
                report.AppendLine($"- **{c.DisplayName}**  (Type: {c.Type}, Slot: {c.Slot}, HP Capacity: {c.HardpointCount})");
                WriteStatBlock(report, c.BaseStats);
                if (c.Tags != null && c.Tags.Count > 0) report.AppendLine($"  - Tags: {string.Join(", ", c.Tags)}");
                if (c.Effects != null && c.Effects.Count > 0) report.AppendLine($"  - Effects: {string.Join(", ", c.Effects.Select(e => e ? $"{e.name} ({e.GetType().Name})" : "<null>"))}");
            }
            report.AppendLine();

            report.AppendLine("### Weapons");
            foreach (var w in weapons)
            {
                report.AppendLine($"- **{w.DisplayName}**  (Req HP: {Mathf.Max(1, w.HardpointsRequired)})");
                WriteStatBlock(report, w.BaseStats);
                if (w.Tags != null && w.Tags.Count > 0) report.AppendLine($"  - Tags: {string.Join(", ", w.Tags)}");
                if (w.Effects != null && w.Effects.Count > 0) report.AppendLine($"  - Effects: {string.Join(", ", w.Effects.Select(e => e ? $"{e.name} ({e.GetType().Name})" : "<null>"))}");
            }
            report.AppendLine();

            // Slot coverage + capacity snapshot (useful for multi-HP sanity)
            report.AppendLine("### Slot Coverage / Capacity Snapshot");
            foreach (SlotType slot in Enum.GetValues(typeof(SlotType)))
            {
                if (slot == SlotType.None) continue;
                var inSlot = structural.Where(s => s.Slot == slot).ToList();
                if (inSlot.Count == 0)
                {
                    report.AppendLine($"- {slot}: (no structural components found)");
                    continue;
                }
                int maxCap = inSlot.Max(s => Mathf.Max(0, s.HardpointCount));
                int avgCap = (int)Math.Round(inSlot.Average(s => Mathf.Max(0, s.HardpointCount)));
                report.AppendLine($"- {slot}: {inSlot.Count} parts • HP Capacity max {maxCap}, avg {avgCap}");
            }
            report.AppendLine();

            // Effects summary
            report.AppendLine("## Effects");
            report.AppendLine($"- Passives: {passives.Count}");
            report.AppendLine($"- Active Abilities: {actives.Count}");
            report.AppendLine($"- Set Bonuses: {sets.Count}");
            report.AppendLine();
        }

        // 6) Databases
        if (includeDatabases)
        {
            var dbDst = Path.Combine(outDir, "Databases");
            Directory.CreateDirectory(dbDst);

            var dbs = LoadAllByType<ComponentDatabaseSO>("t:ComponentDatabaseSO");
            foreach (var db in dbs)
            {
                var p = AssetDatabase.GetAssetPath(db);
                CopyAssetByPath(p, Path.Combine(dbDst, Path.GetFileName(p)));
            }

            // Small DB mention
            if (dbs.Count > 0)
            {
                report.AppendLine("## Component Databases");
                foreach (var db in dbs)
                    report.AppendLine($"- {AssetDatabase.GetAssetPath(db)} : {db.AllComponents?.Count ?? 0} entries");
                report.AppendLine();
            }
        }

        // 7) Write report
        File.WriteAllText(Path.Combine(outDir, "ReviewReport.md"), report.ToString(), Encoding.UTF8);

        // 8) Zip (optional)
        if (zipWhenDone)
        {
            var zipPath = Path.Combine(Path.GetDirectoryName(outDir)!, Path.GetFileName(outDir) + ".zip");
            if (File.Exists(zipPath)) File.Delete(zipPath);
            ZipFile.CreateFromDirectory(outDir, zipPath);
            EditorUtility.RevealInFinder(zipPath);
            Debug.Log($"Review Bundle created: {zipPath}");
        }
        else
        {
            EditorUtility.RevealInFinder(outDir);
            Debug.Log($"Review Bundle created: {outDir}");
        }
    }

    // ---------- helpers ----------
    private static void CopyIfExists(string src, string dst)
    {
        if (!File.Exists(src)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
        File.Copy(src, dst, overwrite:true);
    }

    private static void CopyTreeFiltered(string srcDir, string dstDir, Func<string, bool> filePredicate)
    {
        foreach (var file in Directory.GetFiles(srcDir, "*", SearchOption.AllDirectories))
        {
            if (!filePredicate(file)) continue;
            var rel = file.Substring(srcDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var dst = Path.Combine(dstDir, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
            File.Copy(file, dst, true);
        }
    }

    private static void CopyAssetByPath(string assetPath, string dstPath)
    {
        var projRoot = Directory.GetParent(Application.dataPath)!.FullName;
        var src = Path.Combine(projRoot, assetPath);
        if (!File.Exists(src)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(dstPath)!);
        File.Copy(src, dstPath, overwrite:true);

        var meta = src + ".meta";
        if (File.Exists(meta))
            File.Copy(meta, dstPath + ".meta", overwrite:true);
    }

    private static List<T> LoadAllByType<T>(string filter) where T : UnityEngine.Object
    {
        var guids = AssetDatabase.FindAssets(filter);
        var list = new List<T>();
        foreach (var g in guids)
        {
            var p = AssetDatabase.GUIDToAssetPath(g);
            var obj = AssetDatabase.LoadAssetAtPath<T>(p);
            if (obj) list.Add(obj);
        }
        return list;
    }

   // --- StatBlock printing helpers (support single or list) ---

private static void WriteStatBlock(StringBuilder sb, StatBlock block)
{
    var totals = new Dictionary<StatType, float>();
    block.AddTo(totals);
    AppendStatsLine(sb, totals);
}

private static void WriteStatBlock(StringBuilder sb, IEnumerable<StatBlock> blocks)
{
    var totals = new Dictionary<StatType, float>();
    if (blocks != null)
    {
        foreach (var b in blocks) b.AddTo(totals);
    }
    AppendStatsLine(sb, totals);
}

private static void AppendStatsLine(StringBuilder sb, Dictionary<StatType, float> totals)
{
    float V(StatType t) => totals.TryGetValue(t, out var v) ? v : 0f;
    sb.AppendLine($"  - Stats: Armor {V(StatType.Armor)}, " +
                  $"PwrGen {V(StatType.PowerGeneration)}, PwrUse {V(StatType.PowerConsumption)}, " +
                  $"Speed {V(StatType.Speed)}, Dmg {V(StatType.Damage)}, Range {V(StatType.Range)}, " +
                  $"Acc {V(StatType.Accuracy)}, Wt {V(StatType.Weight)}");
}

}
#endif
