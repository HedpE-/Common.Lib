using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Common.Lib.HelperClasses
{
    public class ObservableObject : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        public ObservableObject() { }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event if needed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            ValidateAsync();
        }

        #endregion

        protected void ResetPropertyChangedHooks()
        {
            if (PropertyChanged != null)
            {
                var delegateList = PropertyChanged.GetInvocationList();
                foreach (var handler in delegateList)
                    PropertyChanged -= (PropertyChangedEventHandler)handler;
            }
        }

        #region INotifyDataErrorInfo implementation

        public IEnumerable GetErrors(string propertyName)
        {
            List<string> errorsForName = new List<string>();

            if (!string.IsNullOrEmpty(propertyName))
                _errors.TryGetValue(propertyName, out errorsForName);

            return errorsForName;
        }

        private bool _hasErrors;
        //public bool HasErrors { get => _errors.Any(kv => kv.Value != null && kv.Value.Count > 0); }
        public bool HasErrors
        {
            get { return _hasErrors; }
            private set
            {
                if (_hasErrors != value)
                {
                    _hasErrors = value;
                    RaisePropertyChanged();
                }
            }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected virtual void RaiseErrorsChanged(string p)
        {
            _hasErrors = _errors.Any(kv => kv.Value != null && kv.Value.Any());

            if (ErrorsChanged != null)
                ErrorsChanged.Invoke(this, new DataErrorsChangedEventArgs(p));
        }

        #endregion

        protected ConcurrentDictionary<string, List<string>> _errors = new ConcurrentDictionary<string, List<string>>();

        protected Task ValidateAsync()
        {
            return Task.Run(() => Validate());
        }

        private readonly object _lock = new object();
        public void Validate()
        {
            lock (_lock)
            {
                var validationContext = new ValidationContext(this, null, null);
                var validationResults = new List<ValidationResult>();
                Validator.TryValidateObject(this, validationContext, validationResults, true);

                foreach (var kv in _errors.ToList())
                {
                    if (validationResults.All(r => r.MemberNames.All(m => m != kv.Key)))
                    {
                        _errors.TryRemove(kv.Key, out List<string> outLi);
                        RaiseErrorsChanged(kv.Key);
                    }
                }

                var q = from r in validationResults
                        from m in r.MemberNames
                        group r by m into g
                        select g;

                foreach (var prop in q)
                {
                    var messages = prop.Select(r => r.ErrorMessage).ToList();

                    if (_errors.ContainsKey(prop.Key))
                        _errors.TryRemove(prop.Key, out List<string> outLi);

                    _errors.TryAdd(prop.Key, messages);
                    RaiseErrorsChanged(prop.Key);
                }
            }
        }
    }
}
