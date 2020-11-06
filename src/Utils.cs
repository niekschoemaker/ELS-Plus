using System;
using System.Runtime.InteropServices;

namespace ELS
{
    class Utils
    {
        public static bool IsDeveloper = false;
        public static bool Debug = false;
        /// <summary>
        /// Print out a message only if the program is compiled for Debug.
        /// </summary>
        /// <param name="data">Data to print in console</param>
        static internal void DebugWrite(string data)
        {
            if (IsDeveloper)
            {
                CitizenFX.Core.Debug.Write($"ELS-Plus: {data}");
            }
        }

        /// <summary>
        /// Print out a message only if the program is compiled for Debug.
        /// </summary>
        /// /// <param name="data">Data to print in console</param>
        /// /// <param name="args">Arugments to be formated into data</param>
        static internal void DebugWriteLine(string data, [Optional]object[] args)
        {
            if (IsDeveloper)
            {
                if (args != null)
                {
                    CitizenFX.Core.Debug.WriteLine($"ELS-Plus: {data}", args);
                }
                else
                {
                    CitizenFX.Core.Debug.WriteLine($"ELS-Plus: {data}");
                }
            }
        }
        /// <summary>
        /// Print out a message only if the program is compiled for all release types.
        /// </summary>
        /// /// <param name="data">Data to print in console</param>
        static internal void ReleaseWrite(string data)
        {
            CitizenFX.Core.Debug.Write($"ELS-Plus: {data}");
        }

        /// <summary>
        /// Print out a message only if the program is compiled for all release types.
        /// </summary>
        /// <param name="data">Data to print in console</param>
        /// <param name="args">Arugments to be formated into data</param>
        static internal void ReleaseWriteLine(string data, [Optional]object[] args)
        {
            if (args != null)
            {
                CitizenFX.Core.Debug.WriteLine($"ELS-Plus: {data}", args);
            }
            else
            {
                CitizenFX.Core.Debug.WriteLine($"ELS-Plus: {data}");
            }
        }

        static internal void DeveloperWriteLine(string data, [Optional]object[] args)
        {
            if (!IsDeveloper)
            {
                return;
            }
            if (args != null)
            {
                CitizenFX.Core.Debug.WriteLine($"ELS-Plus: {data}", args);
            }
            else
            {
                CitizenFX.Core.Debug.WriteLine($"ELS-Plus: {data}");
            }
        }

        /// <summary>
        /// Print out a message only if the program is compiled for all release types.
        /// </summary>
        ///<param name = "ex">Message for exception</param>
        static internal void ThrowException(Exception ex)
        {
            ReleaseWriteLine($"Exception thrown:\n" +
                $"{ex.Message}");
            throw (ex);
        }

    }

    class Timer
    {
        int _limit = 0;

        public int Limit { set { _limit = ELS.GameTime + value; } }
        public bool Expired
        {
            get
            {

                if (ELS.GameTime > _limit)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public Timer()
        {

        }

        public void Reset()
        {
            _limit = 0;
        }
    }
}
