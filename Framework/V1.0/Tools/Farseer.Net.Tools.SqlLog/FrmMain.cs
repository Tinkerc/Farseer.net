﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FS.Core.Infrastructure;
using FS.Extends;
using FS.Utils;
using Microsoft.Win32;

namespace Farseer.Net.Tools.SqlLog
{
    public partial class FrmMain : Form
    {
        private List<SqlRecordEntity> _sqlRecordList;
        private string vsPath;

        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            btnRefresh.PerformClick();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            Clear();
            LoadSqlLog();

            foreach (var result in _sqlRecordList.GroupBy(o => o.CreateAt.ToString("yy-MM-dd"))) { coxDate.Items.Add(result.Key); }
            foreach (var result in _sqlRecordList.GroupBy(o => o.MethodName)) { coxMethodName.Items.Add(result.Key); }
            foreach (var result in _sqlRecordList.GroupBy(o => o.Name)) { coxName.Items.Add(result.Key); }

            btnSelect.PerformClick();
        }

        /// <summary>
        /// 读取日志
        /// </summary>
        private void LoadSqlLog()
        {
            var path = SysMapPath.AppData;
            const string fileName = "SqlLog.xml";
            _sqlRecordList = Serialize.Load<List<SqlRecordEntity>>(path, fileName) ?? new List<SqlRecordEntity>();
            _sqlRecordList = _sqlRecordList.OrderByDescending(o => o.CreateAt.ToString("yy-MM-dd HH:mm")).ThenByDescending(o => o.UserTime).ThenBy(o => o.Name).ToList();
        }

        private void Clear()
        {
            dgv.Rows.Clear();
            coxDate.Items.Clear();
            coxDate.Items.Add("全部");
            coxDate.SelectedIndex = 0;

            coxMethodName.Items.Clear();
            coxMethodName.Items.Add("全部");
            coxMethodName.SelectedIndex = 0;

            coxName.Items.Clear();
            coxName.Items.Add("全部");
            coxName.SelectedIndex = 0;
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            var selectDate = coxDate.SelectedItem.ToString();
            var selectethodName = coxMethodName.SelectedItem.ToString();
            var selecName = coxName.SelectedItem.ToString();
            dgv.Rows.Clear();
            dgv.Visible = false;
            Task.Factory.StartNew(() =>
            {
                var selectSqlRecordList = _sqlRecordList;
                if (selectDate != "全部") { selectSqlRecordList = selectSqlRecordList.FindAll(o => o.CreateAt.ToString("yy-MM-dd") == selectDate); }
                if (selectethodName != "全部") { selectSqlRecordList = selectSqlRecordList.FindAll(o => o.MethodName == selectethodName); }
                if (selecName != "全部") { selectSqlRecordList = selectSqlRecordList.FindAll(o => o.Name == selecName); }

                // 加载表
                foreach (var sqlRecordEntity in selectSqlRecordList)
                {
                    var sb = new StringBuilder();
                    sqlRecordEntity.SqlParamList.ForEach(o => sb.AppendFormat("{0} = {1} ", o.Name, o.Value));
                    var entity = sqlRecordEntity;
                    Invoke((EventHandler)delegate
                    {
                        dgv.Rows.Add(entity.ID, entity.CreateAt, entity.UserTime, entity.MethodName, entity.LineNo, entity.Name, entity.Sql, sb.ToString());
                    });
                }
                Invoke((EventHandler)delegate
                {
                    dgv.Visible = true;
                });
            });
        }

        private void btnDelLog_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要删除Sql日志吗？", "询问", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No) { return; }
            var path = SysMapPath.AppData;
            const string fileName = "SqlLog.xml";
            File.Delete(path + fileName);
            btnRefresh.PerformClick();
        }

        private void dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) { return; }
            if (dgv.CurrentRow.Index == -1) { return; }
            var index = dgv.CurrentRow.Index;
            var currentSqlRecord = _sqlRecordList.Find(o => o.ID.ToString() == dgv.CurrentRow.Cells[0].Value.ToString());

            textBox1.Text = currentSqlRecord.CreateAt.ToString();
            textBox2.Text = currentSqlRecord.UserTime.ToString();
            textBox3.Text = currentSqlRecord.MethodName;
            textBox4.Text = currentSqlRecord.LineNo.ToString();
            textBox5.Text = currentSqlRecord.Name;
            textBox6.Text = currentSqlRecord.Sql;
            textBox8.Text = currentSqlRecord.FileName;

            textBox7.Clear();
            currentSqlRecord.SqlParamList.ForEach(o => textBox7.AppendText(string.Format("{0} = {1}\r\n", o.Name, o.Value)));
        }

        private void btnOpenVS_Click(object sender, EventArgs e)
        {
            #region 注册表打开
            //try
            //{
            //    // 先获取VS安装路径
            //    if (string.IsNullOrWhiteSpace(vsPath))
            //    {
            //        // 通过注册表，找到VS安装路径
            //        var regKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio");
            //        if (regKey != null)
            //        {
            //            // 找到末尾带_Config名字的Key项
            //            var lstSubKey = regKey.GetSubKeyNames().Where(o => o.EndsWith("_Config")).OrderByDescending(o => o.Split('.')[0].ConvertType(0));

            //            foreach (var openSubKey in lstSubKey.Select(subKey => regKey.OpenSubKey(subKey + @"\Setup\VS\")).Where(openSubKey => openSubKey != null))
            //            {
            //                // 找到安装路径
            //                vsPath = openSubKey.GetValue("EnvironmentPath").ToString();
            //                if (File.Exists(vsPath)) break;
            //                vsPath = string.Empty;
            //            }
            //        }
            //    }

            //    // 用VS打开，并定位行号
            //    if (File.Exists(vsPath)) { Process.Start(vsPath, textBox8.Text + " /command  \"Edit.GoTo " + textBox4.Text + "\""); return; }
            //}
            //catch (Exception ex) { MessageBox.Show(ex.Message); }
            #endregion

            // 无法用注册表找到VS程序时，直接打开源程序文件
            Process.Start(textBox8.Text);
            Thread.Sleep(Convert.ToInt32(3 * 1000));//开起程序后等待
            // Ctrl + g
            SendKeys.SendWait("^g");
            Thread.Sleep(100);
            // 清除现有行数
            SendKeys.SendWait("{BACKSPACE}");
            // 输入行号
            foreach (var c in textBox4.Text)
            {
                SendKeys.Send(c.ToString());
                Thread.Sleep(100);
            }
            // 回车
            SendKeys.SendWait("~");
            SendKeys.SendWait("+{END}");
        }

        private void dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) { return; }
            btnOpenVS_Click(null, null);
        }
    }
}
