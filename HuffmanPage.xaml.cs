using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MultimediaApp
{
	public class ListView_Item
	{
		public string charValue { get; set; }
		public string stringValue { get; set; }

		public ListView_Item(char tChar, string tString)
		{
			charValue = tChar.ToString();
			stringValue = tString;
		}
	}

	public sealed partial class HuffmanPage : Page
	{			
		private HuffmanCoding huffman;
		private Dictionary<char, string> keyAndValues;
		List<HuffmanCoding.OneLineNode> dicNodes;

		public HuffmanPage()
		{
			this.InitializeComponent();
		}

		private void HuffmanPage_Txt_Grid_DragOver(object sender, DragEventArgs e)
		{
			e.AcceptedOperation = DataPackageOperation.Copy;
		}

		private async void HuffmanPage_Txt_Grid_Drop(object sender, DragEventArgs e)
		{
			huffman = new HuffmanCoding();
			keyAndValues = null;
			List<StorageFile> storageFiles = new List<StorageFile>();

			if (e.DataView.Contains(StandardDataFormats.StorageItems))
			{
				var itemsReadOnlyList = await e.DataView.GetStorageItemsAsync();
				foreach (var item in itemsReadOnlyList)
				{
					storageFiles.Add((StorageFile)item);
				}

				if (storageFiles.Count > 0)
				{
					StorageFolder storageFolder;

					foreach (var one in storageFiles)
					{
						if (one.FileType != ".txt")
						{
							return;
						}
						else
						{
							storageFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(one.Path));
							var buffer = await FileIO.ReadBufferAsync(await storageFolder.GetFileAsync(one.Name));
							using (var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(buffer))
							{
								if (one.DisplayName.Contains(".Coded"))
								{
									StorageFile decodedFile = await storageFolder.CreateFileAsync(one.DisplayName.Replace(".Coded", ".Decoded") + one.FileType, CreationCollisionOption.ReplaceExisting);

									string txt = dataReader.ReadString(buffer.Length);
									int txtDictionary_Index = txt.IndexOf("Dictionary:");
									string txtHuffmanCode = txt.Substring(0, txtDictionary_Index - 1);
									string txtDictionary = txt.Substring(txtDictionary_Index + 12);

									keyAndValues = huffman.CreateNewDictionary(txtDictionary);
									string txtDecoded = huffman.HuffmanCodeToString(keyAndValues, txtHuffmanCode);

									await FileIO.WriteTextAsync(decodedFile, txtDecoded);
								}
								else
								{
									StorageFile codedFile = await storageFolder.CreateFileAsync(one.DisplayName + ".Coded" + one.FileType, CreationCollisionOption.ReplaceExisting);

									string txtCoded = huffman.StringToHuffmanCode(out keyAndValues, dataReader.ReadString(buffer.Length));
									string txtDictionary = "";
									foreach (var two in keyAndValues)
									{
										txtDictionary = String.Concat(txtDictionary + two.Key + " " + two.Value + "\n");
									}
									string txtFianl = String.Concat(txtCoded + "\n" + "Dictionary:\n" + txtDictionary);

									await FileIO.WriteTextAsync(codedFile, txtFianl);
								}
							}
						}
					}
				}
			}
		}

		private void HuffmanPage_Txt_Grid_DragEnter(object sender, DragEventArgs e)
		{
			Debug.WriteLine(e.DragUIOverride);
		}

		private void Btm_Encoding_Click(object sender, RoutedEventArgs e)
		{
			string codingStr = TextBox_Left.Text;

			if (codingStr != "" && !String.IsNullOrWhiteSpace(codingStr))
			{
				huffman = new HuffmanCoding();
				keyAndValues = null;

				string huffmanCoded = huffman.StringToHuffmanCode(out keyAndValues, codingStr);

				Dialog_ListView.Items.Clear();
				AddDialogListViewItems();

				TextBox_Right.Text = huffmanCoded;
			}
			else
			{
				FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
			}

		}

		private void AddDialogListViewItems()
		{
			dicNodes = new List<HuffmanCoding.OneLineNode>();


			if (Dialog_ListView.Items.Count == 0)
			{
				foreach (var one in keyAndValues)
				{
					dicNodes.Add(new HuffmanCoding.OneLineNode(one.Key, one.Value));
					Dialog_ListView.Items.Add(new ListView_Item(one.Key, one.Value));
				}
			}
		}

		private void Btm_Decoding_Click(object sender, RoutedEventArgs e)
		{
			huffman = new HuffmanCoding();
			string decodingStr = TextBox_Right.Text;

			if (decodingStr != "" && !String.IsNullOrWhiteSpace(decodingStr))
			{
				if (keyAndValues is null)
				{
					DisplayGetDictionaryDialog(decodingStr);
				}
				else
				{
					TextBox_Left.Text = huffman.HuffmanCodeToString(keyAndValues, decodingStr);
				}

			}
		}

		private async void DisplayGetDictionaryDialog(string text)
		{
			ContentDialogResult result = await Dialog_GetDictionary.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				string txtDic_FromClipboard = TextBox_GetDicStrFromClipboard.Text;

				keyAndValues = huffman.CreateNewDictionary(txtDic_FromClipboard);

				AddDialogListViewItems();
				TextBox_Left.Text = huffman.HuffmanCodeToString(keyAndValues, text);
			}
			else
			{

			}

		}

		private void Btm_Swap_Click(object sender, RoutedEventArgs e)
		{
			string textLeft = TextBox_Left.Text;
			TextBox_Left.Text = TextBox_Right.Text;
			TextBox_Right.Text = textLeft;
		}

		private async void Btm_ShowDictionary_ClickAsync(object sender, RoutedEventArgs e)
		{
			if (Dialog_ListView.Items.Count == 0)
			{
				FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
			}
			else
			{
				await Dialog_ShowDictionary.ShowAsync();
			}

		}

		private void Btm_CleanAll_Click(object sender, RoutedEventArgs e)
		{
			Dialog_ListView.Items.Clear();
			keyAndValues = null;
			TextBox_Left.Text = "";
			TextBox_Right.Text = "";
		}

		private void Btm_CopyDicToClipboard_Click(object sender, RoutedEventArgs e)
		{
			string dicToText = string.Empty;
			foreach (var item in dicNodes)
			{
				dicToText = dicToText + item.ToString();
			}
			DataPackage dataPackage = new DataPackage();
			dataPackage.SetText(dicToText);
			Clipboard.SetContent(dataPackage);
		}
	}
}
