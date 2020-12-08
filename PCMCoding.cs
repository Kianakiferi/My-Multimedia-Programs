using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MultimediaApp
{
	//单个信号样本
	public class Sample
	{
		public double time;
		public double value;

		public Sample(double time, double value)
		{
			this.time = time;
			this.value = value;
		}
	}

	//信号List
	public class Signal
	{
		public LinkedList<Sample> sampleList;

		public Signal(double duration, double samplingFrequency)
		{
			this.sampleList = new LinkedList<Sample>();

			double time = 0;
			while (time < duration)
			{
				sampleList.AddLast(new Sample(time, 0));
				time += (1 / samplingFrequency);
			}
		}

		public double GetSignalMaxAmplitude()
		{
			double maxAmplitude = 0;
			double value;
			Sample sample;

			//遍历查找最大
			foreach (var item in sampleList)
			{
				sample = item;
				value = Math.Abs(sample.value);

				if (value > maxAmplitude)
				{
					maxAmplitude = value;
				}
			}
			return maxAmplitude;
		}
	}

	//信号产生方法封装类
	public class SignalGenerator
	{
		private SignalFunction signalFunction;
		private int harmonics;

		public class SignalFunction
		{
			// function parameters
			private double amplitude;
			private double frequency;

			public double Amplitude { get => amplitude; set => amplitude = value; }
			public double Frequency { get => frequency; set => frequency = value; }

			// function setup
			public void SetUpFunctionParameters(double amplitude, double frequency)
			{
				this.Amplitude = amplitude;
				this.Frequency = frequency;
			}

			// calculate value for specified time
			public double CalculateSignalValueFor(double time)
			{
				return (Amplitude * Math.Sin((Math.PI / 180) * (2 * Math.PI * time * Frequency)));
			}
		}

		public SignalGenerator()
		{

		}

		public SignalGenerator(int harmonics)
		{
			this.signalFunction = new SignalFunction();
			this.harmonics = harmonics;
		}

		public Signal GenerateSignal(double duration, double frequency, double amplitude)
		{
			// nyquist frequency
			double samplingFrequency = 2 * frequency * Math.Pow(2, harmonics - 1);
			Signal signal = new Signal(duration, samplingFrequency);

			double time;
			double value;
			double accumulatedValue;

			int index;
			for (int n = 1; n <= harmonics; n++)
			{
				signalFunction.SetUpFunctionParameters(amplitude, frequency);
				time = 0;
				index = 0;

				while (time < duration)
				{
					value = signalFunction.CalculateSignalValueFor(time);
					accumulatedValue = signal.sampleList.ElementAt(index).value;
					signal.sampleList.ElementAt(index).value = value + accumulatedValue;
					time += 1 / samplingFrequency;
					index += 1;
				}

				frequency *= 2;
			}
			return signal;
		}
	}

	//B树
	public class BinaryString
	{
		public static int CalculateIntegerFrom(string binaryString)
		{
			bool isBinaryString = Regex.IsMatch(binaryString, "\\b[01]+\\b");
			int intValue = 0;
			int valueSign = 1;
			int lastIndex = binaryString.Length - 1;
			string charAt;

			if (isBinaryString && binaryString.Length > 1)
			{
				// integer sign
				charAt = GetCharAt(binaryString, lastIndex);
				if (charAt.Equals("1"))
				{
					valueSign = -1;
				}

				// integer value
				for (int i = lastIndex - 1; i >= 0; i--)
				{
					charAt = GetCharAt(binaryString, i);

					if (charAt.Equals("1"))
					{
						intValue += (int)Math.Pow(2, i);
					}
				}
			}

			return intValue * valueSign;
		}

		public static string CalculateBinaryStringFrom(int value, int bits)
		{
			string sign;
			if (value < 0)
			{
				sign = "1";
			}
			else
			{
				sign = "0";
			}

			string binaryString = Convert.ToString(Math.Abs(value), 2);
			binaryString = FillWith("0", binaryString, bits - 1);

			return sign + binaryString;
		}

		public static string Add(string binaryString1, string binaryString2, int bits)
		{
			int value1 = CalculateIntegerFrom(binaryString1);
			int value2 = CalculateIntegerFrom(binaryString2);

			int result = value1 + value2;

			// max value check
			int maxValue = MaxValueFor(bits);

			if (Math.Abs(result) > maxValue)
			{
				if (result < 0)
				{
					result = -1 * maxValue;
				}
				else
				{
					result = maxValue;
				}
			}

			return CalculateBinaryStringFrom(result, bits);
		}

		public static string FillWith(string bit, string binaryString, int requiredLength)
		{
			int actualLength = binaryString.Length;

			if (actualLength < requiredLength)
			{
				for (int i = 0; i < requiredLength - actualLength; i++)
				{
					binaryString = bit + binaryString;
				}
			}

			return binaryString;
		}

		public static int MaxValueFor(int bits)
		{
			string binaryString = "";
			binaryString = FillWith("1", binaryString, bits - 1);
			binaryString = "0" + binaryString;

			return BinaryString.CalculateIntegerFrom(binaryString);
		}

		public static string Negative(string binaryString)
		{
			string result = binaryString.Substring(1);

			if (binaryString.Substring(0, 1).Equals("1"))
			{
				result = "0" + result;
			}
			else
			{
				result = "1" + result;
			}

			return result;
		}

		//获取index处的字符，76543210，倒序
		public static string GetCharAt(string binaryString, int index)
		{
			//return 	binaryString.Substring((binaryString.Length - 1 - index), binaryString.Length - index);
			return binaryString.Substring((binaryString.Length - 1 - index), 1);
		}

		//无用
		public static string XOR(string binaryString1, string binaryString2)
		{
			string result = "";

			char char1;
			char char2;

			for (int i = binaryString1.Length - 1; i >= 0; i--)
			{
				char1 = binaryString1.ElementAt(i);
				char2 = binaryString2.ElementAt(i);

				if (char1 == '1' && char2 == '1')
				{
					result = "0" + result;
				}
				else if (char1 == '0' && char2 == '0')
				{
					result = "0" + result;
				}
				else
				{
					result = "1" + result;
				}
			}
			return result;
		}

		public static string AND(string binaryString1, string binaryString2)
		{
			string result = "";

			char char1;
			char char2;

			for (int i = binaryString1.Length - 1; i >= 0; i--)
			{
				char1 = binaryString1.ElementAt(i);
				char2 = binaryString2.ElementAt(i);

				if (char1 == '1' && char2 == '1')
				{
					result = "1" + result;
				}
				else
				{
					result = "0" + result;
				}
			}

			return result;
		}

		public static string OR(string binaryString1, string binaryString2)
		{
			string result = "";

			char char1;
			char char2;

			for (int i = binaryString1.Length - 1; i >= 0; i--)
			{
				char1 = binaryString1.ElementAt(i);
				char2 = binaryString2.ElementAt(i);

				if (char1 == '0' && char2 == '0')
				{
					result = "0" + result;
				}
				else
				{
					result = "1" + result;
				}
			}
			return result;
		}
	}

	public class PCMFrame
	{
		public string value;

		public PCMFrame()
		{

		}

		public PCMFrame(string value)
		{
			this.value = value;
		}
	}

	public class PCMEncoder
	{
		int bitResolution;

		public LinkedList<PCMFrame> GeneratePCMFramesFrom(Signal signal, int bitResolution)
		{
			LinkedList<PCMFrame> pcmFrames = new LinkedList<PCMFrame>();

			// PCM 参数们
			this.bitResolution = bitResolution;
			double maxAmplitude = GetMaxAmplitudeFrom(signal);
			int quantizationLevels = (int)Math.Pow(2, bitResolution) - 1;
			double quantizationStep = (double)(maxAmplitude / (0.5 * quantizationLevels - 1));

			Sample sample;
			string pcmBits;

			foreach (var item in signal.sampleList)
			{
				sample = item;
				pcmBits = CodeSample(sample, quantizationStep);
				pcmFrames.AddLast(new PCMFrame(pcmBits));
			}

			return pcmFrames;
		}

		public string CodeSample(Sample sample, double quantizationStep)
		{
			double value = sample.value;

			int level = (int)Math.Round((Math.Abs(value) / quantizationStep));

			string code = Convert.ToString(level, 2);
			int lengthOfCode = code.Length;

			//补充位数（填充0）
			for (int i = 0; i < bitResolution - lengthOfCode; i++)
			{
				code = "0" + code;
			}

			//Add sign
			string sign;

			if (value >= 0)
			{
				sign = "0";
			}
			else
			{
				sign = "1";
			}

			code = sign + code;

			return code;
		}

		public double GetMaxAmplitudeFrom(Signal signal)
		{
			double maxAmplitude = 0;
			double value;
			Sample sample;

			foreach (var item in signal.sampleList)
			{
				sample = item;
				value = Math.Abs(sample.value);

				if (value > maxAmplitude)
				{
					maxAmplitude = value;
				}
			}

			return maxAmplitude;
		}
	}

	public class PCMDecoder
	{

		public LinkedList<Sample> ConvertPCMToSignal(LinkedList<PCMFrame> pcmFrames, double maxAmplitude, int bitResolution, double samplingFrequency)
		{
			LinkedList<Sample> samples = new LinkedList<Sample>();

			int quantizationLevels = (int)Math.Pow(2, bitResolution) - 1;
			double quantizationStep = (double)(maxAmplitude / (0.5 * quantizationLevels - 1));

			double time = 0;
			double sampleValue;

			foreach (var item in pcmFrames)
			{
				sampleValue = DecodePCMFrame(item, quantizationStep);
				samples.AddLast(new Sample(time, sampleValue));
				time += 1 / samplingFrequency;
			}
			return samples;
		}

		public double DecodePCMFrame(PCMFrame pcmFrame, double quantizationStep)
		{
			string binaryString = pcmFrame.value;
			int value = BinaryString.CalculateIntegerFrom(binaryString);

			double sampleValue = (double)(value * quantizationStep);

			return sampleValue;
		}
	}

	public class StepSizeBuffer
	{
		public int bufferedValue;
	}

	public class StepSizeValues
	{
		private static readonly int[] stepSizeValues = {
		   16, 17, 19, 21, 23, 25,
			28, 31, 34, 37, 41, 45,
			50, 55, 60, 66, 73, 80,
			88, 97, 107, 118, 130, 143,
			157, 173, 190, 209, 230, 253,
			279, 307, 337, 371, 408
		};

		public static int GetStepSizeValueFrom(int index)
		{

			int maxIndex = stepSizeValues.Length - 1;

			if (index > maxIndex)
			{
				return stepSizeValues[maxIndex];
			}
			else if (index < 0)
			{
				return stepSizeValues[0];
			}
			else
			{
				return stepSizeValues[index];
			}
		}

		public static int CalcuateStepSizeValueFor(ADPCMFrame adpcmFrame, int previousStepSize)
		{
			String adpcmValue = adpcmFrame.value;
			int index;
			int indexChange = 0;

			if (adpcmValue.Equals("1111") || adpcmValue.Equals("0111")) { indexChange = 8; }
			else if (adpcmValue.Equals("1110") || adpcmValue.Equals("0110")) { indexChange = 4; }
			else if (adpcmValue.Equals("1101") || adpcmValue.Equals("0101")) { indexChange = 2; }
			else if (adpcmValue.Equals("1100") || adpcmValue.Equals("0100")) { indexChange = 1; }
			else if (adpcmValue.Equals("1011") || adpcmValue.Equals("0011")) { indexChange = -1; }
			else if (adpcmValue.Equals("1010") || adpcmValue.Equals("0010")) { indexChange = -1; }
			else if (adpcmValue.Equals("1001") || adpcmValue.Equals("0001")) { indexChange = -1; }
			else if (adpcmValue.Equals("1000") || adpcmValue.Equals("0000")) { indexChange = -1; };

			index = GetIndexFromStepSizeTable(previousStepSize) + indexChange;

			return GetStepSizeValueFrom(index);
		}

		public static int GetIndexFromStepSizeTable(int stepSize)
		{
			int index = 0;

			for (int searchedIndex = 0; searchedIndex < stepSizeValues.Length - 1; searchedIndex++)
			{

				if (stepSizeValues[searchedIndex] == stepSize)
				{
					index = searchedIndex;
				}
			}

			return index;
		}
	}

	public class PCMBuffer
	{
		public string bufferedValue;
	}

	public class ADPCMFrame
	{
		public string value;

		public ADPCMFrame(string value)
		{
			this.value = value;
		}
	}

	public class ADPCMEncoder
	{
		public StepSizeBuffer stepSizeBuffer = new StepSizeBuffer();
		public PCMBuffer pcmBuffer = new PCMBuffer();
		public ADPCMDecoder decoder = new ADPCMDecoder();

		public ADPCMFrame EncodeADPCMFrame(string pcmDifference, int intStepSize)
		{

			ADPCMFrame adpcmFrame;
			string adpcmValue = "";
			double pcmDifferenceValue = BinaryString.CalculateIntegerFrom(pcmDifference);
			double stepSize = intStepSize;

			if (pcmDifferenceValue < 0)
			{
				adpcmValue += "1";
			}
			else
			{
				adpcmValue += "0";
			}

			pcmDifferenceValue = Math.Abs(pcmDifferenceValue);

			if (pcmDifferenceValue >= stepSize)
			{
				adpcmValue += "1";
				pcmDifferenceValue -= stepSize;
			}
			else
			{
				adpcmValue += "0";
			}

			if (pcmDifferenceValue >= stepSize / 2.0)
			{
				adpcmValue += "1";
				pcmDifferenceValue -= stepSize / 2.0;
			}
			else
			{
				adpcmValue += "0";
			}

			if (pcmDifferenceValue >= stepSize / 4.0)
			{
				adpcmValue += "1";
				pcmDifferenceValue -= stepSize / 4.0;
			}
			else
			{
				adpcmValue += "0";
			}

			adpcmFrame = new ADPCMFrame(adpcmValue);

			return adpcmFrame;
		}

		public LinkedList<ADPCMFrame> EncodeADPCMFrames(LinkedList<PCMFrame> pcmFrames)
		{
			//Data
			LinkedList<ADPCMFrame> adpcmFrames = new LinkedList<ADPCMFrame>();

			//INIT VALUES
			stepSizeBuffer.bufferedValue = 16;
			pcmBuffer.bufferedValue = "00000000";

			decoder.stepSizeBuffer.bufferedValue = 16;
			decoder.pcmBuffer.bufferedValue = "00000000";

			int stepSize;
			int nextStepSize;
			string pcmDifference;
			PCMFrame previousPcmFrame = new PCMFrame();
			ADPCMFrame adpcmFrame = new ADPCMFrame("0000");

			// ENCODE FRAMES
			foreach (var item in pcmFrames)
			{
				// X(N)

				// X(N-1)
				previousPcmFrame.value = pcmBuffer.bufferedValue;

				// D(N) = X(N) - X(N-1)
				pcmDifference = BinaryString.Add(item.value, BinaryString.Negative(previousPcmFrame.value), 8);

				// SS(N)
				stepSize = stepSizeBuffer.bufferedValue;
				nextStepSize = StepSizeValues.CalcuateStepSizeValueFor(adpcmFrame, stepSize);
				stepSizeBuffer.bufferedValue = nextStepSize;

				adpcmFrame = EncodeADPCMFrame(pcmDifference, stepSize);

				string pcm = decoder.DecodeADPCMFrame(adpcmFrame).value;
				pcmBuffer.bufferedValue = pcm;
				adpcmFrames.AddLast(adpcmFrame);
			}

			return adpcmFrames;

		}
	}

	public class ADPCMDecoder
	{

		public StepSizeBuffer stepSizeBuffer = new StepSizeBuffer();
		public PCMBuffer pcmBuffer = new PCMBuffer();

		public LinkedList<PCMFrame> DecodeADPCMFrames(LinkedList<ADPCMFrame> adpcmFrames)
		{

			// DATA
			LinkedList<PCMFrame> pcmFrames = new LinkedList<PCMFrame>();

			// INIT VALUES
			stepSizeBuffer.bufferedValue = 16;
			pcmBuffer.bufferedValue = "00000000";

			int stepSize;
			int nextStepSize;
			string difference;
			PCMFrame pcmFrame = new PCMFrame();
			PCMFrame previousPcmFrame = new PCMFrame();
			string result;

			//Iterator<ADPCMFrame> adpcmIterator = adpcmFrames.iterator();

			// DECODE FRAMES
			foreach (var item in adpcmFrames)
			{
				// L(N)

				// BUFF => SS(N) | SS(N+1) => BUFF 
				stepSize = stepSizeBuffer.bufferedValue;
				nextStepSize = StepSizeValues.CalcuateStepSizeValueFor(item, stepSize);
				stepSizeBuffer.bufferedValue = nextStepSize;

				// D(N)
				difference = CalculateDifference(item, stepSize);

				// BUFF => X(N-1) | X(N) => BUFF
				previousPcmFrame.value = pcmBuffer.bufferedValue;
				result = BinaryString.Add(previousPcmFrame.value, difference, 8);
				pcmBuffer.bufferedValue = result;
				pcmFrame = new PCMFrame(result);
				pcmFrames.AddLast(pcmFrame);
			}

			return pcmFrames;
		}

		public PCMFrame DecodeADPCMFrame(ADPCMFrame adpcmFrame)
		{
			int stepSize;
			int nextStepSize;
			string difference;
			PCMFrame pcmFrame = new PCMFrame();
			PCMFrame previousPcmFrame = new PCMFrame();

			string result;

			// DECODE FRAME

			// BUFF => SS(N) | SS(N+1) => BUFF 
			stepSize = stepSizeBuffer.bufferedValue;
			nextStepSize = StepSizeValues.CalcuateStepSizeValueFor(adpcmFrame, stepSize);
			stepSizeBuffer.bufferedValue = nextStepSize;

			// D(N)
			difference = CalculateDifference(adpcmFrame, stepSize);

			// BUFF => X(N-1) | X(N) => BUFF
			previousPcmFrame.value = pcmBuffer.bufferedValue;
			result = BinaryString.Add(previousPcmFrame.value, difference, 8);
			pcmBuffer.bufferedValue = result;
			pcmFrame = new PCMFrame(result);

			return pcmFrame;

		}

		public string CalculateDifference(ADPCMFrame adpcmFrame, int stepSize)
		{

			string result;

			double difference = 0.0;

			string bit;

			bit = BinaryString.GetCharAt(adpcmFrame.value, 2);
			if (bit.Equals("1"))
			{
				difference += stepSize;
			}

			bit = BinaryString.GetCharAt(adpcmFrame.value, 1);
			if (bit.Equals("1"))
			{
				difference += stepSize / 2.0;
			}

			bit = BinaryString.GetCharAt(adpcmFrame.value, 0);
			if (bit.Equals("1"))
			{
				difference += stepSize / 4.0;
			}

			difference += stepSize / 8.0;

			bit = BinaryString.GetCharAt(adpcmFrame.value, 3);

			if (bit.Equals("1"))
			{
				difference *= -1;
			}

			result = BinaryString.CalculateBinaryStringFrom((int)difference, 8);

			return result;
		}
	}
}

