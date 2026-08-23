using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Quaver.Shared.Assets;

namespace Quaver.Shared.Skinning.V2
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SkinColorAttribute : ValidationAttribute
    {
        private static readonly Regex Pattern = new Regex("^#[0-9a-fA-F]{6}([0-9a-fA-F]{2})?$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public override bool IsValid(object value) => value is string text && Pattern.IsMatch(text);

        public override string FormatErrorMessage(string name) =>
            $"{name} must use #RRGGBB or #RRGGBBAA format.";
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SkinAssetPathAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null || value is string { Length: 0 })
                return true;
            if (!(value is string path) || path.Length > 1024 || Path.IsPathRooted(path))
                return false;
            if (Uri.TryCreate(path, UriKind.Absolute, out _))
                return false;

            return !path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)
                .Any(part => part == "..");
        }

        public override string FormatErrorMessage(string name) =>
            $"{name} must be an optional skin-root-relative path of at most 1024 characters.";
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SkinFontAttribute : ValidationAttribute
    {
        private static readonly string[] AvailableFonts =
        {
            Fonts.InterRegular,
            Fonts.InterMedium,
            Fonts.InterSemiBold,
            Fonts.InterBold,
            Fonts.InterLight,
            Fonts.InterHeavy,
            Fonts.InterBlack
        };

        public override bool IsValid(object value) => value is string font && AvailableFonts.Contains(font);

        public override string FormatErrorMessage(string name) =>
            $"{name} must be one of: {string.Join(", ", AvailableFonts)}.";
    }
}
