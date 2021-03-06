﻿using csLTDMC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace HuaTianProject.UI
{
    public partial class TeachingForm : Form
    {
        private ushort m_cardID = FormMain.CardId;
        private double dEquiv = FormMain.PulseEquiv;
        private double dStartVel = FormMain.StartSpeed;
        private double dMaxVel = FormMain.MoveSpeed;
        private double dTacc = FormMain.AccTime;
        private double dTdec = FormMain.DecTime;
        private double dStopVel = FormMain.StopSpeed;
        private double dS_para = FormMain.SParamSpeed;
        private double vectorChabu = FormMain.LineInterSpeed;
        private double absolutePosX;//目标位置
        private double absolutePosY;
        private double absolutePosZ;
        private double absolutePosW;

        private static int width = 500;
        private static int height = 500;

        private Dictionary<string, string> dic = new Dictionary<string, string>();

        public TeachingForm()
        {
            InitializeComponent();
            width = pictureBox1.Width;
            height = pictureBox1.Height;
        }

        private void SetPara(ushort axis)
        {
            LTDMC.dmc_set_equiv(0, axis, dEquiv);  //设置脉冲当量

            LTDMC.dmc_set_profile_unit(0, axis, dStartVel, dMaxVel, dTacc, dTdec, dStopVel);//设置速度参数

            LTDMC.dmc_set_s_profile(0, axis, 0, dS_para);//设置S段速度参数

            LTDMC.dmc_set_dec_stop_time(0, axis, dTdec); //设置减速停止时间
        }

        private void btnXPosDir_Click(object sender, EventArgs e)
        {

            if (radioRlt.Checked)
            {
                double dDist;//目标位置
                dDist = Convert.ToDouble(txtRltDis.Text);
                SetPara(0);
                LTDMC.dmc_pmove_unit(m_cardID, 0, dDist, 0);//定长运动
            }
        }

        private void btnXNegDir_Click(object sender, EventArgs e)
        {
            if (radioRlt.Checked)
            {
                double dDist;//目标位置
                dDist = Convert.ToDouble(txtRltDis.Text);
                SetPara(0);
                LTDMC.dmc_pmove_unit(m_cardID, 0, -dDist, 0);//定长运动
            }
        }

        private void btnYPosDir_Click(object sender, EventArgs e)
        {
            if (radioRlt.Checked)
            {
                double dDist;//目标位置
                dDist = Convert.ToDouble(txtRltDis.Text);
                SetPara(1);
                LTDMC.dmc_pmove_unit(m_cardID, 1, dDist, 0);//定长运动
            }
        }

        private void btnYNegDir_Click(object sender, EventArgs e)
        {
            if (radioRlt.Checked)
            {
                double dDist;//目标位置
                dDist = Convert.ToDouble(txtRltDis.Text);
                SetPara(1);
                LTDMC.dmc_pmove_unit(m_cardID, 1, -dDist, 0);//定长运动
            }
        }

        private void btnZPosDir_Click(object sender, EventArgs e)
        {
            if (radioRlt.Checked)
            {
                double dDist;//目标位置
                dDist = Convert.ToDouble(txtRltDis.Text);
                SetPara(2);
                LTDMC.dmc_pmove_unit(m_cardID, 2, dDist, 0);//定长运动
            }
        }

        private void btnZNegDir_Click(object sender, EventArgs e)
        {
            if (radioRlt.Checked)
            {
                double dDist;//目标位置
                dDist = Convert.ToDouble(txtRltDis.Text);
                SetPara(2);
                LTDMC.dmc_pmove_unit(m_cardID, 2, -dDist, 0);//定长运动
            }
        }

        private void btnWPosDir_Click(object sender, EventArgs e)
        {
            if (radioRlt.Checked)
            {
                double dDist;//目标位置
                dDist = Convert.ToDouble(txtRltDis.Text);
                SetPara(3);
                LTDMC.dmc_pmove_unit(m_cardID, 3, dDist, 0);//定长运动
            }
        }

        private void btnWNegDir_Click(object sender, EventArgs e)
        {
            if (radioRlt.Checked)
            {
                double dDist;//目标位置
                dDist = Convert.ToDouble(txtRltDis.Text);
                SetPara(3);
                LTDMC.dmc_pmove_unit(m_cardID, 3, -dDist, 0);//定长运动
            }
        }

        private void ContinueMove(ushort cardId, ushort axis, ushort dir)
        {
            //设置脉冲输出方式
            LTDMC.dmc_set_pulse_outmode(cardId, axis, 0);
            //设置脉冲当量
            LTDMC.dmc_set_equiv(cardId, axis, dEquiv);
            //设置速度参数
            LTDMC.dmc_set_profile_unit(cardId, axis, dStartVel, dMaxVel, dTacc, dTdec, dStopVel);
            //设置S段速度参数
            LTDMC.dmc_set_s_profile(cardId, axis, 0, dS_para);
            //减速停止时间
            LTDMC.dmc_set_dec_stop_time(cardId, axis, dTdec);
            //开始连续运动
            LTDMC.dmc_vmove(cardId, axis, dir);

        }

        private void StopMove(ushort cardId, ushort axis)
        {
            ushort stopMode = 0;//0-减速停止，1-紧急停止
            LTDMC.dmc_stop(cardId, axis, stopMode);
        }

        

        private void btnXPosDir_MouseUp(object sender, MouseEventArgs e)
        {
            if (radioJog.Checked) 
            {
                StopMove(m_cardID, 0);
            }
            
        }

        

        private void btnXNegDir_MouseUp(object sender, MouseEventArgs e)
        {
            if (radioJog.Checked)
            {
                StopMove(m_cardID, 0);
            }
        }

        private void btnYPosDir_MouseUp(object sender, MouseEventArgs e)
        {
            if (radioJog.Checked)
            {
                StopMove(m_cardID, 1);
            }
        }

        private void btnYNegDir_MouseUp(object sender, MouseEventArgs e)
        {
            if (radioJog.Checked)
            {
                StopMove(m_cardID, 1);
            }
        }

        private void btnZPosDir_MouseUp(object sender, MouseEventArgs e)
        {
            if (radioJog.Checked)
            {
                StopMove(m_cardID, 2);
            }
        }

        private void btnZNegDir_MouseUp(object sender, MouseEventArgs e)
        {
            if (radioJog.Checked)
            {
                StopMove(m_cardID, 2);
            }
        }

        private void btnWPosDir_MouseUp(object sender, MouseEventArgs e)
        {
            if (radioJog.Checked)
            {
                StopMove(m_cardID, 3);
            }
        }

        private void btnWNegDir_MouseUp(object sender, MouseEventArgs e)
        {
            if (radioJog.Checked)
            {
                StopMove(m_cardID, 3);
            }
        }
        /// <summary>
        /// 0轴手动正方向
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnXPosDir_MouseDown(object sender, MouseEventArgs e)
        {
            if (radioJog.Checked)
            {
                ContinueMove(m_cardID, 0, 1);
            }
        }
        /// <summary>
        /// 0轴手动负方向
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnXNegDir_MouseDown(object sender, MouseEventArgs e)
        {
            if (radioJog.Checked)
            {
                ContinueMove(m_cardID, 0, 0);
            }
        }

        private void btnYPosDir_MouseDown(object sender, MouseEventArgs e)
        {
            if (radioJog.Checked)
            {
                ContinueMove(m_cardID, 1, 1);
            }
        }

        private void btnYNegDir_MouseDown(object sender, MouseEventArgs e)
        {
            if (radioJog.Checked)
            {
                ContinueMove(m_cardID, 1, 0);
            }
        }

        private void btnZPosDir_MouseDown(object sender, MouseEventArgs e)
        {
            if (radioJog.Checked)
            {
                ContinueMove(m_cardID, 2, 1);
            }
        }

        private void btnZNegDir_MouseDown(object sender, MouseEventArgs e)
        {
            if (radioJog.Checked)
            {
                ContinueMove(m_cardID, 2, 0);
            }
        }

        private void btnWPosDir_MouseDown(object sender, MouseEventArgs e)
        {
            if (radioJog.Checked)
            {
                ContinueMove(m_cardID, 3, 1);
            }
        }

        private void btnWNegDir_MouseDown(object sender, MouseEventArgs e)
        {
            if (radioJog.Checked)
            {
                ContinueMove(m_cardID, 3, 0);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (FormMain.IsInit)
            {
                //LTDMC.dmc_get_position_unit(0, 0, ref dunitPos); //读取指定轴指令位置值
                //读取指定轴的脉冲值
                lbRealX.Text = LTDMC.dmc_get_position(0, 0).ToString();
                lbRealY.Text = LTDMC.dmc_get_position(0, 1).ToString();
                lbRealZ.Text = LTDMC.dmc_get_position(0, 2).ToString();
                lbRealW.Text = LTDMC.dmc_get_position(0, 3).ToString();
            }
        }

        private void btnSavePos_Click(object sender, EventArgs e)
        {
            try
            {
                dic.Clear();
                if (radioStartePos.Checked)
                {
                    dic.Add("StartPositionX", lbPointX.Text);
                    dic.Add("StartPositionY", lbPointY.Text);
                }
                if (radioEndPos.Checked)
                {
                    dic.Add("EndPositionX", lbPointX.Text);
                    dic.Add("EndPositionY", lbPointY.Text);
                }
                if (radioWeldPos1.Checked)
                {
                    dic.Add("WeltPosition1X", lbPointX.Text);
                    dic.Add("WeltPosition1Y", lbPointY.Text);
                }
                if (radioWeldPos2.Checked)
                {
                    dic.Add("WeltPosition2X", lbPointX.Text);
                    dic.Add("WeltPosition2Y", lbPointY.Text);
                }
                if (rabReserve1.Checked)
                {
                    dic.Add("Reserve1Z", lbPointZ.Text);
                    dic.Add("Reserve1W", lbPointW.Text);
                }
                if (rabReserve2.Checked)
                {
                    dic.Add("Reserve2Z", lbPointZ.Text);
                    dic.Add("Reserve2W", lbPointW.Text);
                }

                FormMain.SaveXmlData(dic);
                FormMain.AnalysisDictionary();

                MessageBox.Show("保存成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败！" + ex.Message);
            }
        }

        private void radioWeldPos1_Click(object sender, EventArgs e)
        {
            if (radioStartePos.Checked == true)
            {
                lbPointX.Text = FormMain.StartPositionX.ToString();
                lbPointY.Text = FormMain.StartPositionY.ToString();
                lbPointX.Visible = true;
                labelX.Visible = true;
                lbPointY.Visible = true;
                labelY.Visible = true;
                lbPointZ.Visible = false;
                labelZ.Visible = false;
                lbPointW.Visible = false;
                labelW.Visible = false;
            }
            if (radioEndPos.Checked == true)
            {
                lbPointX.Text = FormMain.EndPositionX.ToString();
                lbPointY.Text = FormMain.EndPositionY.ToString();
                lbPointX.Visible = true;
                labelX.Visible = true;
                lbPointY.Visible = true;
                labelY.Visible = true;
                lbPointZ.Visible = false;
                labelZ.Visible = false;
                lbPointW.Visible = false;
                labelW.Visible = false;
            }
            if (radioWeldPos1.Checked == true)
            {
                lbPointX.Text = FormMain.WeltPosition1X.ToString();
                lbPointY.Text = FormMain.WeltPosition1Y.ToString();
                lbPointX.Visible = true;
                labelX.Visible = true;
                lbPointY.Visible = true;
                labelY.Visible = true;
                lbPointZ.Visible = false;
                labelZ.Visible = false;
                lbPointW.Visible = false;
                labelW.Visible = false;
            }
            if (radioWeldPos2.Checked == true)
            {
                lbPointX.Text = FormMain.WeltPosition2X.ToString();
                lbPointY.Text = FormMain.WeltPosition2Y.ToString();
                lbPointX.Visible = true;
                labelX.Visible = true;
                lbPointY.Visible = true;
                labelY.Visible = true;
                lbPointZ.Visible = false;
                labelZ.Visible = false;
                lbPointW.Visible = false;
                labelW.Visible = false;
            }
            if (rabReserve1.Checked == true)
            {
                lbPointZ.Text = FormMain.Reserve1Z.ToString();
                lbPointW.Text = FormMain.Reserve1W.ToString();
                lbPointX.Visible = false;
                labelX.Visible = false;
                lbPointY.Visible = false;
                labelY.Visible = false;
                lbPointZ.Visible = true;
                labelZ.Visible = true;
                lbPointW.Visible = true;
                labelW.Visible = true;
            }
            if (rabReserve2.Checked == true)
            {
                lbPointZ.Text = FormMain.Reserve2Z.ToString();
                lbPointW.Text = FormMain.Reserve2W.ToString();
                lbPointX.Visible = false;
                labelX.Visible = false;
                lbPointY.Visible = false;
                labelY.Visible = false;
                lbPointZ.Visible = true;
                labelZ.Visible = true;
                lbPointW.Visible = true;
                labelW.Visible = true;
            }
        }
        private void RunPosXy()
        {
            SetPara(0);
            SetPara(1);
            absolutePosX = Convert.ToDouble(lbPointX.Text);
            absolutePosY = Convert.ToDouble(lbPointY.Text);
            LTDMC.dmc_pmove_unit(0, 0, absolutePosX, 1);//定长运动
            LTDMC.dmc_pmove_unit(0, 1, absolutePosY, 1);//定长运动
        }
        private void RunPosZw()
        {
            SetPara(2);
            SetPara(3);
            absolutePosZ = Convert.ToDouble(lbPointZ.Text);
            absolutePosW = Convert.ToDouble(lbPointW.Text);
            LTDMC.dmc_pmove_unit(0, 0, absolutePosX, 1);//定长运动
            LTDMC.dmc_pmove_unit(0, 1, absolutePosY, 1);//定长运动
        }
        private void btnMove_Click(object sender, EventArgs e)
        {
            if (radioStartePos.Checked == true)
            {
                RunPosXy();
            }
            if (radioEndPos.Checked == true)
            {
                RunPosXy();
            }
            if (radioWeldPos1.Checked == true)
            {
                RunPosXy();
            }
            if (radioWeldPos2.Checked == true)
            {
                RunPosXy();
            }
            if (rabReserve1.Checked == true)
            {
                RunPosZw();
            }
            if (rabReserve2.Checked == true)
            {
                RunPosZw();
            }
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            ushort stop_mode = 1; //制动方式，0：减速停止，1：紧急停止
            LTDMC.dmc_stop(0, 0, stop_mode);
            LTDMC.dmc_stop(0, 1, stop_mode);
            LTDMC.dmc_stop(0, 2, stop_mode);
            LTDMC.dmc_stop(0, 3, stop_mode);
        }

        private void cbxMotionTrail_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxMotionTrail.SelectedIndex == 0)
            {
                DrawMotionTrail1();
            }
            if (cbxMotionTrail.SelectedIndex == 1)
            {
                DrawMotionTrail2();
            }
            if (cbxMotionTrail.SelectedIndex == 2)
            {
                DrawMotionTrail3();
            }
            if (cbxMotionTrail.SelectedIndex == 3)
            {
                DrawMotionTrail4();
            }
        }
        private void DrawMotionTrail1()
        {
            Bitmap bm = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bm);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Pen pen2 = new Pen(Color.Black, 4);
            Pen pen = new Pen(Color.Black, 2);
            g.DrawLine(pen, new Point(0, 0), new Point(width, 0));
            g.DrawLine(pen2, new Point(width, 0), new Point(width, height));
            g.DrawLine(pen2, new Point(width, height), new Point(0, height));
            g.DrawLine(pen, new Point(0, height), new Point(0, 0));
            Point A = new Point(width / 3, height / 3);
            Point B = new Point(width * 2 / 3, height / 3);
            Point C = new Point(width * 5 / 6, height / 3 + width / 6);
            Point D = new Point(width * 5 / 6, height * 5 / 6 );
            Pen penRed = new Pen(Color.Red, 2);
            g.DrawLine(penRed, A, B);
            g.DrawArc(pen, width * 1 / 2, height / 3, width / 3, width / 3, 0, -90);
            g.DrawLine(penRed, C, D);
            g.DrawLine(penRed, D, A);
            g.DrawString("A", this.Font, new SolidBrush(Color.Blue), width / 3-5, height / 3-15);
            g.DrawString("B", this.Font, new SolidBrush(Color.Blue), width * 2 / 3 - 5, height / 3 - 15);
            g.DrawString("C", this.Font, new SolidBrush(Color.Blue), width * 5 / 6 + 5, height / 3 + width / 6-3);
            g.DrawString("D", this.Font, new SolidBrush(Color.Blue), width * 5 / 6 - 5, height * 5 / 6 + 2);
            g.Dispose();
            pictureBox1.Image = bm;
        }
        private void DrawMotionTrail2()
        {
            Bitmap bm = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bm);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Pen pen2 = new Pen(Color.Black, 4);
            Pen pen = new Pen(Color.Black, 2);
            g.DrawLine(pen, new Point(0, 0), new Point(width, 0));
            g.DrawLine(pen2, new Point(width, 0), new Point(width, height));
            g.DrawLine(pen2, new Point(width, height), new Point(0, height));
            g.DrawLine(pen, new Point(0, height), new Point(0, 0));
            Point A = new Point(width * 2 / 5, height * 4 / 5);
            Point B = new Point(width * 2 / 5, height * 2 / 5);
            Point C = new Point(width * 3 / 5, height * 2 / 5);
            Point D = new Point(width * 3 / 5, height * 4 / 5);
            Pen penRed = new Pen(Color.Red, 2);
            g.DrawArc(pen, width * 1 / 5, height * 2 / 5, width * 2 / 5, height * 2 / 5, 90, 180);
            g.DrawLine(penRed, B, C);
            g.DrawArc(pen, width * 2 / 5, height * 2 / 5, width * 2 / 5, height * 2 / 5, -270, -180);
            g.DrawLine(penRed, D, A);
            g.DrawString("D", this.Font, new SolidBrush(Color.Blue), width * 2 / 5 - 5, height * 4 / 5 + 3);
            g.DrawString("A", this.Font, new SolidBrush(Color.Blue), width * 2 / 5 - 5, height * 2 / 5 - 15);
            g.DrawString("B", this.Font, new SolidBrush(Color.Blue), width * 3 / 5 - 5, height * 2 / 5 - 15);
            g.DrawString("C", this.Font, new SolidBrush(Color.Blue), width * 3 / 5 - 5, height * 4 / 5 + 3);
            g.Dispose();
            pictureBox1.Image = bm;
        }
        private void DrawMotionTrail3()
        {
            Bitmap bm = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bm);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Pen pen2 = new Pen(Color.Black, 4);
            Pen pen = new Pen(Color.Black, 2);
            g.DrawLine(pen, new Point(0, 0), new Point(width, 0));
            g.DrawLine(pen2, new Point(width, 0), new Point(width, height));
            g.DrawLine(pen2, new Point(width, height), new Point(0, height));
            g.DrawLine(pen, new Point(0, height), new Point(0, 0));
            Point A = new Point(width * 1 / 5, height / 5);
            Point B = new Point(width * 3 / 5, height / 5);
            Point C = new Point(width * 3 / 5, height * 3 / 5);
            Point D = new Point(width * 1 / 5, height * 3 / 5);
            Pen penRed = new Pen(Color.Red, 2);
            g.DrawLine(penRed, A, B);
            g.DrawLine(penRed, B, C);
            g.DrawArc(pen, width * 1 / 5, height * 2 / 5, width * 2 / 5, height * 2 / 5, 0, 180);
            g.DrawLine(penRed, D, A);
            g.DrawString("A", this.Font, new SolidBrush(Color.Blue), width * 1 / 5 - 15, height * 1 / 5 - 15);
            g.DrawString("B", this.Font, new SolidBrush(Color.Blue), width * 3 / 5 + 2, height * 1 / 5 - 15);
            g.DrawString("C", this.Font, new SolidBrush(Color.Blue), width * 3 / 5 + 5, height * 3 / 5 - 2);
            g.DrawString("D", this.Font, new SolidBrush(Color.Blue), width * 1 / 5 - 15, height * 3 / 5 - 2);
            g.Dispose();
            pictureBox1.Image = bm;
        }
        private void DrawMotionTrail4()
        {
            Bitmap bm = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bm);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Pen pen2 = new Pen(Color.Black, 4);
            Pen pen = new Pen(Color.Black, 2);
            g.DrawLine(pen, new Point(0, 0), new Point(width, 0));
            g.DrawLine(pen2, new Point(width, 0), new Point(width, height));
            g.DrawLine(pen2, new Point(width, height), new Point(0, height));
            g.DrawLine(pen, new Point(0, height), new Point(0, 0));
            Point A = new Point(width / 5, height / 5);
            Point B = new Point(width * 3 / 5, height / 5);
            Point C = new Point(width * 4 / 5, height * 2 / 5);
            Point D = new Point(width * 4 / 5, height * 3 / 5);
            Point E = new Point(width * 3 / 5, height * 4 / 5);
            Point F = new Point(width * 1 / 5, height * 4 / 5);
            Pen penRed = new Pen(Color.Red, 2);
            g.DrawLine(penRed, A, B);
            g.DrawArc(pen, width * 2 / 5, height * 1 / 5, width * 2 / 5, height * 2 / 5, 0, -90);
            g.DrawLine(penRed, C, D);
            g.DrawArc(pen, width * 2 / 5, height * 2 / 5, width * 2 / 5, height * 2 / 5, -270, -90);
            g.DrawLine(penRed, E, F);
            g.DrawLine(penRed, F, A);
            g.DrawString("A", this.Font, new SolidBrush(Color.Blue), width * 1 / 5 - 15, height * 1 / 5 - 15);
            g.DrawString("B", this.Font, new SolidBrush(Color.Blue), width * 3 / 5 - 2, height * 1 / 5 - 15);
            g.DrawString("C", this.Font, new SolidBrush(Color.Blue), width * 4 / 5 + 5, height * 2 / 5 - 2);
            g.DrawString("D", this.Font, new SolidBrush(Color.Blue), width * 4 / 5 + 5, height * 3 / 5 - 2);
            g.DrawString("E", this.Font, new SolidBrush(Color.Blue), width * 3 / 5 + 2, height * 4 / 5 + 2);
            g.DrawString("F", this.Font, new SolidBrush(Color.Blue), width * 1 / 5 - 15, height * 4 / 5 - 2);
            g.Dispose();
            pictureBox1.Image = bm;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (cbxMotionTrail.SelectedItem == null)
            {
                MessageBox.Show("请选择轨迹");
                return;
            }
            ushort _CardID = 0;
            ushort AxisX = 0;
            ushort AxisY = 1;
            ushort crd = 0;
            //设置脉冲当量
            LTDMC.dmc_set_equiv(0, AxisX, FormMain.PulseEquiv);
            LTDMC.dmc_set_equiv(0, AxisY, FormMain.PulseEquiv);
            //   画图
            ushort axisnum = 2;
            ushort[] axises = new ushort[] { AxisX, AxisY };
            LTDMC.dmc_conti_open_list(0, crd, axisnum, axises);
            //设置插补速度           
            LTDMC.dmc_set_vector_profile_unit(0, crd, 0, vectorChabu, 0.1, 0.1, 0);
            // LTDMC.dmc_conti_set_blend(0, crd, 1);
            if (cbxMotionTrail.SelectedIndex == 0)
            {
                double[] A = new double[] { 0,0};
                double[] B = new double[] { 200,0 };
                double[] C = new double[] {400,-200};
                double[] D = new double[] { 400,-400 };
                LTDMC.dmc_conti_line_unit(_CardID, crd, axisnum, axises,
                B, 1, 0);  //直线插补，相对模式
                LTDMC.dmc_conti_arc_move_center_unit(_CardID, crd, axisnum, axises,
                    C, new double[] { 200, -200 }, 0, 0, 1, 0);
                //XY平面圆弧插补，顺时针，相对坐标模式。   此函数是基于圆心圆弧扩展的螺旋线插补运动（可作两轴圆弧插补）
                //终点坐标   圆心坐标
                LTDMC.dmc_conti_line_unit(_CardID, crd, axisnum, axises,
                    D, 1, 0);   //直线插补，相对模式
                LTDMC.dmc_conti_line_unit(_CardID, crd, axisnum, axises,
                    A, 1, 0);   //直线插补，相对模式
                LTDMC.dmc_conti_start_list(_CardID, crd);
                Thread.Sleep(1000);
                LTDMC.dmc_conti_close_list(_CardID, crd);
            }
            if (cbxMotionTrail.SelectedIndex == 1)
            {
                double[] A = new double[] { 0, 0 };
                double[] B = new double[] { 200, 0 };
                double[] C = new double[] { 400, -200 };
                double[] D = new double[] { 400, -400 };
                LTDMC.dmc_conti_line_unit(_CardID, crd, axisnum, axises,
                new double[] { 200, 0 }, 1, 0);  //直线插补，相对模式

                LTDMC.dmc_conti_arc_move_center_unit(_CardID, crd, axisnum, axises,
                    new double[] { 400, -200 }, new double[] { 200, -200 }, 0, 0, 1, 0);
                //XY平面圆弧插补，顺时针，相对坐标模式。   此函数是基于圆心圆弧扩展的螺旋线插补运动（可作两轴圆弧插补）
                //终点坐标   圆心坐标
                LTDMC.dmc_conti_line_unit(_CardID, crd, axisnum, axises,
                    new double[] { 400, -400 }, 1, 0);   //直线插补，相对模式

                LTDMC.dmc_conti_line_unit(_CardID, crd, axisnum, axises,
                    new double[] { 0, 0 }, 1, 0);   //直线插补，相对模式

                LTDMC.dmc_conti_start_list(_CardID, crd);
                Thread.Sleep(1000);
                LTDMC.dmc_conti_close_list(_CardID, crd);
            }
            if (cbxMotionTrail.SelectedIndex == 2)
            { 
            
            }
            if (cbxMotionTrail.SelectedIndex == 3)
            { 
            
            }
        }
        private void DrawMarkPoint(Graphics g, double x, double y, string txt)
        {
            int width = pictureBox1.Width;
            int height = pictureBox1.Height;
            float nx = 2000f / 80f;
            float ny = 2000f / 80f;
            float _x = (float)(x / nx + width / 2);
            float _y = (float)(y / ny + height / 3);
            g.FillEllipse(new SolidBrush(Color.Blue), new RectangleF(_x - 2f, _y - 2f, 4f, 4f));
            //
            Matrix matrix = g.Transform;
            g.ResetTransform();
            g.DrawString(txt, this.Font, new SolidBrush(Color.Blue), _x, height - _y);
            g.Transform = matrix;
        }

    }
}
