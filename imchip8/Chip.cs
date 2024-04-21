using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;


namespace Sharp8
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

        public byte[] keys { get; set; }

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
                keys = new byte[16],
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
                case 0x1000:
                    OP_1NNN(NNN);
                    break;
                case 0x6000:
                    OP_6XNN(NN, X);
                    break;
                case 0x7000:
                    OP_7XNN(NN, X);
                    break;
                case 0xA000:
                    OP_ANNN(NNN);
                    break;
                case 0xD000:
                    OP_DXYN(X, Y, N);
                    break;
                default:
                    Console.WriteLine($"error: Invalid OpCode: {opcode:X4} @ PC = 0x{pc:X3}");
                    break;
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
        public void OP_6XNN(ushort NN, ushort X)
        {
            V[X] = (byte)NN;
        }
        public void OP_7XNN(byte NN, byte X)
        {
            V[X] += NN;
        }
        public void OP_ANNN(ushort NNN)
        {
            I = NNN;
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

    }


}