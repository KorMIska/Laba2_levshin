using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Laba1_levshin.Form1;

namespace Laba1_levshin
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

            Graphics g = pictureBox1.CreateGraphics();

            g.Clear(BackColor);


            Model mod = new Model(comands);

            List<Itm> m = mod.Rabot();

            trackBar1.Maximum= m[m.Count - 1].time / 30 + 1;
            int j = trackBar1.Value * 30;
            
            DrawPole(g, j);

            for (int i = 0; i < m.Count; i++)
            {
                
                switch (m[i].type)
                {
                    case 0: ZeroComand(g, m[i].time - j, m[i].nomer+1, m[i].line); break;
                    case 1: OneComand(g, m[i].time - j, m[i].nomer+1, m[i].line); break;
                    case 2: TwoComand(g, m[i].time - j, m[i].nomer+1, m[i].T, m[i].line); break;
                    case 3: ThreeComand(g, m[i].time - j, m[i].nomer + 1, m[i].T, m[i].line); break;
                    case 4: FourComand(g, m[i].time - j, m[i].nomer + 1); break;
                    case 5: FiveComand(g, m[i].time - j, m[i].nomer + 1, m[i].T); break;
                }
            }
        }


        public Comand[] comands;

        #region consts
        public const int HEIGHT = 35;
        public const int WIDTH = 25;
        public const int LOCAL_ONE_LINE = 55;
        public const int LOCAL_TWO_LINE = LOCAL_ONE_LINE + HEIGHT * 2 + HEIGHT / 2;
        public const int LOCAL_Three_LINE = LOCAL_TWO_LINE + HEIGHT * 2 + HEIGHT / 2;
        public const int LOCAL_FOUR_LINE = LOCAL_Three_LINE + HEIGHT * 2 + HEIGHT / 2;
        public const int LENTH_LINE = 2000;
        public const int fss = 2;
        public const int Fop = 3;
        public int[] LOCAL_LAINS = { LOCAL_ONE_LINE, LOCAL_TWO_LINE, LOCAL_Three_LINE, LOCAL_FOUR_LINE }; 
        #endregion

        #region модель
        // 0 - черта 1 - декодировка 2 - вычесление 3 - упровление 4 - кеш 5 - ДМА
        public struct Comand
        {
            public int t;
            public bool kh;
            public int type;   // 0 - черта 1 - декодировка 2 - вычесление 3 - упровление 4 - кеш
            public int nomer;
            public int time;

            public Comand(int t, bool kh, int type)
            {
                this.t = t;
                this.kh = kh;
                this.type = type;
                nomer = 0;
                time = 0;
            }
            public Comand(int t, bool kh)
            {
                this.t = t;
                this.kh = kh;
                this.type = 0;
                nomer = 0;
                time = 0;
            }
        }

        public struct Itm
        {
            public int nomer;
            public int time;
            public int type;
            public int T;
            public int line;
            public Itm(int nomer, int time, int type, int T, int line)
            {
                this.nomer = nomer;
                this.time = time;
                this.type = type;
                this.T = T;
                this.line = line;
            }
        }

        public partial class K
        {
            public int timeStop = 0;
            public List<Itm> queue;
            public bool chek()
            {
                return timeStop <= 0 & queue.Count == 0;
            }

            public K()
            {
                queue = new List<Itm> ();
            }
        }

        public partial class SH
        {
            public int timeStop = 0;
            public List<Itm> queueKK;
            public List<Itm> queueP;
            public int zadacha;

            public bool chek()
            {
                return timeStop <= 0 & queueKK.Count == 0 & queueP.Count == 0;
            }

            public SH() 
            {
                queueKK = new List<Itm> ();
                queueP = new List<Itm> (); 
            }
        }

        public partial class KK
        {
            public int timeStop;
        }

        public partial class Model
        {
            K k1 = new K();
            K k2 = new K();
            SH sh = new SH();


            Comand[] cs;

            public Model(Comand[] cs) { this.cs = cs; }

            public List<Itm> Rabot()
            {
                List<Itm> DrawCom = new List<Itm>();
                int time = 0;

                int countCom = 0;

                //Один цыкл == Один такт

                while (true)
                {
                    // Первый конвеер

                    if (k1.timeStop <= 0 & k1.queue.Count == 0)
                    {
                        // countCom - номер команды
                        // проверка того чтобы countCom не был больше количества команд
                        // Что означает что все команды обработаны
                        if (countCom == cs.Length)
                        {
                            //Доп проверка того что выполнены команды из КК
                            if (k1.chek()  & k2.chek() & sh.chek())
                                return DrawCom;
                        }
                        else
                        {


                            if (cs[countCom].type == 5)
                            {
                                DrawCom.Add(new Itm(countCom, time, 0, cs[countCom].t, 0));

                                sh.queueP.Add( new Itm(countCom, time, cs[countCom].type, cs[countCom].t, 4));

                                countCom++;

                                continue;
                            }
                            else
                            {
                                // Выполняется Декодировка
                                if (cs[countCom].kh)  // Данные есть в кэше
                                {
                                    // занятm на 1 цикл
                                    k1.timeStop = 1;

                                    //Команда для отрисовки 

                                    DrawCom.Add(new Itm(countCom, time, 1, cs[countCom].t, 0));

                                    k1.queue.Insert(0, new Itm(countCom, time, cs[countCom].type, cs[countCom].t, 0));

                                    countCom++;

                                }
                                else // Кэш промах
                                {

                                    // Отпрака запроса в КК
                                    sh.queueKK.Add(new Itm(countCom, time, cs[countCom].type, cs[countCom].t, 0));

                                    //Отрисовка Команды 
                                    DrawCom.Add(new Itm(countCom, time, 0, cs[countCom].t, 0));

                                    countCom++;
                                    // continue чтобы не зашитало такт
                                    continue;
                                }
                            }
                        }
                    }

                    // Второй конвеер
                    if (k2.chek() & k2.queue.Count  == 0)
                    {
                        // countCom - номер команды
                        // проверка того чтобы countCom не был больше количества команд
                        // Что означает что все команды обработаны
                        if (countCom != cs.Length)
                        {

                            if (cs[countCom].type == 5)
                            {
                                DrawCom.Add(new Itm(countCom, time, 0, cs[countCom].t, 1));

                                sh.queueP.Insert(0, new Itm(countCom, time, cs[countCom].type, cs[countCom].t, 4));

                                countCom++;
                                continue;
                            }
                            else
                            {
                                // Выполняется Декодировка

                                if (cs[countCom].kh)  // Данные есть в кэше
                                {
                                    // Кэшь занят на 1 цикл
                                    k2.timeStop = 1;

                                    //Команда для отрисовки 

                                    DrawCom.Add(new Itm(countCom, time, 1, cs[countCom].t, 1));

                                    k2.queue.Insert(0, new Itm(countCom, time, cs[countCom].type, cs[countCom].t, 1));

                                    countCom++;


                                }
                                else // Кэш промах
                                {
                                    //cs[countCom].nomer = countCom;

                                    // Отпрака запроса в КК
                                    sh.queueKK.Add(new Itm(countCom, time, cs[countCom].type, cs[countCom].t, 1));

                                    //Отрисовка Команды 
                                    DrawCom.Add(new Itm(countCom, time, 0, cs[countCom].t, 1));

                                    countCom++;
                                    // continue чтобы не зашитало такт
                                    continue;
                                }
                               
                            }
                        }
                    }



                    // К1 свободен и есть заявка на работу
                    if (k1.timeStop <= 0 & k1.queue.Count  != 0)
                    {
                        // type == уровление устройством 
                        if (k1.queue[0].type == 3)
                        {
                            // КК должен быть свободен 
                            // Иначе Должен ждать 

                            if (sh.timeStop <= 0 & sh.queueP.Count == 0)
                            {
                                if (k2.queue.Count != 0)
                                {
                                    if (!(k2.queue[0].type == 3 & k2.queue[0].time < k1.queue[0].time))
                                    {
                                        var cur = k1.queue[0];
                                        cur.time = time;

                                        sh.timeStop = fss * cur.T;
                                        k1.timeStop = fss * cur.T;

                                        DrawCom.Add(cur);

                                        k1.queue.RemoveAt(0);
                                    }
                                }
                                else
                                {
                                    var cur = k1.queue[0];
                                    cur.time = time;

                                    sh.timeStop = fss * cur.T;
                                    k1.timeStop = fss * cur.T;

                                    DrawCom.Add(cur);

                                    k1.queue.RemoveAt(0);
                                }
                            }
                            else
                            {
                                if (sh.timeStop <= 0 & sh.queueP.Count != 0)
                                {
                                    if (sh.queueP[0].time > k1.queue[0].time)
                                    {
                                        var cur = k1.queue[0];
                                        cur.time = time;

                                        sh.timeStop = fss * cur.T;
                                        k1.timeStop = fss * cur.T;

                                        DrawCom.Add(cur);

                                        k1.queue.RemoveAt(0);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (k1.queue[0].type == 2)
                            {

                                Itm cur = k1.queue[0];
                                cur.time = time;
                                cur.type = 2;
                                DrawCom.Add(cur);
                                k1.queue.RemoveAt(0);
                                k1.timeStop = 1 * cur.T;
                            }
                            else
                            {
                                if (k1.queue[0].type == 1)
                                {
                                    k1.timeStop = 1;
                                    Itm cur = k1.queue[0];
                                    cur.time = time;
                                    cur.type = 1;
                                    DrawCom.Add(cur);
                                    k1.queue.RemoveAt(0);
                                }
                            }
                        }
                    }

                    // К2 свободен и есть заявка на работу
                    if (k2.timeStop <= 0 & k2.queue.Count  != 0)
                    {
                        // type == уровление устройством 
                        if (k2.queue[0].type == 3)
                        {
                            // КК должен быть свободен 
                            // Иначе Должен ждать 
                            if (sh.timeStop <= 0 & sh.queueP.Count == 0)
                            {
                                var cur = k2.queue[0];
                                cur.time = time;

                                sh.timeStop = fss * cur.T;
                                k2.timeStop = fss * cur.T;

                                DrawCom.Add(cur);

                                k2.queue.RemoveAt(0);
                            }
                            else
                            {
                                if (sh.timeStop <= 0 & sh.queueP.Count != 0)
                                {
                                    if (sh.queueP[0].time > k2.queue[0].time)
                                    {
                                        var cur = k2.queue[0];
                                        cur.time = time;

                                        sh.timeStop = fss * cur.T;
                                        k2.timeStop = fss * cur.T;

                                        DrawCom.Add(cur);

                                        k2.queue.RemoveAt(0);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (k2.queue[0].type == 2)
                            {

                                Itm cur = k2.queue[0];
                                cur.time = time;
                                cur.type = 2;
                                DrawCom.Add(cur);
                                k2.queue.RemoveAt(0);
                                k2.timeStop = 1 * cur.T;
                            }
                            else
                            {
                                if (k2.queue[0].type == 1)
                                {
                                    k2.timeStop = 1;
                                    Itm cur = k2.queue[0];
                                    cur.time = time;
                                    cur.type = 1;
                                    DrawCom.Add(cur);
                                    k2.queue.RemoveAt(0);
                                }
                            }
                        }
                    }


                    if (sh.timeStop <= 0)
                    {
                        if(sh.queueP.Count != 0)
                        {
                            if (sh.queueP[0].type == 5)
                            {
                                sh.timeStop = sh.queueP[0].T*fss;

                                var cur =  sh.queueP[0];

                                cur.time = time;

                                DrawCom.Add(cur);

                                sh.queueP.RemoveAt(0);
                            }
                        }
                        else 
                        {
                            if(sh.queueKK.Count != 0)
                            {
                                sh.timeStop = Fop * fss;

                                var cur = sh.queueKK[0];
                                cur.time = time;
                                cur.type = 4;

                                sh.zadacha = 4;


                                DrawCom.Add(cur);
                            }
                        }
                    }


                    if (sh.timeStop - 1 == 0)
                    {

                        if (sh.zadacha != 0)
                        {
                            Itm cur;

                            if (sh.queueKK[0].line == 0)
                            {
                                cur = sh.queueKK[0];
                                cur.time = time;

                                k1.queue.Add(new Itm(cur.nomer, time, 1, 1, 0));
                                k1.queue.Add(cur);

                                sh.zadacha = 0;
                                sh.queueKK.RemoveAt(0);
                            }
                            else
                            {
                                if (sh.queueKK[0].line == 1)
                                {
                                    cur = sh.queueKK[0];
                                    cur.time = time;
                                    k2.queue.Add(new Itm(cur.nomer, time, 1, 1, 1));
                                    k2.queue.Add(cur);

                                    sh.zadacha = 0;
                                    sh.queueKK.RemoveAt(0);
                                }
                            }
                        }
                    }

                    #region
                    //// DMA
                    //if (sh.timeStop <= 0 & queueK1.Count != 0)
                    //{
                    //    if (queueK1[0].type == 5)
                    //    {

                    //        if(queueK1[0].type == 5)
                    //        {
                    //            sh.timeStop = fss * queueK1[0].T;

                    //            DrawCom.Add(new Itm(queueK1[0].nomer, time, 5, queueK1[0].T, 5));

                    //            queueK1.RemoveAt(0);

                    //        }

                    //    }
                    //}
                    //// DMA
                    //if (sh.timeStop <= 0 & queueK2.Count != 0)
                    //{
                    //    if (queueK2[0].type == 5)
                    //    { 
                    //        if (queueK2[0].type == 5)
                    //        {
                    //            sh.timeStop = fss * queueK2[0].T;

                    //            DrawCom.Add(new Itm(queueK2[0].nomer, time, 5, queueK2[0].T, 5));

                    //            queueK2.RemoveAt(0);

                    //        }
                    //    }

                    //}

                    //// К1 свободен и есть заявка на работу
                    //if (k1.timeStop <= 0 & queueK1.Count  != 0)
                    //{
                    //    // type == уровление устройством 
                    //    if (queueK1[0].type == 3)
                    //    {
                    //        // КК должен быть свободен 
                    //        // Иначе Должен ждать 

                    //        if (queueK2.Count == 0)
                    //        {
                    //            if (sh.timeStop <= 0)
                    //            {
                    //                Itm cur = queueK1[0];

                    //                k1.timeStop = fss * cur.T;
                    //                sh.timeStop = fss * cur.T;

                    //                cur.time = time;

                    //                DrawCom.Add(cur);

                    //                queueK1.RemoveAt(0);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            if (sh.timeStop <= 0 & queueK2[0].time > queueK1[0].time)
                    //            {
                    //                Itm cur = queueK1[0];

                    //                k1.timeStop = fss * cur.T;
                    //                sh.timeStop = fss * cur.T;

                    //                cur.time = time;

                    //                DrawCom.Add(cur);

                    //                queueK1.RemoveAt(0);
                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if (queueK1[0].type == 2)
                    //        {

                    //            Itm cur = queueK1[0];
                    //            cur.time = time;
                    //            cur.type = 2;
                    //            DrawCom.Add(cur);
                    //            queueK1.RemoveAt(0);
                    //            k1.timeStop = 1 * cur.T;
                    //        }
                    //        else
                    //        {
                    //            if (queueK1[0].type == 1)
                    //            {
                    //                k1.timeStop = 1;
                    //                Itm cur = queueK1[0];
                    //                cur.time = time;
                    //                cur.type = 1;
                    //                DrawCom.Add(cur);
                    //                queueK1.RemoveAt(0);
                    //            }
                    //        }
                    //    }
                    //}

                    //// К2 свободен и есть заявка на работу
                    //if (k2.timeStop <= 0 & queueK2.Count  != 0)
                    //{
                    //    // type == уровление устройством 
                    //    if (queueK2[0].type == 3)
                    //    {
                    //        // КК должен быть свободен 
                    //        // Иначе Должен ждать 
                    //        if (sh.timeStop <= 0)
                    //        {
                    //            Itm cur = queueK2[0];

                    //            k2.timeStop = fss * cur.T;
                    //            sh.timeStop = fss * cur.T;

                    //            cur.time = time;

                    //            DrawCom.Add(cur);

                    //            queueK2.RemoveAt(0);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if (queueK2[0].type == 2)
                    //        {

                    //            Itm cur = queueK2[0];
                    //            cur.time = time;
                    //            cur.type = 2;
                    //            DrawCom.Add(cur);
                    //            queueK2.RemoveAt(0);
                    //            k2.timeStop = 1 * cur.T;
                    //        }
                    //        else
                    //        {
                    //            if (queueK2[0].type == 1)
                    //            {
                    //                k2.timeStop = 1;
                    //                Itm cur = queueK2[0];
                    //                cur.time = time;
                    //                cur.type = 1;
                    //                DrawCom.Add(cur);
                    //                queueK2.RemoveAt(0);
                    //            }
                    //        }
                    //    }
                    //}


                    //// КК свободен и есть запрос
                    //if (kk.timeStop <= 0 & queueKK.Count  != 0 & sh.timeStop <= 0)
                    //{

                    //    kk.timeStop = fss * 3;
                    //    sh.timeStop = fss * 3;
                    //    Itm cur = queueKK[0];

                    //    cur.time = time;

                    //    DrawCom.Add(new Itm(queueKK[0].nomer, time, 4, queueKK[0].T, cur.line));

                    //}


                    //if (kk.timeStop - 1 == 0)
                    //{
                    //    Itm cur = queueKK[0];

                    //    cur.time = time;

                    //   // DrawCom.Add(new Itm(queueKK[0].nomer, time, 4, queueKK[0].T, cur.line));

                    //    if (cur.line == 0)
                    //    {
                    //        queueK1.Add(new Itm(cur.nomer, time, 1, 1, 0));
                    //        queueK1.Add(cur);
                    //    }
                    //    else
                    //    {
                    //        queueK2.Add(new Itm(cur.nomer, time, 1, 1, 1));
                    //        queueK2.Add(cur);
                    //    }
                    //    queueKK.RemoveAt(0);
                    //}

                    // Условный такт

                    #endregion

                    sh.timeStop--;
                    k1.timeStop--;
                    k2.timeStop--;

                    time++;

                }
            }
        }

        public class TimeChek
        {
            public int start;
            public int end;

            public TimeChek(int start, int end)
            {
                this.start = start;
                this.end = end;
            }
        }

        #endregion

        #region "отрисовка"
        private void ZeroComand(Graphics g, int i, int n, int line)
        {
            g.DrawLine(new Pen(Color.White, 4), new Point(i * WIDTH, LOCAL_LAINS[line]), new Point(i * WIDTH, LOCAL_LAINS[line]-HEIGHT-20));

            var rect = new Rectangle(i * WIDTH + 5, LOCAL_LAINS[line] - HEIGHT - 20, 15, 15);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            TextRenderer.DrawText(g, n.ToString(), new Font("Arial", 9), rect, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        private void OneComand(Graphics g, int i, int n, int line)
        {
            g.DrawPolygon(
                new Pen(Color.White, 3),
                new Point[]{
                new Point(i *     WIDTH, LOCAL_LAINS[line]),
                new Point(i *     WIDTH, LOCAL_LAINS[line] - HEIGHT),
                new Point((i+1) * WIDTH, LOCAL_LAINS[line] - HEIGHT),
                new Point((i+1) * WIDTH, LOCAL_LAINS[line])});

            var rect = new Rectangle(i * WIDTH + 6, LOCAL_LAINS[line] - HEIGHT + 6, 15 , 15);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            TextRenderer.DrawText(g, n.ToString(), new Font("Arial", 10), rect, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        }
        private void TwoComand(Graphics g, int i, int n, int T, int line)
        {
            g.DrawPolygon(
                new Pen(Color.White, 3),
                new Point[]{
                new Point(i * WIDTH,       LOCAL_LAINS[line]),
                new Point(i * WIDTH,       LOCAL_LAINS[line] + HEIGHT),
                new Point((i+T) * WIDTH,   LOCAL_LAINS[line] + HEIGHT),
                new Point((i+T) * WIDTH,   LOCAL_LAINS[line])
            });

            var rect = new Rectangle(i * WIDTH + 6, LOCAL_LAINS[line] + 12, 15 , 15 );

            g.SmoothingMode = SmoothingMode.AntiAlias;
            TextRenderer.DrawText(g, n.ToString(), new Font("Arial", 10), rect, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        private void ThreeComand(Graphics g, int i, int n, int T, int line)
        {
            Brush b = new SolidBrush(Color.FromArgb(255, 255, 255));

            g.FillPolygon(b,
                new Point[]{
                new Point(i * WIDTH,       LOCAL_LAINS[line]),
                new Point(i * WIDTH,       LOCAL_LAINS[line] + HEIGHT),
                new Point((i+2*T) * WIDTH, LOCAL_LAINS[line] + HEIGHT),
                new Point((i+2*T) * WIDTH, LOCAL_LAINS[line])
            });

            var rect = new Rectangle(i * WIDTH + 6, LOCAL_LAINS[line] + 12, 15 , 15 );

            g.SmoothingMode = SmoothingMode.AntiAlias;
            TextRenderer.DrawText(g, n.ToString(), new Font("Arial", 10), rect, Color.Black,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        private void FourComand(Graphics g, int i, int n)
        {
            Brush b = new SolidBrush(Color.FromArgb(255, 255, 255));

            g.FillPolygon(b,
                new Point[]{
                new Point(i * WIDTH, LOCAL_Three_LINE),
                new Point(i * WIDTH, LOCAL_Three_LINE - HEIGHT),
                new Point((i+6) * WIDTH, LOCAL_Three_LINE - HEIGHT),
                new Point((i+6) * WIDTH,LOCAL_Three_LINE)
            });

            var rect = new Rectangle(i * WIDTH + 6, LOCAL_Three_LINE - HEIGHT + 6, 15, 15);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            TextRenderer.DrawText(g, n.ToString(), new Font("Arial", 10), rect, Color.Black,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        private void FiveComand(Graphics g, int i, int n, int T)
        {
            Brush b = new SolidBrush(Color.FromArgb(255, 255, 255));

            g.FillPolygon(b,
                new Point[]{
                new Point(i * WIDTH, LOCAL_FOUR_LINE),
                new Point(i * WIDTH, LOCAL_FOUR_LINE - HEIGHT),
                new Point((i+T*fss) * WIDTH, LOCAL_FOUR_LINE - HEIGHT),
                new Point((i+T*fss) * WIDTH,LOCAL_FOUR_LINE)
            });

            var rect = new Rectangle(i * WIDTH + 6, LOCAL_FOUR_LINE - HEIGHT + 6, 15 + (n % 10), 15 +  (n % 10));

            g.SmoothingMode = SmoothingMode.AntiAlias;
            TextRenderer.DrawText(g, n.ToString(), new Font("Arial", 10), rect, Color.Black,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        private void DrawPole(Graphics g, int j)
        {

            for (int i = 0;i < LOCAL_LAINS.Length; i++)
            {
                g.DrawLine(new Pen(Color.White, 4), new Point(0, LOCAL_LAINS[i]), new Point(2000, LOCAL_LAINS[i]));
            }

            for (int n = 0; n < LOCAL_LAINS.Length; n++) 
            {
                for (int i = 0; i < LENTH_LINE; i+= WIDTH)
                {
                    g.DrawLine(new Pen(Color.White, 1), new Point(i, LOCAL_LAINS[n] + 5), new Point(i, LOCAL_LAINS[n] - 5));
                    TextRenderer.DrawText(g, (j + i / WIDTH).ToString(), new Font("Arial", 5), new Point(j + i, LOCAL_LAINS[n] +  10), Color.White);
                } 
            }

        }


        #endregion

        #region интерфейс

        public List<Comand> curCOM = new List<Comand>();

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            curCOM.Add(new Comand(2, false, 3));
            richTextBox1.Text += curCOM.Count.ToString() + ") 2 т (н.к,уо)\n";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            curCOM.Add(new Comand(1, false, 3));
            richTextBox1.Text += curCOM.Count.ToString() + ") 1 т (н.к,уо)\n";
        }

        private void button9_Click(object sender, EventArgs e)
        {
            curCOM.Add(new Comand(1, false, 2));
            richTextBox1.Text += curCOM.Count.ToString() + ") 1 т (к.к,--)\n";
        }

        private void button10_Click(object sender, EventArgs e)
        {
            curCOM.Add(new Comand(2, false, 2));
            richTextBox1.Text += curCOM.Count.ToString() + ") 2 т (к.к,--)\n";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            curCOM.Add(new Comand(1,true,3));
            richTextBox1.Text += curCOM.Count.ToString() + ") 1 т (кэш,уо)\n";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            comands = new Comand[curCOM.Count];
            for (int i = 0; i < curCOM.Count; i++)
                comands[i] = curCOM[i];
        }

        private void label1_Click(object sender, EventArgs e)
        {
            label1.Text = "Количество тактов";

        }

        private void button2_Click(object sender, EventArgs e)
        {
            curCOM.Add(new Comand(2, true, 3));
            richTextBox1.Text += curCOM.Count.ToString() + ") 2 т (кэш,уо)\n";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            curCOM.Add(new Comand(2, true, 2));
            richTextBox1.Text += curCOM.Count.ToString() + ") 2 т (кэш,--)\n";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            curCOM.Add(new Comand(1, true, 2));
            richTextBox1.Text += curCOM.Count.ToString() + ") 1 т (кэш,--)\n";
        }

        private void button8_Click(object sender, EventArgs e)
        {
            curCOM.Add(new Comand(4, true, 5));
            richTextBox1.Text += curCOM.Count.ToString() + ") 4 т (ДМА)\n";
        }
        #endregion
    }
}

  

