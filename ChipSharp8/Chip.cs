using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;


namespace ChipSharp8
{
    public class Chip
    {

        public ushort opcode { get; set; }
        public byte[] memory { get; set; }
        public byte[] V { get; set; }
        public ushort I { get; set; }
        public ushort pc { get; set; }
        public byte[] gfx { get; set; }
        public byte delay_timer { get; set; }
        public byte sound_timer { get; set; }

        public ushort[] stack { get; set; }
        public ushort sp { get; set; }


        private bool[] Keys;
        private uint Counter;

        const ushort RomStart = 0x200;

        // private IWindow Window;

        private byte[] fonts = {
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
            0xF0, 0x80, 0xF0, 0x80, 0x80  // F
        };


        public static Chip BootChip(byte[] rom)
        {
            Console.WriteLine(rom);
            Chip chip = new Chip
            {
                opcode = 0,
                memory = new byte[4096],
                V = new byte[16],
                I = 0,
                pc = RomStart,
                gfx = new byte[64 * 32],
                delay_timer = 0,
                sound_timer = 0,
                Counter = 0,
                stack = new ushort[12],
                sp = 0,
                Keys = new bool[16],
            };


            chip.LoadRomFile(rom);
            chip.LoadFont();
            return chip;

        }

        public static Chip BootChip(string rom)
        {
            var bytes = File.ReadAllBytes(rom);
            Console.WriteLine(rom);
            return BootChip(bytes);
        }

        public void LoadFont()
        {
            for (int i = 0; i < 80; i++)
            {
                memory[i] = fonts[i];
            }
        }

        public void LoadRomFile(byte[] rom)
        {
            for (int i = 0; i < rom.Length; i++)
            {
                memory[i + 512] = rom[i];
            }

        }

        public void Reset()
        {
            pc = RomStart;
            I = 0;
            sp = 0;
            gfx = new byte[64 * 32];
        }

        public void KeyUp(byte key)
        {
            Keys[key] = false;
        }

        public void KeyDown(byte key)
        {
            Keys[key] = true;
        }

        public void EmulateCycle()
        {
            opcode = (ushort)(memory[pc] << 8 | memory[pc + 1]);
            pc += 2;

            ushort NNN = (ushort)(opcode & 0x0FFF);
            byte NN = (byte)(opcode & 0x00FF);
            byte N = (byte)(opcode & 0x000F);
            byte X = (byte)((opcode & 0x0F00) >> 8);
            byte Y = (byte)((opcode & 0x00F0) >> 4);

            switch (opcode & 0xF000)
            {
                case 0x0000 when opcode == 0x00E0:
                    OP_00E0();
                    break;
                case 0x0000 when opcode == 0x00EE:
                    OP_00EE();
                    break;
                case 0x1000:
                    OP_1NNN(NNN);
                    break;
                case 0x2000:
                    OP_2NNN(NNN);
                    break;
                case 0x3000:
                    OP_3XNN(NN, X);
                    break;
                case 0x4000:
                    OP_4XNN(NN, X);
                    break;
                case 0x5000:
                    OP_5XY0(X, Y);
                    break;
                case 0x6000:
                    OP_6XNN(NN, X);
                    break;
                case 0x7000:
                    OP_7XNN(NN, X);
                    break;
                case 0x8000 when (opcode & 0x000F) == 0:
                    OP_8XY0(X, Y);
                    break;
                case 0x8000 when (opcode & 0x000F) == 1:
                    OP_8XY1(X, Y);
                    break;
                case 0x8000 when (opcode & 0x000F) == 2:
                    OP_8XY2(X, Y);
                    break;
                case 0x8000 when (opcode & 0x000F) == 3:
                    OP_8XY3(X, Y);
                    break;
                case 0x8000 when (opcode & 0x000F) == 4:
                    OP_8XY4(X, Y);
                    break;
                case 0x8000 when (opcode & 0x000F) == 5:
                    OP_8XY5(X, Y);
                    break;
                case 0x8000 when (opcode & 0x000F) == 6:
                    OP_8XY6(X, Y);
                    break;
                case 0x8000 when (opcode & 0x000F) == 7:
                    OP_8XY7(X, Y);
                    break;
                case 0x8000 when (opcode & 0x000F) == 0xE:
                    OP_8XYE(X, Y);
                    break;
                case 0x9000:
                    OP_9XY0(X, Y);
                    break;
                case 0xA000:
                    OP_ANNN(NNN);
                    break;
                case 0xB000:
                    OP_BNNN(NNN);
                    break;
                case 0xC000:
                    OP_CXNN(X, NN);
                    break;
                case 0xD000:
                    OP_DXYN(X, Y, N);
                    break;
                case 0xE000 when (opcode & 0x00FF) == 0x9E:
                    OP_EX9E(X);
                    break;
                case 0xE000 when (opcode & 0x00FF) == 0xA1:
                    OP_EXA1(X);
                    break;
                case 0xF000 when (opcode & 0x00FF) == 0x07:
                    OP_FX07(X);
                    break;
                case 0xF000 when (opcode & 0x00FF) == 0x0A:
                    OP_FX0A(X);
                    break;
                case 0xF000 when (opcode & 0x00FF) == 0x15:
                    OP_FX15(X);
                    break;
                case 0xF000 when (opcode & 0x00FF) == 0x18:
                    OP_FX18(X);
                    break;
                case 0xF000 when (opcode & 0x00FF) == 0x1E:
                    OP_FX1E(X);
                    break;
                case 0xF000 when (opcode & 0x00FF) == 0x29:
                    OP_FX29(X);
                    break;
                case 0xF000 when (opcode & 0x00FF) == 0x33:
                    OP_FX33(X);
                    break;
                case 0xF000 when (opcode & 0x00FF) == 0x55:
                    OP_FX55(X);
                    break;
                case 0xF000 when (opcode & 0x00FF) == 0x65:
                    OP_FX65(X);
                    break;
                default:
                    throw new InvalidOperationException($"error: Invalid OpCode: {opcode:X4} @ PC = 0x{pc:X3}");
            }


            if ((Counter % 10) == 0)
            {
                UpdateTimers();
            }

            Counter++;

        }


        private void UpdateTimers()
        {
            if (delay_timer > 0) delay_timer--;
            if (sound_timer > 0)
            {
                // Window?.Beep();
                sound_timer--;
            }
        }

        public void OP_00E0()
        {
            gfx = new byte[64 * 32];
        }
        public void OP_1NNN(ushort NNN)
        {
            pc = NNN;
        }
        public void OP_00EE()
        {
            pc = stack[sp];
            sp--;

        }
        public void OP_2NNN(ushort NNN)
        {
            sp++;
            stack[sp] = pc;
            pc = NNN;
        }
        public void OP_3XNN(ushort NN, ushort X)
        {
            if (V[X] == NN)
            {
                pc += 2;
            }
        }
        public void OP_4XNN(ushort NN, ushort X)
        {
            if (V[X] != NN)
            {
                pc += 2;
            }
        }
        public void OP_5XY0(ushort X, ushort Y)
        {
            if (V[X] == V[Y])
            {
                pc += 2;
            }
        }
        public void OP_9XY0(ushort X, ushort Y)
        {
            if (V[X] != V[Y])
            {
                pc += 2;
            }
        }
        public void OP_6XNN(ushort NN, ushort X)
        {
            V[X] = (byte)NN;
        }
        public void OP_7XNN(byte NN, byte X)
        {
            V[X] += NN;
        }
        public void OP_8XY0(ushort X, ushort Y)
        {
            V[X] = V[Y];
        }
        public void OP_8XY1(ushort X, ushort Y)
        {
            V[X] = (byte)(V[X] | V[Y]);
        }
        public void OP_8XY2(ushort X, ushort Y)
        {
            V[X] = (byte)(V[X] & V[Y]);
        }
        public void OP_8XY3(ushort X, ushort Y)
        {
            V[X] = (byte)(V[X] ^ V[Y]);
        }
        public void OP_8XY4(ushort X, ushort Y)
        {
            V[X] = (byte)(V[X] + V[Y]);
            if (V[X] > 0xFF)
            {
                V[0xF] = 1;
            }
        }
        public void OP_8XY5(ushort X, ushort Y)
        {
            if (V[Y] > V[X])
                V[0xF] = 0;
            else
                V[0xF] = 1;
            V[X] = (byte)(V[X] - V[Y]);
        }
        public void OP_8XY7(ushort X, ushort Y)
        {
            if (V[X] > V[Y])
                V[0xF] = 0;
            else
                V[0xF] = 1;
            V[X] = (byte)(V[Y] - V[X]);
        }
        public void OP_8XY6(ushort X, ushort Y)
        {
            V[0xF] = (byte)(V[X] & 0x1);
            V[X] >>= 0x1;
        }
        public void OP_8XYE(ushort X, ushort Y)
        {
            V[0xF] = (byte)((V[X] & 0x80) >> 7);
            V[X] <<= 0x1;
        }
        public void OP_ANNN(ushort NNN)
        {
            I = NNN;
        }
        public void OP_BNNN(ushort NNN)
        {
            pc = (ushort)(NNN + V[0]);
        }
        public void OP_CXNN(ushort X, ushort NN)
        {
            V[X] = (byte)(new Random().Next(0, 255) & NN);
        }
        private void OP_DXYN(byte X, byte Y, byte N)
        {
            V[0xF] = 0;
            for (int line = 0; line < N; line++)
            {
                var y = (V[Y] + line) % 32;

                byte pixel = memory[I + line];

                for (int column = 0; column < 8; column++)
                {
                    if ((pixel & 0x80) != 0)
                    {
                        var x = (V[X] + column) % 64;

                        if (gfx[y * 64 + x] == 1)
                        {
                            V[0xF] = 1;
                        }

                        gfx[y * 64 + x] ^= 1;
                    }

                    pixel <<= 0x1;
                }
            }
        }
        private void OP_EX9E(byte X)
        {
            if (Keys[V[X]])
            {
                pc += 2;
            }
        }  
        private void OP_EXA1(byte X)
        {
            if (!Keys[V[X]])
            {
                pc += 2;
            }
        }
        private void OP_FX07(byte X)
        {
            V[X] = delay_timer;
        }
        private void OP_FX15(byte X)
        {
            delay_timer = V[X];
        }
        private void OP_FX18(byte X)
        {
            sound_timer = V[X];
        }
        private void OP_FX1E(byte X)
        {
            I += V[X];
        }
        private void OP_FX0A(byte X)
        {
            while (true)
            {
                for (int i = 0; i < 0xF; i++)
                {
                    if (Keys[i])
                    {
                        V[X] = (byte)i;
                        return;
                    }
                }
            }
        }
        private void OP_FX29(byte X)
        {
            I = (byte)(V[X] * 5);
        }
        private void OP_FX33(byte X)
        {
            byte value = V[X];
            memory[I] = (byte)(value / 100);
            memory[I + 1] = (byte)((value / 10) % 10);
            memory[I + 2] = (byte)((value % 100) % 10);
        }
        private void OP_FX55(byte X)
        {
            for (int i = 0; i <= X; i++)
            {
                memory[I + i] = V[i];
            }
        }
        private void OP_FX65(byte X)
        {
            for (int i = 0; i <= X; i++)
            {
                V[i] = memory[I + i];
            }
        }

    }


}