using System.Numerics;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;
using Wobble.Graphics.Shaders;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Proxy;

public class ShaderProxy
{
    private readonly Shader _shader;

    public ShaderProxy(Shader shader)
    {
        _shader = shader;
    }

    public void SetParameter(string name, object value)
    {
        if (value is double d) value = (float)d;
        _shader.SetParameter(name, value, true);
    }

    public object GetParameter(string name)
    {
        return _shader.Parameters[name];
    }

    public ModChartPropertyFloat ParameterPropFloat(string name) => new(
        () => (float)GetParameter(name),
        v => SetParameter(name, v));

    public ModChartPropertyVector2 ParameterPropVector2(string name) => new(
        () => (Vector2)GetParameter(name),
        v => SetParameter(name, v));

    public ModChartPropertyVector3 ParameterPropVector3(string name) => new(
        () => (Vector3)GetParameter(name),
        v => SetParameter(name, v));

    public ModChartPropertyVector4 ParameterPropVector4(string name) => new(
        () => (Vector4)GetParameter(name),
        v => SetParameter(name, v));
}