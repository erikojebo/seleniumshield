using System;
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