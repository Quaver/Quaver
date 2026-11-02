/*
 * This file contains a C# port of TwiN/go-away v1.8.1:
 * https://github.com/TwiN/go-away/tree/v1.8.1
 *
 * MIT License
 *
 * Copyright (c) 2022-2023 TwiN
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Quaver.Shared.Online.Chat
{
    /// <summary>
    ///     Applies the default profanity-censoring behavior from go-away v1.8.1.
    /// </summary>
    public static class ChatCensor
    {
        /// <summary>
        ///     The upstream go-away version this implementation is based on.
        /// </summary>
        public const string AlgorithmVersion = "v1.8.1";

        /// <summary>
        ///     Characters normalized before the profanity dictionaries are checked.
        /// </summary>
        private static Dictionary<int, char> CharacterReplacements { get; } = new Dictionary<int, char>
        {
            ['0'] = 'o',
            ['1'] = 'i',
            ['3'] = 'e',
            ['4'] = 'a',
            ['5'] = 's',
            ['7'] = 'l',
            ['$'] = 's',
            ['!'] = 'i',
            ['+'] = 't',
            ['#'] = 'h',
            ['@'] = 'a',
            ['<'] = 'c',
            ['-'] = ' ',
            ['_'] = ' ',
            ['|'] = ' ',
            ['.'] = ' ',
            [','] = ' ',
            ['('] = ' ',
            [')'] = ' ',
            ['>'] = ' ',
            ['"'] = ' ',
            ['`'] = ' ',
            ['~'] = ' ',
            ['*'] = ' ',
            ['&'] = ' ',
            ['%'] = ' ',
            ['?'] = ' '
        };

        /// <summary>
        ///     Words checked before false positives are removed.
        /// </summary>
        private static string[] FalseNegatives { get; } =
        {
            "asshole",
            "dumbass",
            "nigger"
        };

        /// <summary>
        ///     Text fragments removed before the main profanity list is checked.
        /// </summary>
        private static string[] FalsePositives { get; } =
        {
            "analy",
            "arsenal",
            "assassin",
            "assaying",
            "assert",
            "assign",
            "assimil",
            "assist",
            "associat",
            "assum",
            "assur",
            "banal",
            "basement",
            "bass",
            "cass",
            "butter",
            "butthe",
            "button",
            "canvass",
            "circum",
            "clitheroe",
            "cockburn",
            "cocktail",
            "cumber",
            "cumbing",
            "cumulat",
            "dickvandyke",
            "document",
            "evaluate",
            "exclusive",
            "expensive",
            "explain",
            "expression",
            "grape",
            "grass",
            "harass",
            "hass",
            "horniman",
            "hotwater",
            "identit",
            "kassa",
            "kassi",
            "lass",
            "leafage",
            "libshitz",
            "magnacumlaude",
            "mass",
            "mocha",
            "pass",
            "penistone",
            "peacock",
            "phoebe",
            "phoenix",
            "pushit",
            "raccoon",
            "sassy",
            "saturday",
            "scrap",
            "serfage",
            "sexist",
            "shoe",
            "scunthorpe",
            "shitake",
            "stitch",
            "sussex",
            "therapist",
            "therapeutic",
            "tysongay",
            "wass",
            "wharfage"
        };

        /// <summary>
        ///     Words checked after false positives are removed.
        /// </summary>
        private static string[] Profanities { get; } =
        {
            "anal",
            "anus",
            "arse",
            "ass",
            "ballsack",
            "balls",
            "bastard",
            "bitch",
            "btch",
            "biatch",
            "blowjob",
            "bollock",
            "bollok",
            "boner",
            "boob",
            "bugger",
            "butt",
            "choad",
            "clitoris",
            "cock",
            "coon",
            "crap",
            "cum",
            "cunt",
            "dick",
            "dildo",
            "douchebag",
            "dyke",
            "fag",
            "feck",
            "fellate",
            "fellatio",
            "felching",
            "fuck",
            "fudgepacker",
            "flange",
            "gtfo",
            "gyat",
            "hoe",
            "horny",
            "incest",
            "jerk",
            "jizz",
            "labia",
            "masturbat",
            "muff",
            "naked",
            "nazi",
            "nigga",
            "niggu",
            "nipple",
            "nips",
            "nude",
            "pedophile",
            "penis",
            "piss",
            "poop",
            "porn",
            "prick",
            "prostitut",
            "pube",
            "pussie",
            "pussy",
            "queer",
            "rape",
            "rapist",
            "retard",
            "rimjob",
            "scrotum",
            "sex",
            "shit",
            "slut",
            "spunk",
            "stfu",
            "suckmy",
            "tits",
            "tittie",
            "titty",
            "turd",
            "twat",
            "vagina",
            "wank",
            "whore"
        };

        /// <summary>
        ///     Replaces detected profanity with asterisks while retaining the original text's spacing and punctuation.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Censor(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value ?? string.Empty;

            var censored = new List<Rune>(value.EnumerateRunes());
            Sanitize(value, out var sanitized, out var originalIndexes);

            CensorMatches(sanitized, originalIndexes, censored, FalseNegatives);
            RemoveFalsePositives(sanitized, originalIndexes);
            CensorMatches(sanitized, originalIndexes, censored, Profanities);

            var result = new StringBuilder(value.Length);

            foreach (var character in censored)
                result.Append(character.ToString());

            return result.ToString();
        }

        /// <summary>
        ///     Normalizes text while remembering where every retained character appeared in the original value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="sanitized"></param>
        /// <param name="originalIndexes"></param>
        private static void Sanitize(string value, out List<Rune> sanitized, out List<int> originalIndexes)
        {
            sanitized = new List<Rune>();
            originalIndexes = new List<int>();
            var originalIndex = 0;

            foreach (var originalCharacter in value.EnumerateRunes())
            {
                var character = Rune.ToLowerInvariant(originalCharacter);

                if (CharacterReplacements.TryGetValue(character.Value, out var replacement))
                    character = new Rune(replacement);

                foreach (var normalizedCharacter in character.ToString().Normalize(NormalizationForm.FormD).EnumerateRunes())
                {
                    if (Rune.GetUnicodeCategory(normalizedCharacter) == UnicodeCategory.NonSpacingMark ||
                        normalizedCharacter.Value == ' ')
                        continue;

                    sanitized.Add(normalizedCharacter);
                    originalIndexes.Add(originalIndex);
                }

                originalIndex++;
            }
        }

        /// <summary>
        ///     Censors every occurrence of each word in the supplied dictionary.
        /// </summary>
        /// <param name="sanitized"></param>
        /// <param name="originalIndexes"></param>
        /// <param name="censored"></param>
        /// <param name="words"></param>
        private static void CensorMatches(IReadOnlyList<Rune> sanitized, IReadOnlyList<int> originalIndexes,
            IList<Rune> censored, IEnumerable<string> words)
        {
            foreach (var word in words)
            {
                var wordCharacters = new List<Rune>(word.EnumerateRunes());
                var searchIndex = 0;

                while ((searchIndex = Find(sanitized, wordCharacters, searchIndex)) != -1)
                {
                    for (var i = 0; i < wordCharacters.Count; i++)
                    {
                        var originalIndex = originalIndexes[searchIndex + i];

                        if (originalIndex < censored.Count)
                            censored[originalIndex] = new Rune('*');
                    }

                    searchIndex += wordCharacters.Count;
                }
            }
        }

        /// <summary>
        ///     Removes every false-positive occurrence and its original-index mappings.
        /// </summary>
        /// <param name="sanitized"></param>
        /// <param name="originalIndexes"></param>
        private static void RemoveFalsePositives(List<Rune> sanitized, List<int> originalIndexes)
        {
            foreach (var word in FalsePositives)
            {
                var wordCharacters = new List<Rune>(word.EnumerateRunes());
                var searchIndex = 0;

                while ((searchIndex = Find(sanitized, wordCharacters, searchIndex)) != -1)
                {
                    sanitized.RemoveRange(searchIndex, wordCharacters.Count);
                    originalIndexes.RemoveRange(searchIndex, wordCharacters.Count);
                }
            }
        }

        /// <summary>
        ///     Finds a sequence of runes without allocating an intermediate string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="target"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        private static int Find(IReadOnlyList<Rune> value, IReadOnlyList<Rune> target, int startIndex)
        {
            for (var i = startIndex; i <= value.Count - target.Count; i++)
            {
                var matches = true;

                for (var j = 0; j < target.Count; j++)
                {
                    if (value[i + j] == target[j])
                        continue;

                    matches = false;
                    break;
                }

                if (matches)
                    return i;
            }

            return -1;
        }
    }
}
