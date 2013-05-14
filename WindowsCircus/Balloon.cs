using Microsoft.Xna.Framework;

namespace CrossPlatformCircus
{
    public class Balloon
    {
        public Vector2 Position;
        public Rectangle BoundingBox
        {
            get
            {
                return new Rectangle((int)(Position.X + 6.0f), (int)(Position.Y + 1.0f), 19, 20);
            }
        }
        public int Color { get; set; }

        private const int switchTime = 350;
        private static int timeTillSwitch = switchTime;
        public static int State = 0;

        public Balloon()
        {
        }

        public static void UpdateAnimation(GameTime gameTime)
        {
            timeTillSwitch -= gameTime.ElapsedGameTime.Milliseconds;

            if (timeTillSwitch <= 0)
            {
                State = 1 - State;
                timeTillSwitch = switchTime;
            }
        }
    }
}
