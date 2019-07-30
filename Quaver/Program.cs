/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Net.Mime;
using System.Threading;
using Quaver.Shared;
using Quaver.Shared.Config;
using Quaver.Shared.Helpers;
using Quaver.Shared.IPC;
using Quaver.Shared.Online;
using Wobble;
using Wobble.Logging;
using Wobble.Platform;
using ZetaIpc.Runtime.Client;
using ZetaIpc.Runtime.Server;

namespace Quaver
{
    public static class Program
    {
        /// <summary>
        ///     The current working directory of the executable.
        /// </summary>
        public static string WorkingDirectory => WobbleGame.WorkingDirectory;

        /// <summary>
        /// </summary>
        private static string Guid = "9151537b-304c-4619-bf54-d367ba7d87ac";

        /// <summary>
        ///     The name of the pipe used for IPC
        /// </summary>
        private static int IpcPort { get; } = 43596;

        [STAThread]
        public static void Main(string[] args)
        {
            // Prevents more than one instance of Quaver to run at a time
            using(var mutex = new Mutex(false, "Global\\" + Guid))
            {
                if(!mutex.WaitOne(0, false))
                {
                    Console.WriteLine("Quaver is already running");

                    // Send to running instance only if we have actual data to send
                    if (args.Length > 0)
                        SendToRunningInstanceIpc(args[0]);

                    return;
                }

                Run();
            }

            // Uncomment this and comment the above mutex to allow multiple instances of Quaver
            // to be run
            // Run();
        }

        /// <summary>
        ///     Starts the game
        /// </summary>
        private static void Run()
        {
            Logger.Initialize();

            // Log all unhandled exceptions.
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var exception = args.ExceptionObject as Exception;
                Logger.Error(exception, LogType.Runtime);
            };

            StartIpcServer();

            // Change the working directory to where the executable is.
            Directory.SetCurrentDirectory(WorkingDirectory);
            Environment.CurrentDirectory = WorkingDirectory;

            try
            {
                using (var p = Process.GetCurrentProcess())
                    p.PriorityClass = ProcessPriorityClass.High;
            }
            catch (Win32Exception) { /* do nothing */ }

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            NativeAssemblies.Copy();
            ConfigManager.Initialize();
            SteamManager.Initialize();

            try
            {
                Utils.NativeUtils.RegisterURIScheme("quaver", "Quaver");
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }

            using (var game = new QuaverGame())
                game.Run();
        }

        /// <summary>
        ///     Starts an IPC server to receive messages from new instances of Quaver
        ///     that want to communicate with the main process
        /// </summary>
        private static void StartIpcServer()
        {
            try
            {
                var s = new IpcServer();
                s.Start(IpcPort);

                Logger.Important($"Started IPC server on port: {IpcPort}", LogType.Runtime);

                s.ReceivedRequest += (o, e) =>
                {
                    QuaverIpcHandler.HandleMessage(e.Request);
                    e.Handled = true;
                };
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Creates an IPC client and sends a message to the already
        ///     running instance of Quaver
        /// </summary>
        private static void SendToRunningInstanceIpc(string message)
        {
            try
            {
                var c = new IpcClient();
                c.Initialize(IpcPort);
                c.Send(message);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }
    }
}
