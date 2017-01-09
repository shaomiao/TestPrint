using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GoMarket.Devices;
using GoMarket.LogSystem;
using System.Drawing.Text;
using System.Drawing;

namespace TestPrint
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

       
       

        private void button4_Click(object sender, EventArgs e)
        {
             StringFormat fmt = new StringFormat();
            ////格式化数字
            fmt.Alignment = StringAlignment.Far;
            Bitmap bm = new Bitmap(576, 1000);
            Graphics g = Graphics.FromImage(bm);
            drawFourthLineString(g, "折扣：：：商品名:购物袋小号商品单价:0.1 折扣数:5 个数:1 姓名:邵淼 当前时间2016-11-7 15:45:48");
            //drawPrint(g, fmt, "开张单数：", "11111", "张");
            //drawPrint(g, fmt, "银行卡营收：", "11111", "元");
        }
        private void printimage() {
            ESCPOS.Open();
            StringFormat fmt = new StringFormat();
            ////格式化数字
            fmt.Alignment = StringAlignment.Far;
            Bitmap bm = new Bitmap(576, 64);
            Graphics g = Graphics.FromImage(bm);
            g.Dispose();
            drawFirstLineString(g,"                 收银员交班明细\n");
            drawFirstLineString(g,string.Format("收银员：[{0}]\n", "xxx"));
            drawFirstLineString(g,string.Format("当班时间：{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}\n", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second));
            drawFirstLineString(g,string.Format("      至：{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}\n", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second));
            drawPrint(g, fmt, "开张单数：", "11111", "张");
            drawPrint(g, fmt, "银行卡营收：", "11111", "元");
            drawPrint(g, fmt, "微信营收：", "11111", "元");
            drawPrint(g, fmt, "支付宝营收：", "11111", "元");
            drawPrint(g, fmt, "现金营收：", "11111", "元");
            drawPrint(g, fmt, "线上货到付款营收：", "11111", "元");
            drawPrint(g, fmt, "会员营收：", "11111", "元");
            drawPrint(g, fmt, "线上支付宝营收：", "11111", "元");
            drawPrint(g, fmt, "汇总营收：", "11111", "元");
            drawFirstLineString(g, "记录：");
            drawFourthLineString(g, "折扣：：：商品名:购物袋小号商品单价:0.1 折扣数:5 个数:1 姓名:邵淼 当前时间2016-11-7 15:45:48");
            drawFourthLineString(g, "折扣：：：商品名:购物袋小号商品单价:0.1 折扣数:5 个数:1 姓名:邵淼 当前时间2016-11-7 15:45:48");
            drawFourthLineString(g, "折扣：：：商品名:购物袋小号商品单价:0.1 折扣数:5 个数:1 姓名:邵淼 当前时间2016-11-7 15:45:48");
            drawPrint(g, fmt, "退单金额：", "11111", "元");
            drawPrint(g, fmt, "上次备用金：", "11111", "元");
            drawPrint(g, fmt, "本次备用金：", "11111", "元");
            drawFirstLineString(g, "备注：");
            ESCPOS.Feed(10);
            drawFirstLineString(g,"收银员签名：            财务签名：\n");
            ESCPOS.Feed(5);
            drawFirstLineString(g,string.Format("打印时间：{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}\n", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second));
            ESCPOS.Feed(10);
            ESCPOS.CutPapper();
            ESCPOS.Close();
        }
        
        private void drawPrint(Graphics g, StringFormat fmt, String name, String count, String unit)
        {
            Bitmap bmp = new Bitmap(576, 32);
            g = Graphics.FromImage(bmp);
            Font font = new Font("宋体", 24, GraphicsUnit.Pixel);
            g.DrawString(name, font, Brushes.White, new Rectangle(0, 0, 32 * name.Length, 32));
            g.DrawString(count, font, Brushes.White, new Rectangle(576 - 32 * count.Length - unit.Length * 32, 0, 32 * count.Length, 32), fmt);
            g.DrawString(unit, font, Brushes.White, new Rectangle(576 - 32, 0, unit.Length * 32, 32));
            g.Dispose();
            ESCPOS.PrintFixedImage(bmp);
            
            //bmp.Save("C:\\github\\TestPrint\\TestPrint\\bin\\Debug\\g8.jpg");
            bmp.Dispose();
        }
        private void drawLine(Graphics g)
        {
            Bitmap bmp = new Bitmap(576, 50);
            g = Graphics.FromImage(bmp);
            g.DrawLine(Pens.White, 0, 50, 286, 50);
            g.Dispose();
            ESCPOS.PrintFixedImage(bmp);

            //bmp.Save("C:\\github\\TestPrint\\TestPrint\\bin\\Debug\\g8.jpg");
            bmp.Dispose();
        }
        private void drawFirstLineString(Graphics g, String str)
        {
            Bitmap bmp = new Bitmap(576, 32);
            g = Graphics.FromImage(bmp);
            Font font = new Font("宋体", 24, GraphicsUnit.Pixel);
            g.DrawString(str, font, Brushes.White, new Rectangle(0, 0, 576, 32));
            g.Dispose();
            ESCPOS.PrintFixedImage(bmp);

            // bmp.Save("C:\\github\\TestPrint\\TestPrint\\bin\\Debug\\g9.jpg");
            bmp.Dispose();
        }
        private void drawFourthLineString(Graphics g,String str)
        {

            Bitmap bmp = new Bitmap(576, 30*4);
            g = Graphics.FromImage(bmp);
            Font font = new Font("宋体", 24, GraphicsUnit.Pixel);
            g.DrawString(str, font, Brushes.White, new Rectangle(0, 0, 576, 30*4));
            g.Dispose();
            ESCPOS.PrintFixedImage(bmp);

           // bmp.Save("C:\\github\\TestPrint\\TestPrint\\bin\\Debug\\g9.jpg");
            bmp.Dispose();
        }
       
        private void button5_Click(object sender, EventArgs e)
        {
            printimage();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            printCOM();
        }

        private void printCOM() {
            CommControl commcon = new CommControl("COM1");
            
            if (commcon.IsOpen())
            {
                commcon.CutPaper();
                commcon.WriteBigLine(" 这是题目");

                commcon.PrintLine();

                commcon.WriteLine("姓名：东方不败");

                commcon.SetUnderLine();

                commcon.NewRow();

                commcon.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), CommControl.HorPos.Right);

                commcon.NewRow(3);

                commcon.CutPaper();

            }

            commcon.Dispose();
        }

        
    }
}
