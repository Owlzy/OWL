using Microsoft.Xna.Framework;

namespace OWL.Graph
{
    public class Container : Node
    {
        public float Width
        {
            get
            {
                return (int)(_scale.X * GetBounds().Width);
            }
            set
            {
                int width = GetBounds().Width;
                if (width != 0)
                {
                    _scale.X = value / width;

                }
                else
                {
                    _scale.X = 1;
                }
            }
        }

        public float Height
        {
            get
            {
                return (int)(_scale.Y * GetBounds().Height);
            }
            set
            {
                int height = GetBounds().Height;

                if (height != 0)
                {
                    _scale.Y = value / height;
                }
                else
                {
                    _scale.Y = 1;
                }
            }
        }

        public Container() : base() { }

        public void SetScale(Vector2 scale)
        {
            Scale = scale;
        }

        public void SetScale(float x, float y)
        {
            _scale.X = x;
            _scale.Y = y;
        }

        public void SetScale(float x)
        {
            _scale.X = x;
            _scale.Y = x;
        }

        public override void CalculateBounds()
        {
            if (!precalculated)
            {
                precalculated = true;
                PrecalculateTransforms();
            }

            Bounds.Clear();
            CalculateVertices();

            foreach (Node child in Children)
            {
                if (!child.Visible || !child.Renderable) continue;

                child.CalculateBounds();

                Bounds += child.Bounds;
            }
        }

        public override void CalculateVertices() { }
    }
}
