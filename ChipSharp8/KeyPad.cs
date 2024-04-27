using ImGuiNET;
using System.Numerics;

namespace ChipSharp8
{
    internal class KeyPad
    {
        // The Chip object
        Chip _chip;
        // Flag to check if a key is pressed. This is used to check if a key is released
        bool _isKeyPadPressed = false;
        // The keys on the keypad
        string[] keys = ["1", "2", "3", "C", "4", "5", "6", "D", "7", "8", "9", "E", "A", "0", "B", "F"];
        // The key values
        int[] keyValues = [0x1, 0x2, 0x3, 0xC, 0x4, 0x5, 0x6, 0xD, 0x7, 0x8, 0x9, 0xE, 0xA, 0x0, 0xB, 0xF];

        // Constructor to initialize the Chip object
        public KeyPad(Chip chip)
        {
            _chip = chip;
        }

        public void Render()
        {
            ImGui.Begin("Keypad");
            ImGui.Columns(4, "mycolumns");
            ImGui.Separator();
            // Get the column width, so that the buttons can be of the same size (full width)
            float columnWidth = ImGui.GetColumnWidth();
            for (int i = 0; i < keys.Length; i++)
            {
                ImGui.Button(keys[i], new Vector2(columnWidth, 45));
                if (ImGui.IsItemActive())
                {
                    // Set the flag to true if a key is pressed and call KeyDown
                    _isKeyPadPressed = true;
                    _chip.KeyDown((byte)keyValues[i]);
                }
                // In case the key is pressed and the mouse is released, set the flag to false and call KeyUp
                else if (_isKeyPadPressed && ImGui.IsItemHovered() && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                {
                    _chip.KeyUp((byte)keyValues[i]);
                    _isKeyPadPressed = false;
                }
                ImGui.NextColumn();
                if ((i + 1) % 4 == 0)
                {
                    ImGui.Columns(1);
                    ImGui.Separator();
                    ImGui.Columns(4);
                }
            }

            ImGui.Columns(1);
            ImGui.End();

            {
                // There must be a better way to do this ;-;
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._0)))
                {
                    _chip.KeyDown(0x0);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._1)))
                {
                    _chip.KeyDown(0x1);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._2)))
                {
                    _chip.KeyDown(0x2);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._3)))
                {
                    _chip.KeyDown(0x3);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._4)))
                {
                    _chip.KeyDown(0x4);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._5)))
                {
                    _chip.KeyDown(0x5);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._6)))
                {
                    _chip.KeyDown(0x6);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._7)))
                {
                    _chip.KeyDown(0x7);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._8)))
                {
                    _chip.KeyDown(0x8);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey._9)))
                {
                    _chip.KeyDown(0x9);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.A)))
                {
                    _chip.KeyDown(0xA);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.B)))
                {
                    _chip.KeyDown(0xB);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.C)))
                {
                    _chip.KeyDown(0xC);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.D)))
                {
                    _chip.KeyDown(0xD);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.E)))
                {
                    _chip.KeyDown(0xE);
                }
                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.F)))
                {
                    _chip.KeyDown(0xF);
                }

                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._0)))
                {
                    _chip.KeyUp(0x0);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._1)))
                {
                    _chip.KeyUp(0x1);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._2)))
                {
                    _chip.KeyUp(0x2);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._3)))
                {
                    _chip.KeyUp(0x3);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._4)))
                {
                    _chip.KeyUp(0x4);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._5)))
                {
                    _chip.KeyUp(0x5);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._6)))
                {
                    _chip.KeyUp(0x6);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._7)))
                {
                    _chip.KeyUp(0x7);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._8)))
                {
                    _chip.KeyUp(0x8);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey._9)))
                {
                    _chip.KeyUp(0x9);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey.A)))
                {
                    _chip.KeyUp(0xA);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey.B)))
                {
                    _chip.KeyUp(0xB);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey.C)))
                {
                    _chip.KeyUp(0xC);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey.D)))
                {
                    _chip.KeyUp(0xD);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey.E)))
                {
                    _chip.KeyUp(0xE);
                }
                if (ImGui.IsKeyReleased(ImGui.GetKeyIndex(ImGuiKey.F)))
                {
                    _chip.KeyUp(0xF);
                }

            }

        }
    }
}
