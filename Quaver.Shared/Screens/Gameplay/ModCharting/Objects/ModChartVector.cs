using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using Quaver.Shared.Helpers;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaVector3 = Microsoft.Xna.Framework.Vector3;
using XnaVector4 = Microsoft.Xna.Framework.Vector4;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
[System.Diagnostics.DebuggerDisplay("{ToString()}")]
public class ModChartVector
{
    private readonly double[] _values;

    /// <summary>
    ///     Number of components of this vector
    /// </summary>
    public readonly int Dimension;

    /// <summary>
    ///     Number of components of this vector
    /// </summary>
    /// <seealso cref="Dimension"/>
    public int Count => Dimension;

    public ModChartVector(params double[] values)
    {
        _values = values;
        Dimension = values.Length;
    }

    /// <summary>
    ///     Initializes a vector with every component being zero
    /// </summary>
    /// <param name="dimension"></param>
    /// <returns></returns>
    public static ModChartVector Zero(int dimension) => new(new double[dimension]);

    /// <summary>
    ///     Initializes a vector with all component being zero, except one component being one
    /// </summary>
    /// <param name="dimension">Number of components of the vector</param>
    /// <param name="unitDimension">1-Index of the component that is 1</param>
    /// <returns></returns>
    public static ModChartVector Unit(int dimension, int unitDimension)
    {
        var values = new double[dimension];
        values[unitDimension - 1] = 1;
        return new ModChartVector(values);
    }

    /// <summary>
    ///     Returns a unit vector in random direction
    /// </summary>
    /// <param name="dimension"></param>
    /// <returns></returns>
    public static ModChartVector RandomUnit(int dimension)
    {
        var values = new double[dimension];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = RandomHelper.RandomGauss();
        }

        return new ModChartVector(values).Normalise();
    }

    /// <summary>
    ///     Converts the vector to a table
    /// </summary>
    /// <param name="ownerScript"></param>
    /// <returns></returns>
    [MoonSharpHidden]
    public Table ToTable(Script ownerScript) => new(ownerScript, _values.Select(DynValue.NewNumber).ToArray());

    /// <summary>
    ///     Gets or sets the component at the specified <see cref="index"/>
    /// </summary>
    /// <param name="index"></param>
    public double this[int index]
    {
        get => _values[index - 1];
        set => _values[index - 1] = value;
    }

    /// <summary>
    ///     Operates on a continuous range of components of the vector
    /// </summary>
    /// <param name="startInclusive"></param>
    /// <param name="endInclusive"></param>
    public ModChartVector this[int startInclusive, int endInclusive]
    {
        get => GetRange(startInclusive, endInclusive);
        set => SetRange(startInclusive, endInclusive, value);
    }

    /// <summary>
    ///     Gets or sets the components at the indices specified by the <see cref="vectorIndex"/>
    /// </summary>
    /// <param name="vectorIndex"></param>
    /// <seealso cref="GetEntries"/>
    /// <seealso cref="SetEntries"/>
    public DynValue this[ModChartVector vectorIndex]
    {
        get => UserData.Create(GetEntries(vectorIndex._values.Select(v => (int)v).ToArray()));
        set => SetEntries(vectorIndex._values.Select(v => (int)v).ToArray(), value);
    }

    /// <summary>
    ///     Tries to parse the field name shorthand (rgba, xyzw, xXxX, etc.) and outputs the parsed indices
    /// </summary>
    /// <param name="shorthand"></param>
    /// <param name="oneIndexedIndices"></param>
    /// <returns></returns>
    public bool TryParseShorthand(string shorthand, out int[] oneIndexedIndices)
    {
        oneIndexedIndices = new int[shorthand.Length];

        if (Dimension == 0)
            return false;

        for (var index = 0; index < shorthand.Length; index++)
        {
            var c = shorthand[index];
            switch (char.ToLower(c))
            {
                case 'r':
                    oneIndexedIndices[index] = 1;
                    break;
                case 'g' when Dimension >= 2:
                    oneIndexedIndices[index] = 2;
                    break;
                case 'b' when Dimension >= 3:
                    oneIndexedIndices[index] = 3;
                    break;
                case 'a' when Dimension >= 4:
                    oneIndexedIndices[index] = 4;
                    break;
                case 'x':
                    oneIndexedIndices[index] = 1;
                    break;
                case 'y' when Dimension >= 2:
                    oneIndexedIndices[index] = 2;
                    break;
                case 'z' when Dimension >= 3:
                    oneIndexedIndices[index] = 3;
                    break;
                case 'w' when Dimension >= 4:
                    oneIndexedIndices[index] = 4;
                    break;
                default:
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Retrieves the components from <see cref="startInclusive"/> to <see cref="endInclusive"/>, inclusively
    /// </summary>
    /// <param name="startInclusive"></param>
    /// <param name="endInclusive"></param>
    /// <returns></returns>
    public ModChartVector GetRange(int startInclusive, int endInclusive)
    {
        var extractedValues = new double[endInclusive - startInclusive + 1];
        var extractedValueIndex = 0;
        for (var i = startInclusive - 1; i < endInclusive; i++)
        {
            extractedValues[extractedValueIndex] = _values[i];
            extractedValueIndex++;
        }

        return new ModChartVector(extractedValues);
    }

    /// <summary>
    ///     Sets the components from <see cref="startInclusive"/> to <see cref="endInclusive"/>, inclusively, to <see cref="value"/>
    /// </summary>
    /// <param name="startInclusive"></param>
    /// <param name="endInclusive"></param>
    /// <param name="value"></param>
    public void SetRange(int startInclusive, int endInclusive, double value)
    {
        for (var i = startInclusive - 1; i < endInclusive; i++)
        {
            _values[i] = value;
        }
    }

    /// <summary>
    ///     Sets the components from <see cref="startInclusive"/> to <see cref="endInclusive"/>, inclusively, to <see cref="vector"/>
    /// </summary>
    /// <param name="startInclusive"></param>
    /// <param name="endInclusive"></param>
    /// <param name="vector"></param>
    public void SetRange(int startInclusive, int endInclusive, ModChartVector vector)
    {
        for (var i = startInclusive - 1; i < endInclusive; i++)
        {
            _values[i] = vector._values[i - startInclusive];
        }
    }

    /// <summary>
    ///     Sets every component to the <see cref="value"/>
    /// </summary>
    /// <param name="value"></param>
    public void Fill(double value) => SetRange(0, Dimension, value);

    /// <summary>
    ///     Sets every component to 0
    /// </summary>
    public void Clear() => SetRange(0, Dimension, 0);

    /// <summary>
    ///     Returns a vector with selected components at specified <see cref="indices"/>
    /// </summary>
    /// <param name="indices"></param>
    /// <returns></returns>
    public ModChartVector GetEntries(int[] indices)
    {
        var extractedValues = new double[indices.Length];
        for (var i = 0; i < indices.Length; i++)
        {
            var extractIndex = indices[i] - 1;
            extractedValues[i] = _values[extractIndex];
        }

        return new ModChartVector(extractedValues);
    }

    /// <summary>
    ///     Sets the selected components at specified <see cref="indices"/> with a vector or a single number
    /// </summary>
    /// <param name="indices"></param>
    /// <param name="value"></param>
    public void SetEntries(int[] indices, DynValue value)
    {
        if (value.Type == DataType.Number)
        {
            foreach (var index in indices)
            {
                _values[index - 1] = value.Number;
            }
        }
        else
        {
            var vector = value.ToObject<ModChartVector>();
            for (var i = 0; i < indices.Length; i++)
            {
                var valueIndex = indices[i] - 1;
                _values[valueIndex] = vector._values[i];
            }
        }
    }

    /// <summary>
    ///     Shorthand for vector[0]
    /// </summary>
    [MoonSharpHidden]
    public double X
    {
        get => _values[0];
        set => _values[0] = value;
    }


    /// <summary>
    ///     Shorthand for vector[1]
    /// </summary>
    [MoonSharpHidden]
    public double Y
    {
        get => _values[1];
        set => _values[1] = value;
    }


    /// <summary>
    ///     Shorthand for vector[2]
    /// </summary>
    [MoonSharpHidden]
    public double Z
    {
        get => _values[2];
        set => _values[2] = value;
    }


    /// <summary>
    ///     Shorthand for vector[3]
    /// </summary>
    [MoonSharpHidden]
    public double W
    {
        get => _values[3];
        set => _values[3] = value;
    }

    /// <summary>
    ///     Sum of the squares of all the components
    /// </summary>
    /// <returns></returns>
    public double LengthSquared() => _values.Sum(value => value * value);

    /// <inheritdoc cref="LengthSquared()"/>
    public static double LengthSquared(ModChartVector vector) => vector.LengthSquared();

    /// <summary>
    ///     Square root sum of the squares of all the components, i.e. Euclidean distance.
    /// </summary>
    /// <returns></returns>
    public double Length() => Math.Sqrt(LengthSquared());

    /// <inheritdoc cref="Length()"/> 
    public static double Length(ModChartVector vector) => vector.Length();

    /// <summary>
    ///     Distance to the <see cref="other"/> vector, squared
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public double DistanceSquared(ModChartVector other) => (this - other).LengthSquared();

    /// <summary>
    ///     Distance between the two vectors, squared
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    /// <returns></returns>
    public static double DistanceSquared(ModChartVector vector1, ModChartVector vector2) =>
        (vector1 - vector2).LengthSquared();

    /// <summary>
    ///     Distance to the <see cref="other"/> vector
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public double Distance(ModChartVector other) => Math.Sqrt(DistanceSquared(other));

    /// <summary>
    ///     Distance between the two vectors
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    /// <returns></returns>
    public static double Distance(ModChartVector vector1, ModChartVector vector2) =>
        Math.Sqrt(DistanceSquared(vector1, vector2));

    /// <summary>
    ///     Dot product of the two vectors
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public double Dot(ModChartVector other)
    {
        var result = 0.0;
        var minLength = Math.Min(Dimension, other.Dimension);
        for (var i = 0; i < minLength; i++)
        {
            result += _values[i] * other._values[i];
        }

        return result;
    }

    /// <summary>
    ///     Dot product of the two vectors
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    /// <returns></returns>
    public static double Dot(ModChartVector vector1, ModChartVector vector2) => vector1.Dot(vector2);

    /// <summary>
    ///     Cross product of the two vectors. This will give the vector perpendicular to both vectors.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    /// <exception cref="ScriptRuntimeException"></exception>
    /// <remarks>The <see cref="Dimension"/> of the vectors must be 3. In the future, we could consider cross products in higher dimensions,
    /// with n-ary operator</remarks>
    public ModChartVector Cross(ModChartVector other)
    {
        if (Dimension != 3 || other.Dimension != 3)
            throw new ScriptRuntimeException($"Vector must be 3D to calculate cross product");

        return new ModChartVector(Y * other.Z - Z * other.Y, Z * other.X - X * other.Z, X * other.Y - Y * other.X);
    }

    /// <inheritdoc cref="Cross(Quaver.Shared.Screens.Gameplay.ModCharting.Objects.ModChartVector)"/> 
    public static ModChartVector Cross(ModChartVector vector1, ModChartVector vector2) => vector1.Cross(vector2);

    /// <summary>
    ///     Normalise the vector
    /// </summary>
    /// <returns>The vector with the same direction as before, but with a unit length</returns>
    public ModChartVector Normalise()
    {
        return this / Length();
    }

    /// <summary>
    ///     Normalise the <see cref="vector"/>
    /// </summary>
    /// <param name="vector"></param>
    /// <returns>The vector with the same direction as before, but with a unit length</returns>
    public static ModChartVector Normalise(ModChartVector vector) => vector.Normalise();

    /// <summary>
    ///     Reflects the vector over a surface with the <see cref="normal"/>
    /// </summary>
    /// <param name="normal"></param>
    /// <returns></returns>
    public ModChartVector Reflect(ModChartVector normal)
    {
        var dot = Dot(normal);
        return this - 2 * dot * normal;
    }

    /// <summary>
    ///     Reflects the <see cref="vector"/> over a surface with the <see cref="normal"/>
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="normal"></param>
    /// <returns></returns>
    public static ModChartVector Reflect(ModChartVector vector, ModChartVector normal) => vector.Reflect(normal);

    /// <summary>
    ///     Takes the square root of each component
    /// </summary>
    /// <returns></returns>
    public ModChartVector SquareRoot()
    {
        return ScalarOperation(Math.Sqrt);
    }

    /// <summary>
    ///     Takes the square root of each component
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static ModChartVector SquareRoot(ModChartVector vector) => vector.SquareRoot();

    /// <summary>
    ///     Sequential minimum of the components of each vector
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    /// <returns></returns>
    public static ModChartVector Min(ModChartVector vector1, ModChartVector vector2)
    {
        return vector1.SequentialOperation(vector2, Math.Min, double.MaxValue);
    }

    /// <summary>
    ///     Sequential maximum of the components of each vector
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    /// <returns></returns>
    public static ModChartVector Max(ModChartVector vector1, ModChartVector vector2)
    {
        return vector1.SequentialOperation(vector2, Math.Max, double.MinValue);
    }

    /// <summary>
    ///     Sequential clamp of the components of each vector
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static ModChartVector Clamp(ModChartVector vector, ModChartVector min, ModChartVector max)
    {
        return Min(Max(vector, min), max);
    }

    /// <summary>
    ///     Linearly interpolate from <see cref="vector1"/> to <see cref="vector2"/>
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    /// <param name="amount">[0..1] the progress of interpolation</param>
    /// <returns></returns>
    public static ModChartVector Lerp(ModChartVector vector1, ModChartVector vector2, float amount)
    {
        return vector1 * (1 - amount) + vector2 * amount;
    }

    /// <summary>
    ///     Angle between this and the <see cref="other"/> vector
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public double Angle(ModChartVector other) => Angle(this, other);

    /// <summary>
    ///     Angle between the two vectors
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    /// <returns></returns>
    public static double Angle(ModChartVector vector1, ModChartVector vector2)
    {
        return Math.Acos(Dot(Normalise(vector1), Normalise(vector2)));
    }

    public DynValue Unpack()
    {
        return DynValue.NewTuple(_values.Select(DynValue.NewNumber).ToArray());
    }

    private ModChartVector SequentialOperation(ModChartVector other,
        Func<double, double, double> operatorFunction,
        double defaultValue = 0)
    {
        var resultLength = Math.Max(Dimension, other.Dimension);
        var resultValues = new double[resultLength];
        for (var i = 0; i < resultLength; i++)
        {
            if (i >= Dimension)
            {
                resultValues[i] = operatorFunction(defaultValue, other._values[i]);
                continue;
            }

            if (i >= other.Dimension)
            {
                resultValues[i] = operatorFunction(_values[i], defaultValue);
                continue;
            }

            resultValues[i] = operatorFunction(_values[i], other._values[i]);
        }

        return new ModChartVector(resultValues);
    }

    private ModChartVector ScalarOperation(Func<double, double> operatorFunction)
    {
        var resultValues = new double[Dimension];
        for (var i = 0; i < Dimension; i++)
        {
            resultValues[i] = operatorFunction(_values[i]);
        }

        return new ModChartVector(resultValues);
    }

    public static ModChartVector operator +(ModChartVector vector1, ModChartVector vector2)
    {
        return vector1.SequentialOperation(vector2, (a, b) => a + b);
    }

    public static ModChartVector operator -(ModChartVector vector1, ModChartVector vector2)
    {
        return vector1.SequentialOperation(vector2, (a, b) => a - b);
    }

    public static ModChartVector operator -(ModChartVector vector)
    {
        return vector.ScalarOperation(a => -a);
    }

    public static ModChartVector operator *(ModChartVector vector, ModChartVector vector2)
    {
        return vector.SequentialOperation(vector2, (a, b) => a * b);
    }

    public static ModChartVector operator *(ModChartVector vector, double factor)
    {
        return vector.ScalarOperation(a => a * factor);
    }

    public static ModChartVector operator *(double factor, ModChartVector vector)
    {
        return vector.ScalarOperation(a => a * factor);
    }

    public static ModChartVector operator /(ModChartVector vector, ModChartVector vector2)
    {
        return vector.SequentialOperation(vector2, (a, b) => a / b);
    }

    public static ModChartVector operator /(ModChartVector vector, double factor)
    {
        return vector.ScalarOperation(a => a / factor);
    }

    public static ModChartVector operator %(ModChartVector vector, ModChartVector vector2)
    {
        return vector.SequentialOperation(vector2, (a, b) => a % b);
    }

    public static ModChartVector operator %(ModChartVector vector, double factor)
    {
        return vector.ScalarOperation(a => a % factor);
    }

    public Vector2 ToVector2() => new((float)X, (float)Y);

    public Vector3 ToVector3() => new((float)X, (float)Y, (float)Z);

    public Vector4 ToVector4() => new((float)X, (float)Y, (float)Z, (float)W);

    public Color ToColor() => new((float)X, (float)Y, (float)Z, (float)W);

    public XnaVector2 ToXnaVector2() => new((float)X, (float)Y);

    public XnaVector3 ToXnaVector3() => new((float)X, (float)Y, (float)Z);

    public XnaVector4 ToXnaVector4() => new((float)X, (float)Y, (float)Z, (float)W);

    public override string ToString()
    {
        return $"<{string.Join(", ", _values.Select(v => v.ToString(CultureInfo.InvariantCulture)))}>";
    }
}