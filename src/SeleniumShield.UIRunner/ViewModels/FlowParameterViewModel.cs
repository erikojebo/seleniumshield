using System;
using System.ComponentModel;
using System.Reflection;
using SeleniumShield.UIRunner.Exceptions;
using SeleniumShield.UIRunner.Mvvm;

namespace SeleniumShield.UIRunner.ViewModels
{
    internal class FlowParameterViewModel : ViewModelBase
    {
        private readonly ParameterInfo _parameterInfo;

        public FlowParameterViewModel(ParameterInfo parameterInfo)
        {
            _parameterInfo = parameterInfo;
            Name = parameterInfo.Name;
            TypeName = parameterInfo.ParameterType.Name;
        }

        public string Name { get; private set; }
        public string TypeName { get; private set; }

        public string Value
        {
            get { return Get(() => Value); }
            set { Set(() => Value, value); }
        }

        public object TypedValue
        {
            get
            {
                try
                {
                    var converter = TypeDescriptor.GetConverter(_parameterInfo.ParameterType);
                    return converter.ConvertFromInvariantString(Value);
                }
                catch (Exception)
                {
                    throw new SeleniumShieldConversionException($"Could not convert value '{Value}' to type '{_parameterInfo.ParameterType}' needed for parameter '{_parameterInfo.Name}'");
                }
            }
        }
    }
}