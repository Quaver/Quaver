using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Settings;
using SQLite;
using Wobble.Bindables;
using Wobble.Logging;

namespace Quaver.Shared.Database.Judgements
{
    public static class JudgementWindowsDatabaseCache
    {
        /// <summary>
        ///     The list of preset judgement windows
        /// </summary>
        public static List<JudgementWindows> Presets { get; private set; } = new List<JudgementWindows>();

        /// <summary>
        ///     The currently selected judgement windows
        /// </summary>
        public static Bindable<JudgementWindows> Selected { get; private set; }

        /// <summary>
        ///     Standard judgement windows
        /// </summary>
        public static JudgementWindows Standard { get; private set; }

        /// <summary>
        ///     Loads the database of presets & adds Quaver defaults
        /// </summary>
        public static void Load()
        {
            CreateTable();
            InitializeDefaultPresetWindows();

            Presets = Presets.Concat(FetchAllWindows()).ToList();

            Selected = new Bindable<JudgementWindows>(Standard)
            {
                Value = Standard
            };

            // Find value from config
            foreach (var window in Presets)
            {
                if (ConfigManager.JudgementWindows.Value != window.Name)
                    continue;

                Selected.Value = window;
                break;
            }

            Selected.ValueChanged += (sender, args) => ConfigManager.JudgementWindows.Value = args.Value.Name;
        }

        /// <summary>
        ///     Creates the `JudgementWindows` database table.
        /// </summary>
        private static void CreateTable()
        {
            try
            {
                DatabaseManager.Connection.CreateTable<JudgementWindows>();
                Logger.Important($"JudgementWindows table has been created", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                throw;
            }
        }

        /// <summary>
        ///     Sets a few defaults the players to use
        /// </summary>
        private static void InitializeDefaultPresetWindows()
        {
            Standard = CreatePresetWindows("Standard*", 0);

            Presets.Add(CreatePresetWindows("Peaceful", -3));
            Presets.Add(CreatePresetWindows("Lenient", -2));
            Presets.Add(CreatePresetWindows("Chill", -1));
            Presets.Add(Standard);
            Presets.Add(CreatePresetWindows("Strict", 1));
            Presets.Add(CreatePresetWindows("Tough", 2));
            Presets.Add(CreatePresetWindows("Extreme", 3));
            Presets.Add(CreatePresetWindows("Impossible", 8));
        }

        /// <summary>
        /// </summary>
        private static List<JudgementWindows> FetchAllWindows()
        {
            return DatabaseManager.Connection.Table<JudgementWindows>().ToList();
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static JudgementWindows CreatePresetWindows(string name, int n)
        {
            var windows = new JudgementWindows
            {
                Name = name,
                IsDefault = true,
            };

            const float constant = 1.1f;

            windows.Marvelous /= (float) Math.Pow(constant, n);
            windows.Perfect /= (float) Math.Pow(constant, n);
            windows.Great /= (float) Math.Pow(constant, n);
            windows.Good /= (float) Math.Pow(constant, n);
            // Only scale the miss timing on easier windows because scaling the bad timings
            // on harder windows enables for less penalty than what easier window players would recieve.
            if (n < 0)
            {
                windows.Okay /= (float) Math.Pow(constant, n);
                windows.Miss /= (float) Math.Pow(constant, n);
            }

            windows.Marvelous = (int) windows.Marvelous;
            windows.Perfect = (int) windows.Perfect;
            windows.Great = (int) windows.Great;
            windows.Good = (int) windows.Good;
            windows.Okay = (int) windows.Okay;
            windows.Miss = (int) windows.Miss;

            Logger.Important($"Created preset judgement windows `{windows.Name}` " +
                             $"- Marv: {windows.Marvelous} | Perf: {windows.Perfect} | " +
                             $"Great: {windows.Great} | Good: {windows.Good} | " +
                             $"Okay: {windows.Okay} | Miss: {windows.Miss}", LogType.Runtime);

            return windows;
        }

        /// <summary>
        /// </summary>
        public static int Insert(JudgementWindows windows)
        {
            try
            {
                return DatabaseManager.Connection.Insert(windows);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                return -1;
            }
        }

        /// <summary>
        /// </summary>
        public static void Update(JudgementWindows windows)
        {
            try
            {
                DatabaseManager.Connection.Update(windows);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="windows"></param>
        public static void Delete(JudgementWindows windows)
        {
            try
            {
                DatabaseManager.Connection.Delete(windows);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        /// </summary>
        public static void UpdateAll()
        {
            try
            {
                foreach (var preset in Presets)
                {
                    if (preset.IsDefault)
                        continue;

                    Update(preset);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }
    }
}
