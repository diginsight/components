#region using
using Common;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
#endregion

namespace Common
{
    public class RegistryHelper
    {
        private ILogger<RegistryHelper> logger;
        #region internal state
        public static Type T = typeof(RegistryHelper);

        #endregion

        public static void ProtectDocumentCommandsRegister()
        {
            using (var sec = TraceLogger.BeginMethodScope(T))
            {
                var process = Process.GetCurrentProcess();
                //MessageBox.Show($"ProtectDocumentCommandsRegister START (pid: {process.Id})");

                var extentions = new[] { ".pdf", ".docx", ".xlsx", ".pptx" };
                var componentFolder = Path.GetDirectoryName(T.Assembly.Location);

                var users = Registry.Users.GetSubKeyNames();

                var installForAllUsers = true;
                var writeHive = installForAllUsers ? "HKEY_LOCAL_MACHINE" : "HKEY_CURRENT_USER";

                foreach (var extention in extentions)
                {
                    var applicationName = Registry.GetValue($@"HKEY_CLASSES_ROOT\{extention}", null, null) as string; // sec.Debug($@"Registry.GetValue($""HKEY_CURRENT_USER\SOFTWARE\Classes\{extention}"", null, null) returned {applicationName}");
                    if (string.IsNullOrEmpty(applicationName)) { applicationName = Registry.GetValue($@"HKEY_CURRENT_USER\SOFTWARE\Classes\{extention}", null, null) as string; }
                    if (string.IsNullOrEmpty(applicationName)) { applicationName = Registry.GetValue($@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\{extention}", null, null) as string; }
                    if (!string.IsNullOrEmpty(applicationName))
                    {
                        //lines.Add("1)");
                        //lines.Add($@"{writeHive}\SOFTWARE\Classes\{applicationName}\shell");
                        //lines.Add(Registry.GetValue($@"HKEY_CLASSES_ROOT\SOFTWARE\Classes\.pdf\OpenWithProgids", null, null) as string);
                        //lines.Add(Registry.GetValue($@"HKEY_CURRENT_USER\SOFTWARE\Classes\.pdf\OpenWithProgids", null, null) as string);

                        if (Registry.GetValue($@"{writeHive}\SOFTWARE\Classes\{applicationName}", null, null) == null)
                        {
                            Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{applicationName}", null, "");
                        }

                        Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{applicationName}\shell", null, "Open");
                        Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{applicationName}\shell\Classifica e proteggi", null, "");
                        Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{applicationName}\shell\Classifica e proteggi", "Icon", $@"{componentFolder}\ProtectDocument1.ico");
                        Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{applicationName}\shell\Classifica e proteggi\command", null, $@"""{componentFolder}\AIPCustomProtection.Windows.exe"" ""%1""");
                    }

                    if (extention == ".pdf")
                    {
                        foreach (var user in users)
                        {
                            using (var key = Registry.Users.OpenSubKey($@"{user}\.pdf\OpenWithProgids", false))
                            {
                                var valueNames = key?.GetValueNames()?.Where(n => !string.IsNullOrEmpty(n))?.ToList();
                                valueNames?.ForEach(progID =>
                                {
                                    //lines.Add("2)");
                                    //lines.Add(progID);

                                    if (!string.IsNullOrEmpty(progID))
                                    {
                                        if (Registry.GetValue($@"{writeHive}\SOFTWARE\Classes\{progID}", null, null) == null)
                                        {
                                            Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{progID}", null, "");
                                        }
                                        //lines.Add($@"{writeHive}\SOFTWARE\Classes\{progID}");
                                        Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{progID}\shell", null, "Open");
                                        Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{progID}\shell\Classifica e proteggi", null, "");
                                        Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{progID}\shell\Classifica e proteggi", "Icon", $@"{componentFolder}\ProtectDocument1.ico");
                                        Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{progID}\shell\Classifica e proteggi\command", null, $@"""{componentFolder}\AIPCustomProtection.Windows.exe"" ""%1""");
                                    }
                                });
                            }
                        }
                    }
                    using (var key = Registry.ClassesRoot.OpenSubKey($@"{extention}\OpenWithProgids", false))
                    {
                        var valueNames = key?.GetValueNames()?.Where(n => !string.IsNullOrEmpty(n))?.ToList();
                        valueNames?.ForEach(progID =>
                        {
                            //lines.Add("2)");
                            //lines.Add(progID);

                            if (!string.IsNullOrEmpty(progID))
                            {
                                if (Registry.GetValue($@"{writeHive}\SOFTWARE\Classes\{progID}", null, null) == null)
                                {
                                    Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{progID}", null, "");
                                }
                                //lines.Add($@"{writeHive}\SOFTWARE\Classes\{progID}");
                                Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{progID}\shell", null, "Open");
                                Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{progID}\shell\Classifica e proteggi", null, "");
                                Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{progID}\shell\Classifica e proteggi", "Icon", $@"{componentFolder}\ProtectDocument1.ico");
                                Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{progID}\shell\Classifica e proteggi\command", null, $@"""{componentFolder}\AIPCustomProtection.Windows.exe"" ""%1""");
                            }
                        });
                    }

                    using (var key = Registry.CurrentUser.OpenSubKey($@"SOFTWARE\Classes\{extention}\OpenWithProgids", false))
                    {
                        var valueNames = key?.GetValueNames()?.Where(n => !string.IsNullOrEmpty(n))?.ToList();
                        valueNames?.ForEach(progID =>
                        {
                            //lines.Add("3)");
                            //lines.Add(progID);
                            if (!string.IsNullOrEmpty(progID))
                            {
                                if (Registry.GetValue($@"{writeHive}\SOFTWARE\Classes\{applicationName}", null, null) == null)
                                {
                                    Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{applicationName}", null, "");
                                }
                                //lines.Add($@"{writeHive}\SOFTWARE\Classes\{progID}");
                                Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{progID}\shell", null, "Open");
                                Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{progID}\shell\Classifica e proteggi", null, "");
                                Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{progID}\shell\Classifica e proteggi", "Icon", $@"{componentFolder}\ProtectDocument1.ico");
                                Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{progID}\shell\Classifica e proteggi\command", null, $@"""{componentFolder}\AIPCustomProtection.Windows.exe"" ""%1""");
                            }
                        });
                    }

                    using (var key = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Classes\{extention}\OpenWithProgids", false))
                    {
                        var valueNames = key?.GetValueNames()?.Where(n => !string.IsNullOrEmpty(n))?.ToList();
                        valueNames?.ForEach(progID =>
                        {
                            //lines.Add("4)");
                            //lines.Add(progID);
                            if (!string.IsNullOrEmpty(progID))
                            {
                                if (Registry.GetValue($@"{writeHive}\SOFTWARE\Classes\{applicationName}", null, null) == null)
                                {
                                    Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{applicationName}", null, "");
                                }
                                //lines.Add($@"{writeHive}\SOFTWARE\Classes\{progID}");
                                Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{progID}\shell", null, "Open");
                                Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{progID}\shell\Classifica e proteggi", null, "");
                                Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{progID}\shell\Classifica e proteggi", "Icon", $@"{componentFolder}\ProtectDocument1.ico");
                                Registry.SetValue($@"{writeHive}\SOFTWARE\Classes\{progID}\shell\Classifica e proteggi\command", null, $@"""{componentFolder}\AIPCustomProtection.Windows.exe"" ""%1""");
                            }
                        });
                    }
                }
                //System.IO.File.WriteAllLines(@"C:\Users\Public\WriteText.txt", lines.ToArray());
            }
        }
        public static void ProtectDocumentCommandsRemove()
        {
            var extentions = new[] { ".pdf", ".docx", ".xlsx", ".pptx" };
            var componentFolder = Path.GetDirectoryName(T.Assembly.Location);

            var users = Registry.Users.GetSubKeyNames();

            var installForAllUsers = true;
            var writeHive = installForAllUsers ? "HKEY_LOCAL_MACHINE" : "HKEY_CURRENT_USER";
            Func<string, bool, RegistryKey> openSubKey = installForAllUsers ? (Func<string, bool, RegistryKey>)Registry.LocalMachine.OpenSubKey : Registry.CurrentUser.OpenSubKey;

            foreach (var extention in extentions)
            {
                var applicationName = Registry.GetValue($@"HKEY_CLASSES_ROOT\{extention}", null, null) as string; // sec.Debug($@"Registry.GetValue($""HKEY_CURRENT_USER\SOFTWARE\Classes\{extention}"", null, null) returned {applicationName}");
                if (string.IsNullOrEmpty(applicationName)) { applicationName = Registry.GetValue($@"HKEY_CURRENT_USER\SOFTWARE\Classes\{extention}", null, null) as string; }
                if (string.IsNullOrEmpty(applicationName)) { applicationName = Registry.GetValue($@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\{extention}", null, null) as string; }
                if (!string.IsNullOrEmpty(applicationName))
                {
                    using (var key = openSubKey($@"SOFTWARE\Classes\{applicationName}\shell", true))
                    {
                        try
                        {
                            key?.DeleteSubKeyTree("Classifica e proteggi");
                        }
                        catch (Exception) { }
                    }

                    foreach (var user in users)
                    {
                        using (var key = Registry.Users.OpenSubKey($@"{user}\SOFTWARE\Classes\{applicationName}\shell", true))
                        {
                            try
                            {
                                key?.DeleteSubKeyTree("Classifica e proteggi");
                            }
                            catch (Exception) { }
                        }
                    }
                }

                using (var key = Registry.ClassesRoot.OpenSubKey($@"{extention}\OpenWithProgids", false))
                {
                    var valueNames = key?.GetValueNames()?.Where(n => !string.IsNullOrEmpty(n))?.ToList();
                    valueNames?.ForEach(progID =>
                    {
                        if (!string.IsNullOrEmpty(progID))
                        {
                            using (var progIDkey = openSubKey($@"SOFTWARE\Classes\{progID}\shell", true))
                            {
                                try
                                {
                                    progIDkey?.DeleteSubKeyTree("Classifica e proteggi");
                                }
                                catch (Exception) { }
                            }
                        }
                    });
                }

                foreach (var user in users)
                {
                    using (var key = Registry.Users.OpenSubKey($@"{user}\SOFTWARE\Classes\{extention}\OpenWithProgids", false))
                    {
                        var valueNames = key?.GetValueNames()?.Where(n => !string.IsNullOrEmpty(n))?.ToList();
                        valueNames?.ForEach(progID =>
                        {
                            if (!string.IsNullOrEmpty(progID))
                            {
                                using (var progIDkey = Registry.Users.OpenSubKey($@"{user}\SOFTWARE\Classes\{progID}\shell", true))
                                {
                                    try
                                    {
                                        progIDkey?.DeleteSubKeyTree("Classifica e proteggi");
                                    }
                                    catch (Exception) { }
                                }
                                using (var progIDkey = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Classes\{progID}\shell", true))
                                {
                                    try
                                    {
                                        progIDkey?.DeleteSubKeyTree("Classifica e proteggi");
                                    }
                                    catch (Exception) { }
                                }
                            }
                        });
                    }
                }
                using (var key = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Classes\{extention}\OpenWithProgids", false))
                {
                    var valueNames = key?.GetValueNames()?.Where(n => !string.IsNullOrEmpty(n))?.ToList();
                    valueNames?.ForEach(progID =>
                    {
                        if (!string.IsNullOrEmpty(progID))
                        {
                            using (var progIDkey = openSubKey($@"SOFTWARE\Classes\{progID}\shell", true))
                            {
                                try
                                {
                                    progIDkey?.DeleteSubKeyTree("Classifica e proteggi");
                                }
                                catch (Exception) { }
                            }
                        }
                    });
                }
            }
        }

        public static void ProtectDocumentCommandsRemoveOldKeys()
        {
            var extentions = new[] { ".pdf", ".docx", ".xlsx", ".pptx" };
            var componentFolder = Path.GetDirectoryName(T.Assembly.Location);

            var users = Registry.Users.GetSubKeyNames();

            var installForAllUsers = true;
            var writeHive = installForAllUsers ? "HKEY_LOCAL_MACHINE" : "HKEY_CURRENT_USER";
            Func<string, bool, RegistryKey> openSubKey = installForAllUsers ? (Func<string, bool, RegistryKey>)Registry.LocalMachine.OpenSubKey : Registry.CurrentUser.OpenSubKey;

            foreach (var extention in extentions)
            {
                var applicationName = Registry.GetValue($@"HKEY_CLASSES_ROOT\{extention}", null, null) as string; // sec.Debug($@"Registry.GetValue($""HKEY_CURRENT_USER\SOFTWARE\Classes\{extention}"", null, null) returned {applicationName}");
                if (string.IsNullOrEmpty(applicationName)) { applicationName = Registry.GetValue($@"HKEY_CURRENT_USER\SOFTWARE\Classes\{extention}", null, null) as string; }
                if (string.IsNullOrEmpty(applicationName)) { applicationName = Registry.GetValue($@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\{extention}", null, null) as string; }
                if (!string.IsNullOrEmpty(applicationName))
                {
                    using (var key = openSubKey($@"SOFTWARE\Classes\{applicationName}\shell", true))
                    {
                        try
                        {
                            key?.DeleteSubKeyTree("Proteggi Documento");
                        }
                        catch (Exception) { }
                    }

                    foreach (var user in users)
                    {
                        using (var key = Registry.Users.OpenSubKey($@"{user}\SOFTWARE\Classes\{applicationName}\shell", true))
                        {
                            try
                            {
                                key?.DeleteSubKeyTree("Proteggi Documento");
                            }
                            catch (Exception) { }
                        }
                    }
                }

                using (var key = Registry.ClassesRoot.OpenSubKey($@"{extention}\OpenWithProgids", false))
                {
                    var valueNames = key?.GetValueNames()?.Where(n => !string.IsNullOrEmpty(n))?.ToList();
                    valueNames?.ForEach(progID =>
                    {
                        if (!string.IsNullOrEmpty(progID))
                        {
                            using (var progIDkey = openSubKey($@"SOFTWARE\Classes\{progID}\shell", true))
                            {
                                try
                                {
                                    progIDkey?.DeleteSubKeyTree("Proteggi Documento");
                                }
                                catch (Exception) { }
                            }
                        }
                    });
                }

                foreach (var user in users)
                {
                    using (var key = Registry.Users.OpenSubKey($@"{user}\SOFTWARE\Classes\{extention}\OpenWithProgids", false))
                    {
                        var valueNames = key?.GetValueNames()?.Where(n => !string.IsNullOrEmpty(n))?.ToList();
                        valueNames?.ForEach(progID =>
                        {
                            if (!string.IsNullOrEmpty(progID))
                            {
                                using (var progIDkey = Registry.Users.OpenSubKey($@"{user}\SOFTWARE\Classes\{progID}\shell", true))
                                {
                                    try
                                    {
                                        progIDkey?.DeleteSubKeyTree("Proteggi Documento");
                                    }
                                    catch (Exception) { }
                                }
                                using (var progIDkey = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Classes\{progID}\shell", true))
                                {
                                    try
                                    {
                                        progIDkey?.DeleteSubKeyTree("Proteggi Documento");
                                    }
                                    catch (Exception) { }
                                }
                            }
                        });
                    }
                }
                using (var key = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Classes\{extention}\OpenWithProgids", false))
                {
                    var valueNames = key?.GetValueNames()?.Where(n => !string.IsNullOrEmpty(n))?.ToList();
                    valueNames?.ForEach(progID =>
                    {
                        if (!string.IsNullOrEmpty(progID))
                        {
                            using (var progIDkey = openSubKey($@"SOFTWARE\Classes\{progID}\shell", true))
                            {
                                try
                                {
                                    progIDkey?.DeleteSubKeyTree("Proteggi Documento");
                                }
                                catch (Exception) { }
                            }
                        }
                    });
                }
            }
        }

        public static void ProtectDocumentAddInRegister()
        {
            //using (var sec = TraceManager.GetCodeSection(T))
            //{
            var installForAllUsers = true;
            var componentFolder = Path.GetDirectoryName(T.Assembly.Location);
            var writeHive = installForAllUsers ? "HKEY_LOCAL_MACHINE" : "HKEY_CURRENT_USER";

            var applicationName = "Word";
            Registry.SetValue($@"{writeHive}\Software\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "Description", $"AIPCustomProtection.{applicationName}");
            Registry.SetValue($@"{writeHive}\Software\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "FriendlyName", $"AIPCustomProtection.{applicationName}");
            Registry.SetValue($@"{writeHive}\Software\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "LoadBehavior", 3);
            Registry.SetValue($@"{writeHive}\Software\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "Manifest", $@"{componentFolder}\AIPCustomProtection.{applicationName}.vsto|vstolocal");

            Registry.SetValue($@"{writeHive}\Software\Microsoft\VSTO\Security\Inclusion\b79f6617-8544-4b28-b767-74bb2dd58c38", "PublicKey", "<RSAKeyValue><Modulus>/AmqzCVaViiTqSZae5PY7SPDM8TKj1UV7Mqn96NQUAZzWh7MmqUMjY/ZaYoXENMrT1pDjpzt8GPs1Ns9YufGgAe+L+cLKASiFeGpk9wUgMy2MMChuU5Dziwc+YCwbUd4R2du6QyNduondBlcam0zsG657LSZo9lSP7bVC2Jr1/E=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>");
            Registry.SetValue($@"{writeHive}\Software\Microsoft\VSTO\Security\Inclusion\b79f6617-8544-4b28-b767-74bb2dd58c38", "Url", $@"{componentFolder}\AIPCustomProtection.{applicationName}.vsto");

            // WOW6432Node
            Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "Description", $"AIPCustomProtection.{applicationName}");
            Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "FriendlyName", $"AIPCustomProtection.{applicationName}");
            Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "LoadBehavior", 3);
            Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "Manifest", $@"{componentFolder}\AIPCustomProtection.{applicationName}.vsto|vstolocal");

            Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\VSTO\Security\Inclusion\b79f6617-8544-4b28-b767-74bb2dd58c39", "PublicKey", "<RSAKeyValue><Modulus>/AmqzCVaViiTqSZae5PY7SPDM8TKj1UV7Mqn96NQUAZzWh7MmqUMjY/ZaYoXENMrT1pDjpzt8GPs1Ns9YufGgAe+L+cLKASiFeGpk9wUgMy2MMChuU5Dziwc+YCwbUd4R2du6QyNduondBlcam0zsG657LSZo9lSP7bVC2Jr1/E=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>");
            Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\VSTO\Security\Inclusion\b79f6617-8544-4b28-b767-74bb2dd58c39", "Url", $@"{componentFolder}\AIPCustomProtection.{applicationName}.vsto");

            applicationName = "PowerPoint";
            Registry.SetValue($@"{writeHive}\Software\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "Description", $"AIPCustomProtection.{applicationName}");
            Registry.SetValue($@"{writeHive}\Software\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "FriendlyName", $"AIPCustomProtection.{applicationName}");
            Registry.SetValue($@"{writeHive}\Software\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "LoadBehavior", 3);
            Registry.SetValue($@"{writeHive}\Software\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "Manifest", $@"{componentFolder}\AIPCustomProtection.{applicationName}.vsto|vstolocal");

            Registry.SetValue($@"{writeHive}\Software\Microsoft\VSTO\Security\Inclusion\b79f6617-8544-4b28-b767-74bb2dd58c39", "PublicKey", "<RSAKeyValue><Modulus>/AmqzCVaViiTqSZae5PY7SPDM8TKj1UV7Mqn96NQUAZzWh7MmqUMjY/ZaYoXENMrT1pDjpzt8GPs1Ns9YufGgAe+L+cLKASiFeGpk9wUgMy2MMChuU5Dziwc+YCwbUd4R2du6QyNduondBlcam0zsG657LSZo9lSP7bVC2Jr1/E=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>");
            Registry.SetValue($@"{writeHive}\Software\Microsoft\VSTO\Security\Inclusion\b79f6617-8544-4b28-b767-74bb2dd58c39", "Url", $@"{componentFolder}\AIPCustomProtection.{applicationName}.vsto");

            // WOW6432Node
            Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "Description", $"AIPCustomProtection.{applicationName}");
            Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "FriendlyName", $"AIPCustomProtection.{applicationName}");
            Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "LoadBehavior", 3);
            Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "Manifest", $@"{componentFolder}\AIPCustomProtection.{applicationName}.vsto|vstolocal");

            Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\VSTO\Security\Inclusion\b79f6617-8544-4b28-b767-74bb2dd58c39", "PublicKey", "<RSAKeyValue><Modulus>/AmqzCVaViiTqSZae5PY7SPDM8TKj1UV7Mqn96NQUAZzWh7MmqUMjY/ZaYoXENMrT1pDjpzt8GPs1Ns9YufGgAe+L+cLKASiFeGpk9wUgMy2MMChuU5Dziwc+YCwbUd4R2du6QyNduondBlcam0zsG657LSZo9lSP7bVC2Jr1/E=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>");
            Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\VSTO\Security\Inclusion\b79f6617-8544-4b28-b767-74bb2dd58c39", "Url", $@"{componentFolder}\AIPCustomProtection.{applicationName}.vsto");


            applicationName = "Excel";
            Registry.SetValue($@"{writeHive}\Software\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "Description", $"AIPCustomProtection.{applicationName}");
            Registry.SetValue($@"{writeHive}\Software\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "FriendlyName", $"AIPCustomProtection.{applicationName}");
            Registry.SetValue($@"{writeHive}\Software\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "LoadBehavior", 3);
            Registry.SetValue($@"{writeHive}\Software\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "Manifest", $@"{componentFolder}\AIPCustomProtection.{applicationName}.vsto|vstolocal");

            Registry.SetValue($@"{writeHive}\Software\Microsoft\VSTO\Security\Inclusion\b79f6617-8544-4b28-b767-74bb2dd58c40", "PublicKey", "<RSAKeyValue><Modulus>/AmqzCVaViiTqSZae5PY7SPDM8TKj1UV7Mqn96NQUAZzWh7MmqUMjY/ZaYoXENMrT1pDjpzt8GPs1Ns9YufGgAe+L+cLKASiFeGpk9wUgMy2MMChuU5Dziwc+YCwbUd4R2du6QyNduondBlcam0zsG657LSZo9lSP7bVC2Jr1/E=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>");
            Registry.SetValue($@"{writeHive}\Software\Microsoft\VSTO\Security\Inclusion\b79f6617-8544-4b28-b767-74bb2dd58c40", "Url", $@"{componentFolder}\AIPCustomProtection.{applicationName}.vsto");

            // WOW6432Node
            Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "Description", $"AIPCustomProtection.{applicationName}");
            Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "FriendlyName", $"AIPCustomProtection.{applicationName}");
            Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "LoadBehavior", 3);
            Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "Manifest", $@"{componentFolder}\AIPCustomProtection.{applicationName}.vsto|vstolocal");

            Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\VSTO\Security\Inclusion\b79f6617-8544-4b28-b767-74bb2dd58c40", "PublicKey", "<RSAKeyValue><Modulus>/AmqzCVaViiTqSZae5PY7SPDM8TKj1UV7Mqn96NQUAZzWh7MmqUMjY/ZaYoXENMrT1pDjpzt8GPs1Ns9YufGgAe+L+cLKASiFeGpk9wUgMy2MMChuU5Dziwc+YCwbUd4R2du6QyNduondBlcam0zsG657LSZo9lSP7bVC2Jr1/E=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>");
            Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\VSTO\Security\Inclusion\b79f6617-8544-4b28-b767-74bb2dd58c40", "Url", $@"{componentFolder}\AIPCustomProtection.{applicationName}.vsto");


            //applicationName = "Outlook";
            //Registry.SetValue($@"{writeHive}\Software\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "Description", $"AIPCustomProtection.{applicationName}");
            //Registry.SetValue($@"{writeHive}\Software\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "FriendlyName", $"AIPCustomProtection.{applicationName}");
            //Registry.SetValue($@"{writeHive}\Software\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "LoadBehavior", 0);
            //Registry.SetValue($@"{writeHive}\Software\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "Manifest", $@"{componentFolder}\AIPCustomProtection.{applicationName}.vsto|vstolocal");

            //Registry.SetValue($@"{writeHive}\Software\Microsoft\VSTO\Security\Inclusion\b79f6617-8544-4b28-b767-74bb2dd58c40", "PublicKey", "<RSAKeyValue><Modulus>/AmqzCVaViiTqSZae5PY7SPDM8TKj1UV7Mqn96NQUAZzWh7MmqUMjY/ZaYoXENMrT1pDjpzt8GPs1Ns9YufGgAe+L+cLKASiFeGpk9wUgMy2MMChuU5Dziwc+YCwbUd4R2du6QyNduondBlcam0zsG657LSZo9lSP7bVC2Jr1/E=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>");
            //Registry.SetValue($@"{writeHive}\Software\Microsoft\VSTO\Security\Inclusion\b79f6617-8544-4b28-b767-74bb2dd58c40", "Url", $@"{componentFolder}\AIPCustomProtection.{applicationName}.vsto");

            //// WOW6432Node
            //Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "Description", $"AIPCustomProtection.{applicationName}");
            //Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "FriendlyName", $"AIPCustomProtection.{applicationName}");
            //Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "LoadBehavior", 0);
            //Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\Office\{applicationName}\Addins\AIPCustomProtection.{applicationName}", "Manifest", $@"{componentFolder}\AIPCustomProtection.{applicationName}.vsto|vstolocal");

            //Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\VSTO\Security\Inclusion\b79f6617-8544-4b28-b767-74bb2dd58c40", "PublicKey", "<RSAKeyValue><Modulus>/AmqzCVaViiTqSZae5PY7SPDM8TKj1UV7Mqn96NQUAZzWh7MmqUMjY/ZaYoXENMrT1pDjpzt8GPs1Ns9YufGgAe+L+cLKASiFeGpk9wUgMy2MMChuU5Dziwc+YCwbUd4R2du6QyNduondBlcam0zsG657LSZo9lSP7bVC2Jr1/E=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>");
            //Registry.SetValue($@"{writeHive}\Software\WOW6432Node\Microsoft\VSTO\Security\Inclusion\b79f6617-8544-4b28-b767-74bb2dd58c40", "Url", $@"{componentFolder}\AIPCustomProtection.{applicationName}.vsto");

        }
        public static void ProtectDocumentAddInRemove()
        {
            //using (var sec = TraceManager.GetCodeSection(T))
            //{
            var installForAllUsers = true;
            var writeHive = installForAllUsers ? "HKEY_LOCAL_MACHINE" : "HKEY_CURRENT_USER";
            Func<string, bool, RegistryKey> openSubKey = installForAllUsers ? (Func<string, bool, RegistryKey>)Registry.LocalMachine.OpenSubKey : Registry.CurrentUser.OpenSubKey;

            var applicationName = "Word";
            using (var key = openSubKey($@"Software\Microsoft\Office\{applicationName}\Addins", true))
            {
                try { key?.DeleteSubKeyTree($"AIPCustomProtection.{applicationName}"); } catch (Exception) { }
            }
            using (var key = openSubKey($@"Software\Microsoft\VSTO\Security\Inclusion", true))
            {
                try { key?.DeleteSubKeyTree($"b79f6617-8544-4b28-b767-74bb2dd58c38"); } catch (Exception) { }
            }


            applicationName = "PowerPoint";
            using (var key = openSubKey($@"Software\Microsoft\Office\{applicationName}\Addins", true))
            {
                try { key?.DeleteSubKeyTree($"AIPCustomProtection.{applicationName}"); } catch (Exception) { }
            }
            using (var key = openSubKey($@"Software\Microsoft\VSTO\Security\Inclusion", true))
            {
                try { key?.DeleteSubKeyTree($"b79f6617-8544-4b28-b767-74bb2dd58c39"); } catch (Exception) { }
            }

            applicationName = "Excel";
            using (var key = openSubKey($@"Software\Microsoft\Office\{applicationName}\Addins", true))
            {
                try { key?.DeleteSubKeyTree($"AIPCustomProtection.{applicationName}"); } catch (Exception) { }
            }
            using (var key = openSubKey($@"Software\Microsoft\VSTO\Security\Inclusion", true))
            {
                try { key?.DeleteSubKeyTree($"b79f6617-8544-4b28-b767-74bb2dd58c40"); } catch (Exception) { }
            }

            applicationName = "Outlook";
            using (var key = openSubKey($@"Software\Microsoft\Office\{applicationName}\Addins", true))
            {
                try { key?.DeleteSubKeyTree($"AIPCustomProtection.{applicationName}"); } catch (Exception) { }
            }
            using (var key = openSubKey($@"Software\Microsoft\VSTO\Security\Inclusion", true))
            {
                try { key?.DeleteSubKeyTree($"b79f6617-8544-4b28-b767-74bb2dd58c40"); } catch (Exception) { }
            }
        }
        public static void DisableULClient()
        {
            var applications = new[] { "Word", "PowerPoint", "Excel", "Outlook" };

            var installForAllUsers = true;
            var writeHive = installForAllUsers ? "HKEY_LOCAL_MACHINE" : "HKEY_CURRENT_USER";

            var users = Registry.Users.GetSubKeyNames();

            var rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            
            using (var regKey64 = rk.OpenSubKey($@"SOFTWARE\Classes\AllFilesystemObjects\shell", true))
            {
                try { regKey64?.DeleteSubKeyTree($"Microsoft.Azip.RightClick"); } catch (Exception) { }
            }

            foreach (var applicationName in applications)
            {
                if (!applicationName.Equals("Outlook", StringComparison.InvariantCultureIgnoreCase))
                {
                    var regKey64 = rk.OpenSubKey($@"Software\Microsoft\Office\{applicationName}\Addins\MSIP.{applicationName}Addin", true);

                    if (regKey64 != null)
                    {
                        regKey64.SetValue("LoadBehavior", 0);
                        Registry.SetValue($@"{writeHive}\Software\Microsoft\Office\{applicationName}\Addins\MSIP.{applicationName}Addin", "LoadBehavior", 0);
                    }
                }
                else
                {
                    var regKey64 = rk.OpenSubKey($@"Software\Microsoft\Office\{applicationName}\Addins\MSIP.{applicationName}Addin", true);
                    
                    if (regKey64 != null)
                    {
                        regKey64.SetValue("LoadBehavior", 3);
                        Registry.SetValue($@"{writeHive}\Software\Microsoft\Office\{applicationName}\Addins\MSIP.{applicationName}Addin", "LoadBehavior", 3);

                    }
                    foreach (var user in users)
                    {
                        try
                        {
                            var key = Registry.Users.OpenSubKey($@"{user}\Software\Microsoft\MSIP", true);
                            if (key != null)
                            {
                                key.SetValue("OutlookHideBar", 0);
                            }
                        }
                        catch { }
                    }
                }
            }
        }
    }
}
