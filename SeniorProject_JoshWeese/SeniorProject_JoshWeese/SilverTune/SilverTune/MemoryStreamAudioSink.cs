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
using System.ComponentModel;
using System.IO;

namespace SilverTune
{
    /*Documentation
     * Class:   MemoryStreamAudioSink
     * Author:  Josh Weese
     * Purpose: Contains the means and methods for handling audio device capture
     * Methods: 
     *          #OnFormatChange.....Event handler when the audio format changes
     *          #OnSamples..........Event handler triggered when audio source takes a sample
     *          #OnCaptureStarted...Event handler that is triggered when capture source starts
     *          #OnCaptureStopped...Event handler that is triggered when capture source stops
     * Globals: 
     *          #MemoryStream stream.......stream that OnSamples writes to
     *          #MemoryStream buffer.......secondary stream that is used to copy the primary to for calculations and processing
     *          #AudioFormat audioFormat...changed audio format
     *          #long lastLength...........last known length of the primary stream
     *          #MainPage page.............pointer to the main page
     *          #MemoryStream AudioData....public variable for access to the buffer
     *          #AudioFormat AudioFormat...public variable for access to the audio format
     * Notes:  
     * ChangeLog:   Version 1.0...5/3/2011--Documented
    */
    public class MemoryStreamAudioSink : AudioSink
    {
        #region "Variables"

        private MemoryStream stream;
        private MemoryStream buffer;
        private AudioFormat audioFormat;
        private long lastLength = 0;
        MainPage page;

        public MemoryStream AudioData
        {
            get
            {
                return (buffer);
            }
            set
            {
                buffer = value;
            }
        }

        public AudioFormat AudioFormat
        {
            get
            {
                return (audioFormat);
            }

        }

        #endregion

        public MemoryStreamAudioSink(MainPage p)
        {
            page = p;
        }

        #region "Methods"

        /*Documentation
         * Method:  OnFormatChange
         * Author:  Josh Weese
         * Purpose: Event handler when the audio format changes
         * Parameters:  AudioFormat audioFormat
         * Returns: 
         * Notes:   
         * ChangeLog:   Version 1.0...5/3/2011--Documented
         */
        protected override void OnFormatChange(AudioFormat audioFormat)
        {
            if (this.audioFormat == null)
            {
                this.audioFormat = audioFormat;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        /*Documentation
         * Method:  OnSamples
         * Author:  Josh Weese
         * Purpose: Event handler triggered when audio source takes a sample
         * Parameters:  #long sampleTime
         *              #long sampleDuration
         *              #byte[] sampleData
         * Returns: 
         * Notes:   
         *          #Writes sample to memory stream each time
         *              -Once stream reaches threshold, it processes the data
         * ChangeLog:   Version 1.0...5/3/2011--Documented
         */
        protected override void OnSamples(long sampleTime, long sampleDuration, byte[] sampleData)
        {
            //check against threshhold, once it hits it, process data
            if ((stream.Length - lastLength) > Constants.fftLength)
            {
                if (buffer.Length == 0)
                {
                    lastLength = stream.Length;
                    //copy to buffer
                    stream.WriteTo(buffer);
                    //begin proccessing
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(page.read);
                    if (stream.Length > stream.Capacity)
                    {
                        stream.Flush();
                        stream.Dispose();
                        stream = new MemoryStream();
                    }
                }
            }
            //write sample
            stream.Write(sampleData, 0, sampleData.Length);
        }

        /*Documentation
         * Method:  OnCaptureStarted
         * Author:  Josh Weese
         * Purpose: Event handler that is triggered when capture source starts
         * Parameters:  
         * Returns: 
         * Notes:   
         * ChangeLog:   Version 1.0...5/3/2011--Documented
         */
        protected override void OnCaptureStarted()
        {
            //initiate memory stream and buffer
            stream = new MemoryStream();
            //initiate buffer
            buffer = new MemoryStream();
        }

        /*Documentation
         * Method:  OnCaptureStopped
         * Author:  Josh Weese
         * Purpose: Event handler that is triggered when capture source stops
         * Parameters:  
         * Returns: 
         * Notes:   
         * ChangeLog:   Version 1.0...5/3/2011--Documented
         */
        protected override void OnCaptureStopped()
        {
            //reset last length of the memory stream to 0
            lastLength = 0;
        }

        #endregion
    }


}
