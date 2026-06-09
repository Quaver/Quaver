using System;
using System.Collections.Generic;
using System.IO;
using Wobble.IO;
using Wobble.Platform;

namespace Quaver
{
    public static class NativeFileAssociationRegistrar
    {
        public static void Register()
        {
            var fileAssociations = CreateFileAssociations();
            var applicationIconPath = GetFileIconPath(GetNativeIconFileName("quaver-logo"));

            Utils.NativeUtils.RegisterURIScheme("quaver", "Quaver");
            ExtractNativeIcons(fileAssociations, applicationIconPath);
            Utils.NativeUtils.RegisterFileAssociations(fileAssociations, applicationIconPath);
        }

        private static void ExtractNativeIcons(IReadOnlyDictionary<string, FileAssociation> fileAssociations,
            string applicationIconPath)
        {
            var iconDirectory = GetFileIconDirectory();
            Directory.CreateDirectory(iconDirectory);

            using (var resources = new DllResourceStore("Quaver.Resources.dll"))
            {
                foreach (var association in fileAssociations)
                {
                    var iconFileName = Path.GetFileName(association.Value.IconPath);
                    var icon = resources.Get($"Quaver.Resources/Icons/{iconFileName}");

                    if (icon == null)
                        continue;

                    File.WriteAllBytes(association.Value.IconPath, icon);
                }

                var applicationIconFileName = Path.GetFileName(applicationIconPath);
                var applicationIcon = resources.Get($"Quaver.Resources/Icons/{applicationIconFileName}");

                if (applicationIcon != null)
                    File.WriteAllBytes(applicationIconPath, applicationIcon);
            }
        }

        private static string GetFileIconPath(string iconFileName) => Path.Combine(GetFileIconDirectory(), iconFileName);

        private static string GetFileIconDirectory()
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Quaver", "File Icons");

        private static string GetNativeIconFileName(string fileType)
        {
            if (OperatingSystem.IsMacOS())
                return $"{fileType}.icns";

            if (OperatingSystem.IsLinux())
                return $"{fileType}.png";

            return $"{fileType}.ico";
        }

        private static Dictionary<string, FileAssociation> CreateFileAssociations()
            => new Dictionary<string, FileAssociation>
            {
                {
                    ".qp",
                    new FileAssociation
                    {
                        ProgId = "Quaver.qp",
                        FriendlyName = "Quaver Mapset",
                        IconPath = GetFileIconPath(GetNativeIconFileName("qp")),
                        MimeType = "application/x-quaver-qp"
                    }
                },
                {
                    ".qpl",
                    new FileAssociation
                    {
                        ProgId = "Quaver.qpl",
                        FriendlyName = "Quaver Playlist",
                        IconPath = GetFileIconPath(GetNativeIconFileName("qpl")),
                        MimeType = "application/x-quaver-qpl"
                    }
                },
                {
                    ".qr",
                    new FileAssociation
                    {
                        ProgId = "Quaver.qr",
                        FriendlyName = "Quaver Replay",
                        IconPath = GetFileIconPath(GetNativeIconFileName("qr")),
                        MimeType = "application/x-quaver-qr"
                    }
                },
                {
                    ".qs",
                    new FileAssociation
                    {
                        ProgId = "Quaver.qs",
                        FriendlyName = "Quaver Skin",
                        IconPath = GetFileIconPath(GetNativeIconFileName("qs")),
                        MimeType = "application/x-quaver-qs"
                    }
                }
            };
    }
}
