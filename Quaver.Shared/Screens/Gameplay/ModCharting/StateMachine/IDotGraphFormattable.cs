using System.IO;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

public interface IDotGraphFormattable
{
    string DotGraphNodeName { get; }
    void WriteDotGraph(TextWriter writer, bool isSubgraph);

}