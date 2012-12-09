using System;
using System.Collections.Generic;
using System.Text;

namespace NumericHepler
{
    public static class NumericHelper
    {
        public static int ReverseNumber(int num, int numWidth)
        {
            string binaryForm = IntToBinaryString(num, numWidth);
            return Convert.ToInt32(binaryForm, 2);
        }

        public static string IntToBinaryString(int num, int numWidth)
        {
            List<char> resultList = new List<char>();
            char[] result = new char[numWidth];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = '0';
            }
            if (num == 1 || num == 0)
            {
                result[result.Length - 1] = num.ToString()[0];
            }
            else
            {
                int i = 1;
                while (!(num == 1 || num == 0))
                {
                    result[numWidth - i++] = (num % 2).ToString()[0];
                    num /= 2;
                }
                result[numWidth - i] = (num % 2).ToString()[0];
            }

            resultList.AddRange(result);
            resultList.Reverse();
            StringBuilder binaryForm = new StringBuilder();
            foreach (var n in resultList)
            {
                binaryForm.Append(n);
            }
            return binaryForm.ToString();
        }
    }
}
