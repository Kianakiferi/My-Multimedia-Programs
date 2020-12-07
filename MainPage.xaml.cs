using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
// using muxc = Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MultimediaApp
{
	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public static bool IsNavViewPaneOpen = false;

		private double NavViewCompactModeThresholdWidth { get { return NavView.CompactModeThresholdWidth; } }

		private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
		{
			("home", typeof(HomePage)),
			("huffman", typeof(HuffmanPage)),
			("pcm", typeof(PCMPage)),
			("jpeg", typeof(JpegPage))

		};

		public MainPage()
		{
			this.InitializeComponent();
			CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

		}

		private void NavView_Loaded(object sender, RoutedEventArgs args)
		{

			//ContentFrame导航的处理程序
			ContentFrame.Navigated += On_Navigated;


			// NavView默认情况下不会加载任何页面，OnLoaded加载主页界面
			NavView.SelectedItem = NavView.MenuItems[0];

			//因为使用ItemInvoked进行导航，所以调用Navigate()来加载主页
			NavView_Navigate("home", new EntranceNavigationTransitionInfo());


			//响应键盘按键返回导航的处理程序
			var goBack = new KeyboardAccelerator { Key = Windows.System.VirtualKey.GoBack };
			goBack.Invoked += BackInvoked;
			this.KeyboardAccelerators.Add(goBack);

			var altLeft = new KeyboardAccelerator
			{
				Key = Windows.System.VirtualKey.Left,
				Modifiers = Windows.System.VirtualKeyModifiers.Menu
			};
			altLeft.Invoked += BackInvoked;
			this.KeyboardAccelerators.Add(altLeft);
		}

		//使用Item的导航
		private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
		{
			FrameNavigationOptions navOptions = new FrameNavigationOptions();
			navOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;
			if (args.IsSettingsInvoked is true)
			{
				//ContentFrame.Navigate(typeof(SettingsPage));
				NavView_Navigate("settingsPage", args.RecommendedNavigationTransitionInfo);
			}
			else if (args.InvokedItemContainer != null)
			{
				//System.Diagnostics.Debug.WriteLine("args.InvokedItem");
				var navItemTag = args.InvokedItemContainer.Tag.ToString();
				NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
			}

		}

		//通用的导航
		private void NavView_Navigate(string navItemTag, NavigationTransitionInfo transitionInfo)
		{
			Type _page = null;
			if (navItemTag == "settingsPage")
			{
				_page = typeof(SettingsPage);
			}
			else
			{
				var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
				_page = item.Page;
			}

			//在导航至页面前，先获取页面类型，以防止在返回栈列中出现重复的对象
			var preNavPageType = ContentFrame.CurrentSourcePageType;

			//只当选择的页面未加载的情况下，进行导航
			if (!(_page is null) && !Type.Equals(preNavPageType, _page))
			{
				ContentFrame.Navigate(_page, null, transitionInfo);
			}
		}
		private void On_Navigated(object sender, NavigationEventArgs e)
		{
			NavView.IsBackEnabled = ContentFrame.CanGoBack;

			if (ContentFrame.SourcePageType == typeof(SettingsPage))
			{
				// SettingsItem不属于NavView.MenuItems，也没有Tag
				NavView.SelectedItem = (NavigationViewItem)NavView.SettingsItem;
				NavView.Header = "Settings";
			}
			else if (ContentFrame.SourcePageType != null)
			{
				var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

				NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().First(newItem => newItem.Tag.Equals(item.Tag));

				NavView.Header = ((NavigationViewItem)NavView.SelectedItem)?.Content?.ToString();
			}
		}

		//返回导航处理
		private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
		{
			On_BackRequested();
		}
		private void BackInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			On_BackRequested();
			args.Handled = true;
		}
		private bool On_BackRequested()
		{
			if (!ContentFrame.CanGoBack)
				return false;

			// 当NavViewPan被overlayed时，不返回
			if (NavView.IsPaneOpen &&
				(NavView.DisplayMode == NavigationViewDisplayMode.Compact ||
				 NavView.DisplayMode == NavigationViewDisplayMode.Minimal))
				return false;

			ContentFrame.GoBack();
			return true;
		}

		private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}

		//在此示例中未使用NavView_SelectionChanged，但出于完整性考虑而显示。
		//通常，您可以使用ItemInvoked或SelectionChanged来执行导航，但不能同时使用它们。
		private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
		{
			if (args.IsSettingsSelected == true)
			{
				NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
			}
			else if (args.SelectedItemContainer != null)
			{
				var navItemTag = args.SelectedItemContainer.Tag.ToString();
				NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
			}
		}

		//private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
		//{

		//}

		private void NavView_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
		{
			AppTitleText.Visibility = Visibility.Collapsed;
			IsNavViewPaneOpen = false;
		}

		private void NavView_PaneOpening(NavigationView sender, object args)
		{
			AppTitleText.Visibility = Visibility.Visible;
			IsNavViewPaneOpen = true;
		}


	}
}
