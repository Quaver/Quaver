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
        Speed05X = 1 << 1, // Speed 0.5x,
        Speed06X = 1 << 2, // Speed 0.6x
        Speed07X = 1 << 3, // Speed 0.7x
        Speed08X = 1 << 4, // Speed 0.8x
        Speed09X = 1 << 5, // Speed 0.9x
        Speed11X = 1 << 6, // Speed 1.1x
        Speed12X = 1 << 7, // Speed 1.2x
        Speed13X = 1 << 8, // Speed 1.3x
        Speed14X = 1 << 9, // Speed 1.4x
        Speed15X = 1 << 10, // Speed 1.5x
        Speed16X = 1 << 11, // Speed 1.6x
        Speed17X = 1 << 12, // Speed 1.7x
        Speed18X = 1 << 13, // Speed 1.8x
        Speed19X = 1 << 14, // Speed 1.9x
        Speed20X = 1 << 15, // Speed 2.0x
    }
}
