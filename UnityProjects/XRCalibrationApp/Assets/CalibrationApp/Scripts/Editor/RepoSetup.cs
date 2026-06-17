#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CalibrationApp.EditorTools
{
    /// <summary>
    /// Writes (or merges into) a .gitignore at the Unity project root so generated
    /// folders, IDE files, captured screenshots, and any proprietary font files are
    /// never committed. Safe to re-run: only missing entries are appended.
    /// </summary>
    public static class RepoSetup
    {
        // Patterns only (no inline comments) so merge stays clean on re-run.
        private static readonly string[] RequiredEntries =
        {
            "[Ll]ibrary/",
            "[Tt]emp/",
            "[Oo]bj/",
            "[Bb]uild/",
            "[Bb]uilds/",
            "[Ll]ogs/",
            "[Uu]serSettings/",
            "[Mm]emoryCaptures/",
            "[Cc]rashlytics/",
            ".vs/",
            ".idea/",
            "*.csproj",
            "*.sln",
            "*.user",
            ".DS_Store",
            // CalibrationApp-specific:
            "/[Aa]ssets/Screenshots/",
            "/[Aa]ssets/Screenshots.meta",
            // Never commit proprietary fonts (e.g. Windows Yu Gothic .ttc):
            "/[Aa]ssets/CalibrationApp/Fonts/*.ttc",
            "/[Aa]ssets/CalibrationApp/Fonts/*.ttc.meta",
        };

        [MenuItem("Tools/CalibrationApp/Write or Update .gitignore")]
        public static void WriteGitignore()
        {
            string root = Directory.GetParent(Application.dataPath).FullName;
            string path = Path.Combine(root, ".gitignore");

            var present = new HashSet<string>();
            bool existed = File.Exists(path);
            if (existed)
                foreach (var line in File.ReadAllLines(path))
                    present.Add(line.Trim());

            var missing = new List<string>();
            foreach (var entry in RequiredEntries)
                if (!present.Contains(entry.Trim()))
                    missing.Add(entry);

            if (missing.Count == 0)
            {
                Debug.Log("[RepoSetup] .gitignore already up to date: " + path);
                return;
            }

            using (var sw = new StreamWriter(path, append: true))
            {
                if (existed) sw.WriteLine();
                sw.WriteLine("# ===== Added by CalibrationApp =====");
                foreach (var entry in missing)
                    sw.WriteLine(entry);
            }

            Debug.Log("[RepoSetup] " + (existed ? "Updated" : "Created") + " .gitignore at " + path +
                      " (added " + missing.Count + " entries: " + string.Join(", ", missing) + ").");
        }
    }
}
#endif
