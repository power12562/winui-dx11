using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace WsiuEngine.Core.System
{
    public class Time
    {
        public Time()
        {
            _stopWatch.Start();
            _lastTicks = _stopWatch.ElapsedTicks;
            _totalTime = _stopWatch.Elapsed;
        }

        /// <summary>
        /// 타이머의 업데이트가 일어난 횟수입니다.
        /// </summary>
        public ulong FrameCount => _frameCount;

        public double TimeScaleAsDouble
        {
            get => _timeScale;
            set => _timeScale = Math.Clamp(value, 0, 10.0);
        }
        /// <summary>
        /// 시간이 흐르는 배율 입니다.
        /// </summary>
        public float TimeScale

        {
            get => (float)TimeScaleAsDouble;
            set =>  TimeScaleAsDouble = (double)value; 
        }

        public double DeltaTimeAsDouble { get; private set; }

        /// <summary>
        /// 델타 타임입니다.
        /// </summary>
        public float DeltaTime => (float)DeltaTimeAsDouble;
        public double UnscaleDeltaTimeAsDouble { get; private set; }

        /// <summary>
        /// 타임 스케일이 적용되지 않는 델타타임입니다.
        /// </summary>
        public float UnscaleDeltaTime => (float)UnscaleDeltaTimeAsDouble;

        /// <summary>
        /// 현재까지 진행된 총 시간입니다.
        /// </summary>
        public TimeSpan UnscaledTotalTime => _stopWatch.Elapsed;

        /// <summary>
        /// 현재까지 진행된 총 델타타임 시간입니다.
        /// </summary>
        public TimeSpan TotalTime => _totalTime;

        internal void UpdateTime()
        {
            ++_frameCount;
            _currentTicks = _stopWatch.ElapsedTicks;
            UpdateTotalTime();
            UpdateDT();
            _lastTicks = _currentTicks;
        }

        private readonly Stopwatch _stopWatch = new();
        long _currentTicks = 0;
        private long _lastTicks = 0;
        private double _timeScale = 1.0;
        private TimeSpan _totalTime;
        private ulong _frameCount = 0;

        private void UpdateDT()
        {   
            UnscaleDeltaTimeAsDouble = (double)(_currentTicks - _lastTicks) / Stopwatch.Frequency;
            DeltaTimeAsDouble = UnscaleDeltaTimeAsDouble * TimeScaleAsDouble;
        }

        private void UpdateTotalTime()
        {
            TimeSpan elapsedTime = TimeSpan.FromSeconds(UnscaleDeltaTimeAsDouble);
            _totalTime += elapsedTime * TimeScaleAsDouble;
        }
    }
}
