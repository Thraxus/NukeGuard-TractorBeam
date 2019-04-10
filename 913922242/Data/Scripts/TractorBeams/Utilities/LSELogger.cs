/*
Code shameless stolen from Phoenix and Shaostoal Laserdrill Mod (with permission)
*/

using System;
using System.Text;

namespace NukeGuard_TractorBeam.TractorBeams.Utilities
{
    // This is a singleton class for logging
    // This should be generic, so it can be included without modification in other mods.
    public class LseLogger
    {
        System.IO.TextWriter _mLogger = null;
        static private LseLogger _mInstance = null;
        readonly StringBuilder _mCache = new StringBuilder(60);
        string _mFilename = "tractorbeam";           
        int _mIndent = 0;
        bool _mInit = false;

        private LseLogger()
        {
            Active = false;
            Enabled = false;
        }

        static public LseLogger Instance
        {
            get
            {
                if (_mInstance == null)
                {
                    _mInstance = new LseLogger();
                    _mInstance.Init();
                }

                return _mInstance;
            }
        }

        /// <summary>
        /// Toggles whether to log messages (exceptions are always logged).
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Toggles whether to log exceptions even if not Active. This is useful during startup.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Enable debug logging (admin only)
        /// </summary>
        public bool Debug { get; set; }

        public void Init(string filename = null)
        {
            _mInit = true;
            
            if (!string.IsNullOrEmpty(filename))
                Filename = filename;

            LogMessage("Starting new session");
        }

        public int IndentLevel
        {
            get { return _mIndent; }
            set
            {
                if (value < 0)
                    value = 0;
                _mIndent = value;
            }
        }

        public string Filename
        {
            get { return _mFilename; }
            set { _mFilename = value; }
        }

        #region Mulithreaded logging
        public void LogDebugOnGameThread(string message)
        {
            Sandbox.ModAPI.MyAPIGateway.Utilities.InvokeOnGameThread(delegate { Instance.LogDebug(message); });
        }

        public void LogMessageOnGameThread(string message)
        {
            Sandbox.ModAPI.MyAPIGateway.Utilities.InvokeOnGameThread(delegate { Instance.LogMessage(message); });
        }
        public void LogExceptionOnGameThread(Exception ex)
        {
            Sandbox.ModAPI.MyAPIGateway.Utilities.InvokeOnGameThread(delegate { Instance.LogException(ex); });
        }
        #endregion

        public void LogDebug(string message)
        {
            if (!Active || !Debug)
                return;

            if (Active && Debug && (Sandbox.ModAPI.MyAPIGateway.Multiplayer != null && Sandbox.ModAPI.MyAPIGateway.Multiplayer.IsServer) ||
                (Sandbox.ModAPI.MyAPIGateway.Session != null && Sandbox.ModAPI.MyAPIGateway.Session.Player != null && Sandbox.ModAPI.MyAPIGateway.Session.Player.IsAdmin()))
                LogMessage(message);
        }

        public void LogAssert(bool trueExpression, string message)
        {
            if (!Active)
                return;

            if (!trueExpression)
            {
                StringBuilder assertmsg = new StringBuilder(message.Length + 30);
                assertmsg.Append("ASSERT FAILURE: ");
                assertmsg.Append(message);
                LogMessage(message);
            }
        }

        public delegate void LoggerCallback(string param1, int time = 2000);

        public void LogException(Exception ex)
        {
            if (!Enabled)
                return;

            if (Sandbox.ModAPI.MyAPIGateway.Utilities != null && Sandbox.ModAPI.MyAPIGateway.Session != null)
            {
                if (Sandbox.ModAPI.MyAPIGateway.Session.Player == null)
                    Sandbox.ModAPI.MyAPIGateway.Utilities.SendMessage(Filename + ": AN ERROR OCCURED! Please report to server admin! Admin: look for " + Filename + ".log.");
                else
                    Sandbox.ModAPI.MyAPIGateway.Utilities.ShowMessage(Filename, "AN ERROR OCCURED! Please report and send " + Filename + ".log in the mod storage directory!");
            }

            // Make sure we ALWAYS log an exception
            bool previousActive = Active;
            Active = true;
            Instance.LogMessage(string.Format("Exception: {0}\r\n{1}", ex.Message, ex.StackTrace));

            if (ex.InnerException != null)
            {
                Instance.LogMessage("Inner Exception Information:");
                Instance.LogException(ex.InnerException);
            }
            Active = previousActive;
        }

        public void LogMessage(string message, LoggerCallback callback = null, int time = 2000)
        {
            if (!Active)
                return;

            _mCache.Append(DateTime.Now.ToString("[HH:mm:ss.fff] "));

            for (int x = 0; x < IndentLevel; x++)
                _mCache.Append("  ");

            _mCache.AppendFormat("{0}{1}", message, (_mLogger != null ? _mLogger.NewLine : "\r\n"));

            if (callback != null)
                callback(message, time);          // Callback to pass message to another logger (ie. ShowNotification)

            if (_mInit)
            {
                if (_mLogger == null)
                {
                    try
                    {
                        _mLogger = Sandbox.ModAPI.MyAPIGateway.Utilities.WriteFileInLocalStorage(Filename + ".log", typeof(LseLogger));
                    }
                    catch { return; }
                }

                Sandbox.ModAPI.MyAPIGateway.Utilities.ShowNotification("writing: " + message);
                _mLogger.Write(_mCache);
                _mLogger.Flush();
                _mCache.Clear();
            }
        }

        public void Close()
        {
            if (!_mInit)
                return;

            if (_mLogger == null)
                return;

            if (_mCache.Length > 0)
                _mLogger.Write(_mCache);

            _mCache.Clear();
            IndentLevel = 0;
            LogMessage("Ending session");
            _mLogger.Flush();
            _mLogger.Close();
            _mLogger = null;
            _mInit = false;
        }
    }
}
