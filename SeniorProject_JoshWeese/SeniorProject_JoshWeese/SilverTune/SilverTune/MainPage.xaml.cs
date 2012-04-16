using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace SilverTune
{
    /*Documentation
     * Class:   MainPage
     * Author:  Josh Weese
     * Purpose: Contains the means and methods for determining the fundamental frequency
     * Methods: 
     *          #SilverTuneMain_Loaded..............Page load event handler
     *          #button_StartCapture_Click..........Start capture event handler
     *          #getAudioFormat.....................Uses LINQ to assure the audio device supports the preferred format
     *          #read...............................Reads and processes audio capture memory stream
     *          #button_StopCapture_Click...........Stop Capture event handler which stops audio capture source
     *          #save...............................these methods write each array to a txt file, one item per line
     *          #graphButton_Click..................Graph button event handler which navigates to the graph page
     *          #saveCheck_Checked..................Save check event handler
     *          #IsPowerOfTwo.......................Determines if number is a power of 2
     *          #CalculateManualFreq_Button_Click...Calculate Manual Frequency button event handler
     *          #manualFrequency....................Calculates the freqeuncy based on user input
     *          #ClearButton_Click..................Clear button event handler
     * Globals: 
     *          #CaptureSource captureSource.......the device capture source
     *          #MemoryStreamAudioSink audioSink...MemoryStreamAudioSink class
     *          #int seekPosition..................where to begin reading in the audio capture memory stream
     *          #Frequency frequency...............Frequency class methods
     *          #FourierTransform ft...............FourierTransform class methods
     *          #FundamentalFrequency fundFreq.....store fundamental frequency
     *          #Pitch pitch.......................store pitch
     *          #public List<Pitch> pitchList......store list of possible pitches
     * Notes:  
     * ChangeLog:   Version 1.0...5/3/2011--Documented
    */
    public partial class MainPage : UserControl
    {
        #region "Variables"
        CaptureSource captureSource;
        MemoryStreamAudioSink audioSink;
        int seekPosition = 0;
        Frequency frequency = new Frequency();
        FourierTransform ft = new FourierTransform();
        FundamentalFrequency fundFreq;
        Pitch pitch;
        public List<Pitch> pitchList;
        
        bool saveFile = false;
            
        #endregion 

        public MainPage()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(SilverTuneMain_Loaded);
        }

        #region "Methods"

        /*Documentation
         * Method:  SilverTuneMain_Loaded
         * Author:  Josh Weese
         * Purpose: Page load event handler
         * Parameters:  
         * Returns: 
         * Notes:   
         *          #loads pitches
         *          #loads audio sources
         *          #initializes fundamental freqeuncy
         * ChangeLog:   Version 1.0...5/3/2011--Documented
         */
        void SilverTuneMain_Loaded(object sender, RoutedEventArgs e)
        {
            pitchList = frequency.loadPitches(Constants.concertAfreq , pitchList);

            //get list of available audio sources
            listBox_AudioSources.ItemsSource = CaptureDeviceConfiguration.GetAvailableAudioCaptureDevices();

            //initialize the fundamental frequency
            fundFreq = new FundamentalFrequency();
            freqItemsControl.DataContext = fundFreq;

            //create a new capture source
            captureSource = new CaptureSource();
        }

        /*Documentation
         * Method:  button_StartCapture_Click
         * Author:  Josh Weese
         * Purpose: Start capture event handler
         * Parameters:  
         * Returns: 
         * Notes:  #starts the audio capture using the selected or default device
         * ChangeLog:   Version 1.0...5/3/2011--Documented
         */
        private void button_StartCapture_Click(object sender, RoutedEventArgs e)
        {
            if (captureSource != null)
            {
                if (listBox_AudioSources.SelectedItem != null)
                {//use selected audio device
                    AudioFormat format;
                    captureSource.AudioCaptureDevice = (AudioCaptureDevice)listBox_AudioSources.SelectedItem;
                    if ((format = getAudioFormat()) != null)
                    {
                        captureSource.AudioCaptureDevice.DesiredFormat = format;
                        try
                        {
                            if (CaptureDeviceConfiguration.RequestDeviceAccess())
                            {
                                if (audioSink == null)
                                {
                                    
                                    audioSink = new MemoryStreamAudioSink(this);
                                    audioSink.CaptureSource = captureSource;
                                    
                                }
                            }
                            captureSource.Start();
                        }
                        catch (Exception E)
                        {
                            MessageBox.Show("An error has occured.  Please try again.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Preferred audio format currently not supported on audio device.");
                    }
                }
                else
                {//use defualt audio device
                    AudioFormat format;
                    captureSource.AudioCaptureDevice = CaptureDeviceConfiguration.GetDefaultAudioCaptureDevice();
                    if ((format = getAudioFormat()) != null)
                    {
                        captureSource.AudioCaptureDevice.DesiredFormat = format;
                        try
                        {
                            if (CaptureDeviceConfiguration.RequestDeviceAccess())
                            {
                                if (audioSink == null)
                                {
                                    audioSink = new MemoryStreamAudioSink(this);
                                    audioSink.CaptureSource = captureSource;
                                }
                            }
                            
                            captureSource.Start();
                        }
                        catch (Exception E)
                        {
                            MessageBox.Show("An error has occured.  Please try again.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Preferred audio format currently not supported on audio device.");
                    }
                }
            }
        }

        /*Documentation
         * Method:  getAudioFormat
         * Author:  Josh Weese
         * Purpose: Uses LINQ to assure the audio device supports the preferred format
         * Parameters:  
         * Returns: AudioFormat format
         * Notes:  
         * ChangeLog:   Version 1.0...5/3/2011--Documented
         */
        public AudioFormat getAudioFormat()
        {
            var query = from f in captureSource.AudioCaptureDevice.SupportedFormats
                        where f.BitsPerSample == Constants.preferredBitsPerSample &&
                              f.Channels == Constants.preferredChannel &&
                              f.SamplesPerSecond == Constants.preferredSamplesPerSecond &&
                              f.WaveFormat == Constants.preferredtWaveFormat
                        select f;
            var format = query.First();
            return format;
        }

        /*Documentation
         * Method:  read
         * Author:  Josh Weese
         * Purpose: Reads and processes audio capture memory stream
         * Parameters:  
         * Returns: 
         * Notes:   #reads audio capture memory stream
         *          #converts stream into usable data
         *          #saves data to txt file if option is checked
         *          #chunks data, converts it to complex numbers, runs FFT, and determines pitch and frequency
         * ChangeLog:   Version 1.0...5/3/2011--Documented
         */
        public void read()
        {
            //make sure data reached the fft lenght thresh hold
            if (audioSink.AudioData.Length >= Constants.fftLength)
            {
                //read the entire memory stream
                audioSink.AudioData.Seek(seekPosition, SeekOrigin.Begin);
                byte[] data = new byte[audioSink.AudioData.Length];
                int chunkSize = 4096;
                int numBytesToRead = (int)audioSink.AudioData.Length;
                int numBytesRead = 0;

                while (numBytesToRead > 0)
                {
                    if (chunkSize + numBytesRead > data.Length)
                    {
                        chunkSize = data.Length - numBytesRead;
                    }
                    int n = audioSink.AudioData.Read(data, numBytesRead, chunkSize);
                    if (n == 0)
                    {
                        break;
                    }
                    numBytesRead += n;
                    numBytesToRead -= n;
                }
                //kill the buffer once read
                audioSink.AudioData.Flush();
                audioSink.AudioData.Dispose();
                //re-initialize
                audioSink.AudioData = new MemoryStream();

                if (saveFile)
                {
                    save(data);
                }
                //length of data
                int length;

                Int16[] int16Data;

                int y = 0;
                int z = 0;

                //convert raw data to useable numbers
                if (audioSink.AudioFormat.BitsPerSample == 16)
                {
                    length = data.Length / 2;
                    int16Data = new Int16[length];
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(data);
                    }

                    for (int i = 0; i < length; i += 2)
                    {
                        if (!(i > data.Length))
                        {
                            int16Data[z] = BitConverter.ToInt16(data, i);
                        }
                        else
                        {
                            int16Data[z] = 0;
                        }

                        z++;
                    }
                }
                else
                {
                    length = data.Length;
                    int16Data = new Int16[length];
                    for (int i = 0; i < length; i++)
                    {
                        int16Data[i] = data[i];
                    }
                }

                if (saveFile)
                {
                    save(int16Data);
                }

                z = 0;

                //chunk audio data to feed to FFT and calculate freqency
                while (true)
                {
                    //chunk
                    int max;
                    if (length > Constants.fftLength)
                    {
                        max = Constants.fftLength;
                    }
                    else
                    {
                        if (IsPowerOfTwo((ulong)length))
                        {
                            max = length;
                        }
                        else
                        {
                            break;
                        }
                    }
                    //convert to complex numbers
                    ComplexNumber[] complexData1 = new ComplexNumber[max];
                    y = 0;
                    while (y < max)
                    {
                        complexData1[y] = new ComplexNumber(int16Data[z] / Constants.downsample);
                        y++;
                        z++;
                    }
                    //run fft
                    ComplexNumber[] fft1 = ft.FFT(complexData1, FourierTransform.Direction.Forward);

                    if (saveFile)
                    {
                        save(fft1);
                    }

                    //retrieve pitch
                    pitch = frequency.getPitch(fundFreq.fundamentalFreq, pitchList);
                    //frequency
                    fundFreq.fundamentalFreq = frequency.calcFundamentalFreq(fft1, audioSink.AudioFormat.SamplesPerSecond);
                    //cents
                    fundFreq.cents = frequency.calculateCents(fundFreq.fundamentalFreq, pitch.fundamentalFreq);
                    //actual note
                    fundFreq.note = pitch.note;
                    //accidental
                    fundFreq.accidental = pitch.accidental;
                    
                    if ((length -= max) == 0)
                    {
                        break;
                    }
                }
            }

        }//end read
        
        #region "SAVE METHODS"
        //these methods write each array to a txt file, one item per line
        private void save(byte[] data)
        {
            try
            {
                //byte[] fileData = new byte[data.Length];

                SaveFileDialog save = new SaveFileDialog();
                save.DefaultExt = ".txt";
                bool? saveResult = save.ShowDialog();
                if (saveResult == true)
                {
                    Stream fs = save.OpenFile();
                    StreamWriter sw = new StreamWriter(fs);
                    for (int i = 0; i < data.Length; i++)
                    {
                        sw.WriteLine(data[i].ToString());
                    }
                    sw.Flush();
                    sw.Close();
                }

            }
            catch (Exception e)
            {

            }
        }

        private void save(Int16[] data)
        {
            try
            {
                //byte[] fileData = new byte[data.Length];

                SaveFileDialog save = new SaveFileDialog();
                save.DefaultExt = ".txt";
                bool? saveResult = save.ShowDialog();
                if (saveResult == true)
                {
                    Stream fs = save.OpenFile();
                    StreamWriter sw = new StreamWriter(fs);
                    for (int i = 0; i < data.Length; i++)
                    {
                        sw.WriteLine(data[i].ToString());
                    }
                    sw.Flush();
                    sw.Close();
                }

            }
            catch (Exception e)
            {

            }
        }

        private void save(ComplexNumber[] data)
        {
            try
            {
                //byte[] fileData = new byte[data.Length];

                SaveFileDialog save = new SaveFileDialog();
                save.DefaultExt = ".txt";
                bool? saveResult = save.ShowDialog();
                if (saveResult == true)
                {
                    Stream fs = save.OpenFile();
                    StreamWriter sw = new StreamWriter(fs);
                    for (int i = 0; i < data.Length; i++)
                    {
                        sw.WriteLine(data[i].SquaredMagnitude.ToString());
                    }
                    sw.Flush();
                    sw.Close();
                }

            }
            catch (Exception e)
            {

            }
        }

        private void save(double[] data)
        {
            try
            {
                //byte[] fileData = new byte[data.Length];

                SaveFileDialog save = new SaveFileDialog();
                save.DefaultExt = ".txt";
                bool? saveResult = save.ShowDialog();
                if (saveResult == true)
                {
                    Stream fs = save.OpenFile();
                    StreamWriter sw = new StreamWriter(fs);
                    for (int i = 0; i < data.Length; i++)
                    {
                        sw.WriteLine(data[i].ToString());
                    }
                    sw.Flush();
                    sw.Close();
                }

            }
            catch (Exception e)
            {

            }
        }
        #endregion

        /*Documentation
         * Method:  button_StopCapture_Click
         * Author:  Josh Weese
         * Purpose: Stop Capture event handler which stops audio capture source
         * Parameters:  
         * Returns: 
         * Notes:  
         * ChangeLog:   Version 1.0...5/3/2011--Documented
         */
        private void button_StopCapture_Click(object sender, RoutedEventArgs e)
        {
            if (audioSink.CaptureSource.State == CaptureState.Started)
            {
                audioSink.CaptureSource.Stop();
            }

            /*
             * using (FileStream stream = File.OpenWrite(
            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\test.wav"))
            {
                byte[] wavFileHeader = WavFileHelper.GetWavFileHeader(audioSink.AudioData.Length,
                  audioSink.AudioFormat);

                stream.Write(wavFileHeader, 0, wavFileHeader.Length);

                // Now write the rest of the data...  
                byte[] buffer = new byte[4096];
                int r = 0;

                audioSink.AudioData.Seek(0, SeekOrigin.Begin);

                while ((r = audioSink.AudioData.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, r);
                }
                stream.Flush();
                stream.Close();
            }  
           */
        }

        //this sort is not currently in use
        /*
        #region "Quick Sort"
        //--------------------------------------------------------------
        public void recQuickSort(ref ComplexNumber[] x, int left, int right)
        {
            int size = right - left + 1;
            if (size <= 3)                  // manual sort if small
                manualSort(ref x, left, right);
            else                           // quicksort if large
            {
                double median = medianOf3(ref x, left, right);
                int partition = partitionIt(ref x, left, right, median);
                recQuickSort(ref x, left, partition - 1);
                recQuickSort(ref x, partition + 1, right);
            }
        }  // end recQuickSort()
        //--------------------------------------------------------------
        public double medianOf3(ref ComplexNumber[] x, int left, int right)
        {
            int center = (left + right) / 2;
            // order left & center
            if (x[left].Magnitude > x[center].Magnitude)
                swap(ref x, left, center);
            // order left & right
            if (x[left].Magnitude > x[right].Magnitude)
                swap(ref x, left, right);
            // order center & right
            if (x[center].Magnitude > x[right].Magnitude)
                swap(ref x, center, right);

            swap(ref x, center, right - 1);             // put pivot on right
            return x[right - 1].Magnitude;          // return median value
        }  // end medianOf3()
        //--------------------------------------------------------------
        public void swap(ref ComplexNumber[] x, int dex1, int dex2)  // swap two elements
        {
            ComplexNumber temp = x[dex1];        // A into temp
            x[dex1] = x[dex2];   // B into A
            x[dex2] = temp;             // temp into B
        }  // end swap(
        //--------------------------------------------------------------
        public int partitionIt(ref ComplexNumber[] x, int left, int right, double pivot)
        {
            int leftPtr = left;             // right of first elem
            int rightPtr = right - 1;       // left of pivot

            while (true)
            {
                while (x[++leftPtr].Magnitude < pivot)  // find bigger
                    ;                                  //    (nop)
                while (x[--rightPtr].Magnitude > pivot) // find smaller
                    ;                                  //    (nop)
                if (leftPtr >= rightPtr)      // if pointers cross,
                    break;                    //    partition done
                else                         // not crossed, so
                    swap(ref x, leftPtr, rightPtr);  // swap elements
            }  // end while(true)
            swap(ref x, leftPtr, right - 1);         // restore pivot
            return leftPtr;                 // return pivot location
        }  // end partitionIt()
        //--------------------------------------------------------------
        public void manualSort(ref ComplexNumber[] x, int left, int right)
        {
            int size = right - left + 1;
            if (size <= 1)
                return;         // no sort necessary
            if (size == 2)
            {               // 2-sort left and right
                if (x[left].Magnitude > x[right].Magnitude)
                    swap(ref x, left, right);
                return;
            }
            else               // size is 3
            {               // 3-sort left, center, & right
                if (x[left].Magnitude > x[right - 1].Magnitude)
                    swap(ref x, left, right - 1);                // left, center
                if (x[left].Magnitude > x[right].Magnitude)
                    swap(ref x, left, right);                  // left, right
                if (x[right - 1].Magnitude > x[right].Magnitude)
                    swap(ref x, right - 1, right);               // center, right
            }
        }

        #endregion
        */

        /*Documentation
         * Method:  graphButton_Click
         * Author:  Josh Weese
         * Purpose: Graph button event handler which navigates to the graph page
         * Parameters:  
         * Returns: 
         * Notes:  
         * ChangeLog:   Version 1.0...5/3/2011--Documented
         */
        private void graphButton_Click(object sender, RoutedEventArgs e)
        {
            PageSwitcher ps = this.Parent as PageSwitcher;
            ps.navigate(new SilverTuneGraph());
        }

        /*Documentation
         * Method:  saveCheck_Checked
         * Author:  Josh Weese
         * Purpose: Save check event handler
         * Parameters:  
         * Returns: 
         * Notes:  
         *          #Notifies when the save check box is check or unchecked
         * ChangeLog:   Version 1.0...5/3/2011--Documented
         */
        private void saveCheck_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)saveCheck.IsChecked)
            {
                saveFile = true;
            }
            else
            {
                saveFile = false;
            }
        }

        /*Documentation
         * Method:  IsPowerOfTwo
         * Author:  Josh Weese
         * Purpose: Determines if number is a power of 2
         * Parameters:  #ulong x
         * Returns: bool...true/false
         * Notes:  
         * ChangeLog:   Version 1.0...5/3/2011--Documented
         */
        bool IsPowerOfTwo(ulong x)
        {
            return (x & (x - 1)) == 0;
        }

        /*Documentation
         * Method:  CalculateManualFreq_Button_Click
         * Author:  Josh Weese
         * Purpose: Calculate Manual Frequency button event handler
         * Parameters:  
         * Returns: 
         * Notes:  
         *          #validates data and calls manualFrequency()
         * ChangeLog:   Version 1.0...5/3/2011--Documented
         */
        private void CalculateManualFreq_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double freq = Convert.ToDouble(textBox_Freq.Text);
                int sampleCount = Convert.ToInt32(textBox_SampleSize.Text);
                int sampleRate = Convert.ToInt32(textBox_SampleRate.Text);
                manualFrequency(freq, sampleCount, sampleRate);
            }
            catch(Exception x)
            {
                MessageBox.Show("Check input! \n\n" + x.Message);
            }
        }

        /*Documentation
         * Method:  manualFrequency
         * Author:  Josh Weese
         * Purpose: Calculates the freqeuncy based on user input
         * Parameters:  
         * Returns: 
         * Notes:
         *          #This method is ment for accuracy testing
         *          #creates a sine wave based on the input frequency and sample count
         *          #converts to complex numbers
         *          #runs fft and calculates pitch
         * ChangeLog:   Version 1.0...5/3/2011--Documented
         */
        private void manualFrequency(double freq, int sampleCount, int sampleRate)
        {
            //create sine wave
            double increment = (double)(2 * Math.PI) * freq / sampleRate;
            double angle = 0;
            double[] samples = new double[sampleCount];

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = ((double)Math.Sin(angle));
                angle += increment;
            }

            if ((bool)saveCheck.IsChecked)
            {
                save(samples);
            }

            //convert to complex numbers
            ComplexNumber[] complexData1 = new ComplexNumber[sampleCount];
            int y = 0;
            while (y < sampleCount)
            {
                complexData1[y] = new ComplexNumber(samples[y]);
                y++;
            }

            //run fft
            ComplexNumber[] fft1 = ft.FFT(complexData1, FourierTransform.Direction.Forward);
            if ((bool)saveCheck.IsChecked)
            {
                save(fft1);
            }

            //frequency
            fundFreq.fundamentalFreq = frequency.calcFundamentalFreq(fft1, sampleRate);
            //get pitch
            pitch = frequency.getPitch(fundFreq.fundamentalFreq, pitchList);
            
            //cents
            fundFreq.cents = frequency.calculateCents(fundFreq.fundamentalFreq, pitch.fundamentalFreq);
            //actual note
            fundFreq.note = pitch.note;
            //accidental
            fundFreq.accidental = pitch.accidental;
            
        }

        /*Documentation
         * Method:  ClearButton_Click
         * Author:  Josh Weese
         * Purpose: Clear button event handler
         * Parameters:  
         * Returns: 
         * Notes:   
         *          #clears all calculated data on the screen
         * ChangeLog:   Version 1.0...5/3/2011--Documented
         */
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            fundFreq.accidental = "";
            fundFreq.note = "";
            fundFreq.fundamentalFreq = 0;
            fundFreq.cents = 0;
        }

        #endregion
    }//endclass

}//end namespace

