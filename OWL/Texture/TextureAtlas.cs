using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace OWL.Texture
{
    public class TextureAtlas : IEnumerable<KeyValuePair<string, TextureRegion2D>>, IDisposable
    {
        public static TextureAtlas TextureCache { get; protected set; } = new TextureAtlas(null, null);

        private readonly IDictionary<string, TextureRegion2D> regionList = new Dictionary<string, TextureRegion2D>();

        public string Name { get; protected set; }
        public Texture2D Texture { get; protected set; }

        public TextureAtlas(string name, Texture2D texture)
        {
            Name = name;
            Texture = texture;
        }

        public void Add(string name, TextureRegion2D region)
        {
            if (regionList.ContainsKey(name))
                throw new InvalidOperationException($"Region {name} already has an entry in texture atlas");

            regionList.Add(name, region);
        }

        public void Add(string name, int x, int y, int width, int height)
        {
            TextureRegion2D region = new TextureRegion2D(Texture, x, y, width, height);

            regionList.Add(name, region);
        }

        public void Add(TextureAtlas atlas)
        {
            foreach (KeyValuePair<string, TextureRegion2D> entry in atlas.regionList)
                Add(entry.Key, entry.Value);
        }

        public void Remove(string name)
        {
            regionList.Remove(name);
        }

        public void Remove(TextureAtlas atlas)
        {
            foreach (KeyValuePair<string, TextureRegion2D> entry in atlas.regionList)
                Remove(entry.Key);
        }

        public TextureRegion2D GetRegion(string name)
        {
            return regionList[name];
        }

        public bool Contains(string name)
        {
            return regionList.ContainsKey(name);
        }

        public int RegionCount => regionList.Count;

        public TextureRegion2D this[string name] => GetRegion(name);

        public IEnumerator<KeyValuePair<string, TextureRegion2D>> GetEnumerator()
        {
            return regionList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Texture.Dispose();
                    Texture = null;
                    // TextureCache.Dispose();
                    //  TextureCache = null;
                    //Name = null;
                    //regionList.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TextureAtlas() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
