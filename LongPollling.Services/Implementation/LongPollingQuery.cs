using LongPolling.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Reactive;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace LongPollling.Services.Implementation
{
    public class LongPollingQuery<T>
    {

        public class LongPollingValue<T>
        {
            public T Value { get; set; }
            public DateTime Marker { get; set; }

            public LongPollingValue<T> Next { get; set; }
        }
        TimeSpan TimeOut = TimeSpan.FromSeconds(40);
        TimeSpan WatchDogTimeOut = TimeSpan.FromMinutes(5);

        private LongPollingValue<T> _first;
        private LongPollingValue<T> _last;

        private event Action Added;

        private readonly object _lock = new object();

        public LongPollingQuery()
        {
            var clearWatchDog = Observable.FromEvent(h => Added += h, h => Added -= h);
            clearWatchDog
                .Sample(TimeSpan.FromMinutes(1))
                .Subscribe(_ =>
                {
                    var dt = DateTime.Now - WatchDogTimeOut;
                    while (_first != null && _first.Marker < dt) _first = _first.Next;
                });
        }

        public async Task Add(T value)
        {
            lock (_lock)
            {
                var added = new LongPollingValue<T> { Value = value, Marker = DateTime.Now };
                if(_first==null)
                {
                    _first = _last = added;
                }
                else
                {
                    _last = _last.Next = added;
                }
            }
            Added();
        }

        public async Task<List<T>> ReadAsync(DateTime marker, Func<T, bool> filter = null)
        {
            var result = new List<T>();
            var dt = DateTime.Now;

            do
            {
                var item = _first;
                while (item != null)
                {
                    if(item.Marker > marker && (filter?.Invoke(item.Value) ?? true))
                    {
                        result.Add(item.Value);
                        
                    }
                    item = item.Next;
                }
            } while (result.Count==0 && (DateTime.Now-dt)<TimeOut);
            return result;
        }
    }
}
