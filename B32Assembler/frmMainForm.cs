using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace B32Assembler
{
    public partial class frmMainForm : Form
    {
        /// <summary>
        /// Virtual Machine Registers
        /// </summary>
        private enum Registers
        {
            /// <summary>
            /// Unknown Register
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// 8-bit Register A
            /// </summary>
            A = 4,
            /// <summary>
            /// 8-bit Register B
            /// </summary>
            B = 2,
            /// <summary>
            /// 16-bit Register D containing A concatonated with B
            /// </summary>
            D = 1,
            /// <summary>
            /// 16-bit Register X
            /// </summary>
            X = 16,
            /// <summary>
            /// 16-bit Register Y
            /// </summary>
            Y = 8
        }

        #region [Private Variables]
        /// <summary>
        /// Contains the program source code
        /// </summary>
        private string SourceProgram; 
        
        /// <summary>
        /// Hash table containing the program execution
        /// </summary>
        private System.Collections.Hashtable LabelTable;
  
        /// <summary>
        /// Current location in the source file
        /// </summary>
        private int CurrentNdx;             
        
        /// <summary>
        /// How big the current file is
        /// </summary>
        private ushort AsLength;   
        
        /// <summary>
        /// Flag for end of the program
        /// </summary>
        private bool IsEnd; 
        
        /// <summary>
        /// The program execution address
        /// </summary>
        private ushort ExecutionAddress;     
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public frmMainForm()
        {
            InitializeComponent();
            
            //Initialize Variables
            LabelTable = new System.Collections.Hashtable(50);
            CurrentNdx = 0;
            AsLength = 0;
            ExecutionAddress = 0;
            IsEnd = false;
            SourceProgram = "";
            txtOrigin.Text = "1000";
        }

        /// <summary>
        /// Click handler for browing to source file
        /// Openes fdSourceFile dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSourceBrowse_Click(object sender, EventArgs e)
        {
            this.fdSourceFile.ShowDialog();
            this.txtSourceFileName.Text = fdSourceFile.FileName;
        }

        /// <summary>
        /// Click handler for selecting output file
        /// Opens fdDestinationFile dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOutputBrowse_Click(object sender, EventArgs e)
        {
            this.fdDestinationFile.ShowDialog();
            this.txtOutputFileName.Text = fdDestinationFile.FileName;
        }

        /// <summary>
        /// Click handler to begin assembling the file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAssemble_Click(object sender, EventArgs e)
        {
            if (File.Exists(txtSourceFileName.Text))
            {
                if (!File.Exists(txtOutputFileName.Text))
                {
                    txtOutputFileName.Text = txtSourceFileName.Text.Replace("asm", "B32");
                }
                //Read the length of the file
                AsLength = Convert.ToUInt16(this.txtOrigin.Text, 16);

                //Create writer, reader, and filestream
                BinaryWriter output;
                TextReader input;
                FileStream fs = new FileStream(txtOutputFileName.Text, FileMode.Create);
                output = new BinaryWriter(fs);

                //Read the input file
                input = File.OpenText(this.txtSourceFileName.Text);
                SourceProgram = input.ReadToEnd();
                input.Close();

                //Write header, length, output
                output.Write('B');
                output.Write('3');
                output.Write('2');
                output.Write(AsLength);
                output.Write((ushort)0);
                Parse(output);

                //Write execution address and close
                output.Seek(5, SeekOrigin.Begin);
                output.Write(ExecutionAddress);
                output.Close();
                fs.Close();
                MessageBox.Show("Done!");
            }
            else
            {
                MessageBox.Show("ERROR: Invalid File");
            }
        }

        /// <summary>
        /// Parse the output file through LabelScan until EOF
        /// </summary>
        /// <param name="OutputFile"></param>
        private void Parse(BinaryWriter OutputFile)
        {
            //First scan, enumerating labels
            CurrentNdx = 0;
            while (!IsEnd)
            {
                LabelScan(OutputFile, true);
            }

            //Reset index and end flag
            IsEnd = false;
            CurrentNdx = 0;
            AsLength = Convert.ToUInt16(this.txtOrigin.Text, 16);

            //Second Scan
            while (!IsEnd)
            {
                LabelScan(OutputFile, false);
            }
        }

        /// <summary>
        /// Scans for label at the current index. Assumes there is a label if
        /// the character at the location is not whitespace
        /// </summary>
        /// <param name="OutputFile">Output file writer</param>
        /// <param name="IsLabelScan">Adding labels to LabelTable</param>
        private void LabelScan(BinaryWriter OutputFile, bool IsLabelScan)
        {
            //char is not whitespace
            if (char.IsLetter(SourceProgram[CurrentNdx]))
            {
                //add label to table
                if (IsLabelScan)
                {
                    LabelTable.Add(GetLabelName(), AsLength);
                }
                //skip to end of line
                while (SourceProgram[CurrentNdx] != '\n')
                {
                    CurrentNdx++;
                }
                CurrentNdx++;
                return;
            }
            //otherwise remove whitespace
            EatWhiteSpaces();
            //Read the Mneumonic
            ReadMneumonic(OutputFile, IsLabelScan);
        }

        /// <summary>
        /// Reads the next Mneumonic to appear
        /// </summary>
        /// <param name="OutputFile">Output file writer</param>
        /// <param name="IsLabelScan">Adding labels to LabelTable</param>
        private void ReadMneumonic(BinaryWriter OutputFile, bool IsLabelScan) 
        {
            //Reads in the next Mneumonic, adds non=whitespace chars
            string Mneumonic = string.Empty;
            while (!(char.IsWhiteSpace(SourceProgram[CurrentNdx])))
            {
                Mneumonic += SourceProgram[CurrentNdx];
                CurrentNdx++;
            }

            //Interpret the Mneumonic
            #region [Load Operations]
            //LDX -- Load into Register X
            if (Mneumonic.ToUpper() == "LDX")
                InterpretLDX(OutputFile, IsLabelScan);
            //LDY -- Load into Register Y
            if (Mneumonic.ToUpper() == "LDY")
                InterpretLDY(OutputFile, IsLabelScan);
            //LDA -- Load into Register A
            if (Mneumonic.ToUpper() == "LDA")
                InterpretLDA(OutputFile, IsLabelScan);
            //LDB -- Load into Register B
            if (Mneumonic.ToUpper() == "LDB")
                InterpretLDB(OutputFile, IsLabelScan);
            #endregion
            #region [Store Operations]
            //STA -- Store value in Register A
            if (Mneumonic.ToUpper() == "STA")
                InterpretSTA(OutputFile, IsLabelScan);
            #endregion
            #region [Compare Operations]
            //CMPA -- Compare with Register A
            if (Mneumonic.ToUpper() == "CMPA")
                InterpretCMPA(OutputFile, IsLabelScan);
            //CMPB -- Compare with Register B
            if (Mneumonic.ToUpper() == "CMPB")
                InterpretCMPB(OutputFile, IsLabelScan);
            //CMPD -- Compare with Register D
            if (Mneumonic.ToUpper() == "CMPD")
                InterpretCMPD(OutputFile, IsLabelScan);
            //CMPX -- Compare with Register X
            if (Mneumonic.ToUpper() == "CMPX")
                InterpretCMPX(OutputFile, IsLabelScan);
            //CMPY -- Compare with Register Y
            if (Mneumonic.ToUpper() == "CMPY")
                InterpretCMPY(OutputFile, IsLabelScan);
            #endregion
            #region [Jump Operations]
            //JMP -- Jump
            if (Mneumonic.ToUpper() == "JMP")
                InterpretJMP(OutputFile, IsLabelScan);
            //JEQ -- Jump on Equal
            if (Mneumonic.ToUpper() == "JEQ")
                InterpretJEQ(OutputFile, IsLabelScan);
            //JNE -- Jump on Not Equal
            if (Mneumonic.ToUpper() == "JNE")
                InterpretJNE(OutputFile, IsLabelScan);
            //JGT -- Jump on Greater Than
            if (Mneumonic.ToUpper() == "JGT")
                InterpretJGT(OutputFile, IsLabelScan);
            //JLT -- Jump on Less Than
            if (Mneumonic.ToUpper() == "JLT")
                InterpretJLT(OutputFile, IsLabelScan);
            #endregion
            #region [Increment Operations]
            //INCA -- Increment Register A
            if (Mneumonic.ToUpper() == "INCA")
                InterpretINCA(OutputFile, IsLabelScan);
            //INCB -- Increment Register B
            if (Mneumonic.ToUpper() == "INCB")
                InterpretINCB(OutputFile, IsLabelScan);
            //INCX -- Increment Register X
            if (Mneumonic.ToUpper() == "INCX")
                InterpretINCX(OutputFile, IsLabelScan);
            //INCY -- Increment Register Y
            if (Mneumonic.ToUpper() == "INCY")
                InterpretINCY(OutputFile, IsLabelScan);
            //INCD -- Increment Register D
            if (Mneumonic.ToUpper() == "INCD")
                InterpretINCY(OutputFile, IsLabelScan);
            #endregion
            #region [Decrement Operations]
            //DECA -- Decrement Register A
            if (Mneumonic.ToUpper() == "DECA")
                InterpretDECA(OutputFile, IsLabelScan);
            //DECB -- Decrement Register B
            if (Mneumonic.ToUpper() == "DECB")
                InterpretDECB(OutputFile, IsLabelScan);
            //DECX -- Decrement Register X
            if (Mneumonic.ToUpper() == "DECX")
                InterpretDECX(OutputFile, IsLabelScan);
            //DECY -- Decrement Register Y
            if (Mneumonic.ToUpper() == "DECY")
                InterpretDECY(OutputFile, IsLabelScan);
            //DECD -- Decrement Register D
            if (Mneumonic.ToUpper() == "DECD")
                InterpretDECD(OutputFile, IsLabelScan);
            #endregion
            #region [Rotate Operations]
            //ROLA -- Rotate Register A Left
            if (Mneumonic.ToUpper() == "ROLA")
                InterpretROLA(OutputFile, IsLabelScan);
            //ROLB -- Rotate Register B Left
            if (Mneumonic.ToUpper() == "ROLB")
                InterpretROLB(OutputFile, IsLabelScan);
            //RORA -- Rotate Register A Right
            if (Mneumonic.ToUpper() == "RORA")
                InterpretRORA(OutputFile, IsLabelScan);
            //RORB -- Rotate Register B Right
            if (Mneumonic.ToUpper() == "RORB")
                InterpretRORB(OutputFile, IsLabelScan);
            #endregion
            #region [Add Operations]
            //ADCA -- Add 1 to Register A if Carry Flag is set
            if (Mneumonic.ToUpper() == "ADCA")
                InterpretADCA(OutputFile, IsLabelScan);
            //ADCB -- Add 1 to Register B if Carry Flag is set
            if (Mneumonic.ToUpper() == "ADCB")
                InterpretADCB(OutputFile, IsLabelScan);
            //ADDA -- Adds value to the A Register
            if (Mneumonic.ToUpper() == "ADDA")
                InterpretADDA(OutputFile, IsLabelScan);
            //ADDB -- Adds value to the B Register
            if (Mneumonic.ToUpper() == "ADDB")
                InterpretADDB(OutputFile, IsLabelScan);
            //ADDAB -- Adds the value of Register A to Register B, store in D
            if (Mneumonic.ToUpper() == "ADDAB")
                InterpretADDAB(OutputFile, IsLabelScan);
            #endregion
            //END -- Set IsEnd flag, execution address
            if (Mneumonic.ToUpper() == "END")
            {
                IsEnd = true;
                DoEnd(OutputFile, IsLabelScan);
                EatWhiteSpaces();
                ExecutionAddress = (ushort)LabelTable[GetLabelName()];
                return;
            }

            //Advance to end of line
            while (SourceProgram[CurrentNdx] != '\n')
            {
                CurrentNdx++;
            }
            CurrentNdx++;
        }
        
        #region [Mneumonic Interpreters]
        #region [LOADS]
        /// <summary>
        /// Interprets the LDA Mneumonic and writes the value to OutputFile
        /// Loads a specified 8-bit value into Register A
        /// </summary>
        /// <param name="OutputFile">Output file writer</param>
        /// <param name="IsLabelScan">Adding labels to LabelTable</param>
        private void InterpretLDA(BinaryWriter OutputFile, bool IsLabelScan)
        {
            //Remove whitespaces
            EatWhiteSpaces();
            //Add number to the file
            if (SourceProgram[CurrentNdx] == '#')
            {
                CurrentNdx++;
                byte val = ReadByteValue();
                AsLength += 2;
                //write byte value to the output file
                if (!IsLabelScan)
                {
                    OutputFile.Write((byte)0x01);
                    OutputFile.Write(val);
                }
            }
        }

        /// <summary>
        /// Interprets the LDB Mneumonic and writes the value to OutputFile
        /// Loads a specified 8-bit value into Register B
        /// </summary>
        /// <param name="OutputFile">Output file writer</param>
        /// <param name="IsLabelScan">Adding labels to LabelTable</param>
        private void InterpretLDB(BinaryWriter OutputFile, bool IsLabelScan)
        {
            //Remove whitespaces
            EatWhiteSpaces();
            //Add number to the file
            if (SourceProgram[CurrentNdx] == '#')
            {
                CurrentNdx++;
                byte val = ReadByteValue();
                AsLength += 2;
                //write byte value to the output file
                if (!IsLabelScan)
                {
                    OutputFile.Write((byte)0x22);
                    OutputFile.Write(val);
                }
            }
        }

        /// <summary>
        /// Interprets the LDX Mneumonic and writes the value to OutputFile
        /// Loads a specified 16-bit value into X
        /// </summary>
        /// <param name="OutputFile">Output file writer</param>
        /// <param name="IsLabelScan">Adding labels to LabelTable</param>
        private void InterpretLDX(BinaryWriter OutputFile, bool IsLabelScan)
        {
            //Remove whitespaces
            EatWhiteSpaces();
            //add the word to the file
            if(SourceProgram[CurrentNdx] == '#')
            {
                CurrentNdx++;
                ushort val = ReadWordValue();
                AsLength += 3;
                //write word to the output file
                if (!IsLabelScan)
                {
                    OutputFile.Write((byte)0x02);
                    OutputFile.Write(val);
                }
            }
        }

        /// <summary>
        /// Interprets the LDY Mneumonic and writes the value to OutputFile
        /// Loads a specified 16-bit value into Y
        /// </summary>
        /// <param name="OutputFile">Output file writer</param>
        /// <param name="IsLabelScan">Adding labels to LabelTable</param>
        private void InterpretLDY(BinaryWriter OutputFile, bool IsLabelScan)
        {
            EatWhiteSpaces();
            if (SourceProgram[CurrentNdx] == '#')
            {
                CurrentNdx++;
                ushort val = ReadWordValue();
                AsLength += 3;
                if (!IsLabelScan)
                {
                    OutputFile.Write((byte)(0x23));
                    OutputFile.Write(val);
                }
            }
        }
        #endregion
        #region [STORE]
        /// <summary>
        /// Interprets the STA Mneumonic and writes the value to OutputFile
        /// Stores the value in Register A to memory location of other register
        /// </summary>
        /// <param name="OutputFile">Output file writer</param>
        /// <param name="IsLabelScan">Adding labels to LabelTable</param>
        private void InterpretSTA(BinaryWriter OutputFile, bool IsLabelScan)
        {
            //Remove whitespaces
            EatWhiteSpaces();
            if (SourceProgram[CurrentNdx] == ',')
            {
                Registers r;
                byte opcode = 0x00;

                //Reads the register label
                CurrentNdx++;
                EatWhiteSpaces();
                r = ReadRegister();
                //sets opcode based on register
                switch (r)
                {
                    case Registers.X:
                        opcode = 0x03;
                        break;
                }
                //writes code to output file
                AsLength += 1;
                if (!IsLabelScan)
                {
                    OutputFile.Write(opcode);
                }
            }
        }
        #endregion
        #region [COMPARES]
        /// <summary>
        /// Compare the given value with Register A
        /// </summary>
        /// <param name="OutputFile">Output file writer</param>
        /// <param name="IsLabelScan">Adding labels to LabelTable</param>
        private void InterpretCMPA(BinaryWriter OutputFile, bool IsLabelScan)
        {
            //remove whitespaces
            EatWhiteSpaces();
            if (SourceProgram[CurrentNdx] == '#')
            {
                //read number to compare
                CurrentNdx++;
                byte val = ReadByteValue();
                AsLength += 2;
                //write operation code, value
                if (!IsLabelScan)
                {
                    OutputFile.Write((byte)0x05);
                    OutputFile.Write(val);
                }
            }
        }

        /// <summary>
        /// Compare the given value with Register B
        /// </summary>
        /// <param name="OutputFile">Output file writer</param>
        /// <param name="IsLabelScan">Adding labels to LabelTable</param>
        private void InterpretCMPB(BinaryWriter OutputFile, bool IsLabelScan)
        {
            //remove whitespaces
            EatWhiteSpaces();
            if (SourceProgram[CurrentNdx] == '#')
            {
                //read number to compare
                CurrentNdx++;
                byte val = ReadByteValue();
                AsLength += 2;
                //write operation code, value
                if (!IsLabelScan)
                {
                    OutputFile.Write((byte)0x06);
                    OutputFile.Write(val);
                }
            }
        }

        /// <summary>
        /// Compare the given value with Register D
        /// </summary>
        /// <param name="OutputFile">Output file writer</param>
        /// <param name="IsLabelScan">Adding labels to LabelTable</param>
        private void InterpretCMPD(BinaryWriter OutputFile, bool IsLabelScan)
        {
            //remove whitespaces
            EatWhiteSpaces();
            if (SourceProgram[CurrentNdx] == '#')
            {
                //read word to compare
                CurrentNdx++;
                ushort val = ReadWordValue();
                AsLength += 3;
                //write operation code, value
                if (!IsLabelScan)
                {
                    OutputFile.Write((byte)0x09);
                    OutputFile.Write(val);
                }
            }
        }

        /// <summary>
        /// Compare the given value with Register X
        /// </summary>
        /// <param name="OutputFile">Output file writer</param>
        /// <param name="IsLabelScan">Adding labels to LabelTable</param>
        private void InterpretCMPX(BinaryWriter OutputFile, bool IsLabelScan)
        {
            //remove whitespaces
            EatWhiteSpaces();
            if (SourceProgram[CurrentNdx] == '#')
            {
                //read word to compare
                CurrentNdx++;
                ushort val = ReadWordValue();
                AsLength += 3;
                //write operation code, value
                if (!IsLabelScan)
                {
                    OutputFile.Write((byte)0x07);
                    OutputFile.Write(val);
                }
            }
        }

        /// <summary>
        /// Compare the given value with Register Y
        /// </summary>
        /// <param name="OutputFile">Output file writer</param>
        /// <param name="IsLabelScan">Adding labels to LabelTable</param>
        private void InterpretCMPY(BinaryWriter OutputFile, bool IsLabelScan)
        {
            //remove whitespaces
            EatWhiteSpaces();
            if (SourceProgram[CurrentNdx] == '#')
            {
                //read word to compare
                CurrentNdx++;
                ushort val = ReadWordValue();
                AsLength += 3;
                //write operation code, value
                if (!IsLabelScan)
                {
                    OutputFile.Write((byte)0x08);
                    OutputFile.Write(val);
                }
            }
        }
        #endregion
        #region [JUMPS]
        /// <summary>
        /// Jumps to the specified location
        /// </summary>
        /// <param name="OutputFile">Output file writer</param>
        /// <param name="IsLabelScan">Adding labels to LabelTable</param>
        private void InterpretJMP(BinaryWriter OutputFile, bool IsLabelScan)
        {
            //remove whitespaces
            EatWhiteSpaces();
            if (SourceProgram[CurrentNdx] == '#')
            {
                //read label, memory to jump to
                CurrentNdx++;
                AsLength += 3;
                if (IsLabelScan) return;
                ushort val = ReadWordValue();

                //write operation code, value
                if (!IsLabelScan)
                {
                    OutputFile.Write((byte)0x0A);
                    OutputFile.Write(val);
                }
            }
        }

        /// <summary>
        /// Jumps on Equal to the specified location
        /// </summary>
        /// <param name="OutputFile">Ouput file writer</param>
        /// <param name="IsLabelScan">Adding labels to LabelTable</param>
        private void InterpretJEQ(BinaryWriter OutputFile, bool IsLabelScan)
        {
            //remove whitespaces
            EatWhiteSpaces();
            if (SourceProgram[CurrentNdx] == '#')
            {
                //read label, memory to jump to
                CurrentNdx++;
                AsLength += 3;
                if (IsLabelScan) return;
                ushort val = ReadWordValue();

                //write operation code, value
                if (!IsLabelScan)
                {
                    OutputFile.Write((byte)0x0B);
                    OutputFile.Write(val);
                }
            }
        }

        /// <summary>
        /// Jumps on Not Equal to the specified location
        /// </summary>
        /// <param name="OutputFile">Ouput file writer</param>
        /// <param name="IsLabelScan">Adding labels to LabelTable</param>
        private void InterpretJNE(BinaryWriter OutputFile, bool IsLabelScan)
        {
            //remove whitespaces
            EatWhiteSpaces();
            if (SourceProgram[CurrentNdx] == '#')
            {
                //read label, memory to jump to
                CurrentNdx++;
                AsLength += 3;
                if (IsLabelScan) return;
                ushort val = ReadWordValue();

                //write operation code, value
                if (!IsLabelScan)
                {
                    OutputFile.Write((byte)0x0C);
                    OutputFile.Write(val);
                }
            }
        }

        /// <summary>
        /// Jumps on Greater Than to the specified location
        /// </summary>
        /// <param name="OutputFile">Ouput file writer</param>
        /// <param name="IsLabelScan">Adding labels to LabelTable</param>
        private void InterpretJGT(BinaryWriter OutputFile, bool IsLabelScan)
        {
            //remove whitespaces
            EatWhiteSpaces();
            if (SourceProgram[CurrentNdx] == '#')
            {
                //read label, memory to jump to
                CurrentNdx++;
                AsLength += 3;
                if (IsLabelScan) return;
                ushort val = ReadWordValue();

                //write operation code, value
                if (!IsLabelScan)
                {
                    OutputFile.Write((byte)0x0D);
                    OutputFile.Write(val);
                }
            }
        }

        /// <summary>
        /// Jumps on Less Than to the specified location
        /// </summary>
        /// <param name="OutputFile">Ouput file writer</param>
        /// <param name="IsLabelScan">Adding labels to LabelTable</param>
        private void InterpretJLT(BinaryWriter OutputFile, bool IsLabelScan)
        {
            //remove whitespaces
            EatWhiteSpaces();
            if (SourceProgram[CurrentNdx] == '#')
            {
                //read label, memory to jump to
                CurrentNdx++;
                AsLength += 3;
                if (IsLabelScan) return;
                ushort val = ReadWordValue();

                //write operation code, value
                if (!IsLabelScan)
                {
                    OutputFile.Write((byte)0x0E);
                    OutputFile.Write(val);
                }
            }
        }
        #endregion
        #region [INCREMENTS]
        /// <summary>
        /// Interprets the INCA Mneumonic
        /// Increments the value in register A
        /// </summary>
        /// <param name="OutputFile"></param>
        /// <param name="IsLabelScan"></param>
        private void InterpretINCA(BinaryWriter OutputFile, bool IsLabelScan)
        {
            if (!IsLabelScan)
            {
                OutputFile.Write((byte)0x0F);
            }
            AsLength++;
        }

        /// <summary>
        /// Interprets the INCB Mneumonic
        /// Increments the value in register B
        /// </summary>
        /// <param name="OutputFile"></param>
        /// <param name="IsLabelScan"></param>
        private void InterpretINCB(BinaryWriter OutputFile, bool IsLabelScan)
        {
            if (!IsLabelScan)
            {
                OutputFile.Write((byte)0x10);
            }
            AsLength++;
        }

        /// <summary>
        /// Interprets the INCX Mneumonic
        /// Increments the value in register X
        /// </summary>
        /// <param name="OutputFile"></param>
        /// <param name="IsLabelScan"></param>
        private void InterpretINCX(BinaryWriter OutputFile, bool IsLabelScan)
        {
            if (!IsLabelScan)
            {
                OutputFile.Write((byte)0x11);
            }
            AsLength++;
        }

        /// <summary>
        /// Interprets the INCY Mneumonic
        /// Increments the value in register Y
        /// </summary>
        /// <param name="OutputFile"></param>
        /// <param name="IsLabelScan"></param>
        private void InterpretINCY(BinaryWriter OutputFile, bool IsLabelScan)
        {
            if (!IsLabelScan)
            {
                OutputFile.Write((byte)0x12);
            }
            AsLength++;
        }

        /// <summary>
        /// Interprets the INCD Mneumonic
        /// Increments the value in register D
        /// </summary>
        /// <param name="OutputFile"></param>
        /// <param name="IsLabelScan"></param>
        private void InterpretINCD(BinaryWriter OutputFile, bool IsLabelScan)
        {
            if (!IsLabelScan)
            {
                OutputFile.Write((byte)0x13);
            }
            AsLength++;
        }
        #endregion
        #region [DECREMENTS]
        /// <summary>
        /// Interprets the DECA Mneumonic
        /// Decrements the value in Register A
        /// </summary>
        /// <param name="OutoutFile"></param>
        /// <param name="IsLabelScan"></param>
        private void InterpretDECA(BinaryWriter OutputFile, bool IsLabelScan)
        {
            if (!IsLabelScan)
            {
                OutputFile.Write((byte)0x14);
            }
            AsLength++;
        }

        /// <summary>
        /// Interprets the DECB Mneumonic
        /// Decrements the value in Register B
        /// </summary>
        /// <param name="OutoutFile"></param>
        /// <param name="IsLabelScan"></param>
        private void InterpretDECB(BinaryWriter OutputFile, bool IsLabelScan)
        {
            if (!IsLabelScan)
            {
                OutputFile.Write((byte)0x15);
            }
            AsLength++;
        }

        /// <summary>
        /// Interprets the DECX Mneumonic
        /// Decrements the value in Register X
        /// </summary>
        /// <param name="OutoutFile"></param>
        /// <param name="IsLabelScan"></param>
        private void InterpretDECX(BinaryWriter OutputFile, bool IsLabelScan)
        {
            if (!IsLabelScan)
            {
                OutputFile.Write((byte)0x16);
            }
            AsLength++;
        }

        /// <summary>
        /// Interprets the DECY Mneumonic
        /// Decrements the value in Register Y
        /// </summary>
        /// <param name="OutoutFile"></param>
        /// <param name="IsLabelScan"></param>
        private void InterpretDECY(BinaryWriter OutputFile, bool IsLabelScan)
        {
            if (!IsLabelScan)
            {
                OutputFile.Write((byte)0x17);
            }
            AsLength++;
        }

        /// <summary>
        /// Interprets the DECD Mneumonic
        /// Decrements the value in Register D
        /// </summary>
        /// <param name="OutoutFile"></param>
        /// <param name="IsLabelScan"></param>
        private void InterpretDECD(BinaryWriter OutputFile, bool IsLabelScan)
        {
            if (!IsLabelScan)
            {
                OutputFile.Write((byte)0x18);
            }
            AsLength++;
        }
        #endregion
        #region [ROTATES]
        /// <summary>
        /// Rotates the value in Register A left
        /// </summary>
        /// <param name="OutputFile"></param>
        /// <param name="IsLabelScan"></param>
        private void InterpretROLA(BinaryWriter OutputFile, bool IsLabelScan)
        {
            if (!IsLabelScan)
            {
                OutputFile.Write((byte)0x19);
            }
            AsLength++;
        }

        /// <summary>
        /// Rotates the value in Register B left
        /// </summary>
        /// <param name="OutputFile"></param>
        /// <param name="IsLabelScan"></param>
        private void InterpretROLB(BinaryWriter OutputFile, bool IsLabelScan)
        {
            if (!IsLabelScan)
            {
                OutputFile.Write((byte)0x1A);
            }
            AsLength++;
        }

        /// <summary>
        /// Rotates the value in Register A right
        /// </summary>
        /// <param name="OutputFile"></param>
        /// <param name="IsLabelScan"></param>
        private void InterpretRORA(BinaryWriter OutputFile, bool IsLabelScan)
        {
            if (!IsLabelScan)
            {
                OutputFile.Write((byte)0x1B);
            }
            AsLength++;
        }

        /// <summary>
        /// Rotates the value in Register B right
        /// </summary>
        /// <param name="OutputFile"></param>
        /// <param name="IsLabelScan"></param>
        private void InterpretRORB(BinaryWriter OutputFile, bool IsLabelScan)
        {
            if (!IsLabelScan)
            {
                OutputFile.Write((byte)0x1C);
            }
            AsLength++;
        }
        #endregion
        #region [ADDS]
        /// <summary>
        /// Adds 1 to the value of the A Register
        /// if the carry flag is set
        /// </summary>
        /// <param name="OutputFile"></param>
        /// <param name="IsLabelScan"></param>
        private void InterpretADCA(BinaryWriter OutputFile, bool IsLabelScan)
        {
            if (!IsLabelScan)
            {
                OutputFile.Write((byte)0x1D);
            }
            AsLength++;
        }

        /// <summary>
        /// Adds 1 to the value of the B Register
        /// if the carry flag is set
        /// </summary>
        /// <param name="OutputFile"></param>
        /// <param name="IsLabelScan"></param>
        private void InterpretADCB(BinaryWriter OutputFile, bool IsLabelScan)
        {
            if (!IsLabelScan)
            {
                OutputFile.Write((byte)0x1E);
            }
            AsLength++;
        }

        /// <summary>
        /// Adds to the value of the A Register
        /// </summary>
        /// <param name="OutputFile"></param>
        /// <param name="IsLabelScan"></param>
        private void InterpretADDA(BinaryWriter OutputFile, bool IsLabelScan)
        {
            EatWhiteSpaces();
            if (SourceProgram[CurrentNdx] == '#')
            {
                CurrentNdx++;
                AsLength += 2;
                if (!IsLabelScan) return;
                ushort val = ReadByteValue();

                if(!IsLabelScan)
                {
                    OutputFile.Write((byte)0x1F);
                    OutputFile.Write(val);
                }
            }
        }

        /// <summary>
        /// Adds to the value of the B Register
        /// </summary>
        /// <param name="OutputFile"></param>
        /// <param name="IsLabelScan"></param>
        private void InterpretADDB(BinaryWriter OutputFile, bool IsLabelScan)
        {
            EatWhiteSpaces();
            if (SourceProgram[CurrentNdx] == '#')
            {
                CurrentNdx++;
                AsLength += 2;
                if (!IsLabelScan) return;
                ushort val = ReadByteValue();

                if (!IsLabelScan)
                {
                    OutputFile.Write((byte)0x20);
                    OutputFile.Write(val);
                }
            }
        }

        /// <summary>
        /// Adds to the value of the A and B Registers
        /// Stores the result in the D Register
        /// </summary>
        /// <param name="OutputFile"></param>
        /// <param name="IsLabelScan"></param>
        private void InterpretADDAB(BinaryWriter OutputFile, bool IsLabelScan)
        {
            if (!IsLabelScan)
            {
                OutputFile.Write((byte)0x21);
            }
            AsLength++;
        }
        #endregion
        #region [END]
        /// <summary>
        /// Writes the END code to the output file
        /// </summary>
        /// <param name="OutputFile">Output file writer</param>
        /// <param name="IsLabelScan">Adding labels to LabelTable</param>
        private void DoEnd(BinaryWriter OutputFile, bool IsLabelScan)
        {
            AsLength++;
            //write code to output file
            if (!IsLabelScan)
            {
                OutputFile.Write((byte)0x04);
            }
        }
        #endregion
        #endregion

        /// <summary>
        /// Determines the register specified at the current index
        /// </summary>
        /// <returns>The register specified at the current index</returns>
        private Registers ReadRegister()
        {
            Registers r = Registers.Unknown;
            char rchar = SourceProgram[CurrentNdx];
            if (rchar == 'X' || rchar == 'x') r = Registers.X;
            if (rchar == 'Y' || rchar == 'y') r = Registers.Y;
            if (rchar == 'D' || rchar == 'd') r = Registers.D;
            if (rchar == 'A' || rchar == 'a') r = Registers.A;
            if (rchar == 'B' || rchar == 'b') r = Registers.B;

            CurrentNdx++;
            return r;
        }

        /// <summary>
        /// Reads the word at the current index
        /// </summary>
        /// <returns>The word at the current index</returns>
        private ushort ReadWordValue()
        {
            ushort val = 0;
            bool IsHex = false;
            string sval = string.Empty;

            //Identify if the number is Hex
            if (SourceProgram[CurrentNdx] == '$')
            {
                CurrentNdx++;
                IsHex = true;
            }

            //Identify is it is a label, read location of label
            if (!IsHex && char.IsLetter(SourceProgram[CurrentNdx]) && !char.IsWhiteSpace(SourceProgram[CurrentNdx]))
            {
                val = (ushort)LabelTable[GetLabelName()];
                return val;
            }

            //Read word
            while (char.IsLetterOrDigit(SourceProgram[CurrentNdx]))
            {
                sval += SourceProgram[CurrentNdx];
                CurrentNdx++;
            }
            //convert Hex
            if (IsHex)
            {
                val = Convert.ToUInt16(sval, 16);
            }
            //parse non-hex values
            else
            {
                val = ushort.Parse(sval);
            }

            return val;
        }

        /// <summary>
        /// Reads the byte at the current index
        /// </summary>
        /// <returns>The byte at the current index</returns>
        private byte ReadByteValue()
        {
            byte val = 0;
            bool IsHex = false;
            string sval = string.Empty;

            //determine if the value is Hex
            if (SourceProgram[CurrentNdx] == '$')
            {
                CurrentNdx++;
                IsHex = true;
            }
            //Read value
            while (char.IsLetterOrDigit(SourceProgram[CurrentNdx]))
            {
                sval = sval + SourceProgram[CurrentNdx];
                CurrentNdx++;
            }
            //Convert hex values
            if (IsHex)
            {
                val = Convert.ToByte(sval, 16);
            }
            //parse byte value
            else
            {
                val = byte.Parse(sval);
            }

            return val;
        }

        /// <summary>
        /// Skips over whitespaces
        /// </summary>
        private void EatWhiteSpaces()
        {
            while (char.IsWhiteSpace(SourceProgram[CurrentNdx]))
            {
                CurrentNdx++;
            }
        }

        /// <summary>
        /// Returns the name of the label and converts it to uppercase
        /// </summary>
        /// <returns>Uppercase name of the label</returns>
        private string GetLabelName()
        {
            string lblname = string.Empty;

            while (char.IsLetterOrDigit(SourceProgram[CurrentNdx]))
            {
                if (SourceProgram[CurrentNdx] == ':')
                {
                    CurrentNdx++;
                    break;
                }

                lblname += SourceProgram[CurrentNdx];
                CurrentNdx++;

                if (CurrentNdx >= SourceProgram.Length)
                {
                    break;
                }
            }

            return lblname.ToUpper();
        }
    }
}
