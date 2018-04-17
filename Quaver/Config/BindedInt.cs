using System;

namespace Quaver.Config
{
    /// <inheritdoc />
    /// <summary>
    ///     Bindable Int32 value. Contains extra stuff such as Max/Min values.
    /// </summary>
    internal class BindedInt : BindedValue<int>
    {
        /// <summary>
        ///     The min value for this BindedInt
        /// </summary>
        internal int MinValue { get; }

        /// <summary>
        ///     The max value for this BindedInt
        /// </summary>
        internal int MaxValue { get; }

        /// <summary>
        ///     The value of this BindedInt
        /// </summary>
        private int _value;
        internal new int Value
        {
            get => _value;
            set
            {
                if (value < MinValue)
                    _value = MinValue;
                else if (value > MaxValue)
                    _value = MaxValue;
                else
                    _value = value;
                 
                OnValueChanged?.Invoke(this, new BindedValueEventArgs<int>(_value));
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Contructor - Creates a BindedInt
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultVal"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="action"></param>
        public BindedInt(string name, int defaultVal, int min, int max, EventHandler<BindedValueEventArgs<int>> action = null) : base(name, defaultVal, action)
        {
            MinValue = min;
            MaxValue = max;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Lol
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Value.ToString();
    }
}