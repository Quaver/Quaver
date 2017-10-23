using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Modifiers
{
    internal enum ModIdentifier
    {
        NoSliderVelocity = 1 << 0, // No Slider Velocity
        Speed15X = 1 << 1, // 1.5x Speed
    }
}
