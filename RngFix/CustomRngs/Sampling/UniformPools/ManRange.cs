using System;
using System.Collections.Generic;
using System.Text;

namespace RngFix.CustomRngs.Sampling.UniformPools
{
    public readonly struct ManRange : IComparable<ManRange>
    {
        public int Start { get; }
        public int End { get; }

        public int Count { get => End - Start; }

        public ManRange(int start, int end)
        {
            if (start > end)
            {
                throw new ArgumentException("Start must be less or equal to End.");
            }
            Start = start;
            End = end;
        }

        public bool Contains(int value)
        {
            return Start <= value && value < End;
        }

        public bool OverlapsOrTouches(ManRange other)
        {
            return !(End <= other.Start || Start >= other.End);
        }

        public int CompareTo(ManRange other)
        {
            if (End <= other.Start) return -1;
            if (Start > other.End) return 1;
            return 0;
        }

        public ManRange Merge(ManRange other)
        {
            if (!OverlapsOrTouches(other))
                throw new ArgumentException($"{this} does not overlap with {other}");
            return new ManRange(Math.Min(Start, other.Start), Math.Max(End, other.End));
        }

        public ManRange ShrinkEnd(int by)
        {
            if (by <= 0)
                throw new ArgumentException($"{by} must be greater than 0");
            return new ManRange(Start, End - by);
        }

        public ManRange Shift(int by)
        {
            return new ManRange(Start + by, End + by);
        }

        public override string ToString()
        {
            return $"[{Start}, {End})";
        }
    }
}
