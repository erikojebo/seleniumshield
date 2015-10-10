using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SeleniumShield.UIRunner.Infrastructure;
using SeleniumShield.UIRunner.ViewModels;

namespace SeleniumShield.UIRunner
{
    public partial class MainWindow
    {
        private bool _autoScroll = true;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new FlowListViewModel();

            EventAggregator.FlowExecutionFailed += OnFlowExecutionFailed;

            LogScrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
        }

        private void OnFlowExecutionFailed(string message)
        {
            MessageBox.Show($"The flow could not be executed: {message}", "Flow execution failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        internal FlowListViewModel ViewModel => DataContext as FlowListViewModel;

        private void OnBrowseFlowAssemblyClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = "DLL (*.dll)|*.dll",
                Title = "Choose assembly containing automation flows"
            };

            var result = fileDialog.ShowDialog();

            if (result == true)
            {
                ViewModel.FlowAssemblyPath = fileDialog.FileName;
            }
        }

        private void ScrollViewer_ScrollChanged(Object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset autoscroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if (LogScrollViewer.VerticalOffset == LogScrollViewer.ScrollableHeight)
                {   // Scroll bar is in bottom
                    // Set autoscroll mode
                    _autoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset autoscroll mode
                    _autoScroll = false;
                }
            }

            // Content scroll event : autoscroll eventually
            if (_autoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and autoscroll mode set
                // Autoscroll
                LogScrollViewer.ScrollToVerticalOffset(LogScrollViewer.ExtentHeight);
            }
        }
    }
}
