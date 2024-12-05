using System;
using System.Linq;
using System.Text;
using KModkit;

public partial class InfineedyScript
{
    private abstract class EdgeworkBoolean
    {
        public static EdgeworkBoolean Random(Func<double> nextDouble)
        {
            var choice = (int)(nextDouble() * (1671 + _allModules.Count));
            if (choice < 360)
                return new SNBoolean();
            if (choice < 510)
                return new SingleMiscBoolean();
            if (choice < 546)
                return new IndicatorBoolean();
            if (choice < 996)
                return new DoubleMiscBoolean();
            if (choice < 1671)
                return new DoubleEqualMiscBoolean();
            return new ModuleBoolean();
        }

        public abstract bool Calculate(KMBombInfo info);
        public abstract void FillDigits(Func<double> nextDouble);
        public abstract void Fill(Func<double> nextDouble);
        public abstract override string ToString();
        private class SNBoolean : EdgeworkBoolean
        {
            private byte Count, Threshold, AtMost, ComparisonMode, Odd, Less, OrEqualTo, Comparison;

            public override bool Calculate(KMBombInfo info)
            {
                int count;
                switch (Comparison)
                {
                    case 0: count = info.GetSerialNumberLetters().Count(); break;
                    case 1: count = info.GetSerialNumberLetters().Count("AEIOU".Contains); break;
                    case 2: count = info.GetSerialNumberLetters().Count("BCDFGHJKLMNPQRSTVWXYZ".Contains); break;
                    case 3: count = info.GetSerialNumberNumbers().Count(Odd == 1 ? (Func<int, bool>)(d => d % 2 == 1) : d => d % 2 == 0); break;
                    case 4: count = info.GetSerialNumberNumbers().Count(GoodDigit); break;
                    default: throw new Exception("Unreachable");
                }
                switch (ComparisonMode)
                {
                    case 0: return AtMost == 1 ? count <= Count : count >= Count;
                    case 1: return count > 0;
                    case 2: return count == 0;
                    default: throw new Exception("Unreachable");
                }
            }
            private bool GoodDigit(int d)
            {
                if (Less == 1)
                    return OrEqualTo == 1 ? d <= Threshold : d < Threshold;
                else
                    return OrEqualTo == 1 ? d >= Threshold : d > Threshold;
            }
            public override void FillDigits(Func<double> nextDouble)
            {
                Count = (byte)(2 + nextDouble() * 3);
                Threshold = (byte)(nextDouble() * 10);
            }
            public override void Fill(Func<double> nextDouble)
            {
                AtMost = (byte)(nextDouble() * 2);
                ComparisonMode = (byte)(nextDouble() * 3);
                Odd = (byte)(nextDouble() * 2);
                Less = (byte)(nextDouble() * 2);
                OrEqualTo = (byte)(nextDouble() * 2);
                Comparison = (byte)(nextDouble() * 5);
            }
            public override string ToString()
            {
                var sb = new StringBuilder("the serial number contains ");
                switch (ComparisonMode)
                {
                    case 0: sb.Append("at ").Append(AtMost == 1 ? "most" : "least").Append(' ').Append(Count); break;
                    case 1: sb.Append("any"); break;
                    case 2: sb.Append("no"); break;
                    default: throw new Exception("Unreachable");
                }
                sb.Append(' ');
                switch (Comparison)
                {
                    case 0: sb.Append("letters"); break;
                    case 1: sb.Append("vowels"); break;
                    case 2: sb.Append("consonants"); break;
                    case 3: sb.Append(Odd == 1 ? "odd" : "even").Append(" digits"); break;
                    case 4: sb.Append("digits ").Append(Less == 1 ? "less" : "greater").Append(" than ").Append(OrEqualTo == 1 ? "or equal to " : "").Append(Threshold); break;
                    default: throw new Exception("Unreachable");
                }
                return sb.ToString();
            }
        }
        private class SingleMiscBoolean : EdgeworkBoolean
        {
            private byte Threshold, AtMost, Mode;
            private EdgeworkPart Edgework;
            public override bool Calculate(KMBombInfo info)
            {
                switch (Mode)
                {
                    case 0: return AtMost == 1 ? Edgework.Calculate(info) <= Threshold : Edgework.Calculate(info) >= Threshold;
                    case 1: return Edgework.Calculate(info) > 0;
                    case 2: return Edgework.Calculate(info) == 0;
                    default: throw new Exception("Unreachable");
                }
            }
            public override void FillDigits(Func<double> nextDouble)
            {
                Threshold = (byte)(2 + nextDouble() * 4);
            }
            public override void Fill(Func<double> nextDouble)
            {
                AtMost = (byte)(nextDouble() * 2);
                Mode = (byte)(nextDouble() * 3);
                Edgework = EdgeworkPart.Fill(nextDouble);
            }
            public override string ToString()
            {
                var sb = new StringBuilder("There are ");
                switch (Mode)
                {
                    case 0: sb.Append("at ").Append(AtMost == 1 ? "most" : "least").Append(' ').Append(Threshold); break;
                    case 1: sb.Append("any"); break;
                    case 2: sb.Append("no"); break;
                    default: throw new Exception("Unreachable");
                }
                return sb.Append(' ').Append(Edgework).ToString();
            }
        }
        private class IndicatorBoolean : EdgeworkBoolean
        {
            private byte Mode, Label;
            private static readonly string[] Labels = new string[] { "SND", "CLR", "CAR", "IND", "FRQ", "SIG", "NSA", "MSA", "TRN", "BOB", "FRK", "NLL" };
            public override bool Calculate(KMBombInfo info)
            {
                switch (Mode)
                {
                    case 0: return info.IsIndicatorPresent(Labels[Label]);
                    case 1: return info.IsIndicatorOn(Labels[Label]);
                    case 2: return info.IsIndicatorOff(Labels[Label]);
                    default: throw new Exception("Unreachable");
                }
            }
            public override void FillDigits(Func<double> nextDouble) { }
            public override void Fill(Func<double> nextDouble)
            {
                Mode = (byte)(nextDouble() * 3);
                Label = (byte)(nextDouble() * 12);
            }
            public override string ToString()
            {
                var sb = new StringBuilder("there is a");
                switch (Mode)
                {
                    case 0: sb.Append("n"); break;
                    case 1: sb.Append(" lit"); break;
                    case 2: sb.Append("n unlit"); break;
                    default: throw new Exception("Unreachable");
                }
                return sb.Append(" indicator labeled ").Append(Labels[Label]).ToString();
            }
        }
        private class DoubleMiscBoolean : EdgeworkBoolean
        {
            private byte Fewer;
            private EdgeworkPart First, Second;
            public override bool Calculate(KMBombInfo info)
            {
                return Fewer == 1 ? First.Calculate(info) < Second.Calculate(info) : First.Calculate(info) > Second.Calculate(info);
            }
            public override void FillDigits(Func<double> nextDouble) { }
            public override void Fill(Func<double> nextDouble)
            {
                Fewer = (byte)(nextDouble() * 2);
                First = EdgeworkPart.Fill(nextDouble);
                Second = EdgeworkPart.Fill(nextDouble);
            }
            public override string ToString()
            {
                return new StringBuilder("there are ")
                    .Append(Fewer == 1 ? "fewer" : "more")
                    .Append(' ')
                    .Append(First)
                    .Append(" than ")
                    .Append(Second)
                    .ToString();
            }
        }
        private class DoubleEqualMiscBoolean : EdgeworkBoolean
        {
            private byte Mode;
            private EdgeworkPart First, Second;
            public override bool Calculate(KMBombInfo info)
            {
                switch (Mode)
                {
                    case 0: return First.Calculate(info) == Second.Calculate(info);
                    case 1: return First.Calculate(info) >= Second.Calculate(info);
                    case 2: return First.Calculate(info) <= Second.Calculate(info);
                    default: throw new Exception("Unreachable");
                }
            }
            public override void FillDigits(Func<double> nextDouble) { }
            public override void Fill(Func<double> nextDouble)
            {
                Mode = (byte)(nextDouble() * 3);
                First = EdgeworkPart.Fill(nextDouble);
                Second = EdgeworkPart.Fill(nextDouble);
            }
            public override string ToString()
            {
                return new StringBuilder("there are ")
                    .Append(Mode == 0 ? "exactly as many" : Mode == 1 ? "at least as many" : "at most as many")
                    .Append(' ')
                    .Append(First)
                    .Append(" as ")
                    .Append(Second)
                    .ToString();
            }
        }
        private class ModuleBoolean : EdgeworkBoolean
        {
            private byte Nt;
            private int Module;
            public override bool Calculate(KMBombInfo info)
            {
                return info.GetModuleIDs().Contains(_allModules[Module].ModuleId) ^ Nt == 1;
            }
            public override void FillDigits(Func<double> nextDouble) { }
            public override void Fill(Func<double> nextDouble)
            {
                Nt = (byte)(nextDouble() * 2);
                Module = (int)(nextDouble() * _allModules.Count);
            }
            public override string ToString()
            {
                var sb = new StringBuilder("there are");
                if (Nt == 1) sb.Append("n't");
                return sb.Append(" any \"").Append(_allModules[Module].Name).Append("\" modules on the bomb").ToString();
            }
        }

        private struct EdgeworkPart
        {
            private byte BatteryMode, PortMode, PlateMode, IndicatorMode, OverallMode;
            public static EdgeworkPart Fill(Func<double> nextDouble)
            {
                return new EdgeworkPart
                {
                    BatteryMode = (byte)(nextDouble() * 3),
                    PortMode = (byte)(nextDouble() * 7),
                    PlateMode = (byte)(nextDouble() * 2),
                    IndicatorMode = (byte)(nextDouble() * 3),
                    OverallMode = (byte)(nextDouble() * 4)
                };
            }
            public int Calculate(KMBombInfo info)
            {
                switch (OverallMode)
                {
                    case 0:
                        switch (BatteryMode)
                        {
                            case 0: return info.GetBatteryCount();
                            case 1: return (info.GetBatteryCount() - info.GetBatteryHolderCount()) * 2 % 10;
                            case 2: return (info.GetBatteryHolderCount() * 2 - info.GetBatteryCount()) % 10;
                            default: throw new Exception("Unreachable");
                        }
                    case 1:
                        switch (PortMode)
                        {
                            case 0: return info.GetPortCount();
                            case 1: return info.GetPortCount(Port.Parallel);
                            case 2: return info.GetPortCount(Port.Serial);
                            case 3: return info.GetPortCount(Port.DVI);
                            case 4: return info.GetPortCount(Port.StereoRCA);
                            case 5: return info.GetPortCount(Port.RJ45);
                            case 6: return info.GetPortCount(Port.PS2);
                            default: throw new Exception("Unreachable");
                        }
                    case 2: return PlateMode == 1 ? info.GetPortPlates().Count(p => p.Length == 0) : info.GetPortPlateCount();
                    case 3:
                        switch (IndicatorMode)
                        {
                            case 0: return info.GetIndicators().Count();
                            case 1: return info.GetOnIndicators().Count();
                            case 2: return info.GetOffIndicators().Count();
                            default: throw new Exception("Unreachable");
                        }
                    default: throw new Exception("Unreachable");
                }
            }
            public override string ToString()
            {
                switch (OverallMode)
                {
                    case 0:
                        switch (BatteryMode)
                        {
                            case 0: return "batteries";
                            case 1: return "AA batteries";
                            case 2: return "D batteries";
                            default: throw new Exception("Unreachable");
                        }
                    case 1:
                        switch (PortMode)
                        {
                            case 0: return "ports";
                            case 1: return "parallel ports";
                            case 2: return "serial ports";
                            case 3: return "DVI-D ports";
                            case 4: return "stereo RCA ports";
                            case 5: return "RJ-45 ports";
                            case 6: return "PS/2 ports";
                            default: throw new Exception("Unreachable");
                        }
                    case 2: return PlateMode == 1 ? "empty port plates" : "port plates";
                    case 3:
                        switch (IndicatorMode)
                        {
                            case 0: return "indicators";
                            case 1: return "lit indicators";
                            case 2: return "unlit indicators";
                            default: throw new Exception("Unreachable");
                        }
                    default: throw new Exception("Unreachable");
                }
            }
        }
    }
}
