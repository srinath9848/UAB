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

        public static Dictionary<int, string> GetQADxCodes(string QAdxCodes)
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

            return dictQADxCodes;
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
                    dictQADxCodes.Add(j+1, "NA");
                }
            }

            return dictQADxCodes;
        }

        public static Dictionary<int, string> GetQADxRemarks(string QAdxRemarks, int count = 0)
        {
            if (QAdxRemarks == "")
                return null;

            List<string> lstQADxRemarks = QAdxRemarks.Split('|').ToList();

            Dictionary<int, string> dictQADxRemarks = new Dictionary<int, string>();

            int rno = 1;
            foreach (var item in lstQADxRemarks)
            {
                string[] strItem = item.Split('^');

                if (rno != Convert.ToInt16(strItem[0]))
                {
                    for (int j = rno; j < Convert.ToInt16(strItem[0]); j++)
                    {
                        dictQADxRemarks.Add(j, "");
                        rno += 1;
                    }
                }
                dictQADxRemarks.Add(Convert.ToInt16(strItem[0]), strItem[1]);
                rno += 1;
            }
            if (rno <= count)
            {
                for (int j = rno; j <= count; j++)
                {
                    dictQADxRemarks.Add(j, "");
                    rno += 1;
                }
            }

            return dictQADxRemarks;
        }

        public static Dictionary<int, string> GetShadowQADxCodes(string ShadowQAdxCodes)
        {
            if (ShadowQAdxCodes == "")
                return null;

            List<string> lstShadowQADxCodes = ShadowQAdxCodes.Split('|').ToList();

            Dictionary<int, string> dictShadowQADxCodes = new Dictionary<int, string>();

            int rno = 1;

            foreach (var item in lstShadowQADxCodes)
            {
                string[] strItem = item.Split('^');

                if (rno != Convert.ToInt16(strItem[0]))
                {
                    for (int j = rno; j < Convert.ToInt16(strItem[0]); j++)
                    {
                        dictShadowQADxCodes.Add(j, "");
                        rno += 1;
                    }
                }

                dictShadowQADxCodes.Add(Convert.ToInt16(strItem[0]), strItem[1]);
                rno += 1;
            }

            return dictShadowQADxCodes;
        }

        public static Dictionary<int, string> GetShadowQADxRemarks(string ShadowQAdxRemarks, int count = 0)
        {
            if (ShadowQAdxRemarks == "")
                return null;

            List<string> lstShadowQADxRemarks = ShadowQAdxRemarks.Split('|').ToList();

            Dictionary<int, string> dictShadowQADxRemarks = new Dictionary<int, string>();

            int rno = 1;
            foreach (var item in lstShadowQADxRemarks)
            {
                string[] strItem = item.Split('^');

                if (rno != Convert.ToInt16(strItem[0]))
                {
                    for (int j = rno; j < Convert.ToInt16(strItem[0]); j++)
                    {
                        dictShadowQADxRemarks.Add(j, "");
                        rno += 1;
                    }
                }
                dictShadowQADxRemarks.Add(Convert.ToInt16(strItem[0]), strItem[1]);
                rno += 1;
            }
            if (rno <= count)
            {
                for (int j = rno; j <= count; j++)
                {
                    dictShadowQADxRemarks.Add(j, "");
                    rno += 1;
                }
            }

            return dictShadowQADxRemarks;
        }
    }
}
