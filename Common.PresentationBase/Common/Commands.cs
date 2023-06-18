#region using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
#endregion

namespace Common
{
    public class Commands
    {
        // Window commands
        public static readonly RoutedUICommand Exit = new RoutedUICommand("Exit", "Exit", typeof(Commands));
        public static readonly RoutedUICommand Close = new RoutedUICommand("Close", "Close", typeof(Commands));
        public static readonly RoutedUICommand Cancel = new RoutedUICommand("Cancel", "Cancel", typeof(Commands));
        public static readonly RoutedUICommand Maximize = new RoutedUICommand("Maximize", "Maximize", typeof(Commands));
        public static readonly RoutedUICommand Restore = new RoutedUICommand("Restore", "Restore", typeof(Commands));
        public static readonly RoutedUICommand Minimize = new RoutedUICommand("Minimize", "Minimize", typeof(Commands));
        public static readonly RoutedUICommand ToggleWindowState = new RoutedUICommand("ToggleWindowState", "ToggleWindowState", typeof(Commands));
        public static readonly RoutedUICommand Hide = new RoutedUICommand("Hide", "Hide", typeof(Commands));
        public static readonly RoutedUICommand Settings = new RoutedUICommand("Settings", "Settings", typeof(Commands));
        public static readonly RoutedUICommand HideSettings = new RoutedUICommand("HideSettings", "HideSettings", typeof(Commands));
        public static readonly RoutedUICommand SetWindowState = new RoutedUICommand("SetWindowState", "SetWindowState", typeof(Commands));
        // Menu commands 
        public static readonly RoutedUICommand ToggleMenu = new RoutedUICommand("ToggleMenu", "ToggleMenu", typeof(Commands));
        // Login commands
        public static readonly RoutedUICommand Login = new RoutedUICommand("Login", "Login", typeof(Commands));
        public static readonly RoutedUICommand Logout = new RoutedUICommand("Logout", "Logout", typeof(Commands));
        public static readonly RoutedUICommand LoginToggle = new RoutedUICommand("LoginToggle", "LoginToggle", typeof(Commands));
        // Menu control commands
        public static readonly RoutedUICommand ToggleIsCollapsed = new RoutedUICommand("ToggleIsCollapsed", "ToggleIsCollapsed", typeof(Commands));
        public static readonly RoutedUICommand Apply = new RoutedUICommand("Apply", "Apply", typeof(Commands));
        public static readonly RoutedUICommand Refresh = new RoutedUICommand("Refresh", "Refresh", typeof(Commands)); 
        public static readonly RoutedUICommand Idea = new RoutedUICommand("Idea", "Idea", typeof(Commands));
        // Settings
        public static readonly RoutedUICommand Clear = new RoutedUICommand("Clear", "Clear", typeof(Commands));
        public static readonly RoutedUICommand Reset = new RoutedUICommand("Reset", "Reset", typeof(Commands));
        public static readonly RoutedUICommand AddItem = new RoutedUICommand("AddItem", "AddItem", typeof(Commands));
        public static readonly RoutedUICommand RemoveItem = new RoutedUICommand("RemoveItem", "RemoveItem", typeof(Commands));
        public static readonly RoutedUICommand RegisterPanel = new RoutedUICommand("RegisterPanel", "RegisterPanel", typeof(Commands));
        public static readonly RoutedUICommand AddSettingsPanel = new RoutedUICommand("AddSettingsPanel", "AddSettingsPanel", typeof(Commands));
        
        public static readonly RoutedUICommand Themes = new RoutedUICommand("Themes", "Themes", typeof(Commands));
        public static readonly RoutedUICommand Developers = new RoutedUICommand("Developers", "Developers", typeof(Commands));
        public static readonly RoutedUICommand Download = new RoutedUICommand("Download", "Download", typeof(Commands));
        public static readonly RoutedUICommand Languages = new RoutedUICommand("Languages", "Languages", typeof(Commands));
        public static readonly RoutedUICommand ChangeCulture = new RoutedUICommand("ChangeCulture", "ChangeCulture", typeof(Commands));
        public static readonly RoutedUICommand LicenseKey = new RoutedUICommand("LicenseKey", "License Key", typeof(Commands));
        public static readonly RoutedUICommand About = new RoutedUICommand("About", "About", typeof(Commands));
        public static readonly RoutedUICommand ToggleDeveloperTools = new RoutedUICommand("ToggleDeveloperTools", "ToggleDeveloperTools", typeof(Commands));
        public static readonly RoutedUICommand ToggleModifyReadonly = new RoutedUICommand("ToggleModifyReadonly", "ToggleModifyReadonly", typeof(Commands));
        public static readonly RoutedUICommand Watch = new RoutedUICommand("Watch", "Watch", typeof(Commands));
        public static readonly RoutedUICommand Cut = new RoutedUICommand("Cut", "Cut", typeof(Commands));
        public static readonly RoutedUICommand Copy = new RoutedUICommand("Copy", "Copy", typeof(Commands));
        public static readonly RoutedUICommand Paste = new RoutedUICommand("Paste", "Paste", typeof(Commands));

        public static readonly RoutedUICommand OK = new RoutedUICommand("OK", "OK", typeof(Commands));
        // Developer window
        public static readonly RoutedUICommand Play = new RoutedUICommand("Play", "Play", typeof(Commands));
        public static readonly RoutedUICommand Pause = new RoutedUICommand("Pause", "Pause", typeof(Commands));
        public static readonly RoutedUICommand Stop = new RoutedUICommand("Stop", "Stop", typeof(Commands));
        public static readonly RoutedUICommand ClearWindow = new RoutedUICommand("ClearWindow", "ClearWindow", typeof(Commands));
        public static readonly RoutedUICommand Execute = new RoutedUICommand("Execute", "Execute", typeof(Commands));
        public static readonly RoutedUICommand ExecuteCommand = new RoutedUICommand("ExecuteCommand", "ExecuteCommand", typeof(Commands));
        public static readonly RoutedUICommand AddWatch = new RoutedUICommand("AddWatch", "AddWatch", typeof(Commands));
        // Classic View
        public static readonly RoutedUICommand NotifyVariableChange = new RoutedUICommand("NotifyVariableChange", "NotifyVariableChange", typeof(Commands));
        public static readonly RoutedUICommand NotifyChange = new RoutedUICommand("NotifyChange", "NotifyChange", typeof(Commands));
        public static readonly RoutedUICommand SaveSnapshot = new RoutedUICommand("SaveSnapshot", "SaveSnapshot", typeof(Commands));
        // Simulator 
        //public static readonly RoutedUICommand StartSimulator = new RoutedUICommand("StartSimulator", "StartSimulator", typeof(Commands));
        //public static readonly RoutedUICommand PauseSimulator = new RoutedUICommand("PauseSimulator", "PauseSimulator", typeof(Commands));
        //public static readonly RoutedUICommand StopSimulator = new RoutedUICommand("StopSimulator", "StopSimulator", typeof(Commands));
        //public static readonly RoutedUICommand ToggleSimulator = new RoutedUICommand("ToggleSimulator", "ToggleSimulator", typeof(Commands));
        public static readonly RoutedUICommand Clone = new RoutedUICommand("Clone", "Clone", typeof(Commands));
        public static readonly RoutedUICommand SetLabel = new RoutedUICommand("SetLabel", "SetLabel", typeof(Commands));
        public static readonly RoutedUICommand SelectLabel = new RoutedUICommand("SelectLabel", "SelectLabel", typeof(Commands));
        public static readonly RoutedUICommand ApplyLabel = new RoutedUICommand("ApplyLabel", "ApplyLabel", typeof(Commands));
        public static readonly RoutedUICommand GetDescriptor = new RoutedUICommand("GetDescriptor", "GetDescriptor", typeof(Commands));

        public static readonly RoutedUICommand ApplyProtection = new RoutedUICommand("ApplyProtection", "ApplyProtection", typeof(Commands));
        public static readonly RoutedUICommand ApplyProtectionConfirmed = new RoutedUICommand("ApplyProtectionConfirmed", "ApplyProtectionConfirmed", typeof(Commands));
        public static readonly RoutedUICommand ApplyProtectionConfirmedAddIn = new RoutedUICommand("ApplyProtectionConfirmedAddIn", "ApplyProtectionConfirmedAddIn", typeof(Commands));
        public static readonly RoutedUICommand RemoveProtection = new RoutedUICommand("RemoveProtection", "RemoveProtection", typeof(Commands));
        public static readonly RoutedUICommand RemoveProtectionConfirmed = new RoutedUICommand("RemoveProtectionConfirmed", "RemoveProtectionConfirmed", typeof(Commands));

        public static readonly RoutedUICommand ClearMessage = new RoutedUICommand("ClearMessage", "ClearMessage", typeof(Commands));
        public static readonly RoutedUICommand SelectInternalContacts = new RoutedUICommand("SelectInternalContacts", "SelectInternalContacts", typeof(Commands));
        public static readonly RoutedUICommand SelectExternalContacts = new RoutedUICommand("SelectExternalContacts", "SelectExternalContacts", typeof(Commands));
        public static readonly RoutedUICommand SelectOwnerContacts = new RoutedUICommand("SelectOwnerContacts", "SelectOwnerContacts", typeof(Commands));
        //
    }
}
