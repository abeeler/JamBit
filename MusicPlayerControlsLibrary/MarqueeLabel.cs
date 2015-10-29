using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicPlayerControlsLibrary
{
    public partial class MarqueeLabel : UserControl
    {
        private Timer moveTimer;
        private Timer pauseTimer;
        private int curText = 0;

        private static int defaultLabelSpeed = 30;
        private static int defaultPauseLength = 1000;
        private static string[] defaultText = { "marqueeLabel1" };

        private int labelSpeed = defaultLabelSpeed;
        private int pauseLength = defaultPauseLength;
        private string[] cycleText = defaultText;

        [Category("Behavior")]
        [Description("The interval between the label moving one pixel. Lower values move the label faster.")]
        public int LabelSpeed
        {
            get { return labelSpeed; }
            set {
                labelSpeed = value;
                moveTimer.Interval = labelSpeed;
            }
        }

        [Category("Behavior")]
        [Description("The length in milliseconds the label will pause at the left side before scrolling off. Set to 0 to prevent pausing.")]
        public int PauseLength
        {
            get { return pauseLength; }
            set {
                pauseLength = value;
                pauseTimer.Interval = pauseLength;
            }
        }

        [Category("Appearance")]
        [Description("The text that will be cycled after each pass of the label.")]
        public string[] CycleText
        {
            get { return cycleText; }
            set {
                cycleText = value;
                if (cycleText == null)
                {
                    cycleText = new string[] { "" };
                    curText = 0;
                }
                else if (curText >= cycleText.Length)
                    curText = cycleText.Length - 1;

                lblText.Text = cycleText[curText];
            }
        }

        public MarqueeLabel()
        {
            InitializeComponent();
            lblText.Text = cycleText[0];

            moveTimer = new Timer();
            moveTimer.Interval = labelSpeed;
            moveTimer.Tick += moveTimer_OnTick;
            moveTimer.Start();
            
            pauseTimer = new Timer();
            pauseTimer.Interval = pauseLength;
            pauseTimer.Tick += pauseTimer_OnTick;

            lblText.MouseEnter += (sender, e) => OnMouseEnter(e);
            lblText.MouseLeave += (sender, e) => OnMouseLeave(e);
            lblText.MouseUp += (sender, e) => OnMouseUp(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (++curText == cycleText.Length) curText = 0;
            lblText.Text = cycleText[curText];
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            moveTimer.Stop();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            moveTimer.Start();
        }

        private void moveTimer_OnTick(object sender, EventArgs e)
        {
            if (pauseLength > 0 && lblText.Left == 0)
            {
                moveTimer.Stop();
                pauseTimer.Start();                
            }
            else if (lblText.Right <= 0)
            {
                lblText.Location = new Point(Width + 1, lblText.Location.Y);
                if (++curText == cycleText.Length) curText = 0;
                lblText.Text = cycleText[curText];
            }
            else
                lblText.Location = new Point(lblText.Location.X - 1, lblText.Location.Y);
        }

        private void pauseTimer_OnTick(object sender, EventArgs e)
        {
            pauseTimer.Stop();
            moveTimer.Start();
            lblText.Location = new Point(lblText.Location.X - 1, lblText.Location.Y);
        }
    }
}
