using Chip8Emulator.OpCodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;

namespace Chip8Emulator
{
    public class EmulatorController
    {
        //Convert to using displayBuffer as private
        public byte[,] displayBuffer;
        private bool drawFlag;

        private byte DelayTimer;

        // For timing..
        readonly Stopwatch stopWatch = Stopwatch.StartNew();
        readonly TimeSpan targetElapsedTime60Hz = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);
        readonly TimeSpan targetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 1000);
        TimeSpan lastTime;

        //Differnt Componenets of the Chip8
        private readonly Instructions _instructions;
        private readonly CPU _cpu;
        private readonly RandomAccessMemory _randomAccessMemory;
        private readonly GUI _screen;
        private readonly Keyboard _keyboard;
        //This will need to be changed, also made variable for different roms
        string program = "C:\\Users\\Matthew.bicknell\\DEVELOPMENT\\Chip8Emulator\\Chip-8\\ROMS\\IBM.ch8";
        public EmulatorController(GUI screen, Keyboard keyboard)
        {
            _screen = screen;
            _keyboard = keyboard;
            _randomAccessMemory = new RandomAccessMemory(fontSet);
            _cpu = new CPU();
            _instructions = new Instructions(this);

            //displayBuffer = new byte[_screen.GetWidth(), _screen.GetHeight()];
            displayBuffer = new byte[64, 32];
            drawFlag = false;

            DelayTimer = 0;

            LoadProgram(program);
        }

        public GUI GetScreen()
        {
            return _screen;
        }

        public Keyboard GetKeyboard()
        {
            return _keyboard;
        }

        public RandomAccessMemory GetMemory()
        {
            return _randomAccessMemory;
        }

        public CPU GetCPU()
        {
            return _cpu;
        }

        public bool GetDrawFlag()
        {
            return drawFlag;
        }

        public void SetDrawFlag(bool value)
        {
            drawFlag = value;
        }

        public void SetDelayTimer(byte delay)
        {
            DelayTimer = delay;
        }

        public byte GetDelayTimer()
        {
            return DelayTimer;
        }

        public Task Tick()
        {
            ushort opcode = _randomAccessMemory.FetchOpcode();

            if(opcode == 0x0000)
            {
                return Task.CompletedTask;
            }

            string hex = opcode.ToString("X4");
            Debug.WriteLine(hex);

            _instructions.GetInstructions()[(byte)(opcode >> 12)](new Opcode(opcode));
            
            return Task.CompletedTask;
        }

        public void Tick60Hz()
        {
            if(DelayTimer > 0)
            {
                DelayTimer--;
            }

            if(drawFlag)
            {
                drawFlag = false;
                _screen.DrawDisplay(ScaleDisplayBuffer(displayBuffer));
            }
        }

        public byte[,] ScaleDisplayBuffer(byte[,] buffer)
        {
            byte[,] newBuffer = new byte[64 * _screen.GetScale(), 32 * _screen.GetScale()];
            
            for(int i = 0; i < newBuffer.GetLength(0); i++)
            {
                for(int j = 0; j < newBuffer.GetLength(1); j++)
                {
                    newBuffer[i, j] = buffer[i / _screen.GetScale(), j / _screen.GetScale()];
                }
            }

            return newBuffer;
        }

        public async Task EmulationLoop()
        {
            while (true)
            {
                var currentTime = stopWatch.Elapsed;
                var elapsedTime = currentTime - lastTime;

                while (elapsedTime >= targetElapsedTime60Hz)
                {
                    Tick60Hz();
                    elapsedTime -= targetElapsedTime60Hz;
                    lastTime += targetElapsedTime60Hz;
                }

                await Tick();

                Thread.Sleep(targetElapsedTime);
            }
        }

        private void LoadProgram(string program)
        {
            try
            {
                byte[] buffer = File.ReadAllBytes(program);

                _randomAccessMemory.LoadProgramIntoMemory(buffer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private readonly byte[] fontSet =
        {
            0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
            0x20, 0x60, 0x20, 0x20, 0x70, // 1
            0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
            0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
            0x90, 0x90, 0xF0, 0x10, 0x10, // 4
            0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
            0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
            0xF0, 0x10, 0x20, 0x40, 0x40, // 7
            0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
            0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
            0xF0, 0x90, 0xF0, 0x90, 0x90, // A
            0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
            0xF0, 0x80, 0x80, 0x80, 0xF0, // C
            0xE0, 0x90, 0x90, 0x90, 0xE0, // D
            0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
            0xF0, 0x80, 0xF0, 0x80, 0x80 // F
        };
    }
}
