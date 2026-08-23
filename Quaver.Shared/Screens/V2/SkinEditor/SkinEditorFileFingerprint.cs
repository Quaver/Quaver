using System;
using System.IO;
using System.Security.Cryptography;

namespace Quaver.Shared.Screens.V2.SkinEditor
{
    internal sealed class SkinEditorFileFingerprint
    {
        private readonly bool exists;
        private readonly byte[] hash;

        private SkinEditorFileFingerprint(bool exists, byte[] hash)
        {
            this.exists = exists;
            this.hash = hash;
        }

        public static SkinEditorFileFingerprint Capture(string path)
        {
            if (!File.Exists(path))
                return new SkinEditorFileFingerprint(false, Array.Empty<byte>());

            using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var algorithm = SHA256.Create();
            return new SkinEditorFileFingerprint(true, algorithm.ComputeHash(stream));
        }

        public bool Matches(string path)
        {
            var current = Capture(path);
            if (exists != current.exists || hash.Length != current.hash.Length)
                return false;

            for (var i = 0; i < hash.Length; i++)
            {
                if (hash[i] != current.hash[i])
                    return false;
            }

            return true;
        }
    }
}
