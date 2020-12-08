# My-Multimedia-Programs
 Huffman, PCM, ADPCM
### Huffman Coding
- Encode from .txt file
- Encode keyboard input
- Showing codes dictionary
- Decode from .txt file
- Decode keyboard input
- Get Dictionary from ClipBoard
### Usage

``` C#
HuffmanCoding huffman = new HuffmanCoding();
Dictionary<char, string> keyAndValues = new Dictionary<char, string>();
    
string huffmanCoded = huffman.StringToHuffmanCode(out keyAndValues, input);
```
AAAAABDBBCCC => 00000101101010111111111

Dictionary:

A 0

B 10

D 110

C 111

```C#
//Split Text
string textHuffmanCode = input.Substring(0, Dictionary_Index - 1);
string textDictionary = input.Substring(Dictionary_Index + 12);

HuffmanCoding huffman = new HuffmanCoding();
Dictionary<char, string> keyAndValues = huffman.CreateNewDictionary(textDictionary);
string stringDecoded = huffman.HuffmanCodeToString(keyAndValues, textHuffmanCode);

```
00000101101010111111111

Dictionary:

A 0

B 10

D 110

C 111

=> AAAAABDBBCCC

###PCM, ADPCM Coding
- Generate signals with Trigonometric function
- PCM Encode
- ADPCM Encode
- PCM Decode
- ADPCM Decode

### Usage
``` C#
int harmonics = 1;
double duration = 10;
double frequency = 50;
double amplitude = 10;
int pcmBitResolution = 8;

SignalGenerator generator = new SignalGenerator(harmonics);
Signal signalList = generator.GenerateSignal(duration, frequency, amplitude);

//Encode
PCMEncoder pcmEncoder = new PCMEncoder();
LinkedList<PCMFrame> pcmFrames = pcmEncoder.GeneratePCMFramesFrom(signalList, bitResolution);

ADPCMEncoder adpcmEncoder = new ADPCMEncoder();
LinkedList<ADPCMFrame> adpcmFrames = adpcmEncoder.EncodeADPCMFrames(pcmFrames);

//Decode
double samplingFrequency = 2 * frequency * Math.Pow(2, harmonics - 1);
double maxAmplitude = signalList.GetSignalMaxAmplitude();

PCMDecoder pcmDecoder = new PCMDecoder();
LinkedList<Sample> samplesList_Decoded_PCM = pcmDecoder.ConvertPCMToSignal(pcmFrames, maxAmplitude, bitResolution, samplingFrequency);

LinkedList<Sample> samplesList_Decoded_ADPCM = pcmDecoder.ConvertPCMToSignal(pcmFrames, maxAmplitude, bitResolution, samplingFrequency);
```
Signals => pcmFrames
