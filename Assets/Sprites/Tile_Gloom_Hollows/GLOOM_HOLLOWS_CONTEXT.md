# Contexto — Tileset Gloom Hollows no projeto Evangeline

Handoff de uma sessão no Claude Cowork (23/09/2026) para continuar no Claude Code.

## Projeto
- **Evangeline**: roguelite RPG 2D em pixel art, feito em **Unity**, sobre uma bruxa que é ruim de magia. Time Academigos.
- Repo local: `/Users/ludivikdepaula/Documents/GitHub/prototipo-evange`
- Asset pack: **Minifantasy – Gloom Hollows** (Krishna Palacio), em
  `Assets/Sprites/Tile_Gloom_Hollows/Minifantasy_Gloom_Hollows_Assets/`

## Especificações do asset pack
- Grade de **8×8 px**, perspectiva top-down 3/4 (dá para ver o topo e a face das paredes).
- `Tileset/Tileset.png` tem 1072×840 px. `Tileset/Tileset_Shadows.png` tem o mesmo tamanho e fica **alinhado 1:1** com o Tileset: é a sombra de cada tile na mesma posição.
- `Tileset/_Use_Guidelines/Tileset_Use_Guidelines.png` é um mapa colorido dos grupos:
  - Ground (chão com variações).
  - Inner cave walls (paredes internas) e Perimeter cave walls (bordas contra o vazio, com contorno preto).
  - Blocos 2×2, que funcionam como moldes de 9 partes (cantos, lados, centro).
  - Wall protuberances (encaixam nas bordas para quebrar retas).
  - Wall variations e Diagonal wall variations (trocam faces da parede).
  - Diagonal walls (octógonos que servem de molde de canto a 45°).
  - Chasms (abismo abaixo da face das paredes de perímetro).
  - Entrances (arcos), Stairs, Lakes & water, Ruins (piso, colunas, portas e janelas).
- Água animada em `Tileset/Animated_Tiles/`: `Only_Water`, `Water_Lake` (240×80) e `*_Diagonals` (432×144). Cada folha tem **6 frames lado a lado na horizontal**, com **300 ms por frame**. Tiles de 8×8.
- Props em `Props/Props.png` (960×592) mais `Props_Shadows.png`. Líquen animado em `Props/Animated_Props/Bioluminiscent_Lichen.png` (frames de 24×24).
- `Premade/` traz um mapa de exemplo (632×376), o `.aseprite` e `_Separate_Layers/`. As camadas separadas mostram a ordem de empilhamento do artista, de cima para baixo: a‑darkness, b‑shadows, c‑stalactites, d‑ruins_debris, e‑mushrooms, f‑lichen, g‑crystals_and_rocks, h‑ruins, i‑stairs, j‑cave_walls_perimeter, k‑chasms, l‑other_walls, m‑lakes, n‑ruins_floor, o‑ground.

## Estado atual da importação (verificado nos .meta)
- ✅ Tileset.png: 2625 sprites de 8×8, PPU 8, filtro Point, sem compressão (fatiado em grid pelo usuário).
- ✅ Tileset_Shadows.png: 600 sprites de 8×8, PPU 8, Point.
- ✅ As 4 folhas de água: fatiadas em 8×8, PPU 8, Point.
- ⚠️ Props.png, Props_Shadows.png e Bioluminiscent_Lichen.png ainda estão com **PPU 100, filtro Bilinear e compressão ligada**. O fatiamento automático pode continuar, porque props são objetos inteiros. É preciso trocar para PPU 8, Point e sem compressão. O script abaixo faz isso.

## O que foi entregue: script de Editor
`Assets/Sprites/Tile_Gloom_Hollows/Editor/GloomHollowsSetup.cs`. Está envolvido em `#if UNITY_EDITOR` e usa o namespace `Evangeline.EditorTools`. **Ainda não foi compilado nem testado no Unity.**

Menu **Evangeline → Gloom Hollows**:
1. **Configurar tudo**
   - Ajusta a importação de todos os sprites do pack (menos `_Use_Guidelines`): PPU 8, Point, Uncompressed, sem mipmaps.
   - Cria um `Tile` `.asset` para cada sprite em `Assets/Tiles/GloomHollows/Tileset/` (colliderType = Grid) e em `.../Shadows/` (colliderType = None).
   - Cria as paletas via `GridPaletteUtility.CreateNewPalette` em `Assets/Tiles/GloomHollows/Palettes/`: **Gloom_Tileset** e **Gloom_Shadows**. Cada tile fica na posição `rect/8` da folha, preservando o layout do guia.
   - Cria a água animada: um `AnimatedTile` por célula do frame 0, com os 6 sprites nas posições `x + f*larguraDoFrame` e velocidade de 3.33 fps. Vai para `.../Water/<folha>/`, na paleta **Gloom_Water**, com as 4 folhas empilhadas.
   - O `AnimatedTile` é acessado por **reflection**, para o script compilar mesmo sem o pacote. Se faltar `com.unity.2d.tilemap.extras`, ele oferece instalar via `PackageManager.Client.Add`. Depois é preciso rodar o menu de novo.
   - O script é idempotente: atualiza os assets que já existem em vez de duplicar.
2. **Criar Grid com camadas na cena aberta**: cria `Gloom_Grid` (cell 1×1) com as Tilemaps abaixo.

   | Tilemap | Sorting Order | Collider |
   |---|---|---|
   | 00_Ground | 0 | — |
   | 01_RuinsFloor | 10 | — |
   | 02_Water | 20 | — |
   | 03_Chasms | 30 | TilemapCollider2D |
   | 04_Walls | 40 | TilemapCollider2D |
   | 05_Stairs | 50 | — |
   | 06_Ruins | 60 | TilemapCollider2D |
   | 07_Decor | 70 | — |
   | 08_Stalactites | 90 | — |
   | 09_Shadows | 100 | — |

   O player sugerido fica em Order 80.
3. **Colocar Premade como referência**: põe o Premade.png como SpriteRenderer com alpha 0.35, sorting -100, com o canto inferior esquerdo na origem, para servir de decalque.

### Premissas e riscos do script
- O caminho está fixo em `AssetsRoot = "Assets/Sprites/Tile_Gloom_Hollows/Minifantasy_Gloom_Hollows_Assets"`.
- `GridPaletteUtility.CreateNewPalette` recebe um caminho **absoluto** (`Path.GetFullPath(folder)`).
- Se existir um `.asmdef` acima da pasta `Editor/` sem referência a `Unity.2D.Tilemap.Editor`, a compilação quebra.
- A versão do Unity e os pacotes do projeto **não foram verificados**, porque a sessão só tinha acesso à pasta do tileset.

## Próximos passos sugeridos
1. Abrir o Unity, conferir se o script compila (corrigir erros do Console, se houver) e rodar "Configurar tudo".
2. Confirmar se as paletas aparecem em Window → 2D → Tile Palette e se a água anima.
3. Adicionar o Pixel Perfect Camera (Assets PPU = 8).
4. Resolver a ordenação do player contra paredes 3/4: Transparency Sort Mode = Custom Axis (0,1,0). No URP 2D isso fica no Renderer2D Data. Também pode ser preciso deixar o TilemapRenderer em modo Individual nas camadas de parede e decor.
5. Transformar props (cogumelos, cristais, líquen) em prefabs com a sombra correspondente, em vez de pintar na tilemap.
6. Opcional: criar Rule Tiles com saída Random para as variações de chão, e Rule Tiles para as paredes.

## Unity MCP
O usuário tem `unity-mcp`, com o relay em `~/.unity/relay/relay_mac_arm64.app/Contents/MacOS/relay_mac_arm64 --mcp`. Ele não estava disponível na sessão do Cowork. No Claude Code, dá para adicionar com
`claude mcp add unity-mcp -- /Users/ludivikdepaula/.unity/relay/relay_mac_arm64.app/Contents/MacOS/relay_mac_arm64 --mcp`
com o Unity Editor aberto. Assim é possível rodar o menu e ler o Console direto.
