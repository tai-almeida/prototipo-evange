// Evangeline — tile automático para o tileset Gloom Hollows (visão 3/4).
//
// Pinte tudo numa Tilemap só com 4 tiles: Chão, Parede, Buraco e Rocha.
// O tile olha os vizinhos e escolhe o sprite certo sozinho:
//   Parede  -> vazio preto com contorno; acima do chão vira a face de 2 tiles de altura.
//   Buraco  -> vazio com contorno; logo abaixo da borda de cima aparece o abismo.
//   Rocha   -> bloco de pedra: faces nas 2 linhas de baixo, topo de pedra no resto.
//   Chão    -> variações aleatórias (estáveis pela posição) + sombra na base das faces.
// Células vazias contam como Parede, então é só cercar a sala com Parede.

using UnityEngine;
using UnityEngine.Tilemaps;

namespace Evangeline.Level
{
    [CreateAssetMenu(menuName = "Evangeline/Gloom Auto Tile")]
    public class GloomAutoTile : TileBase
    {
        public enum Kind { Floor, Wall, Pit, Rock }

        public Kind kind;
        public GloomAutoSprites sprites;

        static readonly Vector3Int Up = Vector3Int.up, Down = Vector3Int.down, Left = Vector3Int.left, Right = Vector3Int.right;

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = sprites != null ? Resolve(position, tilemap) : null;
            tileData.color = Color.white;
            tileData.transform = Matrix4x4.identity;
            tileData.flags = TileFlags.LockTransform | TileFlags.LockColor;
            tileData.colliderType = kind == Kind.Floor ? Tile.ColliderType.None : Tile.ColliderType.Grid;
        }

        // Uma célula influencia até 3 abaixo (abismo) e 2 acima (faces), então atualiza essa janela.
        public override void RefreshTile(Vector3Int position, ITilemap tilemap)
        {
            for (int dy = -3; dy <= 3; dy++)
            for (int dx = -1; dx <= 1; dx++)
                tilemap.RefreshTile(position + new Vector3Int(dx, dy, 0));
        }

        // ------------------------------------------------------------------ vizinhos

        static Kind KindAt(ITilemap tm, Vector3Int p)
        {
            var t = tm.GetTile(p) as GloomAutoTile;
            return t != null ? t.kind : Kind.Wall;
        }

        static bool FloorLike(ITilemap tm, Vector3Int p)
        {
            var k = KindAt(tm, p);
            return k == Kind.Floor || k == Kind.Rock;
        }

        // 1 = parte de baixo da face, 2 = parte de cima, 0 = não é face.
        static int WallFace(ITilemap tm, Vector3Int p)
        {
            if (KindAt(tm, p) != Kind.Wall) return 0;
            if (FloorLike(tm, p + Down)) return 1;
            if (KindAt(tm, p + Down) == Kind.Wall && FloorLike(tm, p + Down * 2)) return 2;
            return 0;
        }

        static int RockFace(ITilemap tm, Vector3Int p)
        {
            if (KindAt(tm, p) != Kind.Rock) return 0;
            if (KindAt(tm, p + Down) != Kind.Rock) return 1;
            if (KindAt(tm, p + Down * 2) != Kind.Rock) return 2;
            return 0;
        }

        // -1 ponta esquerda, 1 ponta direita, 0 meio (ou as duas pontas).
        static int WallFaceEnd(ITilemap tm, Vector3Int p)
        {
            bool l = FloorLike(tm, p + Left), r = FloorLike(tm, p + Right);
            return l == r ? 0 : (l ? -1 : 1);
        }

        static int RockEnd(ITilemap tm, Vector3Int p)
        {
            bool l = KindAt(tm, p + Left) != Kind.Rock, r = KindAt(tm, p + Right) != Kind.Rock;
            if (l && r) return 2;
            return l ? -1 : (r ? 1 : 0);
        }

        // ------------------------------------------------------------------ escolha do sprite

        Sprite Resolve(Vector3Int p, ITilemap tm)
        {
            switch (kind)
            {
                case Kind.Floor: return FloorSprite(p, tm);
                case Kind.Wall: return WallSprite(p, tm);
                case Kind.Pit: return PitSprite(p, tm);
                case Kind.Rock: return RockSprite(p, tm);
            }
            return null;
        }

        Sprite FloorSprite(Vector3Int p, ITilemap tm)
        {
            var s = sprites;
            int end = -99;
            if (WallFace(tm, p + Up) == 1) end = WallFaceEnd(tm, p + Up);
            else if (RockFace(tm, p + Up) == 1) end = RockEnd(tm, p + Up);

            if (end != -99)
            {
                var lips = end == -1 ? s.floorLipLeft : end == 1 ? s.floorLipRight : s.floorLip;
                if (lips != null && lips.Length > 0) return Pick(lips, p.x, p.y);
            }
            return Pick(s.floor, p.x, p.y);
        }

        Sprite WallSprite(Vector3Int p, ITilemap tm)
        {
            var s = sprites;
            int face = WallFace(tm, p);
            if (face > 0)
            {
                int end = WallFaceEnd(tm, p);
                // Upper e lower usam a mesma variação: sorteio pela célula de baixo da face.
                int yBase = face == 1 ? p.y : p.y - 1;
                Sprite[] upper = s.faceUpper, lower = s.faceLower;
                if (end == -1) { upper = s.faceUpperLeft; lower = s.faceLowerLeft; }
                else if (end == 1) { upper = s.faceUpperRight; lower = s.faceLowerRight; }
                return Pick(face == 2 ? upper : lower, p.x, yBase);
            }

            bool Open(Vector3Int q) => FloorLike(tm, q) || WallFace(tm, q) > 0;
            return Rim(p, Open);
        }

        Sprite PitSprite(Vector3Int p, ITilemap tm)
        {
            var s = sprites;
            bool Open(Vector3Int q) => FloorLike(tm, q);

            // Distância até o chão acima, atravessando só Buraco.
            int depth = 0;
            for (int k = 1; k <= 3; k++)
            {
                var q = p + Up * k;
                var kq = KindAt(tm, q);
                if (kq == Kind.Pit) continue;
                if (kq == Kind.Floor || kq == Kind.Rock) depth = k;
                break;
            }

            if (!Open(p + Left) && !Open(p + Right))
            {
                // Linha 1 é a borda; linhas 2 e 3 mostram a parede do abismo.
                if (depth == 2) return Pick(s.chasmUpper, p.x, 0);
                if (depth == 3) return Pick(s.chasmLower, p.x, 0);
            }
            return Rim(p, Open);
        }

        Sprite RockSprite(Vector3Int p, ITilemap tm)
        {
            var s = sprites;
            int face = RockFace(tm, p);
            int end = RockEnd(tm, p);
            if (face == 1)
            {
                if (end == 2) return s.rockFaceLowerSingle;
                if (end == -1) return s.rockFaceLowerLeft;
                if (end == 1) return s.rockFaceLowerRight;
                return Pick(s.faceLower, p.x, p.y);
            }
            if (face == 2)
            {
                if (end == 2) return s.rockFaceUpperSingle;
                if (end == -1) return s.rockFaceUpperLeft;
                if (end == 1) return s.rockFaceUpperRight;
                return Pick(s.faceUpper, p.x, p.y - 1);
            }
            if (end == 2) return s.rockTopSingle;
            if (end == -1) return s.rockTopLeft;
            if (end == 1) return s.rockTopRight;
            return s.rockTop;
        }

        Sprite Rim(Vector3Int p, System.Func<Vector3Int, bool> open)
        {
            var s = sprites;
            bool n = open(p + Up), so = open(p + Down), w = open(p + Left), e = open(p + Right);

            if (n && w) return s.rimNW;
            if (n && e) return s.rimNE;
            if (so && w) return s.rimSW;
            if (so && e) return s.rimSE;
            if (n) return s.rimN;
            if (so) return Pick(s.rimS, p.x, p.y);
            if (w) return s.rimW;
            if (e) return s.rimE;
            if (open(p + Down + Right)) return s.rimDiagSE;
            if (open(p + Down + Left)) return s.rimDiagSW;
            if (open(p + Up + Right)) return s.rimDiagNE;
            if (open(p + Up + Left)) return s.rimDiagNW;
            return s.black;
        }

        static Sprite Pick(Sprite[] arr, int x, int y)
        {
            if (arr == null || arr.Length == 0) return null;
            return arr[Hash(x, y) % arr.Length];
        }

        static int Hash(int x, int y)
        {
            unchecked
            {
                uint h = (uint)(x * 73856093) ^ (uint)(y * 19349663);
                h ^= h >> 13;
                h *= 0x5bd1e995;
                h ^= h >> 15;
                return (int)(h & 0x7fffffff);
            }
        }
    }
}
