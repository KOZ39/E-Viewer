﻿using ExClient;
using ExViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ExViewer.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CachedPage : Page,IHasAppBar
    {
        public CachedPage()
        {
            this.InitializeComponent();
            VM = CachedVM.Instance;
            cdg_ConfirmClear = new ContentDialog()
            {
                Title = LocalizedStrings.Resources.ClearCachedDialogTitle,
                Content = LocalizedStrings.Resources.ClearCachedDialogContent,
                PrimaryButtonText = LocalizedStrings.Resources.OK,
                SecondaryButtonText = LocalizedStrings.Resources.Cancel,
                PrimaryButtonCommand = VM.Clear
            };
        }

        public CachedVM VM
        {
            get
            {
                return (CachedVM)GetValue(VMProperty);
            }
            set
            {
                SetValue(VMProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register("VM", typeof(CachedVM), typeof(CachedPage), new PropertyMetadata(null));

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.NavigationMode != NavigationMode.Back || VM.Galleries == null)
            {
                VM.Refresh.Execute(null);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void btn_Pane_Click(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.SwitchSplitView();
        }

        private void lv_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(VM.Open.CanExecute(e.ClickedItem))
                VM.Open.Execute(e.ClickedItem);
        }

        private ContentDialog cdg_ConfirmClear;

        private async void abb_DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            cdg_ConfirmClear.RequestedTheme = Settings.SettingCollection.Current.Theme.ToElementTheme();
            await cdg_ConfirmClear.ShowAsync();
        }

        private void gv_Gallery_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            e.Handled = true;
        }

        private void gv_Gallery_Holding(object sender, HoldingRoutedEventArgs e)
        {
            var s = (FrameworkElement)sender;
            switch(e.HoldingState)
            {
            case Windows.UI.Input.HoldingState.Started:
                FlyoutBase.ShowAttachedFlyout(s);
                e.Handled = true;
                break;
            case Windows.UI.Input.HoldingState.Canceled:
                FlyoutBase.GetAttachedFlyout(s).Hide();
                e.Handled = true;
                break;
            default:
                break;
            }
        }

        private void lv_RefreshRequested(object sender, EventArgs args)
        {
            VM.Refresh.Execute(null);
        }

        private void lv_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var source = e.OriginalSource as ListViewItem;
            if(source == null)
                return;
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)source.ContentTemplateRoot);
            e.Handled = true;
        }

        public void CloseAll()
        {
            cb_top.IsOpen = false;
        }
    }
}