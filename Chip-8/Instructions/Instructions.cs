using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Chip8Emulator.OpCodes
{
    public class Instructions
    {
        private readonly Dictionary<byte, Action<Opcode>> _instructions;
        private readonly Dictionary<byte, Action<Opcode>> FCodeInstructions;

        private EmulatorController _controller;
        private readonly CPU _cpu;
        private readonly RandomAccessMemory _randomAccessMemory;
        private readonly Screen _screen;
        private readonly Keyboard _keyboard;

        public Instructions(
            EmulatorController controller, 
            CPU cpu, 
            RandomAccessMemory randomAcessMemory,
            Screen screen,
            Keyboard keyboard
        )
        {
            _cpu = cpu;
            _randomAccessMemory = randomAcessMemory;
            _screen = screen;
            _keyboard = keyboard;
            _instructions = new Dictionary<byte, Action<Opcode>>
            {
                {0x0, ClearScreen},
                {0x1, Jump},
                {0x2, CallSubroutine},
                {0x3, SkipWhenPCEqual},
                {0x4, SkipWhenPCNotEqual},
                {0x5, SkipWhenEqual},
                {0x6, SetRegister},
                {0x7, AddValue},
                {0x8, ArithmeticInstructions},
                {0x9, SkipWhenNotEqual},
                {0xA, SetIndexRegister},
                {0xB, JumpOffset},
                {0xC, Random},
                {0xD, Draw},
                {0xE, SkipIfKey},
                {0xF, FCodes}
            };

            FCodeInstructions = new Dictionary<byte, Action<Opcode>>
            {
                {0x07, GetDelayValue},
                {0x15, SetDelayValue},
                {0x18, SetSoundValue},
                {0x1E, AddToIndex},
                {0x0A, GetKey},
                {0x29, GetFont},
                {0x33, BinaryToDecimals},
                {0x55, StoreMemory},
                {0x65, LoadMemory},
            };

            _controller = controller;
        }

        public Dictionary<byte, Action<Opcode>> GetInstructions()
        {
            return _instructions;
        }

        //0x00E0 -- 0x00EE
        private void ClearScreen(Opcode _opcode) 
        {
            ushort NN = (ushort)(_opcode.GetOpcode() & 0x00FF);
            //The Clear Screen Opcode
            if(NN == 0xE0)
            {
                _screen.Clear();
                _randomAccessMemory.IncrementProgramCounter();
            } 
            //The Subroutine Return Opcode
            else if(NN == 0xEE)
            {
                ushort address = _randomAccessMemory.Pop();
                _randomAccessMemory.JumpTo(address);
            }           
        }

        //0x1NNN
        private void Jump(Opcode _opcode) 
        {
            ushort NNN = (ushort)(_opcode.GetOpcode() & 0x0FFF);
            _randomAccessMemory.JumpTo(NNN);
            //_randomAccessMemory.IncrementProgramCounter();
        }

        //0x2NNN
        private void CallSubroutine(Opcode _opcode)
        {
            _randomAccessMemory.Push(_randomAccessMemory.GetProgramCounter());
            Jump(_opcode);
     
        }

        //0x3XNN
        private void SkipWhenPCEqual(Opcode _opcode)
        {
            byte X = (byte)(_opcode.GetOpcode() & 0x0F00);
            byte NN = (byte)(_opcode.GetOpcode() & 0x00FF);

            byte VX = _cpu.GetRegisterValue(X);
            if(VX == NN)
            {
                _randomAccessMemory.IncrementProgramCounter();
            }

            //Do I need to do this right
            _randomAccessMemory.IncrementProgramCounter();
        }

        //0x4XNN
        private void SkipWhenPCNotEqual(Opcode _opcode)
        {
            byte X = (byte)(_opcode.GetOpcode() & 0x0F00);
            byte NN = (byte)(_opcode.GetOpcode() & 0x00FF);

            byte VX = _cpu.GetRegisterValue(X);
            if (VX != NN)
            {
                _randomAccessMemory.IncrementProgramCounter();
            }

            //Do I need to do this right
            _randomAccessMemory.IncrementProgramCounter();
        }

        //0x5XY0
        private void SkipWhenEqual(Opcode _opcode)
        {
            byte X = (byte)(_opcode.GetOpcode() & 0x0F00);
            byte Y = (byte)((_opcode.GetOpcode() & 0x00F0) >> 4); // << 16
            Debug.WriteLine("Y: " + Y);
            byte VX = _cpu.GetRegisterValue(X);
            byte VY = _cpu.GetRegisterValue(Y);

            if(VX == VY)
            {
                _randomAccessMemory.IncrementProgramCounter();
            }

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

        //0x8XY0
        private void ArithmeticInstructions(Opcode _opcode)
        {
            byte X = (byte)(_opcode.GetOpcode() & 0x0F00);
            byte Y = (byte)(_opcode.GetOpcode() & 0x00F0);
            byte N = (byte)(_opcode.GetOpcode() & 0x000F);

            byte VY = _cpu.GetRegisterValue(Y);
            byte VX = _cpu.GetRegisterValue(X);
            byte flag;
            switch (N) {
                case 0: //Set
                    _cpu.SetRegisterValue(X, VY);
                    break;
                case 1: //Bitwise OR
                    _cpu.SetRegisterValue(X, (byte)(VX | VY));
                    break;
                case 2: //Bitwise AND
                    _cpu.SetRegisterValue(X, (byte)(VX & VY));
                    break;
                case 3: //Bitwise XOR
                    _cpu.SetRegisterValue(X, (byte)(VX ^ VY));
                    break;
                case 4: //Add
                    byte sum = (byte)(VX + VY);
                    flag = (byte)(sum > 0xFF ? 1 : 0);
                    _cpu.SetFlagReigster(flag);
                    _cpu.SetRegisterValue(X, (byte)(VX + VY));
                    break;
                case 5: //Subtract -- Case 1
                    flag = (byte)(VX > VY ? 1 : 0);
                    _cpu.SetFlagReigster(flag);
                    _cpu.SetRegisterValue(X, (byte)(VX - VY));
                    break;
                case 6: //Shift 1 bit to the right
                    flag = (byte)((VX & 0x1) == 1 ? 1 : 0);
                    _cpu.SetRegisterValue(X, (byte)(VY >> 1));
                    _cpu.SetFlagReigster(flag); 
                    break;
                case 7: //Subtract -- Case 2
                    flag = (byte)(VY > VX ? 1 : 0);
                    _cpu.SetFlagReigster(flag);
                    _cpu.SetRegisterValue(X, (byte)(VY - VX));
                    break;
                default: //Shift 1 bit to the left
                    flag = (byte)((VX >> 7 & 0x1) == 1 ? 1 : 0);
                    _cpu.SetRegisterValue(X, (byte)(VY << 1));
                    _cpu.SetFlagReigster(flag);
                    break;
            }

            _randomAccessMemory.IncrementProgramCounter();
        }

        //0x9XY0
        private void SkipWhenNotEqual(Opcode _opcode)
        {
            byte X = (byte)(_opcode.GetOpcode() & 0x0F00);
            byte Y = (byte)((_opcode.GetOpcode() & 0x00F0) >> 4);

            byte VX = _cpu.GetRegisterValue(X);
            byte VY = _cpu.GetRegisterValue(Y);

            if (VX != VY)
            {
                _randomAccessMemory.IncrementProgramCounter();
            }

            _randomAccessMemory.IncrementProgramCounter();
        }

        //0xANNN
        private void SetIndexRegister(Opcode _opcode)
        {
            ushort NNN = (ushort)(_opcode.GetOpcode() & 0x0FFF);
            _randomAccessMemory.SetIndexRegister(NNN);
            _randomAccessMemory.IncrementProgramCounter();
        }

        //0xBNNN
        private void JumpOffset(Opcode _opcode)
        {
            ushort NNN = (ushort)(_opcode.GetOpcode() & 0x0FFF);
            ushort value = _cpu.GetRegisterValue(0);

            _randomAccessMemory.JumpTo((ushort)(NNN + value));
            _randomAccessMemory.IncrementProgramCounter();
        }

        //0xCXNN
        private void Random(Opcode _opcode)
        {
            byte X = (byte)(_opcode.GetOpcode() & 0x0F00);
            byte NN = (byte)(_opcode.GetOpcode() & 0x00FF);
            
            //Do i generate between 0 & NN?
            Random _random = new Random();
            byte randomNum = (byte)(_random.Next(256) & NN);
            _cpu.SetRegisterValue(X, randomNum);

            _randomAccessMemory.IncrementProgramCounter();
        }

        //0xDXYN
        private void Draw(Opcode _opcode)
        {
            byte X = (byte)((_opcode.GetOpcode() & 0x0F00) >> 8);
            byte Y = (byte)((_opcode.GetOpcode() & 0x00F0) >> 4);
            byte N = (byte)(_opcode.GetOpcode() & 0x000F);
            
            //Do I need to increase these to the actual WIDTH/HEIGHT
            byte x = (byte)(_cpu.GetRegisterValue(X));
            byte y = (byte)(_cpu.GetRegisterValue(Y));

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

        //0xEX9E && 0xEXA1 -- COME BACK WHEN KEY PRESSING IS IMPLEMENTED
        private void SkipIfKey(Opcode _opcode)
        {
            byte X = (byte)(_opcode.GetOpcode() & 0x0F00);
            byte NN = (byte)(_opcode.GetOpcode() & 0x00FF);

            if(NN == 0x9E)
            {
                if(_keyboard.isPressed(_cpu.GetRegisterValue(X)))
                {
                    _randomAccessMemory.IncrementProgramCounter();
                }
            } 
            else if(NN == 0xA1)
            {
                if (!_keyboard.isPressed(_cpu.GetRegisterValue(X)))
                {
                    _randomAccessMemory.IncrementProgramCounter();
                }
            }
            _randomAccessMemory.IncrementProgramCounter();
        }

        //0xF
        private void FCodes(Opcode _opcode)
        {
            byte NN = (byte)(_opcode.GetOpcode() & 0x00FF);
            if(FCodeInstructions.ContainsKey(NN))
            {
                FCodeInstructions[NN](_opcode);
            }
        }

        //0xFX07
        private void GetDelayValue(Opcode _opcode)
        {
            byte X = (byte)(_opcode.GetOpcode() & 0x0F00);

            byte timer = _controller.GetDelayTimer();
            _cpu.SetRegisterValue(X, timer);
            _randomAccessMemory.IncrementProgramCounter();
        }

        //0xFX15
        private void SetDelayValue(Opcode _opcode)
        {
            byte X = (byte)(_opcode.GetOpcode() & 0x0F00);
            byte VX = _cpu.GetRegisterValue(X);

            _controller.SetDelayTimer(VX);
            _randomAccessMemory.IncrementProgramCounter();
        }

        //0xFX18
        private void SetSoundValue(Opcode _opcode)
        {
            Debug.WriteLine("Sound not yet implemented");
            _randomAccessMemory.IncrementProgramCounter();
        }

        //0xFX1E -- OVERFLOW MIGHT NOT BE HANDLED PROPERLY LOOK INTO THIS
        private void AddToIndex(Opcode _opcode)
        {
            byte X = (byte)(_opcode.GetOpcode() & 0x0F00);
            byte VX = _cpu.GetRegisterValue(X);

            ushort registerValue = _randomAccessMemory.GetIndexRegister();
            ushort sum = (ushort)(registerValue + VX);

            byte flag = (byte)(sum > 0x0FFF ? 1 : 0);

            _cpu.SetFlagReigster(flag);
            _randomAccessMemory.SetIndexRegister(sum);
            _randomAccessMemory.IncrementProgramCounter();
        }

        //0xFX0A
        private void GetKey(Opcode _opcode)
        {
            byte X = (byte)(_opcode.GetOpcode() & 0x0F00);

            for(byte i = 0; i <= 0xF; i++)
            {
                if (_keyboard.isPressed(i))
                {
                    _cpu.SetRegisterValue(X, i);
                    _randomAccessMemory.IncrementProgramCounter();
                }
            }
        }

        //0xFX29
        private void GetFont(Opcode _opcode)
        {
            byte X = (byte)(_opcode.GetOpcode() & 0x0F00);
            byte VX = _cpu.GetRegisterValue(X);

            _randomAccessMemory.SetIndexRegister((ushort)(VX * 5));
            _controller.SetDrawFlag(true);
            _randomAccessMemory.IncrementProgramCounter();
        }

        //0xFX33
        private void BinaryToDecimals(Opcode _opcode)
        {
            byte X = (byte)(_opcode.GetOpcode() & 0x0F00);
            byte VX = _cpu.GetRegisterValue(X);

            ushort indexRegister = _randomAccessMemory.GetIndexRegister();
            //Set memory at Index Locations I, I + 1, I + 2
            _randomAccessMemory.StoreInMemory(indexRegister, (byte)(VX / 100));
            _randomAccessMemory.StoreInMemory(indexRegister + 1, (byte)((VX % 100) / 10));
            _randomAccessMemory.StoreInMemory(indexRegister + 2, (byte)((VX % 100) % 10));

            _randomAccessMemory.IncrementProgramCounter();
        }

        //0xFX55 && 0xFX65
        private void StoreMemory(Opcode _opcode)
        {
            byte X = (byte)(_opcode.GetOpcode() & 0x0F00);
            ushort indexRegister = _randomAccessMemory.GetIndexRegister();
            for(int i = 0; i <= X; i++)
            {
                _randomAccessMemory.StoreInMemory(indexRegister, _cpu.GetRegisterValue(i));
                indexRegister++;
            }
            _randomAccessMemory.IncrementProgramCounter();
        }

        private void LoadMemory(Opcode _opcode)
        {
            byte X = (byte)(_opcode.GetOpcode() & 0x0F00);
            ushort indexRegister = _randomAccessMemory.GetIndexRegister();
            for (int i = 0; i <= X; i++)
            {
                var value = _randomAccessMemory.GetMemoryFromLocation(indexRegister);
                _cpu.SetRegisterValue(i, value);
                indexRegister++;
            }
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
