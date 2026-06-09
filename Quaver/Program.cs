/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Quaver.Shared;
using Quaver.Shared.Config;
using Quaver.Shared.Helpers;
using Quaver.Shared.IPC;
using Quaver.Shared.Online;
using Wobble;
using Wobble.IO;
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

        private static string[] LaunchArguments { get; set; } = Array.Empty<string>();

        [STAThread]
        public static void Main(string[] args)
        {
            LaunchArguments = Array.ConvertAll(args, NormalizeLaunchArgument);

            // Prevents more than one instance of Quaver to run at a time
            using (var mutex = new Mutex(false, "Global\\" + Guid))
            {
                if (!mutex.WaitOne(0, false))
                {
                    Logger.Error("Quaver is already running", LogType.Runtime);

                    // Send to running instance only if we have actual data to send
                    if (LaunchArguments.Length > 0)
                        SendToRunningInstanceIpc(LaunchArguments);

                    return;
                }

                Run();
                return;
            }

            // Uncomment this and comment the above mutex to allow multiple instances of Quaver to be run
            Run();
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
                SendCrashLog(exception);
                OnlineManager.Client?.Disconnect();
            };

            if (OperatingSystem.IsWindows())
                LogWineVersion();

            // Change the working directory to where the executable is.
            Directory.SetCurrentDirectory(WorkingDirectory);
            Environment.CurrentDirectory = WorkingDirectory;

            ConfigManager.Initialize();
            StructuredConfigManager.Initialize();
            StartIpcServer();

            foreach (var argument in LaunchArguments)
                QuaverIpcHandler.HandleMessage(argument);

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            NativeAssemblies.Copy();
            SteamManager.Initialize();

            try
            {
                var fileAssociations = CreateFileAssociations();
                var applicationIconPath = GetFileIconPath(GetNativeIconFileName("quaver-logo"));

                Utils.NativeUtils.RegisterURIScheme("quaver", "Quaver");
                ExtractNativeIcons(fileAssociations, applicationIconPath);
                Utils.NativeUtils.RegisterFileAssociations(fileAssociations, applicationIconPath);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }

#if VISUAL_TESTS
            using (var game = new QuaverGame(new HotLoader("../../../../Quaver.Shared/")))
#else
            using (var game = new QuaverGame())
#endif
                game.Run();
        }

        [SupportedOSPlatform("windows")]
        private static void LogWineVersion()
        {
            var dll = Kernel32.GetModuleHandle("ntdll.dll");
            var getVersion = Kernel32.GetProcAddress(dll, "wine_get_version");

            if (getVersion is not 0)
                Logger.Warning(
                    "Detected wine runtime. Using wine over the native version is discouraged as it may lead to stability or compatibility issues. If possible, please use the native version of the game.",
                    LogType.Runtime
                );
        }

        private static string NormalizeLaunchArgument(string argument)
        {
            if (Uri.TryCreate(argument, UriKind.Absolute, out var uri) && uri.IsFile)
                return uri.LocalPath;

            return argument;
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
        private static void SendToRunningInstanceIpc(string[] messages)
        {
            try
            {
                var c = new IpcClient();
                c.Initialize(IpcPort);

                foreach (var message in messages)
                    c.Send(message);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Extracts embedded file icons to disk so the operating system can use them for file associations.
        /// </summary>
        private static void ExtractNativeIcons(IReadOnlyDictionary<string, FileAssociation> fileAssociations,
            string applicationIconPath)
        {
            var iconDirectory = GetFileIconDirectory();
            Directory.CreateDirectory(iconDirectory);

            using (var resources = new DllResourceStore("Quaver.Resources.dll"))
            {
                foreach (var association in fileAssociations)
                {
                    var iconFileName = Path.GetFileName(association.Value.IconPath);
                    var icon = resources.Get($"Quaver.Resources/Icons/{iconFileName}");

                    if (icon == null)
                        continue;

                    File.WriteAllBytes(association.Value.IconPath, icon);
                }

                var applicationIconFileName = Path.GetFileName(applicationIconPath);
                var applicationIcon = resources.Get($"Quaver.Resources/Icons/{applicationIconFileName}");

                if (applicationIcon != null)
                    File.WriteAllBytes(applicationIconPath, applicationIcon);
            }
        }

        private static string GetFileIconPath(string iconFileName) => Path.Combine(GetFileIconDirectory(), iconFileName);

        private static string GetFileIconDirectory()
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Quaver", "File Icons");

        private static string GetNativeIconFileName(string fileType)
        {
            if (OperatingSystem.IsMacOS())
                return $"{fileType}.icns";

            if (OperatingSystem.IsLinux())
                return $"{fileType}.png";

            return $"{fileType}.ico";
        }

        private static Dictionary<string, FileAssociation> CreateFileAssociations()
            => new Dictionary<string, FileAssociation>
            {
                {
                    ".qp",
                    new FileAssociation
                    {
                        ProgId = "Quaver.qp",
                        FriendlyName = "Quaver Mapset",
                        IconPath = GetFileIconPath(GetNativeIconFileName("qp")),
                        MimeType = "application/x-quaver-qp"
                    }
                },
                {
                    ".qpl",
                    new FileAssociation
                    {
                        ProgId = "Quaver.qpl",
                        FriendlyName = "Quaver Playlist",
                        IconPath = GetFileIconPath(GetNativeIconFileName("qpl")),
                        MimeType = "application/x-quaver-qpl"
                    }
                },
                {
                    ".qr",
                    new FileAssociation
                    {
                        ProgId = "Quaver.qr",
                        FriendlyName = "Quaver Replay",
                        IconPath = GetFileIconPath(GetNativeIconFileName("qr")),
                        MimeType = "application/x-quaver-qr"
                    }
                },
                {
                    ".qs",
                    new FileAssociation
                    {
                        ProgId = "Quaver.qs",
                        FriendlyName = "Quaver Skin",
                        IconPath = GetFileIconPath(GetNativeIconFileName("qs")),
                        MimeType = "application/x-quaver-qs"
                    }
                }
            };

        /// <summary>
        ///     Automatically sends crash logs to the server.
        /// </summary>
        private static void SendCrashLog(Exception e)
        {
            var game = (QuaverGame)GameBase.Game;

            // Exclude non-steam builds
            if (!game.IsDeployedBuild)
                return;

            var runtime = File.ReadAllText(Logger.GetLogPath(LogType.Runtime));
            var network = File.ReadAllText(Logger.GetLogPath(LogType.Network));

            OnlineManager.Client?.SendCrashLog(runtime, network, e.ToString(), game.Version);
        }

        static class Kernel32
        {
            [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, SetLastError = true)]
            internal static extern nint GetModuleHandle(string moduleName);

            [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
            internal static extern nint GetProcAddress(
                nint hModule,
                [MarshalAs(UnmanagedType.LPStr)] string procedureName
            );
        }
    }
}
