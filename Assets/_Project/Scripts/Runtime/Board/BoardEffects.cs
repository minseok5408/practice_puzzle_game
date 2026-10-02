using System.Collections.Generic;
using PuzzleGame.Core.Board;
using UnityEngine;

namespace PuzzleGame.Runtime.Board
{
    // Presentation clock only. Targets come from Core; effects never decide what is removed.
    public sealed class BoardEffects : MonoBehaviour
    {
        public const float PopLead = .075f;
        public const float PopDuration = .26f;
        private const float WaveSpeed = 18f;
        private readonly Dictionary<int, float> hits = new Dictionary<int, float>();
        private readonly Dictionary<int, float> fires = new Dictionary<int, float>();
        private readonly Dictionary<int, PieceView> targets = new Dictionary<int, PieceView>();
        private readonly List<SpriteRenderer> pool = new List<SpriteRenderer>();
        private ResolutionStep step;
        private Texture2D texture;
        private Material material;
        private Sprite glow, ring, chip;
        private int used, width, height;
        public bool IsPlaying { get; private set; }
        public float Elapsed { get; private set; }
        public float Duration { get; private set; }
        public int ActiveVisualCount => used;
        public float HitTime(int id) => hits.TryGetValue(id, out float time) ? time : 0;

        public void Begin(ResolutionStep resolution, IReadOnlyDictionary<int, PieceView> pieces, int columns, int rows)
        {
            Clear();
            step = resolution; width = columns; height = rows;
            foreach (var pair in pieces) targets.Add(pair.Key, pair.Value);
            if (!material)
            {
                foreach (var piece in pieces.Values) { InitializeGraphics(piece.FaceMaterial); break; }
            }
            foreach (int id in step.RemovedIds) hits[id] = float.PositiveInfinity;
            foreach (int id in step.InitialHitIds) hits[id] = 0;
            // Dijkstra-style propagation keeps chains synchronized with the first arriving wave.
            var pending = new List<SpecialActivation>(step.SpecialActivations);
            while (pending.Count > 0)
            {
                int next = 0;
                for (int i = 1; i < pending.Count; i++)
                    if (HitTime(pending[i].PieceId) + Charge(pending[i].Type) <
                        HitTime(pending[next].PieceId) + Charge(pending[next].Type)) next = i;
                var activation = pending[next]; pending.RemoveAt(next);
                float hit = HitTime(activation.PieceId);
                if (float.IsPositiveInfinity(hit)) hit = 0;
                float fire = hit + Charge(activation.Type);
                fires[activation.PieceId] = fire;
                Vector3 origin = targets[activation.PieceId].transform.localPosition;
                foreach (int id in activation.AffectedIds)
                {
                    float distance = Vector3.Distance(origin, targets[id].transform.localPosition);
                    float travel = activation.Type == SpecialPieceType.ColorClear ? .23f + distance * .018f :
                        activation.Type == SpecialPieceType.Bomb ? distance * .085f : distance / WaveSpeed;
                    if (id != activation.PieceId) hits[id] = Mathf.Min(HitTime(id), fire + travel - PopLead);
                }
            }
            Duration = step.SpecialCreations.Count > 0 ? .62f : 0;
            if (step.FrostDamage.Count > 0) Duration = Mathf.Max(Duration,.35f);
            foreach (int id in step.RemovedIds)
            {
                if (float.IsPositiveInfinity(hits[id])) hits[id] = 0;
                if (fires.TryGetValue(id, out float fire)) hits[id] = fire - PopLead;
                Duration = Mathf.Max(Duration, hits[id] + .48f);
            }
            foreach (var activation in step.SpecialActivations)
                Duration = Mathf.Max(Duration, fires[activation.PieceId] +
                    (activation.Type == SpecialPieceType.Row || activation.Type == SpecialPieceType.Column ? .68f : .62f));
            IsPlaying = true;
            Sample(0);
        }

        public void Sample(float time)
        {
            Elapsed = time; used = 0;
            foreach (int id in step.RemovedIds)
            {
                PieceView piece = targets[id];
                float age = time - HitTime(id) - PopLead;
                Burst(piece.transform.localPosition, CandyColor(piece.CandyColor), age, id);
            }
            foreach (var activation in step.SpecialActivations)
            {
                Vector3 origin = targets[activation.PieceId].transform.localPosition;
                float age = time - fires[activation.PieceId];
                Color color = CandyColor(targets[activation.PieceId].CandyColor);
                if (age < 0 && age > -.2f)
                {
                    float charge = 1 + age / .2f;
                    Disc(origin, .7f + .4f * charge, color, charge * .6f);
                    Ring(origin, 1.25f - charge * .75f, Color.white, charge * .8f);
                }
                if (age < 0) continue;
                switch (activation.Type)
                {
                    case SpecialPieceType.Row: Wave(origin, Vector3.right, color, age); break;
                    case SpecialPieceType.Column: Wave(origin, Vector3.up, color, age); break;
                    case SpecialPieceType.Bomb:
                        if (age < .55f)
                        {
                            float t = age / .55f;
                            Disc(origin, 1.2f + t * 3, color, (1-t) * .55f);
                            Ring(origin, .25f + 3.1f * (1 - Mathf.Pow(1-t, 2)), new Color(1,.87f,.53f), (1-t) * .9f);
                            Ring(origin, .2f + 2.7f * t, Color.white, (1-t) * .7f);
                            Burst(origin, color, age, activation.PieceId, 2.2f);
                        }
                        break;
                    case SpecialPieceType.ColorClear:
                        Rainbow(activation, origin, age);
                        break;
                }
            }
            foreach (var creation in step.SpecialCreations)
            {
                Vector3 position = targets[creation.Piece.Id].transform.localPosition;
                if (time > .6f) continue;
                float t = time / .6f;
                Ring(position, 1.7f * (1-t) + .15f, new Color(1,.87f,.45f), Mathf.Sin(t * Mathf.PI));
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * Mathf.PI / 4 + t * 1.8f;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * (1-t) * 1.15f;
                    Spark(position + offset, .14f, Color.white, Mathf.Sin(t * Mathf.PI));
                }
            }
            for (int i = used; i < pool.Count; i++) pool[i].enabled = false;
        }

        private void Wave(Vector3 origin, Vector3 axis, Color color, float age)
        {
            for (int direction = -1; direction <= 1; direction += 2)
            {
                float extent = axis.x != 0 ? width * .5f - origin.x * direction : height * .5f - origin.y * direction;
                float distance = Mathf.Min(age * WaveSpeed, extent + .15f);
                float fade = 1 - Mathf.Clamp01((age - extent / WaveSpeed) / .2f);
                if (fade <= 0) continue;
                Vector3 tip = origin + axis * (distance * direction);
                Trail(origin, tip, .55f, color, .85f * fade);
                Trail(origin, tip, .16f, new Color(1, .98f, .88f), fade);
                Disc(tip, .88f, color, fade);
                Spark(tip, .62f, Color.white, fade);
                for (int i = 0; i < 3; i++)
                {
                    Vector3 side = new Vector3(-axis.y, axis.x) * ((i - 1) * .17f);
                    Trail(tip + side - axis * direction * .5f, tip + side, .045f, Color.white, fade * .8f);
                }
            }
        }

        private static float Charge(SpecialPieceType type) => type == SpecialPieceType.ColorClear ? .28f : .13f;

        private void Rainbow(SpecialActivation activation, Vector3 origin, float age)
        {
            if (age > .62f) return;
            float fade = 1 - Mathf.Clamp01((age - .33f) / .25f);
            Ring(origin, .35f + age * 3, new Color(1,.87f,.6f), fade);
            Disc(origin, 1.2f, Color.white, fade * .8f);
            foreach (int id in activation.AffectedIds)
            {
                if (id == activation.PieceId) continue;
                Vector3 end = targets[id].transform.localPosition;
                float distance = Vector3.Distance(origin, end);
                float t = Mathf.Clamp01(age / (.23f + distance * .018f));
                Color color = CandyColor(targets[id].CandyColor);
                Vector3 previous = origin;
                // Curved ribbons make the selected color readable before the target pops.
                for (int segment = 1; segment <= 5; segment++)
                {
                    float s = t * segment / 5;
                    Vector3 point = Vector3.Lerp(origin, end, s);
                    point += new Vector3(-(end-origin).y, (end-origin).x).normalized *
                        (Mathf.Sin(s * Mathf.PI) * .28f * (id % 2 == 0 ? 1 : -1));
                    Trail(previous, point, .12f, color, fade * .85f);
                    Trail(previous, point, .04f, Color.white, fade);
                    previous = point;
                }
                Spark(previous, .24f, Color.white, fade);
            }
        }

        private void Burst(Vector3 origin, Color color, float age, int seed, float power = 1)
        {
            if (age < 0 || age > .4f) return;
            float t = age / .4f, fade = (1-t) * (1-t);
            Disc(origin, (.55f + t * .75f) * power, color, fade * .7f);
            Disc(origin, (.5f + t * .3f) * power, Color.white, Mathf.Max(0, 1-age/.16f));
            Ring(origin, (.2f + t * 1.3f) * power, Color.Lerp(color, Color.white, .6f), fade);
            for (int i = 0; i < 7; i++)
            {
                float angle = i * 2 * Mathf.PI / 7 + seed * 2.399f;
                Vector3 velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * (1.6f + i % 3 * .4f) * power;
                Vector3 position = origin + velocity * age + Vector3.down * (age * age * 2.4f);
                float size = (.17f + i % 2 * .05f) * power * (1 - t * .65f);
                Draw(chip, position, new Vector2(size, size * 1.4f), angle * Mathf.Rad2Deg + age * 290,
                    Color.Lerp(color, Color.white, i % 3 == 0 ? .65f : .12f), 1-t);
                if (i % 2 == 0) Spark(position + Vector3.up * .06f, .075f * power, Color.white, fade);
                if (age < .18f)
                {
                    Vector3 direction = velocity.normalized;
                    Trail(origin + direction * (.13f + age) * power,
                        origin + direction * (.36f + age * 2.2f) * power, .065f * power,
                        new Color(1,.97f,.85f), 1-age/.18f);
                }
            }
        }

        private void Disc(Vector3 position, float size, Color color, float alpha) => Draw(glow, position, Vector2.one * size, 0, color, alpha);
        private void Ring(Vector3 position, float size, Color color, float alpha) => Draw(ring, position, Vector2.one * size, 0, color, alpha);
        private void Spark(Vector3 position, float size, Color color, float alpha)
        {
            Draw(glow, position, new Vector2(size, size * .2f), 0, color, alpha);
            Draw(glow, position, new Vector2(size * .2f, size), 0, color, alpha);
        }
        private void Trail(Vector3 from, Vector3 to, float thickness, Color color, float alpha)
        {
            Vector3 delta = to - from;
            Draw(glow, (from+to) * .5f, new Vector2(delta.magnitude + thickness, thickness),
                Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, color, alpha);
        }
        private void Draw(Sprite sprite, Vector3 position, Vector2 size, float rotation, Color color, float alpha)
        {
            if (alpha <= .005f || used >= 1400) return;
            if (used == pool.Count)
            {
                var renderer = new GameObject("SugarFX").AddComponent<SpriteRenderer>();
                renderer.transform.SetParent(transform, false);
                renderer.sharedMaterial = material; renderer.sortingOrder = 12;
                pool.Add(renderer);
            }
            SpriteRenderer item = pool[used++];
            item.enabled = true; item.sprite = sprite;
            item.transform.localPosition = position;
            item.transform.localScale = new Vector3(size.x, size.y, 1);
            item.transform.localRotation = Quaternion.Euler(0,0,rotation);
            color.a = Mathf.Clamp01(alpha); item.color = color;
        }

        public static Color CandyColor(PieceColor color)
        {
            switch (color)
            {
                case PieceColor.Red: return new Color(1,.16f,.3f);
                case PieceColor.Orange: return new Color(1,.48f,.08f);
                case PieceColor.Yellow: return new Color(1,.85f,.12f);
                case PieceColor.Green: return new Color(.38f,1,.14f);
                case PieceColor.Blue: return new Color(.12f,.67f,1);
                case PieceColor.Purple: return new Color(.77f,.28f,1);
                default: return new Color(1,.75f,.9f);
            }
        }

        private void InitializeGraphics(Material template)
        {
            const int size = 64;
            texture = new Texture2D(size * 3, size, TextureFormat.RGBA32, false) { name = "Sugar FX masks", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color[size * size * 3];
            for (int y = 0; y < size; y++) for (int x = 0; x < size * 3; x++)
            {
                float px = ((x % size + .5f) / size - .5f) * 2, py = ((y + .5f) / size - .5f) * 2;
                float radius = Mathf.Sqrt(px*px + py*py);
                float alpha = x / size == 0 ? Mathf.Pow(Mathf.Clamp01((1-radius)*1.6f), 1.25f) :
                    x / size == 1 ? Mathf.Clamp01(1-Mathf.Abs(radius-.78f)/.07f) :
                    Mathf.Clamp01((1 - Mathf.Abs(px) - Mathf.Abs(py)) * 12);
                pixels[y * size * 3 + x] = new Color(1,1,1,alpha);
            }
            texture.SetPixels(pixels); texture.Apply(false, true);
            glow = Sprite.Create(texture, new Rect(0,0,size,size), Vector2.one*.5f, size);
            ring = Sprite.Create(texture, new Rect(size,0,size,size), Vector2.one*.5f, size);
            chip = Sprite.Create(texture, new Rect(size*2,0,size,size), Vector2.one*.5f, size);
            material = new Material(template) { name = "Sugar FX", mainTexture = texture };
        }

        public void Clear()
        {
            foreach (var renderer in pool) if (renderer) renderer.enabled = false;
            used = 0; IsPlaying = false; Elapsed = 0; Duration = 0;
            hits.Clear(); fires.Clear(); targets.Clear(); step = null;
        }
        private void OnDisable() => Clear();
        private void OnDestroy()
        {
            if (glow) Destroy(glow); if (ring) Destroy(ring); if (chip) Destroy(chip);
            if (texture) Destroy(texture); if (material) Destroy(material);
        }
    }
}
