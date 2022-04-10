using Microsoft.Xna.Framework;
using MonoGame;
using OWL.Rendering;
using OWL.Util;
using System;
using System.Collections.Generic;

namespace OWL.Graph
{
    public abstract class Node
    {
        public Bounds Bounds { get; protected set; } = new Bounds();

        public Matrix2 WorldMatrix = Matrix2.Identity;
        public Matrix2 LocalMatrix = Matrix2.Identity;

        public Node parent = null;

        public List<Node> Children { get; private set; } = new List<Node>();
        public Rectangle? HitArea = null;

        public float Depth { get; set; } = 0f;

        public int DisplayID { get; protected set; } = -1;

        //setup getters / setters for easy coordinate access
        public float X { get { return _position.X; } set { _position.X = value; } }
        public float Y { get { return _position.Y; } set { _position.Y = value; } }

        protected Vector2 _position = new Vector2();
        public Vector2 Position
        {
            get => _position;
            set
            {
                _position = value;
            }
        }

        protected Vector2 _scale = new Vector2(1f, 1f);
        public Vector2 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
            }
        }

        protected float _rotation = 0f;
        public float Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                UpdateSkew();
            }
        }

        public float Alpha { get; set; } = 1f;
        public float WorldAlpha { get; protected set; } = 1f;

        public bool Visible { get; set; } = true;
        public bool Renderable { get; set; } = true;

        private Vector2 Skew { get; set; } = new Vector2();
        private float cx = 1f;  //cos rotation + skewY;
        private float sx = 0f;  //sin rotation + skewY;
        private float cy = 0f;  //cos rotation + Math.PI/2 - skewX;
        private float sy = 1f;  //sin rotation + Math.PI/2 - skewX;

        protected Point[] vertexData = new Point[4];

        public Point[] VertexData { get { return vertexData; } }

        public Guid InstanceID { get; private set; }

        protected bool precalculated = false;

        public Node()
        {
            InstanceID = Guid.NewGuid();
        }

        public void AddChild(Node child)
        {
            if (child.parent != null)
            {
                child.parent.RemoveChild(child);
                child.parent = this;
            }

            child.parent = this;
            Children.Add(child);
        }

        public void AddChildAt(Node child, int index)
        {
            if (child.parent != null)
            {
                child.parent.RemoveChild(child);
                child.parent = this;
            }

            child.parent = this;
            Children.Insert(index, child);
        }

        public void RemoveChild(Node child)
        {
            child.parent = null;
            Children.Remove(child);
        }

        public void RemoveAllChildren()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].parent = null;
                Children[i] = null;
            }
            Children.Clear();
        }

        public virtual void Draw(Renderer renderer) { }

        protected void PrecalculateTransforms()
        {
            LocalMatrix.M11 = cx * Scale.X;//a
            LocalMatrix.M12 = sx * Scale.X;//b
            LocalMatrix.M21 = cy * Scale.Y;//c
            LocalMatrix.M22 = sy * Scale.Y;//d

            LocalMatrix.M31 = Position.X;
            LocalMatrix.M32 = Position.Y;

            //calculate our world matrix by multiplying local matix by parents world
            WorldMatrix = LocalMatrix;
            //calculate world alpha by multplying this alpha by our parents alpha
            WorldAlpha = Alpha;

            foreach (Node child in Children)
            {
                child.PrecalculateTransforms(this);
            }
        }

        protected void PrecalculateTransforms(Node parentNode)
        {
            LocalMatrix.M11 = cx * Scale.X;//a
            LocalMatrix.M12 = sx * Scale.X;//b
            LocalMatrix.M21 = cy * Scale.Y;//c
            LocalMatrix.M22 = sy * Scale.Y;//d

            LocalMatrix.M31 = Position.X;
            LocalMatrix.M32 = Position.Y;

            //calculate our world matrix by multiplying local matix by parents world
            WorldMatrix = LocalMatrix * parentNode.WorldMatrix;
            //calculate world alpha by multplying this alpha by our parents alpha
            WorldAlpha = Alpha * parentNode.WorldAlpha;

            foreach (Node child in Children)
            {
                child.PrecalculateTransforms(this);
            }
        }

        public void UpdateTransforms(Renderer renderer, Node parentNode)
        {

            LocalMatrix.M11 = cx * Scale.X;//a
            LocalMatrix.M12 = sx * Scale.X;//b
            LocalMatrix.M21 = cy * Scale.Y;//c
            LocalMatrix.M22 = sy * Scale.Y;//d

            LocalMatrix.M31 = Position.X;
            LocalMatrix.M32 = Position.Y;

            WorldMatrix.M11 = (LocalMatrix.M11 * parentNode.WorldMatrix.M11) + (LocalMatrix.M12 * parentNode.WorldMatrix.M21);
            WorldMatrix.M12 = (LocalMatrix.M11 * parentNode.WorldMatrix.M12) + (LocalMatrix.M12 * parentNode.WorldMatrix.M22);
            WorldMatrix.M21 = (LocalMatrix.M21 * parentNode.WorldMatrix.M11) + (LocalMatrix.M22 * parentNode.WorldMatrix.M21);
            WorldMatrix.M22 = (LocalMatrix.M21 * parentNode.WorldMatrix.M12) + (LocalMatrix.M22 * parentNode.WorldMatrix.M22);

            WorldMatrix.M31 = (LocalMatrix.M31 * parentNode.WorldMatrix.M11) + (LocalMatrix.M32 * parentNode.WorldMatrix.M21) + parentNode.WorldMatrix.M31;
            WorldMatrix.M32 = (LocalMatrix.M31 * parentNode.WorldMatrix.M12) + (LocalMatrix.M32 * parentNode.WorldMatrix.M22) + parentNode.WorldMatrix.M32;

            //calculate world alpha by multplying this alpha by our parents alpha
            WorldAlpha = Alpha * parentNode.WorldAlpha;

            //draw then update children
            if (Renderable) Draw(renderer);

            DisplayID = renderer.DisplayID;

            foreach (Node child in Children)
            {
                //todo - this world alpha optimisation caused anything with zero alpha to never render again, it got stuck that way
                if (child.Visible /*&& child.WorldAlpha > 0*/)
                    child.UpdateTransforms(renderer, this);
            }
        }

        public void UpdateTransforms(Renderer renderer)
        {
            LocalMatrix.M11 = cx * Scale.X;//a
            LocalMatrix.M12 = sx * Scale.X;//b
            LocalMatrix.M21 = cy * Scale.Y;//c
            LocalMatrix.M22 = sy * Scale.Y;//d

            LocalMatrix.M31 = Position.X;
            LocalMatrix.M32 = Position.Y;

            WorldMatrix = LocalMatrix;

            //draw then update children
            if (Renderable) Draw(renderer);
            DisplayID = renderer.DisplayID;

            foreach (Node child in Children)
            {
                if (child.Visible)
                    child.UpdateTransforms(renderer, this);
            }
        }

        public void SetPosition(Vector2 position)
        {
            Position = position;
        }

        public void SetPosition(float x, float y)
        {
            Position = new Vector2(x, y);
        }

        public void SetPositionAndRotation(Vector2 position, float rotation)
        {
            SetPosition(position);
            Rotation = rotation;
        }

        public void SetPositionAndRotation(float x, float y, float rotation)
        {
            SetPosition(x, y);
            Rotation = rotation;
        }

        /// <summary>
        ///     Gets the bounding box for the display object
        /// </summary>
        /// <returns>The resulting <see cref="Rectangle" bounds />.</returns>
        public Rectangle GetBounds()
        {
            CalculateBounds();
            Rectangle boundingBox = Bounds.GetRectangle();
            return boundingBox;
        }

        /// <summary>
        ///     Calculates bounds
        /// </summary>
        public virtual void CalculateBounds() { }

        /// <summary>
        ///     Calculates vertices
        /// </summary>
        public virtual void CalculateVertices() { }

        /// <summary>
        ///     For cleanup of display objects
        /// </summary>
        public virtual void Destroy()
        {
            if (parent != null)
                parent.RemoveChild(this);
        }

        /// <summary>
        ///     Updates the skew values on rotation
        /// </summary>
        private void UpdateSkew()
        {
            cx = (float)Math.Cos(Rotation + Skew.Y);
            sx = (float)Math.Sin(Rotation + Skew.Y);
            cy = (float)-Math.Sin(Rotation - Skew.X);
            sy = (float)Math.Cos(Rotation - Skew.X);
        }

        /// <summary>
        ///     Transforms the specified <see cref="Vector2" /> from world space to local space of the display node
        /// </summary>
        /// <param name="point">The vector to be transformed to this nodes local space.</param>
        /// <returns>The resulting <see cref="Vector2" />.</returns>
        public Vector2 toLocal(Vector2 point)
        {
            return Matrix2.Invert(LocalMatrix).Transform(point);
        }
    }
}