using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Chip8Emulator.Memory;

namespace Chip8Emulator.OpCodes
{
    public class Instructions
    {
        private readonly Dictionary<byte, Action<Opcode>> _instructions;
        private EmulatorController _controller;
        readonly CPU _cpu;
        readonly RandomAccessMemory _randomAccessMemory;
        readonly Screen _screen;
        public Instructions(
            EmulatorController controller, 
            CPU cpu, 
            RandomAccessMemory randomAcessMemory,
            Screen screen
        )
        {
            _cpu = cpu;
            _randomAccessMemory = randomAcessMemory;
            _screen = screen;

            _instructions = new Dictionary<byte, Action<Opcode>>
            {
                {0x0, ClearScreen},
                {0x1, Jump},
                {0x6, SetRegister},
                {0x7, AddValue},
                {0xA, SetIndexRegister},
                {0xD, Draw}
            };
            _controller = controller;
        }

        public Dictionary<byte, Action<Opcode>> GetInstructions()
        {
            return _instructions;
        }
        //0x00E0
        private void ClearScreen(Opcode _opcode) 
        {
            _screen.Clear();
            _randomAccessMemory.IncrementProgramCounter();
        }

        //0x1NNN
        private void Jump(Opcode _opcode) 
        {
            ushort NNN = (ushort)(_opcode.GetOpcode() & 0x0FFF);
            _randomAccessMemory.JumpTo(NNN);
            _randomAccessMemory.IncrementProgramCounter();
        }

        //0x6XNN
        private void SetRegister(Opcode _opcode)
        {
            byte X = (byte)(_opcode.GetOpcode() & 0x0F00);
            byte NN = (byte)(_opcode.GetOpcode() & 0x00FF);
            _cpu.SetRegisterValue(X, NN);
            _randomAccessMemory.IncrementProgramCounter();
        }

        //0x7XNN
        private void AddValue(Opcode _opcode)
        {
            byte X = (byte)(_opcode.GetOpcode() & 0x0F00);
            byte NN = (byte)(_opcode.GetOpcode() & 0x00FF);
            _cpu.AddToRegisterValue(X, NN);
            _randomAccessMemory.IncrementProgramCounter();
        }

        //0xANNN
        private void SetIndexRegister(Opcode _opcode)
        {
            ushort NNN = (ushort)(_opcode.GetOpcode() & 0x0FFF);
            _randomAccessMemory.SetIndexRegister(NNN);
            _randomAccessMemory.IncrementProgramCounter();
        }

        //0xDXYN
        private void Draw(Opcode _opcode)
        {
            byte X = (byte)((_opcode.GetOpcode() & 0x0F00) >> 8);
            byte Y = (byte)((_opcode.GetOpcode() & 0x00F0) >> 4);
            byte N = (byte)(_opcode.GetOpcode() & 0x000F);
            
            //Do I need to increase these to the actual WIDTH/HEIGHT
            byte x = (byte)(_cpu.GetReigsterValue(X));
            byte y = (byte)(_cpu.GetReigsterValue(Y));

            _cpu.SetFlagReigster(0);

            ushort indexRegister = _randomAccessMemory.GetIndexRegister();
            for(int height = 0; height < N; height++)
            {
                byte pixel = _randomAccessMemory.GetMemoryFromLocation(indexRegister + height);
                
                for(int bit = 0; bit < 8; bit++)
                {
                    if((pixel & 0x80) != 0)
                    {
                        var screenY = (y + height);
                        var screenX = (x + bit);

                        if (_controller.displayBuffer[screenX, screenY] == 1)
                        {
                            _cpu.SetFlagReigster(1);
                        }
                        _controller.displayBuffer[screenX, screenY] ^= 1;
                    }
                    pixel <<= 1;
                }
            }

            _controller.SetDrawFlag(true);
            _randomAccessMemory.IncrementProgramCounter();
        }
    }

    public class Opcode
    {
        private readonly ushort _opcode;

        public Opcode(ushort _opcode)
        {
            this._opcode = _opcode;
        }

        public ushort GetOpcode()
        {
            return this._opcode;
        }
    }
}
