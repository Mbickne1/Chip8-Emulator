using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8Emulator.Memory
{
    public class RandomAccessMemory
    {
        byte[] MemoryRegisters;
        private ushort IndexRegister;
        private ushort ProgramCounter;

        public RandomAccessMemory(byte[] fonts)
        {
            //Memory is 4K, should length by 4096 or 512?
            MemoryRegisters = new byte[4096];
            IndexRegister = 0;
            //Program Counter starts at 0x200
            ProgramCounter = 0x200;

            LoadFonts(fonts);
        }

        public ushort GetIndexRegister()
        {
            return IndexRegister;
        }

        public void SetIndexRegister(ushort value)
        {
            IndexRegister = value;
        }

        public void IncrementProgramCounter()
        {
            ProgramCounter += 2;
        }

        public byte GetMemoryFromLocation(int location)
        {
            return MemoryRegisters[location];
        }

        public void JumpTo(ushort location)
        {
            ProgramCounter += location;
        }

        public void LoadProgramIntoMemory(byte[] program)
        {
            for (var i = 0; i < program.Length; ++i)
            {
                //Load into memory after 0x200 which is 512
                MemoryRegisters[i + 512] = program[i];
            }
        }

        public ushort FetchOpcode()
        {
            return (ushort)((MemoryRegisters[ProgramCounter] << 8 | MemoryRegisters[ProgramCounter + 1]));
        }

        public void LoadFonts(byte[] fonts)
        {
            for(int i = 0; i < 80; i++)
            {
                MemoryRegisters[i] = fonts[i];
            }
        }
    }
}
