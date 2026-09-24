#if UNITY_EDITOR
// Evangeline — cria os tiles automáticos (Chão, Parede, Buraco, Rocha) do Gloom Hollows.
// Menu: Evangeline > Gloom Hollows
//   4. Criar Auto Tiles       -> GloomAuto_Sprites + 4 tiles + paleta Gloom_Auto
//   5. Criar sala automática  -> Tilemap "Sala_Auto" na cena com uma sala pronta pra editar
// Depende do "1. Configurar tudo" já ter ajustado a importação do Tileset.png (PPU 8, fatiado 8x8).

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Evangeline.Level;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Evangeline.EditorTools
{
    public static class GloomAutoTileSetup
    {
        const string TilesetPath = "Assets/Sprites/Tile_Gloom_Hollows/Minifantasy_Gloom_Hollows_Assets/Tileset/Tileset.png";
        const string OutFolder = "Assets/Tiles/GloomHollows/Auto";
        const string LipsFolder = OutFolder + "/Lips";
        const string PaletteFolder = "Assets/Tiles/GloomHollows/Palettes";
        const string PaletteName = "Gloom_Auto";
        const int PPU = 8;
        const string MenuRoot = "Evangeline/Gloom Hollows/";

        // Sala de exemplo mostrada na paleta: # Parede, . Chão, o Buraco, r Rocha.
        static readonly string[] PaletteDemo =
        {
            "###############",
            "###############",
            "###############",
            "#.............#",
            "#..rrr........#",
            "#..rrr...ooo..#",
            "#..rrr...ooo..#",
            "#........ooo..#",
            "#........ooo..#",
            "#.............#",
            "###############",
        };

        // ------------------------------------------------------------------ 4
        [MenuItem(MenuRoot + "4. Criar Auto Tiles (Chão, Parede, Buraco, Rocha)", priority = 20)]
        public static void CreateAutoTiles()
        {
            var cells = LoadTilesetCells();
            if (cells == null) return;

            EnsureFolder(OutFolder);
            FixLipImports();

            var set = LoadOrCreate<GloomAutoSprites>(OutFolder + "/GloomAuto_Sprites.asset");
            var missing = new List<string>();
            Sprite S(int col, int row)
            {
                // (col, row) contados a partir do canto superior esquerdo do Tileset.png, em tiles de 8px.
                var key = new Vector2Int(col, 104 - row);
                if (cells.TryGetValue(key, out var sp)) return sp;
                missing.Add($"({col},{row})");
                return null;
            }
            Sprite[] Grid3(int[] cols, int[] rows, int rowOffset) =>
                rows.SelectMany(r => cols.Select(c => S(c, r + rowOffset))).ToArray();

            int[] varRows = { 58, 61, 64 };

            set.black = S(46, 80);

            set.floor = new[]
            {
                S(82, 32), S(82, 32), S(82, 32), S(82, 32), S(82, 32), S(82, 32),
                S(76, 32), S(78, 32), S(80, 32), S(84, 32),
                S(76, 34), S(78, 34), S(80, 34), S(82, 34),
            };
            set.floorLip = LoadLips("C");
            set.floorLipLeft = LoadLips("L");
            set.floorLipRight = LoadLips("R");

            set.faceUpper = new[] { S(17, 42), S(18, 42), S(20, 42), S(21, 42) }.Concat(Grid3(new[] { 17, 19, 21 }, varRows, 0)).ToArray();
            set.faceLower = new[] { S(17, 43), S(18, 43), S(20, 43), S(21, 43) }.Concat(Grid3(new[] { 17, 19, 21 }, varRows, 1)).ToArray();
            set.faceUpperLeft = new[] { S(26, 34) }.Concat(Grid3(new[] { 10, 12, 14 }, varRows, 0)).ToArray();
            set.faceLowerLeft = new[] { S(26, 35) }.Concat(Grid3(new[] { 10, 12, 14 }, varRows, 1)).ToArray();
            set.faceUpperRight = new[] { S(28, 34) }.Concat(Grid3(new[] { 24, 26, 28 }, varRows, 0)).ToArray();
            set.faceLowerRight = new[] { S(28, 35) }.Concat(Grid3(new[] { 24, 26, 28 }, varRows, 1)).ToArray();

            set.rimN = S(26, 41);
            set.rimS = new[] { S(17, 41), S(18, 41), S(20, 41), S(21, 41) };
            set.rimW = S(29, 38);
            set.rimE = S(25, 38);
            set.rimNW = S(26, 31);
            set.rimNE = S(28, 31);
            set.rimSW = S(26, 33);
            set.rimSE = S(28, 33);
            set.rimDiagSE = S(25, 37);
            set.rimDiagSW = S(29, 37);
            set.rimDiagNE = S(25, 41);
            set.rimDiagNW = S(29, 41);

            set.chasmUpper = Grid3(new[] { 17, 19, 21 }, new[] { 72, 75, 78 }, 0);
            set.chasmLower = Grid3(new[] { 17, 19, 21 }, new[] { 72, 75, 78 }, 1);

            set.rockTopSingle = S(5, 3);
            set.rockFaceUpperSingle = S(5, 4);
            set.rockFaceLowerSingle = S(5, 5);
            set.rockTopLeft = S(18, 3);
            set.rockTop = S(19, 3);
            set.rockTopRight = S(20, 3);
            set.rockFaceUpperLeft = S(18, 4);
            set.rockFaceUpperRight = S(20, 4);
            set.rockFaceLowerLeft = S(18, 5);
            set.rockFaceLowerRight = S(20, 5);

            EditorUtility.SetDirty(set);

            var floor = MakeTile("Gloom_Chao", GloomAutoTile.Kind.Floor, set);
            var wall = MakeTile("Gloom_Parede", GloomAutoTile.Kind.Wall, set);
            var pit = MakeTile("Gloom_Buraco", GloomAutoTile.Kind.Pit, set);
            var rock = MakeTile("Gloom_Rocha", GloomAutoTile.Kind.Rock, set);
            AssetDatabase.SaveAssets();

            BuildPalette(new Dictionary<char, TileBase> { { '#', wall }, { '.', floor }, { 'o', pit }, { 'r', rock } });
            AssetDatabase.SaveAssets();

            if (missing.Count > 0)
                Debug.LogWarning("[Gloom Auto] Sprites não encontrados no Tileset (célula vazia ou fatiamento diferente): " + string.Join(", ", missing));
            Debug.Log("[Gloom Auto] Pronto: tiles em " + OutFolder + " e paleta " + PaletteName + ".");
        }

        // ------------------------------------------------------------------ 5
        [MenuItem(MenuRoot + "5. Criar sala automática na cena", priority = 21)]
        public static void CreateRoom()
        {
            var wall = AssetDatabase.LoadAssetAtPath<GloomAutoTile>(OutFolder + "/Gloom_Parede.asset");
            var floor = AssetDatabase.LoadAssetAtPath<GloomAutoTile>(OutFolder + "/Gloom_Chao.asset");
            if (wall == null || floor == null)
            {
                EditorUtility.DisplayDialog("Gloom Auto", "Rode primeiro '4. Criar Auto Tiles'.", "OK");
                return;
            }

            var grid = Object.FindAnyObjectByType<Grid>();
            if (grid == null)
            {
                grid = new GameObject("Gloom_Grid", typeof(Grid)).GetComponent<Grid>();
                Undo.RegisterCreatedObjectUndo(grid.gameObject, "Criar Grid");
            }

            var go = new GameObject("Sala_Auto", typeof(Tilemap), typeof(TilemapRenderer));
            Undo.RegisterCreatedObjectUndo(go, "Criar sala automática");
            go.transform.SetParent(grid.transform, false);
            // Nasce onde a Scene View está olhando, pra não cair em cima do que já foi pintado.
            var view = SceneView.lastActiveSceneView;
            if (view != null)
            {
                var center = grid.transform.InverseTransformPoint(view.pivot);
                go.transform.localPosition = new Vector3(Mathf.Round(center.x) - 8, Mathf.Round(center.y) - 5, 0);
            }

            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
            var tileCollider = go.AddComponent<TilemapCollider2D>();
            tileCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            go.AddComponent<CompositeCollider2D>();

            // Sala 16x10 de chão, com Parede em volta (3 em cima por causa da face, 2 nos outros lados).
            const int w = 16, h = 10;
            var tm = go.GetComponent<Tilemap>();
            for (int y = -2; y < h + 3; y++)
            for (int x = -2; x < w + 2; x++)
            {
                bool inside = x >= 0 && x < w && y >= 0 && y < h;
                tm.SetTile(new Vector3Int(x, y, 0), inside ? floor : wall);
            }

            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(go.scene);
            Debug.Log("[Gloom Auto] Sala criada. Abra Window > 2D > Tile Palette, escolha a paleta Gloom_Auto e pinte em Sala_Auto.");
        }

        // ================================================================== helpers

        static Dictionary<Vector2Int, Sprite> LoadTilesetCells()
        {
            var sprites = AssetDatabase.LoadAllAssetsAtPath(TilesetPath).OfType<Sprite>().ToList();
            if (sprites.Count == 0)
            {
                EditorUtility.DisplayDialog("Gloom Auto", "Não achei sprites em\n" + TilesetPath + "\n\nRode '1. Configurar tudo' antes.", "OK");
                return null;
            }
            var cells = new Dictionary<Vector2Int, Sprite>();
            foreach (var s in sprites)
            {
                if (Mathf.RoundToInt(s.rect.width) != PPU || Mathf.RoundToInt(s.rect.height) != PPU) continue;
                cells[new Vector2Int(Mathf.RoundToInt(s.rect.x / PPU), Mathf.RoundToInt(s.rect.y / PPU))] = s;
            }
            return cells;
        }

        static void FixLipImports()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { LipsFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var imp = (TextureImporter)AssetImporter.GetAtPath(path);
                if (imp.textureType == TextureImporterType.Sprite && imp.spriteImportMode == SpriteImportMode.Single &&
                    Mathf.Approximately(imp.spritePixelsPerUnit, PPU) && imp.filterMode == FilterMode.Point &&
                    imp.textureCompression == TextureImporterCompression.Uncompressed && !imp.mipmapEnabled)
                    continue;

                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Single;
                imp.spritePixelsPerUnit = PPU;
                imp.filterMode = FilterMode.Point;
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                imp.mipmapEnabled = false;
                imp.alphaIsTransparency = true;
                imp.SaveAndReimport();
            }
        }

        static Sprite[] LoadLips(string kind) =>
            Enumerable.Range(0, 4)
                .Select(i => AssetDatabase.LoadAssetAtPath<Sprite>($"{LipsFolder}/FloorLip_{kind}_{i}.png"))
                .Where(s => s != null)
                .ToArray();

        static GloomAutoTile MakeTile(string name, GloomAutoTile.Kind kind, GloomAutoSprites set)
        {
            var tile = LoadOrCreate<GloomAutoTile>($"{OutFolder}/{name}.asset");
            tile.kind = kind;
            tile.sprites = set;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        static void BuildPalette(Dictionary<char, TileBase> legend)
        {
            EnsureFolder(PaletteFolder);
            string path = $"{PaletteFolder}/{PaletteName}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            {
                var created = GridPaletteUtility.CreateNewPalette(
                    Path.GetFullPath(PaletteFolder), PaletteName,
                    GridLayout.CellLayout.Rectangle, GridPalette.CellSizing.Automatic,
                    Vector3.one, GridLayout.CellSwizzle.XYZ);
                if (created != null) path = AssetDatabase.GetAssetPath(created);
            }

            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var tm = root.GetComponentInChildren<Tilemap>();
                tm.ClearAllTiles();
                int rows = PaletteDemo.Length;
                for (int r = 0; r < rows; r++)
                for (int c = 0; c < PaletteDemo[r].Length; c++)
                    if (legend.TryGetValue(PaletteDemo[r][c], out var t))
                        tm.SetTile(new Vector3Int(c, rows - 1 - r, 0), t);
                tm.CompressBounds();
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
#endif
