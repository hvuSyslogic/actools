using System;
using System.Collections.Generic;
using System.Linq;
using AcManager.Tools.Filters;
using AcManager.Tools.Filters.Testers;
using AcTools.Utils.Helpers;
using FirstFloor.ModernUI.Helpers;

namespace AcManager.Controls.Helpers {
    public static class FilterHints {
        public static string GetReadableType(this KeywordType type) {
            switch (type) {
                case KeywordType.Child | KeywordType.String:
                    return "Text or sub-filter";
                case KeywordType.TimeSpan | KeywordType.Flag:
                    return "Duration or existence";
                case KeywordType.String:
                    return "Text";
                case KeywordType.Child:
                    return "Sub-filter";
                case KeywordType.Flag:
                    return "Flag";
                case KeywordType.Number:
                    return "Number";
                case KeywordType.Distance:
                    return "Distance";
                case KeywordType.DateTime:
                    return "Date";
                case KeywordType.TimeSpan:
                    return "Duration";
                case KeywordType.FileSize:
                    return "File size";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static string GetShortList_Line(KeywordDescription x) {
            string codeExample;
            string extras;

            if (x.Key == @"tag") {
                codeExample = "#[i]tag[/i]";
                extras = null;
            } else {
                switch (x.Type) {
                    case KeywordType.Child:
                        codeExample = $"{x.Key}([i]{GetReadableType(x.Type).ToSentenceMember()}[/i])";
                        break;
                    case KeywordType.Child | KeywordType.String:
                        codeExample = $"{x.Key}: [i]{GetReadableType(x.Type).ToSentenceMember()}[/i]";
                        break;
                    case KeywordType.Flag:
                        codeExample = $"{x.Key}+";
                        break;
                    default:
                        codeExample = $"{x.Key}: [i]{GetReadableType(x.Type).ToSentenceMember()}[/i]";
                        break;
                }

                var altKeys = x.AlternativeKeys.Count(y => y.Length < x.Key.Length) > 0
                        ? $"shortcut: {x.AlternativeKeys.Where(y => y.Length < x.Key.Length).Select(y => $"[mono][b]{y}[/b][/mono]").First()}"
                        : null;
                var units = x.Unit != null ? $"in {x.Unit} by default" : null;
                extras = new[] { units, altKeys }.JoinToString(@", ");
                if (extras.Length > 0) {
                    extras = $" ({extras})";
                }
            }

            return $" • [mono][b]{codeExample}[/b][/mono] – {x.Description.ToSentenceMember()}{extras};";
        }

        public static string GetShortList(ITesterDescription descriptions) {
            var result = EnumExtension.GetValues<KeywordPriority>().OrderByDescending(x => (int)x).ToDictionary(x => x, x => new List<string>());
            foreach (var description in descriptions.GetDescriptions().DistinctBy(x => x.Key).OrderBy(x => x.Key)) {
                result[description.Priority].Add(GetShortList_Line(description));
            }
            return result.Where(x => x.Value.Count > 0).Select(x => x.Value.JoinToString('\n')).JoinToString("\n\n");
        }

        private static string GetHint(ITesterDescription descriptions) {
            return $"Supported properties (for numbers, you can use “<” or “>” instead of “:”):\n\n{GetShortList(descriptions).ToSentence()}\n\nTo learn more about filtering, go to [b]About/Everything About Filtering[/b] section";
        }

        public static Lazy<string> Cars { get; } = new Lazy<string>(() => GetHint(CarObjectTester.Instance));
        public static Lazy<string> CarSetups { get; } = new Lazy<string>(() => GetHint(CarSetupObjectTester.Instance));
        public static Lazy<string> CarSkins { get; } = new Lazy<string>(() => GetHint(CarSkinObjectTester.Instance));
        public static Lazy<string> Files { get; } = new Lazy<string>(() => GetHint(FileTester.Instance));
        public static Lazy<string> LapTimes { get; } = new Lazy<string>(() => GetHint(LapTimeTester.Instance));
        public static Lazy<string> Replays { get; } = new Lazy<string>(() => GetHint(ReplayObjectTester.Instance));
        public static Lazy<string> ServerEntries { get; } = new Lazy<string>(() => GetHint(ServerEntryTester.Instance));
        public static Lazy<string> ServerPresets { get; } = new Lazy<string>(() => GetHint(ServerPresetObjectTester.Instance));
        public static Lazy<string> Showrooms { get; } = new Lazy<string>(() => GetHint(ShowroomObjectTester.Instance));
        public static Lazy<string> SpecialEvents { get; } = new Lazy<string>(() => GetHint(SpecialEventObjectTester.Instance));
        public static Lazy<string> Tracks { get; } = new Lazy<string>(() => GetHint(TrackObjectTester.Instance));
        public static Lazy<string> TrackSkins { get; } = new Lazy<string>(() => GetHint(TrackSkinObjectTester.Instance));
    }
}