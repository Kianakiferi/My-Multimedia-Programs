using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Compression;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MultimediaApp
{
	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	public sealed partial class CreatePage : Page
	{
		public List<IStorageItem> filesPaths_AddOnly;

		public CreatePage()
		{
			this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (e.Parameter is IReadOnlyList<IStorageItem> items)
			{
				filesPaths_AddOnly = new List<IStorageItem>();
				foreach (var one in items)
				{
					Debug.WriteLine(one.Path);
					ListView_CreateFile.Items.Add(one.Path);
					filesPaths_AddOnly.Add(one);
				}
				TextBox_NewFilePath.Text = String.Concat(Path.GetDirectoryName(items[0].Path) + "\\NewFile.zip");
			}
			else
			{
				Debug.WriteLine("e is null!");
			}
		}

		private void Btm_ChoooooPath_Click(object sender, RoutedEventArgs e)
		{

		}

		private async void Btm_DoZip_Click(object sender, RoutedEventArgs e)
		{
			/* 文件和流 I/O：Windows 应用商店应用程序中的 I/O 操作：
			 * 基于路径的压缩类型 ZipFile 和 ZipFileExtensions 不可用。请改用 Windows.Storage.Compression 命名空间中的所有类型。
			
			string startPath = @"D:\MyZip\files";
			string zipPath = @"D:\MyZip\result.zip";
			string extractPath = @"D:\MyZip\extract";

			ZipFile.CreateFromDirectory(startPath, zipPath);
			ZipFile.ExtractToDirectory(zipPath, extractPath);
			*/

			foreach (var one in filesPaths_AddOnly)
			{
				Debug.WriteLine(one.Path);
			}

			await DoYaSuo(null);
		}

		private async Task DoYaSuo(CompressAlgorithm? Algorithm)
		{
			try
			{

				//选择文件                
				var picker = new FileOpenPicker();
				picker.FileTypeFilter.Add("*");
				var originalFiles = await picker.PickMultipleFilesAsync();
				if (originalFiles.Count > 0)
				{
					foreach (var one in originalFiles)
					{
						Debug.WriteLine(one.Path);
						ListView_CreateFile.Items.Add(one.Path);
					}

				}


				/*//Progress_Text.Text += String.Format("\"{0}\" has been picked\n", originalFile.Name);

                //在原文件的名称后面加上 ".compressed"
                var compressedFilename = "result";
                var compressedFile = await KnownFolders.DocumentsLibrary.CreateFileAsync(
                                                        compressedFilename, CreationCollisionOption.GenerateUniqueName);

				//Progress_Text.Text += String.Format("\"{0}\" has been created to store compressed data\n", compressedFile.Name);

				// ** 压缩操作**
				// 1. 打开输入原始文件。
				// 2.打开输出流在文件被压缩和包装成压缩机对象.
				// 3.复制原始流进压缩机包装器
				// 4. 结束压缩机- 它把目标的标志到流中,然后输出中间缓冲
				//    buffers.
				//
				using (var originalInput = await originalFile.OpenReadAsync())
				using (var compressedOutput = await compressedFile.OpenAsync(FileAccessMode.ReadWrite))
				using (var compressor = !Algorithm.HasValue ?
				  new Compressor(compressedOutput.GetOutputStreamAt(0)) :
				  new Compressor(compressedOutput.GetOutputStreamAt(0), Algorithm.Value, 0))//所采用的压缩算法
				{
					//Progress_Text.Text += "All streams wired for compression\n";

					//复制源流到目标流。                   
					var bytesCompressed = await RandomAccessStream.CopyAsync(originalInput, compressor);
					var finished = await compressor.FinishAsync();//完成压缩流编写。    
					Progress_Text.Text += String.Format("Compressed {0} bytes into {1}\n", bytesCompressed, compressedOutput.Size);
				}

				var decompressedFilename = originalFile.Name + ".decompressed";
				var decompressedFile = await KnownFolders.DocumentsLibrary.CreateFileAsync(
														  decompressedFilename, CreationCollisionOption.GenerateUniqueName);
				Progress_Text.Text += String.Format("\"{0}\" has been created to store decompressed data\n", decompressedFile.Name);

				// ** 解压缩操作**
				// 1. 打开输入流在压缩文件并包装成减压器对象              
				// 2. 从文件中打开输出流,然后将解压缩存储数据写入到文件中。
				// 3. 从解压缩器(Decompressor)中把流复制到解压缩文件的流中
				using (var compressedInput = await compressedFile.OpenSequentialReadAsync())
				using (var decompressor = new Decompressor(compressedInput))
				using (var decompressedOutput = await decompressedFile.OpenAsync(FileAccessMode.ReadWrite))
				{
					Progress_Text.Text += "All streams wired for decompression\n";
					var bytesDecompressed = await RandomAccessStream.CopyAsync(decompressor, decompressedOutput);
					Progress_Text.Text += String.Format("Decompressed {0} bytes of data\n", bytesDecompressed);
				}*/


			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}


}
