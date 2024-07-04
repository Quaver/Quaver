using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Shaders;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Proxy;

public class SpriteBatchOptionsProxy
{
    private readonly SpriteBatchOptions _spriteBatchOptions;

    public SpriteBatchOptionsProxy(SpriteBatchOptions spriteBatchOptions)
    {
        _spriteBatchOptions = spriteBatchOptions;
    }

    public Shader Shader
    {
        get => _spriteBatchOptions.Shader;
        set => _spriteBatchOptions.Shader = value;
    }

    public BlendState BlendState
    {
        get => _spriteBatchOptions.BlendState;
        set => _spriteBatchOptions.BlendState = value;
    }

    public RasterizerState RasterizerState
    {
        get => _spriteBatchOptions.RasterizerState;
        set => _spriteBatchOptions.RasterizerState = value;
    }

    public DepthStencilState DepthStencilState
    {
        get => _spriteBatchOptions.DepthStencilState;
        set => _spriteBatchOptions.DepthStencilState = value;
    }

    public SamplerState SamplerState
    {
        get => _spriteBatchOptions.SamplerState;
        set => _spriteBatchOptions.SamplerState = value;
    }

    public SpriteSortMode SortMode
    {
        get => _spriteBatchOptions.SortMode;
        set => _spriteBatchOptions.SortMode = value;
    }
}