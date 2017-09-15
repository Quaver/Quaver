﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Wenzil.Console
{
    /// <summary>
    /// The behavior of the console.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ConsoleController))]
    public class ConsoleController : MonoBehaviour
    {
        private const int inputHistoryCapacity = 20;

        public ConsoleUI ui;
        public KeyCode toggleKey = KeyCode.BackQuote;
        public bool closeOnEscape = false;

        private ConsoleInputHistory _inputHistory = new ConsoleInputHistory(inputHistoryCapacity);

        private void Awake()
        {
            /* This instantiation causes a bug when Unity rebuilds the project while in play mode
               Solution: move it to class level initialization, and make inputHistoryCapacity a const */
            // inputHistory = new ConsoleInputHistory(inputHistoryCapacity); 
        }
        private void OnEnable()
        {
            Console.OnConsoleLog += ui.AddNewOutputLine;
            ui.onSubmitCommand += ExecuteCommand;
            ui.onClearConsole += _inputHistory.Clear;
        }

        private void OnDisable()
        {
            Console.OnConsoleLog -= ui.AddNewOutputLine;
            ui.onSubmitCommand -= ExecuteCommand;
            ui.onClearConsole -= _inputHistory.Clear;
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                ui.ToggleConsole();
            else if (Input.GetKeyDown(KeyCode.Escape) && closeOnEscape)
                ui.CloseConsole();
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                NavigateInputHistory(true);
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                NavigateInputHistory(false);
        }

        private void NavigateInputHistory(bool up)
        {
            string navigatedToInput = _inputHistory.Navigate(up);
            ui.SetInputText(navigatedToInput);
        }

        private void ExecuteCommand(string input)
        {
            string[] parts = input.Split(' ');
            string command = parts[0];
            string[] args = parts.Skip(1).ToArray();

            Console.Log("> " + input);
            Console.Log(ConsoleCommandsDatabase.ExecuteCommand(command, args));
            _inputHistory.AddNewInputEntry(input);
        }
    }
}