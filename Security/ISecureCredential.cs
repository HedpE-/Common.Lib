using Common.Lib.Enumerators;
using System;
using System.Net;
using System.Security;

namespace Common.Lib.Security
{
    public interface ISecureCredential : ICredentials
    {
        string Description { get; set; }
        string Domain { get; set; }
        bool IsValid { get; }
        SecureString SecurePassword { get; set; }
        string TargetApplication { get; set; }
        string Username { get; set; }

        string GetUnsecurePassword();
        void SetPassword(string password);
        void ValidateCredentials();
    }
}