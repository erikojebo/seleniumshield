using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using SeleniumShield.UIRunner.Exceptions;
using SeleniumShield.UIRunner.Infrastructure;
using SeleniumShield.UIRunner.Mvvm;

namespace SeleniumShield.UIRunner.ViewModels
{
    internal class FlowViewModel : ViewModelBase
    {
        private readonly ConstructorInfo _constructor;
        private readonly AutomationFlowRunnerOptions _options;
        private AutomationFlow _flow;

        public FlowViewModel(
            Type flowType,
            ConstructorInfo constructor,
            IEnumerable<FlowParameterViewModel> parameterViewModels,
            AutomationFlowRunnerOptions options)
        {
            _constructor = constructor;
            _options = options;

            Name = flowType.Name;
            Parameters = new ObservableCollection<FlowParameterViewModel>(parameterViewModels);
            ExecuteCommand = new DelegateCommand(Execute);
        }

        public bool? WasSuccessful
        {
            get { return Get(() => WasSuccessful); }
            set { Set(() => WasSuccessful, value); }
        }

        public bool? IsFlowExecuting
        {
            get { return Get(() => IsFlowExecuting); }
            set { Set(() => IsFlowExecuting, value); }
        }

        public double FlowProgressPercentageValue
        {
            get { return Get(() => FlowProgressPercentageValue); }
            set { Set(() => FlowProgressPercentageValue, value); }
        }

        public bool? IsFlowProgressIndeterminate
        {
            get { return Get(() => IsFlowProgressIndeterminate); }
            set { Set(() => IsFlowProgressIndeterminate, value); }
        }

        public string Name { get; private set; }
        public ObservableCollection<FlowParameterViewModel> Parameters { get; }
        public DelegateCommand ExecuteCommand { get; private set; }

        private async void Execute()
        {
            IsFlowExecuting = true;
            IsFlowProgressIndeterminate = true;

            WasSuccessful = await ExecuteAsync();

            IsFlowProgressIndeterminate = false;
            IsFlowExecuting = false;
        }

        private async Task<bool> ExecuteAsync()
        {
            return await Task.Run(() =>
            {
                using (var runner = new AutomationFlowRunner(_options))
                {
                    try
                    {
                        runner.StepCompleted += OnStepCompleted;

                        _flow = (AutomationFlow)_constructor.Invoke(Parameters.Select(x => x.TypedValue).ToArray());
                        runner.AppendFlow(_flow);
                        var result = runner.Execute();

                        return result.WasSuccessful;
                    }
                    catch (Exception ex)
                    {
                        EventAggregator.FireFlowExecutionFailed(ex.Message);
                    }
                    finally
                    {
                        runner.StepCompleted -= OnStepCompleted;

                        InvokeUIAction(() => { IsFlowProgressIndeterminate = true; });
                    }

                    return false;
                }
            });
        }

        private void OnStepCompleted(AutomationStepRunResult result, int stepIndex, int totalStepCount)
        {
            InvokeUIAction(() =>
            {
                IsFlowProgressIndeterminate = false;

                FlowProgressPercentageValue = (stepIndex + 1) / (double)totalStepCount * 100;
            });
        }
    }
}