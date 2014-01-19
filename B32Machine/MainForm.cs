using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace B32Machine
{
    public partial class MainForm : Form
    {
        #region [Private Members]
        /// <summary>
        /// 64K Memory
        /// </summary>
        private byte[] B32Memory;

        /// <summary>
        /// Starting address of the binary file
        /// </summary>
        private ushort StartAddr;

        /// <summary>
        /// Execution address of the binary file
        /// </summary>
        private ushort ExecAddr;

        /// <summary>
        /// Address of next bytecode to be executed
        /// </summary>
        private ushort InstructionPointer;

        /// <summary>
        /// Value of 8-bit Register A
        /// </summary>
        private byte Register_A;

        /// <summary>
        /// Value of 8-bit Register B
        /// </summary>
        private byte Register_B;

        /// <summary>
        /// Value of 16-bit Register X
        /// </summary>
        private ushort Register_X;

        /// <summary>
        /// Value of 16-bit Register Y
        /// </summary>
        private ushort Register_Y;

        /// <summary>
        /// Value of 16-bit Register D
        /// Contains Register A concatenated with Register B
        /// </summary>
        private ushort Register_D;


        /// <summary>
        /// Byte used to set compare flags
        /// </summary>
        private byte CompareFlag;

        /// <summary>
        /// Time Delay from 0 to 10MHz to 0.25Hz
        /// </summary>
        private TimeSpan TimeDelay;

        /// <summary>
        /// Variable for the flags
        /// </summary>
        private byte ProcessorFlags;

        /// <summary>
        /// Thread on which the execution program is run
        /// </summary>
        private Thread program;

        /// <summary>
        /// ManualResetEvent to control stopping and starting the program
        /// </summary>
        private ManualResetEvent PauseEvent;
        #endregion

        /// <summary>
        /// Delegate for setting the text on the register bar
        /// </summary>
        /// <param name="text">Text to display on register bar</param>
        delegate void SetTextCallback(string text);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="value"></param>
        delegate void PokeCallback(ushort addr, byte value);

        /// <summary>
        /// Constructor
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            program = null;
            ProcessorFlags = 0;
            TimeDelay = TimeSpan.FromSeconds(0);
            realTime0SecondsToolStripMenuItem.Checked = true;
            resumeToolStripMenuItem.Enabled = false;
            pauseToolStripMenuItem.Enabled = true;
            B32Memory = new byte[65535];    //initialize 64Kb of memory
            CompareFlag = 0;
            StartAddr = 0;
            ExecAddr = 0;
            Register_A = 0;
            Register_B = 0;
            Register_D = 0;
            Register_X = 0;
            Register_Y = 0;
            UpdateRegisterStatus();
        }

        /// <summary>
        /// Updates the Register values to the display label
        /// </summary>
        private void UpdateRegisterStatus()
        {
            string strRegisters = string.Format(
                "Register A = ${0},     " +
                "Register B = ${1},     " +
                "Register D = ${2},\n" +
                "Register X = ${3},   " +
                "Register Y = ${4},   " +
                "\nInstruction Pointer = ${5}   " +
                "CompareFlag = ${6}",
                Register_A.ToString("X").PadLeft(2, '0'),
                Register_B.ToString("X").PadLeft(2, '0'),
                Register_D.ToString("X").PadLeft(4, '0'),
                Register_X.ToString("X").PadLeft(4, '0'),
                Register_Y.ToString("X").PadLeft(4, '0'),
                InstructionPointer.ToString("X").PadLeft(4, '0'),
                CompareFlag.ToString("X").PadLeft(2, '0'));

            if (lblRegisters.InvokeRequired)
            {
                SetTextCallback z = new SetTextCallback(SetRegisterText);
                this.Invoke(z, new object[] { strRegisters });
            }
            else
            {
                SetRegisterText(strRegisters);
            }
        }


        /// <summary>
        /// Opens a B32 file and begins file execution
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte Magic1;    //First character of the header 'B'
            byte Magic2;    //Second character of the header '3'
            byte Magic3;    //Third character of the header '2'
            DialogResult dr = fdOpenB32File.ShowDialog();

            if (dr == DialogResult.Cancel) return;

            lock (b32Screen1)
            {
                b32Screen1.Reset();
            }

            string FileName = fdOpenB32File.FileName;

            if (!String.IsNullOrEmpty(FileName) && File.Exists(FileName))
            {
                BinaryReader br;
                FileStream fs = new FileStream(FileName, FileMode.Open);
                br = new BinaryReader(fs);

                //check header
                Magic1 = br.ReadByte();
                Magic2 = br.ReadByte();
                Magic3 = br.ReadByte();
                if (Magic1 != 'B' && Magic2 != '3' && Magic3 != '2')
                {
                    MessageBox.Show("ERROR: INVALID FILE. CHECK B32 HEADER", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //Read program into memory
                StartAddr = br.ReadUInt16();
                ExecAddr = br.ReadUInt16();
                ushort Counter = 0;
                while (br.PeekChar() != -1)
                {
                    B32Memory[(StartAddr + Counter)] = br.ReadByte();
                    Counter++;
                }

                br.Close();
                fs.Close();

                //Begin Execution on new thread
                InstructionPointer = ExecAddr;
                program = new Thread(delegate() { ExecuteProgram(ExecAddr, Counter); });
                PauseEvent = new ManualResetEvent(true);
                program.Start();
            }
        }

        /// <summary>
        /// Execute the Program
        /// </summary>
        /// <param name="ExecAddr">Execution Address</param>
        /// <param name="ProgLength">Program Length</param>
        private void ExecuteProgram(ushort ExecAddr, ushort ProgLength)
        {
            ProgLength = 64000;
            //read program to end
            while (ProgLength > 0)
            {
                //time delay
                Thread.Sleep(TimeDelay);
                PauseEvent.WaitOne(Timeout.Infinite);

                //read instruction
                byte Instruction = B32Memory[InstructionPointer];
                ProgLength--;
                #region [Load]
                //LDA #<value>
                if (Instruction == 0x01)
                {
                    Register_A = B32Memory[(InstructionPointer + 1)];
                    SetRegisterD();
                    ProgLength -= 1;
                    InstructionPointer += 2;

                    UpdateRegisterStatus();

                    continue;
                }
                //LDB #<value>
                if (Instruction == 0x22)
                {
                    Register_B = B32Memory[(InstructionPointer + 1)];
                    SetRegisterD();
                    ProgLength -= 1;
                    InstructionPointer += 2;

                    UpdateRegisterStatus();

                    continue;
                }
                //LDX #<value>
                if (Instruction == 0x02)
                {
                    Register_X = (ushort)((B32Memory[(InstructionPointer + 2)]) << 8);

                    Register_X += B32Memory[(InstructionPointer + 1)];
                    ProgLength -= 2;
                    InstructionPointer += 3;

                    UpdateRegisterStatus();

                    continue;
                }
                //LDY #<value>
                if (Instruction == 0x23)
                {
                    Register_Y = (ushort)((B32Memory[(InstructionPointer + 2)]) << 8);
                    Register_Y += B32Memory[(InstructionPointer + 1)];
                    ProgLength -= 2;
                    InstructionPointer += 3;

                    UpdateRegisterStatus();

                    continue;
                }
                #endregion
                #region [Store]
                //STA ,X
                if (Instruction == 0x03)
                {
                    B32Memory[Register_X] = Register_A;
                    ThreadPoke(Register_X, Register_A);
                    InstructionPointer++;

                    UpdateRegisterStatus();

                    continue;
                }
                #endregion
                #region [Compare]
                //CMPA
                if (Instruction == 0x05)
                {
                    byte CompValue = B32Memory[InstructionPointer + 1];

                    CompareFlag = 0;
                    
                    //Equal
                    if (Register_A == CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 1);
                    }
                    //Not Equal
                    if (Register_A != CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 2);
                    }
                    //Greater than
                    if (Register_A < CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 4);
                    }
                    //Less than
                    if (Register_A > CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 8);
                    }

                    InstructionPointer += 2;

                    UpdateRegisterStatus();

                    continue;
                }
                //CPMB
                if (Instruction == 0x06)
                {
                    byte CompValue = B32Memory[InstructionPointer + 1];

                    CompareFlag = 0;

                    //Equal
                    if (Register_B == CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 1);
                    }
                    //Not Equal
                    if (Register_B != CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 2);
                    }
                    //Greater than
                    if (Register_B < CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 4);
                    }
                    //Less than
                    if (Register_B > CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 8);
                    }

                    InstructionPointer += 2;

                    UpdateRegisterStatus();

                    continue;
                }
                //CPMX
                if (Instruction == 0x07)
                {
                    ushort CompValue = (ushort)(B32Memory[InstructionPointer + 2] << 8);
                    CompValue += B32Memory[(InstructionPointer + 1)];

                    CompareFlag = 0;

                    //Equal
                    if (Register_X == CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 1);
                    }
                    //Not Equal
                    if (Register_X != CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 2);
                    }
                    //Greater than
                    if (Register_X < CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 4);
                    }
                    //Less than
                    if (Register_X > CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 8);
                    }

                    InstructionPointer += 3;

                    UpdateRegisterStatus();

                    continue;
                }
                //CPMY
                if (Instruction == 0x08)
                {
                    ushort CompValue = (ushort)(B32Memory[InstructionPointer + 2] << 8);
                    CompValue += B32Memory[(InstructionPointer + 1)];

                    CompareFlag = 0;

                    //Equal
                    if (Register_Y == CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 1);
                    }
                    //Not Equal
                    if (Register_Y != CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 2);
                    }
                    //Greater than
                    if (Register_Y < CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 4);
                    }
                    //Less than
                    if (Register_Y > CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 8);
                    }

                    InstructionPointer += 3;

                    UpdateRegisterStatus();

                    continue;
                }
                //CPMD
                if (Instruction == 0x09)
                {
                    ushort CompValue = (ushort)(B32Memory[InstructionPointer + 2] << 8);
                    CompValue += B32Memory[(InstructionPointer + 1)];

                    CompareFlag = 0;

                    //Equal
                    if (Register_D == CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 1);
                    }
                    //Not Equal
                    if (Register_D != CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 2);
                    }
                    //Greater than
                    if (Register_D < CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 4);
                    }
                    //Less than
                    if (Register_D > CompValue)
                    {
                        CompareFlag = (byte)(CompareFlag | 8);
                    }

                    InstructionPointer += 3;

                    UpdateRegisterStatus();

                    continue;
                }
                #endregion
                #region [Jump]
                //JMP
                if (Instruction == 0x0A)
                {
                    ushort JmpValue = (ushort)((B32Memory[(InstructionPointer + 2)]) << 8);
                    JmpValue += B32Memory[(InstructionPointer + 1)];

                    InstructionPointer = JmpValue;

                    UpdateRegisterStatus();

                    continue;
                }
                //JEQ
                if (Instruction == 0x0B)
                {
                    ushort JmpValue = (ushort)((B32Memory[(InstructionPointer + 2)]) << 8);
                    JmpValue += B32Memory[(InstructionPointer + 1)];

                    if ((CompareFlag & 1) == 1)
                    {
                        InstructionPointer = JmpValue;
                    }
                    else
                    {
                        InstructionPointer += 3;
                    }

                    UpdateRegisterStatus();

                    continue;
                }
                //JNE
                if (Instruction == 0x0C)
                {
                    ushort JmpValue = (ushort)((B32Memory[(InstructionPointer + 2)]) << 8);
                    JmpValue += B32Memory[(InstructionPointer + 1)];

                    if ((CompareFlag & 2) == 2)
                    {
                        InstructionPointer = JmpValue;
                    }
                    else
                    {
                        InstructionPointer += 3;
                    }

                    UpdateRegisterStatus();

                    continue;
                }
                //JGT
                if (Instruction == 0x0D)
                {
                    ushort JmpValue = (ushort)((B32Memory[(InstructionPointer + 2)]) << 8);
                    JmpValue += B32Memory[(InstructionPointer + 1)];

                    if ((CompareFlag & 4) == 4)
                    {
                        InstructionPointer = JmpValue;
                    }
                    else
                    {
                        InstructionPointer += 3;
                    }

                    UpdateRegisterStatus();

                    continue;
                }
                //JLT
                if (Instruction == 0x0E)
                {
                    ushort JmpValue = (ushort)((B32Memory[(InstructionPointer + 2)]) << 8);
                    JmpValue += B32Memory[(InstructionPointer + 1)];

                    if ((CompareFlag & 8) == 8)
                    {
                        InstructionPointer = JmpValue;
                    }
                    else
                    {
                        InstructionPointer += 3;
                    }

                    UpdateRegisterStatus();

                    continue;
                }
                #endregion
                #region [Increment]
                //INCA
                if (Instruction == 0x0F)
                {
                    if (Register_A == 0xFF)
                    {
                        ProcessorFlags = (byte)(ProcessorFlags | 1);
                    }
                    else
                    {
                        ProcessorFlags = (byte)(ProcessorFlags & 0xFE);
                    }

                    unchecked { Register_A++; }
                    SetRegisterD();
                    InstructionPointer++;
                    UpdateRegisterStatus();
                    continue;
                }
                //INCB
                if (Instruction == 0x10)
                {
                    if (Register_B == 0xFF)
                    {
                        ProcessorFlags = (byte)(ProcessorFlags | 1);
                    }
                    else
                    {
                        ProcessorFlags = (byte)(ProcessorFlags & 0xFE);
                    }

                    unchecked { Register_B++; }
                    SetRegisterD();
                    InstructionPointer++;
                    UpdateRegisterStatus();
                    continue;
                }
                //INCX
                if (Instruction == 0x11)
                {
                    if (Register_X == 0xFFFF)
                    {
                        ProcessorFlags = (byte)(ProcessorFlags | 1);
                    }
                    else
                    {
                        ProcessorFlags = (byte)(ProcessorFlags & 0xFE);
                    }

                    unchecked { Register_X++; }
                    InstructionPointer++;
                    UpdateRegisterStatus();
                    continue;
                }
                //INCY
                if (Instruction == 0x12)
                {
                    if (Register_Y == 0xFFFF)
                    {
                        ProcessorFlags = (byte)(ProcessorFlags | 1);
                    }
                    else
                    {
                        ProcessorFlags = (byte)(ProcessorFlags & 0xFE);
                    }

                    unchecked { Register_Y++; }
                    InstructionPointer++;
                    UpdateRegisterStatus();
                    continue;
                }
                //INCD
                if (Instruction == 0x13)
                {
                    if (Register_X == 0xFFFF)
                    {
                        ProcessorFlags = (byte)(ProcessorFlags | 1);
                    }
                    else
                    {
                        ProcessorFlags = (byte)(ProcessorFlags & 0xFE);
                    }

                    unchecked
                    {
                        Register_D++;
                        Register_A = (byte)(Register_D >> 8);
                        Register_B = (byte)(Register_D & 255);
                    }
                    InstructionPointer++;
                    UpdateRegisterStatus();
                    continue;
                }
                #endregion
                #region [Decrement]
                //DECA
                if (Instruction == 0x14)
                {
                    ProcessorFlags = (byte)(ProcessorFlags & 0xFE);
                    unchecked { Register_A--; }
                    SetRegisterD();
                    InstructionPointer++;
                    UpdateRegisterStatus();
                    continue;
                }
                //DECB
                if (Instruction == 0x15)
                {
                    ProcessorFlags = (byte)(ProcessorFlags & 0xFE);
                    unchecked { Register_B--; }
                    SetRegisterD();
                    InstructionPointer++;
                    UpdateRegisterStatus();
                    continue;
                }
                //DECX
                if (Instruction == 0x16)
                {
                    ProcessorFlags = (byte)(ProcessorFlags & 0xFE);
                    unchecked { Register_X--; }
                    InstructionPointer++;
                    UpdateRegisterStatus();
                    continue;
                }
                //DECY
                if (Instruction == 0x17) 
                {
                    ProcessorFlags = (byte)(ProcessorFlags & 0xFE);
                    unchecked { Register_Y--; }
                    InstructionPointer++;
                    UpdateRegisterStatus();
                    continue;
                }
                #endregion
                #region [Rotate]
                //ROLA
                if (Instruction == 0x19)
                {
                    byte OldCarryFlag = (byte)(ProcessorFlags & 2);
                    if ((Register_A & 128) == 128)
                    {
                        ProcessorFlags = (byte)(ProcessorFlags | 2);
                    }
                    else
                    {
                        ProcessorFlags = (byte)(ProcessorFlags & 0xFD);
                    }
                    Register_A = (byte)(Register_A << 1);
                    if (OldCarryFlag > 0)
                    {
                        Register_A = (byte)(Register_A | 1);
                    }
                    SetRegisterD();
                    InstructionPointer++;
                    UpdateRegisterStatus();
                    continue;
                }
                //ROLB
                if (Instruction == 0x1A)
                {
                    byte OldCarryFlag = (byte)(ProcessorFlags & 2);
                    if ((Register_B & 128) == 128)
                    {
                        ProcessorFlags = (byte)(ProcessorFlags | 2);
                    }
                    else
                    {
                        ProcessorFlags = (byte)(ProcessorFlags & 0xFD);
                    }
                    Register_B = (byte)(Register_B << 1);
                    if (OldCarryFlag > 0)
                    {
                        Register_B = (byte)(Register_B | 1);
                    }
                    SetRegisterD();
                    InstructionPointer++;
                    UpdateRegisterStatus();
                    continue;
                }
                //RORA
                if (Instruction == 0x1B)
                {
                    byte OldCarryFlag = (byte)(ProcessorFlags & 2);
                    if ((Register_A & 1) == 1)
                    {
                        ProcessorFlags = (byte)(ProcessorFlags | 2);
                    }
                    else
                    {
                        ProcessorFlags = (byte)(ProcessorFlags & 0xFD);
                    }
                    Register_A = (byte)(Register_A >> 1);
                    if (OldCarryFlag > 0)
                    {
                        Register_A = (byte)(Register_A | 128);
                    }
                    SetRegisterD();
                    InstructionPointer++;
                    UpdateRegisterStatus();
                    continue;
                }
                //RORB
                if (Instruction == 0x1C)
                {
                    byte OldCarryFlag = (byte)(ProcessorFlags & 2);
                    if ((Register_B & 1) == 1)
                    {
                        ProcessorFlags = (byte)(ProcessorFlags | 2);
                    }
                    else
                    {
                        ProcessorFlags = (byte)(ProcessorFlags & 0xFD);
                    }
                    Register_B = (byte)(Register_B >> 1);
                    if (OldCarryFlag > 0)
                    {
                        Register_B = (byte)(Register_B | 128);
                    }
                    SetRegisterD();
                    InstructionPointer++;
                    UpdateRegisterStatus();
                    continue;
                }
                #endregion
                #region [Add]
                //ADCA
                if (Instruction == 0x1D) 
                {
                    if ((byte)(ProcessorFlags & 2) == 2)
                    {
                        if (Register_A == 0xFF)
                        {
                            ProcessorFlags = (byte)(ProcessorFlags | 1);
                        }
                        else
                        {
                            ProcessorFlags = (byte)(ProcessorFlags & 0xFE);
                        }
                        unchecked { Register_A++; }
                        SetRegisterD();
                    }
                    InstructionPointer++;
                    UpdateRegisterStatus();
                    continue;
                }
                //ADCB
                if (Instruction == 0x1E)
                {
                    if ((byte)(ProcessorFlags & 2) == 2)
                    {
                        if (Register_B == 0xFF)
                        {
                            ProcessorFlags = (byte)(ProcessorFlags | 1);
                        }
                        else
                        {
                            ProcessorFlags = (byte)(ProcessorFlags & 0xFE);
                        }
                        unchecked { Register_B++; }
                        SetRegisterD();
                    }
                    InstructionPointer++;
                    UpdateRegisterStatus();
                    continue;
                }
                //ADDA
                if (Instruction == 0x1F)
                {
                    byte val = B32Memory[(InstructionPointer + 1)];
                    if (Register_A == 0xFF && val > 0)
                    {
                        ProcessorFlags = (byte)(ProcessorFlags | 1);
                    }
                    else
                    {
                        ProcessorFlags = (byte)(ProcessorFlags & 0xFE);
                    }
                    unchecked { Register_A += val; }
                    SetRegisterD();
                    InstructionPointer += 2;
                    UpdateRegisterStatus();
                    continue;
                }
                //ADDB
                if (Instruction == 0x20) 
                {
                    byte val = B32Memory[(InstructionPointer + 1)];
                    if (Register_B == 0xFF && val > 0)
                    {
                        ProcessorFlags = (byte)(ProcessorFlags | 1);
                    }
                    else
                    {
                        ProcessorFlags = (byte)(ProcessorFlags & 0xFE);
                    }
                    unchecked { Register_B += val; }
                    SetRegisterD();
                    InstructionPointer += 2;
                    UpdateRegisterStatus();
                    continue;
                }
                //ADDAB
                if (Instruction == 0x21)
                {
                    if ((255 - Register_A) > Register_B)
                    {
                        ProcessorFlags = (byte)(ProcessorFlags | 1);
                    }
                    else
                    {
                        ProcessorFlags = (byte)(ProcessorFlags & 0xFE);
                    }
                    unchecked { Register_D = (ushort)(((ushort)Register_B) + ((ushort)Register_A)); }

                    Register_A = (byte)(Register_D >> 8);
                    Register_B = (byte)(Register_D & 255);

                    InstructionPointer++;
                    UpdateRegisterStatus();
                    continue;
                }
                #endregion
                //END
                if (Instruction == 0x04)
                {
                    InstructionPointer++;
                    UpdateRegisterStatus();
                    break;
                }
            }
        }

        /// <summary>
        /// Updates Register D to be the concatenation of Register A and Register B
        /// </summary>
        private void SetRegisterD()
        {
            Register_D = (ushort)(Register_A << 8 + Register_B);
        }

        #region [Time Delay Handlers]
        /// <summary>
        /// Change time delay to 0 seconds (real time)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void realTime0SecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UncheckAll();
            ((ToolStripMenuItem)sender).Checked = true;
            TimeDelay = TimeSpan.FromTicks(0);
        }

        /// <summary>
        /// Change time delay to 100 ns
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mHz100NanoSecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UncheckAll();
            ((ToolStripMenuItem)sender).Checked = true;
            TimeDelay = TimeSpan.FromTicks(1);
        }

        /// <summary>
        /// Change time delay to 200 ns
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mHz200NanoSecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UncheckAll();
            ((ToolStripMenuItem)sender).Checked = true;
            TimeDelay = TimeSpan.FromTicks(2);
        }

        /// <summary>
        /// Change time delay to 500 ns
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mHz500NanoSecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UncheckAll();
            ((ToolStripMenuItem)sender).Checked = true;
            TimeDelay = TimeSpan.FromTicks(5);
        }

        /// <summary>
        /// Change time delay to 1000 ns
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mHz1000NanoSecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UncheckAll();
            ((ToolStripMenuItem)sender).Checked = true;
            TimeDelay = TimeSpan.FromTicks(1000);
        }

        /// <summary>
        /// Change time delay to 0.5 Seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hz05SecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UncheckAll();
            ((ToolStripMenuItem)sender).Checked = true;
            TimeDelay = TimeSpan.FromSeconds(0.5);
        }

        /// <summary>
        /// Change time delay to 1 Second
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hz1SecondToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UncheckAll();
            ((ToolStripMenuItem)sender).Checked = true;
            TimeDelay = TimeSpan.FromSeconds(1);
        }

        /// <summary>
        /// Change time delay to 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hz2SecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UncheckAll();
            ((ToolStripMenuItem)sender).Checked = true;
            TimeDelay = TimeSpan.FromSeconds(2);
        }

        /// <summary>
        /// Change time delay to 4 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hz4SecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UncheckAll();
            ((ToolStripMenuItem)sender).Checked = true;
            TimeDelay = TimeSpan.FromSeconds(4);
        }

        //Uncheck all time delay options in menu
        private void UncheckAll()
        {
            mHz100NanoSecondsToolStripMenuItem.Checked = false;
            mHz200NanoSecondsToolStripMenuItem.Checked = false;
            mHz500NanoSecondsToolStripMenuItem.Checked = false;
            mHz1000NanoSecondsToolStripMenuItem.Checked = false;
            hz05SecondsToolStripMenuItem.Checked = false;
            hz1SecondToolStripMenuItem.Checked = false;
            hz2SecondsToolStripMenuItem.Checked = false;
            hz4SecondsToolStripMenuItem.Checked = false;
            realTime0SecondsToolStripMenuItem.Checked = false;
        }
    #endregion

        /// <summary>
        /// Invokes the poke callback method
        /// </summary>
        /// <param name="Addr">Memory Address to poke</param>
        /// <param name="value">Value to poke</param>
        private void ThreadPoke(ushort Addr, byte value)
        {
            //invoke through delegate
            if (b32Screen1.InvokeRequired)
            {
                PokeCallback pcb = new PokeCallback(Poke);
                this.Invoke(pcb, new object[] { Addr, value });
            }
            else
            {
                Poke(Addr, value);
            }
        }

        /// <summary>
        /// Poke value to the screen
        /// </summary>
        /// <param name="Addr">Memory Address to poke</param>
        /// <param name="value">Value to poke to screen</param>
        private void Poke(ushort Addr, byte value)
        {
            lock (b32Screen1)
            {
                b32Screen1.Poke(Addr, value);
            }
        }

        /// <summary>
        /// Set register text
        /// </summary>
        /// <param name="text"></param>
        private void SetRegisterText(string text)
        {
            lblRegisters.Text = text;
        }

        /// <summary>
        /// Handler to resume program execution
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resumeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resumeToolStripMenuItem.Enabled = false;
            pauseToolStripMenuItem.Enabled = true;
            PauseEvent.Set();
        }

        /// <summary>
        /// Handler to pause program execution
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resumeToolStripMenuItem.Enabled = true;
            pauseToolStripMenuItem.Enabled = false;
            PauseEvent.Reset();
        }

        /// <summary>
        /// Program to restart execution
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Abort existing thread
            if (program != null)
            {
                program.Abort();
                program = null;
            }
            InstructionPointer = ExecAddr;
            resumeToolStripMenuItem.Enabled = false;
            pauseToolStripMenuItem.Enabled = true;

            //start new thread and execute
            program = new Thread(delegate() { ExecuteProgram(ExecAddr, 64000); });
            PauseEvent = new ManualResetEvent(true);
            b32Screen1.Reset();
            program.Start();
        }
    }
}
