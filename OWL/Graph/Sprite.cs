using Microsoft.Xna.Framework.Graphics;
using OWL.Rendering;
using OWL.Texture;
using System;

namespace OWL.Graph
{
    public class Sprite : Drawable
    {

        private Sprite _mask;
        public Sprite Mask
        {
            get { return _mask; }

            set
            {
                if (value != null)
                {
                    _mask = value;
                    Material.depthStencilState = Material.alphaMaskRead;
                    _mask.Material.depthStencilState = Material.alphaMaskWrite;

                    //Material.effect = Game.Instance.AlphaTestEffect;
                    //_mask.Material.effect = Game.Instance.AlphaTestEffect;

                    //Disable writing to screen
                    BlendState disableColorBufferState = new BlendState();
                    disableColorBufferState.ColorWriteChannels = ColorWriteChannels.None;
                    _mask.Material.blendState = disableColorBufferState;
                }
                else
                {
                    Material = new Material();
                    _mask.Material = new Material();
                    _mask = null;
                }
            }
        }

        public new float Width
        {
            get
            {
                return (int)(Math.Abs(_scale.X) * TextureRegion.Size.X);
            }
            set
            {
                int s = Math.Sign(_scale.X);
                s = s == 0 ? 1 : s;
                _scale.X = s * value / TextureRegion.Size.X;
            }
        }

        public new float Height
        {
            get
            {
                return (int)(Math.Abs(_scale.Y) * TextureRegion.Size.Y);
            }
            set
            {
                int s = Math.Sign(_scale.Y);
                s = s == 0 ? 1 : s;
                _scale.Y = s * value / TextureRegion.Size.Y;
            }
        }

        public Sprite(Texture2D texture) : base()
        {
            TextureRegion = new TextureRegion2D(texture);
        }

        public Sprite(TextureRegion2D region) : base()
        {
            TextureRegion = region;
        }

        public override void Draw(Renderer renderer)
        {
            CalculateVertices();
            renderer.Render(this);
        }

        public override void CalculateVertices()
        {
            var a = WorldMatrix.M11;
            var b = WorldMatrix.M12;
            var c = WorldMatrix.M21;
            var d = WorldMatrix.M22;
            var tx = WorldMatrix.M31;
            var ty = WorldMatrix.M32;

            var w1 = -Anchor.X * TextureRegion.SourceRectangle.Width;
            var w0 = w1 + TextureRegion.SourceRectangle.Width;

            var h1 = -Anchor.Y * TextureRegion.SourceRectangle.Height;
            var h0 = h1 + TextureRegion.SourceRectangle.Height;

            // xy
            vertexData[0].X = (int)((a * w1) + (c * h1) + tx);
            vertexData[0].Y = (int)((d * h1) + (b * w1) + ty);

            // xy
            vertexData[1].X = (int)((a * w0) + (c * h1) + tx);
            vertexData[1].Y = (int)((d * h1) + (b * w0) + ty);

            // xy
            vertexData[2].X = (int)((a * w0) + (c * h0) + tx);
            vertexData[2].Y = (int)((d * h0) + (b * w0) + ty);

            // xy
            vertexData[3].X = (int)((a * w1) + (c * h0) + tx);
            vertexData[3].Y = (int)((d * h0) + (b * w1) + ty);

            //after calculation update the bounds
            for (int i = 0; i < vertexData.Length; i++)
                Bounds.AddPoint(vertexData[i]);
        }
    }
}