using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Quaver.Shared.Skinning.V2;
using Wobble.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Quaver.Shared.Screens.V2.SkinEditor
{
    internal sealed class SkinEditorProperty
    {
        private static readonly Regex WordBoundary =
            new Regex("(?<!^)([A-Z])", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly PropertyInfo[] chain;

        public string Path { get; }

        public string Name { get; }

        public Type ValueType => chain[chain.Length - 1].PropertyType;

        public bool IsAssetPath => Attribute<SkinAssetPathAttribute>() != null;

        public bool IsColor => Attribute<SkinColorAttribute>() != null;

        public bool IsFont => Attribute<SkinFontAttribute>() != null;

        public bool IsGradientStops => Attribute<SkinV2GradientStopsAttribute>() != null;

        public RangeAttribute Range => Attribute<RangeAttribute>();

        public IReadOnlyList<ValidationAttribute> Validators { get; }

        internal SkinEditorProperty(string path, IReadOnlyList<PropertyInfo> properties)
        {
            Path = path;
            chain = properties.ToArray();
            Name = WordBoundary.Replace(chain[chain.Length - 1].Name, " $1");
            Validators = chain[chain.Length - 1].GetCustomAttributes<ValidationAttribute>(true).ToArray();
        }

        public T Attribute<T>() where T : Attribute =>
            chain[chain.Length - 1].GetCustomAttribute<T>(true);

        public object GetValue(SkinV2Config root)
        {
            object current = root;
            foreach (var property in chain)
            {
                if (current == null)
                    return null;
                current = property.GetValue(current);
            }

            return current;
        }

        public bool TrySetText(SkinV2Config root, string text, out string error)
        {
            if (!TryConvert(text, out var converted, out error))
                return false;

            return TrySetValue(root, converted, out error);
        }

        public bool TrySetValue(SkinV2Config root, object value, out string error)
        {
            object parent = root;
            for (var i = 0; i < chain.Length - 1; i++)
            {
                parent = chain[i].GetValue(parent);
                if (parent == null)
                {
                    error = "The containing configuration value is missing.";
                    return false;
                }
            }

            var property = chain[chain.Length - 1];
            var results = new List<ValidationResult>();
            var context = new ValidationContext(parent) { MemberName = property.Name };
            if (!Validator.TryValidateValue(value, context, results, Validators))
            {
                error = string.Join("; ", results.Select(x => x.ErrorMessage));
                return false;
            }

            property.SetValue(parent, value);
            error = null;
            return true;
        }

        private bool TryConvert(string text, out object value, out string error)
        {
            var type = Nullable.GetUnderlyingType(ValueType) ?? ValueType;
            try
            {
                if (type == typeof(string))
                    value = text ?? string.Empty;
                else if (type == typeof(bool))
                    value = bool.Parse(text);
                else if (type.IsEnum)
                    value = Enum.Parse(type, text, true);
                else if (type == typeof(int))
                    value = int.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);
                else if (type == typeof(float))
                    value = float.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
                else if (type == typeof(double))
                    value = double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
                else if (type == typeof(long))
                    value = long.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);
                else
                {
                    value = null;
                    error = $"Values of type {type.Name} require a specialized editor.";
                    return false;
                }
            }
            catch (Exception)
            {
                value = null;
                error = $"'{text}' is not a valid {Name.ToLowerInvariant()} value.";
                return false;
            }

            error = null;
            return true;
        }
    }

    internal static class SkinEditorConfigDescriptor
    {
        public static IReadOnlyList<SkinEditorProperty> Discover()
        {
            var result = new List<SkinEditorProperty>();
            Walk(typeof(SkinV2Config), string.Empty, false, new List<PropertyInfo>(), result);
            return result;
        }

        private static void Walk(Type type, string parentPath, bool parentEditable,
            List<PropertyInfo> chain, ICollection<SkinEditorProperty> result)
        {
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (property.GetMethod == null || property.SetMethod == null ||
                    property.GetIndexParameters().Length != 0 ||
                    property.GetCustomAttribute<YamlIgnoreAttribute>() != null)
                    continue;

                var name = property.GetCustomAttribute<YamlMemberAttribute>()?.Alias;
                if (string.IsNullOrWhiteSpace(name))
                    name = property.Name;

                var path = string.IsNullOrEmpty(parentPath) ? name : parentPath + "." + name;
                var explicitlyEditable = property.GetCustomAttribute<ConfigEditableAttribute>() != null;
                var editable = parentEditable || explicitlyEditable;
                chain.Add(property);

                if (IsNested(property.PropertyType))
                    Walk(property.PropertyType, path, editable, chain, result);
                else if (editable)
                    result.Add(new SkinEditorProperty(path, chain));

                chain.RemoveAt(chain.Count - 1);
            }
        }

        private static bool IsNested(Type type) =>
            type.IsClass && type != typeof(string) && !typeof(IEnumerable).IsAssignableFrom(type);
    }

    internal sealed class SkinEditorSession
    {
        private static readonly ISerializer Serializer = new SerializerBuilder()
            .WithNamingConvention(new PascalCaseNamingConvention())
            .Build();

        private string initialSerialized;
        private bool workingDirty;

        public SkinV2Config Initial { get; private set; }

        public SkinV2Config Working { get; private set; }

        public SkinV2Config Defaults { get; } = new SkinV2Config();

        public IReadOnlyList<SkinEditorProperty> Properties { get; } = SkinEditorConfigDescriptor.Discover();

        public SkinEditorProperty FocusedAssetProperty { get; set; }

        public bool HasInvalidInput { get; set; }

        public string StagedWorkshopPreviewPath { get; set; }

        public bool HasStagedWorkshopPreview =>
            !string.IsNullOrWhiteSpace(StagedWorkshopPreviewPath);

        public bool IsDirty => HasStagedWorkshopPreview || workingDirty;

        public SkinEditorSession(SkinStoreV2 store)
        {
            Initial = store.CreateEditableSnapshot();
            Working = store.CreateEditableSnapshot();
            initialSerialized = Serializer.Serialize(Initial);
        }

        public IReadOnlyList<SkinEditorProperty> GetProperties(string componentPath) =>
            Properties.Where(x => x.Path == componentPath ||
                                  x.Path.StartsWith(componentPath + ".", StringComparison.Ordinal))
                .ToArray();

        public void RefreshDirtyState() =>
            workingDirty = initialSerialized != Serializer.Serialize(Working);

        public void RestoreInitial()
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new PascalCaseNamingConvention())
                .Build();
            Working = deserializer.Deserialize<SkinV2Config>(initialSerialized);
            FocusedAssetProperty = null;
            StagedWorkshopPreviewPath = null;
            HasInvalidInput = false;
            workingDirty = false;
        }

        public void AcceptWorkingAsBaseline()
        {
            initialSerialized = Serializer.Serialize(Working);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new PascalCaseNamingConvention())
                .Build();
            Initial = deserializer.Deserialize<SkinV2Config>(initialSerialized);
            StagedWorkshopPreviewPath = null;
            workingDirty = false;
        }
    }
}
