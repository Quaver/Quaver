﻿using System;
using System.IO;
using IniFileParser.Model;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Assets;

namespace Quaver.Shared.Skinning.Menus
{
    public abstract class SkinMenu
    {
        protected SkinStore Store { get; }

        protected IniData Config { get; }

        public SkinMenu(SkinStore store, IniData config)
        {
            Store = store;
            Config = config;

            LoadAll();
        }

        private void LoadAll()
        {
            if (Config != null)
                ReadConfig();

            LoadElements();
        }

        protected abstract void ReadConfig();

        protected abstract void LoadElements();

        protected void ReadIndividualConfig(string value, Action load)
        {
            if (value == null)
                return;

            load?.Invoke();
        }

        protected Texture2D LoadSkinElement(string folder, string file)
        {
            try
            {
                var path = $"{Store.Dir}/{folder}/{file}";
                return File.Exists(path) ? AssetLoader.LoadTexture2DFromFile(path) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
