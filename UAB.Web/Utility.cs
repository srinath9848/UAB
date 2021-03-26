using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UAB
{
    public static class Utility
    {
        public static Dictionary<int, string> GetDxCodes(string dxCodes, int count)
        {
            List<string> lstdxCodes = dxCodes.Split(',').ToList();

            Dictionary<int, string> dictDxCodes = new Dictionary<int, string>();


            for (int i = 0; i < count; i++)
            {
                if (i < lstdxCodes.Count())
                    dictDxCodes.Add(i + 1, lstdxCodes[i]);
                else
                    dictDxCodes.Add(i + 1, "NA");
            }

            return dictDxCodes;
        }

        public static Dictionary<int, string> GetQAOrShadowQADxCodes(string dxCodes)
        {
            if (dxCodes == "")
                return null;

            List<string> lstDxCodes = dxCodes.Split('|').ToList();

            Dictionary<int, string> dictDxCodes = new Dictionary<int, string>();

            int rno = 1;

            foreach (var item in lstDxCodes)
            {
                string[] strItem = item.Split('^');

                if (rno != Convert.ToInt16(strItem[0]))
                {
                    for (int j = rno; j < Convert.ToInt16(strItem[0]); j++)
                    {
                        dictDxCodes.Add(j, "");
                        rno += 1;
                    }
                }

                dictDxCodes.Add(Convert.ToInt16(strItem[0]), strItem[1]);
                rno += 1;
            }

            return dictDxCodes;
        }

        public static Dictionary<int, string> GetQADxCodes(string QAdxCodes,int? count)
        {
            if (QAdxCodes == "")
                return null;

            List<string> lstQADxCodes = QAdxCodes.Split('|').ToList();

            Dictionary<int, string> dictQADxCodes = new Dictionary<int, string>();

            int rno = 1;

            foreach (var item in lstQADxCodes)
            {
                string[] strItem = item.Split('^');

                if (rno != Convert.ToInt16(strItem[0]))
                {
                    for (int j = rno; j < Convert.ToInt16(strItem[0]); j++)
                    {
                        dictQADxCodes.Add(j, "");
                        rno += 1;
                    }
                }

                dictQADxCodes.Add(Convert.ToInt16(strItem[0]), strItem[1]);
                rno += 1;
            }
            if(count> dictQADxCodes.Count)
            {
                int j = dictQADxCodes.Count;
                for(int i = j; i < count; i++)
                {
                    dictQADxCodes.Add(j+1, "");
                }
            }

            return dictQADxCodes;
        }

        public static Dictionary<int, string> GetQAOrShadowQADxRemarks(string dxRemarks, int count = 0)
        {
            if (dxRemarks == "")
                return null;

            List<string> lstDxRemarks = dxRemarks.Split('|').ToList();

            Dictionary<int, string> dictDxRemarks = new Dictionary<int, string>();

            int rno = 1;
            foreach (var item in lstDxRemarks)
            {
                string[] strItem = item.Split('^');

                if (rno != Convert.ToInt16(strItem[0]))
                {
                    for (int j = rno; j < Convert.ToInt16(strItem[0]); j++)
                    {
                        dictDxRemarks.Add(j, "");
                        rno += 1;
                    }
                }
                dictDxRemarks.Add(Convert.ToInt16(strItem[0]), strItem[1]);
                rno += 1;
            }
            if (rno <= count)
            {
                for (int j = rno; j <= count; j++)
                {
                    dictDxRemarks.Add(j, "");
                    rno += 1;
                }
            }

            return dictDxRemarks;
        }

        public static Dictionary<int, string> GetDxCodes(string dxCodes)
        {
            List<string> lstdxCodes = dxCodes.Split(',').ToList();

            Dictionary<int, string> dictDxCodes = new Dictionary<int, string>();

            for (int i = 0; i < lstdxCodes.Count(); i++)
            {
                dictDxCodes.Add(i + 1, lstdxCodes[i]);
            }

            return dictDxCodes;
        }

        public static Dictionary<int, string> AddEmptyRows(Dictionary<int, string> dxCodes, int maxCount, string value)
        {
            if (dxCodes == null)
                dxCodes = new Dictionary<int, string>();

            for (int i = dxCodes.Count() + 1; i <= maxCount; i++)
            {
                dxCodes.Add(i, value);
            }
            return dxCodes;
        }
    }
}
