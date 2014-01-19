using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace B32Machine
{
    public partial class B32Screen : UserControl
    {
        #region [Private Variables]
        /// <summary>
        /// Backing field for screen memory location
        /// </summary>
        private ushort m_ScreenMemoryLocation;

        /// <summary>
        /// Backing field for screen memory
        /// </summary>
        private byte[] m_ScreenMemory;
        #endregion

        #region [Public Members]
        /// <summary>
        /// Location of the Screen Memory
        /// </summary>
        public ushort ScreenMemoryLocation
        {
            get
            {
                return m_ScreenMemoryLocation;
            }
            set
            {
                m_ScreenMemoryLocation = value;
            }
        }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public B32Screen()
        {
            InitializeComponent();

            //Initialize Screen Memory
            //Memory begins at 0xA000 and is 4Kb
            m_ScreenMemoryLocation = 0xA000;
            m_ScreenMemory = new byte[4000];
            Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < 4000; i += 2)
            {
                m_ScreenMemory[i] = 32;
                m_ScreenMemory[i + 1] = 7;
            }
            Refresh();
        }

        /// <summary>
        /// Loads a value into the area specified in memory
        /// </summary>
        /// <param name="Address">The Address to change</param>
        /// <param name="Value">The value to load</param>
        public void Poke(ushort Address, byte Value)
        {
            ushort MemLoc;

            //Attempt to parse location
            try
            {
                MemLoc = (ushort)(Address - m_ScreenMemoryLocation);
            }
            catch (Exception)
            {
                return;
            }

            //invalid memory location
            if (MemLoc < 0 || MemLoc > 3999)
                return;

            //load value and refresh
            m_ScreenMemory[MemLoc] = Value;
            Refresh();
        }

        /// <summary>
        /// Peeks at a location in the screen memory
        /// </summary>
        /// <param name="Address">The Address to look at</param>
        /// <returns>The byte value at the address</returns>
        public byte Peek(ushort Address)
        {
            ushort MemLoc;

            //attempt to parse memory location
            try
            {
                MemLoc = (ushort)(Address - m_ScreenMemoryLocation);
            }
            catch (Exception)
            {
                return (byte)0;
            }

            //memory out of bounds
            if (MemLoc < 0 || MemLoc > 3999)
                return (byte)0;

            return m_ScreenMemory[MemLoc];
        }

        /// <summary>
        /// Paints the screen based on memory values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void B32Screen_Paint(object sender, PaintEventArgs e)
        {
            //Create bitmap and graphics, font, starting x,y locations
            Bitmap bmp = new Bitmap(this.Width, this.Height);
            Graphics bmpGraphics = Graphics.FromImage(bmp);
            Font f = new Font("Courier New", 10f, FontStyle.Bold);
            int xLoc = 0;
            int yLoc = 0;

            //Paint all positions in memory
            for (int i = 0; i < 4000; i += 2)
            {
                SolidBrush bgBrush = null;
                SolidBrush fgBrush = null;

                #region [BackGround]
                if ((m_ScreenMemory[i + 1] & 112) == 112)
                {
                    bgBrush = new SolidBrush(Color.Gray);
                }
                if ((m_ScreenMemory[i + 1] & 112) == 96)
                {
                    bgBrush = new SolidBrush(Color.Brown);
                }
                if ((m_ScreenMemory[i + 1] & 112) == 80)
                {
                    bgBrush = new SolidBrush(Color.Magenta);
                }
                if ((m_ScreenMemory[i + 1] & 112) == 64)
                {
                    bgBrush = new SolidBrush(Color.Red);
                }
                if ((m_ScreenMemory[i + 1] & 112) == 48)
                {
                    bgBrush = new SolidBrush(Color.Cyan);
                }
                if ((m_ScreenMemory[i + 1] & 112) == 32)
                {
                    bgBrush = new SolidBrush(Color.Green);
                }
                if ((m_ScreenMemory[i + 1] & 112) == 16)
                {
                    bgBrush = new SolidBrush(Color.Blue);
                }
                if ((m_ScreenMemory[i + 1] & 112) == 0)
                {
                    bgBrush = new SolidBrush(Color.Black);
                }
                #endregion

                #region [ForeGround]
                if ((m_ScreenMemory[i + 1] & 7) == 0)
                {
                    if ((m_ScreenMemory[i + 1] & 8) == 8)
                    {
                        fgBrush = new SolidBrush(Color.Gray);
                    }
                    else
                    {
                        fgBrush = new SolidBrush(Color.Black);
                    }
                }
                if ((m_ScreenMemory[i + 1] & 7) == 1)
                {
                    if ((m_ScreenMemory[i + 1] & 8) == 8)
                    {
                        fgBrush = new SolidBrush(Color.LightBlue);
                    }
                    else
                    {
                        fgBrush = new SolidBrush(Color.Blue);
                    }
                }
                if ((m_ScreenMemory[i + 1] & 7) == 2)
                {
                    if ((m_ScreenMemory[i + 1] & 8) == 8)
                    {
                        fgBrush = new SolidBrush(Color.LightGreen);
                    }
                    else
                    {
                        fgBrush = new SolidBrush(Color.Green);
                    }
                }
                if ((m_ScreenMemory[i + 1] & 7) == 3)
                {
                    if ((m_ScreenMemory[i + 1] & 8) == 8)
                    {
                        fgBrush = new SolidBrush(Color.LightCyan);
                    }
                    else
                    {
                        fgBrush = new SolidBrush(Color.Cyan);
                    }
                }
                if ((m_ScreenMemory[i + 1] & 7) == 4)
                {
                    if ((m_ScreenMemory[i + 1] & 8) == 8)
                    {
                        fgBrush = new SolidBrush(Color.Pink);
                    }
                    else
                    {
                        fgBrush = new SolidBrush(Color.Red);
                    }
                }
                if ((m_ScreenMemory[i + 1] & 7) == 5)
                {
                    if ((m_ScreenMemory[i + 1] & 8) == 8)
                    {
                        fgBrush = new SolidBrush(Color.Fuchsia);
                    }
                    else
                    {
                        fgBrush = new SolidBrush(Color.Magenta);
                    }
                }
                if ((m_ScreenMemory[i + 1] & 7) == 6)
                {
                    if ((m_ScreenMemory[i + 1] & 8) == 8)
                    {
                        fgBrush = new SolidBrush(Color.Yellow);
                    }
                    else
                    {
                        fgBrush = new SolidBrush(Color.Brown);
                    }
                }
                if ((m_ScreenMemory[i + 1] & 7) == 7)
                {
                    if ((m_ScreenMemory[i + 1] & 8) == 8)
                    {
                        fgBrush = new SolidBrush(Color.White);
                    }
                    else
                    {
                        fgBrush = new SolidBrush(Color.Gray);
                    }
                }
                #endregion

                //Defaults
                if (bgBrush == null)
                    bgBrush = new SolidBrush(Color.Black);
                if (fgBrush == null)
                    fgBrush = new SolidBrush(Color.Gray);

                //Newline
                if (((xLoc % 640) == 0) && (xLoc != 0))
                {
                    yLoc += 11;
                    xLoc = 0;
                }
                string s = System.Text.Encoding.ASCII.GetString(m_ScreenMemory, i, 1);
                PointF pf = new PointF(xLoc, yLoc);

                //Paint the background and text
                bmpGraphics.FillRectangle(bgBrush, xLoc + 2, yLoc + 2, 8f, 11f);
                bmpGraphics.DrawString(s, f, fgBrush, pf);
                xLoc += 8;
            }

            //Draw final bitmap to the screen
            e.Graphics.DrawImage(bmp, new Point(0, 0));
            bmpGraphics.Dispose();
            bmp.Dispose();
        }
    }
}
