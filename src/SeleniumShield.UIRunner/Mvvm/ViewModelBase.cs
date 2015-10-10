using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;

namespace SeleniumShield.UIRunner.Mvvm
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        protected IList<PropertyValue> Values = new List<PropertyValue>();
        private bool _isDirty;

        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                _isDirty = value;

                if (!_isDirty)
                {
                    ResetAllPropertyDirtyFlags();
                }

                FirePropertyChanged(() => IsDirty);
            }
        }

        public void InvokeUIAction(Action action)
        {
            Application.Current.Dispatcher.Invoke(action);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ResetAllPropertyDirtyFlags()
        {
            foreach (var propertyValue in Values)
            {
                propertyValue.IsDirty = false;
            }
        }

        public void RevertProperty(string propertyName)
        {
            if (!HasPropertyValue(propertyName))
                return;

            var propertyValue = GetPropertyValue(propertyName);

            propertyValue.Revert();

            RefreshIsDirty();
        }

        private void RefreshIsDirty()
        {
            IsDirty = Values.Any(x => x.IsDirty);
        }

        public bool IsPropertyDirty(string propertyName)
        {
            if (!HasPropertyValue(propertyName))
                return false;

            var propertyValue = GetPropertyValue(propertyName);
            return propertyValue.IsDirty;
        }

        protected void Set<T>(Expression<Func<T>> getter, T value)
        {
            var propertyName = GetPropertyName(getter);

            var propertyValue = GetPropertyValue(propertyName);
            var hasPreviousValue = HasPropertyValue(propertyName);

            var oldValue = default(T);

            if (hasPreviousValue)
                oldValue = (T)propertyValue.Value;
            else
            {
                propertyValue = new PropertyValue(propertyName, default(T));
                Values.Add(propertyValue);
            }

            if (!Equals(value, oldValue))
            {
                propertyValue.Value = value;

                RefreshIsDirty();

                FirePropertyChanged(propertyName);
            }
        }

        private bool HasPropertyValue(string propertyName)
        {
            return Values.Any(x => x.PropertyName == propertyName);
        }

        private PropertyValue GetPropertyValue(string propertyName)
        {
            return Values.FirstOrDefault(x => x.PropertyName == propertyName);
        }

        protected T Get<T>(Expression<Func<T>> getter)
        {
            var propertyName = GetPropertyName(getter);

            if (!HasPropertyValue(propertyName))
                return default(T);

            return (T)GetPropertyValue(propertyName).Value;
        }

        protected void FirePropertyChanged<T>(Expression<Func<T>> getter)
        {
            var propertyName = ExpressionParser.GetPropertyName(getter);
            FirePropertyChanged(propertyName);
        }

        protected void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private string GetPropertyName<T>(Expression<Func<T>> getter)
        {
            var propertyName = ExpressionParser.GetPropertyName(getter);
            return propertyName;
        }

        public void ForceValidation()
        {
            foreach (var propertyName in GetAllPublicPropertyNames())
            {
                FirePropertyChanged(propertyName);
            }
        }

        private IEnumerable<string> GetAllPublicPropertyNames()
        {
            return GetAllPublicProperties().Select(x => x.Name).ToList();
        }

        private IEnumerable<PropertyInfo> GetAllPublicProperties()
        {
            var properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            return properties;
        }

        protected class PropertyValue
        {
            private object _value;
            private object _lastCleanValue;

            public PropertyValue(string propertyName, object initialValue)
            {
                PropertyName = propertyName;
                _value = initialValue;
                _lastCleanValue = initialValue;
            }

            public string PropertyName { get; private set; }

            public object Value
            {
                get { return _value; }
                set
                {
                    _value = value;
                    IsDirty = true;
                }
            }

            private bool _isDirty;
            public bool IsDirty
            {
                get { return _isDirty; }
                set
                {
                    _isDirty = value;

                    if (!_isDirty)
                        _lastCleanValue = Value;
                }
            }

            public void Revert()
            {
                Value = _lastCleanValue;
                IsDirty = false;
            }
        }
    }
}