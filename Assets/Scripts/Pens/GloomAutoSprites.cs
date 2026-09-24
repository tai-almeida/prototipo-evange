// Evangeline — conjunto de sprites usado pelos GloomAutoTile (Chão, Parede, Buraco, Rocha).
// Gerado automaticamente por "Evangeline > Gloom Hollows > 4. Criar Auto Tiles".
// Os arrays "Upper" e "Lower" das faces andam juntos: o índice i de um combina com o índice i do outro.

using UnityEngine;

namespace Evangeline.Level
{
    [CreateAssetMenu(menuName = "Evangeline/Gloom Auto Sprites")]
    public class GloomAutoSprites : ScriptableObject
    {
        [Header("Vazio")]
        public Sprite black;

        [Header("Chão")]
        public Sprite[] floor;
        // Chão logo abaixo de uma face (com a "sombrinha" da base da parede). 4 variações cada.
        public Sprite[] floorLip;       // meio da face
        public Sprite[] floorLipLeft;   // ponta esquerda
        public Sprite[] floorLipRight;  // ponta direita

        [Header("Faces de parede (2 tiles de altura)")]
        public Sprite[] faceUpper, faceLower;
        public Sprite[] faceUpperLeft, faceLowerLeft;    // ponta esquerda (bloco solto no meio da sala)
        public Sprite[] faceUpperRight, faceLowerRight;  // ponta direita

        [Header("Borda do vazio (nome = lado onde fica o chão)")]
        public Sprite rimN;
        public Sprite[] rimS;
        public Sprite rimW, rimE;
        public Sprite rimNW, rimNE, rimSW, rimSE;                 // chão em dois lados (bloco solto)
        public Sprite rimDiagNW, rimDiagNE, rimDiagSW, rimDiagSE; // chão só na diagonal (cantos da sala)

        [Header("Abismo (dentro do Buraco, abaixo da borda de cima)")]
        public Sprite[] chasmUpper, chasmLower;

        [Header("Rocha (bloco com topo de pedra)")]
        public Sprite rockTop, rockTopLeft, rockTopRight, rockTopSingle;
        public Sprite rockFaceUpperLeft, rockFaceUpperRight, rockFaceUpperSingle;
        public Sprite rockFaceLowerLeft, rockFaceLowerRight, rockFaceLowerSingle;
    }
}
