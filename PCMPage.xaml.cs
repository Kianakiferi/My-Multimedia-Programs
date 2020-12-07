using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MultimediaApp
{
	/// <summary>
	/// 可用于自身或导航至 Frame 内部的空白页。
	/// </summary>
	public sealed partial class PCMPage : Page
	{
		SignalGenerator generator;
		Signal signals;

		public PCMPage()
		{
			this.InitializeComponent();

		}

		private async void BtmStaticMusicPlay_ClickAsync(object sender, RoutedEventArgs e)
		{
			int harmonics = 1;

			double duration = 1;
			double frequency = 50;
			double amplitude = 1;

			generator = new SignalGenerator(harmonics);
			signals = generator.GenerateSignal(duration, frequency, amplitude);

			//PCM encoding
			Debug.WriteLine("PCM encoding");
			PCMEncoder pcmEncoder = new PCMEncoder();
			int pcmBitResolution = 8;
			LinkedList<PCMFrame> pcmFrames = pcmEncoder.GeneratePCMFramesFrom(signals, pcmBitResolution);

			// ADPCM encoding
			Debug.WriteLine("ADPCM encoding");
			ADPCMEncoder adpcmEncoder = new ADPCMEncoder();
			LinkedList<ADPCMFrame> adpcmFrames = adpcmEncoder.EncodeADPCMFrames(pcmFrames);

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
		}


		private void canvas_Left_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
		{
			int Width = (int)sender.ActualWidth;
			int Height = (int)sender.ActualHeight;

			args.DrawingSession.DrawLine(0, Height / 2, Width, Height / 2, Colors.Azure);
		}

		private void canvas_Right_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
		{

		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{

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
