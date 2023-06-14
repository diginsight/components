#region using
using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
#endregion

namespace Common
{
    #region GlobalConstants
    /// <summary>Defines global constants for the application.
    /// It is recommended to AVOID using global state variables to pass parameters between commands and modules.</summary>
    public class GlobalConstants
    {
        public const string KEYNAME_LOG_LISTENERKEY = "LOG_LISTENERKEY";
        public const string KEYDEFAULTVALUE_LOG_LISTENERKEY = @"";
        public const string KEYNAME_LOG_FILEPATH = "LOG_FILEPATH";
        public const string KEYDEFAULTVALUE_LOG_FILEPATH = @"\Logs";
        public const string KEYNAME_LOG_FILENAME = "LOG_FILENAME";
        public const string KEYDEFAULTVALUE_LOG_FILENAME = @"";

        public const string CONFIGVALUE_ENVIRONMENT = "Environment";
        public const string DEFAULTVALUE_ENVIRONMENT = "dev";
        public const string CONFIGVALUE_EXECUTIONMODE = "ExecutionMode";
        public const ExecutionMode DEFAULTVALUE_EXECUTIONMODE = ExecutionMode.Normal;
        public const string CONFIGVALUE_USEIDLEPROCESSING = "UseIdleProcessing";
        public const bool DEFAULTVALUE_USEIDLEPROCESSING = true;
        public const string CONFIGVALUE_IDLEPROCESSINGINTERVAL = "IdleProcessingInterval";
        public const int DEFAULTVALUE_IDLEPROCESSINGINTERVAL = 100;
        public const string CONFIGVALUE_JOINIDLEPROCESSINGTIMEOUT = "JoinIdleProcessingTimeout";
        public const int DEFAULTVALUE_JOINIDLEPROCESSINGTIMEOUT = 200;
        public const string CONFIGVALUE_RESIZEMODE = "ResizeMode";
        public const ResizeMode DEFAULTVALUE_RESIZEMODE = ResizeMode.NoResize;

        public const int ONE = 1;
        public const double ZOOMDELTA = 0.05;
        public const double ZOOMDEFAULT = 1;
        public const double ZOOMMAX = 1.5;
        public const double ZOOMMIN = 0.5;

        public const string ENVIRONMENTUSER_FORMAT = "E_{0}-U_{1}-N_{2}"; // 
        //public const string ENVIRONMENTUSEROLD_FORMAT = "ENV_'{0}'-USER_'{1}'";

        #region .ctor
        static GlobalConstants()
        {
            if (ApplicationBase.Current == null) { return; }

            //ApplicationBase.Current.Properties[ABCGlobali.GLOBAL_FORMAVVIO] = null;
            //ApplicationBase.Current.Properties[ABCGlobali.GLOBAL_POSTAZIONE] = null;
            //ApplicationBase.Current.Properties[ABCGlobali.GLOBAL_POSTAZIONE_UTENTE] = null;
            //ApplicationBase.Current.Properties[ABCGlobali.GLOBAL_CONTROLLOROOT] = null;

            //ApplicationBase.Current.Properties[ABCGlobali.GLOBAL_TERMINALE] = null;
            //ApplicationBase.Current.Properties[ABCGlobali.GLOBAL_DEVICES] = null;

            //ApplicationBase.Current.Properties[ABCGlobali.SHAREDDICTIONARY_RESOURCES] = null;
        }
        #endregion
        #region informazioni globali della shell
        /// <summary>la variabile di stato GLOBAL_FORMAVVIO contiene la form di avvio della applicazione</summary>
        public static string GLOBAL_FORMAVVIO = "Avvio";
        public static string GLOBAL_CONTROLLOROOT = "ABCControlloRoot";
        public static string GLOBAL_ABCSHELL = "ABCShell";
        public static string GLOBAL_MENUPROVIDER = "MenuProvider";
        #endregion
    }
    #endregion

    public static class BrushConstants
    {
        public static SolidColorBrush Blue1 = new SolidColorBrush(Color.FromArgb(0xFF, 0xDD, 0xED, 0xFD)); //<SolidColorBrush x:Key="Blue1">#DDEDFD</SolidColorBrush >
        public static SolidColorBrush Blue2 = new SolidColorBrush(Color.FromArgb(0xFF, 0xAB, 0xD4, 0xFD)); //<SolidColorBrush x:Key="Blue2">#ABD4FD</SolidColorBrush >
        public static SolidColorBrush Blue3 = new SolidColorBrush(Color.FromArgb(0xFF, 0x73, 0xB7, 0xFF)); //<SolidColorBrush x:Key="Blue3">#73B7FF</SolidColorBrush >
        public static SolidColorBrush Blue4 = new SolidColorBrush(Color.FromArgb(0xFF, 0x2E, 0x92, 0xFA)); //<SolidColorBrush x:Key="Blue4">#2E92FA</SolidColorBrush >
        public static SolidColorBrush Blue5 = new SolidColorBrush(Color.FromArgb(0xFF, 0x0C, 0x74, 0xDA)); //<SolidColorBrush x:Key="Blue5">#0C74DA</SolidColorBrush >
        public static SolidColorBrush Blue6 = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x4C, 0x97)); //<SolidColorBrush x:Key="Blue6">#004C97</SolidColorBrush >
        public static SolidColorBrush Blue7 = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x96, 0xEA)); //<SolidColorBrush x:Key="Blue7">#FF0096EA</SolidColorBrush >
    }

    public static class ExceptionCodes
    {
        public const long PRESSLOGIN = 1;
        public const long INCOMPATIBLEADDIN = 2;
    }
}
