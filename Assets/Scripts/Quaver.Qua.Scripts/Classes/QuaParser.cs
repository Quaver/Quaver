using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Linq;

namespace Qua.Scripts {

	public static class QuaParser 
	{

		// Takes a .qua file and parses it into an instance of the Qua Class.
		public static QuaFile Parse(string filePath) 
		{
			// Check if the file path and extension are correct if not, return an
			// invalid qua object.
			if (!File.Exists(filePath) || !filePath.ToLower().EndsWith(".qua"))
			{
				QuaFile tempQua = new QuaFile();
				tempQua.IsValidQua = false;
				return tempQua;
			}

			// Create a new Qua instance that we'll return to the caller.
			QuaFile quaFile = new QuaFile();
			quaFile.TimingPoints = new List<TimingPoint>();
			quaFile.SliderVelocities = new List<SliderVelocity>();
			quaFile.HitObjects = new List<HitObject>();
			quaFile.IsValidQua = true;

			// This will hold the section of the file that we are currently parsing.
			string section = "";

			foreach(string line in File.ReadAllLines(filePath))
			{
				switch(line.Trim())
				{
					case "# General":
						section = "General";
						break;
					case "# Metadata":
						section = "Metadata";
						break;
					case "# Difficulty":
						section = "Difficulty";
						break;
					case "# Timing":
						section = "Timing";
						break;
					case "# SV":
						section = "SV";
						break;
					case "# HitObjects":
						section = "HitObjects";
						break;
				}

				// Parse General Section
				if (section.Equals("General"))
				{
					if (line.Contains(":"))
					{
						string key = line.Substring(0, line.IndexOf(':'));
						string value = line.Trim().Split(':').Last().Trim();

						switch(key.Trim())
						{
							case "AudioFile":
								quaFile.AudioFile = value;
								break;
							case "AudioLeadIn":
								quaFile.AudioLeadIn = Int32.Parse(value);
								break;
							case "SongPreviewTime":
								quaFile.SongPreviewTime = Int32.Parse(value);
								break;
							case "BackgroundFile":
								quaFile.BackgroundFile = value;
								break;
						}

					}
				}

				// Parse Metadata Section
				if (section.Equals("Metadata"))
				{
					if (line.Contains(":"))
					{
						string key = line.Substring(0, line.IndexOf(':'));
						string value = line.Trim().Split(':').Last().Trim();

						switch(key.Trim())
						{
							case "Title":
								quaFile.Title = value;
								break;
							case "TitleUnicode":
								quaFile.TitleUnicode = value;
								break;
							case "Artist":
								quaFile.Artist = value;
								break;
							case "ArtistUnicode":
								quaFile.ArtistUnicode = value;
								break;
							case "Source":
								quaFile.Source = value;
								break;
							case "Tags":
								quaFile.Tags = value;
								break;
							case "Creator":
								quaFile.Creator = value;
								break;
							case "DifficultyName":
								quaFile.DifficultyName = value;
								break;
							case "MapID":
								quaFile.MapID = Int32.Parse(value);
								break;
							case "MapSetID":
								quaFile.MapSetID = Int32.Parse(value);
								break;
						}						
					}
				}

				// Parse Difficulty Section
				if (section.Equals("Difficulty"))
				{
					if (line.Contains(":"))
					{
						string key = line.Substring(0, line.IndexOf(':'));
						string value = line.Trim().Split(':').Last().Trim();

						switch(key.Trim())
						{
							case "HPDrain":
								quaFile.HPDrain = float.Parse(value);
								break;
							case "AccuracyStrain":
								quaFile.AccuracyStrain = float.Parse(value);
								break;
						}						
					}
				}

				// Parse Timing Section
				if (section.Equals("Timing"))
				{
					if (line.Contains("|") && !line.Contains("#"))
					{
						string[] values = line.Split('|');

						if (values.Length != 2) {
							quaFile.IsValidQua = false;
							return quaFile;
						}

						TimingPoint timing = new TimingPoint();

						timing.StartTime = float.Parse(values[0]);
						timing.BPM = float.Parse(values[1]);

						quaFile.TimingPoints.Add(timing);
					}
				}

				// Parse SV
				if (section.Equals("SV"))
				{
					if (line.Contains("|") && !line.Contains("#"))
					{
						string[] values = line.Split('|');

						// There should only be 3 values in an SV, if not, it's an invalid map.
						if (values.Length != 3) {
							quaFile.IsValidQua = false;
							return quaFile;
						}

						SliderVelocity sv = new SliderVelocity();

						sv.StartTime = float.Parse(values[0]);
						sv.Multiplier = float.Parse(values[1]);
						sv.Volume = Int32.Parse(values[2]);

						quaFile.SliderVelocities.Add(sv);
					}
				}

				// Parse Hit Objects
				if (section.Equals("HitObjects"))
				{	
					if (line.Contains("|") && !line.Contains("HitObjects"))
					{
						string[] values = line.Split('|');	

						if (values.Length != 3) {
							quaFile.IsValidQua = false;
							return quaFile;
						}

						HitObject ho = new HitObject(); // lol, ho. 

						ho.StartTime = Int32.Parse(values[0]);
						ho.KeyLane = Int32.Parse(values[1]);

						// If the key lane isn't in 1-4, then we'll consider the map to be invalid.
						if (ho.KeyLane < 1 || ho.KeyLane > 4) 
						{
							quaFile.IsValidQua = false;
							return quaFile;
						}

						ho.EndTime = Int32.Parse(values[2]);

						quaFile.HitObjects.Add(ho);
					
					}
				}

			}		

			// If there are zero timing points in the beatmap we'll consider that invalid.
			if (quaFile.TimingPoints.Count == 0)
			{
				quaFile.IsValidQua = false;
				return quaFile;
			}

			return quaFile;
		}

	}

}
