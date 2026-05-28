using System;
using System.Collections.Generic;
using System.Linq;
using breaking_worse.Input.Enums;
using breaking_worse.Objects.Collisions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TiledCS;

namespace breaking_worse.Objects.Scene;

public class Map(GameManager gameManager)
{
    private TiledMap _map;
    private Dictionary<int, TiledTileset> _tileSets;
    private Dictionary<string, Texture2D> _tileSetTextures;
    private  IEnumerable<TiledLayer> tileLayers;
    private bool[,] _collisionMatrix;
    private bool[,] _collisionMatrixWithoutFences;
    private bool _collisionMatrixAlreadyCalculated;
    // maps cell id to list of collision objects to find tile collisions more efficiently
    public readonly Dictionary<int, List<TiledObject>> Grid = new();
 
    public int MapHeight => _map.Height;
    public int MapWidth => _map.Width;

    public Vector2 PathStart {get; set;} = Vector2.Zero;

    public Vector2 PathEnd {get; set;} = Vector2.One;


    public TiledLayer CollisionLayer
    {
        get
        {
            return _map.Layers.First(layer => layer.name == "Collision");
        }
    }
    
    public void loadContent()
    {
        _map = new TiledMap(gameManager.Content.RootDirectory + "/map/Map.tmx");
        tileLayers = _map.Layers.ToList().Where(x => x.type == TiledLayerType.TileLayer);
        _tileSets = _map.GetTiledTilesets(gameManager.Content.RootDirectory + "/map/");
        _tileSetTextures = new Dictionary<string, Texture2D> { { "base", gameManager.AssetManager.Images["base"] } };
        _tileSetTextures.Add("Road", gameManager.AssetManager.Images["Road"]);
        _tileSetTextures.Add("Grass", gameManager.AssetManager.Images["Grass"]);
        _tileSetTextures.Add("Building", gameManager.AssetManager.Images["Building"]);
        _tileSetTextures.Add("PoliceStation", gameManager.AssetManager.Images["PoliceStation"]);
        _tileSetTextures.Add("Building Options", gameManager.AssetManager.Images["Building Options"]);
        _tileSetTextures.Add("Building Options Big", gameManager.AssetManager.Images["Building Options Big"]);
        _tileSetTextures.Add("Buildings", gameManager.AssetManager.Images["Buildings"]);
        _tileSetTextures.Add("FireStation", gameManager.AssetManager.Images["FireStation"]);
        _tileSetTextures.Add("tree", gameManager.AssetManager.Images["tree"]);
        _tileSetTextures.Add("Shops", gameManager.AssetManager.Images["Shops"]);
        _tileSetTextures.Add("Shop", gameManager.AssetManager.Images["Shop"]);
        _tileSetTextures.Add("Slum", gameManager.AssetManager.Images["Slum"]);
        _tileSetTextures.Add("Broken House", gameManager.AssetManager.Images["Broken House"]);
        _tileSetTextures.Add("Urban", gameManager.AssetManager.Images["Urban"]);
        _tileSetTextures.Add("base2", gameManager.AssetManager.Images["base2"]);
        _tileSetTextures.Add("base3", gameManager.AssetManager.Images["base3"]);
        _tileSetTextures.Add("Chimney", gameManager.AssetManager.Images["Chimney"]);
        _tileSetTextures.Add("Interior", gameManager.AssetManager.Images["Interior"]);
        _tileSetTextures.Add("shop-and-hospital", gameManager.AssetManager.Images["shop-and-hospital"]);
        _tileSetTextures.Add("TilesHospital", gameManager.AssetManager.Images["TilesHospital"]);
        _tileSetTextures.Add("DoorsHospital", gameManager.AssetManager.Images["DoorsHospital"]);
        fillGridWithTiledObjects();
    }

    public void render(GameTime gameTime, bool renderBaseMap = false, bool renderMiniMap = false)
    {
        //iterates over every layer of map and draws tiles one by one
        int layerCounter = 0;
        foreach (var layer in tileLayers)
        {
            layerCounter++;
            if (renderBaseMap && layerCounter > 7) break; // if "renderBaseMap" only render the first couple of layers
            if (!renderBaseMap && !renderMiniMap && layerCounter <= 7) continue; // else: don't render first the couple of layers, only last
            if (!renderBaseMap && !renderMiniMap && layerCounter > 8) break;
            if (!renderBaseMap &&  renderMiniMap && layerCounter <= 8) continue;
            
            for (var y = 0; y < layer.height; y++)
            {
                float layerDepth = renderBaseMap ? layerCounter*0.01f + y / 100000f : 1;
                for (var x = 0; x < layer.width; x++)
                {
                    var index = y * layer.width + x;
                    var gid = layer.data[index];
                    var tileX = x * _map.TileWidth;
                    var tileY = y * _map.TileHeight;

                    if (gid == 0)
                    {
                        continue;
                    }

                    var mapTileSet = _map.GetTiledMapTileset(gid);
                    var tileSet = _tileSets[mapTileSet.firstgid];

                    var rect = _map.GetSourceRect(mapTileSet, tileSet, gid);
                    var source = new Rectangle(rect.x, rect.y, rect.width, rect.height);
                    var destination = new Rectangle(tileX, tileY, _map.TileWidth, _map.TileHeight);
                    string sourceString = tileSet.Image.source;

                    if (sourceString.Contains("../Images/Map/"))
                    {
                        int lengthSourceString = sourceString.Length;
                        sourceString = sourceString.Substring(14, lengthSourceString-14);
                    }
                    
                    int indexOfDot = sourceString.IndexOf('.');

                    if (indexOfDot != -1)
                    {
                        sourceString = sourceString.Substring(0, indexOfDot);
                    }

                    // check for tile transitions (if the tile is flipped or rotated)
                    Transition tileTrans = Transition.None;
                    if (_map.IsTileFlippedHorizontal(layer, x, y)) tileTrans |= Transition.Flip_H;
                    if (_map.IsTileFlippedVertical(layer, x, y)) tileTrans |= Transition.Flip_V;
                    if (_map.IsTileFlippedDiagonal(layer, x, y)) tileTrans |= Transition.Flip_D;

                    SpriteEffects effects = SpriteEffects.None;
                    double rotation = 0f;
                    switch (tileTrans)
                    {
                        case Transition.Flip_H: effects = SpriteEffects.FlipHorizontally; break;
                        case Transition.Flip_V: effects = SpriteEffects.FlipVertically; break;

                        case Transition.Rotate_90:
                            rotation = Math.PI * .5f;
                            destination.X += _map.TileWidth;
                            break;

                        case Transition.Rotate_180:
                            rotation = Math.PI;
                            destination.X += _map.TileWidth;
                            destination.Y += _map.TileHeight;
                            break;

                        case Transition.Rotate_270:
                            rotation = Math.PI * 3 / 2;
                            destination.Y += _map.TileHeight;
                            break;

                        case Transition.Rotate_90AndFlip_H:
                            effects = SpriteEffects.FlipHorizontally;
                            rotation = Math.PI * .5f;
                            destination.X += _map.TileWidth;
                            break;
                    }

                    // draw the map to the map render target in full resolution
                    gameManager.SpriteBatch.Draw(_tileSetTextures[sourceString], destination, source, Color.White,
                        (float)rotation, Vector2.Zero, effects, layerDepth);

                }
            }
        }
        


    }

    // now necessary to draw collisions because map gets rendered only once
    public void draw()
    {
        if (gameManager.InputHandler.isPressed(gameManager.SettingsManager.DrawTileCollisionRects, PressType.HoldWithoutRelease))
            paintTileCollisions();
        
        if (gameManager.InputHandler.isPressed(gameManager.SettingsManager.DrawHitBoxKey, PressType.HoldWithoutRelease))
            paintCollisions();
    }

    //while f9 is pressed, collision rectangles will be drawn onto the screen
    private void paintCollisions()
    {
        Texture2D collisions = new Texture2D(gameManager.GraphicsDeviceManager.GraphicsDevice, 1, 1);
        collisions.SetData(new Color[] { Color.White });
        TiledLayer collisionLayer = _map.Layers.First(layer => layer.name == "Collision");
        {
            foreach (var obj in collisionLayer.objects)
            {
                Rectangle collisionRect = new Rectangle((int)obj.x, (int)obj.y, (int)obj.width, (int)obj.height);
                gameManager.SpriteBatch.Draw(collisions, collisionRect, 0.5f * Color.Red);
            }
        }
    }

    // gives array of bool, true if there's a collision on the sprite at the position
    // WithoutFence flag when true return the collision matrix ignoring fences and things that can be shot through
    public bool[,] getCollisionMatrix(bool withoutFence = false)
    {
        if (!_collisionMatrixAlreadyCalculated)
        {
            calculateCollisionMatrix();
            calculateCollisionMatrix(true);
            _collisionMatrixAlreadyCalculated = true;
        }
        return withoutFence ? ref _collisionMatrixWithoutFences : ref _collisionMatrix;
    }
        
    private void calculateCollisionMatrix(bool withoutFence = false) 
    {
        String layerName = withoutFence ? "CollisionWithoutFence" : "Collision";
        TiledLayer collisionLayer = _map.Layers.First(layer => layer.name == layerName);

        bool[,] cols = new bool[_map.Width, _map.Height];
        foreach (var obj in collisionLayer.objects)
        {
            for (var x = Math.Max((int)(obj.x / 32), 0);
                 x <= (int)((obj.x + obj.width) / 32) && x < _map.Width;
                 x++)
            {
                for (var y = Math.Max((int)(obj.y / 32), 0);
                     y <= (int)((obj.y + obj.height) / 32) && y < _map.Height;
                     y++)
                {
                    cols[x, y] = true;
                }
            }
        }
        if (withoutFence)_collisionMatrixWithoutFences = cols;
        else _collisionMatrix = cols;
    }

    // to try out if "getCollisionMatrix()" works, paints the full tiles containing collisions light blue to debug
    private void paintTileCollisions()
    {
        bool[,] collisionMatrix = getCollisionMatrix();
        
        Texture2D collisions = new Texture2D(gameManager.GraphicsDeviceManager.GraphicsDevice, 1, 1);
        collisions.SetData(new Color[] { Color.White });
        
        for (int x = 0; x < _map.Width; x++)
        {
            for (int y = 0; y < _map.Height; y++)
            {
                if (collisionMatrix[x, y])
                {
                    Rectangle collisionRect = new Rectangle(x*32, y*32, 32, 32);
                    gameManager.SpriteBatch.Draw(collisions, collisionRect, 0.4f*Color.Blue);
                }
            }
        }
    }
    
    private void fillGridWithTiledObjects()
    // fills the Grid with the tiled collision objects that intersect the respective tiles
    {
        for (int y = 0; y < _map.Height; y++) 
        {
            for (int x = 0; x < _map.Width; x++)
            {
                if (!Grid.TryGetValue(y*_map.Width + x, out var tiledObjects))
                {
                    tiledObjects = [];
                }
                foreach (var collisionFrame in CollisionLayer.objects)
                {
                    var tileHitBox = new HitBox(new Vector2(x * 32, y * 32), 32, 32);
                    var collisionFrameHitBox = new HitBox(new Vector2(collisionFrame.x, collisionFrame.y), (int)collisionFrame.width, (int)collisionFrame.height);
                    if (!tileHitBox.intersects(collisionFrameHitBox)) continue;
                    tiledObjects.Add(collisionFrame);
                }
                Grid[y * _map.Width + x] = tiledObjects;
            } 
        }
    }
    
    
    // returns a list of all positions of tiles with the given id
    public List<System.Numerics.Vector2> getTilePositions(List<int> tileId)
    {
        List<System.Numerics.Vector2> positions = new List<System.Numerics.Vector2>();

        if (_map?.Layers == null)
        {
            return positions;
        }
        
        foreach (var layer in tileLayers)
        {
            if (layer.data == null)
            {
                continue;
            }
            for (int y = 0; y < layer.height; y++)
            {
                for (int x = 0; x < layer.width; x++)
                {
                    int index = y * layer.width + x;
                    int gid = layer.data[index];

                    foreach (var id in tileId)
                    {
                        if (gid == id)
                        {
                            positions.Add(new System.Numerics.Vector2(x * _map.TileWidth, y * _map.TileHeight));
                        }   
                    }
                }
            }
        }

        return positions;
    }
}
