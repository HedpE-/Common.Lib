using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace Common.Lib.AttachedProperties
{
    // http://dotnetharbour.blogspot.com/2014/10/mvvm-passwordbox-securepassword-and.html

    public static class PasswordBoxHelpers
    {
        public static readonly DependencyProperty BindablePasswordProperty =
            DependencyProperty.RegisterAttached("BindablePassword",
            typeof(SecureString), typeof(PasswordBoxHelpers),
            new FrameworkPropertyMetadata(new SecureString(), OnPasswordPropertyChanged));

        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached("BindPassword",
            typeof(bool), typeof(PasswordBoxHelpers), new PropertyMetadata(false, BindPassword));

        private static readonly DependencyProperty UpdatingPasswordProperty =
            DependencyProperty.RegisterAttached("UpdatingPassword", typeof(bool),
            typeof(PasswordBoxHelpers));

        public static void SetBindPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(BindPasswordProperty, value);
        }

        public static bool GetBindPassword(DependencyObject dp)
        {
            return (bool)dp.GetValue(BindPasswordProperty);
        }

        public static string GetBindablePassword(DependencyObject dp)
        {
            return (string)dp.GetValue(BindablePasswordProperty);
        }

        public static void SetBindablePassword(DependencyObject dp, SecureString value)
        {
            dp.SetValue(BindablePasswordProperty, value);
        }

        private static bool GetUpdatingPassword(DependencyObject dp)
        {
            return (bool)dp.GetValue(UpdatingPasswordProperty);
        }

        private static void SetUpdatingPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(UpdatingPasswordProperty, value);
        }

        private static void OnPasswordPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            if (passwordBox is PasswordBox)
            {
                if (!(bool)passwordBox.GetValue(UpdatingPasswordProperty))
                {
                    passwordBox.PasswordChanged -= PasswordChanged;
                    passwordBox.Password = ConvertToUnsecureString((SecureString)e.NewValue);
                    passwordBox.PasswordChanged += PasswordChanged;
                }
            }
        }

        // http://blogs.msdn.com/b/fpintos/archive/2009/06/12/how-to-properly-convert-securestring-to-string.aspx

        private static string ConvertToUnsecureString(SecureString securePassword)
        {
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

        private static void BindPassword(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            if (passwordBox is PasswordBox)
            {
                bool wasBound = (bool)(e.OldValue);
                bool needToBind = (bool)(e.NewValue);

                if (wasBound)
                    passwordBox.PasswordChanged -= PasswordChanged;

                if (needToBind)
                    passwordBox.PasswordChanged += PasswordChanged;
            }
        }

        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            if (passwordBox is PasswordBox)
            {
                passwordBox.SetValue(UpdatingPasswordProperty, true);
                SetBindablePassword(passwordBox, passwordBox.SecurePassword);
                passwordBox.SetValue(UpdatingPasswordProperty, false);
            }
        }
    }
}