using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OWL.Graph;
using System.Runtime.CompilerServices;

/// <summary>
/// Simple renderer class, manages a single batcher instance and can have an optional interaction manager
/// adapted from Nez - https://github.com/prime31/Nez/blob/a3229fa9d30898e48769f0a29ce3992fc14bcd6a/Nez.Portable/Graphics/Renderers/Renderer.cs
/// </summary>
namespace OWL.Rendering
{
    public class Renderer
    {
        private const float ClockwiseNinetyDegreeRotation = (float)(System.Math.PI / 2.0f);

        public Batcher Batcher { get; protected set; }

        protected Material currentMaterial = new Material();

        private int _displayID = -1;
        public int DisplayID { get { return _displayID++; } }//tracks draw order

        public Renderer(GraphicsDevice graphicsDevice)
        {
            Batcher = new Batcher(graphicsDevice);
        }

        public Renderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            Batcher = new Batcher(graphicsDevice, spriteBatch);
        }

        /// <summary>
        /// begins the batch rendering
        /// </summary>
        public virtual void BeginRender()
        {
            Batcher.Begin(currentMaterial);
            _displayID = 0;
        }

        /// <summary>
		/// ends the Batcher and clears the RenderTarget if it had a RenderTarget
		/// </summary>
		public virtual void EndRender()
        {
            Batcher.End();
            _displayID = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Render(Container container)
        {
            container.UpdateTransforms(this);
        }

        /// <summary>
        /// renders the RenderableComponent flushing the Batcher and resetting current material if necessary
        /// </summary>
        /// <param name="renderable">Renderable.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Render(Sprite sprite)
        {
            Rectangle frame = sprite.TextureRegion.Frame;
            Rectangle sourceRect = sprite.TextureRegion.SourceRectangle;
            Rectangle trim = sprite.TextureRegion.Trim;
            Vector2 size = sprite.TextureRegion.Size;

            Vector2 origin = new Vector2(size.X * sprite.Anchor.X - trim.X, size.Y * sprite.Anchor.Y - trim.Y);

            sprite.WorldMatrix.Decompose(out Vector2 scale, out float rotation, out Vector2 translation);

            //todo - complete rotated texture implementation
            /*
            if (sprite.TextureRegion.IsRotated)
            {
                frame = new Rectangle(frame.X, frame.Y, frame.Height, frame.Width);
                rotation -= ClockwiseNinetyDegreeRotation;
                origin = new Vector2(size.X * (1 - sprite.Anchor.Y) - trim.Y, size.Y * sprite.Anchor.X - trim.X);
                origin = new Vector2(origin.X, origin.Y);
            }
            */

            //todo - this is exclusive with flip X taking precendent, this can probably be fixed by using a binary op between the two
            if (sprite.Scale.Y < 0)
                sprite.Effect = SpriteEffects.FlipVertically;
            else
                sprite.Effect = SpriteEffects.None;

            if (sprite.Scale.X < 0)
                sprite.Effect = SpriteEffects.FlipHorizontally;
            else
                sprite.Effect = SpriteEffects.None;

            bool flushBeforeDraw = false;

            if (sprite.Material.blendState != currentMaterial.blendState)
            {
                currentMaterial.blendState = sprite.Material.blendState;
                flushBeforeDraw = true;
            }

            if (sprite.Material.depthStencilState != currentMaterial.depthStencilState)
            {
                currentMaterial.depthStencilState = sprite.Material.depthStencilState;
                flushBeforeDraw = true;
            }

            if (sprite.Material.effect != currentMaterial.effect)
            {
                currentMaterial.effect = sprite.Material.effect;
                flushBeforeDraw = true;
            }

            if (flushBeforeDraw)
                Flush();

            Batcher.DrawSprite(
               sprite.TextureRegion.BaseTexture,
               translation,
               frame,
               sprite.Tint * sprite.WorldAlpha,
               rotation,
               origin,
               scale,
               sprite.Effect,
               0
           );
        }

        /// <summary>
        /// Force flushes the Batcher by calling End then Begin on it.
        /// </summary>
        void Flush()
        {
            Batcher.End();
            Batcher.Begin(currentMaterial);
        }
    }
}