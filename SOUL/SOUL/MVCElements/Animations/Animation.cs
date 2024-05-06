using Microsoft.Xna.Framework.Graphics;

namespace game.MVCElements.Animations
{
    public class Animation
    {
        public Texture2D Texture { get => texture; }
        Texture2D texture;
        public float FrameTime { get => frameTime; }
        float frameTime;
        public bool IsLooping { get => isLooping; }
        bool isLooping;
        public int FrameCount { get => (Texture.Width / FrameHeight); }
        public int FrameWidth { get => (Texture.Height / 2); }
        public int FrameHeight { get => (Texture.Height); }
     
        public Animation(Texture2D texture, float frameTime, bool isLooping)
        {
            this.texture = texture;
            this.frameTime = frameTime;
            this.isLooping = isLooping;
        }
    }
}
