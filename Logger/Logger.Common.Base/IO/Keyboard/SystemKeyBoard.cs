using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;




namespace Logger.Common.Base.IO.Keyboard
{
    public static class SystemKeyboard
    {
        #region Static Methods

        public static bool IsKeyPressed (SystemKeyboardKey key)
        {
            bool keyPressed = false;

            short result = SystemKeyboard.GetKeyState((int)key);

            switch (( result & 0x8000 ) >> 15)
            {
                default:
                {
                    keyPressed = true;
                    break;
                }

                case 0:
                {
                    keyPressed = false;
                    break;
                }

                case 1:
                {
                    keyPressed = true;
                    break;
                }
            }

            return keyPressed;
        }

        public static bool IsKeystrokePressed (IEnumerable<SystemKeyboardKey> keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            bool allPressed = true;

            foreach (SystemKeyboardKey key in keys)
            {
                if (!SystemKeyboard.IsKeyPressed(key))
                {
                    allPressed = false;
                }
            }

            return allPressed;
        }

        public static bool IsKeystrokeToggledOn (IEnumerable<SystemKeyboardKey> keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            bool allPressed = true;

            foreach (SystemKeyboardKey key in keys)
            {
                if (!SystemKeyboard.IsKeyToggledOn(key))
                {
                    allPressed = false;
                }
            }

            return allPressed;
        }

        public static bool IsKeyToggledOn (SystemKeyboardKey key)
        {
            bool keyToggledOn = false;

            short result = SystemKeyboard.GetKeyState((int)key);

            switch (result & 0x0001)
            {
                default:
                {
                    keyToggledOn = true;
                    break;
                }

                case 0:
                {
                    keyToggledOn = false;
                    break;
                }

                case 1:
                {
                    keyToggledOn = true;
                    break;
                }
            }

            return keyToggledOn;
        }

        public static SystemKeyboardKey[] MapCharacterToKey (string characters)
        {
            return SystemKeyboard.MapCharacterToKey(characters.ToArray());
        }

        public static SystemKeyboardKey[] MapCharacterToKey (params char[] characters)
        {
            List<SystemKeyboardKey> keys = new List<SystemKeyboardKey>();

            foreach (char character in characters)
            {
                char chr = char.ToUpperInvariant(character);

                if (( ( chr >= 65 ) && ( chr <= 90 ) ) || ( ( chr >= 48 ) && ( chr <= 57 ) ))
                {
                    keys.Add((SystemKeyboardKey)chr);
                }
            }

            return keys.ToArray();
        }

        [DllImport ("user32.dll", SetLastError = false)]
        private static extern short GetKeyState (int nVirtKey);

        #endregion
    }
}
