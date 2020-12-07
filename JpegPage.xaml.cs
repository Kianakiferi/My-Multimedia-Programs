using System;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MultimediaApp
{
	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	public sealed partial class JpegPage : Page
	{
		public JpegPage()
		{
			this.InitializeComponent();
		}

		private async void BtmOpenFile_ClickAsnyc(object sender, RoutedEventArgs e)
		{
			var picker = new Windows.Storage.Pickers.FileOpenPicker();
			picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail; //可通过使用图片缩略图创建丰富的视觉显示，以显示文件选取器中的文件
			picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
			picker.FileTypeFilter.Add(".jpg");
			picker.FileTypeFilter.Add(".jpeg");
			picker.FileTypeFilter.Add(".png");

			StorageFile inputFile = await picker.PickSingleFileAsync();

			if (inputFile != null)
			{
				SoftwareBitmap softwareBitmap;

				using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
				{
					// Create the decoder from the stream
					BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

					// Get the SoftwareBitmap representation of the file
					softwareBitmap = await decoder.GetSoftwareBitmapAsync();

					//Image control only support BGRA8 coding images, if not convert.
					if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
					{
						softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
					}

					var source = new SoftwareBitmapSource();
					await source.SetBitmapAsync(softwareBitmap);

					// Set the source of the Image control
					ImgOriginal.Source = source;
				}
			}

		}
	}
}
