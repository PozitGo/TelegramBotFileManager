using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotFilesManager
{
    public class RequestRateLimiter
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly TimeSpan _timeInterval = TimeSpan.FromSeconds(5);
        private readonly int _maxRequestsPerInterval = 5;

        private int _requestCount;

        public bool CanMakeRequest()
        {
            if (!_stopwatch.IsRunning)
            {
                _stopwatch.Start();
                _requestCount = 1;
                return true;
            }

            if (_stopwatch.Elapsed >= _timeInterval)
            {
                _stopwatch.Restart();
                _requestCount = 1;
                return true;
            }

            if (_requestCount < _maxRequestsPerInterval)
            {
                _requestCount++;
                return true;
            }

            return false;
        }

        public void UpdateUserRequestTime()
        {
            if (!_stopwatch.IsRunning)
            {
                _stopwatch.Start();
                _requestCount = 1;
            }
        }
    }

}
