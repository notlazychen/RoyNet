using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyNet.Common
{
    public static class MathUtils
    {
        private static readonly Random Random = new Random();

        public static void Xor(byte[] bs, int offset, int size, byte[] ks)
        {
            if (ks != null && ks.Length > 0)
            {
                for (int i = 0; i < size; i++)
                {
                    bs[offset + i] = (byte)(bs[offset + i] ^ ks[i % ks.Length]);
                }
            }
        }

        public static void Xor(byte[] bs, byte[] ks)
        {
            if (ks != null && ks.Length > 0)
            {
                for (int i = 0; i < bs.Length; i++)
                {
                    bs[i] = (byte)(bs[i] ^ ks[i % ks.Length]);
                }
            }
        }

        public static void Xor(ArraySegment<byte> bs, byte[] ks)
        {
            if (ks != null && ks.Length > 0)
            {
                for (int i = 0; i < bs.Count; i++)
                {
                    bs.Array[i + bs.Offset] = (byte)(bs.Array[i + bs.Offset] ^ ks[i % ks.Length]);
                }
            }
        }
        public static short ToShort(byte[] b, int index)
        {
            return (short)(((b[index + 0] << 8) | b[index + 1] & 0xff));
        }

        public static void PutShort(byte[] b, short s, int index)
        {
            b[index + 0] = (byte)(s >> 8);
            b[index + 1] = (byte)(s >> 0);
        }

        public static bool ContainsSameValue<T>(this IEnumerable<T> src)
        {
            int same;
            foreach (T element in src)
            {
                same = 0;
                foreach (T elementOther in src)
                {
                    if (element.Equals(elementOther))
                    {
                        same++;
                        if (same > 1)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static int RandomNumber(int min, int max)
        {
            return Random.Next(min, max);
        }

        public static int RandomIndex(int[] weight)
        {
            int sum = 0;
            foreach (int item in weight)
            {
                sum += item;
            }

            int rand = Random.Next(0, sum);
            int cur = 0;
            for (int i = 0; i < weight.Length; i++)
            {
                cur += weight[i];
                if (rand < cur)
                {
                    return i;
                }
            }
            return -1;
        }

        public static T RandomElement<T>(IEnumerable<T> elements, Func<T, int> weightFunc)
            where T : class
        {
            int sum = 0;
            int length = 0;
            foreach (T element in elements)
            {
                sum += weightFunc(element);
                length++;
            }

            int rand = Random.Next(0, sum);
            int cur = 0;
            foreach (T element in elements)
            {
                cur += weightFunc(element);
                if (rand < cur)
                {
                    return element;
                }
            }
            return null;
        }
    }
}
