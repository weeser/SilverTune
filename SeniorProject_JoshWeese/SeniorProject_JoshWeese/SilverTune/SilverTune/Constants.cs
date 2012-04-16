using System.Windows.Media;

namespace SilverTune
{
    /*Documentation
     * class:   Constants
     * Author:  Josh Weese
     * Purpose: Contains the constants used for various calculations and settings
     * Methods: 
     * Globals:     preferredtWaveFormat.........the preferred wave format of the audio sample, PCM is the only format allowed in this version of SL 
     *              preferredChannel.............the number of preferred channels of the audio sample...program currently only supports 1
     *              preferredBitsPerSample.......the number of bits per audio sample...program currently only supports 8
     *              preferredSamplesPerSecond....the number of samples taken per second, adjusting this will affect accuracy
     *              minFreq......................minimum allowed frequency
     *              maxFreq......................maximum allowed frequency
     *              peaksCount...................number of peaks in the FFT to detect
     *              fftLength....................length of the FFT...must be in powers of 2, adjusting this number will affect accuracy
     *              downsample...................the factor to scale down the audio sample by
     *              maxGraphPoints...............maximum number of points to plot on the graph
     *              sampleTimerDelay.............not currently in use
     *              sampleTimerPeriod............not currently in use
     *              concertAfreq.................the frequency of a conert A
     * Notes:  #All constants are static, read only
     *         #Adjusting numbers may affect accuracy and results, adjust with caution
     * ChangeLog:   Version 1.0...5/3/2011--Documented
    */
    public class Constants
    {
        public static readonly WaveFormatType preferredtWaveFormat = WaveFormatType.Pcm;
        public static readonly int preferredChannel = 1;
        public static readonly int preferredBitsPerSample = 8;
        //public static readonly int preferredBitsPerSample = 16;
        public static readonly int preferredSamplesPerSecond = 44100;
        public static readonly int minFreq = 60;
        public static readonly int maxFreq = 1300;
        public static readonly int peaksCount = 1;
        //public static readonly int fftLength = 4096;
        public static readonly int fftLength = 8192;
        //public static readonly int fftLength = 16384;
        public static readonly double downsample = 1.0;
        //public static readonly double downsample = 32768.0;
        public static readonly int maxGraphPoints = 4050;
        public static readonly int sampleTimerDelay = 0;
        public static readonly int sampleTimerPeriod = 2000;
        public static readonly double concertAfreq = 440.0;
    }
}
