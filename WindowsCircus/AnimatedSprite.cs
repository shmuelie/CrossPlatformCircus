using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CrossPlatformCircus
{
    public class Animation
    {
        private double _timePerFrame;
        private Texture2D _spriteSheet;
        private int _currentFrame;
        private double _timeElapsed;
        private List<Rectangle> _frames;

        public Rectangle CurrentFrame
        {
            get
            {
                return _frames[_currentFrame];
            }
        }

        public Animation(Texture2D spriteSheet, double timePerFrame, int frameWidth, int frameHeight)
        {
            _spriteSheet = spriteSheet;
            _timePerFrame = timePerFrame;
            _timeElapsed = 0;

            if ((_spriteSheet.Width % frameWidth != 0) || (_spriteSheet.Height % frameHeight != 0))
            {
                throw new FormatException("The width and height of the sprite sheet does not create an exact number of frames");
            }

            _frames = new List<Rectangle>();

            for (int i = 0; i < _spriteSheet.Height; i += frameHeight)
            {
                for (int j = 0; j < _spriteSheet.Width; j += frameWidth)
                {
                    Rectangle thisFrame = new Rectangle(j, i, frameWidth, frameHeight);
                    _frames.Add(thisFrame);
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            _timeElapsed += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_timeElapsed >= _timePerFrame)
            {
                _currentFrame++;
                if (_currentFrame >= _frames.Count)
                {
                    _currentFrame = 0;
                }

                _timeElapsed -= _timePerFrame;
            }
        }

        public struct FrameInfo
        {
            public int Width;
            public int Height;

            public FrameInfo(int width, int height)
            {
                Width = width;
                Height = height;
            }
        }
    }


}
