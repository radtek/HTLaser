﻿using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using MES.Core;
using MES.DAL;
using Opc.Ua;
using OpcUaHelper;
using ProductManage.Language.MyLanguageTool;

namespace MES.UserControls
{
    public partial class CollectingSystem : UserControl
    {
        private FormMain m_main;

        private DBTool m_tool;

        private bool m_isThreadRun;

        private OpcUaClient m_opcUaClient;

        //反馈读取焊接参数结果
        private string ReadWeldParmBack = "ns=3;s=\"WeldPara\".\"WeldParmBackResult\"";

        //保存焊接参数结果
        private string SaveWeldParmBack = "ns=3;s=\"WeldPara\".\"SaveParmBackResult\"";

        private string[] nodes = {
            "ns=3;s=\"WeldPara\".\"Xpos\"",
            "ns=3;s=\"WeldPara\".\"Ypos\"",
            "ns=3;s=\"WeldPara\".\"Zpos\"",
            "ns=3;s=\"WeldPara\".\"Wpos\"",
            "ns=3;s=\"WeldPara\".\"WeldPower\"",
            "ns=3;s=\"WeldPara\".\"Pressure\"",
            "ns=3;s=\"WeldPara\".\"Flow\"",
            "ns=3;s=\"WeldPara\".\"WeldSpeed\"",
            "ns=3;s=\"WeldPara\".\"WeldTime\""
        };

        private object[] datas = {
            "98o1235216",
            100,200,300,60,
            10,10,10,10,10
        };

        private string m_barcode;

        private double m_XPos, m_YPos, m_ZPos, m_RPos, m_press, m_flow, m_weldPower;

        private int m_speed, m_time;

        private bool m_startSave = false;//是否开始保存数据

        private int m_culture = 1;

        private bool b_windowShow = true;

        public CollectingSystem()
        {
            InitializeComponent();
        }

        public CollectingSystem(FormMain main)
        {
            InitializeComponent();

            //设定按字体来缩放控件  
            this.AutoScaleMode = AutoScaleMode.Font;
            //设定字体大小为12px       
            this.Font = new Font("Tahoma", 12F, FontStyle.Regular, GraphicsUnit.Pixel, 134);

            m_main = main;
            m_culture = m_main.Culture;
            CultureChange();
            m_main.BarCodeChange += BarCodeChange;//条码改变
            if (m_main.UseLanguage == 1) m_main.CultureChangeEvent += M_main_CultureChangeEvent;//语言改变
            m_main.WindowStateEvent += M_main_WindowStateEvent;//程序关闭

        }

        private void M_main_WindowStateEvent(object obj, MyEvent e)
        {
            b_windowShow = e.IsWindowShow;
        }

        private void CollectDataControl_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void Init()
        {
            m_tool = m_main.DbTool;
            m_opcUaClient = m_main.OpcUaClient;

            Thread t_collectThread = new Thread(Collecting);
            t_collectThread.IsBackground = true;
            t_collectThread.Start();
            m_isThreadRun = true;

            Thread t_updateWeldParam = new Thread(UpdateDbData);
            t_updateWeldParam.IsBackground = true;
            //t_updateWeldParam.Start();
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
            labBarCode.Text = ResourceCulture.GetValue("BarCode");
            labWeldPower.Text = ResourceCulture.GetValue("WeldPower");
            labWeldSpeed.Text = ResourceCulture.GetValue("WeldSpeed");
            labProtectFlow.Text = ResourceCulture.GetValue("ProtectFlow");
            labWeldTime.Text = ResourceCulture.GetValue("WeldTime");
            labX.Text = ResourceCulture.GetValue("WeldXPos");
            labY.Text = ResourceCulture.GetValue("WeldYPos");
            labZ.Text = ResourceCulture.GetValue("WeldZPos");
            labR.Text = ResourceCulture.GetValue("WeldRPos");
            if (m_main.IsStation_S) labPressName.Text = ResourceCulture.GetValue("ChuckClamp");
            else labPressName.Text = ResourceCulture.GetValue("WorkpiecePressure");

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //if (!m_isThreadRun)
            //{
            //    btnStart.UIText = "停止";

            //    Thread collectThread = new Thread(Collecting);
            //    collectThread.IsBackground = true;
            //    m_isThreadRun = true;
            //    collectThread.Start();
            //}
            //else
            //{
            //    btnStart.UIText = "采集";
            //    m_isThreadRun = false;
            //}
        }

        private ArrayList objs = new ArrayList();

        /// <summary>
        /// 批量采集参数
        /// </summary>
        private void Collecting()
        {
            while (b_windowShow)
            {
                if (!m_isThreadRun || m_opcUaClient == null || !m_opcUaClient.Connected) break;

                try
                {
                    // 批量读取： 因为不能保证读取的节点类型一致，所以只提供统一的DataValue读取，每个节点需要单独解析
                    objs?.Clear();
                    int index = 0;
                    foreach (DataValue value in m_opcUaClient.ReadNodes(nodes))
                    {
                        // 获取到了值，具体的每个变量的解析参照上面类型不确定的解析
                        object data = value.WrappedValue.Value;

                        // 单独操作data
                        if (data != null)
                        {
                            objs.Add(data);
                        }
                        else
                        {
                            m_main.LogNetProgramer.WriteError("采集异常", "节点名：" + nodes[index] + ",读取失败！");
                            //m_isThreadRun = false;
                            //btnStart.UIText = "采集";
                            break;
                        }
                        index++;
                    }
                }
                catch (Exception ex)
                {
                    m_isThreadRun = false;
                    CommonLibrary.Log.LogHelper.WriteLog("OPC采集焊接参数异常", ex);
                }

                if (m_isThreadRun && objs.Count > 0)
                {
                    Invoke(new Action(() =>
                    {
                        txtXPos.Text = Convert.ToDouble(objs[0]).ToString("0.000");
                        txtYPos.Text = Convert.ToDouble(objs[1]).ToString("0.000");
                        txtZPos.Text = Convert.ToDouble(objs[2]).ToString("0.000");
                        txtRPos.Text = Convert.ToDouble(objs[3]).ToString("0.000");
                        txtWeldPower.Text = Convert.ToDouble(objs[4]).ToString("0.000");
                        txtPressure.Text = Convert.ToDouble(objs[5]).ToString("0.000");
                        txtFlow.Text = Convert.ToDouble(objs[6]).ToString("0.000");
                        txtWeldTime.Text = Convert.ToDouble(objs[8]).ToString("0.000");
                        txtSpeed.Text = objs[7].ToString();
                    }));

                    // if (!String.IsNullOrEmpty(m_barcode)) m_startSave = true;
                }
                Thread.Sleep(1000);
            }
        }

        private void UpdateDbData()
        {
            while (b_windowShow)
            {
                if (m_startSave && m_main.DeviceState != 2)
                {
                    m_barcode = txtBarCode.Text;
                    m_XPos = Convert.ToDouble(txtXPos.Text);
                    m_YPos = Convert.ToDouble(txtYPos.Text);
                    m_ZPos = Convert.ToDouble(txtZPos.Text);
                    m_RPos = Convert.ToDouble(txtRPos.Text);
                    m_time = Convert.ToInt32(txtWeldTime.Text);
                    m_weldPower = Convert.ToDouble(txtWeldPower.Text);
                    m_press = Convert.ToDouble(txtPressure.Text);
                    m_flow = Convert.ToDouble(txtFlow.Text);
                    m_speed = Convert.ToInt32(txtSpeed.Text);

                    //Stopwatch sw = Stopwatch.StartNew();

                    bool boo = SaveWeldParam(m_weldPower, m_speed, m_XPos, m_YPos, m_ZPos, m_RPos, m_time, m_press, m_flow, m_barcode);

                    //sw.Stop();
                    //Console.WriteLine("保存焊接参数耗时：" + sw.ElapsedMilliseconds + "ms");

                    if (boo)
                    {
                        m_startSave = !boo;

                        //保存成功
                        //SendSign(SaveWeldParmBack, 1);
                    }
                    else
                    {
                        //保存失败
                        //SendSign(SaveWeldParmBack, 2);
                    }
                }

                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 保存焊接参数并显示数据
        /// </summary>
        /// <param name="power">焊接功率</param>
        /// <param name="speed">焊接转速</param>
        /// <param name="pos">XYZR坐标</param>
        /// <param name="time">焊接时间</param>
        /// <param name="pressure">保护气体压力值</param>
        /// <param name="flow">保护气体流量值</param>
        /// <param name="pno">设备编号</param>
        private bool SaveWeldParam(double power, int speed, double xpos, double ypos, double zpos, double rpos,
            int time, double pressure, double flow, string pno)
        {
            string sql = "UPDATE Product SET XPos=@xpos,YPos=@ypos,ZPos=@zpos,RPos=@rpos WHERE PNo=@pno";

            SqlParameter[] sqlParameters =
            {
                new SqlParameter {ParameterName = "@xpos", SqlDbType = SqlDbType.Decimal, SqlValue = xpos},
                new SqlParameter {ParameterName = "@ypos", SqlDbType = SqlDbType.Decimal, SqlValue = ypos},
                new SqlParameter {ParameterName = "@zpos", SqlDbType = SqlDbType.Decimal, SqlValue = zpos},
                new SqlParameter {ParameterName = "@rpos", SqlDbType = SqlDbType.Decimal, SqlValue = rpos},
                new SqlParameter {ParameterName = "@pno", SqlDbType = SqlDbType.NVarChar, SqlValue = pno}
            };

            int c = m_tool.ModifyTable(sql, sqlParameters);

            if (c > 0)
            {
                //Debug.WriteLine("焊接参数保存成功！");
                return true;
            }
            else
            {
                m_main.AddTips("焊接参数保存失败 ！", true);
                return false;
            }
        }

        public void BarCodeChange(MyEvent e)
        {
            m_barcode = e.BarCode;
            txtBarCode.Text = m_barcode;
        }

        /// <summary>
        ///读取未知类型节点 
        /// </summary>
        private void ReadUnknownNodes(string node)
        {
            DataValue value = m_opcUaClient.ReadNode(node);
            // 一个数据的类型是不是数组由 value.WrappedValue.TypeInfo.ValueRank 来决定的
            // -1 说明是一个数值
            // 1  说明是一维数组，如果类型BuiltInType是Int32，那么实际是int[]
            // 2  说明是二维数组，如果类型BuiltInType是Int32，那么实际是int[,]
            if (value.WrappedValue.TypeInfo.BuiltInType == BuiltInType.Int32)
            {
                if (value.WrappedValue.TypeInfo.ValueRank == ValueRanks.Scalar)
                {
                    int temp = (int)value.WrappedValue.Value;               // 最终值
                }
                else if (value.WrappedValue.TypeInfo.ValueRank == ValueRanks.OneDimension)
                {
                    int[] temp = (int[])value.WrappedValue.Value;           // 最终值
                }
                else if (value.WrappedValue.TypeInfo.ValueRank == ValueRanks.TwoDimensions)
                {
                    int[,] temp = (int[,])value.WrappedValue.Value;         // 最终值
                }
            }
            else if (value.WrappedValue.TypeInfo.BuiltInType == BuiltInType.UInt32)
            {
                uint temp = (uint)value.WrappedValue.Value;                 // 数组的情况参照上面的例子
            }
            else if (value.WrappedValue.TypeInfo.BuiltInType == BuiltInType.Float)
            {
                float temp = (float)value.WrappedValue.Value;               // 数组的情况参照上面的例子
            }
            else if (value.WrappedValue.TypeInfo.BuiltInType == BuiltInType.String)
            {
                string temp = (string)value.WrappedValue.Value;             // 数组的情况参照上面的例子
            }
            else if (value.WrappedValue.TypeInfo.BuiltInType == BuiltInType.DateTime)
            {
                DateTime temp = (DateTime)value.WrappedValue.Value;         // 数组的情况参照上面的例子
            }
        }

    }

}


