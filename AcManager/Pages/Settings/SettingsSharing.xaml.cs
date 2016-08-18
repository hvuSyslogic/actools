﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AcManager.LargeFilesSharing;
using AcManager.Tools.Helpers;
using AcManager.Tools.Lists;
using AcManager.Tools.Miscellaneous;
using AcManager.Tools.SemiGui;
using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Helpers;
using FirstFloor.ModernUI.Presentation;

namespace AcManager.Pages.Settings {
    public partial class SettingsSharing {
        public class SharingViewModel : NotifyPropertyChanged {
            public SettingsHolder.SharingSettings Sharing => SettingsHolder.Sharing;

            public BetterObservableCollection<SharedEntry> History => SharingHelper.Instance.History;

            public ILargeFileUploader[] UploadersList => Uploaders.List;

            private ILargeFileUploader _selectedUploader;

            public ILargeFileUploader SelectedUploader {
                get { return _selectedUploader; }
                set {
                    if (Equals(value, _selectedUploader)) return;
                    _selectedUploader = value;
                    OnPropertyChanged();
                    SignInCommand.OnCanExecuteChanged();

                    SelectedUploader.Prepare(default(CancellationToken)).Forget();
                }
            }

            private DirectoryEntry _uploaderDirectory;

            public DirectoryEntry UploaderDirectory {
                get { return _uploaderDirectory; }
                set {
                    if (Equals(value, _uploaderDirectory)) return;
                    _uploaderDirectory = value;
                    OnPropertyChanged();
                    SelectedUploader.DestinationDirectoryId = value.Id;
                }
            }

            private DirectoryEntry[] _uploaderDirectories;

            public DirectoryEntry[] UploaderDirectories {
                get { return _uploaderDirectories; }
                set {
                    if (Equals(value, _uploaderDirectories)) return;
                    _uploaderDirectories = value;
                    OnPropertyChanged();
                }
            }

            private AsyncCommand _signInCommand;

            public AsyncCommand SignInCommand => _signInCommand ?? (_signInCommand = new AsyncCommand(async o => {
                if (SelectedUploader == null) return;

                try {
                    await SelectedUploader.SignIn(default(CancellationToken));
                } catch (Exception e) {
                    NonfatalError.Notify("Can’t sign in", "Make sure Internet-connection works.", e);
                }
            }, o => SelectedUploader?.IsReady == false));

            private RelayCommand _resetCommand;

            public RelayCommand ResetCommand => _resetCommand ?? (_resetCommand = new RelayCommand(o => {
                SelectedUploader.Reset();
                CommandManager.InvalidateRequerySuggested();
            }, o => SelectedUploader?.IsReady == true));

            private AsyncCommand _updateDirectoriesCommand;

            public AsyncCommand UpdateDirectoriesCommand => _updateDirectoriesCommand ?? (_updateDirectoriesCommand = new AsyncCommand(async o => {
                if (SelectedUploader == null) return;

                try {
                    UploaderDirectories = await SelectedUploader.GetDirectories(default(CancellationToken));
                    UploaderDirectory = UploaderDirectories.GetChildByIdOrDefault(SelectedUploader.DestinationDirectoryId);
                } catch (Exception e) {
                    NonfatalError.Notify("Can’t load list of directories", "Make sure Internet-connection works.", e);
                }
            }, o => SelectedUploader?.SupportsDirectories == true));

            public SharingViewModel() {
                SelectedUploader = UploadersList.FirstOrDefault();
            }
        }

        public SettingsSharing() {
            InitializeComponent();
            DataContext = new SharingViewModel();
            Model.PropertyChanged += Model_PropertyChanged;
        }

        public SharingViewModel Model => (SharingViewModel)DataContext;

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            Model.UploaderDirectory = UploaderDirectoriesTreeView.SelectedItem as DirectoryEntry ?? Model.UploaderDirectories?.FirstOrDefault();
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SharingViewModel.UploaderDirectories):
                    break;

                case nameof(SharingViewModel.UploaderDirectory):
                    UploaderDirectoriesTreeView.SetSelectedItem(Model.UploaderDirectory);
                    break;
            }
        }

        private void ScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            var scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void History_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var value = HistoryDataGrid.SelectedValue as SharedEntry;
            if (value != null) {
                Process.Start(value.Url + "#noauto");
            }
        }
    }
}
