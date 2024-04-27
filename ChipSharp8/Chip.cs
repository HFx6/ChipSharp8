
using System.Net;

namespace ChipSharp8
{
    public class Chip
    {
        // 35 opcodes
        public ushort opcode { get; set; }
        // 4kb memory
        public byte[] memory { get; set; }
        // 16 8-bit registers
        public byte[] V { get; set; }
        // Index register
        public ushort I { get; set; }
        // Program counter
        public ushort pc { get; set; }
        // 32 * 64 display
        public byte[] gfx { get; set; }
        // 8-bit delay timer
        public byte delay_timer { get; set; }
        // 8-bit sound timer
        public byte sound_timer { get; set; }
        // 16-bit stack
        public ushort[] stack { get; set; }
        // 16-bit stack pointer
        public ushort sp { get; set; }

        // 16 key inputs
        private bool[] Keys;

        // Timer counter
        private uint Counter;

        // Start of the ROM position expected in memory
        const ushort RomStart = 0x200;

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
            var bytes = readRomFile(rom);
            return BootChip(bytes);
        }

        // Load the fontset into memory
        public void LoadFont()
        {
            for (int i = 0; i < 80; i++)
            {
                memory[i] = fonts[i];
            }
        }

        public static byte[] readRomFile(string rom)
        {
            var bytes = File.ReadAllBytes(rom);
            return bytes;
        }

        public void LoadRomFile(byte[] rom)
        {
            for (int i = 0; i < rom.Length; i++)
            {
                memory[i + 512] = rom[i];
            }

        }

        public void Reset(string rom)
        {
            //pc = RomStart;
            //I = 0;
            //sp = 0;
            //gfx = new byte[64 * 32];


            opcode = 0;
            memory = new byte[4096];
            V = new byte[16];
            I = 0;
            pc = RomStart;
            gfx = new byte[64 * 32];
            delay_timer = 0;
            sound_timer = 0;
            Counter = 0;
            stack = new ushort[12];
            sp = 0;
            Keys = new bool[16];

            var bytes = readRomFile(rom);
            LoadRomFile(bytes);
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
            // Opcode is 2 bytes long
            opcode = (ushort)(memory[pc] << 8 | memory[pc + 1]);
            pc += 2;

            //  X: The second nibble.Used to look up one of the 16 registers(VX) from V0 through VF.
            //  Y: The third nibble.Also used to look up one of the 16 registers(VY) from V0 through VF.
            //  N: The fourth nibble.A 4 - bit number.
            //  NN: The second byte(third and fourth nibbles).An 8 - bit immediate number.
            //  NNN: The second, third and fourth nibbles.A 12 - bit immediate memory address.
            ushort NNN = (ushort)(opcode & 0x0FFF);
            byte NN = (byte)(opcode & 0x00FF);
            byte N = (byte)(opcode & 0x000F);
            byte X = (byte)((opcode & 0x0F00) >> 8);
            byte Y = (byte)((opcode & 0x00F0) >> 4);

            // We only need to check the first 4 bits of the opcode
            switch (opcode & 0xF000)
            {
                // We also need to check the last 4 bits of the opcode to determine the instruction type
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
                    // taken from demo
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
                sound_timer--;
            }
        }

        // Clear the screen
        public void OP_00E0()
        {
            gfx = new byte[64 * 32];
        }

        // Jump to address NNN
        public void OP_1NNN(ushort NNN)
        {
            pc = NNN;
        }

        // Return from a subroutine
        public void OP_00EE()
        {
            pc = stack[sp];
            sp--;

        }

        // Call subroutine at NNN
        public void OP_2NNN(ushort NNN)
        {
            sp++;
            stack[sp] = pc;
            pc = NNN;
        }

        // Skip next instruction if V[X] == NN
        public void OP_3XNN(ushort NN, ushort X)
        {
            if (V[X] == NN)
            {
                pc += 2;
            }
        }

        // Skip next instruction if V[X] != NN
        public void OP_4XNN(ushort NN, ushort X)
        {
            if (V[X] != NN)
            {
                pc += 2;
            }
        }

        // Skip next instruction if V[X] == V[Y]
        public void OP_5XY0(ushort X, ushort Y)
        {
            if (V[X] == V[Y])
            {
                pc += 2;
            }
        }

        // Skip next instruction if V[X] != V[Y]
        public void OP_9XY0(ushort X, ushort Y)
        {
            if (V[X] != V[Y])
            {
                pc += 2;
            }
        }

        // Set V[X] to NN
        public void OP_6XNN(ushort NN, ushort X)
        {
            V[X] = (byte)NN;
        }

        // Add NN to V[X]
        public void OP_7XNN(byte NN, byte X)
        {
            V[X] += NN;
        }

        //
        // Logical and arithmetic instructions
        //

        // Set V[X] to V[Y]
        public void OP_8XY0(ushort X, ushort Y)
        {
            V[X] = V[Y];
        }

        // Set V[X] to V[X] | V[Y]
        public void OP_8XY1(ushort X, ushort Y)
        {
            V[X] = (byte)(V[X] | V[Y]);
        }

        // Set V[X] to V[X] & V[Y]
        public void OP_8XY2(ushort X, ushort Y)
        {
            V[X] = (byte)(V[X] & V[Y]);
        }

        // Set V[X] to V[X] ^ V[Y]
        public void OP_8XY3(ushort X, ushort Y)
        {
            V[X] = (byte)(V[X] ^ V[Y]);
        }

        // Add V[Y] to V[X]
        public void OP_8XY4(ushort X, ushort Y)
        {
            V[X] = (byte)(V[X] + V[Y]);
            if (V[X] > 0xFF)
            {
                V[0xF] = 1;
            }
        }

        // Subtract V[Y] from V[X]
        public void OP_8XY5(ushort X, ushort Y)
        {
            if (V[Y] > V[X])
                V[0xF] = 0;
            else
                V[0xF] = 1;
            V[X] = (byte)(V[X] - V[Y]);
        }

        // Subtract V[X] from V[Y]
        public void OP_8XY7(ushort X, ushort Y)
        {
            if (V[X] > V[Y])
                V[0xF] = 0;
            else
                V[0xF] = 1;
            V[X] = (byte)(V[Y] - V[X]);
        }

        // Shift V[X] to the right by 1 and set V[F] to the least significant bit of V[X]
        public void OP_8XY6(ushort X, ushort Y)
        {
            V[0xF] = (byte)(V[X] & 0x1);
            V[X] >>= 0x1;
        }

        // Shift V[X] to the left by 1 and set V[F] to the most significant bit of V[X]
        public void OP_8XYE(ushort X, ushort Y)
        {
            V[0xF] = (byte)((V[X] & 0x80) >> 7);
            V[X] <<= 0x1;
        }

        // Set Index
        public void OP_ANNN(ushort NNN)
        {
            I = NNN;
        }

        // Jump to offset
        public void OP_BNNN(ushort NNN)
        {
            pc = (ushort)(NNN + V[0]);
        }

        // Random number
        public void OP_CXNN(ushort X, ushort NN)
        {
            V[X] = (byte)(new Random().Next(0, 255) & NN);
        }

        // Display n-byte sprite starting at memory location I at (V[X], V[Y]), set V[F] = collision
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

        // Skip next instruction if key with the value of V[X] is pressed
        private void OP_EX9E(byte X)
        {
            if (Keys[V[X]])
            {
                pc += 2;
            }
        }

        // Skip next instruction if key with the value of V[X] is not pressed
        private void OP_EXA1(byte X)
        {
            if (!Keys[V[X]])
            {
                pc += 2;
            }
        }

        // Set V[X] to the value of the delay timer
        private void OP_FX07(byte X)
        {
            V[X] = delay_timer;
        }

        // Set the delay timer to V[X]
        private void OP_FX15(byte X)
        {
            delay_timer = V[X];
        }

        // Set the sound timer to V[X]
        private void OP_FX18(byte X)
        {
            sound_timer = V[X];
        }

        // Add V[X] to I
        private void OP_FX1E(byte X)
        {
            I += V[X];
        }

        // Wait for a key press and store the result in V[X]
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

        // Set I to the location of the sprite for the character in V[X]
        private void OP_FX29(byte X)
        {
            I = (byte)(V[X] * 5);
        }

        // Binary-coded decimal representation of V[X] at I, I+1, I+2 (modulo 10)
        private void OP_FX33(byte X)
        {
            byte value = V[X];
            memory[I] = (byte)(value / 100);
            memory[I + 1] = (byte)((value / 10) % 10);
            memory[I + 2] = (byte)((value % 100) % 10);
        }

        // Store V[0] to V[X] in memory starting at I
        private void OP_FX55(byte X)
        {
            for (int i = 0; i <= X; i++)
            {
                memory[I + i] = V[i];
            }
        }

        // Read V[0] to V[X] from memory starting at I
        private void OP_FX65(byte X)
        {
            for (int i = 0; i <= X; i++)
            {
                V[i] = memory[I + i];
            }
        }

    }


}