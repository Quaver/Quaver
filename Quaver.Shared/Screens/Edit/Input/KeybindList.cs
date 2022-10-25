using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace Quaver.Shared.Screens.Edit.Input
{
    [Serializable]
    public class KeybindList : HashSet<Keybind>
    {
        public KeybindList() => Add(new Keybind(Keys.None));

        public KeybindList(string notation) : base(notation.Split(',').Select(s => new Keybind(s)))
        {
        }

        public KeybindList(Keys key) => Add(new Keybind(key));
        public KeybindList(KeyModifiers mod, Keys key) => Add(new Keybind(mod, key));
        public KeybindList(Keybind keybind) => Add(keybind);

        public KeybindList(IEnumerable<Keybind> keybinds) : base(keybinds)
        {
        }

        new public void Add(Keybind keybind)
        {
            if (keybind == Keybind.None) return;

            if (Count == 1 && Contains(Keybind.None)) Remove(Keybind.None);
            base.Add(keybind);
        }

        new public bool Remove(Keybind keybind)
        {
            if (keybind == Keybind.None) return false;

            var result = base.Remove(keybind);
            if (Count == 0) Add(Keybind.None);

            return result;
        }

        public bool IsUniqueKeypress() => this.Any(k => k.IsUniqueKeypress());
        public bool IsDown() => this.Any(k => k.IsDown());
        public bool IsUp() => this.Any(k => k.IsUp());
    }

    public class FlowStyleKeybinds : ChainedEventEmitter
    {
        public FlowStyleKeybinds(IEventEmitter nextEmitter) : base(nextEmitter)
        {
        }

        public override void Emit(SequenceStartEventInfo eventInfo, IEmitter emitter)
        {
            if (typeof(IEnumerable<Keybind>).IsAssignableFrom(eventInfo.Source.Type))
                eventInfo = new SequenceStartEventInfo(eventInfo.Source) {Style = SequenceStyle.Flow};
            nextEmitter.Emit(eventInfo, emitter);
        }
    }
}