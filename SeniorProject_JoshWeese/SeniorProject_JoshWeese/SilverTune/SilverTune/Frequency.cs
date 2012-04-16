using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace SilverTune
{
    /*Documentation
     * Class:   Frequency
     * Author:  Josh Weese
     * Purpose: Contains the means and methods for determining the fundamental frequency
     * Methods: calcFundamentalFreq...Calculates fundamental frequency
     *          findPeaks...Detects the maximum peaks of the spectrogram
     *          calculateCents...Calculates the number of cents that the note (freqeuncy) is out of tune
     *          calculateCents...Calculates the number of cents that the note (freqeuncy) is out of tune
     *          getPitch...Searches pitch list for closest note
     *          loadPitches...Calculates all pitches (sharps and fundamental) from concert pitch
     * Globals:   
     * Notes:  
     * ChangeLog:   Version 1.0...5/3/2011--Documented
    */
    public class Frequency
    {
		/*Documentation
		 * Method:  calcFundamentalFreq
		 * Author:  Josh Weese and http://www.codeproject.com/KB/audio-video/FftGuitarTuner.aspx
		 * Purpose: Calculates fundamental frequency
		 * Parameters:  #ComplexNumber[] fftData...complex number array, the result from the fft
		 *              #int samplesPerSec...sample rate
		 * Returns: the fundamental frequency as a double...this is in Hz
		 * Notes:  #detects peaks in fft data
		 *         #determines exact bin the fundamental lies in
		 *         #returns fundamental frequency
		 * ChangeLog:   Version 1.0...5/3/2011--Documented
		 */
		public double calcFundamentalFreq(ComplexNumber[] fftData, int samplesPerSec)
        {
            //array to store fft data
            double[] spectrogram = new double[fftData.Length];

            for (int i = 0; i < spectrogram.Length; i++)
            {
                //the squared magnitude represents the spectogram (strength of the audio signal)
                spectrogram[i] = fftData[i].SquaredMagnitude;
            }

            int usefullMinSpectrogram = Math.Max(0, (Constants.minFreq * spectrogram.Length / samplesPerSec));
            int usefullMaxSpectrogram = Math.Min(spectrogram.Length, (Constants.maxFreq * fftData.Length / samplesPerSec) + 1);

            int[] peakIndices = findPeaks(spectrogram, usefullMinSpectrogram, usefullMaxSpectrogram - usefullMinSpectrogram, Constants.peaksCount);

            if (Array.IndexOf(peakIndices, usefullMinSpectrogram) >= 0)
            {
                // lowest usefull frequency bin shows active
                // looks like is no detectable sound, return 0
                return 0;
            }

            //Quinn's interpolation to guess exact bin of the fundamental frequency
            double maximizerRatio1 = 0;
            double maximizerRatio2 = 0;
            double quinnEstimator = 0;
            double quinnEstimator1 = 0;
            double quinnEstimator2 = 0;

            maximizerRatio1 = fftData[peakIndices[0] - 1].Magnitude / fftData[peakIndices[0]].Magnitude;
            maximizerRatio2 = fftData[peakIndices[0] + 1].Magnitude / fftData[peakIndices[0]].Magnitude;
            quinnEstimator1 = maximizerRatio1 / (1 - maximizerRatio1);
            quinnEstimator2 = -maximizerRatio2 / (1 - maximizerRatio2);

            if (quinnEstimator1 > 0 && quinnEstimator2 > 0)
            {
                quinnEstimator = quinnEstimator1;
            }
            else
            {
                quinnEstimator = quinnEstimator2;
            }

            //return the fundamental frequency in hz
            return ((quinnEstimator + peakIndices[0]) / (double)fftData.Length * samplesPerSec);
        }

		/*Documentation
		 * Method:  findPeaks
		 * Author:  Josh Weese and http://www.codeproject.com/KB/audio-video/FftGuitarTuner.aspx
		 * Purpose: Detects the maximum peaks of the spectrogram
		 * Parameters:  #double[] spectrogram...squaredmagnitude of the fft data
		 *              #int index...starting index
		 *              #int length...length of the spectrogram to check
		 *              #int peaksCount...number of peaks to detect
		 * Returns: peakIndices
		 * Notes:  #
		 * ChangeLog:   Version 1.0...5/3/2011--Documented
		 */
		private int[] findPeaks(double[] spectrogram, int index, int length, int peaksCount)
        {
            double[] peakValues = new double[peaksCount];
            int[] peakIndices = new int[peaksCount];

            for (int i = 0; i < peaksCount; i++)
            {
                peakValues[i] = spectrogram[peakIndices[i] = i + index];
            }

            // find min peaked value
            double minStoredPeak = peakValues[0];
            int minIndex = 0;
            for (int i = 1; i < peaksCount; i++)
            {
                if (minStoredPeak > peakValues[i]) minStoredPeak = peakValues[minIndex = i];
            }

            for (int i = peaksCount; i < length; i++)
            {
                if (minStoredPeak < spectrogram[i + index])
                {
                    // replace the min peaked value with bigger one
                    peakValues[minIndex] = spectrogram[peakIndices[minIndex] = i + index];

                    // and find min peaked value again
                    minStoredPeak = peakValues[minIndex = 0];
                    for (int j = 1; j < peaksCount; j++)
                    {
                        if (minStoredPeak > peakValues[j]) minStoredPeak = peakValues[minIndex = j];
                    }
                }
            }

            return peakIndices;
        }

		/*Documentation
		* Method:  calculateCents
		* Author:  Josh Weese
		* Purpose: Calculates the number of cents that the note (freqeuncy) is out of tune
		* Parameters:  #double fundFreq...the frequency of the note
		* Returns: int cents
		* Notes:  #This method only calcualtes the cents based on a concert A
		* ChangeLog:   Version 1.0...5/3/2011--Documented
		*/
		public int calculateCents(double fundFreq)
        {
            int cents;
            double nFreq = 440;

            cents = (int)(1200 * (Math.Log(fundFreq / nFreq, 2)));

            return cents;
        }

		/*Documentation
		 * Method:  calculateCents
		 * Author:  Josh Weese
		 * Purpose: Calculates the number of cents that the note (freqeuncy) is out of tune
		 * Parameters:  #double fundFreq...the frequency of the note
		 *              #double nFreq...the frequency the not is supposed to be
		 * Returns: int cents
		 * Notes:  
		 * ChangeLog:   Version 1.0...5/3/2011--Documented
		 */
		public int calculateCents(double fundFreq, double nFreq)
        {
            int cents;

            cents = (int)(1200 * (Math.Log(fundFreq / nFreq, 2)));

            return cents;
        }

		/*Documentation
		 * Method:  getPitch
		 * Author:  Josh Weese
		 * Purpose: Searches pitch list for closest note
		 * Parameters:  #double frequency...the frequency of the note
		 *              #List<Pitch> pitchList...the list of known pitches
		 * Returns: Pitch
		 * Notes:  
		 * ChangeLog:   Version 1.0...5/3/2011--Documented
		 */
		public Pitch getPitch(double frequency, List<Pitch> pitchList)
        {
            //LINQ query
            return (from f in pitchList
                         orderby Math.Abs(f.fundamentalFreq - frequency)
                         select f).First();
        }

		/*Documentation
		 * Method:  loadPitches
		 * Author:  Josh Weese
		 * Purpose: Calculates all pitches (sharps and fundamental) from concert pitch
		 * Parameters:  #double concertAfreq...concert pitch to base notes from
		 *              #List<Pitch> pitchList...pitch list to store in
		 * Returns: List<Pitch> pitchList
		 * Notes:  #B and E have no sharp note
		 * ChangeLog:   Version 1.0...5/3/2011--Documented
		 */
		public List<Pitch> loadPitches(double concertAfreq, List<Pitch> pitchList)
        {
            PitchConstants pConstants = new PitchConstants();
            pitchList = new List<Pitch>();
            int x = 3;
            int z = 0;
            bool y = true;
            for (int i = -33; i < 27; i++)
            {
                Pitch pitch = new Pitch();
                //frequency
                pitch.fundamentalFreq = concertAfreq * (Math.Pow(2.0, (i / 12.0)));
                //accidentals
                if (y)
                {
                    switch (z)
                    {
                        case 0:
                            pitch.accidental = pConstants.natural;
                            z++;
                            break;
                        case 1:
                            pitch.accidental = pConstants.sharp;
                            z = 0;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    pitch.accidental = pConstants.natural;
                }
                //note
                switch (x)
                {
                    case 1:
                        pitch.note = pConstants.A;
                        y = true;
                        break;
                    case 2:
                        pitch.note = pConstants.B;
                        y = false;
                        break;
                    case 3:
                        pitch.note = pConstants.C;
                        y = true;
                        break;
                    case 4:
                        pitch.note = pConstants.D;
                        y = true;
                        break;
                    case 5:
                        pitch.note = pConstants.E;
                        y = false;
                        break;
                    case 6:
                        pitch.note = pConstants.F;
                        y = true;
                        break;
                    case 7:
                        pitch.note = pConstants.G;
                        y = true;
                        break;
                    default:
                        x = 1;
                        break;
                }
                if (pitch.accidental == pConstants.sharp || !y)
                {
                    x++;
                    if (x == 8)
                    {
                        x = 1;
                    }
                }
                pitchList.Add(pitch);
            }
            return pitchList;
        }

    }//end Frequency Class

    /*Documentation
     * Class:   FundamentalFrequency
     * Author:  Josh Weese
     * Purpose: Variables for fundamental frequency
     * Methods: OnPropertyChanged...event handler which updates binded properties
     * Variables:   #double fundamentalFreq..frequency
     *              #int cents...cents off pitch
     *              #string note...note the frequency is
     *              #string accidental...sharp or natural
     * Notes:  
     * ChangeLog:   Version 1.0...5/3/2011--Documented
    */
    public class FundamentalFrequency : INotifyPropertyChanged
    {
        // Declare the PropertyChanged event
        public event PropertyChangedEventHandler PropertyChanged;
        private double _fundamentalFreq;
        public double fundamentalFreq
        {
            get
            {
                return _fundamentalFreq;
            }
            set
            {
                _fundamentalFreq = Math.Round(value, 2);
                OnPropertyChanged("fundamentalFreq");
            }
        }

        private int _cents;
        public int cents
        {
            get
            {
                return _cents;
            }
            set
            {
                _cents = value;
                OnPropertyChanged("cents");
            }
        }

        private string _note;
        public string note
        {
            get
            {
                return _note;
            }
            set
            {
                _note = value;
                OnPropertyChanged("note");
            }
        }

        private string _accidental;
        public string accidental
        {
            get
            {
                return _accidental;
            }
            set
            {
                _accidental = value;
                OnPropertyChanged("accidental");
            }
        }

        // OnPropertyChanged will raise the PropertyChanged event passing the
        // source property that is being updated.
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        } 
    }

    public class FourierTransform
    {
        // AForge Math Library
        // AForge.NET framework
        // http://www.aforgenet.com/framework/
        //
        // Copyright © Andrew Kirillov, 2005-2009
        // andrew.kirillov@aforgenet.com
        //
        // FFT idea from Exocortex.DSP library
        // http://www.exocortex.org/dsp/
        //
        /// <summary>
        /// Fourier transformation direction.
        /// </summary>
        public enum Direction
        {
            /// <summary>
            /// Forward direction of Fourier transformation.
            /// </summary>
            Forward = 1,

            /// <summary>
            /// Backward direction of Fourier transformation.
            /// </summary>
            Backward = -1
        };


        
        /// <summary>
        /// One dimensional Discrete Fourier Transform.
        /// </summary>
        /// 
        /// <param name="data">Data to transform.</param>
        /// <param name="direction">Transformation direction.</param>
        /// 
        public void DFT(ComplexNumber[] data, Direction direction)
        {
            int n = data.Length;
            double arg, cos, sin;
            ComplexNumber[] dst = new ComplexNumber[n];

            // for each destination element
            for (int i = 0; i < n; i++)
            {
                dst[i] = ComplexNumberConstants.Zero;

                arg = -(int)direction * 2.0 * System.Math.PI * (double)i / (double)n;

                // sum source elements
                for (int j = 0; j < n; j++)
                {
                    cos = System.Math.Cos(j * arg);
                    sin = System.Math.Sin(j * arg);

                    dst[i].Re += (data[j].Re * cos - data[j].Im * sin);
                    dst[i].Im += (data[j].Re * sin + data[j].Im * cos);
                }
            }

            // copy elements
            if (direction == Direction.Forward)
            {
                // devide also for forward transform
                for (int i = 0; i < n; i++)
                {
                    data[i].Re = dst[i].Re / n;
                    data[i].Im = dst[i].Im / n;
                }
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    data[i].Re = dst[i].Re;
                    data[i].Im = dst[i].Im;
                }
            }
        }

        /// <summary>
        /// Two dimensional Discrete Fourier Transform.
        /// </summary>
        /// 
        /// <param name="data">Data to transform.</param>
        /// <param name="direction">Transformation direction.</param>
        /// 
        public void DFT2(ComplexNumber[,] data, Direction direction)
        {
            int n = data.GetLength(0);	// rows
            int m = data.GetLength(1);	// columns
            double arg, cos, sin;
            ComplexNumber[] dst = new ComplexNumber[System.Math.Max(n, m)];

            // process rows
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    dst[j] = ComplexNumberConstants.Zero;

                    arg = -(int)direction * 2.0 * System.Math.PI * (double)j / (double)m;

                    // sum source elements
                    for (int k = 0; k < m; k++)
                    {
                        cos = System.Math.Cos(k * arg);
                        sin = System.Math.Sin(k * arg);

                        dst[j].Re += (data[i, k].Re * cos - data[i, k].Im * sin);
                        dst[j].Im += (data[i, k].Re * sin + data[i, k].Im * cos);
                    }
                }

                // copy elements
                if (direction == Direction.Forward)
                {
                    // devide also for forward transform
                    for (int j = 0; j < m; j++)
                    {
                        data[i, j].Re = dst[j].Re / m;
                        data[i, j].Im = dst[j].Im / m;
                    }
                }
                else
                {
                    for (int j = 0; j < m; j++)
                    {
                        data[i, j].Re = dst[j].Re;
                        data[i, j].Im = dst[j].Im;
                    }
                }
            }

            // process columns
            for (int j = 0; j < m; j++)
            {
                for (int i = 0; i < n; i++)
                {
                    dst[i] = ComplexNumberConstants.Zero;

                    arg = -(int)direction * 2.0 * System.Math.PI * (double)i / (double)n;

                    // sum source elements
                    for (int k = 0; k < n; k++)
                    {
                        cos = System.Math.Cos(k * arg);
                        sin = System.Math.Sin(k * arg);

                        dst[i].Re += (data[k, j].Re * cos - data[k, j].Im * sin);
                        dst[i].Im += (data[k, j].Re * sin + data[k, j].Im * cos);
                    }
                }

                // copy elements
                if (direction == Direction.Forward)
                {
                    // devide also for forward transform
                    for (int i = 0; i < n; i++)
                    {
                        data[i, j].Re = dst[i].Re / n;
                        data[i, j].Im = dst[i].Im / n;
                    }
                }
                else
                {
                    for (int i = 0; i < n; i++)
                    {
                        data[i, j].Re = dst[i].Re;
                        data[i, j].Im = dst[i].Im;
                    }
                }
            }
        }


        /// <summary>
        /// One dimensional Fast Fourier Transform.
        /// </summary>
        /// 
        /// <param name="data">Data to transform.</param>
        /// <param name="direction">Transformation direction.</param>
        /// 
        /// <remarks><para><note>The method accepts <paramref name="data"/> array of 2<sup>n</sup> size
        /// only, where <b>n</b> may vary in the [1, 14] range.</note></para></remarks>
        /// 
        /// <exception cref="ArgumentException">Incorrect data length.</exception>
        /// 
        public ComplexNumber[] FFT(ComplexNumber[] data, Direction direction)
        {
            int n = data.Length;
            int m = Tools.Log2(n);

            // reorder data first
            ReorderData(data);

            // compute FFT
            int tn = 1, tm;

            for (int k = 1; k <= m; k++)
            {
                ComplexNumber[] rotation = FourierTransform.GetComplexRotation(k, direction);

                tm = tn;
                tn <<= 1;

                for (int i = 0; i < tm; i++)
                {
                    ComplexNumber t = rotation[i];

                    for (int even = i; even < n; even += tn)
                    {
                        int odd = even + tm;
                        ComplexNumber ce = data[even];
                        ComplexNumber co = data[odd];

                        double tr = co.Re * t.Re - co.Im * t.Im;
                        double ti = co.Re * t.Im + co.Im * t.Re;

                        data[even].Re += tr;
                        data[even].Im += ti;

                        data[odd].Re = ce.Re - tr;
                        data[odd].Im = ce.Im - ti;
                    }
                }
            }

            if (direction == Direction.Forward)
            {
                for (int i = 0; i < n; i++)
                {
                    data[i].Re /= (double)n;
                    data[i].Im /= (double)n;
                }
            }
            return data;
        }

        /// <summary>
        /// Two dimensional Fast Fourier Transform.
        /// </summary>
        /// 
        /// <param name="data">Data to transform.</param>
        /// <param name="direction">Transformation direction.</param>
        /// 
        /// <remarks><para><note>The method accepts <paramref name="data"/> array of 2<sup>n</sup> size
        /// only in each dimension, where <b>n</b> may vary in the [1, 14] range. For example, 16x16 array
        /// is valid, but 15x15 is not.</note></para></remarks>
        /// 
        /// <exception cref="ArgumentException">Incorrect data length.</exception>
        /// 
        public void FFT2(ComplexNumber[,] data, Direction direction)
        {
            int k = data.GetLength(0);
            int n = data.GetLength(1);

            // check data size
            if (
                (!Tools.IsPowerOf2(k)) ||
                (!Tools.IsPowerOf2(n)) ||
                (k < minLength) || (k > maxLength) ||
                (n < minLength) || (n > maxLength)
                )
            {
                throw new ArgumentException("Incorrect data length.");
            }

            // process rows
            ComplexNumber[] row = new ComplexNumber[n];

            for (int i = 0; i < k; i++)
            {
                // copy row
                for (int j = 0; j < n; j++)
                    row[j] = data[i, j];
                // transform it
                FFT(row, direction);
                // copy back
                for (int j = 0; j < n; j++)
                    data[i, j] = row[j];
            }

            // process columns
            ComplexNumber[] col = new ComplexNumber[k];

            for (int j = 0; j < n; j++)
            {
                // copy column
                for (int i = 0; i < k; i++)
                    col[i] = data[i, j];
                // transform it
                FFT(col, direction);
                // copy back
                for (int i = 0; i < k; i++)
                    data[i, j] = col[i];
            }
        }

        #region Private Region

        private const int minLength = 2;
        private const int maxLength = 16384;
        private const int minBits = 1;
        private const int maxBits = 14;
        private static int[][] reversedBits = new int[maxBits][];
        private static ComplexNumber[,][] complexRotation = new ComplexNumber[maxBits, 2][];

        // Get array, indicating which data members should be swapped before FFT
        private static int[] GetReversedBits(int numberOfBits)
        {
            if ((numberOfBits < minBits) || (numberOfBits > maxBits))
                throw new ArgumentOutOfRangeException();

            // check if the array is already calculated
            if (reversedBits[numberOfBits - 1] == null)
            {
                int n = Tools.Pow2(numberOfBits);
                int[] rBits = new int[n];

                // calculate the array
                for (int i = 0; i < n; i++)
                {
                    int oldBits = i;
                    int newBits = 0;

                    for (int j = 0; j < numberOfBits; j++)
                    {
                        newBits = (newBits << 1) | (oldBits & 1);
                        oldBits = (oldBits >> 1);
                    }
                    rBits[i] = newBits;
                }
                reversedBits[numberOfBits - 1] = rBits;
            }
            return reversedBits[numberOfBits - 1];
        }

        // Get rotation of complex number
        private static ComplexNumber[] GetComplexRotation(int numberOfBits, Direction direction)
        {
            int directionIndex = (direction == Direction.Forward) ? 0 : 1;

            // check if the array is already calculated
            if (complexRotation[numberOfBits - 1, directionIndex] == null)
            {
                int n = 1 << (numberOfBits - 1);
                double uR = 1.0;
                double uI = 0.0;
                double angle = System.Math.PI / n * (int)direction;
                double wR = System.Math.Cos(angle);
                double wI = System.Math.Sin(angle);
                double t;
                ComplexNumber[] rotation = new ComplexNumber[n];

                for (int i = 0; i < n; i++)
                {
                    rotation[i] = new ComplexNumber(uR, uI);
                    t = uR * wI + uI * wR;
                    uR = uR * wR - uI * wI;
                    uI = t;
                }

                complexRotation[numberOfBits - 1, directionIndex] = rotation;
            }
            return complexRotation[numberOfBits - 1, directionIndex];
        }

        // Reorder data for FFT using
        private static void ReorderData(ComplexNumber[] data)
        {
            int len = data.Length;

            // check data length
            if ((len < minLength) || (len > maxLength) || (!Tools.IsPowerOf2(len)))
                throw new ArgumentException("Incorrect data length.");

            int[] rBits = GetReversedBits(Tools.Log2(len));

            for (int i = 0; i < len; i++)
            {
                int s = rBits[i];

                if (s > i)
                {
                    ComplexNumber t = data[i];
                    data[i] = data[s];
                    data[s] = t;
                }
            }
        }

        #endregion
    }

    /*Documentation
     * Class:   Pitch
     * Author:  Josh Weese
     * Purpose: Contain variables needed for a pitch
     * Methods: 
     * Variables:   #int cents...cents off pitch
     *              #string note...note the frequency is
     *              #string accidental...sharp or natural
     * Notes:  
     * ChangeLog:   Version 1.0...5/3/2011--Documented
    */
    public class Pitch
    {
        private string _note;
        public string note
        {
            get
            {
                return _note;
            }
            set
            {
                _note = value;
            }
        }

        private string _accidental;
        public string accidental
        {
            get
            {
                return _accidental;
            }
            set
            {
                _accidental = value;
            }
        }

        private double _fundamentalFreq;
        public double fundamentalFreq
        {
            get
            {
                return _fundamentalFreq;
            }
            set
            {
                _fundamentalFreq = value;
            }
        }
    }

    /*Documentation
     * Class:   PitchConstants
     * Author:  Josh Weese
     * Purpose: Constants for when pitch list is loaded
     * Methods: 
     * Variables:   strings for each possible note (A-G)
     * Notes:  
     * ChangeLog:   Version 1.0...5/3/2011--Documented
    */
    public class PitchConstants
    {
        public readonly string natural = "natural";
        public readonly string sharp = "sharp";

        public readonly string A = "A";
        public readonly string B = "B";
        public readonly string C = "C";
        public readonly string D = "D";
        public readonly string E = "E";
        public readonly string F = "F";
        public readonly string G = "G";
    }
}//end namespace

