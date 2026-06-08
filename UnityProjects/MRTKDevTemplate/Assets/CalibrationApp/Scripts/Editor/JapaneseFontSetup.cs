#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using TMPro;

namespace CalibrationApp.EditorTools
{
    /// <summary>
    /// One-click Japanese font setup. Builds a DYNAMIC TMP font asset from a Japanese
    /// source font and registers it as a global TMP fallback, so every TMP element
    /// (instructions, buttons, marker labels) renders Japanese glyphs.
    ///
    /// Source font resolution order:
    ///   1. Any .ttf/.otf/.ttc already placed in Assets/CalibrationApp/Fonts/
    ///   2. A Windows system Japanese font (Yu Gothic / Meiryo / MS Gothic), which is
    ///      copied into the project so it also ships with the Android (Quest) build.
    ///
    /// Note: Dynamic mode keeps the source font in the build and rasterises glyphs on
    /// demand, which is appropriate for large CJK glyph sets.
    /// </summary>
    public static class JapaneseFontSetup
    {
        private const string FontFolder = "Assets/CalibrationApp/Fonts";
        private const string OutputAssetPath = FontFolder + "/JP Dynamic SDF.asset";

        private static readonly string[] WindowsJpFonts =
        {
            @"C:/Windows/Fonts/YuGothR.ttc",
            @"C:/Windows/Fonts/YuGothM.ttc",
            @"C:/Windows/Fonts/meiryo.ttc",
            @"C:/Windows/Fonts/msgothic.ttc",
            @"C:/Windows/Fonts/msmincho.ttc",
        };

        [MenuItem("Tools/CalibrationApp/Build and Register Japanese Font")]
        public static void BuildAndRegister()
        {
            if (!Directory.Exists(FontFolder))
                Directory.CreateDirectory(FontFolder);

            string sourceAssetPath = FindOrImportSourceFont();
            if (string.IsNullOrEmpty(sourceAssetPath))
            {
                EditorUtility.DisplayDialog(
                    "Japanese Font Setup",
                    "No Japanese font found.\n\nDrop a Japanese .ttf/.otf (e.g. NotoSansJP-Regular.ttf) into:\n" +
                    FontFolder + "\n\nthen run this menu item again.",
                    "OK");
                return;
            }

            var srcFont = AssetDatabase.LoadAssetAtPath<Font>(sourceAssetPath);
            if (srcFont == null)
            {
                Debug.LogError("[JapaneseFontSetup] Could not load Font at " + sourceAssetPath);
                return;
            }

            // Reuse the existing asset if present, otherwise create a dynamic font asset.
            TMP_FontAsset jp = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(OutputAssetPath);
            if (jp == null)
            {
                jp = TMP_FontAsset.CreateFontAsset(
                    srcFont, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024,
                    AtlasPopulationMode.Dynamic, true);
                if (jp == null)
                {
                    Debug.LogError("[JapaneseFontSetup] CreateFontAsset failed.");
                    return;
                }
                jp.name = "JP Dynamic SDF";
                AssetDatabase.CreateAsset(jp, OutputAssetPath);

                foreach (var tex in jp.atlasTextures)
                {
                    if (tex == null) continue;
                    tex.name = "JP Atlas";
                    AssetDatabase.AddObjectToAsset(tex, jp);
                }
                if (jp.material != null)
                {
                    jp.material.name = "JP Material";
                    AssetDatabase.AddObjectToAsset(jp.material, jp);
                }
                AssetDatabase.SaveAssets();
            }

            RegisterFallback(jp);

            EditorUtility.SetDirty(jp);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[JapaneseFontSetup] Japanese font ready and registered as a TMP fallback: " + OutputAssetPath);
        }

        private static string FindOrImportSourceFont()
        {
            // 1. Existing source font in the project Fonts folder.
            string[] exts = { "*.ttf", "*.otf", "*.ttc" };
            foreach (var ext in exts)
            {
                var existing = Directory.GetFiles(FontFolder, ext)
                    .Where(p => !p.EndsWith(".meta"))
                    .Select(p => p.Replace("\\", "/"))
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(existing))
                    return existing;
            }

            // 2. Copy a Windows system Japanese font into the project.
            foreach (var sysPath in WindowsJpFonts)
            {
                if (!File.Exists(sysPath)) continue;
                string dest = FontFolder + "/" + Path.GetFileName(sysPath);
                File.Copy(sysPath, dest, true);
                AssetDatabase.ImportAsset(dest, ImportAssetOptions.ForceUpdate);
                Debug.Log("[JapaneseFontSetup] Imported system Japanese font: " + sysPath + " -> " + dest +
                          "\nCheck licensing before redistributing; Noto Sans JP (OFL) is recommended for distribution.");
                return dest;
            }

            return null;
        }

        private static void RegisterFallback(TMP_FontAsset jp)
        {
            var settings = TMP_Settings.instance;
            if (settings != null)
            {
                var globalFallbacks = TMP_Settings.fallbackFontAssets;
                if (globalFallbacks != null && !globalFallbacks.Contains(jp))
                {
                    globalFallbacks.Add(jp);
                    EditorUtility.SetDirty(settings);
                }

                // Also add to the default font's own fallback table for robustness.
                var def = TMP_Settings.defaultFontAsset;
                if (def != null && def.fallbackFontAssetTable != null && !def.fallbackFontAssetTable.Contains(jp))
                {
                    def.fallbackFontAssetTable.Add(jp);
                    EditorUtility.SetDirty(def);
                }
            }
            else
            {
                Debug.LogWarning("[JapaneseFontSetup] TMP_Settings not found. " +
                                 "Assign the JP font as a fallback manually in Project Settings > TextMesh Pro.");
            }
        }
    }
}
#endif
