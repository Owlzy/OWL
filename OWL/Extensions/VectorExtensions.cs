using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework
{

    public static class VectorExtensions
    {
        /// <summary>
        /// Turns this <see cref="Vector3"/> to a unit vector with the same direction safely without returning NaN.  In case of zero distance, vector will be left untouched.
        /// </summary>
        public static Vector3 NormalizeSafe(this Vector3 vector)
        {
            float factor = (float)Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y) + (vector.Z * vector.Z));
            if (factor != 0)
            {
                vector.X /= factor;
                vector.Y /= factor;
                vector.Z /= factor;
            }
            /*
            float factor = (float)Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y) + (vector.Z * vector.Z));
            if (factor != 0) factor = 1f / factor;
            vector.X *= factor;
            vector.Y *= factor;
            vector.Z *= factor;
            return vector;
             */
            return vector;
        }

        /// <summary>
        /// Turns this <see cref="Vector2"/> to a unit vector with the same direction safely without returning NaN.  In case of zero distance, vector will be left untouched.
        /// </summary>
        public static Vector2 NormalizeSafe(this Vector2 vector)
        {
            float factor = (float)Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y));
            if (factor != 0)
            {
                vector.X /= factor;
                vector.Y /= factor;
            }
            return vector;
        }

        /// <summary>
        /// Returns the angle between this <see cref="Vector2"/> and the given vector.
        /// </summary> 
        public static float AngleBetween(this Vector2 vectorA, Vector2 vectorB)
        {
            return (float)Math.Atan2(vectorB.Y - vectorA.Y, vectorB.X - vectorA.X);
        }

        /// <summary>
        /// Returns the left hand normal of this <see cref="Vector2"/>.
        /// </summary>
        public static Vector2 LeftHandNormal(this Vector2 vector)
        {
            var temp = vector.X;
            vector.X = vector.Y;
            vector.Y = -temp;
            return vector;
        }
    }
}
