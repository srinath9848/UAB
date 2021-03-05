using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UAB.DAL.Models;

namespace UAB.UnitTest
{
    public class CommonTestMethod
    {
        internal string RandomValue()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }

        public int GetFirstProjectTypeId()
        {
            using (var context = new UABContext())
            {
                return context.Project.Select(p => p.ProjectTypeId).FirstOrDefault();
            };
        }

        public int GetFirstClintId()
        {
            using (var context = new UABContext())
            {
                return context.Project.Select(p => p.ClientId).FirstOrDefault();
            };
        }
    }
}
