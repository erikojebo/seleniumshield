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
            IsOptional = parameterInfo.IsOptional;
            DefaultValue = (parameterInfo.DefaultValue ?? "").ToString();
            HasDefaultValue = parameterInfo.DefaultValue != null;

            DisplayText = GetDisplayText(parameterInfo);
        }

        public string Name { get; }
        public string TypeName { get; }
        public string DisplayText { get; }
        public string DefaultValue { get; }
        public bool HasDefaultValue { get; }
        public bool IsOptional { get; }

        public string Value
        {
            get { return Get(() => Value); }
            set { Set(() => Value, value); }
        }

        public object TypedValue
        {
            get
            {
                if (IsOptional && HasDefaultValue && string.IsNullOrEmpty(Value))
                {
                    return Type.Missing;
                }

                try
                {
                    return ConvertValueFromString();
                }
                catch (Exception)
                {
                    throw new SeleniumShieldConversionException($"Could not convert value '{Value}' to type '{_parameterInfo.ParameterType}' needed for parameter '{_parameterInfo.Name}'");
                }
            }
        }

        private object ConvertValueFromString()
        {
            var converter = TypeDescriptor.GetConverter(_parameterInfo.ParameterType);
            return converter.ConvertFromInvariantString(Value);
        }

        private bool CanConvertValueFromString()
        {
            try
            {
                ConvertValueFromString();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool HasValidValue => CanConvertValueFromString() || IsOptional;

        private string GetDisplayText(ParameterInfo parameterInfo)
        {
            var displayText = Name;

            if (parameterInfo.IsOptional)
            {
                displayText += " (optional";

                if (HasDefaultValue && !string.IsNullOrWhiteSpace(DefaultValue))
                {
                    displayText += ", default: " + DefaultValue;
                }

                displayText += ")";
            }

            return displayText;
        }
    }
}