using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverTune
{

    //this class simply handles navigating between user controls
    public partial class PageSwitcher : UserControl
    {
        public PageSwitcher()
        {
            InitializeComponent();
            if (this.Content == null)
            {
                this.Content = new MainPage();
            }
        }

        public void navigate(UserControl nextPage)
        {
            this.Content = nextPage;
        }
    }
    
}
