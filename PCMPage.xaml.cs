using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace MultimediaApp
{
	public class PCMListView_Item
	{
		//public string Key { get; private set; }
		public string Value { get; private set; }

		public PCMListView_Item(string value)
		{
			this.Value = value;
		}
	}

	public class ValueToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return string.Format("{0:F2}", value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return value;
		}
	}

	public sealed partial class PCMPage : Page
	{
		SignalGenerator generator = new SignalGenerator(1);
		Signal signalList;

		LinkedList<PCMFrame> pcmFrames;
		LinkedList<ADPCMFrame> adpcmFrames;
		LinkedList<Sample> samplesList_Decoded_PCM;
		LinkedList<Sample> samplesList_Decoded_ADPCM;

		int harmonics = 1;
		double duration = 20;
		double frequency = 10;
		double amplitude = 10;
		int pcmBitResolution = 8;

		public PCMPage()
		{
			this.InitializeComponent();
			//signals = generator.GenerateSignal(duration, frequency, amplitude);
		}

		private async void Button_GeneratingSignal_ClickAsync(object sender, RoutedEventArgs e)
		{
			harmonics = (int)Slider_Harmonics.Value;
			if (harmonics >= 3)
			{
				ProgressBar_GeneratingSignal.Visibility = Visibility.Visible;
				ProgressBar_GeneratingSignal.IsIndeterminate = true;
				ProgressBar_GeneratingSignal.ShowPaused = false;
			}
			duration = Slider_Duration.Value;
			frequency = Slider_Frequency.Value;
			amplitude = Slider_Amplitude.Value;
			pcmBitResolution = 8;

			await Task.Run(() =>
			{
				generator = new SignalGenerator(harmonics);
				signalList = generator.GenerateSignal(duration, frequency, amplitude);
			});

			canvas_Left.Invalidate();
			ProgressBar_GeneratingSignal.ShowPaused = true;
			ProgressBar_GeneratingSignal.Visibility = Visibility.Collapsed;
		}

		private async void Button_DoCoding_ClickAsync(object sender, RoutedEventArgs e)
		{
			harmonics = (int)Slider_Harmonics.Value;
			if (harmonics >= 3)
			{
				ProgressBar_GeneratingSignal.Visibility = Visibility.Visible;
				ProgressBar_GeneratingSignal.IsIndeterminate = true;
				ProgressBar_GeneratingSignal.ShowPaused = false;
			}
			duration = Slider_Duration.Value;
			frequency = Slider_Frequency.Value;
			amplitude = Slider_Amplitude.Value;
			pcmBitResolution = 8;

			await Task.Run(() =>
			{
				pcmFrames = PCMCoding(pcmBitResolution);
				adpcmFrames = ADPCMCoding();
			});

			samplesList_Decoded_PCM = PCMDecoding(pcmBitResolution);
			samplesList_Decoded_ADPCM = ADPCMDecoding(pcmBitResolution);

			AddTo_PCMListView();
			AddTo_ADPCMListView();

			ReDrawCanvas();
		}


		private LinkedList<PCMFrame> PCMCoding(int bitResolution)
		{
			PCMEncoder pcmEncoder = new PCMEncoder();
			return pcmEncoder.GeneratePCMFramesFrom(signalList, bitResolution);
		}

		private LinkedList<ADPCMFrame> ADPCMCoding()
		{
			ADPCMEncoder adpcmEncoder = new ADPCMEncoder();
			return adpcmEncoder.EncodeADPCMFrames(pcmFrames);
		}

		private LinkedList<Sample> PCMDecoding(int bitResolution)
		{
			double samplingFrequency = 2 * frequency * Math.Pow(2, harmonics - 1);
			double maxAmplitude = signalList.GetSignalMaxAmplitude();
			PCMDecoder pcmDecoder = new PCMDecoder();

			return pcmDecoder.ConvertPCMToSignal(pcmFrames, maxAmplitude, bitResolution, samplingFrequency);
		}

		private LinkedList<Sample> ADPCMDecoding(int bitResolution)
		{
			ADPCMDecoder adpcmDecoder = new ADPCMDecoder();
			pcmFrames = adpcmDecoder.DecodeADPCMFrames(adpcmFrames);
			double samplingFrequency = 2 * frequency * Math.Pow(2, harmonics - 1);
			double maxAmplitude = signalList.GetSignalMaxAmplitude();
			PCMDecoder pcmDecoder = new PCMDecoder();

			return pcmDecoder.ConvertPCMToSignal(pcmFrames, maxAmplitude, bitResolution, samplingFrequency);
		}

		private void AddTo_PCMListView()
		{
			ObservableCollection<PCMListView_Item> observablePCMFrames = new ObservableCollection<PCMListView_Item>();
			foreach (var item in pcmFrames)
			{
				observablePCMFrames.Add(new PCMListView_Item(item.value));
			}
			ListView_PCM.ItemsSource = observablePCMFrames;
		}
		private void AddTo_ADPCMListView()
		{
			ObservableCollection<PCMListView_Item> observablePCMFrames = new ObservableCollection<PCMListView_Item>();
			foreach (var item in adpcmFrames)
			{
				observablePCMFrames.Add(new PCMListView_Item(item.value));
			}
			ListView_ADPCM.ItemsSource = observablePCMFrames;
		}

		private void ReDrawCanvas()
		{
			canvas_Left.Invalidate();
			canvas_Right_PCM.Invalidate();
			canvas_Right_ADPCM.Invalidate();
		}

		/*

		// ADPCM decoding
		ADPCMDecoder adpcmDecoder = new ADPCMDecoder();
		pcmFrames = adpcmDecoder.DecodeADPCMFrames(adpcmFrames);


		//PCM decoding
		double samplingFrequency = 2 * frequency * Math.Pow(2, harmonics - 1);
		double maxAmplitude = signals.GetSignalMaxAmplitude();
		PCMDecoder pcmDecoder = new PCMDecoder();

		LinkedList<Sample> samplesDecoded = pcmDecoder.ConvertPCMToSignal(pcmFrames, maxAmplitude, pcmBitResolution, 100);

		// Print decoded signalS

		foreach (var item in samplesDecoded)
		{
			Debug.WriteLine(item.value);
		}
		*/

		private void canvas_Left_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
		{
			int Width = (int)sender.ActualWidth;
			int Height = (int)sender.ActualHeight;

			args.DrawingSession.DrawLine(0, Height / 2, Width, Height / 2, Colors.Azure);
			if (signalList != null)
			{
				if (signalList.sampleList.Count != 0)
				{
					foreach (var item in signalList.sampleList)
					{
						args.DrawingSession.FillCircle((float)(item.time * 20), (float)((Height / 2) + (item.value * 10)), 2, Colors.White);
					}
				}
			}
		}

		private void canvas_Right_PCM_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
		{
			int Width = (int)sender.ActualWidth;
			int Height = (int)sender.ActualHeight;

			args.DrawingSession.DrawLine(0, Height / 2, Width, Height / 2, Colors.Azure);
			if (samplesList_Decoded_PCM != null)
			{
				if (samplesList_Decoded_PCM.Count != 0)
				{
					foreach (var item in samplesList_Decoded_PCM)
					{
						args.DrawingSession.FillCircle((float)(item.time * 20), (float)((Height / 2) + (item.value * 10)), 2, Colors.White);
					}
				}
			}
		}

		private void canvas_Right_ADPCM_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
		{
			int Width = (int)sender.ActualWidth;
			int Height = (int)sender.ActualHeight;

			args.DrawingSession.DrawLine(0, Height / 2, Width, Height / 2, Colors.Azure);
			if (samplesList_Decoded_ADPCM != null)
			{
				if (samplesList_Decoded_ADPCM.Count != 0)
				{
					foreach (var item in samplesList_Decoded_ADPCM)
					{
						args.DrawingSession.FillCircle((float)(item.time * 20), (float)((Height / 2) + (item.value * 10)), 2, Colors.White);
					}
				}
			}
		}

		private async void Button_ResetParameters_ClickAsync(object sender, RoutedEventArgs e)
		{
			harmonics = 1;
			duration = 20;
			frequency = 10;
			amplitude = 10;
			pcmBitResolution = 8;
			Slider_Harmonics.Value = harmonics;
			Slider_Duration.Value = duration;
			Slider_Frequency.Value = frequency;
			Slider_Amplitude.Value = amplitude;

			await Task.Run(() =>
			{
				generator = new SignalGenerator(harmonics);
				signalList = generator.GenerateSignal(duration, frequency, amplitude);
			});

			ReDrawCanvas();
			ProgressBar_GeneratingSignal.ShowPaused = true;
			ProgressBar_GeneratingSignal.Visibility = Visibility.Collapsed;

			ListView_PCM.ItemsSource = null;
			ListView_ADPCM.ItemsSource = null;
			pcmFrames = null;
			adpcmFrames = null;
			samplesList_Decoded_PCM = null;
			samplesList_Decoded_ADPCM = null;
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
			canvas_Left.RemoveFromVisualTree();
			canvas_Left = null;
			canvas_Right_PCM.RemoveFromVisualTree();
			canvas_Right_PCM = null;

			canvas_Right_ADPCM.RemoveFromVisualTree();
			canvas_Right_ADPCM = null;
		}
		/*
private void BtmStaticMusicStop_Click(object sender, RoutedEventArgs e)
{
	if (mediaPlayer != null)
	{
		if (mediaPlayer.Source != null)
		{
			mediaPlayer.Dispose();

			TextBlcok_Playback.Text = "";
			TextBlock_Info.Text = "";
		}
	}
}

private async void BtmRecode_ClickAsync(object sender, RoutedEventArgs e)
{
	mediaCapture = new MediaCapture();
	await mediaCapture.InitializeAsync();
	mediaCapture.Failed += MediaCapture_Failed;
	mediaCapture.RecordLimitationExceeded += MediaCapture_RecordLimitationExceeded;

	StorageFolder recodingFolder = await myMusic.CreateFolderAsync("Recoding", CreationCollisionOption.OpenIfExists);
	StorageFile file = await recodingFolder.CreateFileAsync("recoded_audio.mp3", CreationCollisionOption.GenerateUniqueName);
	TextBlock_Info.Text = file.Path;
	recodedFile = file;
	_mediaRecording = await mediaCapture.PrepareLowLagRecordToStorageFileAsync(MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High), file);
	ProgressBar_Recoding.ShowPaused = false;

	BtmRecode.Content = "Recoding...";
	await _mediaRecording.StartAsync();
}

private async void BtmPause_ClickAsync(object sender, RoutedEventArgs e)
{
	if (_mediaRecording != null)
	{
		if (IsRecodingPaused)
		{
			IsRecodingPaused = false;
			ProgressBar_Recoding.ShowPaused = false;
			BtmPause.Content = "Pause";

			await _mediaRecording.ResumeAsync();
		}
		else
		{
			IsRecodingPaused = true;
			ProgressBar_Recoding.ShowPaused = true;
			BtmPause.Content = "Resume";

			await _mediaRecording.PauseAsync(Windows.Media.Devices.MediaCapturePauseBehavior.ReleaseHardwareResources);
		}
	}
}

private async void BtmStop_ClickAsync(object sender, RoutedEventArgs e)
{
	if (_mediaRecording != null)
	{
		ProgressBar_Recoding.ShowPaused = true;
		BtmRecode.Content = "Recode";
		await _mediaRecording.StopAsync();
		await _mediaRecording.FinishAsync();
	}
}

private void MediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs args)
{
	ProgressBar_Recoding.ShowError = true;
	throw new Exception("Capture Failed!");
}

private async void MediaCapture_RecordLimitationExceeded(MediaCapture sender)
{
	ProgressBar_Recoding.ShowError = true;
	await _mediaRecording.StopAsync();
	System.Diagnostics.Debug.WriteLine("Record limitation exceeded.");
}

private async void BtmMusicPlay_ClickAsync(object sender, RoutedEventArgs e)
{
	mediaPlayer = new MediaPlayer();
	StorageFolder recodingFolder = await myMusic.GetFolderAsync("Recoding");
	StorageFile file = await recodingFolder.GetFileAsync("recoded_audio.mp3");

	if (file != null)
	{
		TextBlcok_Playback.Text = "NowPlaying: ";
		TextBlock_Info.Text = file.Path;
		mediaPlayer.Source = MediaSource.CreateFromStorageFile(file);

		mediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;
		mediaPlayer.Play();
	}

}

private void BtmMusicPause_ClickAsync(object sender, RoutedEventArgs e)
{
	if (mediaPlayer != null)
	{
		if (mediaPlayer.Source != null)
		{
			if (IsPlayingPaused)
			{
				IsPlayingPaused = false;
				BtmMusicPause.Content = "Pause";

				mediaPlayer.Play();
			}
			else
			{
				IsPlayingPaused = true;
				BtmMusicPause.Content = "Play";

				mediaPlayer.Pause();
			}

		}
	}
}

private void BtmMusicStop_Click(object sender, RoutedEventArgs e)
{
	if (mediaPlayer != null)
	{
		if (mediaPlayer.Source != null)
		{
			mediaPlayer.Dispose();
			TextBlcok_Playback.Text = "";
			TextBlock_Info.Text = "";
		}
	}
}

*/
	}
}