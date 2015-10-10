using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using SeleniumShield.Loader;
using SeleniumShield.Output;
using SeleniumShield.UIRunner.Mvvm;

namespace SeleniumShield.UIRunner.ViewModels
{
    internal class FlowListViewModel : ViewModelBase, IResultListener
    {
        private readonly FlowAssemblyLoader _flowLoader;

        public FlowListViewModel()
        {
            InitializeFlowListCommand = new DelegateCommand(InitializeFlowList);
            Flows = new ObservableCollection<FlowViewModel>();

            _flowLoader = new FlowAssemblyLoader();

            if (Environment.MachineName == "WL08127")
                FlowAssemblyPath = @"C:\code\seleniumshield\src\SeleniumShield.Tests\bin\Debug\SeleniumShield.Tests.dll";
        }

        public string Log
        {
            get { return Get(() => Log); }
            set { Set(() => Log, value); }
        }

        public string FlowAssemblyPath
        {
            get { return Get(() => FlowAssemblyPath); }
            set { Set(() => FlowAssemblyPath, value); }
        }

        public ObservableCollection<FlowViewModel> Flows { get; set; }

        private async void InitializeFlowList()
        {
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

            var flowTypes = await _flowLoader.LoadFlowTypes(FlowAssemblyPath);

            Flows.Clear();

            var options = new AutomationFlowRunnerOptions()
            {
                ResultListener = this
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

        public DelegateCommand InitializeFlowListCommand
        {
            get { return Get(() => InitializeFlowListCommand); }
            set { Set(() => InitializeFlowListCommand, value); }
        }

        public void OutputLine(string message, params object[] formatParams)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Log += Environment.NewLine + string.Format(message, formatParams);
            });
        }
    }
}