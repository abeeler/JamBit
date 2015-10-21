using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicPlayerControlsLibrary
{
    public partial class SlidableProgressBar : ProgressBar
    {
        private bool sliding = false;

        [Category("Action")]
        public event EventHandler ValueSelected;
        [Category("Action")]
        public event EventHandler ValueSlidTo;

        public SlidableProgressBar()
        {
            InitializeComponent();
        }

        public void SetValue(int value, bool ignoreSlide = false)
        {
            if (!ignoreSlide && sliding)
                return;
            if (value < Minimum)
                value = Minimum;
            if (value > Maximum)
                value = Maximum;
            Value = value;

            if (Value > 0)
            {
                Value--;
                Value++;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            sliding = true;
            OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!sliding)
                return;
            sliding = false;
            if (ValueSelected != null)
                ValueSelected.Invoke(this, new EventArgs());
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!sliding)
                return;
            double percent;            
            if (e.X > Width)
                percent = 1;
            else if (e.X < 0)
                percent = 0;
            else
                percent = (double)e.X / Width;            
            SetValue((int)(percent * Maximum), true);
            if (ValueSlidTo != null)
                ValueSlidTo.Invoke(this, new EventArgs());
        }
    }
}
