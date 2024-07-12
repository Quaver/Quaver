using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;
using Wobble.Graphics.Shaders;
using Vector2 = Microsoft.Xna.Framework.Vector2;

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

    public ModChartPropertyXnaVector2 ParameterPropVector2(string name) => new(
        () => (Vector2)GetParameter(name),
        v => SetParameter(name, v));

    public ModChartPropertyFloat GreyscaleProp => ParameterPropFloat("GreyscaleStrength");
    public ModChartPropertyXnaVector2 RedOffsetProp => ParameterPropVector2("ChromaticAberrationRedOffset");
    public ModChartPropertyXnaVector2 GreenOffsetProp => ParameterPropVector2("ChromaticAberrationGreenOffset");
    public ModChartPropertyXnaVector2 BlueOffsetProp => ParameterPropVector2("ChromaticAberrationBlueOffset");
    public ModChartPropertyXnaVector2 MosaicBlockSizeProp => ParameterPropVector2("MosaicBlockSize");
}