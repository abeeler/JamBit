using NUnit.Framework;
using NUnit.Extensions.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamBitTesting
{
    [TestFixture]
    public class JamBitFormTests : NUnitFormTest
    {
        JamBit.JamBitForm TestForm;

        [Test]
        public void ProperFormName()
        {

        }

        public void LaunchForm()
        {
            TestForm = new JamBit.JamBitForm();
            
        }
    }
}
