using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Direct3DHookLib.Hook
{
    /// <summary>
    /// Used to determine the FPS
    /// </summary>
    public class FramesPerSecond
    {
        //https://msdn.microsoft.com/ko-kr/library/windows/desktop/dn553408(v=vs.85).aspx
        UInt32 _frames = 0;
        Int64 _lastTimestamp = 0;
        Int64 _lastSecTimestamp = 0;
        Int64 _currTimestamp = 0;
        Int64 _QPCFrequency = 0;
        double _lastFrameRate = 0;
        
        public FramesPerSecond()
        {
#if DEBUG
            System.Console.Write("[INFO] Using High Precision Timer : ");
            if (Stopwatch.IsHighResolution)
                System.Console.WriteLine("Yes");
            else
                System.Console.WriteLine("No");
#endif
            _QPCFrequency = Stopwatch.Frequency;
            _currTimestamp = Stopwatch.GetTimestamp();
            _lastTimestamp = _currTimestamp;
            _lastSecTimestamp = _currTimestamp;

            _frames = 0;
        }

        /// <summary>
        /// Must be called each frame
        /// </summary>
        public void Frame(bool differential = true)
        {
            _currTimestamp = Stopwatch.GetTimestamp();
            if (differential)
            {
                _lastFrameRate = 1.0 / ((_currTimestamp - _lastTimestamp) * (1.0 / _QPCFrequency));
                
            }
            else
            {
                double elapsed = Math.Abs((_currTimestamp - _lastSecTimestamp) * (1.0 / _QPCFrequency));
                _frames++;
                if (elapsed > 1.0)
                {
                    _lastFrameRate = (double)(_frames / elapsed);
                    _lastSecTimestamp = _currTimestamp;
                    _frames = 0;
                }
            }
            _lastTimestamp = _currTimestamp;
        }

        /// <summary>
        /// Return the current frames per second
        /// </summary>
        /// <returns></returns>
        public double GetFPS()
        {
            return _lastFrameRate;
        }
    }
}
