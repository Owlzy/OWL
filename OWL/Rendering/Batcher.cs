/*
The MIT License(MIT)

Copyright(c) 2016 Mike

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/**
 * adapted from Nez - https://github.com/prime31/Nez/blob/a3229fa9d30898e48769f0a29ce3992fc14bcd6a/Nez.Portable/Graphics/Batcher/Batcher.cs
 */
namespace OWL.Rendering
{
    public class Batcher : GraphicsResource
    {
        /// <summary>
        /// If true, destination positions will be rounded before being drawn.
        /// </summary>
        public bool shouldRoundDestinations = false;

        #region variables

        public enum BatchState { Quad, Text, Spine };
        BatchState currentState = BatchState.Quad;

        // Buffer objects used for actual drawing
        DynamicVertexBuffer _vertexBuffer;
        IndexBuffer _indexBuffer;

        // Local data stored before buffering to GPU
        VertexPositionColorTexture4[] _vertices;
        Texture2D[] _textures;

        //current number of sprites in batch
        int _numSprites = 0;

        // Default SpriteEffect
        SpriteEffect _spriteEffect;
        EffectPass _spriteEffectPass;

        Effect _currentEffect;
        public SpriteBatch SpriteBatch { get; private set; }

        // Keep render state for non-Immediate modes.
        BlendState _blendState;
        // SamplerState _samplerState;
        DepthStencilState _depthStencilState;
        RasterizerState _rasterizerState;

        private bool _beginCalled = false;

        #endregion

        #region static variables and constants

        const int MAX_SPRITES = 2048;
        const int MAX_VERTICES = MAX_SPRITES * 4;
        const int MAX_INDICES = MAX_SPRITES * 6;

        // Used to calculate texture coordinates
        static readonly float[] _cornerOffsetX = new float[] { 0.0f, 1.0f, 0.0f, 1.0f };
        static readonly float[] _cornerOffsetY = new float[] { 0.0f, 0.0f, 1.0f, 1.0f };
        static readonly short[] _indexData = GenerateIndexArray();

        #endregion

        public Batcher(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;

            SpriteBatch = new SpriteBatch(graphicsDevice);

            _vertices = new VertexPositionColorTexture4[MAX_SPRITES];
            _textures = new Texture2D[MAX_SPRITES];
            _vertexBuffer = new DynamicVertexBuffer(graphicsDevice, typeof(VertexPositionColorTexture), MAX_VERTICES, BufferUsage.WriteOnly);
            _indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, MAX_INDICES, BufferUsage.WriteOnly);
            _indexBuffer.SetData(_indexData);

            _spriteEffect = new SpriteEffect(graphicsDevice);
            _spriteEffectPass = _spriteEffect.CurrentTechnique.Passes[0];
        }

        public Batcher(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            this.graphicsDevice = graphicsDevice;

            SpriteBatch = spriteBatch;

            _vertices = new VertexPositionColorTexture4[MAX_SPRITES];
            _textures = new Texture2D[MAX_SPRITES];
            _vertexBuffer = new DynamicVertexBuffer(graphicsDevice, typeof(VertexPositionColorTexture), MAX_VERTICES, BufferUsage.WriteOnly);
            _indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, MAX_INDICES, BufferUsage.WriteOnly);
            _indexBuffer.SetData(_indexData);

            _spriteEffect = new SpriteEffect(graphicsDevice);
            _spriteEffectPass = _spriteEffect.CurrentTechnique.Passes[0];
        }

        protected override void Dispose(bool disposing)
        {
            if (!isDisposed && disposing)
            {
                _spriteEffect.Dispose();
                _indexBuffer.Dispose();
                _vertexBuffer.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// begins batch
        /// </summary>
        public void Begin()
        {
            Begin(BlendState.AlphaBlend, DepthStencilState.None, RasterizerState.CullCounterClockwise);
        }

        /// <summary>
        /// begins batch, takes a material as param
        /// </summary>
        public void Begin(Material material)
        {
            Begin(material.blendState, material.depthStencilState, RasterizerState.CullCounterClockwise, material.effect);
        }

        /// <summary>
        /// begins batch
        /// </summary>
        public void Begin(BlendState blendState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect)
        {
            _beginCalled = true;

            _blendState = blendState ?? BlendState.AlphaBlend;
            _depthStencilState = depthStencilState ?? DepthStencilState.None;
            _rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;

            _currentEffect = effect;
        }

        public void Begin(BlendState blendState, DepthStencilState depthStencilState, RasterizerState rasterizerState)
        {
            _beginCalled = true;

            _blendState = blendState ?? BlendState.AlphaBlend;
            _depthStencilState = depthStencilState ?? DepthStencilState.None;
            _rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
        }

        public void End()
        {
            //Insist.isTrue(_beginCalled, "End was called, but Begin has not yet been called. You must call Begin successfully before you can call End.");
            _beginCalled = false;

            //  if (!_disableBatching)
            FlushBatch();

            _currentEffect = null;
        }

        public void DrawSprite(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            CheckBegin();
            PushSprite(
              texture,
              sourceRectangle,
              position.X,
              position.Y,
              scale.X,
              scale.Y,
              color,
              origin,
              rotation,
              layerDepth,
              (byte)(effects & (SpriteEffects)0x03),
              false,
              0, 0, 0, 0
          );
        }

        public void DrawQuad(Texture2D texture, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth,
            Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight)
        {
            CheckBegin();
            PushQuad(
                texture,
                sourceRectangle,
                color,
                rotation,
                origin,
                layerDepth,
                (byte)(effects & (SpriteEffects)0x03),
                false,
                topLeft,
                topRight,
                bottomLeft,
                bottomRight
            );
        }

        /// <summary>
		/// the meat of the Batcher. This is where it all goes down
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        void PushSprite(Texture2D texture, Rectangle? sourceRectangle, float destinationX, float destinationY, float destinationW, float destinationH, Color color, Vector2 origin,
                        float rotation, float depth, byte effects, bool destSizeInPixels, float skewTopX, float skewBottomX, float skewLeftY, float skewRightY)
        {
            if (currentState == BatchState.Text)
            {
                currentState = BatchState.Quad;
                SpriteBatch.End();
            }

            // out of space, flush
            if (_numSprites >= MAX_SPRITES)
                FlushBatch();

            if (shouldRoundDestinations)
            {
                destinationX = Mathf.Round(destinationX);
                destinationY = Mathf.Round(destinationY);
            }

            // Source/Destination/Origin Calculations
            float sourceX, sourceY, sourceW, sourceH;
            float originX, originY;
            if (sourceRectangle.HasValue)
            {
                var inverseTexW = 1.0f / (float)texture.Width;
                var inverseTexH = 1.0f / (float)texture.Height;

                sourceX = sourceRectangle.Value.X * inverseTexW;
                sourceY = sourceRectangle.Value.Y * inverseTexH;
                sourceW = sourceRectangle.Value.Width * inverseTexW;
                sourceH = sourceRectangle.Value.Height * inverseTexH;

                originX = (origin.X / sourceW) * inverseTexW;
                originY = (origin.Y / sourceH) * inverseTexH;

                if (!destSizeInPixels)
                {
                    destinationW *= sourceRectangle.Value.Width;
                    destinationH *= sourceRectangle.Value.Height;
                }
            }
            else
            {
                sourceX = 0.0f;
                sourceY = 0.0f;
                sourceW = 1.0f;
                sourceH = 1.0f;

                originX = origin.X * (1.0f / texture.Width);
                originY = origin.Y * (1.0f / texture.Height);

                if (!destSizeInPixels)
                {
                    destinationW *= texture.Width;
                    destinationH *= texture.Height;
                }
            }

            // Rotation Calculations
            float rotationMatrix1X;
            float rotationMatrix1Y;
            float rotationMatrix2X;
            float rotationMatrix2Y;
            if (!Mathf.WithinEpsilon(rotation, 0))
            {
                var sin = Mathf.Sin(rotation);
                var cos = Mathf.Cos(rotation);
                rotationMatrix1X = cos;
                rotationMatrix1Y = sin;
                rotationMatrix2X = -sin;
                rotationMatrix2Y = cos;
            }
            else
            {
                rotationMatrix1X = 1.0f;
                rotationMatrix1Y = 0.0f;
                rotationMatrix2X = 0.0f;
                rotationMatrix2Y = 1.0f;
            }

            // flip our skew values if we have a flipped sprite
            if (effects != 0)
            {
                skewTopX *= -1;
                skewBottomX *= -1;
                skewLeftY *= -1;
                skewRightY *= -1;
            }

            // calculate vertices
            // top-left
            var cornerX = (_cornerOffsetX[0] - originX) * destinationW + skewTopX;
            var cornerY = (_cornerOffsetY[0] - originY) * destinationH - skewLeftY;
            _vertices[_numSprites].position0.X = (
                (rotationMatrix2X * cornerY) +
                (rotationMatrix1X * cornerX) +
                destinationX
            );
            _vertices[_numSprites].position0.Y = (
                (rotationMatrix2Y * cornerY) +
                (rotationMatrix1Y * cornerX) +
                destinationY
            );

            // top-right
            cornerX = (_cornerOffsetX[1] - originX) * destinationW + skewTopX;
            cornerY = (_cornerOffsetY[1] - originY) * destinationH - skewRightY;
            _vertices[_numSprites].position1.X = (
                (rotationMatrix2X * cornerY) +
                (rotationMatrix1X * cornerX) +
                destinationX
            );
            _vertices[_numSprites].position1.Y = (
                (rotationMatrix2Y * cornerY) +
                (rotationMatrix1Y * cornerX) +
                destinationY
            );

            // bottom-left
            cornerX = (_cornerOffsetX[2] - originX) * destinationW + skewBottomX;
            cornerY = (_cornerOffsetY[2] - originY) * destinationH - skewLeftY;
            _vertices[_numSprites].position2.X = (
                (rotationMatrix2X * cornerY) +
                (rotationMatrix1X * cornerX) +
                destinationX
            );
            _vertices[_numSprites].position2.Y = (
                (rotationMatrix2Y * cornerY) +
                (rotationMatrix1Y * cornerX) +
                destinationY
            );

            // bottom-right
            cornerX = (_cornerOffsetX[3] - originX) * destinationW + skewBottomX;
            cornerY = (_cornerOffsetY[3] - originY) * destinationH - skewRightY;
            _vertices[_numSprites].position3.X = (
                (rotationMatrix2X * cornerY) +
                (rotationMatrix1X * cornerX) +
                destinationX
            );
            _vertices[_numSprites].position3.Y = (
                (rotationMatrix2Y * cornerY) +
                (rotationMatrix1Y * cornerX) +
                destinationY
            );

            _vertices[_numSprites].textureCoordinate0.X = (_cornerOffsetX[0 ^ effects] * sourceW) + sourceX;
            _vertices[_numSprites].textureCoordinate0.Y = (_cornerOffsetY[0 ^ effects] * sourceH) + sourceY;
            _vertices[_numSprites].textureCoordinate0.Z = 1;
            _vertices[_numSprites].textureCoordinate1.X = (_cornerOffsetX[1 ^ effects] * sourceW) + sourceX;
            _vertices[_numSprites].textureCoordinate1.Y = (_cornerOffsetY[1 ^ effects] * sourceH) + sourceY;
            _vertices[_numSprites].textureCoordinate1.Z = 1;
            _vertices[_numSprites].textureCoordinate2.X = (_cornerOffsetX[2 ^ effects] * sourceW) + sourceX;
            _vertices[_numSprites].textureCoordinate2.Y = (_cornerOffsetY[2 ^ effects] * sourceH) + sourceY;
            _vertices[_numSprites].textureCoordinate2.Z = 1;
            _vertices[_numSprites].textureCoordinate3.X = (_cornerOffsetX[3 ^ effects] * sourceW) + sourceX;
            _vertices[_numSprites].textureCoordinate3.Y = (_cornerOffsetY[3 ^ effects] * sourceH) + sourceY;
            _vertices[_numSprites].textureCoordinate3.Z = 1;
            _vertices[_numSprites].position0.Z = depth;
            _vertices[_numSprites].position1.Z = depth;
            _vertices[_numSprites].position2.Z = depth;
            _vertices[_numSprites].position3.Z = depth;
            _vertices[_numSprites].color0 = color;
            _vertices[_numSprites].color1 = color;
            _vertices[_numSprites].color2 = color;
            _vertices[_numSprites].color3 = color;

            _textures[_numSprites] = texture;
            _numSprites++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void PushQuad(Texture2D texture, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float depth, byte effects, bool destSizeInPixels, Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight)
        {

            if (currentState == BatchState.Text)
            {
                currentState = BatchState.Quad;
                SpriteBatch.End();
            }

            // out of space, flush
            if (_numSprites >= MAX_SPRITES)
                FlushBatch();

            // Source/Destination/Origin Calculations
            float sourceX, sourceY, sourceW, sourceH;
            float originX, originY;
            if (sourceRectangle.HasValue)
            {
                var inverseTexW = 1.0f / (float)texture.Width;
                var inverseTexH = 1.0f / (float)texture.Height;

                sourceX = sourceRectangle.Value.X * inverseTexW;
                sourceY = sourceRectangle.Value.Y * inverseTexH;
                sourceW = sourceRectangle.Value.Width * inverseTexW;
                sourceH = sourceRectangle.Value.Height * inverseTexH;

                originX = (origin.X / sourceW) * inverseTexW;
                originY = (origin.Y / sourceH) * inverseTexH;
            }
            else
            {
                sourceX = 0.0f;
                sourceY = 0.0f;
                sourceW = 1.0f;
                sourceH = 1.0f;

                originX = origin.X * (1.0f / texture.Width);
                originY = origin.Y * (1.0f / texture.Height);
            }

            // Rotation Calculations
            float rotationMatrix1X;
            float rotationMatrix1Y;
            float rotationMatrix2X;
            float rotationMatrix2Y;

            if (!Mathf.WithinEpsilon(rotation, 0))
            {
                var sin = Mathf.Sin(rotation);
                var cos = Mathf.Cos(rotation);
                rotationMatrix1X = cos;
                rotationMatrix1Y = sin;
                rotationMatrix2X = -sin;
                rotationMatrix2Y = cos;
            }
            else
            {
                rotationMatrix1X = 1.0f;
                rotationMatrix1Y = 0.0f;
                rotationMatrix2X = 0.0f;
                rotationMatrix2Y = 1.0f;
            }

            // calculate vertices
            // top-left
            var cornerX = topLeft.X - originX;
            var cornerY = topLeft.Y - originY;
            _vertices[_numSprites].position0.X = (
                (rotationMatrix2X * cornerY) +
                (rotationMatrix1X * cornerX)
            );
            _vertices[_numSprites].position0.Y = (
                (rotationMatrix2Y * cornerY) +
                (rotationMatrix1Y * cornerX)
            );

            // top-right
            cornerX = topRight.X - originX;
            cornerY = topRight.Y - originY;
            _vertices[_numSprites].position1.X = (
                (rotationMatrix2X * cornerY) +
                (rotationMatrix1X * cornerX)
            );
            _vertices[_numSprites].position1.Y = (
                (rotationMatrix2Y * cornerY) +
                (rotationMatrix1Y * cornerX)
            );

            // bottom-left
            cornerX = bottomLeft.X - originX;
            cornerY = bottomLeft.Y - originY;
            _vertices[_numSprites].position2.X = (
                (rotationMatrix2X * cornerY) +
                (rotationMatrix1X * cornerX)
            );
            _vertices[_numSprites].position2.Y = (
                (rotationMatrix2Y * cornerY) +
                (rotationMatrix1Y * cornerX)
            );

            // bottom-right
            cornerX = bottomRight.X - originX;
            cornerY = bottomRight.Y - originY;
            _vertices[_numSprites].position3.X = (
                (rotationMatrix2X * cornerY) +
                (rotationMatrix1X * cornerX)
            );
            _vertices[_numSprites].position3.Y = (
                (rotationMatrix2Y * cornerY) +
                (rotationMatrix1Y * cornerX)
            );

            var p0 = _vertices[_numSprites].position0;
            var p1 = _vertices[_numSprites].position1;
            var p2 = _vertices[_numSprites].position2;
            var p3 = _vertices[_numSprites].position3;

            //todo - try and refactor this so its less lines and less ugly?
            var A0 = p1.Y - p2.Y;
            var B0 = p2.X - p1.X;

            var A1 = p3.Y - p0.Y;
            var B1 = p0.X - p3.X;

            var C0 = A0 * p2.X + B0 * p2.Y;
            var C1 = A1 * p0.X + B1 * p0.Y;

            var det = A0 * B1 - A1 * B0;
            var center = new Vector2((B1 * C0 - B0 * C1) / det, (A0 * C1 - A1 * C0) / det);

            float d0 = (new Vector2(p0.X, p0.Y) - center).Length();
            float d1 = (new Vector2(p1.X, p1.Y) - center).Length();
            float d2 = (new Vector2(p2.X, p2.Y) - center).Length();
            float d3 = (new Vector2(p3.X, p3.Y) - center).Length();

            var q0 = (d0 + d2) / d2;
            var q1 = (d1 + d3) / d3;
            var q2 = (d2 + d0) / d0;
            var q3 = (d3 + d1) / d1;

            _vertices[_numSprites].textureCoordinate0.X = (_cornerOffsetX[0 ^ effects] * sourceW) + sourceX;
            _vertices[_numSprites].textureCoordinate0.Y = (_cornerOffsetY[0 ^ effects] * sourceH) + sourceY;
            _vertices[_numSprites].textureCoordinate0.Z = 1;
            _vertices[_numSprites].textureCoordinate0 *= float.IsNaN(q0) || q0 == 0 ? 1f : q0;

            _vertices[_numSprites].textureCoordinate1.X = (_cornerOffsetX[1 ^ effects] * sourceW) + sourceX;
            _vertices[_numSprites].textureCoordinate1.Y = (_cornerOffsetY[1 ^ effects] * sourceH) + sourceY;
            _vertices[_numSprites].textureCoordinate1.Z = 1;
            _vertices[_numSprites].textureCoordinate1 *= float.IsNaN(q1) || q1 == 0 ? 1f : q1;

            _vertices[_numSprites].textureCoordinate2.X = (_cornerOffsetX[2 ^ effects] * sourceW) + sourceX;
            _vertices[_numSprites].textureCoordinate2.Y = (_cornerOffsetY[2 ^ effects] * sourceH) + sourceY;
            _vertices[_numSprites].textureCoordinate2.Z = 1;
            _vertices[_numSprites].textureCoordinate2 *= float.IsNaN(q2) || q2 == 0 ? 1f : q2;

            _vertices[_numSprites].textureCoordinate3.X = (_cornerOffsetX[3 ^ effects] * sourceW) + sourceX;
            _vertices[_numSprites].textureCoordinate3.Y = (_cornerOffsetY[3 ^ effects] * sourceH) + sourceY;
            _vertices[_numSprites].textureCoordinate3.Z = 1;
            _vertices[_numSprites].textureCoordinate3 *= float.IsNaN(q3) || q3 == 0 ? 1f : q3;

            _vertices[_numSprites].position0.Z = depth;
            _vertices[_numSprites].position1.Z = depth;
            _vertices[_numSprites].position2.Z = depth;
            _vertices[_numSprites].position3.Z = depth;
            _vertices[_numSprites].color0 = color;
            _vertices[_numSprites].color1 = color;
            _vertices[_numSprites].color2 = color;
            _vertices[_numSprites].color3 = color;

            _textures[_numSprites] = texture;
            _numSprites++;
        }

        public void FlushBatch()
        {
            if (_numSprites == 0)
                return;

            var offset = 0;
            Texture2D curTexture = null;

            PrepRenderState();

            _vertexBuffer.SetData(0, _vertices, 0, _numSprites, VertexPositionColorTexture4.realStride, SetDataOptions.None);

            curTexture = _textures[0];
            for (var i = 1; i < _numSprites; i++)
            {
                if (_textures[i] != curTexture)
                {
                    DrawPrimitives(curTexture, offset, i - offset);
                    curTexture = _textures[i];
                    offset = i;
                }
            }
            DrawPrimitives(curTexture, offset, _numSprites - offset);

            _numSprites = 0;
        }

        void PrepRenderState()
        {
            graphicsDevice.BlendState = _blendState;
            graphicsDevice.DepthStencilState = _depthStencilState;
            graphicsDevice.RasterizerState = _rasterizerState;

            graphicsDevice.SetVertexBuffer(_vertexBuffer);
            graphicsDevice.Indices = _indexBuffer;

            // we have to Apply here because custom effects often wont have a vertex shader and we need the default SpriteEffect's
            _spriteEffectPass.Apply();
        }

        void DrawPrimitives(Texture2D texture, int baseSprite, int batchSize)
        {
            if (_currentEffect != null)
            {
                foreach (var pass in _currentEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    // Whatever happens in pass.Apply, make sure the texture being drawn ends up in Textures[0].
                    graphicsDevice.Textures[0] = texture;
                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, baseSprite * 4, 0, batchSize * 2);
                }
            }
            else
            {
                graphicsDevice.Textures[0] = texture;
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, baseSprite * 4, 0, batchSize * 2);
            }
        }

        static short[] GenerateIndexArray()
        {
            var result = new short[MAX_INDICES];
            for (int i = 0, j = 0; i < MAX_INDICES; i += 6, j += 4)
            {
                result[i] = (short)(j);
                result[i + 1] = (short)(j + 1);
                result[i + 2] = (short)(j + 2);
                result[i + 3] = (short)(j + 3);
                result[i + 4] = (short)(j + 2);
                result[i + 5] = (short)(j + 1);
            }
            return result;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        void CheckBegin()
        {
            if (!_beginCalled)
                throw new InvalidOperationException("Begin has not been called. Begin must be called before you can draw");
        }

        #region Sprite Data Container Struct

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VertexPositionColorTexture4 : IVertexType
        {
            public const int realStride = 112;

            VertexDeclaration IVertexType.VertexDeclaration { get { throw new NotImplementedException(); } }

            public Vector3 position0;
            public Color color0;
            public Vector3 textureCoordinate0;

            public Vector3 position1;
            public Color color1;
            public Vector3 textureCoordinate1;

            public Vector3 position2;
            public Color color2;
            public Vector3 textureCoordinate2;

            public Vector3 position3;
            public Color color3;
            public Vector3 textureCoordinate3;
        }

        #endregion
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionColorTexture : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 TextureCoordinate;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexPositionColorTexture(Vector3 position, Color color, Vector3 textureCoordinate)
        {
            Position = position;
            Color = color;
            TextureCoordinate = textureCoordinate;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Color.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return "{{Position:" + this.Position + " Color:" + this.Color + " TextureCoordinate:" + this.TextureCoordinate + "}}";
        }

        public static bool operator ==(VertexPositionColorTexture left, VertexPositionColorTexture right)
        {
            return (((left.Position == right.Position) && (left.Color == right.Color)) && (left.TextureCoordinate == right.TextureCoordinate));
        }

        public static bool operator !=(VertexPositionColorTexture left, VertexPositionColorTexture right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != base.GetType())
                return false;

            return (this == ((VertexPositionColorTexture)obj));
        }

        static VertexPositionColorTexture()
        {
            var elements = new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
            };
            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}
