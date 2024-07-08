using System.Collections.Generic;
using MoonSharp.Interpreter;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
public class ModChartLayers : ModChartGlobalVariable
{
    private readonly LayerManager _layerManager;

    public ModChartLayers(ElementAccessShortcut shortcut) : base(shortcut)
    {
        _layerManager = shortcut.GameplayPlayfieldKeys.GameplayLayerManager;
    }

    public Layer this[string name] => _layerManager.Layers.GetValueOrDefault(name);

    public void RequireOrder(Layer[] layerOrder) => LayerManager.RequireOrder(layerOrder);

    [MoonSharpHidden]
    public Layer NewLayer(string name) => _layerManager.NewLayer(name);

    public void Dump() => _layerManager.Dump();
}