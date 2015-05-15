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
            SpeedTest.Initialize();
            var ID = Table.Data.User.Desc(o => o.ID).ToEntity().ID.GetValueOrDefault();
            Table.Data.User.Where(o => o.ID == ID).Update(new UserVO() { UserName = "zz" });

//            SpeedTest.ConsoleTime("xxxxxxxxxxxx", 1, () =>
//            {
//                for (int i = 0; i < 1; i++)
//                {
//                    Table.Data.User.Where(o => o.ID == ID).Update(new UserVO() { UserName = "zz" });
//                }
//            });


            var context = new Table();
            SpeedTest.ConsoleTime("批量提交", 1, () =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    context.User.Where(o => o.ID == ID).Update(new UserVO() { UserName = "zz" });
                }
                context.SaveChanges();
            });


            SpeedTest.ConsoleTime("单次提交", 1, () =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    Table.Data.User.Where(o => o.ID == ID).Update(new UserVO() { UserName = "zz" });
                }
            });
        }
    }
}
