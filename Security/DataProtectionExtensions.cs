﻿using System;
using System.Security.Cryptography;
using System.Text;

namespace Common.Lib.Extensions
{
    public static class DataProtectionExtensions
    {
        public static string Protect(
            this string clearText,
            string optionalEntropy = null,
            DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            if (string.IsNullOrEmpty(clearText))
                return string.Empty;

            //if (clearText == null)
            //throw new ArgumentNullException("clearText");

            byte[] clearBytes = Encoding.UTF8.GetBytes(clearText);
            byte[] entropyBytes = string.IsNullOrEmpty(optionalEntropy)
                ? null
                : Encoding.UTF8.GetBytes(optionalEntropy);
            byte[] encryptedBytes = ProtectedData.Protect(clearBytes, entropyBytes, scope);
            return Convert.ToBase64String(encryptedBytes);
        }

        public static string Unprotect(
            this string encryptedText,
            string optionalEntropy = null,
            DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            //if (encryptedText == null)
            //    throw new ArgumentNullException("encryptedText");

            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] entropyBytes = string.IsNullOrEmpty(optionalEntropy)
                ? null
                : Encoding.UTF8.GetBytes(optionalEntropy);
            byte[] clearBytes = ProtectedData.Unprotect(encryptedBytes, entropyBytes, scope);
            return Encoding.UTF8.GetString(clearBytes);
        }
    }
}
