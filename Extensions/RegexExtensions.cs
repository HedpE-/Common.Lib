using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common.Lib.Extensions
{
    public static class RegexExtensions
    {
        public static string ReplaceGroups(this Regex regex, string input, IDictionary<string, string> replacementPerGroupName)
        {
            if (regex == null || string.IsNullOrEmpty(input) || replacementPerGroupName == null || !replacementPerGroupName.Any())
                return null;

            return regex.Replace(
                input,
                m =>
                {
                    var keys = replacementPerGroupName.Keys;

                    var groups = m.Groups.Cast<Group>().Where(g => replacementPerGroupName.Keys.Contains(g.Name));
                    var sb = new StringBuilder();
                    var previousCaptureEnd = 0;
                    foreach(var group in groups)
                    {
                        foreach (var capture in group.Captures.Cast<Capture>())
                        {
                            var currentCaptureEnd =
                                capture.Index + capture.Length - m.Index;
                            var currentCaptureLength =
                                capture.Index - m.Index - previousCaptureEnd;
                            sb.Append(
                                m.Value.Substring(
                                    previousCaptureEnd, currentCaptureLength));
                            sb.Append(replacementPerGroupName[group.Name]);
                            previousCaptureEnd = currentCaptureEnd;
                        }
                    }
                    sb.Append(m.Value.Substring(previousCaptureEnd));

                    return sb.ToString();
                });
        }
    }
}
