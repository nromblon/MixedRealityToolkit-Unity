#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TextCore.LowLevel;
using TMPro;

namespace CalibrationApp.EditorTools
{
    /// <summary>
    /// One-click Japanese font setup using Noto Sans JP (SIL Open Font License).
    /// Builds a DYNAMIC TMP font asset and registers it as a global TMP fallback so
    /// every TMP element (instructions, buttons, marker labels) renders Japanese.
    ///
    /// Source font resolution order:
    ///   1. Any .ttf/.otf already placed in Assets/CalibrationApp/Fonts/
    ///   2. Otherwise, download Noto Sans JP (OFL) from the official Google Fonts repo.
    ///
    /// This intentionally does NOT fall back to a Windows system font (e.g. Yu Gothic),
    /// because those are proprietary and must not be committed to a public repository.
    /// </summary>
    public static class JapaneseFontSetup
    {
        private const string FontFolder = "Assets/CalibrationApp/Fonts";
        private const string OutputAssetPath = FontFolder + "/JP Dynamic SDF.asset";
        private const string NotoFileName = "NotoSansJP-Regular.ttf";

        // Official OFL Noto Sans JP (variable font) mirrored on Google's fonts repo + jsDelivr.
        private static readonly string[] NotoUrls =
        {
            "https://cdn.jsdelivr.net/gh/google/fonts@main/ofl/notosansjp/NotoSansJP%5Bwght%5D.ttf",
            "https://raw.githubusercontent.com/google/fonts/main/ofl/notosansjp/NotoSansJP%5Bwght%5D.ttf",
        };

        [MenuItem("Tools/CalibrationApp/Build and Register Japanese Font (Noto)")]
        public static void BuildAndRegister()
        {
            if (!Directory.Exists(FontFolder))
                Directory.CreateDirectory(FontFolder);

            string sourceAssetPath = FindProjectFont() ?? DownloadNoto();
            if (string.IsNullOrEmpty(sourceAssetPath))
            {
                EditorUtility.DisplayDialog(
                    "Japanese Font Setup",
                    "Could not obtain a Japanese font.\n\nEither allow editor internet access, or download Noto Sans JP (OFL) manually and drop the .ttf into:\n" +
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

            // Force a clean rebuild so the asset always reflects the current source font.
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(OutputAssetPath);
            if (existing != null)
            {
                UnregisterFallback(existing);
                AssetDatabase.DeleteAsset(OutputAssetPath);
            }

            var jp = TMP_FontAsset.CreateFontAsset(
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

            RegisterFallback(jp);

            EditorUtility.SetDirty(jp);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[JapaneseFontSetup] Noto Sans JP ready and registered as a TMP fallback: " + OutputAssetPath);
        }

        private static string FindProjectFont()
        {
            foreach (var ext in new[] { "*.ttf", "*.otf" })
            {
                var existing = Directory.GetFiles(FontFolder, ext)
                    .Where(p => !p.EndsWith(".meta"))
                    .Select(p => p.Replace("\\", "/"))
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(existing))
                    return existing;
            }
            return null;
        }

        private static string DownloadNoto()
        {
            string dest = FontFolder + "/" + NotoFileName;
            foreach (var url in NotoUrls)
            {
                try
                {
                    using (var req = UnityWebRequest.Get(url))
                    {
                        var op = req.SendWebRequest();
                        while (!op.isDone)
                            System.Threading.Thread.Sleep(50);

                        var data = req.downloadHandler != null ? req.downloadHandler.data : null;
                        if (req.result == UnityWebRequest.Result.Success && data != null && data.Length > 50000)
                        {
                            File.WriteAllBytes(dest, data);
                            AssetDatabase.ImportAsset(dest, ImportAssetOptions.ForceUpdate);
                            Debug.Log("[JapaneseFontSetup] Downloaded Noto Sans JP (OFL) from " + url +
                                      " (" + data.Length + " bytes). Remember to include the OFL license in your repo.");
                            return dest;
                        }

                        int len = data != null ? data.Length : 0;
                        Debug.LogWarning("[JapaneseFontSetup] Download failed from " + url + " : " + req.error + " (" + len + " bytes)");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("[JapaneseFontSetup] Download error from " + url + " : " + e.Message);
                }
            }
            return null;
        }

        private static void RegisterFallback(TMP_FontAsset jp)
        {
            var settings = TMP_Settings.instance;
            if (settings == null)
            {
                Debug.LogWarning("[JapaneseFontSetup] TMP_Settings not found. " +
                                 "Assign the JP font as a fallback manually in Project Settings > TextMesh Pro.");
                return;
            }

            var globals = TMP_Settings.fallbackFontAssets;
            if (globals != null)
            {
                globals.RemoveAll(f => f == null);
                if (!globals.Contains(jp)) globals.Add(jp);
                EditorUtility.SetDirty(settings);
            }

            var def = TMP_Settings.defaultFontAsset;
            if (def != null && def.fallbackFontAssetTable != null)
            {
                def.fallbackFontAssetTable.RemoveAll(f => f == null);
                if (!def.fallbackFontAssetTable.Contains(jp)) def.fallbackFontAssetTable.Add(jp);
                EditorUtility.SetDirty(def);
            }
        }

        private static void UnregisterFallback(TMP_FontAsset jp)
        {
            var globals = TMP_Settings.fallbackFontAssets;
            if (globals != null) globals.RemoveAll(f => f == null || f == jp);

            var def = TMP_Settings.defaultFontAsset;
            if (def != null && def.fallbackFontAssetTable != null)
                def.fallbackFontAssetTable.RemoveAll(f => f == null || f == jp);
        }
    }
}
#endif
