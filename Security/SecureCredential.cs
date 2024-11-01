﻿using Common.Lib.Enumerators;
using Common.Lib.Extensions;
using Common.Lib.HelperClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace Common.Lib.Security
{
    public class SecureCredential : ObservableObject, ICredentials, IEquatable<SecureCredential>, IDisposable
    {
        static object _lockObject = new object();
        bool _disposed;

        static SecurityPermission _unmanagedCodePermission;

        private string _targetApplication;
        public string TargetApplication
        {
            get
            {
                CheckNotDisposed();
                return _targetApplication;
            }
            set
            {
                CheckNotDisposed();
                if (_targetApplication != value)
                {
                    _targetApplication = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _domain;
        public string Domain
        {
            get { return _domain; }
            set
            {
                if (_domain != value)
                {
                    _domain = value;
                    RaisePropertyChanged();
                }
            }
        }

        string _description;
        public string Description
        {
            get
            {
                CheckNotDisposed();
                return _description;
            }
            set
            {
                CheckNotDisposed();
                _description = value;
                RaisePropertyChanged();
            }
        }

        private string _username;
        public string Username
        {
            get
            {
                CheckNotDisposed();
                return _username;
            }
            set
            {
                CheckNotDisposed();
                if (_username != value)
                {
                    _username = value;
                    RaisePropertyChanged();
                    IsValid = !string.IsNullOrEmpty(Username) && SecurePassword.Length > 0;
                }
            }
        }

        private SecureString _securePassword;
        public SecureString SecurePassword
        {
            get
            {
                CheckNotDisposed();
                _unmanagedCodePermission.Demand();
                return null == _securePassword ? new SecureString() : _securePassword.Copy();
            }
            set
            {
                CheckNotDisposed();
                if (_securePassword != null)
                {
                    _securePassword.Clear();
                    _securePassword.Dispose();
                }
                _securePassword = null == value ? new SecureString() : value.Copy();
                RaisePropertyChanged();
                IsValid = !string.IsNullOrEmpty(Username) && SecurePassword.Length > 0;
            }
        }

        DateTime _lastWriteTime;
        public DateTime LastWriteTime
        {
            get
            {
                return LastWriteTimeUtc.ToLocalTime();
            }
        }
        public DateTime LastWriteTimeUtc
        {
            get
            {
                CheckNotDisposed();
                return _lastWriteTime;
            }
            private set 
            {
                _lastWriteTime = value;
                RaisePropertyChanged();
            }
        }

        CredentialType _type;
        public CredentialType Type
        {
            get
            {
                CheckNotDisposed();
                return _type;
            }
            set
            {
                CheckNotDisposed();
                _type = value;
                RaisePropertyChanged();
            }
        }

        PersistanceType _persistanceType;
        public PersistanceType PersistanceType
        {
            get
            {
                CheckNotDisposed();
                return _persistanceType;
            }
            set
            {
                CheckNotDisposed();
                _persistanceType = value;
                RaisePropertyChanged();
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


        private Dictionary<string, string> _customFlags;
        public ReadOnlyDictionary<string, string> CustomFlags
        {
            get { return _customFlags.AsReadOnlyDictionary(); }
            private set
            {
                if (value == null)
                {
                     if(_customFlags != null)
                    {
                        _customFlags = null;
                        RaisePropertyChanged();
                    }
                }
                else
                {
                    _customFlags = new Dictionary<string, string>(value);
                    RaisePropertyChanged();
                }
            }
        }

        public readonly CredentialsSettings Settings;

        static SecureCredential()
        {
            lock (_lockObject)
            {
                _unmanagedCodePermission = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
            }
        }

        public SecureCredential(CredentialsSettings settings = null)
        {
            SecurePassword = new SecureString();
            Settings = settings ?? new CredentialsSettings();
            PersistanceType = PersistanceType.Session;
            _lastWriteTime = DateTime.MinValue;
            ResetCustomFlags();
        }

        public SecureCredential(string targetApplication, CredentialsSettings settings = null) : this(settings)
        {
            TargetApplication = targetApplication;
        }

        public SecureCredential(string targetApplication, string username, SecureString password, CredentialsSettings settings = null) : this(targetApplication, settings)
        {
            Username = username;
            SecurePassword = password;
        }

        public SecureCredential(string targetApplication, string username, string password, CredentialsSettings settings = null) : this(targetApplication, username, ConvertToSecureString(password), settings) { }

        public SecureCredential(string targetApplication, string username, SecureString password, CredentialType type, CredentialsSettings settings = null) : this(targetApplication, username, password, settings)
        {
            Type = type;
        }

        public SecureCredential(string targetApplication, string username, string password, CredentialType type, CredentialsSettings settings = null) : this(targetApplication, username, ConvertToSecureString(password), type, settings) { }

        ~SecureCredential()
        {
            Dispose(false);
        }

        public void AddCustomFlag(string key, string value)
        {
            if (CustomFlags == null)
                ResetCustomFlags();

            if (!_customFlags.ContainsKey(key) || value != _customFlags[key])
            {
                _customFlags[key] = value;
                RaisePropertyChanged("CustomFlags");
            }
        }

        public void RemoveCustomFlag(string key)
        {
            if (CustomFlags != null && _customFlags.ContainsKey(key))
            {
                _customFlags.Remove(key);
                RaisePropertyChanged("CustomFlags");
            }
        }

        public void ResetCustomFlags()
        {
            CustomFlags = new Dictionary<string, string>().AsReadOnlyDictionary();
        }

        public void Clear()
        {
            Username = null;
            SecurePassword = new SecureString();
            Domain = null;
            ResetCustomFlags();
            PersistanceType = PersistanceType.Session;
            _lastWriteTime = DateTime.MinValue;
        }

        public void SetPassword(string password)
        {
            SecurePassword = ConvertToSecureString(password);
        }

        public void ValidateCredentials()
        {
            IsValid = !string.IsNullOrEmpty(Username) && SecurePassword.Length > 0;
        }

        public string GetUnsecurePassword()
        {
            return ConvertToUnsecureString(SecurePassword);
        }

        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            return new NetworkCredential(Username, SecurePassword);
        }

        // http://blogs.msdn.com/b/fpintos/archive/2009/06/12/how-to-properly-convert-securestring-to-string.aspx

        private string ConvertToUnsecureString(SecureString securePassword)
        {
            if (securePassword == null)
                throw new ArgumentNullException("securePassword");
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        private static SecureString ConvertToSecureString(string inputString)
        {
            if (inputString == null)
                throw new ArgumentNullException("inputString");
            unsafe
            {
                fixed (char* inputChars = inputString)
                {
                    var securePassword = new SecureString(inputChars, inputString.Length);
                    securePassword.MakeReadOnly();
                    return securePassword;
                }
            }
        }

        private static bool CompareSecureStringEquality(SecureString ss1, SecureString ss2)
        {
            IntPtr bstr1 = IntPtr.Zero;
            IntPtr bstr2 = IntPtr.Zero;
            try
            {
                bstr1 = Marshal.SecureStringToBSTR(ss1);
                bstr2 = Marshal.SecureStringToBSTR(ss2);
                int length1 = Marshal.ReadInt32(bstr1, -4);
                int length2 = Marshal.ReadInt32(bstr2, -4);
                if (length1 == length2)
                {
                    for (int x = 0; x < length1; ++x)
                    {
                        byte b1 = Marshal.ReadByte(bstr1, x);
                        byte b2 = Marshal.ReadByte(bstr2, x);
                        if (b1 != b2) return false;
                    }
                }
                else return false;
                return true;
            }
            finally
            {
                if (bstr2 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr2);
                if (bstr1 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr1);
            }
        }

        public bool SaveToCredentialManager()
        {
            CheckNotDisposed();
            _unmanagedCodePermission.Demand();

            if (SecurePassword.Length > (512))
                throw new ArgumentOutOfRangeException("The password has exceeded 512 bytes.");

            CredentialManagement.CREDENTIAL credential = new CredentialManagement.CREDENTIAL();
            credential.TargetName = TargetApplication;
            credential.TargetAlias = Domain;
            credential.UserName = Username;
            credential.CredentialBlob = Marshal.StringToCoTaskMemUni(GetUnsecurePassword());
            credential.CredentialBlobSize = Encoding.Unicode.GetBytes(GetUnsecurePassword()).Length;
            credential.Comment = Description;
            credential.Type = (int)Type;
            credential.Persist = (int)PersistanceType;

            bool result = CredentialManagement.CredWrite(ref credential, 0);
            if (!result)
                return false;

            LastWriteTimeUtc = DateTime.UtcNow;
            return true;
        }

        public bool DeleteFromCredentialManager()
        {
            CheckNotDisposed();
            _unmanagedCodePermission.Demand();

            if (string.IsNullOrEmpty(TargetApplication))
            {
                throw new InvalidOperationException("Target must be specified to delete a credential.");
            }

            StringBuilder target = string.IsNullOrEmpty(TargetApplication) ? new StringBuilder() : new StringBuilder(TargetApplication);
            bool result = CredentialManagement.CredDelete(target, Type, 0);
            return result;
        }

        public static SecureCredential LoadFromCredentialManager(string targetApplication, CredentialType type)
        {
            _unmanagedCodePermission.Demand();

            IntPtr credPointer;

            bool result = CredentialManagement.CredRead(targetApplication, type, 0, out credPointer);
            if (!result)
                return null;
            using (CredentialManagement.CriticalCredentialHandle credentialHandle = new CredentialManagement.CriticalCredentialHandle(credPointer))
            {
                var secureCredential = new SecureCredential();
                secureCredential.LoadInternal(credentialHandle.GetCredential());
                return secureCredential;
            }
        }

        public bool LoadFromCredentialManager()
        {
            CheckNotDisposed();
            _unmanagedCodePermission.Demand();

            IntPtr credPointer;

            bool result = CredentialManagement.CredRead(TargetApplication, Type, 0, out credPointer);
            if (!result)
                return false;

            using (CredentialManagement.CriticalCredentialHandle credentialHandle = new CredentialManagement.CriticalCredentialHandle(credPointer))
                LoadInternal(credentialHandle.GetCredential());

            return true;
        }

        public bool ExistsOnCredentialManager()
        {
            CheckNotDisposed();
            _unmanagedCodePermission.Demand();

            if (string.IsNullOrEmpty(TargetApplication))
            {
                throw new InvalidOperationException("Target must be specified to check existance of a credential.");
            }

            using (SecureCredential existing = new SecureCredential { TargetApplication = TargetApplication, Type = Type })
            {
                return existing.LoadFromCredentialManager();
            }
        }

        internal void LoadInternal(CredentialManagement.CREDENTIAL credential)
        {
            Username = credential.UserName;
            if (credential.CredentialBlobSize > 0)
                SetPassword(Marshal.PtrToStringUni(credential.CredentialBlob, credential.CredentialBlobSize / 2));

            TargetApplication = credential.TargetName;
            Domain = credential.TargetAlias;
            Type = (CredentialType)credential.Type;
            PersistanceType = (PersistanceType)credential.Persist;
            Description = credential.Comment;
            LastWriteTimeUtc = DateTime.FromFileTimeUtc(credential.LastWritten);
        }

        /// <summary>
        ///     Loads all credentials
        /// </summary>
        //public static IEnumerable<SecureCredential> LoadAllFromCredentialManager()
        //{
        //    _unmanagedCodePermission.Demand();

        //    return CredentialManagement.CredEnumerate()
        //        .Select(c => new Credential(c.UserName, null, c.TargetName))
        //        .Where(c => c.Load());
        //}

        public void Dispose()
        {
            Dispose(true);

            // Prevent GC Collection since we have already disposed of this object
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    SecurePassword.Clear();
                    SecurePassword.Dispose();
                }

                _disposed = true;
            }
        }

        private void CheckNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("SecureCredential object is already disposed.");
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SecureCredential);
        }

        public bool Equals(SecureCredential other)
        {
            return other != null &&
                   _disposed == other._disposed &&
                   TargetApplication == other.TargetApplication &&
                   Domain == other.Domain &&
                   Description == other.Description &&
                   Username == other.Username &&
                   EqualityComparer<SecureString>.Default.Equals(SecurePassword, other.SecurePassword) &&
                   LastWriteTime == other.LastWriteTime &&
                   Type == other.Type &&
                   PersistanceType == other.PersistanceType &&
                   IsValid == other.IsValid &&
                   EqualityComparer<CredentialsSettings>.Default.Equals(Settings, other.Settings);
        }

        public override int GetHashCode()
        {
            int hashCode = 1681013262;
            hashCode = hashCode * -1521134295 + _disposed.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TargetApplication);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Domain);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Username);
            hashCode = hashCode * -1521134295 + EqualityComparer<SecureString>.Default.GetHashCode(SecurePassword);
            hashCode = hashCode * -1521134295 + LastWriteTime.GetHashCode();
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + PersistanceType.GetHashCode();
            hashCode = hashCode * -1521134295 + IsValid.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<CredentialsSettings>.Default.GetHashCode(Settings);
            return hashCode;
        }

        public static bool operator ==(SecureCredential left, SecureCredential right)
        {
            return EqualityComparer<SecureCredential>.Default.Equals(left, right);
        }

        public static bool operator !=(SecureCredential left, SecureCredential right)
        {
            return !(left == right);
        }
    }
}
