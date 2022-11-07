using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Chip8Emulator
{
    public class RandomAccessMemory
    {
        private byte[] MemoryRegisters;
        private Stack<ushort> MemoryRegistersStack;

        private ushort IndexRegister;
        private ushort ProgramCounter;
        
        public RandomAccessMemory(byte[] fonts)
        {
            //Memory is 4K, should length by 4096 or 512?
            MemoryRegisters = new byte[4096];
            MemoryRegistersStack = new Stack<ushort>();
            
            IndexRegister = 0;

            //Program Counter starts at 0x200
            ProgramCounter = 0x200;

            LoadFonts(fonts);
        }

        public ushort GetIndexRegister()
        {
            return IndexRegister;
        }
        
        public ushort GetProgramCounter()
        {
            return ProgramCounter;
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

        public void StoreInMemory(int location, byte value)
        {
            MemoryRegisters[location] = value;
        }


        /// <summary>
        /// Set the ProgramCounter to a specific memory locations
        /// </summary>
        /// <param name="location"></param>
        public void JumpTo(ushort location)
        {
            ProgramCounter = location;
            Debug.WriteLine("Program Counter: " + ProgramCounter);
            //Debug.WriteLine(MemoryRegisters[location]);
        }

        public void LoadProgramIntoMemory(byte[] program)
        {
            for (var i = 0; i < program.Length; i++)
            {
                //Load into memory after 0x200 which is 512
                MemoryRegisters[i + 512] = program[i];
            }
        }

        public ushort FetchOpcode()
        {
            return (ushort)(MemoryRegisters[ProgramCounter] << 8 | MemoryRegisters[ProgramCounter + 1]);
        }

        public void LoadFonts(byte[] fonts)
        {
            for(int i = 0; i < 80; i++)
            {
                MemoryRegisters[i] = fonts[i];
            }
        }

        public void Push(ushort item)
        {
            MemoryRegistersStack.Push(item);
        }

        public ushort Pop()
        {
            return MemoryRegistersStack.Pop();
        }
    }
}
