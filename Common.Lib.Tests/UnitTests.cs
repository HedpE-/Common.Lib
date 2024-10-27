using Common.Lib.Enumerators;
using Common.Lib.Extensions;
using Common.Lib.HelperClasses;
using Common.Lib.JSON;
using Common.Lib.Security;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Common.Lib.Tests
{
    [TestFixture]
    public class UnitTests
    {
        [Test]
        public void Test_ExtractResourceFileToTempFile()
        {
            string assemblyName = "Selenium.Framework";
            string resourcePath = "Selenium.Framework.chromedriver.exe";
            string outputFilePath = Path.Combine(Path.GetTempPath(), "chromedriver.exe");

            if (File.Exists(outputFilePath))
                File.Delete(outputFilePath);

            var dllPath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
                .Parent.Parent.Parent.GetDirectories(assemblyName).FirstOrDefault()
                .GetDirectories("bin").FirstOrDefault()
                .GetDirectories("Debug").FirstOrDefault().FullName;

            dllPath = Path.Combine(dllPath, assemblyName + ".dll");

            Assert.IsTrue(File.Exists(dllPath));

            var dllBytes = File.ReadAllBytes(dllPath);

            Assert.IsNotNull(dllBytes);

            AppDomain.CurrentDomain.Load(dllBytes);

            Tools.ExtractResourceToLocalFile(resourcePath, assemblyName, outputFilePath);

            Assert.IsTrue(File.Exists(outputFilePath));
        }

        [Test]
        public void Test_GetAssemblyByName()
        {
            string name = "Common.Lib";
            var assembly = Tools.GetAssemblyByName(name);

            Assert.That(assembly, Is.Not.Null.And.Property("FullName").StartsWith(name));
        }

        [Test]
        public void TestEncryptText()
        {
            var filePath = Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName, "TJFlow.Core", "EnvironmentAccesses.json");
            //var json = File.ReadAllText(@"C:\Users\goncalvesr6\Desktop\EnvironmentAccesses.json");
            var json = File.ReadAllText(filePath);
            var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> dict = (Dictionary<string, object>)jsSerializer.DeserializeObject(json);

            EncryptAllPasswords(dict);

            json = JsonHelper.FormatJson(jsSerializer.Serialize(dict));
            //File.WriteAllText(@"C:\Users\goncalvesr6\Desktop\EnvironmentAccesses.json", json);
            File.WriteAllText(filePath, json);
        }

        [Test]
        public void TestDecryptText()
        {
            var filePath = Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName, "TJFlow.Core", "EnvironmentAccesses.json");
            //var json = File.ReadAllText(@"C:\Users\goncalvesr6\Desktop\EnvironmentAccesses.json");
            var json = File.ReadAllText(filePath);
            var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> dict = (Dictionary<string, object>)jsSerializer.DeserializeObject(json);

            DecryptAllPasswords(dict);

            json = JsonHelper.FormatJson(jsSerializer.Serialize(dict));
            //File.WriteAllText(@"C:\Users\goncalvesr6\Desktop\EnvironmentAccesses.json", json);
            File.WriteAllText(filePath, json);
        }

        private Dictionary<string, object> EncryptAllPasswords(Dictionary<string, object> dict)
        {
            for (int index = 0; index < dict.Count; index++)
            {
                var key = dict.ElementAt(index).Key;
                if (dict[key] is Dictionary<string, object> newDict)
                    dict[key] = EncryptAllPasswords(newDict);
                else
                {
                    if (key == "password")
                        dict[key] = dict[key].ToString().Encrypt();
                }
            }

            return dict;
        }

        private Dictionary<string, object> DecryptAllPasswords(Dictionary<string, object> dict)
        {
            for (int index = 0; index < dict.Count; index++)
            {
                var key = dict.ElementAt(index).Key;
                if (dict[key] is Dictionary<string, object> newDict)
                    dict[key] = DecryptAllPasswords(newDict);
                else
                {
                    if (key == "password")
                        dict[key] = dict[key].ToString().Decrypt();
                }
            }

            return dict;
        }

        [Test]
        public void Test_SecureCredentials_SaveToCredentialManager()
        {
            string target = "testTarget";
            string username = "testUser";
            string password = "testPassword!$*_?&";

            SecureCredential testCredentials = new SecureCredential(target, username, password, CredentialType.Generic);
            testCredentials.PersistanceType = PersistanceType.LocalComputer;

            if (testCredentials.ExistsOnCredentialManager())
                testCredentials.DeleteFromCredentialManager();

            Assert.IsFalse(testCredentials.ExistsOnCredentialManager());

            Assert.That(testCredentials, Is.Not.Null
                .And.Property("Username").EqualTo(username)
                .And.Property("SecurePassword").Property("Length").EqualTo(password.Length)
                .And.Property("TargetApplication").EqualTo(target)
                .And.Property("Type").EqualTo(CredentialType.Generic)
                .And.Property("PersistanceType").EqualTo(PersistanceType.LocalComputer));

            Assert.IsTrue(testCredentials.SaveToCredentialManager());

            testCredentials.Dispose();

            testCredentials = new SecureCredential(target);
            testCredentials.Type = CredentialType.Generic;

            Assert.That(testCredentials, Is.Not.Null
                .And.Property("SecurePassword").Property("Length").EqualTo(0)
                .And.Property("TargetApplication").EqualTo(target)
                .And.Property("Type").EqualTo(CredentialType.Generic));
            Assert.IsTrue(string.IsNullOrEmpty(testCredentials.Username));

            Assert.IsTrue(testCredentials.LoadFromCredentialManager());

            Assert.That(testCredentials, Is.Not.Null
                .And.Property("Username").EqualTo(username)
                .And.Property("SecurePassword").Property("Length").EqualTo(password.Length)
                .And.Property("TargetApplication").EqualTo(target)
                .And.Property("Type").EqualTo(CredentialType.Generic));
            Assert.AreEqual(password, testCredentials.GetUnsecurePassword());
        }

        [Test]
        public void Test_SecureCredentials_UpdateOnCredentialManager()
        {
            string target = "testTarget";
            string username = "testUser";
            string password1 = "testPassword!$*_?&";
            string password2 = "newTestPassword!$*_?&";

            SecureCredential testCredentials = new SecureCredential(target, username, password1, CredentialType.Generic);
            testCredentials.PersistanceType = PersistanceType.LocalComputer;

            if (testCredentials.ExistsOnCredentialManager())
                testCredentials.DeleteFromCredentialManager();

            Assert.IsFalse(testCredentials.ExistsOnCredentialManager());

            Assert.That(testCredentials, Is.Not.Null
                .And.Property("Username").EqualTo(username)
                .And.Property("SecurePassword").Property("Length").EqualTo(password1.Length)
                .And.Property("TargetApplication").EqualTo(target)
                .And.Property("Type").EqualTo(CredentialType.Generic)
                .And.Property("PersistanceType").EqualTo(PersistanceType.LocalComputer));

            Assert.IsTrue(testCredentials.SaveToCredentialManager());

            testCredentials.Dispose();

            testCredentials = new SecureCredential(target);
            testCredentials.Type = CredentialType.Generic;

            Assert.That(testCredentials, Is.Not.Null
                .And.Property("SecurePassword").Property("Length").EqualTo(0)
                .And.Property("TargetApplication").EqualTo(target)
                .And.Property("Type").EqualTo(CredentialType.Generic));
            Assert.IsTrue(string.IsNullOrEmpty(testCredentials.Username));

            Assert.IsTrue(testCredentials.LoadFromCredentialManager());

            Assert.That(testCredentials, Is.Not.Null
                .And.Property("Username").EqualTo(username)
                .And.Property("SecurePassword").Property("Length").EqualTo(password1.Length)
                .And.Property("TargetApplication").EqualTo(target)
                .And.Property("Type").EqualTo(CredentialType.Generic));
            Assert.AreEqual(password1, testCredentials.GetUnsecurePassword());

            testCredentials.SetPassword(password2);

            Assert.AreEqual(password2, testCredentials.GetUnsecurePassword());

            Assert.IsTrue(testCredentials.SaveToCredentialManager());

            testCredentials.Dispose();

            testCredentials = new SecureCredential(target);
            testCredentials.Type = CredentialType.Generic;

            Assert.That(testCredentials, Is.Not.Null
                .And.Property("SecurePassword").Property("Length").EqualTo(0)
                .And.Property("TargetApplication").EqualTo(target)
                .And.Property("Type").EqualTo(CredentialType.Generic));
            Assert.IsTrue(string.IsNullOrEmpty(testCredentials.Username));

            Assert.IsTrue(testCredentials.LoadFromCredentialManager());

            Assert.That(testCredentials, Is.Not.Null
                .And.Property("Username").EqualTo(username)
                .And.Property("SecurePassword").Property("Length").EqualTo(password2.Length)
                .And.Property("TargetApplication").EqualTo(target)
                .And.Property("Type").EqualTo(CredentialType.Generic));
            Assert.AreEqual(password2, testCredentials.GetUnsecurePassword());
        }

        [Test]
        public void Test_SecureCredentials_LoadFromCredentialManager()
        {
            string target = "testTarget";
            string username = "testUser";
            string password = "testPassword!$*_?&";

            SecureCredential testCredentials = new SecureCredential(target);
            testCredentials.Type = CredentialType.Generic;

            if (!testCredentials.ExistsOnCredentialManager())
            {
                testCredentials = new SecureCredential(target, username, password, CredentialType.Generic);
                testCredentials.PersistanceType = PersistanceType.LocalComputer;

                testCredentials.SaveToCredentialManager();

                testCredentials.Dispose();

                testCredentials = new SecureCredential(target);
                testCredentials.Type = CredentialType.Generic;
            }

            Assert.That(testCredentials, Is.Not.Null
                .And.Property("SecurePassword").Property("Length").EqualTo(0)
                .And.Property("TargetApplication").EqualTo(target)
                .And.Property("Type").EqualTo(CredentialType.Generic));
            Assert.IsTrue(string.IsNullOrEmpty(testCredentials.Username));

            Assert.IsTrue(testCredentials.ExistsOnCredentialManager());

            Assert.IsTrue(testCredentials.LoadFromCredentialManager());

            Assert.That(testCredentials, Is.Not.Null
                .And.Property("Username").EqualTo(username)
                .And.Property("SecurePassword").Property("Length").EqualTo(password.Length)
                .And.Property("TargetApplication").EqualTo(target)
                .And.Property("Type").EqualTo(CredentialType.Generic));
            Assert.AreEqual(password, testCredentials.GetUnsecurePassword());
        }

        [Test]
        public void Test_SecureCredentials_ExistsOnCredentialManager()
        {
            string target = "testTarget";
            string username = "testUser";
            string password = "testPassword!$*_?&";

            SecureCredential testCredentials = new SecureCredential(target);
            testCredentials.Type = CredentialType.Generic;

            if (testCredentials.ExistsOnCredentialManager())
                testCredentials.DeleteFromCredentialManager();

            Assert.IsFalse(testCredentials.ExistsOnCredentialManager());

            testCredentials = new SecureCredential(target, username, password, CredentialType.Generic);
            testCredentials.PersistanceType = PersistanceType.LocalComputer;

            Assert.That(testCredentials, Is.Not.Null
                .And.Property("Username").EqualTo(username)
                .And.Property("SecurePassword").Property("Length").EqualTo(password.Length)
                .And.Property("TargetApplication").EqualTo(target)
                .And.Property("Type").EqualTo(CredentialType.Generic)
                .And.Property("PersistanceType").EqualTo(PersistanceType.LocalComputer));

            Assert.IsTrue(testCredentials.SaveToCredentialManager());

            testCredentials.Dispose();

            testCredentials = new SecureCredential(target);
            testCredentials.Type = CredentialType.Generic;

            Assert.That(testCredentials, Is.Not.Null
                .And.Property("SecurePassword").Property("Length").EqualTo(0)
                .And.Property("TargetApplication").EqualTo(target)
                .And.Property("Type").EqualTo(CredentialType.Generic));
            Assert.IsTrue(string.IsNullOrEmpty(testCredentials.Username));

            Assert.IsTrue(testCredentials.ExistsOnCredentialManager());
        }

        [Test]
        public void Test_SecureCredentials_DeleteFromCredentialManager()
        {
            string target = "testTarget";
            string username = "testUser";
            string password = "testPassword!$*_?&";

            SecureCredential testCredentials = new SecureCredential(target);
            testCredentials.Type = CredentialType.Generic;

            if (!testCredentials.ExistsOnCredentialManager())
            {
                testCredentials = new SecureCredential(target, username, password, CredentialType.Generic);
                testCredentials.PersistanceType = PersistanceType.LocalComputer;

                testCredentials.SaveToCredentialManager();

                testCredentials.Dispose();

                testCredentials = new SecureCredential(target);
                testCredentials.Type = CredentialType.Generic;
            }

            Assert.IsTrue(testCredentials.ExistsOnCredentialManager());

            testCredentials.DeleteFromCredentialManager();

            Assert.IsFalse(testCredentials.ExistsOnCredentialManager());
        }

        [Test]
        public void BuildClass()
        {
            List<string> privateMembers = new List<string>();
            privateMembers.AddRange(new string[] {
                "id_fi_site",
                "id_fi_site_locl",
                "ds_fi_orgn_schm",
                "cd_fi_site",
                "ds_fi_addr",
                "ds_fi_site_post_code",
                "ds_fi_site_city",
                "id_fi_site_coun",
                "ds_fi_coun",
                "id_fi_site_stat",
                "ds_fi_stat",
                "ds_fi_site_cout",
                "id_fi_site_orgn_lat",
                "id_fi_site_orgn_long",
                "ds_fi_site_clli",
                "id_fi_site_role",
                "ds_fi_site_role",
                "id_fi_site_stts",
                "ds_fi_stts",
                "ds_fi_site_acce_rest",
                "fl_fi_site_ligh_loca",
                "fl_fi_site_crit_site",
                "ds_fi_site_comm",
                "fl_fi_site_shrd_site",
                "ds_fi_site_oper_code",
                "id_fi_site_pow_prov_circ",
                "ds_fi_site_last_mod_user",
                "dt_fi_site_last_mod",
                "tm_fi_site_last_mod",
                "fl_del",
                "tm_crtn",
                "tm_last_upt",
                "dt_exec",
                "id_srvc_prvd",
                "qt_fi_eqpt_siu",
                "qt_fi_eqpt_bts_2g",
                "qt_fi_eqpt_nodeb_3g",
                "qt_fi_eqpt_enodeb_4g",
                "qt_fi_eqpt_gnodeb_5g",
                "qt_fi_eqpt_cells_2g",
                "qt_fi_eqpt_cells_3g",
                "qt_fi_eqpt_cells_4g",
                "qt_fi_eqpt_cells_5g"
            });

            List<string> publicMembers = new List<string>();
            for (int i = 0; i < privateMembers.Count; i++)
            {
                //var temp_member = privateMembers[i].Replace("private string ", "");
                //temp_member = temp_member.Replace(";", "");
                //privateMembers[i] = $"private string {privateMembers[i]};";

                var public_member = "public string " + privateMembers[i].CapitalizeWords('_').Replace("_", "");
                public_member += "\n{";
                public_member += $"\n   get {{ return {privateMembers[i]}; }}";
                public_member += "\n   set";
                public_member += "\n   {";
                public_member += $"\n       if ({privateMembers[i]} != value)";
                public_member += "\n       {";
                public_member += $"\n           {privateMembers[i]} = value;";
                public_member += $"\n           RaisePropertyChanged();";
                public_member += "\n       }";
                public_member += "\n    }";
                public_member += "\n}";

                publicMembers.Add(public_member);

                privateMembers[i] = $"private string {privateMembers[i]};";
            }

            string finalClassString = "";
            for (int i = 0; i < privateMembers.Count; i++)
            {
                //finalClassString += $"//[FieldOrder({i+1})]\n";
                //finalClassString += "//[FieldTrim(TrimMode.Both)]\n";
                //finalClassString += "//[FieldQuoted('§', QuoteMode.AlwaysQuoted, MultilineMode.NotAllow)]\n";
                finalClassString += privateMembers[i];
                finalClassString += "\n";
                finalClassString += publicMembers[i];
                if (i < privateMembers.Count - 1)
                    finalClassString += "\n\n";
            }

            System.Diagnostics.Debug.WriteLine(finalClassString);
        }

        [Test]
        public void InsertPropInClassMembers()
        {
            var classString = File.ReadAllText("C:\\Users\\goncalvesr6\\Desktop\\test.txt").Split('\n');
            for (int c = 0; c < classString.Length; c++)
            {
                if (classString[c].Trim().StartsWith("[FieldQuoted("))
                {
                    int pos = classString[c].IndexOf("[FieldQuoted(") + "[FieldQuoted(".Length;
                    classString[c] = classString[c].Insert(pos, "'§', ");
                }
            }

            var finalString = string.Join("\n", classString);
        }
    }
}
