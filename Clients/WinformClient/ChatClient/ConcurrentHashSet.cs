using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WinformClient {
    public class ConcurrentHashSet<T> : IDisposable {
        private HashSet<T> Channels = new HashSet<T>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_lock != null) {
                    _lock.Dispose();
                }
            }
        }
        ~ConcurrentHashSet() {
            Dispose(false);
        }

        public bool Add(T channel) {
            _lock.EnterWriteLock();
            try {
                return Channels.Add(channel);
            } finally {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }
        public void Clear() {
            _lock.EnterWriteLock();
            try {
                Channels.Clear();
            } finally {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }
        public bool Contains(T channel) {
            _lock.EnterReadLock();
            try {
                return Channels.Contains(channel);
            } finally {
                if (_lock.IsReadLockHeld) _lock.ExitReadLock();
            }
        }
        public bool Delete(T channel) {
            _lock.EnterWriteLock();
            try {
                return this.Channels.Remove(channel);
            } finally {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }
        public int Count {
            get {
                _lock.EnterReadLock();
                try {
                    return Channels.Count;
                } finally {
                    if (_lock.IsReadLockHeld) _lock.ExitReadLock();
                }
            }
        }
        public IEnumerable<T> GetAll() {
            _lock.EnterReadLock();
            try {
                return this.Channels;
            } finally {
                if (_lock.IsReadLockHeld) {
                    _lock.ExitReadLock();
                }
            }
           
        }

    }
}
