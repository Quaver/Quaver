using System;

namespace Quaver.Config.Bindings
{
    /// <summary>
    ///     Generic class for values that you want to keep track of when they change.
    ///     This is usually used for configuration variables.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class BindedValue<T>
    {
        /// <summary>
        ///     Emitted when this value changes.
        /// </summary>
        internal EventHandler<BindedValueEventArgs<T>> OnValueChanged;

        /// <summary>
        ///     The binded value string of this property.
        /// </summary>
        internal BindedValueString Name { get; }

        /// <summary>
        ///     The containing binded value.
        /// </summary>
        private T _value;
        internal T Value
        {
            get => _value;
            set
            {
                _value = value;
                OnValueChanged?.Invoke(this, new BindedValueEventArgs<T>(value));
            }
        }

        /// <summary>
        ///     Constructor that takes in an action to call upon changing the value Bindable.
        /// </summary>
        /// <param name="action"></param>
        public BindedValue(BindedValueString s, EventHandler<BindedValueEventArgs<T>> action = null)
        {
            Name = s;
            
            if (action != null)
                OnValueChanged += action;
        }

        /// <summary>
        ///     Used as a failsafe. If trying to ToString() a BindedValue itself, it'll throw an exception.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public override string ToString()
        {
            throw new Exception("BindedValues cannot be written to a string!");
        }
    }
    
    /// <summary>
    ///     EventArgs containing the value that was changed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class BindedValueEventArgs<T> : EventArgs
    {        
        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="value"></param>
        internal BindedValueEventArgs(T value)
        {
            Value = value;
        }
        
        /// <summary>
        ///     The value passed when 
        /// </summary>
        internal T Value { get; set; }
    }
}