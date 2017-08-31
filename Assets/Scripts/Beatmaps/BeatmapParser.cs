using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Wenzil.Console;

public class BeatmapParser : MonoBehaviour
{
    public static Beatmap Parse(string filePath)
    {
        // Check if the file path is empty.
        if (Strings.IsNullOrEmptyOrWhiteSpace(filePath))
        {
            // If it is, we'll return invalid data.
            Beatmap tempBeatmap = new Beatmap();
            tempBeatmap.valid = false;
            return tempBeatmap;
        }

        // Create a boolean value that we'll use to check whether
        // we are currently parsing the notes or other metadata.
        bool inNotes = false;

        Beatmap beatmap = new Beatmap();
        // Initialize beatmap data
        // If it encounters any major erros during parsing, valid is set to false,
        // and the song cannot be selected.
        beatmap.valid = true;
        beatmap.beginnerExists = false;
        beatmap.easyExists = false;
        beatmap.mediumExists = false;
        beatmap.hardExists = false;
        beatmap.challengeExists = false;

        // Collect the raw data from the .sm formatted file all at once.
        List<string> fileData = File.ReadAllLines(filePath).ToList();

        // Get the file directoryu and make sure it ends with either a forward or backslash
        string fileDir = Path.GetDirectoryName(filePath);
        if (!fileDir.EndsWith("\\") && !fileDir.EndsWith("/"))
        {
            fileDir += "\\";
        }

        // Go through the file data
        for (int i = 0; i < fileData.Count; i++)
        {
            // Parse the data from the document
            string line = fileData[i].Trim();

            if (line.StartsWith("//"))
            {
                // If the line starts with //, it's a comment, so move on to the next line.
                continue;
            }
            else if (line.StartsWith("#"))
            {
                // The # symbol denotes generic metadata for the song.
                string key = line.Substring(0, line.IndexOf(':')).Trim('#').Trim(':');

                switch(key.ToUpper())
                {
                    case "TITLE":
                        beatmap.title = line.Substring(line.IndexOf(':')).Trim(':').Trim(';');
                        break;
                    case "SUBTITLE":
                        beatmap.subtitle = line.Substring(line.IndexOf(':')).Trim(':').Trim(';');
                        break;
                    case "ARTIST":
                        beatmap.artist = line.Substring(line.IndexOf(':')).Trim(':').Trim(';');
                        break;
                    case "BANNER":
                        beatmap.bannerPath = fileDir + line.Substring(line.IndexOf(':')).Trim(':').Trim(';');
                        break;
                    case "BACKGROUND":
                        beatmap.backgroundPath = fileDir + line.Substring(line.IndexOf(':')).Trim(':').Trim(';');
                        break;
                    case "MUSIC":
                        beatmap.musicPath = fileDir + line.Substring(line.IndexOf(':')).Trim(':').Trim(';');

                        if (Strings.IsNullOrEmptyOrWhiteSpace(beatmap.musicPath) || !File.Exists(beatmap.musicPath))
                        {
                            // No music file found, so we'll return an invalid beatmap
                            beatmap.musicPath = null;
                            beatmap.valid = false;
                        }
                        break;
                    case "OFFSET":
                        if (!float.TryParse(line.Substring(line.IndexOf(':')).Trim(':').Trim(';'), out beatmap.offset))
                        {
                            //Error Parsing
                            beatmap.offset = 0.0f;
                        }
                        break;
                    case "SAMPLESTART":
                        if (!float.TryParse(line.Substring(line.IndexOf(':')).Trim(':').Trim(';'), out beatmap.sampleStart))
                        {
                            //Error Parsing
                            beatmap.sampleStart = 0.0f;
                        }
                        break;
                    case "SAMPLELENGTH":
                        if (!float.TryParse(line.Substring(line.IndexOf(':')).Trim(':').Trim(';'), out beatmap.sampleLength))
                        {
                            //Error Parsing
                            beatmap.sampleLength = 0.0f;
                        }
                        break;
                    case "DISPLAYBPM":
                        if (!float.TryParse(line.Substring(line.IndexOf(':')).Trim(':').Trim(';'), out beatmap.bpm) || beatmap.bpm <= 0)
                        {
                            //Error Parsing - BPM not valid
                            beatmap.valid = false;
                            beatmap.bpm = 0.0f;
                        }
                        break;
                    case "NOTES":
                        inNotes = true;
                        break;
                    default:
                        break;
                }

                // If we're now parsing the step data.
                if (inNotes)
                {
                }
            }

        }

        string beatmapLogData = "Displaying Parsed Beatmap Information: \n" +
                                "Title: " + beatmap.title + "\n" + 
                                "Subtitle: " + beatmap.subtitle + "\n" +
                                "Artist: " + beatmap.artist + "\n" + 
                                "Banner: " + beatmap.bannerPath + "\n" + 
                                "Background: " + beatmap.backgroundPath + "\n" +
                                "Music: " + beatmap.musicPath + "\n" + 
                                "Offset:" + beatmap.offset + "\n" + 
                                "Sample Start: " + beatmap.sampleStart + "\n" +
                                "Sample Length: " + beatmap.sampleLength + "\n" +
                                "BPM: " + beatmap.bpm + "\n";
        Console.Log(beatmapLogData);

        return beatmap;
    }
}
