using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IniParser;
using IniParser.Model;

namespace Quaver.Skin
{
	public class SkinConfigLoader
	{
		// Responsible for return back a parsed skin.ini file.
		// I would seriously suggest reading the example to learn how to use
		// this - https://github.com/rickyah/ini-parser
		public static Skin Load(string path)
		{
			var parser = new FileIniDataParser();

			IniData data = parser.ReadFile(path);

			Skin skin = new Skin();

			skin.Name = data["General"]["Name"];
			skin.Author = data["General"]["Author"];
			skin.Version = data["General"]["Version"];

			return skin;
		}
	}
}