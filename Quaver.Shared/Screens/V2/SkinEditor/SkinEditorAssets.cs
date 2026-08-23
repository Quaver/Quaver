using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Quaver.Shared.Screens.V2.SkinEditor
{
    internal sealed class SkinEditorAsset
    {
        public string FullPath { get; }

        public string RelativePath { get; }

        public string Folder { get; }

        public string Name => Path.GetFileName(RelativePath);

        public SkinEditorAsset(string fullPath, string relativePath)
        {
            FullPath = fullPath;
            RelativePath = relativePath;
            Folder = Path.GetDirectoryName(relativePath)?.Replace('\\', '/') ?? string.Empty;
        }
    }

    internal static class SkinEditorAssetCatalog
    {
        private static readonly HashSet<string> Extensions =
            new HashSet<string>(new[] { ".png", ".jpg", ".jpeg" }, StringComparer.OrdinalIgnoreCase);

        public static IReadOnlyList<SkinEditorAsset> Scan(string rootDirectory,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(rootDirectory) || !Directory.Exists(rootDirectory))
                return Array.Empty<SkinEditorAsset>();

            var root = Path.GetFullPath(rootDirectory);
            var rootPrefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                             Path.DirectorySeparatorChar;
            var pending = new Stack<string>();
            var assets = new List<SkinEditorAsset>();
            pending.Push(root);

            while (pending.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var directory = pending.Pop();
                try
                {
                    foreach (var child in Directory.EnumerateDirectories(directory))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var info = new DirectoryInfo(child);
                        if ((info.Attributes & FileAttributes.ReparsePoint) == 0)
                            pending.Push(child);
                    }

                    foreach (var file in Directory.EnumerateFiles(directory))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (!Extensions.Contains(Path.GetExtension(file)))
                            continue;

                        var fullPath = Path.GetFullPath(file);
                        if (!fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
                            continue;

                        var relative = fullPath.Substring(rootPrefix.Length).Replace('\\', '/');
                        assets.Add(new SkinEditorAsset(fullPath, relative));
                    }
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }

            return assets.OrderBy(x => x.Folder, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}
