using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SeleniumShield.UIRunner.Mvvm;

namespace SeleniumShield.UIRunner.ViewModels
{
    internal class FlowGroupViewModel : ViewModelBase
    {
        private readonly FlowListViewModel _flowListViewModel;

        public FlowGroupViewModel(string name, IEnumerable<Type> flowTypes, FlowListViewModel flowListViewModel)
        {
            _flowListViewModel = flowListViewModel;

            Name = name;
            Flows = new ObservableCollection<FlowViewModel>();

            var allowedParameterTypes = new[]
            {
                typeof(int),
                typeof(int?),
                typeof(decimal),
                typeof(decimal?),
                typeof(short),
                typeof(short?),
                typeof(long),
                typeof(long?),
                typeof(float),
                typeof(float?),
                typeof(bool),
                typeof(bool?),
                typeof(string),
                typeof(char),
                typeof(char?),
                typeof(DateTime),
                typeof(DateTime?)
            };

            var options = new AutomationFlowRunnerOptions()
            {
                ResultListener = _flowListViewModel
            };

            foreach (var flowType in flowTypes)
            {
                var constructors = flowType.GetConstructors().OrderByDescending(x => x.GetParameters().Length);

                foreach (var constructorInfo in constructors)
                {
                    var parameters = constructorInfo.GetParameters();

                    if (parameters.All(x => allowedParameterTypes.Contains(x.ParameterType)))
                    {
                        var parameterViewModels = parameters.Select(x => new FlowParameterViewModel(x));
                        var flowViewModel = new FlowViewModel(flowType, constructorInfo, parameterViewModels, options);

                        Flows.Add(flowViewModel);
                    }
                }
            }
        }

        public string Name { get; }
        public ObservableCollection<FlowViewModel> Flows { get; } 
    }
}