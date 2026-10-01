using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Config;
using UnityEngine;

namespace PuzzleGame.Runtime.Board
{
    public sealed class PieceView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer face;
        [SerializeField] private SpriteRenderer shadow;
        private Vector3 baseScale;
        private Color baseColor;
        private Color shadowColor;
        public PieceColor CandyColor { get; private set; }
        public Material FaceMaterial => face.sharedMaterial;
        public int PieceId { get; private set; }
        public SpecialPieceType SpecialType { get; private set; }
        public GridPosition Position { get; private set; }
        public bool IsSelected { get; private set; }

        private void Awake() => baseScale = transform.localScale;

        public void Configure(SpriteRenderer faceRenderer, SpriteRenderer shadowRenderer)
        { face = faceRenderer; shadow = shadowRenderer; }

        public void Initialize(PieceState piece, GridPosition position, PieceCatalog.Entry appearance)
        {
            PieceId = piece.Id;
            baseScale = transform.localScale;
            shadowColor = shadow.color;
            SetAppearance(piece, appearance);
            SetPosition(position);
        }

        public void SetAppearance(PieceState piece, PieceCatalog.Entry appearance)
        {
            SpecialType = piece.SpecialType;
            CandyColor = piece.Color;
            baseColor = appearance.tint;
            face.sprite = appearance.sprite;
            face.sharedMaterial = appearance.material;
            face.color = appearance.tint;
            shadow.sprite = appearance.sprite;
            shadow.sharedMaterial = appearance.material;
        }

        public void SetPosition(GridPosition position)
        { Position = position; name = $"Piece_{PieceId}_{position.X}_{position.Y}"; }

        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            transform.localScale = baseScale * (selected ? 1.12f : 1f);
            face.color = selected ? Color.Lerp(baseColor, Color.white, 0.4f) : baseColor;
            face.sortingOrder = selected ? 4 : 2;
            shadow.sortingOrder = selected ? 3 : 1;
        }

        public void SetRemovalProgress(float progress)
        {
            float t = Mathf.Clamp01(progress);
            float scale = t < .16f ? Mathf.Lerp(1, .84f, t / .16f) : t < .32f ?
                Mathf.Lerp(.84f, 1.32f, (t-.16f)/.16f) : Mathf.Lerp(1.32f, 0, (t-.32f)/.68f);
            float squash = t < .16f ? Mathf.Sin(t/.16f*Mathf.PI)*.15f : 0;
            transform.localScale = Vector3.Scale(baseScale, new Vector3(scale + squash, scale - squash, 1));
            transform.localRotation = Quaternion.Euler(0, 0, (PieceId % 2 == 0 ? 24 : -24) * t);
            float alpha = 1 - Mathf.Clamp01((t-.32f)/.48f);
            face.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            shadow.color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, shadowColor.a * alpha);
        }

        public void SetFormationProgress(float progress)
        {
            float t = Mathf.Clamp01(progress);
            float pulse = 1 + Mathf.Sin(t * Mathf.PI * 3) * (1-t) * .24f;
            transform.localScale = baseScale * pulse;
            transform.localRotation = Quaternion.Euler(0,0,Mathf.Sin(t*Mathf.PI*2)*(1-t)*12);
        }

        public void SetLandingProgress(float progress)
        {
            float squash = Mathf.Sin(Mathf.Clamp01(progress)*Mathf.PI) * .16f;
            transform.localScale = Vector3.Scale(baseScale, new Vector3(1+squash, 1-squash, 1));
        }

        public void ResetMotion()
        {
            transform.localScale = baseScale;
            transform.localRotation = Quaternion.identity;
            face.color = baseColor; shadow.color = shadowColor;
        }
    }
}
