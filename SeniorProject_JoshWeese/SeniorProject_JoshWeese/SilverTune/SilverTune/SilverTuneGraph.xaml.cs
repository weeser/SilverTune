using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;


namespace SilverTune
{
    /*Documentation
     * Class:   Frequency
     * Author:  Josh Weese
     * Purpose: Contains the means and methods for handling audio device capture
     * Methods: 
     *          #Button_Click.....Event handler when the open file button is clicked
     *          #switch_Click.....Navigation button back to the main page
     * Globals: 
     * Notes:   #***Please note that this Page is UNFINISHED***
     *          #***Although it is operational, but has no error checking***
     * ChangeLog:   Version 1.0...5/3/2011--Documented
    */
    public partial class SilverTuneGraph : UserControl
    {
        public SilverTuneGraph()
        {
            InitializeComponent();
        }

        /*Documentation
         * Method:  Button_Click
         * Author:  Josh Weese
         * Purpose: Event handler when the open file button is clicked
         * Parameters:  AudioFormat audioFormat
         * Returns: 
         * Notes:   
         *          #Opens and reads file
         *              -****ONLY SUPPORTS TXT FILES WITH ONE NUMBER PER LINE*****
         *          #Loads points into graph
         * ChangeLog:   Version 1.0...5/3/2011--Documented
         */
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //open file
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            dlg.Filter = "All Files|*.*";
            List<GraphData> data = new List<GraphData>();
            //read file
            if ((bool)dlg.ShowDialog())
            {
                StreamReader stream = dlg.File.OpenText();
                int i = 0;
                double value = 0;
                //only load max number of points
                while (i < Constants.maxGraphPoints )
                {
                    value = Convert.ToDouble(stream.ReadLine());
                    if (true)
                    {
                        GraphData item = new GraphData();
                        item.position = i;
                        item.value = value;
                        data.Add(item);
                    }
                    i++;
                }
                //graph name
                audioGraph.Title = dlg.File.Name;
                LineSeries ss = audioGraph.Series[0] as LineSeries;
                //load graph
                ss.ItemsSource = data;
            }
            else
            {
                statusText.Text = "No File Selected...";
            }
        }

        //navigate back to tuner
        private void switch_Click(object sender, RoutedEventArgs e)
        {
            PageSwitcher ps = this.Parent as PageSwitcher;
            ps.navigate(new MainPage());
        }
        
    }

    //represents each data point
    public class GraphData
    {
        public double value { get; set; }
        public int position { get; set; }
    }
}
