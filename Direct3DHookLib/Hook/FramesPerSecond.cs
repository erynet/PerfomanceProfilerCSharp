using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Direct3DHookLib.Hook.Common;

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

        SMMHeap _fpsHeap;
        
        public FramesPerSecond()
        {
            _QPCFrequency = Stopwatch.Frequency;
            _currTimestamp = Stopwatch.GetTimestamp();
            _lastTimestamp = _currTimestamp;
            _lastSecTimestamp = _currTimestamp;

            //_frames = 0;

            _fpsHeap = new SMMHeap(2048);
        }

        /// <summary>
        /// Must be called each frame
        /// </summary>
        public void Frame(bool differential = false)
        {
            _currTimestamp = Stopwatch.GetTimestamp();
            if (differential)
            {
                _lastFrameRate = 1.0 / ((_currTimestamp - _lastTimestamp) * (1.0 / _QPCFrequency));
                
            }
            else
            {
                double elapsed = Math.Abs((_currTimestamp - _lastSecTimestamp) * (1.0 / _QPCFrequency));
                //_frames++;
                //if (elapsed > 1.0)
                //{
                //    _lastFrameRate = (double)(_frames / elapsed);
                //    _lastSecTimestamp = _currTimestamp;
                //    _frames = 0;
                //}
                //InsertTimestamp(_currTimestamp);
                InsertTimestamp((Int64)((_currTimestamp / (Stopwatch.Frequency * 1.0)) * 10000000));
                if (elapsed > 0.1)
                {
                    _lastFrameRate = _fpsHeap.Size;
                    _lastSecTimestamp = _currTimestamp;
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

        public void InsertTimestamp(Int64 Timestamp)
        {
            _fpsHeap.Insert(Timestamp);
            while(true)
            {
                if ((_fpsHeap.Max - _fpsHeap.Min)  >= 9800000)
                    _fpsHeap.DeleteMin();
                else
                    break;
            }
        }
    }
}
