using System;
using System.Text;

public partial class InfineedyScript
{
    private abstract class Stage
    {
        public static Stage Random(Func<double> nextDouble)
        {
            var choice = (int)(nextDouble() * 6);
            switch (choice)
            {
                case 0: return new TapIfStage(nextDouble);
                case 1: return new TapWhenStage(nextDouble);
                case 2: return new HoldIfStage(nextDouble);
                case 3: return new HoldStage(nextDouble);
                case 4: return new MashIfStage(nextDouble);
                case 5: return new MashStage(nextDouble);
                default: throw new Exception("Unreachable");
            }
        }

        public abstract Solution Calculate(KMBombInfo info);
        public abstract override string ToString();
        private class TapIfStage : Stage
        {
            private readonly int _count;
            private readonly EdgeworkDigit[] _numbers;
            private readonly EdgeworkBoolean[] _booleans;
            private readonly int[] _digits;
            private readonly bool[] _modes;
            public TapIfStage(Func<double> nextDouble)
            {
                _count = (int)(3 + nextDouble() * 4);
                _numbers = new EdgeworkDigit[_count];
                _booleans = new EdgeworkBoolean[_count - 1];
                _digits = new int[_count];
                _modes = new bool[_count];
                for (int i = 0; i < _count; i++)
                    _numbers[i] = EdgeworkDigit.Random(nextDouble);
                for (int i = 0; i < _count - 1; i++)
                    _booleans[i] = EdgeworkBoolean.Random(nextDouble);
                for (int i = 0; i < _count - 1; i++)
                {
                    _booleans[i].FillDigits(nextDouble);
                    _digits[i] = (int)(nextDouble() * 10);
                }
                _digits[_count - 1] = (int)(nextDouble() * 10);
                for (int i = 0; i < _count - 1; i++)
                {
                    _booleans[i].Fill(nextDouble);
                    _numbers[i].Fill(nextDouble);
                    _modes[i] = (int)(nextDouble() * 2) == 1;
                }
                _numbers[_count - 1].Fill(nextDouble);
                _modes[_count - 1] = (int)(nextDouble() * 2) == 1;
            }
            public override Solution Calculate(KMBombInfo info)
            {
                for (int i = 0; i < _count - 1; i++)
                    if (_booleans[i].Calculate(info))
                        return new Solution(SolutionType.Press,
                            (byte)(_modes[i] ? _digits[i] : _numbers[i].Calculate(info)),
                            0);
                return new Solution(SolutionType.Press,
                    (byte)(_modes[_count - 1] ? _digits[_count - 1] : _numbers[_count - 1].Calculate(info)),
                    0);
            }
            public override string ToString()
            {
                var sb = new StringBuilder("If ")
                    .Append(_booleans[0])
                    .Append(',');
                for (int i = 0; i < _count; i++)
                {
                    sb.Append(" press the button labeled ");
                    if (_modes[i]) sb.Append(_digits[i]);
                    else sb.Append("with ").Append(_numbers[i]);
                    if (i < _count - 1) sb.Append(". Otherwise,");
                    if (i < _count - 2) sb.Append(" if ").Append(_booleans[i + 1]).Append(',');
                }
                return sb.Append('.').ToString();
            }
        }
        private class TapWhenStage : Stage
        {
            private readonly EdgeworkDigit[] _numbers;
            private readonly int[] _digits;
            private readonly bool[] _modes;
            public TapWhenStage(Func<double> nextDouble)
            {
                _numbers = new EdgeworkDigit[2];
                _digits = new int[2];
                _modes = new bool[2];
                _numbers[0] = EdgeworkDigit.Random(nextDouble);
                _numbers[1] = EdgeworkDigit.Random(nextDouble);
                _digits[0] = (int)(nextDouble() * 10);
                _digits[1] = (int)(nextDouble() * 10);
                _numbers[0].Fill(nextDouble);
                _modes[0] = (int)(nextDouble() * 2) == 1;
                _numbers[1].Fill(nextDouble);
                _modes[1] = (int)(nextDouble() * 2) == 1;
            }
            public override Solution Calculate(KMBombInfo info)
            {
                return new Solution(SolutionType.PressAtTime,
                    (byte)(_modes[0] ? _digits[0] : _numbers[0].Calculate(info)),
                    (byte)(_modes[1] ? _digits[1] : _numbers[1].Calculate(info)));
            }
            public override string ToString()
            {
                var sb = new StringBuilder("Press the button labeled ");
                if (_modes[0]) sb.Append(_digits[0]);
                else sb.Append("with ").Append(_numbers[0]);
                sb.Append(" when the seconds digit of the bomb's timer is ");
                if (_modes[1]) sb.Append(_digits[1]);
                else sb.Append(_numbers[1]);
                return sb.Append('.').ToString();
            }
        }
        private class HoldIfStage : Stage
        {
            private readonly int _count;
            private readonly EdgeworkDigit[] _numbers, _numbersTwo;
            private readonly EdgeworkBoolean[] _booleans;
            private readonly int[] _digits, _digitsTwo, _modesTwo;
            private readonly bool[] _modes, _modesThree;
            public HoldIfStage(Func<double> nextDouble)
            {
                _count = (int)(3 + nextDouble() * 3);
                _numbers = new EdgeworkDigit[_count];
                _numbersTwo = new EdgeworkDigit[_count];
                _booleans = new EdgeworkBoolean[_count - 1];
                _digits = new int[_count];
                _digitsTwo = new int[_count];
                _modes = new bool[_count];
                _modesTwo = new int[_count];
                _modesThree = new bool[_count];
                for (int i = 0; i < _count; i++)
                {
                    _numbers[i] = EdgeworkDigit.Random(nextDouble);
                    _numbersTwo[i] = EdgeworkDigit.Random(nextDouble);
                }
                for (int i = 0; i < _count - 1; i++)
                    _booleans[i] = EdgeworkBoolean.Random(nextDouble);
                for (int i = 0; i < _count - 1; i++)
                {
                    _booleans[i].FillDigits(nextDouble);
                    _digits[i] = (int)(nextDouble() * 10);
                    _digitsTwo[i] = 2 + (int)(nextDouble() * 12);
                }
                _digits[_count - 1] = (int)(nextDouble() * 10);
                _digitsTwo[_count - 1] = 2 + (int)(nextDouble() * 12);
                for (int i = 0; i < _count - 1; i++)
                {
                    _booleans[i].Fill(nextDouble);
                    _numbers[i].Fill(nextDouble);
                    _modes[i] = (int)(nextDouble() * 2) == 1;
                    _numbersTwo[i].Fill(nextDouble);
                    _modesTwo[i] = 2 + (int)(nextDouble() * 3);
                    _modesThree[i] = (int)(nextDouble() * 2) == 1;
                }
                _numbers[_count - 1].Fill(nextDouble);
                _modes[_count - 1] = (int)(nextDouble() * 2) == 1;
                _numbersTwo[_count - 1].Fill(nextDouble);
                _modesTwo[_count - 1] = 2 + (int)(nextDouble() * 3);
                _modesThree[_count - 1] = (int)(nextDouble() * 2) == 1;
            }
            public override Solution Calculate(KMBombInfo info)
            {
                for (int i = 0; i < _count - 1; i++)
                    if (_booleans[i].Calculate(info))
                        return new Solution(SolutionType.Hold,
                            (byte)(_modes[i] ? _digits[i] : _numbers[i].Calculate(info)),
                            (byte)(_modesThree[i] ? _digitsTwo[i] : _numbersTwo[i].Calculate(info) + _modesTwo[i]));
                return new Solution(SolutionType.Hold,
                    (byte)(_modes[_count - 1] ? _digits[_count - 1] : _numbers[_count - 1].Calculate(info)),
                    (byte)(_modesThree[_count - 1] ? _digitsTwo[_count - 1] : _numbersTwo[_count - 1].Calculate(info) + _modesTwo[_count - 1]));
            }
            public override string ToString()
            {
                var sb = new StringBuilder("If ")
                    .Append(_booleans[0])
                    .Append(',');
                for (int i = 0; i < _count; i++)
                {
                    sb.Append(" hold the button labeled ");
                    if (_modes[i]) sb.Append(_digits[i]);
                    else sb.Append("with ").Append(_numbers[i]);
                    sb.Append(" for ");
                    if (_modesThree[i]) sb.Append(_digitsTwo[i]).Append(" timer seconds");
                    else sb.Append("a number of timer seconds equal to ").Append(_numbersTwo[i]).Append(", plus ").Append(_modesTwo[i] == 2 ? "two" : _modesTwo[i] == 3 ? "three" : "four");
                    if (i < _count - 1) sb.Append(". Otherwise, ");
                    if (i < _count - 2) sb.Append("if ").Append(_booleans[i + 1]).Append(',');
                }
                return sb.Append('.').ToString();
            }
        }
        private class HoldStage : Stage
        {
            private readonly EdgeworkDigit[] _numbers;
            private readonly int[] _digits;
            private readonly bool[] _modes;
            private readonly int _modeTwo;
            public HoldStage(Func<double> nextDouble)
            {
                _numbers = new EdgeworkDigit[2];
                _digits = new int[2];
                _modes = new bool[2];
                _numbers[0] = EdgeworkDigit.Random(nextDouble);
                _numbers[1] = EdgeworkDigit.Random(nextDouble);
                _digits[0] = (int)(nextDouble() * 10);
                _digits[1] = 2 + (int)(nextDouble() * 12);
                _numbers[0].Fill(nextDouble);
                _modes[0] = (int)(nextDouble() * 2) == 1;
                _numbers[1].Fill(nextDouble);
                _modeTwo = 2 + (int)(nextDouble() * 3);
                _modes[1] = (int)(nextDouble() * 2) == 1;
            }
            public override Solution Calculate(KMBombInfo info)
            {
                return new Solution(SolutionType.Hold,
                    (byte)(_modes[0] ? _digits[0] : _numbers[0].Calculate(info)),
                    (byte)(_modes[1] ? _digits[1] : _numbers[1].Calculate(info) + _modeTwo));
            }
            public override string ToString()
            {
                var sb = new StringBuilder("Hold the button labeled ");
                if (_modes[0]) sb.Append(_digits[0]);
                else sb.Append("with ").Append(_numbers[0]);
                sb.Append(" for ");
                if (_modes[1]) sb.Append(_digits[1]).Append(" timer seconds");
                else sb.Append("a number of timer seconds equal to ").Append(_numbers[1]).Append(", plus ").Append(_modeTwo == 2 ? "two" : _modeTwo == 3 ? "three" : "four");
                return sb.Append('.').ToString();
            }
        }
        private class MashIfStage : Stage
        {
            private readonly int _count;
            private readonly EdgeworkDigit[] _numbers;
            private readonly EdgeworkBoolean[] _booleans;
            private readonly int[] _digits, _modesTwo;
            private readonly bool[] _modes;
            public MashIfStage(Func<double> nextDouble)
            {
                _count = (int)(3 + nextDouble() * 3);
                _numbers = new EdgeworkDigit[_count];
                _booleans = new EdgeworkBoolean[_count - 1];
                _digits = new int[_count];
                _modes = new bool[_count];
                _modesTwo = new int[_count];
                for (int i = 0; i < _count; i++)
                    _numbers[i] = EdgeworkDigit.Random(nextDouble);
                for (int i = 0; i < _count - 1; i++)
                    _booleans[i] = EdgeworkBoolean.Random(nextDouble);
                for (int i = 0; i < _count - 1; i++)
                {
                    _booleans[i].FillDigits(nextDouble);
                    _digits[i] = (int)(nextDouble() * 10);
                }
                _digits[_count - 1] = (int)(nextDouble() * 10);
                for (int i = 0; i < _count - 1; i++)
                {
                    _booleans[i].Fill(nextDouble);
                    _modesTwo[i] = 1 + (int)(nextDouble() * 3);
                    _numbers[i].Fill(nextDouble);
                    _modes[i] = (int)(nextDouble() * 2) == 1;
                }
                _modesTwo[_count - 1] = 1 + (int)(nextDouble() * 3);
                _numbers[_count - 1].Fill(nextDouble);
                _modes[_count - 1] = (int)(nextDouble() * 2) == 1;
            }
            public override Solution Calculate(KMBombInfo info)
            {
                for (int i = 0; i < _count - 1; i++)
                    if (_booleans[i].Calculate(info))
                        return new Solution(SolutionType.Mash,
                            (byte)(_modes[i] ? _digits[i] : _numbers[i].Calculate(info)),
                            (byte)_modesTwo[i]);
                return new Solution(SolutionType.Mash,
                    (byte)(_modes[_count - 1] ? _digits[_count - 1] : _numbers[_count - 1].Calculate(info)),
                    (byte)_modesTwo[_count - 1]);
            }
            public override string ToString()
            {
                var sb = new StringBuilder("If ")
                   .Append(_booleans[0])
                   .Append(',');
                for (int i = 0; i < _count; i++)
                {
                    sb.Append(' ');
                    if (_modesTwo[i] == 2) sb.Append("double ");
                    else if (_modesTwo[i] == 3) sb.Append("triple ");
                    sb.Append("tap the button labeled ");
                    if (_modes[i]) sb.Append(_digits[i]);
                    else sb.Append("with ").Append(_numbers[i]);
                    if (i < _count - 1) sb.Append(". Otherwise,");
                    if (i < _count - 2) sb.Append(" if ").Append(_booleans[i + 1]).Append(',');
                }
                return sb.Append('.').ToString();
            }
        }
        private class MashStage : Stage
        {
            private readonly EdgeworkDigit _number;
            private readonly int _digit;
            private readonly bool[] _modes;
            public MashStage(Func<double> nextDouble)
            {
                _modes = new bool[2];
                _number = EdgeworkDigit.Random(nextDouble);
                _digit = (int)(nextDouble() * 10);
                _modes[0] = (int)(nextDouble() * 2) == 1;
                _number.Fill(nextDouble);
                _modes[1] = (int)(nextDouble() * 2) == 1;
            }
            public override Solution Calculate(KMBombInfo info)
            {
                return new Solution(SolutionType.Mash,
                    (byte)(_modes[1] ? _digit : _number.Calculate(info)),
                    (byte)(_modes[0] ? 3 : 2));
            }
            public override string ToString()
            {
                var sb = new StringBuilder(_modes[0] ? "Triple" : "Double").Append(" tap the button labeled ");
                if (_modes[1]) sb.Append(_digit);
                else sb.Append("with ").Append(_number);
                return sb.Append('.').ToString();
            }
        }
    }
}
