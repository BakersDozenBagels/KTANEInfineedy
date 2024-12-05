using System;

public partial class InfineedyScript
{
    private enum SolutionType : byte
    {
        Press,
        PressAtTime,
        Hold,
        Mash
    }

    private struct Solution
    {
        public SolutionType Type;
        public byte Label;
        public byte Param;

        public Solution(SolutionType type, byte label, byte param)
        {
            Type = type;
            Label = label;
            Param = param;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case SolutionType.Press: return "Press button " + Label;
                case SolutionType.PressAtTime: return "Press button " + Label + " when the timer's last digit is " + Param;
                case SolutionType.Hold: return "Hold button " + Label + " for " + Param + " timer seconds";
                case SolutionType.Mash: return "Press button " + Label + " " + Param + " time(s)";
                default: throw new Exception("Unreachable");
            }
        }
    }
}
