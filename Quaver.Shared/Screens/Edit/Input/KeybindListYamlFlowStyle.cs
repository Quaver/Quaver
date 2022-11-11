using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace Quaver.Shared.Screens.Edit.Input
{
    public sealed class KeybindListYamlFlowStyle : ChainedEventEmitter
    {
        public KeybindListYamlFlowStyle(IEventEmitter nextEmitter) : base(nextEmitter)
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