using System;
using System.Linq;
using System.Text;
using KModkit;

public partial class InfineedyScript
{
    private abstract class EdgeworkDigit
    {
        public static EdgeworkDigit Random(Func<double> nextDouble)
        {
            int choice = (int)(nextDouble() * 16);
            if (choice < 4)
                return new SNDigit();
            if (choice == 4)
                return new SNSum();
            return new MiscDigit();
        }

        public abstract int Calculate(KMBombInfo info);
        public abstract void Fill(Func<double> nextDouble);
        public abstract override string ToString();
        private class SNDigit : EdgeworkDigit
        {
            private enum SNDigitType : byte { Largest = 0, Smallest = 1, First = 2, Last = 3 }
            private SNDigitType _type;
            public override int Calculate(KMBombInfo info)
            {
                var digits = info.GetSerialNumberNumbers();
                switch (_type)
                {
                    case SNDigitType.Largest: return digits.Max();
                    case SNDigitType.Smallest: return digits.Min();
                    case SNDigitType.First: return digits.First();
                    case SNDigitType.Last: return digits.Last();
                    default: throw new Exception("Unreachable");
                }
            }
            public override void Fill(Func<double> nextDouble) { _type = (SNDigitType)(nextDouble() * 4); }
            public override string ToString()
            {
                var sb = new StringBuilder("the ");
                switch (_type)
                {
                    case SNDigitType.Largest: sb.Append("largest"); break;
                    case SNDigitType.Smallest: sb.Append("smallest"); break;
                    case SNDigitType.First: sb.Append("first"); break;
                    case SNDigitType.Last: sb.Append("last"); break;
                    default: throw new Exception("Unreachable");
                }
                return sb.Append(" digit in the serial number").ToString();
            }
        }
        private class SNSum : EdgeworkDigit
        {
            public override int Calculate(KMBombInfo info) { return info.GetSerialNumberNumbers().Sum() % 10; }
            public override void Fill(Func<double> nextDouble) { }
            public override string ToString() { return "the sum of every digit in the serial number (modulo 10)"; }
        }
        private class MiscDigit : EdgeworkDigit
        {
            private enum MiscModuleType : byte { Any = 0, Solvable = 1, Needy = 2 }
            private MiscModuleType _moduleType;
            private bool _isPlates;
            private enum MiscBatteryType : byte { Any = 0, AA = 1, D = 2 }
            private MiscBatteryType _batteryType;
            private enum MiscIndicatorType : byte { Any = 0, Lit = 1, Unlit = 2 }
            private MiscIndicatorType _indicatorType;
            private enum MiscDigitType : byte { Module = 0, Port = 1, Batteries = 2, Indicators = 3 }
            private MiscDigitType _type;
            public override int Calculate(KMBombInfo info)
            {
                switch (_type)
                {
                    case MiscDigitType.Module:
                        switch (_moduleType)
                        {
                            case MiscModuleType.Any: return info.GetModuleNames().Count % 10;
                            case MiscModuleType.Solvable: return info.GetSolvableModuleIDs().Count % 10;
                            case MiscModuleType.Needy: return (info.GetModuleNames().Count - info.GetSolvableModuleIDs().Count) % 10;
                            default: throw new Exception("Unreachable");
                        }
                    case MiscDigitType.Port: return (_isPlates ? info.GetPortPlateCount() : info.GetPortCount()) % 10;
                    case MiscDigitType.Batteries:
                        switch (_batteryType)
                        {
                            case MiscBatteryType.Any: return info.GetBatteryCount() % 10;
                            case MiscBatteryType.AA: return (info.GetBatteryCount() - info.GetBatteryHolderCount()) * 2 % 10;
                            case MiscBatteryType.D: return (info.GetBatteryHolderCount() * 2 - info.GetBatteryCount()) % 10;
                            default: throw new Exception("Unreachable");
                        }
                    case MiscDigitType.Indicators:
                        switch (_indicatorType)
                        {
                            case MiscIndicatorType.Any: return info.GetIndicators().Count() % 10;
                            case MiscIndicatorType.Lit: return info.GetOnIndicators().Count() % 10;
                            case MiscIndicatorType.Unlit: return info.GetOffIndicators().Count() % 10;
                            default: throw new Exception("Unreachable");
                        }
                    default: throw new Exception("Unreachable");
                }
            }
            public override void Fill(Func<double> nextDouble)
            {
                _moduleType = (MiscModuleType)(nextDouble() * 3);
                _isPlates = (int)(nextDouble() * 2) == 1;
                _batteryType = (MiscBatteryType)(nextDouble() * 3);
                _indicatorType = (MiscIndicatorType)(nextDouble() * 3);
                _type = (MiscDigitType)(nextDouble() * 4);
            }
            public override string ToString()
            {
                var sb = new StringBuilder("the number of ");
                switch (_type)
                {
                    case MiscDigitType.Module:
                        switch (_moduleType)
                        {
                            case MiscModuleType.Any: sb.Append("modules"); break;
                            case MiscModuleType.Solvable: sb.Append("solvable modules"); break;
                            case MiscModuleType.Needy: sb.Append("needy modules"); break;
                            default: throw new Exception("Unreachable");
                        }
                        break;
                    case MiscDigitType.Port: sb.Append(_isPlates ? "port plates" : "ports"); break;
                    case MiscDigitType.Batteries:
                        switch (_batteryType)
                        {
                            case MiscBatteryType.Any: sb.Append("batteries"); break;
                            case MiscBatteryType.AA: sb.Append("AA batteries"); break;
                            case MiscBatteryType.D: sb.Append("D batteries"); break;
                            default: throw new Exception("Unreachable");
                        }
                        break;
                    case MiscDigitType.Indicators:
                        switch (_indicatorType)
                        {
                            case MiscIndicatorType.Any: sb.Append("indicators"); break;
                            case MiscIndicatorType.Lit: sb.Append("lit indicators"); break;
                            case MiscIndicatorType.Unlit: sb.Append("unlit indicators"); break;
                            default: throw new Exception("Unreachable");
                        }
                        break;
                    default: throw new Exception("Unreachable");
                }
                return sb.Append(" (module 10)").ToString();
            }
        }
    }
}
