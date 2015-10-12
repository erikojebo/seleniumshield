using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using SeleniumShield.Loader;
using SeleniumShield.Metadata;
using SeleniumShield.Output;
using SeleniumShield.UIRunner.Infrastructure;
using SeleniumShield.UIRunner.Mvvm;

namespace SeleniumShield.UIRunner.ViewModels
{
    internal class FlowListViewModel : ViewModelBase, IResultListener
    {
        private readonly FlowAssemblyLoader _flowLoader;
        private readonly UserSettingsService _settingsService;

        public FlowListViewModel()
        {
            InitializeFlowListCommand = new DelegateCommand(InitializeFlowList);
            ExecuteAllFlowsCommand = new DelegateCommand(ExecuteAllFlows);
            FlowGroups = new ObservableCollection<FlowGroupViewModel>();

            _flowLoader = new FlowAssemblyLoader();
            _settingsService = new UserSettingsService();

            FlowAssemblyPath = _settingsService.LastFlowAssemblyPath;

            if (!string.IsNullOrWhiteSpace(FlowAssemblyPath))
            {
                InitializeFlowList();
            }
        }

        public string Log
        {
            get { return Get(() => Log); }
            set { Set(() => Log, value); }
        }

        public string FlowAssemblyPath
        {
            get { return Get(() => FlowAssemblyPath); }
            set
            {
                Set(() => FlowAssemblyPath, value);

                FlowAssemblyFileName = Path.GetFileName(value);

                _settingsService.LastFlowAssemblyPath = value;
            }
        }

        public string FlowAssemblyFileName
        {
            get { return Get(() => FlowAssemblyFileName); }
            set
            {
                Set(() => FlowAssemblyFileName, value);
            }
        }

        public ObservableCollection<FlowGroupViewModel> FlowGroups { get; }
        public DelegateCommand InitializeFlowListCommand { get; }
        public DelegateCommand ExecuteAllFlowsCommand { get; }

        public async void InitializeFlowList()
        {
            FlowGroups.Clear();

            var flowTypes = await _flowLoader.LoadFlowTypes(FlowAssemblyPath);
            var flowTypeList = flowTypes.ToList();

            var flowGroups = flowTypeList.GroupBy(x => x.GetCustomAttribute<UIExecutableAttribute>().DependencyGroup);

            foreach (var flowGroup in flowGroups)
            {
                var groupViewModel = new FlowGroupViewModel(flowGroup.Key, flowGroup, this);

                FlowGroups.Add(groupViewModel);
            }
        }

        private void ExecuteAllFlows()
        {
            var flowsToExecute = new List<FlowViewModel>();

            var allFlowViewModels = FlowGroups.SelectMany(x => x.Flows).ToList();

            foreach (var flowViewModel in allFlowViewModels)
            {
                flowViewModel.CouldBeExecuted = null;
                flowViewModel.WasIgnored = null;
            }

            var viewModelsByFlowType = allFlowViewModels.GroupBy(x => x.FlowType).ToList();

            foreach (var g in viewModelsByFlowType)
            {
                var executableViewModel = g.OrderByDescending(x => x.Parameters.Count).FirstOrDefault(x => x.HasValidParameterValues);

                if (executableViewModel != null)
                    flowsToExecute.Add(executableViewModel);
            }

            var flowViewModelsWhichCouldNotBeExecuted = allFlowViewModels.Where(x => !flowsToExecute.Any(y => y.FlowType == x.FlowType));

            foreach (var flowViewModel in flowViewModelsWhichCouldNotBeExecuted)
            {
                flowViewModel.CouldBeExecuted = false;
            }

            var flowViewModelsWhichWereIgnored = allFlowViewModels.Where(x => flowsToExecute.Any(y => y.FlowType == x.FlowType) && !flowsToExecute.Contains(x));

            foreach (var flowViewModel in flowViewModelsWhichWereIgnored)
            {
                flowViewModel.WasIgnored = true;
            }

            foreach (var flowListViewModel in flowsToExecute)
            {
                flowListViewModel.Execute();
            }
        }

        public void OutputLine(string message, params object[] formatParams)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                Log += Environment.NewLine + string.Format(message, formatParams);
            }));
        }
    }
}