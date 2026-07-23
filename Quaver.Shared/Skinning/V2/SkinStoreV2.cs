using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Assets;
using Wobble.Configuration;
using Wobble.Logging;

namespace Quaver.Shared.Skinning.V2
{
    /// <summary>
    ///     Owns one immutable generation of the selected skin's V2 configuration and custom image assets.
    /// </summary>
    public sealed class SkinStoreV2 : IDisposable
    {
        private static long nextGeneration;

        private readonly object sync = new object();
        private readonly Dictionary<string, Texture2D> textures =
            new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> reportedAssetWarnings =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> warnings = new List<string>();
        private int leaseCount;
        private bool retired;
        private bool disposed;

        public long Generation { get; } = Interlocked.Increment(ref nextGeneration);

        public string RootDirectory { get; }

        public string ConfigPath { get; }

        public YamlConfig<SkinV2Config> Source { get; }

        public SkinV2Config Config { get; }

        public IReadOnlyList<string> Warnings => warnings.AsReadOnly();

        public SkinStoreV2(string rootDirectory)
        {
            RootDirectory = Path.GetFullPath(string.IsNullOrWhiteSpace(rootDirectory) ? "." : rootDirectory);
            ConfigPath = Path.Combine(RootDirectory, "skin.yml");
            Source = YamlConfig<SkinV2Config>.LoadOptional(ConfigPath);
            Config = Source.GetSnapshot();

            foreach (var warning in Source.Warnings)
            {
                var message = $"Skin V2 ({ConfigPath}): {warning}";
                warnings.Add(message);
                Logger.Warning(message, LogType.Runtime, false);
            }
        }

        public SkinStoreV2Lease Acquire()
        {
            lock (sync)
            {
                if (disposed || retired)
                    throw new ObjectDisposedException(nameof(SkinStoreV2));

                leaseCount++;
                return new SkinStoreV2Lease(this);
            }
        }

        /// <summary>
        ///     Writes the required metadata and every property opted into skin-author editing.
        /// </summary>
        public bool TrySaveEditableConfig(out IReadOnlyList<string> errors) =>
            Source.TrySaveMain(Config, out errors);

        internal Texture2D LoadTexture(string relativePath, Texture2D fallback)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return fallback;

            string fullPath;
            try
            {
                if (Path.IsPathRooted(relativePath) || Uri.TryCreate(relativePath, UriKind.Absolute, out _))
                    return WarnAndFallback(relativePath, "absolute paths and URLs are not allowed", fallback);

                fullPath = Path.GetFullPath(Path.Combine(RootDirectory, relativePath));
                var rootWithSeparator = RootDirectory.TrimEnd(Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
                if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
                    return WarnAndFallback(relativePath, "the path leaves the skin directory", fallback);
            }
            catch (Exception e)
            {
                return WarnAndFallback(relativePath, e.Message, fallback);
            }

            lock (sync)
            {
                if (disposed)
                    return fallback;
                if (textures.TryGetValue(fullPath, out var cached) && !cached.IsDisposed)
                    return cached;
            }

            if (!File.Exists(fullPath))
                return WarnAndFallback(relativePath, "the file does not exist", fallback);

            try
            {
                var texture = AssetLoader.LoadTexture2DFromFile(fullPath);
                lock (sync)
                {
                    if (disposed)
                    {
                        texture.Dispose();
                        return fallback;
                    }

                    textures[fullPath] = texture;
                }

                return texture;
            }
            catch (Exception e)
            {
                return WarnAndFallback(relativePath, e.Message, fallback);
            }
        }

        internal void Release()
        {
            lock (sync)
            {
                if (leaseCount > 0)
                    leaseCount--;
                if (retired && leaseCount == 0)
                    DisposeOwnedTextures();
            }
        }

        public void Retire()
        {
            lock (sync)
            {
                retired = true;
                if (leaseCount == 0)
                    DisposeOwnedTextures();
            }
        }

        public void Dispose()
        {
            lock (sync)
            {
                retired = true;
                if (leaseCount == 0)
                    DisposeOwnedTextures();
            }
        }

        private Texture2D WarnAndFallback(string relativePath, string reason, Texture2D fallback)
        {
            lock (sync)
            {
                var key = relativePath + "\0" + reason;
                if (reportedAssetWarnings.Add(key))
                {
                    var message = $"Skin V2 asset '{relativePath}' was ignored because {reason}.";
                    warnings.Add(message);
                    Logger.Warning(message, LogType.Runtime, false);
                }
            }

            return fallback;
        }

        private void DisposeOwnedTextures()
        {
            if (disposed)
                return;

            disposed = true;
            foreach (var texture in textures.Values)
            {
                if (texture != null && !texture.IsDisposed)
                    texture.Dispose();
            }

            textures.Clear();
        }
    }

    public sealed class SkinStoreV2Lease : IDisposable
    {
        private SkinStoreV2 store;

        public long Generation => store?.Generation ?? -1;

        public SkinV2Config Config => store?.Config ?? throw new ObjectDisposedException(nameof(SkinStoreV2Lease));

        internal SkinStoreV2Lease(SkinStoreV2 store) => this.store = store;

        public Texture2D LoadTexture(string relativePath, Texture2D fallback) =>
            store?.LoadTexture(relativePath, fallback) ?? fallback;

        public void Dispose()
        {
            var oldStore = Interlocked.Exchange(ref store, null);
            oldStore?.Release();
        }
    }
}
