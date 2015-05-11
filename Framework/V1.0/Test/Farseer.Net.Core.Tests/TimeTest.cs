using System;
using Demo.PO;
using Demo.VO.Members;
using FS.Core;
using FS.Core.Data;
using FS.Core.Data.Table;
using FS.Core.Infrastructure;
using FS.Utils;
using FS.Utils.Component;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Farseer.Net.Core.Tests
{
    [TestClass]
    public class TimeTest
    {
        [TestMethod]
        public void TestTime()
        {

            var ID = Table.Data.User.Desc(o => o.ID).ToEntity().ID.GetValueOrDefault();


            var where = Table.Data.User.Where(o => o.ID == ID);
            SpeedTest.ConsoleTime("xxxxxxxxxxxx", 1, () =>
            {
                for (int i = 0; i < 1; i++)
                {
                    where.Update(new UserVO() { UserName = "zz" });
                }
            });


            var context = new Table();
            where = context.User.Where(o => o.ID == ID);
            SpeedTest.ConsoleTime("IsMergeCommand", 1, () =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    where.Update(new UserVO() { UserName = "zz" });
                }
                context.SaveChanges();
            });

            where = Table.Data.User.Where(o => o.ID == ID);
            SpeedTest.ConsoleTime("NotIsMerge", 1, () =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    where.Update(new UserVO() { UserName = "zz" });
                }
            });
        }
    }
}
