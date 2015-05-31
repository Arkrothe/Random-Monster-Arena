using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace RandomMonsterArena
{
    public static class InputManager
    {
        private static MouseState mouseState;
        public static KeyboardState keyboardState;
        private static bool wasMouseLeftPressed = false;
        private static bool wasMouseRightPressed = false;

        /*public static String GetKeyPressed()
        {
            String returnString = "";
            foreach (Keys key in keyboardState.GetPressedKeys())
            {
                returnString += key.ToString();
            }
            return returnString;
        }*/        
        public static Vector2 MouseCoordinates
        {
            get { return new Vector2(mouseState.X, mouseState.Y); }
        }
        public static bool WasMouseLeftClicked()
        {
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (wasMouseLeftPressed == false)
                {
                    wasMouseLeftPressed = true;
                    return true;
                }
            }
            else
            {
                if (wasMouseLeftPressed == true)
                {
                    wasMouseLeftPressed = false;
                }
            }
            return false;
        }
        public static bool WasMouseRightClicked()
        {
            if (mouseState.RightButton == ButtonState.Pressed)
            {
                if (wasMouseRightPressed == false)
                {
                    wasMouseRightPressed = true;
                    return true;
                }
            }
            else
            {
                if (wasMouseRightPressed == true)
                {
                    wasMouseRightPressed = false;
                }
            }
            return false;
        }        
        public static void Update(GameTime gameTime)
        {
            mouseState = Mouse.GetState();
            keyboardState = Keyboard.GetState();
        }

    }
}
