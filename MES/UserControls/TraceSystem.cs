﻿using System;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MES.Core;
using MES.UI;
using System.IO;
using ProductManage.Language.MyLanguageTool;

namespace MES.UserControls
{
    public partial class TraceSystem : UserControl
    {
        private FormMain m_main;

        private int Inum = 1;//行号，索引初始值为1

        private int PageCount;//总页数

        private int AllCount;//总条数

        private int PageSize = 10;//每页的数量

        private int Remain;//总条数%总页数是否为0

        private DataTable m_productTable;

        private string sqlPage;

        private string[] surfaceStringS = new string[] { "OK", "同心度NG", "瑕疵NG" };
        private string[] surfaceStringSEN = new string[] { "OK", "CoaxialityNG", "SurfaceNG" };

        private string[] surfaceStringL = new string[] { "OK", "同心度NG", "焊缝NG" };
        private string[] surfaceStringLEN = new string[] { "OK", "CoaxialityNG", "WeldDepthNG" };

        private string[] lwmStrings = new string[] { "OK", "NG" };

        private string[] coaxialStrings = new string[] { "A:0.8~0.9", "B:0.7~1.0", "C:0.6~0.7", "D:1.0~1.3" };

        public DataTable ExcelProductTable;

        private string m_manualInfo;

        private string m_qcResult;

        private bool m_ifUpdateResult;

        private int m_culture = 1;

        private object[] conditionS = { "检测结果", "条码", "表面成型", "LWM检测", "入库时间" };
        private object[] conditionL = { "检测结果", "条码", "表面成型", "LWM检测", "入库时间" };

        private Stopwatch sw = new Stopwatch();

        private TimeSpan startTime;
        private TimeSpan endTime;
        private TimeSpan countTime;

        /// <summary>
        /// 执最终行查询的sql语句
        /// </summary>
        private string sqlByCondition = string.Empty;

        /// <summary>
        /// 额外的查询条件
        /// </summary>
        private string m_conditionExtra = string.Empty;

        private string m_dbColunmsNames = string.Empty;

        private bool m_weldSuccess = false;

        public TraceSystem()
        {
            InitializeComponent();
        }

        public TraceSystem(FormMain main)
        {
            InitializeComponent();

            m_main = main;
            m_culture = m_main.Culture;
            CultureChange();
            if (main.UseLanguage == 1) m_main.CultureChangeEvent += M_main_CultureChangeEvent;

            m_main.WeldSuccessEvent += M_main_WeldSuccessEvent;

            //设定按字体来缩放控件  
            this.AutoScaleMode = AutoScaleMode.Font;
            //设定字体大小为12px       
            this.Font = new Font("Tahoma", 10F, FontStyle.Regular, GraphicsUnit.Pixel, ((byte)(134)));

            dgvLookBoard.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 15F, FontStyle.Regular, GraphicsUnit.Pixel, 134);
            dgvLookBoard.RowsDefaultCellStyle.Font = new Font("Tahoma", 15F, FontStyle.Regular, GraphicsUnit.Pixel, 134);
        }

        private void TraceSystem_Load(object sender, EventArgs e)
        {
            Init();

        }

        private void Init()
        {
            if (m_main.IsStation_S)
            {
                dgvLookBoard.Columns["colWeldDepth"].Visible = false;
                dgvLookBoard.Columns["colPressure"].HeaderText = "卡盘夹紧力(MPa)";
            }
            else
            {
                dgvLookBoard.Columns["colPressure"].HeaderText = "工件压紧力(MPa)";
            }
            timeCheckStart.Value = timeCheckEnd.Value = DateTime.Now;
            cmbPageSize.SelectedIndex = 0;

            m_dbColunmsNames = "ID,PNo,QCResult,Coaxiality,WeldDepth,Surface,LwmCheck,WeldPower,WeldSpeed,Pressure,Flow,XPos,YPos,ZPos,RPos," +
                "WeldTime,ManualCheck,PType,StorageTime ";

            sqlByCondition = "select * from Product where QCResult is not null order by StorageTime desc;";

            m_conditionExtra = "QCResult is not null";

            //锁定列头不可排序
            foreach (DataGridViewColumn column in dgvLookBoard.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            txtCurPage.Text = txtPageCount.Text = txtAllCount.Text = "1";
            timeCheckStart.Value = DateTime.Now.AddDays(-1);
            timeCheckEnd.Value = DateTime.Now;
        }

        private void M_main_WeldSuccessEvent(object obj, MyEvent e)
        {
            m_weldSuccess = e.WeldSuccess;
            if (m_weldSuccess)
            {
                Invoke(new Action(() =>
                {
                    string sqlLast = " select top 20 * from Product where QCResult is not null order by StorageTime desc;";

                    m_productTable = m_main.DbTool.SelectTable(sqlLast);

                    if (m_productTable != null)
                    {
                        dgvLookBoard.DataSource = m_productTable;
                    }

                    Thread.Sleep(50);

                    UpdateProduction();
                }));
            }
        }

        private void M_main_CultureChangeEvent(object obj, MyEvent e)
        {
            m_culture = e.Culture;
            CultureChange();
        }

        private void CultureChange()
        {
            if (m_culture == 1)
            {
                ResourceCulture.SetCurrentCulture("zh-CN");
                SetResourceCulture();
            }
            else
            {
                ResourceCulture.SetCurrentCulture("en-US");
                SetResourceCulture();
            }

        }

        private void SetResourceCulture()
        {
            cmbSelectCondition.Items.Clear();
            cmbQCResult.Items.Clear();
            if (m_main.IsStation_S)
            {
                conditionS = new object[] { ResourceCulture.GetValue("FinalCheckResult"), ResourceCulture.GetValue("BarCode"),
                ResourceCulture.GetValue("Surface"),ResourceCulture.GetValue("LwmCheck"),ResourceCulture.GetValue("StoreTime")};
                cmbSelectCondition.Items.AddRange(conditionS);

                cmbQCResult.Items.AddRange(surfaceStringS);
            }
            else
            {
                dgvLookBoard.Columns["colWeldDepth"].HeaderText = ResourceCulture.GetValue("WeldDepth") + "(mm)";

                conditionL = new object[] { ResourceCulture.GetValue("FinalCheckResult"), ResourceCulture.GetValue("BarCode"),
                ResourceCulture.GetValue("Surface"),ResourceCulture.GetValue("LwmCheck"),ResourceCulture.GetValue("StoreTime")};
                cmbSelectCondition.Items.AddRange(conditionL);

                cmbQCResult.Items.AddRange(surfaceStringL);
            }
            cmbQCResult.SelectedIndex = cmbSelectCondition.SelectedIndex = 0;

            dgvLookBoard.Columns["colPNo"].HeaderText = ResourceCulture.GetValue("BarCode");
            dgvLookBoard.Columns["colCheckResult"].HeaderText = ResourceCulture.GetValue("FinalResult");
            dgvLookBoard.Columns["colCoaxiality"].HeaderText = ResourceCulture.GetValue("Coaxiality") + "(mm)";
            dgvLookBoard.Columns["colSurface"].HeaderText = ResourceCulture.GetValue("Surface");
            dgvLookBoard.Columns["colLwmCheck"].HeaderText = ResourceCulture.GetValue("LwmResult");
            dgvLookBoard.Columns["colWeldPower"].HeaderText = ResourceCulture.GetValue("WeldPower") + "(W)";
            dgvLookBoard.Columns["colWeldSpeed"].HeaderText = ResourceCulture.GetValue("WeldSpeed") + "(°/s)";
            dgvLookBoard.Columns["colPressure"].HeaderText = ResourceCulture.GetValue("Pressure") + "(MPa)";
            dgvLookBoard.Columns["colFlow"].HeaderText = ResourceCulture.GetValue("ProtectFlow") + "(L/min)";
            dgvLookBoard.Columns["colXPos"].HeaderText = ResourceCulture.GetValue("WeldXPos") + "(mm)";
            dgvLookBoard.Columns["colYPos"].HeaderText = ResourceCulture.GetValue("WeldYPos") + "(mm)";
            dgvLookBoard.Columns["colZPos"].HeaderText = ResourceCulture.GetValue("WeldZPos") + "(mm)";
            dgvLookBoard.Columns["colRPos"].HeaderText = ResourceCulture.GetValue("WeldRPos") + "(°)";
            dgvLookBoard.Columns["colWeldTime"].HeaderText = ResourceCulture.GetValue("WeldTime") + "(S)";
            dgvLookBoard.Columns["colMnaualCheck"].HeaderText = ResourceCulture.GetValue("ManualCheck");
            dgvLookBoard.Columns["colTime"].HeaderText = ResourceCulture.GetValue("StoreTime");

            labTotal.Text = ResourceCulture.GetValue("Production");
            labEach.Text = ResourceCulture.GetValue("PageSize");
            labWhich.Text = ResourceCulture.GetValue("Which");
            labHomePage.Text = ResourceCulture.GetValue("HomePage");
            labLastPage.Text = ResourceCulture.GetValue("LastPage");
            labNextPage.Text = ResourceCulture.GetValue("NextPage");
            labAfterPage.Text = ResourceCulture.GetValue("AfterPage");
            labPage.Text = ResourceCulture.GetValue("Page");
            labPage2.Text = ResourceCulture.GetValue("Page");
            labItem.Text = ResourceCulture.GetValue("Item");
            labSum.Text = ResourceCulture.GetValue("Sum");
            labSum2.Text = ResourceCulture.GetValue("Sum");

            labSelectMethod.Text = ResourceCulture.GetValue("SelectMethod");
            labSelectValue.Text = ResourceCulture.GetValue("QueryValue");
            btnSelectByTerm.Text = ResourceCulture.GetValue("Query");
            btnOutExcel.Text = ResourceCulture.GetValue("OutExcel");
            btnManualCheck.Text = ResourceCulture.GetValue("ManualCheck");

        }

        private string count, pass, ng;

        /// <summary>
        /// 更新产量UI
        /// </summary>
        private void UpdateProduction()
        {
            /*
            sw.Start();
            string s_count = "select count(*) from product";
            string count = m_main.DbTool.GetTableCount(s_count).ToString();
            string pass = m_main.DbTool.GetTableCount("select count(*) from product where QCResult='OK'").ToString();
            string ng = m_main.DbTool.GetTableCount("select count(*) from product where QCResult='NG'").ToString();
            sw.Stop();
            Console.WriteLine("GetTableCount总耗时：" + sw.Elapsed.ToString());
            */

            m_main.DbTool.GetTableCountByProcedure(out count, out pass, out ng);

            txtSumPro.Text = count;
            txtPassPro.Text = pass;
            txtNGPro.Text = ng;
        }

        private void cmbPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePageCount();
        }

        /// <summary>
        /// 更新追溯界面页数数据
        /// </summary>
        private void UpdatePageCount()
        {
            PageSize = Convert.ToInt32(cmbPageSize.SelectedItem);
            PageCount = AllCount % PageSize;

            //只能显示一页,判断是否是整除
            if (PageCount == 0)
            {
                PageCount = AllCount / PageSize;
            }
            else
            {
                PageCount = AllCount / PageSize + 1;
            }

            if (Inum > PageCount && PageCount > 0)
            {
                Inum = PageCount;
            }
            else
            {
                Inum = 1;
            }

            txtCurPage.Text = Inum.ToString();
            txtPageCount.Text = PageCount.ToString();
        }

        private void cmbPageSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        //条件改变
        private void cmbSelectCondition_SelectedIndexChanged(object sender, EventArgs e)
        {
            string trem = cmbSelectCondition.SelectedItem.ToString();

            switch (trem)
            {
                case "入库时间":
                case "StoreTime":
                    timeCheckStart.Visible = true;
                    timeCheckEnd.Visible = true;
                    label6.Visible = true;
                    txtQCResult.Visible = false;
                    cmbQCResult.Visible = false;
                    break;
                case "检测结果":
                case "FinalResult":
                case "LWM检测":
                case "LwmCheck":
                    cmbQCResult.Items.Clear();
                    cmbQCResult.Items.AddRange(lwmStrings);
                    cmbQCResult.SelectedIndex = 0;
                    timeCheckStart.Visible = false;
                    timeCheckEnd.Visible = false;
                    label6.Visible = false;
                    txtQCResult.Visible = false;
                    cmbQCResult.Visible = true;
                    break;
                case "表面成型":
                case "Surface":
                    cmbQCResult.Items.Clear();

                    if (m_culture == 1)
                    {
                        if (m_main.IsStation_S)
                        {
                            cmbQCResult.Items.AddRange(surfaceStringS);
                        }
                        else
                        {
                            cmbQCResult.Items.AddRange(surfaceStringL);
                        }
                    }
                    else
                    {
                        if (m_main.IsStation_S)
                        {
                            cmbQCResult.Items.AddRange(surfaceStringSEN);
                        }
                        else
                        {
                            cmbQCResult.Items.AddRange(surfaceStringLEN);
                        }
                    }

                    cmbQCResult.SelectedIndex = 0;
                    timeCheckStart.Visible = false;
                    timeCheckEnd.Visible = false;
                    label6.Visible = false;
                    txtQCResult.Visible = false;
                    cmbQCResult.Visible = true;
                    break;
                case "同心度":
                case "Coaxiality":
                    cmbQCResult.Items.Clear();
                    cmbQCResult.Items.AddRange(coaxialStrings);
                    cmbQCResult.SelectedIndex = 0;
                    timeCheckStart.Visible = false;
                    timeCheckEnd.Visible = false;
                    label6.Visible = false;
                    txtQCResult.Visible = false;
                    cmbQCResult.Visible = true;
                    break;
                default:
                    timeCheckStart.Visible = false;
                    timeCheckEnd.Visible = false;
                    label6.Visible = false;
                    txtQCResult.Visible = true;
                    cmbQCResult.Visible = false;
                    break;
            }
        }

        //按钮查询
        private void btnSelectByTerm_Click(object sender, EventArgs e)
        {
            //ThreadPool.QueueUserWorkItem(t =>
            //{
            //    Invoke(new Action(() =>
            //    {

            Query();

            //    }));
            //});
        }

        /// <summary>
        /// 条件判断
        /// </summary>
        /// <returns></returns>
        private bool ConditionJudge()
        {
            bool judge = true;

            //TimeSpan start = new TimeSpan(System.DateTime.Now.Ticks);

            if (!m_main.CheckDbConnected()) return false;

            if (cmbSelectCondition.SelectedIndex < 0)
            {
                MessageBox.Show(ResourceCulture.GetValue("PleaseChoseMethod"));
                return false;
            }

            string condition = cmbSelectCondition.SelectedItem.ToString();
            string value = string.Empty;

            if (condition.Equals("检测结果") || condition.Equals("FinalResult")
                || condition.Equals("表面成型") || condition.Equals("Surface")
                || condition.Equals("同心度") || condition.Equals("Coaxiality")
                || condition.Equals("LWM检测") || condition.Equals("LwmCheck"))
            {
                value = cmbQCResult.SelectedItem.ToString();
                if (String.IsNullOrEmpty(value)) return false;
                if (value == "CoaxialityNG")
                {
                    value = "同心度NG";
                }
                else if (value == "WeldDepthNG")
                {
                    value = "焊缝NG";
                }
                else if (value == "SurfaceNG")
                {
                    value = "瑕疵NG";
                }
            }
            else
            {
                value = txtQCResult.Text;
            }

            sqlByCondition = string.Empty;

            switch (condition)
            {
                case "条码":
                case "BarCode":
                    sqlByCondition = "select * from product where PNo = '" + value + "'";
                    m_conditionExtra = "PNo = '" + value + "'";
                    break;
                case "Coaxiality":
                case "同心度"://"A:0.7~1.0", "B:0.6~0.7", "C:1.0~1.3", "D:0.8~0.9" 
                    if (value.Contains("A"))
                    {
                        value = "between 0.8 and 0.9";
                    }
                    else if (value.Contains("B"))
                    {
                        value = "between 0.7 and 1.0";
                    }
                    else if (value.Contains("C"))
                    {
                        value = "between 0.6 and 0.7";
                    }
                    else if (value.Contains("D"))
                    {
                        value = "between 1.0 and 1.3";
                    }
                    sqlByCondition = "select * from product where Coaxiality " + value;
                    m_conditionExtra = "Coaxiality " + value;

                    break;
                case "表面成型":
                case "Surface":
                    sqlByCondition = "select * from product where Surface = '" + value + "'";
                    m_conditionExtra = "Surface = '" + value + "'";

                    break;
                case "检测结果":
                case "FinalResult":
                    sqlByCondition = "select * from product where QCResult = '" + value + "'";
                    m_conditionExtra = "QCResult = '" + value + "'";

                    break;
                case "LWM检测":
                case "LwmCheck":
                    sqlByCondition = "select * from product where LwmCheck = '" + value + "'";
                    m_conditionExtra = "LwmCheck = '" + value + "'";

                    break;
                case "入库时间":
                case "StoreTime":
                    DateTime starTime = timeCheckStart.Value;
                    DateTime endTime = timeCheckEnd.Value;

                    if (starTime > endTime)
                    {
                        MessageBox.Show("起始时间不能大于终止时间！");
                        break;
                    }

                    sqlByCondition = "select * from product where StorageTime between '" + starTime + "' and '" + endTime + "' ";
                    m_conditionExtra = "StorageTime between '" + starTime + "' and '" + endTime + "' ";
                    break;
                default:
                    break;
            }

            //TimeSpan end = new TimeSpan(DateTime.Now.Ticks);
            //TimeSpan use = start.Subtract(end).Duration();
            //Console.WriteLine("判断耗时：" + use.TotalMilliseconds.ToString("0.000") + "s");

            return judge;
        }

        /// <summary>
        /// 根据查询条件更新看板数据
        /// </summary>
        /// <param name="sql"></param>
        private void UpdateLookBoard(string sql)
        {
            string sqlCount = String.Format("select count(*) from ({0}) t", sql);

            AllCount = m_main.DbTool.GetTableCount(sqlCount); //m_productTable.Rows.Count;//更新pagecount

            //更新总条数
            txtAllCount.Text = AllCount.ToString();

            Inum = 1;
            Remain = AllCount % PageSize;
            txtCurPage.Text = Inum.ToString();

            UpdatePageCount();

            //分页显示数据
            ShowPage(Inum, PageSize);

        }

        /// <summary>
        /// 执行查询
        /// </summary>
        private void Query()
        {
            startTime = new TimeSpan(DateTime.Now.Ticks);

            if (!ConditionJudge()) return;

            m_main.AddTips("正在查询数据，请稍等...", false);

            UpdateLookBoard(sqlByCondition);

            if (m_productTable == null || m_productTable.Rows.Count < 1)
            {
                txtAllCount.Text = "1";
                txtPageCount.Text = "1";
                txtCurPage.Text = "1";
                MessageBox.Show(ResourceCulture.GetValue("NotData"));
            }

            UpdateProduction();

            endTime = new TimeSpan(DateTime.Now.Ticks);
            countTime = startTime.Subtract(endTime).Duration();
            Console.WriteLine("Query总耗时：" + countTime.TotalSeconds + "s");
            m_main.AddTips("查询成功，耗时：" + countTime.TotalSeconds.ToString("0.000") + "s", false);
        }

        //导出Excel
        private void btnOutExcel_Click(object sender, EventArgs e)
        {
            ButtonOutExcel(sqlByCondition);
        }

        private void ButtonOutExcel(string sqlSelectFinal)
        {
            if (m_productTable == null || m_productTable.Rows.Count == 0)
            {
                dgvLookBoard.DataSource = m_productTable;
                MessageBox.Show(ResourceCulture.GetValue("NotData"));
                return;
            }

            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Title = "保存导出文件";
            saveFile.Filter = "导出数据文件(.csv) | *.csv";
            saveFile.FilterIndex = 1;
            //saveFile.InitialDirectory = Application.StartupPath;
            //saveFile.RestoreDirectory = true;
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                m_main.AddTips("正在导出数据，请稍等...", false);

                ThreadPool.QueueUserWorkItem((t =>
                {
                    ExcelProductTable = m_main.DbTool.SelectTable(sqlSelectFinal);
                    SetCsvFile(saveFile.FileName);
                }));
            }
        }

        /// <summary>
        /// 导出CSV文件
        /// </summary>
        /// <param name="fileName"></param>
        private void SetCsvFile(string fileName)
        {
            try
            {
                startTime = new TimeSpan(System.DateTime.Now.Ticks);

                #region FileStream 超高速导出，无法设置样式

                FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, Encoding.Default);//Encoding.GetEncoding("UTF-8")

                for (int col = 1; col < ExcelProductTable.Columns.Count; col++)
                {
                    //string colName = ExcelProductTable.Columns[col].ColumnName + ",";
                    string headName = dgvLookBoard.Columns[col].HeaderText + ",";

                    if (headName.Contains("产品类型") || headName.Contains("PType")) continue;
                    if (m_main.IsStation_S) if (headName.Contains("焊缝高度差(mm)") || headName.Contains("WeldDepth(mm)")) continue;

                    sw.Write(headName);
                    //if (col < 1)
                    //{
                    //    sw.Write(m_productTable.Columns[col].ColumnName + ",");//产品编号
                    //}
                }

                sw.WriteLine(String.Empty);

                for (int row = 0; row < ExcelProductTable.Rows.Count; row++)
                {
                    //sw.Write((row + 1).ToString() + ",");
                    for (int col = 1; col < ExcelProductTable.Columns.Count; col++)
                    {
                        //string str = String.Format("\t{0}", ExcelProductTable.Rows[row][col]);

                        string headName = dgvLookBoard.Columns[col].HeaderText;

                        //不导出产品类型列
                        if (headName.Contains("产品类型") || headName.Contains("PType")/*col == 17*/) continue;
                        if (m_main.IsStation_S) if (headName.Contains("焊缝高度差(mm)") || headName.Contains("WeldDepth(mm)")) continue;

                        sw.Write(String.Format("\t{0}", ExcelProductTable.Rows[row][col]) + ",");
                    }
                    sw.Write("\n");
                }

                sw.Close();
                fs.Close();

                #endregion

                endTime = new TimeSpan(DateTime.Now.Ticks);
                countTime = startTime.Subtract(endTime).Duration();

                MessageBox.Show(ResourceCulture.GetValue("OutExcelOK"));
                //Console.WriteLine("操作耗时:" + countTime.TotalSeconds + " s"); //TotalMilliseconds.ToString();
                m_main.AddTips(string.Format("导出成功，耗时:{0} s", countTime.TotalSeconds.ToString("0.000")), false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("导出失败，失败原因：" + ex.Message, "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                m_main.LogNetProgramer.WriteError("异常", "导出Excel失败---->" + ex.Message);
            }
        }

        //人工干预
        private void btnManualCheck_Click(object sender, EventArgs e)
        {
            if (!m_main.CheckUserAuth())
            {
                return;
            }

            DataGridViewCell curCell = dgvLookBoard.CurrentCell;

            if (curCell != null)
            {
                //int curIndex = dgvLookBoard.CurrentRow.Index;
                //int rowIndex = curCell.RowIndex;

                int colIndex = curCell.ColumnIndex;
                string colName = dgvLookBoard.Columns[colIndex].HeaderText;
                if (colName.Equals("人工干预") || colName.Equals("ManualCheck"))
                {
                    bool boo = dgvLookBoard.SelectedCells.IsReadOnly;
                }

                object obj2 = dgvLookBoard.SelectedCells[1].Value;

                object obj3 = dgvLookBoard.SelectedCells[16].Value;

                object obj4 = dgvLookBoard.SelectedCells[2].Value;

                string pno = string.Empty;
                string manual = string.Empty;
                string result = string.Empty;

                if (obj2 != null)
                {
                    pno = obj2.ToString();
                }

                if (obj3 != null)
                {
                    manual = obj3.ToString();
                }
                if (obj4 != null)
                {
                    result = obj4.ToString();
                }

                ManualCheckForm form = new ManualCheckForm(m_culture, manual, result);
                form.ModifyManual += GetManualInfo;
                form.ShowDialog();

                if (!m_ifUpdateResult) return;

                string sql = "update product set QCResult='" + m_qcResult + "' ,ManualCheck='" + m_manualInfo + "' where PNo='" + pno + "'";

                int c = m_main.DbTool.TransactionTable(sql);

                if (c < 0)
                {
                    MessageBox.Show(ResourceCulture.GetValue("UpdateFail"));
                    return;
                }
                else
                {
                    MessageBox.Show(ResourceCulture.GetValue("UpdateSuccess"));
                }

                UpdateCurrentPage();
            }
        }

        public void GetManualInfo(object sender, MyEvent e)
        {
            m_ifUpdateResult = e.IfUpdateResult;
            m_manualInfo = e.ManualInfo;
            m_qcResult = e.QCResult.ToString();
        }

        /// <summary>
        /// 更新人工干预后的数据
        /// </summary>
        private void UpdateCurrentPage()
        {
            Remain = AllCount % PageSize;
            Inum = Convert.ToInt32(txtCurPage.Text);
            if (Inum == PageCount && Remain != 0)
            {
                ShowPage(Inum, Remain);
            }
            else
            {
                ShowPage(Inum, PageSize);
            }
        }

        //分页显示数据
        private void ShowPage(int Inum, int pageSize)
        {
            sqlPage = string.Empty;

            if (Remain != 0 && Inum == PageCount)
            {
                sqlPage = "select top " + Remain + " * from Product where Id not in (select top " + Convert.ToInt32(cmbPageSize.SelectedItem) * (Inum - 1)
                          + " Id from Product where " + m_conditionExtra + " order by StorageTime desc )" + " and " + m_conditionExtra + " order by StorageTime desc";
            }
            else
            {
                sqlPage = "select top " + pageSize + " * from Product where Id not in (select top " + pageSize * (Inum - 1)
                          + " Id from Product where " + m_conditionExtra + " order by StorageTime desc )" + " and " + m_conditionExtra + " order by StorageTime desc";
            }

            m_productTable = m_main.DbTool.SelectTable(sqlPage);

            if (m_productTable != null)
            {
                dgvLookBoard.DataSource = m_productTable;
            }
        }

        //首页
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (m_productTable == null || m_productTable.Rows.Count < 1) return;

            Inum = 1;

            ShowPage(Inum, PageSize);

            txtCurPage.Text = Inum.ToString();
        }

        //上一页
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (m_productTable == null || m_productTable.Rows.Count < 1) return;

            Inum--;
            if (Inum > 0)                     //如果当前不是首页
            {
                ShowPage(Inum, PageSize);    //显示上一页记录
            }
            else
            {
                MessageBox.Show(ResourceCulture.GetValue("IsFirstPage"));
                Inum = 1;
                return;
            }
            txtCurPage.Text = Inum.ToString();
        }

        //下一页
        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (m_productTable == null || m_productTable.Rows.Count < 1) return;

            Inum++;

            if (Inum <= PageCount)              //如果没有超出总页数
            {
                //ShowPage(Inum, PageSize);

                Remain = AllCount % PageSize;
                if (Inum == PageCount && Remain != 0)
                {
                    ShowPage(Inum, Remain);
                }
                else
                {
                    ShowPage(Inum, PageSize);
                }
            }
            else
            {
                MessageBox.Show(ResourceCulture.GetValue("IsLastPage"));
                Inum = PageCount;
                return;
            }
            txtCurPage.Text = Inum.ToString();
        }

        //末页
        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (m_productTable == null || m_productTable.Rows.Count < 1) return;

            Inum = PageCount;
            Remain = AllCount % PageSize;
            if (Remain != 0)
            {
                ShowPage(Inum, Remain);
            }
            else
            {
                ShowPage(Inum, PageSize);
            }

            txtCurPage.Text = Inum.ToString();
        }

        //数据发送变化
        private void dgvLookBoard_DataSourceChanged(object sender, EventArgs e)
        {
            if (m_productTable == null)
            {
                return;
            }
            try
            {
                SetDGVStyle(dgvLookBoard);

                for (int i = 0; i < dgvLookBoard.RowCount - 1; i++)
                {
                    DataGridViewRow dgvRow = dgvLookBoard.Rows[i];

                    string no = dgvRow.Cells["colPNo"].Value.ToString();
                    string value = dgvRow.Cells["colCheckResult"].Value.ToString();
                    if (!String.IsNullOrEmpty(value))
                    {
                        if (value.Equals("OK"))
                        {
                            //dgvLookBoard.Rows[i].DefaultCellStyle.BackColor = Color.LawnGreen;
                            dgvRow.Cells["colCheckResult"].Style.BackColor = Color.LawnGreen;
                        }
                        else if (value.Equals("NG"))
                        {
                            //dgvLookBoard.Rows[i].DefaultCellStyle.BackColor = Color.OrangeRed;
                            dgvRow.Cells["colCheckResult"].Style.BackColor = Color.OrangeRed;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_main.LogNetProgramer.WriteError("异常", "追溯系统更新异常-->" + ex.Message);
            }
        }

        private void SetDGVStyle(DataGridView dgv)
        {
            if (dgv.Rows.Count != 0)
            {
                for (int i = 0; i < dgv.Rows.Count; i++)
                {
                    if ((i + 1) % 2 == 0)
                    {
                        dgv.Rows[i].DefaultCellStyle.BackColor = Color.White;
                    }
                    else
                    {
                        dgv.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(224, 254, 254);
                    }
                }
            }
        }
    }
}