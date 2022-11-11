using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Quaver.Shared.Screens.Edit.Input
{
    internal sealed class KeybindYamlTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(Keybind);

        public object ReadYaml(IParser parser, Type type)
        {
            if (!parser.Accept<Scalar>()) return Keybind.None;
            var scalar = parser.Current as Scalar;
            if (scalar == null) return Keybind.None;

            var result = new Keybind(scalar.Value);
            parser.MoveNext();

            return result;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type) =>
            emitter.Emit(new Scalar(null, ((Keybind)value).ToString()));
    }
}