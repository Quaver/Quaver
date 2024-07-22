using System;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
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
    ///     Converts the vector to a table
    /// </summary>
    /// <param name="ownerScript"></param>
    /// <returns></returns>
    [MoonSharpHidden]
    public Table ToTable(Script ownerScript) => new(ownerScript, _values.Select(DynValue.NewNumber).ToArray());

    public double this[int index]
    {
        get => _values[index - 1];
        set => _values[index - 1] = value;
    }

    /// <summary>
    ///     Selects components of the indices and returns a vector of the two.
    ///     If set with a number, both component will be set the value
    /// </summary>
    /// <param name="i1"></param>
    /// <param name="i2"></param>
    public DynValue this[int i1, int i2]
    {
        get => UserData.Create(GetEntries(new[] { i1, i2 }));
        set => SetEntries(new[] { i1, i2 }, value);
    }

    /// <inheritdoc cref="this[int, int]"/>
    public DynValue this[int i1, int i2, int i3]
    {
        get => UserData.Create(GetEntries(new[] { i1, i2, i3 }));
        set => SetEntries(new[] { i1, i2, i3 }, value);
    }

    /// <inheritdoc cref="this[int, int]"/>
    public DynValue this[int i1, int i2, int i3, int i4]
    {
        get => UserData.Create(GetEntries(new[] { i1, i2, i3, i4 }));
        set => SetEntries(new[] { i1, i2, i3, i4 }, value);
    }

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

    public void SetRange(int startInclusive, int endInclusive, double value)
    {
        for (var i = startInclusive - 1; i < endInclusive; i++)
        {
            _values[i] = value;
        }
    }

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
    public double X
    {
        get => _values[0];
        set => _values[0] = value;
    }


    /// <summary>
    ///     Shorthand for vector[1]
    /// </summary>
    public double Y
    {
        get => _values[1];
        set => _values[1] = value;
    }


    /// <summary>
    ///     Shorthand for vector[2]
    /// </summary>
    public double Z
    {
        get => _values[2];
        set => _values[2] = value;
    }


    /// <summary>
    ///     Shorthand for vector[3]
    /// </summary>
    public double W
    {
        get => _values[3];
        set => _values[3] = value;
    }

    public double LengthSquared() => _values.Sum(value => value * value);

    public static double LengthSquared(ModChartVector vector) => vector.LengthSquared();

    public double Length() => Math.Sqrt(LengthSquared());

    public static double Length(ModChartVector vector) => vector.Length();

    public double DistanceSquared(ModChartVector other) => (this - other).LengthSquared();

    public static double DistanceSquared(ModChartVector vector1, ModChartVector vector2) =>
        (vector1 - vector2).LengthSquared();

    public double Distance(ModChartVector other) => Math.Sqrt(DistanceSquared(other));

    public static double Distance(ModChartVector vector1, ModChartVector vector2) =>
        Math.Sqrt(DistanceSquared(vector1, vector2));

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

    public static double Dot(ModChartVector vector1, ModChartVector vector2) => vector1.Dot(vector2);

    public ModChartVector Cross(ModChartVector other)
    {
        if (Dimension != 3 || other.Dimension != 3)
            throw new ScriptRuntimeException($"Vector must be 3D to calculate cross product");

        return new ModChartVector(Y * other.Z - Z * other.Y, Z * other.X - X * other.Z, X * other.Y - Y * other.X);
    }

    public static ModChartVector Cross(ModChartVector vector1, ModChartVector vector2) => vector1.Cross(vector2);

    public ModChartVector Normalise()
    {
        return this / Length();
    }

    public static ModChartVector Normalise(ModChartVector vector) => vector.Normalise();

    public ModChartVector Reflect(ModChartVector normal)
    {
        var dot = Dot(normal);
        return this - 2 * dot * normal;
    }

    public static ModChartVector Reflect(ModChartVector vector, ModChartVector normal) => vector.Reflect(normal);

    public ModChartVector SquareRoot()
    {
        return ScalarOperation(Math.Sqrt);
    }

    public static ModChartVector SquareRoot(ModChartVector vector) => vector.SquareRoot();

    public static ModChartVector Min(ModChartVector vector1, ModChartVector vector2)
    {
        return vector1.SequentialOperation(vector2, Math.Min, double.MaxValue);
    }

    public static ModChartVector Max(ModChartVector vector1, ModChartVector vector2)
    {
        return vector1.SequentialOperation(vector2, Math.Max, double.MinValue);
    }

    public static ModChartVector Clamp(ModChartVector vector, ModChartVector min, ModChartVector max)
    {
        return Min(Max(vector, min), max);
    }

    public static ModChartVector Lerp(ModChartVector vector1, ModChartVector vector2, float amount)
    {
        return vector1 * (1 - amount) + vector2 * amount;
    }

    public double Angle(ModChartVector other) => Angle(this, other);

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