using Common.Lib.Extensions;
using Common.Lib.HelperClasses;

namespace Common.Lib.Security
{
    public class Credentials : ObservableObject
    {
        public string TargetApplication { get; private set; }

        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                if (_username != value)
                {
                    _username = value;
                    RaisePropertyChanged();
                    IsValid = !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
                }
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                if (Unprotect(_password) != value)
                {
                    _password = value.Protect();
                    RaisePropertyChanged();
                    IsValid = !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
                }
            }
        }

        private bool _isValid;
        public bool IsValid
        {
            get { return _isValid; }
            private set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    RaisePropertyChanged();
                }
            }
        }

        private readonly CredentialsSettings Settings;

        public Credentials() { }

        public Credentials(string targetApplication) : this()
        {
            TargetApplication = targetApplication;
        }

        public Credentials(string targetApplication, string username, string password, CredentialsSettings settings = null) : this(targetApplication)
        {
            Username = username;
            Password = password;

            Settings = settings ?? new CredentialsSettings();
        }

        public Credentials(string username, string password, CredentialsSettings settings = null) : this(string.Empty, username, password, settings) { }

        public static string Unprotect(string encryptedPassword) => encryptedPassword.Unprotect();
    }

    public class CredentialsSettings
    {
        //[Flags]
        //public enum CharacterGroups
        //{
        //    UpperCase,
        //    LowerCase,
        //    Symbols,
        //    Digits,
        //    Space
        //}

        public int UsernameMinimumLength { get; set; } = 0;
        public byte[] UsernameForbiddenCharacters { get; set; }

        private int PasswordMinimumLength { get; set; } = 0;
        public byte[] PasswordForbiddenCharacters { get; set; }

        public CredentialsSettings()
        {
            UsernameForbiddenCharacters = new byte[0];
            PasswordForbiddenCharacters = new byte[0];
        }
    }
}
