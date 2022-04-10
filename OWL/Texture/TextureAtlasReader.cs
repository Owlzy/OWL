using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace OWL.Texture
{
    public class TextureAtlasReader : ContentTypeReader<TextureAtlas>
    {
        protected override TextureAtlas Read(ContentReader reader, TextureAtlas existingInstance)
        {
            string assetName = GetRelativeAssetName(reader, reader.ReadString());
            var texture = reader.ContentManager.Load<Texture2D>(assetName);
            var atlas = new TextureAtlas(assetName, texture);

            int regionCount = reader.ReadInt32();

            for (int i = 0; i < regionCount; ++i)
            {               
                var filename = reader.ReadString();
                var frame = new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                var sourceRect = new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                var size = new Vector2(reader.ReadInt32(), reader.ReadInt32());
                var isRotated = reader.ReadBoolean();
                var isTrimmed = reader.ReadBoolean();

                atlas.Add(filename, new TextureRegion2D(texture, frame, sourceRect, size, isRotated, isTrimmed));               
            }            
            return atlas;
        }

        protected static string GetRelativeAssetName(ContentReader reader, string relativeName)
        {
            string assetDirectory = Path.GetDirectoryName(reader.AssetName);
            return Path.Combine(assetDirectory, relativeName).Replace('\\', '/');
        }
    }
}
