using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FS.Core.Infrastructure;
using FS.Utils;
using FS.Extends;

namespace Farseer.Net.Tools.SqlLog
{
    public partial class FrmMain : Form
    {
        private List<SqlRecordEntity> _sqlRecordList;
        public FrmMain()
        {
            InitializeComponent();
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

        private void FrmMain_Load(object sender, EventArgs e)
        {
            btnRefresh.PerformClick();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            var selectSqlRecordList = _sqlRecordList;
            if (coxDate.SelectedItem != "全部") { selectSqlRecordList = selectSqlRecordList.FindAll(o => o.CreateAt.ToString("yy-MM-dd") == coxDate.SelectedItem.ToString()); }
            if (coxMethodName.SelectedItem != "全部") { selectSqlRecordList = selectSqlRecordList.FindAll(o => o.MethodName == coxMethodName.SelectedItem.ToString()); }
            if (coxName.SelectedItem != "全部") { selectSqlRecordList = selectSqlRecordList.FindAll(o => o.Name == coxName.SelectedItem.ToString()); }

            // 加载表
            dgv.Rows.Clear();
            foreach (var sqlRecordEntity in selectSqlRecordList)
            {
                var sb = new StringBuilder();
                sqlRecordEntity.SqlParamList.ForEach(o => sb.AppendFormat("{0} = {1} ", o.Name, o.Value));
                dgv.Rows.Add(sqlRecordEntity.ID, sqlRecordEntity.CreateAt, sqlRecordEntity.UserTime, sqlRecordEntity.MethodName, sqlRecordEntity.LineNo, sqlRecordEntity.Name, sqlRecordEntity.Sql, sb.ToString());
            }
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

            textBox7.Clear();
            currentSqlRecord.SqlParamList.ForEach(o => textBox7.AppendText(string.Format("{0} = {1}\r\n", o.Name, o.Value)));
        }
    }
}
