/*
 * Created by SharpDevelop.
 * User: GONCARJ3
 * Date: 06/01/2017
 * Time: 20:43
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using Common.Lib.HelperClasses;
using Common.Lib.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace Common.Lib.Extensions
{
    public static class StringExtensions
    {
        public static T ToEnum<T>(this string str, bool ignoreCase = false) where T : struct, IConvertible
        {
            return EnumExtensions.Parse<T>(str, ignoreCase);
        }

        public static bool ContainsAny(this string str, IEnumerable<string> matches, StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (matches == null)
                throw new ArgumentNullException("matches");
            if (str == null)
                throw new ArgumentNullException("input string");

            foreach (string match in matches)
            {
                if (str.Contains(match, stringComparison))
                    return true;
            }

            return false;
        }

        public static bool ContainsAll(this string str, IEnumerable<string> matches, StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (matches == null)
                throw new ArgumentNullException("matches");
            if (str == null)
                throw new ArgumentNullException("input string");

            foreach (string match in matches)
            {
                if (!str.Contains(match, stringComparison))
                    return false;
            }

            return true;
        }

        public static string Reverse(this string str)
        {
            string output = "";
            for (var i = str.Length - 1; i >= 0; i--)
                output += str[i];

            return output;
        }

        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

        public static string RemoveDigits(this string str)
        {
            string regex = "[0-9]";
            return Regex.Replace(str, regex, string.Empty, RegexOptions.IgnoreCase);
            //return new string(str.Where(c => !char.IsDigit(c)).ToArray());
        }

        public static string RemoveLetters(this string str)
        {
            string regex = "[a-z]";
            return Regex.Replace(str, regex, string.Empty, RegexOptions.IgnoreCase);
            //return new string(str.Where(c => !char.IsLetter(c)).ToArray());
        }

        public static string RemoveNonDigits(this string str)
        {
            string regex = "[^0-9]";
            return Regex.Replace(str, regex, string.Empty, RegexOptions.IgnoreCase);
            //return new string(str.Where(c => char.IsDigit(c)).ToArray());
        }

        public static int CountStringOccurrences(this string str, string pattern)
        {
            int num = 0;
            int startIndex = 0;
            while ((startIndex = str.IndexOf(pattern, startIndex, StringComparison.Ordinal)) != -1)
            {
                startIndex += pattern.Length;
                num++;
            }
            return num;
        }

        public static bool IsAllLetters(this string str)
        {
            foreach (char ch in str)
                if (!char.IsLetter(ch))
                    return false;

            return true;
        }

        public static bool IsAllDigits(this string str)
        {
            foreach (char ch in str)
                if (!char.IsDigit(ch))
                    return false;

            return true;
        }

        public static bool IsDigit(this char @char)
        {
            return IsAllDigits(@char.ToString());
        }

        public static bool IsValidEmail(this string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidRootedLocalPath(this string pathString)
        {
            //Regex driveCheck = new Regex(@"^[a-zA-Z]:\\$");
            Uri pathUri = null;
            bool isValidUri = Uri.TryCreate(pathString, UriKind.Absolute, out pathUri);

            return isValidUri && pathUri != null && pathUri.IsLoopback;
        }

        public static bool IsValidPath(this string pathString)
        {
            if (string.IsNullOrWhiteSpace(pathString) || pathString.Length < 3)
                return false;

            Regex driveCheck = new Regex(@"^[a-zA-Z]:\\$");
            if (!driveCheck.IsMatch(pathString.Substring(0, 3)))
                return false;

            string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidPathChars());
            strTheseAreInvalidFileNameChars += @":/?*" + "\"";
            Regex containsABadCharacter = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");
            if (containsABadCharacter.IsMatch(pathString.Substring(3, pathString.Length - 3)))
                return false;

            string tempPath = Path.GetFullPath(pathString);
            if (!Path.GetFileName(pathString).IsNullOrEmpty())
                tempPath = tempPath.Replace(Path.GetFileName(pathString), "");
            DirectoryInfo directoryInfo = new DirectoryInfo(tempPath);
            if (!directoryInfo.Exists)
                return false;

            return true;
        }

        public static bool IsValidIPv4(this string ipString)
        {
            if (ipString.Count(c => c == '.') != 3)
                return false;

            string portRegex = ":[0-9]*$";
            string finalIp = Regex.Replace(ipString, portRegex, "");

            return IPAddress.TryParse(finalIp, out IPAddress address);
        }

        public static bool IsValidHttpOrHttpsUri(this string uri)
        {
            bool result = Uri.TryCreate(uri, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
        }

        public static string RemoveDiacritics(this string str) //, bool removeSpace = true)
        {
            string regex = "[^a-z 0-9]";
            var result = Regex.Replace(str, regex, "", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            return result;
            //var normalizedString = str.Normalize(NormalizationForm.FormD);
            //var stringBuilder = new StringBuilder();

            //foreach (var c in normalizedString)
            //{
            //    var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            //    if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            //    {
            //        stringBuilder.Append(c);
            //    }
            //}

            //return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string RemoveSpecialCharacters(this string str, char[] exclusionTable = null)
        {
            string regex = "[^a-z0-9";
            if (exclusionTable != null)
            {
                foreach (char ch in exclusionTable)
                    regex += ch;
            }
            return Regex.Replace(str, regex + "]", string.Empty, RegexOptions.IgnoreCase);
        }

        public static string RemoveSpecialCharactersOnLeft(this string str, char[] exclusionTable = null)
        {
            string regex = "^[^a-z0-9";
            if (exclusionTable != null)
            {
                foreach (char ch in exclusionTable)
                    regex += ch;
            }
            return Regex.Replace(str, regex + "]+", string.Empty, RegexOptions.IgnoreCase);
        }

        public static string RemoveSpecialCharactersOnRight(this string str, char[] exclusionTable = null)
        {
            string regex = "[^a-z0-9";
            if (exclusionTable != null)
            {
                foreach (char ch in exclusionTable)
                    regex += ch;
            }
            return Regex.Replace(str, regex + "]+$", string.Empty, RegexOptions.IgnoreCase);
        }

        public static string RemoveSpecialCharactersOnLeftAndRight(this string str, char[] exclusionTable = null)
        {
            str = str.RemoveSpecialCharactersOnLeft(exclusionTable);
            return str.RemoveSpecialCharactersOnRight(exclusionTable);
        }

        public static string RemoveHtmlTags(this string str)
        {
            char[] array = new char[str.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < str.Length; i++)
            {
                char let = str[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }

        public static string CapitalizeWords(this string str, char wordSeparatorChar = ' ')
        {
            char[] array = str.ToCharArray();
            // Handle the first letter in the string.
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == wordSeparatorChar)
                {
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
            }
            return new string(array);
        }

        //public static string EncryptText(this string str)
        //{
        //    if (!string.IsNullOrEmpty(str))
        //    {
        //        string text = string.Empty;
        //        foreach (char ch in str)
        //            text += Convert.ToInt32(ch).ToString("x");
        //        str = text;
        //    }
        //    return str;
        //}

        public static string DecryptText(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                str = str.Replace(" ", "");
                byte[] bytes = new byte[str.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                    bytes[i] = Convert.ToByte(str.Substring(i * 2, 2), 0x10);

                str = Encoding.ASCII.GetString(bytes);
            }
            return str;
        }

        public static string Encrypt(this string str, string passPhrase)
        {
            return StringCipher.Encrypt(str, passPhrase);
        }

        public static string Encrypt(this string str)
        {
            return StringCipher.Encrypt(str);
        }

        public static string Decrypt(this string str)
        {
            return StringCipher.Decrypt(str);
        }

        public static string Decrypt(this string str, string passPhrase)
        {
            return StringCipher.Decrypt(str, passPhrase);
        }

        public static bool Contains(this string str, string pattern, StringComparison stringComparison)
        {
            return str.IndexOf(pattern, stringComparison) >= 0;
        }

        public static bool Contains(this string str, string pattern, bool matchCase, bool matchWholeWord)
        {
            string regex = pattern;
            if (matchWholeWord)
                regex = "\\b" + regex + "\\b";
            return Regex.IsMatch(str, regex, matchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
        }

        public static bool ContainsAny(this string sourceText, params string[] patterns)
        {
            return patterns.Any(sourceText.Contains);
            //foreach (string needle in patterns)
            //{
            //    if (sourceText.Contains(needle))
            //        return true;
            //}

            //return false;
        }

        public static string[] Split(this string self, char separator, int occurrence)
        {
            return self.Split(new string(separator, 1), occurrence);
        }

        public static string[] Split(this string self, string separator, int occurrence)
        {
            string[] chunks = self.Split(new[] { separator }, StringSplitOptions.None);
            string firstPart = string.Join(separator, chunks.Take(occurrence)) + separator;
            string secondPart = string.Join(separator, chunks.Skip(occurrence));
            return new string[] { firstPart, secondPart };
        }

        public static string Utf16ToUtf8(this string utf16String, Encoding outputStringEncoding)
        {
            var encodedString = Tools.ConvertStringEncoding(utf16String, Encoding.Unicode, Encoding.UTF8, outputStringEncoding);
            return encodedString;
        }

        public static string Utf16ToASCII(this string utf16String, Encoding outputStringEncoding)
        {
            var encodedString = Tools.ConvertStringEncoding(utf16String, Encoding.Unicode, Encoding.ASCII, outputStringEncoding);
            return encodedString;
        }
    }
}