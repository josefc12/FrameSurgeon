using FrameSurgeon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FrameSurgeon.Services
{
    public class Calculator
    {
        public static FlipbookResolution CalculateFlipbookDimensions(int frameAmount)
        {
            if (frameAmount <= 0)
            {
                return new FlipbookResolution { HorizontalAmount = 0, VerticalAmount = 0 };
            }

            double squareRoot = Math.Sqrt((double)frameAmount);

            // Frame amount has a perfect square root
            if (squareRoot == Math.Floor(squareRoot))
            {
                return new FlipbookResolution { HorizontalAmount = (int)squareRoot, VerticalAmount = (int)squareRoot };
            }

            // Frame amount doesn't have a perfect square root and is odd, add +1
            if (frameAmount % 2 != 0)
            {
                frameAmount = frameAmount + 1;
            }


            int largestEvenDivisor = 0;
            if (frameAmount == 2)
            {
                largestEvenDivisor = 2;
            }
            else
            {
                // Frame amount should now be even
                for (int i = frameAmount / 2 * 2; i > 0; i -= 2)
                {
                    if (frameAmount % i == 0 && i != frameAmount)
                    {
                        largestEvenDivisor = i;
                        break;
                    }
                }
            }
            return new FlipbookResolution { HorizontalAmount = largestEvenDivisor, VerticalAmount = (frameAmount / largestEvenDivisor) };
        }
    }


}
