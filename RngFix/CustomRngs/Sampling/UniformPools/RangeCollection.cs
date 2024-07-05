using RngFix.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RngFix.CustomRngs.Sampling.UniformPools
{
    public class RangeCollection : ICollection<ManRange>
    {
        SortedSet<ManRange> ranges = new SortedSet<ManRange>();
        int count = 0;

        public int Count => count;

        public bool IsReadOnly => false;

        

        public void Add(ManRange newRange)
        {
            if (newRange.Count == 0)
                return;

            var overlappingRanges = new List<ManRange>();

            foreach (var range in ranges)
            {
                if (range.OverlapsOrTouches(newRange))
                {
                    overlappingRanges.Add(range);
                }
            }

            // Remove all overlapping ranges
            foreach (var range in overlappingRanges)
            {
                Remove(range);
            }

            // Merge all overlapping ranges into the new range
            foreach (var range in overlappingRanges)
            {
                newRange = newRange.Merge(range);
            }

            ranges.Add(newRange);
            count += newRange.Count;
        }

        public void Add(int a, int b)
        {
            Add(new ManRange(a, b));
        }



        public void Clear()
        {
            ranges.Clear();
            count = 0;
        }

        public bool Contains(int i)
        {
            foreach (var r in ranges)
            {
                if (r.Contains(i))
                    return true;
            }
            return false;
        }

        public ManRange WhichRangeContains(int i)
        {
            foreach (var r in ranges)
            {
                if (r.Contains(i))
                    return r;
            }
            throw new ArgumentException($"{i} is not contained within {this}");
        }

        public void CascadeShrinkRanges(ManRange range, int by) 
        {
            if (!Contains(range))
                throw new ArgumentException($"{this} does not contain {range}");


            var shrunkR = range.ShrinkEnd(by);

            ranges.Remove(range);
            ranges.Add(shrunkR);

            ShiftRanges(shrunkR.End, -by);

        }

        public void ShiftRanges(int from, int by)
        {
            var greaterRanges = ranges.Where(r => r.Start >= from).ToList();
            foreach (var r in greaterRanges)
            {
                var shiftedR = r.Shift(-by);
                ranges.Remove(r);
                ranges.Add(shiftedR);
            }
        }

        public bool Contains(ManRange item)
        {
            return ranges.Contains(item);
        }

        public void CopyTo(ManRange[] array, int arrayIndex)
        {
            ranges.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ManRange> GetEnumerator()
        {
            return ranges.GetEnumerator();
        }

        public bool Remove(ManRange item)
        {
            var rez = ranges.Remove(item);
            if(rez)
                count -= item.Count;
            return rez;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join(";", ranges);
        }
    }
}
