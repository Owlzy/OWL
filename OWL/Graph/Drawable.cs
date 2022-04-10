using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OWL.Rendering;
using OWL.Texture;

namespace OWL.Graph
{
    public abstract class Drawable : Container
    {
        public Color Tint { get; set; } = Color.White;

        public Material Material { get; set; } = new Material();

        public Texture2D Texture { get { return TextureRegion.BaseTexture; } set { TextureRegion = new TextureRegion2D(value); } }
        public TextureRegion2D TextureRegion { get; set; }

        public Vector2 Anchor = new Vector2();
        public SpriteEffects Effect = SpriteEffects.None;

        public Drawable() : base() { }

        public void SetAnchor(Vector2 anchor)
        {
            Anchor = anchor;
        }
        public void SetAnchor(float x, float y)
        {
            Anchor.X = x;
            Anchor.Y = y;
        }
        public void SetAnchor(float x)
        {
            Anchor.X = x;
            Anchor.Y = x;
        }
    }
}
