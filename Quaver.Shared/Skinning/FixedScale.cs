using System;

namespace Quaver.Shared.Skinning
{
    /// <summary>
    ///     An attribute for SkinKeys properties, which marks that the property should be rescaled with QuaverGame.SkinScalingFactor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FixedScale : Attribute
    {
    }
}