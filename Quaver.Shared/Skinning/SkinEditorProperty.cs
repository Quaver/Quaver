namespace Quaver.Shared.Skinning
{
    internal sealed class SkinEditorProperty : System.IEquatable<SkinEditorProperty>
    {
        public string Group { get; }

        public string Section { get; }

        public string Key { get; }

        public string Label { get; }

        public string Placeholder { get; }

        public string DefaultValue { get; }

        public string Id => $"{Section}.{Key}";

        public SkinEditorProperty(string group, string section, string key, string label, string placeholder,
            string defaultValue = "")
        {
            Group = group;
            Section = section;
            Key = key;
            Label = label;
            Placeholder = placeholder;
            DefaultValue = defaultValue;
        }

        public bool Equals(SkinEditorProperty other) => other != null && Id == other.Id;

        public override bool Equals(object obj) => obj is SkinEditorProperty other && Equals(other);

        public override int GetHashCode() => Id.GetHashCode();
    }
}
