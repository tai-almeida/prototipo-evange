#if UNITY_EDITOR
// Evangeline — configuração automática do tileset Minifantasy: Gloom Hollows.
// Menu: Evangeline > Gloom Hollows
//   1. Configurar tudo  -> ajusta importação, cria os Tiles, as paletas e a água animada
//   2. Criar Grid com camadas na cena aberta
//   3. Colocar mapa de exemplo (Premade) como referência
// Pode rodar quantas vezes quiser: ele atualiza o que já existe em vez de duplicar.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Evangeline.EditorTools
{
    public static class GloomHollowsSetup
    {
        const string AssetsRoot = "Assets/Sprites/Tile_Gloom_Hollows/Minifantasy_Gloom_Hollows_Assets";
        const string OutRoot = "Assets/Tiles/GloomHollows";
        const int PPU = 8;
        const int WaterFrames = 6;
        const float WaterFps = 1f / 0.3f; // 300 ms por frame (AnimationInfo.txt)
        const string MenuRoot = "Evangeline/Gloom Hollows/";

        // ------------------------------------------------------------------ 1
        [MenuItem(MenuRoot + "1. Configurar tudo (importação + tiles + paletas)", priority = 0)]
        public static void SetupAll()
        {
            if (!AssetDatabase.IsValidFolder(AssetsRoot))
            {
                EditorUtility.DisplayDialog("Gloom Hollows",
                    "Não achei a pasta:\n" + AssetsRoot + "\n\nSe você moveu o pacote, ajuste AssetsRoot no topo do script.", "OK");
                return;
            }

            int tileCount = 0, shadowCount = 0, waterCount = -1;
            try
            {
                EditorUtility.DisplayProgressBar("Gloom Hollows", "Ajustando importação das texturas...", 0.05f);
                int fixedTextures = FixImportSettings();
                Debug.Log($"[Gloom Hollows] Importação ajustada em {fixedTextures} textura(s).");

                EditorUtility.DisplayProgressBar("Gloom Hollows", "Criando tiles do Tileset...", 0.25f);
                var tiles = CreateTilesFromTexture(AssetsRoot + "/Tileset/Tileset.png", OutRoot + "/Tileset", Tile.ColliderType.Grid);
                tileCount = tiles.Count;

                EditorUtility.DisplayProgressBar("Gloom Hollows", "Montando paleta Gloom_Tileset...", 0.5f);
                BuildPalette("Gloom_Tileset", tiles);

                EditorUtility.DisplayProgressBar("Gloom Hollows", "Criando tiles de sombra...", 0.6f);
                var shadows = CreateTilesFromTexture(AssetsRoot + "/Tileset/Tileset_Shadows.png", OutRoot + "/Shadows", Tile.ColliderType.None);
                shadowCount = shadows.Count;
                BuildPalette("Gloom_Shadows", shadows);

                EditorUtility.DisplayProgressBar("Gloom Hollows", "Criando água animada...", 0.8f);
                waterCount = CreateWaterTiles();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string waterMsg = waterCount >= 0
                ? $"Água animada: {waterCount} tiles (paleta Gloom_Water)"
                : "Água animada: PULADA (falta o pacote 2D Tilemap Extras; depois de instalar, rode o menu de novo)";

            EditorUtility.DisplayDialog("Gloom Hollows pronto",
                $"Tileset: {tileCount} tiles (paleta Gloom_Tileset)\n" +
                $"Sombras: {shadowCount} tiles (paleta Gloom_Shadows)\n" +
                waterMsg + "\n\n" +
                "Abra Window > 2D > Tile Palette e escolha a paleta no dropdown.\n" +
                "Depois use 'Evangeline > Gloom Hollows > 2. Criar Grid' para montar as camadas na cena.", "Valeu");
        }

        // ------------------------------------------------------------------ 2
        [MenuItem(MenuRoot + "2. Criar Grid com camadas na cena aberta", priority = 1)]
        public static void CreateSceneGrid()
        {
            var gridGo = new GameObject("Gloom_Grid", typeof(Grid));
            gridGo.GetComponent<Grid>().cellSize = Vector3.one;
            Undo.RegisterCreatedObjectUndo(gridGo, "Criar Gloom Grid");

            // Ordem baseada nas camadas do Premade do artista (o-ground embaixo ... b-shadows em cima).
            // Espaçado de 10 em 10 para você encaixar o player (sugestão: Order 80).
            var layers = new (string name, int order, bool collider)[]
            {
                ("00_Ground",       0,  false),
                ("01_RuinsFloor",  10,  false),
                ("02_Water",       20,  false),
                ("03_Chasms",      30,  true),
                ("04_Walls",       40,  true),
                ("05_Stairs",      50,  false),
                ("06_Ruins",       60,  true),
                ("07_Decor",       70,  false),
                ("08_Stalactites", 90,  false),
                ("09_Shadows",    100,  false),
            };

            foreach (var l in layers)
            {
                var go = new GameObject(l.name, typeof(Tilemap), typeof(TilemapRenderer));
                go.transform.SetParent(gridGo.transform, false);
                go.GetComponent<TilemapRenderer>().sortingOrder = l.order;
                if (l.collider) go.AddComponent<TilemapCollider2D>();
            }

            Selection.activeGameObject = gridGo;
            EditorSceneManager.MarkSceneDirty(gridGo.scene);
            Debug.Log("[Gloom Hollows] Grid criado. Na janela Tile Palette, troque a 'Active Tilemap' para pintar em cada camada.");
        }

        // ------------------------------------------------------------------ 3
        [MenuItem(MenuRoot + "3. Colocar mapa de exemplo (Premade) como referência", priority = 2)]
        public static void PlacePremadeReference()
        {
            var sprite = AssetDatabase.LoadAllAssetsAtPath(AssetsRoot + "/Premade/Premade.png").OfType<Sprite>().FirstOrDefault();
            if (sprite == null)
            {
                EditorUtility.DisplayDialog("Gloom Hollows", "Não achei um sprite em Premade/Premade.png (ele precisa estar como Texture Type = Sprite).", "OK");
                return;
            }

            var go = new GameObject("Premade_Referencia (apague depois)");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(1f, 1f, 1f, 0.35f);
            sr.sortingOrder = -100;
            // Alinha o canto inferior esquerdo na origem para bater com a grade.
            go.transform.position = -sprite.bounds.min;
            Undo.RegisterCreatedObjectUndo(go, "Premade referência");
            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(go.scene);
        }

        // ================================================================== helpers

        static int FixImportSettings()
        {
            int changedCount = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { AssetsRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("_Use_Guidelines")) continue; // imagens de referência gigantes
                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp == null || imp.textureType != TextureImporterType.Sprite) continue;

                bool dirty = false;
                if (!Mathf.Approximately(imp.spritePixelsPerUnit, PPU)) { imp.spritePixelsPerUnit = PPU; dirty = true; }
                if (imp.filterMode != FilterMode.Point) { imp.filterMode = FilterMode.Point; dirty = true; }
                if (imp.textureCompression != TextureImporterCompression.Uncompressed) { imp.textureCompression = TextureImporterCompression.Uncompressed; dirty = true; }
                if (imp.mipmapEnabled) { imp.mipmapEnabled = false; dirty = true; }

                if (dirty)
                {
                    imp.SaveAndReimport();
                    changedCount++;
                }
            }
            return changedCount;
        }

        static Vector3Int CellOf(Rect r) =>
            new Vector3Int(Mathf.RoundToInt(r.x / PPU), Mathf.RoundToInt(r.y / PPU), 0);

        static Dictionary<Vector3Int, TileBase> CreateTilesFromTexture(string texPath, string outFolder, Tile.ColliderType collider)
        {
            var result = new Dictionary<Vector3Int, TileBase>();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(texPath).OfType<Sprite>().ToList();
            if (sprites.Count == 0)
            {
                Debug.LogWarning("[Gloom Hollows] Nenhum sprite encontrado em " + texPath + " (está fatiado como Multiple?)");
                return result;
            }

            EnsureFolder(outFolder);
            var created = new List<(Vector3Int cell, string path)>();

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var s in sprites)
                {
                    string p = $"{outFolder}/{s.name}.asset";
                    var tile = AssetDatabase.LoadAssetAtPath<Tile>(p);
                    bool isNew = tile == null;
                    if (isNew) tile = ScriptableObject.CreateInstance<Tile>();

                    tile.sprite = s;
                    tile.color = Color.white;
                    tile.colliderType = collider;

                    if (isNew) AssetDatabase.CreateAsset(tile, p);
                    else EditorUtility.SetDirty(tile);

                    created.Add((CellOf(s.rect), p));
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            foreach (var (cell, path) in created)
            {
                var t = AssetDatabase.LoadAssetAtPath<TileBase>(path);
                if (t != null) result[cell] = t;
            }
            return result;
        }

        static void BuildPalette(string name, Dictionary<Vector3Int, TileBase> tiles)
        {
            if (tiles.Count == 0) return;

            string folder = OutRoot + "/Palettes";
            EnsureFolder(folder);
            string path = $"{folder}/{name}.prefab";

            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            {
                var createdPalette = GridPaletteUtility.CreateNewPalette(
                    Path.GetFullPath(folder), name,
                    GridLayout.CellLayout.Rectangle,
                    GridPalette.CellSizing.Automatic,
                    Vector3.one,
                    GridLayout.CellSwizzle.XYZ);
                if (createdPalette != null) path = AssetDatabase.GetAssetPath(createdPalette);
            }

            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var tm = root.GetComponentInChildren<Tilemap>();
                if (tm == null)
                {
                    Debug.LogError("[Gloom Hollows] Paleta sem Tilemap: " + path);
                    return;
                }
                tm.ClearAllTiles();
                var positions = tiles.Keys.ToArray();
                var tileArray = positions.Select(p => tiles[p]).ToArray();
                tm.SetTiles(positions, tileArray);
                tm.CompressBounds();
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static Type FindAnimatedTileType()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType("UnityEngine.Tilemaps.AnimatedTile", false);
                if (t != null) return t;
            }
            return null;
        }

        // Retorna quantos tiles animados criou, ou -1 se o pacote não estiver instalado.
        static int CreateWaterTiles()
        {
            var type = FindAnimatedTileType();
            if (type == null)
            {
                EditorUtility.ClearProgressBar();
                if (EditorUtility.DisplayDialog("2D Tilemap Extras",
                        "A água animada precisa do pacote '2D Tilemap Extras'.\nQuer que eu instale agora?\n\n" +
                        "Depois que o Unity terminar de compilar, rode 'Configurar tudo' de novo.",
                        "Instalar", "Pular"))
                {
                    UnityEditor.PackageManager.Client.Add("com.unity.2d.tilemap.extras");
                    Debug.Log("[Gloom Hollows] Instalando com.unity.2d.tilemap.extras... rode o menu de novo depois.");
                }
                return -1;
            }

            const BindingFlags F = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var fSprites = type.GetField("m_AnimatedSprites", F);
            var fMin = type.GetField("m_MinSpeed", F);
            var fMax = type.GetField("m_MaxSpeed", F);
            var fCol = type.GetField("m_TileColliderType", F);
            if (fSprites == null || fMin == null || fMax == null)
            {
                Debug.LogError("[Gloom Hollows] Versão do AnimatedTile inesperada; não consegui configurar a água.");
                return -1;
            }

            string[] sheets = { "Only_Water", "Water_Lake", "Only_Water_Diagonals", "Water_Lake_Diagonals" };
            var palette = new Dictionary<Vector3Int, TileBase>();
            int yOffset = 0;
            int total = 0;

            foreach (var sheet in sheets)
            {
                string texPath = $"{AssetsRoot}/Tileset/Animated_Tiles/{sheet}.png";
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                var sprites = AssetDatabase.LoadAllAssetsAtPath(texPath).OfType<Sprite>().ToList();
                if (texture == null || sprites.Count == 0) continue;

                // As folhas têm 6 frames lado a lado na horizontal.
                int frameWidthTiles = texture.width / WaterFrames / PPU;
                int rowsTiles = texture.height / PPU;

                var byCell = new Dictionary<Vector3Int, Sprite>();
                foreach (var s in sprites) byCell[CellOf(s.rect)] = s;

                string outFolder = $"{OutRoot}/Water/{sheet}";
                EnsureFolder(outFolder);
                var created = new List<(Vector3Int cell, string path)>();

                AssetDatabase.StartAssetEditing();
                try
                {
                    for (int y = 0; y < rowsTiles; y++)
                    for (int x = 0; x < frameWidthTiles; x++)
                    {
                        var frames = new Sprite[WaterFrames];
                        bool complete = true;
                        for (int f = 0; f < WaterFrames; f++)
                        {
                            if (!byCell.TryGetValue(new Vector3Int(x + f * frameWidthTiles, y, 0), out var s)) { complete = false; break; }
                            frames[f] = s;
                        }
                        if (!complete) continue; // célula vazia (transparente) nessa folha

                        string p = $"{outFolder}/{sheet}_{x}_{y}.asset";
                        var tile = AssetDatabase.LoadAssetAtPath(p, type) as TileBase;
                        bool isNew = tile == null;
                        if (isNew) tile = (TileBase)ScriptableObject.CreateInstance(type);

                        fSprites.SetValue(tile, frames);
                        fMin.SetValue(tile, WaterFps);
                        fMax.SetValue(tile, WaterFps);
                        if (fCol != null) fCol.SetValue(tile, Tile.ColliderType.None);

                        if (isNew) AssetDatabase.CreateAsset(tile, p);
                        else EditorUtility.SetDirty(tile);

                        created.Add((new Vector3Int(x, y + yOffset, 0), p));
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }

                foreach (var (cell, path) in created)
                {
                    var t = AssetDatabase.LoadAssetAtPath<TileBase>(path);
                    if (t != null) { palette[cell] = t; total++; }
                }

                yOffset += rowsTiles + 2; // espaço entre as folhas na paleta
            }

            BuildPalette("Gloom_Water", palette);
            return total;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
