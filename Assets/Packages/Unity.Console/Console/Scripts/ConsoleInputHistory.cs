// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Wenzil.Console
{
    /// <summary>
    /// Utility for caching and navigating recently executed console commands. 
    /// </summary>
    public class ConsoleInputHistory
    {
        // Input history from most recent to oldest
        private List<string> _inputHistory;
        public int maxCapacity;

        // The go-to input entry index. The one to navigate to when first navigating up. It's usually the one most recently navigated-to.
        private int _currentInput;

        private bool _isNavigating;

        public ConsoleInputHistory(int maxCapacity)
        {
            _inputHistory = new List<string>(maxCapacity);
            this.maxCapacity = maxCapacity;
        }

        /// <summary>
        /// Navigates up or down the input history
        /// </summary>
        /// <returns>The navigated-to input entry</returns>
        public string Navigate(bool up)
        {
            bool down = !up;

            // When first navigating up (if there is an input entry), navigate to the go-to input entry (we actually are already there)
            // If navigating up again, navigate to the input entry ABOVE (if there is one) the go-to input entry
            // If navigating down at any time, navigate to the input entry BELOW (if there is one) the go-to input entry
            if (!_isNavigating)
                _isNavigating = (up && _inputHistory.Count > 0) || (down && _currentInput > 0);
            else if (up)
                _currentInput++;
            if (down)
                _currentInput--;

            _currentInput = Mathf.Clamp(_currentInput, 0, _inputHistory.Count - 1);

            // Return the navigated-to input entry
            if (_isNavigating)
                return _inputHistory[_currentInput];
            else
                return "";
        }

        /// <summary>
        /// Add a new input entry to the input history.
        /// </summary>
        public void AddNewInputEntry(string input)
        {
            // Give the opportunity to "first" navigate up again, so that we can resume navigating up from the go-to input entry
            _isNavigating = false;

            // Don't add the same input twice in a row
            if (_inputHistory.Count > 0 && input.Equals(_inputHistory[0], StringComparison.OrdinalIgnoreCase))
                return;

            // If we went over capacity, remove the oldest input entry to make room for a new one
            if (_inputHistory.Count == maxCapacity)
                _inputHistory.RemoveAt(maxCapacity - 1);

            // Insert the new input entry
            _inputHistory.Insert(0, input);

            // If the go-to input entry was removed for capacity reasons, then the new input entry becomes go-to input entry
            if (_currentInput == maxCapacity - 1)
                _currentInput = 0;
            // Otherwise make sure the go-to input entry remains the same by shifting the index
            // Note that if there was no input entry before, then the go-to input entry index remains 0 which is the new input entry
            else
                _currentInput = Mathf.Clamp(++_currentInput, 0, _inputHistory.Count - 1);

            // If the new input entry is different than the go-to input entry, then it becomes the go-to input entry
            if (!input.Equals(_inputHistory[_currentInput], StringComparison.OrdinalIgnoreCase))
                _currentInput = 0;
        }

        public void Clear()
        {
            _inputHistory.Clear();
            _currentInput = 0;
            _isNavigating = false;
        }
    }
}
