using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8Emulator
{
    public class CPU
    {
        //General Purpose Registers 
        private byte[] Registers;

        public CPU()
        {
            Registers = new byte[16];
        }

        public byte GetRegisterValue(int index)
        {
            Debug.WriteLine("Index: " + index);
            return Registers[index];
        }

        public void SetRegisterValue(int index, byte value)
        {
            Registers[index] = value;
        }

        public void AddToRegisterValue(int index, byte value)
        {
            Registers[index] += value;
        }
        //The Last Register is used as a Flag Register
        public byte GetFlagRegister()
        {
            
            return Registers[15];
        }

        public void SetFlagReigster(byte flag)
        {
            Registers[15] = flag;
        }
    }
}
