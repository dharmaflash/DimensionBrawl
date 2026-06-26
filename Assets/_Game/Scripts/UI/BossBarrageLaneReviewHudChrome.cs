using UnityEngine;

namespace DimensionBrawl.UI
{
    internal static class BossBarrageLaneReviewHudChrome
    {
        private static readonly Color PanelBackColor = new Color(0.015f, 0.022f, 0.034f, 0.74f);
        private static readonly Color PanelBackSoftColor = new Color(0.03f, 0.045f, 0.065f, 0.48f);
        private static readonly Color GoldColor = new Color(1f, 0.72f, 0.34f, 0.92f);
        private static readonly Color DimGoldColor = new Color(0.92f, 0.62f, 0.28f, 0.48f);
        private static readonly Color IceColor = new Color(0.46f, 0.9f, 1f, 0.88f);
        private static readonly Color TextColor = new Color(0.93f, 0.97f, 1f, 0.96f);
        private static readonly Color MutedTextColor = new Color(0.72f, 0.82f, 0.9f, 0.78f);

        private static Texture2D circleTexture;
        private static Texture2D softCircleTexture;
        private static Texture2D ringTexture;
        private static GUIStyle centerLabelStyle;
        private static GUIStyle smallCenterLabelStyle;
        private static GUIStyle leftLabelStyle;
        private static GUIStyle titleLabelStyle;

        public static void DrawObjectivePanel(
            Rect rect,
            string title,
            string detail,
            string badge,
            string subdetail = null,
            string footnote = null)
        {
            DrawPanel(rect, GoldColor);
            DrawDiamond(new Vector2(rect.x + 32f, rect.center.y), 21f, GoldColor);
            DrawDiamond(new Vector2(rect.x + 32f, rect.center.y), 11f, new Color(0.05f, 0.08f, 0.11f, 0.96f));

            EnsureStyles();
            ResetLabelColors();
            titleLabelStyle.fontSize = Mathf.RoundToInt(Mathf.Clamp(rect.height * 0.24f, 18f, 25f));
            leftLabelStyle.fontSize = Mathf.RoundToInt(Mathf.Clamp(rect.height * 0.16f, 13f, 18f));
            GUI.Label(new Rect(rect.x + 66f, rect.y + 13f, rect.width - 108f, 28f), title, titleLabelStyle);
            GUI.Label(new Rect(rect.x + 66f, rect.y + 43f, rect.width - 108f, 25f), detail, leftLabelStyle);
            if (!string.IsNullOrWhiteSpace(subdetail))
            {
                smallCenterLabelStyle.alignment = TextAnchor.MiddleLeft;
                smallCenterLabelStyle.fontSize = 12;
                smallCenterLabelStyle.normal.textColor = MutedTextColor;
                GUI.Label(new Rect(rect.x + 66f, rect.y + 65f, rect.width - 108f, 18f), subdetail, smallCenterLabelStyle);
                if (!string.IsNullOrWhiteSpace(footnote))
                {
                    GUI.Label(new Rect(rect.x + 66f, rect.y + 83f, rect.width - 108f, 18f), footnote, smallCenterLabelStyle);
                }

                smallCenterLabelStyle.alignment = TextAnchor.MiddleCenter;
                smallCenterLabelStyle.normal.textColor = TextColor;
            }

            Rect badgeRect = new Rect(rect.xMax - 96f, rect.y + 31f, 68f, 25f);
            DrawSolid(badgeRect, new Color(0.02f, 0.04f, 0.055f, 0.74f));
            DrawBorder(badgeRect, DimGoldColor, 1f);
            smallCenterLabelStyle.fontSize = 12;
            GUI.Label(badgeRect, badge, smallCenterLabelStyle);
        }

        public static void DrawBossBar(Rect rect, string title, string phase, float hpFill, float costFill)
        {
            EnsureStyles();
            ResetLabelColors();
            titleLabelStyle.fontSize = Mathf.RoundToInt(Mathf.Clamp(rect.height * 0.28f, 18f, 28f));
            leftLabelStyle.fontSize = Mathf.RoundToInt(Mathf.Clamp(rect.height * 0.18f, 12f, 17f));

            DrawDiamond(new Vector2(rect.x - 22f, rect.y + 24f), 18f, GoldColor);
            GUI.Label(new Rect(rect.x, rect.y, rect.width, 28f), title, titleLabelStyle);
            GUI.Label(new Rect(rect.x, rect.y + 28f, rect.width, 20f), phase, leftLabelStyle);

            Rect hpRect = new Rect(rect.x, rect.y + 50f, rect.width, 14f);
            DrawBar(hpRect, Mathf.Clamp01(hpFill), new Color(1f, 0.27f, 0.19f, 0.95f), GoldColor);

            Rect costRect = new Rect(rect.x, rect.y + 68f, rect.width * 0.82f, 8f);
            DrawBar(costRect, Mathf.Clamp01(costFill), new Color(0.96f, 0.66f, 0.22f, 0.92f), DimGoldColor);
        }

        public static void DrawPlayerResourcePanel(
            Rect rect,
            string title,
            string survivalCueText,
            string hpText,
            float hpFill,
            Color hpFillColor,
            string energyText,
            float energyFill,
            bool energyReady)
        {
            DrawPanel(rect, IceColor);
            EnsureStyles();
            ResetLabelColors();
            titleLabelStyle.fontSize = Mathf.RoundToInt(Mathf.Clamp(rect.height * 0.22f, 15f, 20f));
            leftLabelStyle.fontSize = Mathf.RoundToInt(Mathf.Clamp(rect.height * 0.16f, 11f, 15f));

            GUI.Label(new Rect(rect.x + 18f, rect.y + 9f, rect.width * 0.42f, 24f), title, titleLabelStyle);
            smallCenterLabelStyle.alignment = TextAnchor.MiddleRight;
            smallCenterLabelStyle.fontSize = Mathf.RoundToInt(Mathf.Clamp(rect.height * 0.14f, 10f, 13f));
            bool urgentSurvivalCue = !string.IsNullOrWhiteSpace(survivalCueText)
                && survivalCueText.StartsWith("Critical", System.StringComparison.Ordinal);
            smallCenterLabelStyle.normal.textColor = urgentSurvivalCue
                ? new Color(1f, 0.42f, 0.26f, 0.96f)
                : energyReady
                    ? GoldColor
                    : MutedTextColor;
            GUI.Label(new Rect(rect.x + rect.width * 0.45f, rect.y + 10f, rect.width * 0.49f, 20f), survivalCueText, smallCenterLabelStyle);
            smallCenterLabelStyle.alignment = TextAnchor.MiddleCenter;
            smallCenterLabelStyle.normal.textColor = TextColor;
            GUI.Label(new Rect(rect.x + 18f, rect.y + 32f, rect.width * 0.42f, 18f), hpText, leftLabelStyle);
            GUI.Label(new Rect(rect.x + rect.width * 0.58f, rect.y + 32f, rect.width * 0.38f, 18f), energyText, leftLabelStyle);

            DrawBar(
                new Rect(rect.x + 18f, rect.y + 54f, rect.width * 0.44f, 10f),
                Mathf.Clamp01(hpFill),
                hpFillColor,
                IceColor);
            DrawBar(
                new Rect(rect.x + rect.width * 0.54f, rect.y + 54f, rect.width * 0.36f, 10f),
                Mathf.Clamp01(energyFill),
                energyReady ? GoldColor : new Color(0.2f, 0.78f, 1f, 0.86f),
                energyReady ? GoldColor : IceColor);
        }

        public static void DrawActionButton(Rect rect, string label, bool held, bool pending, Color accent)
        {
            float size = Mathf.Min(rect.width, rect.height);
            Rect circle = RectFromCenter(rect.center, size);
            Color resolvedAccent = pending ? new Color(0.5f, 0.56f, 0.62f, 0.55f) : accent;
            float pulse = 0.5f + Mathf.Sin(Time.unscaledTime * 4.2f) * 0.5f;
            Color glow = resolvedAccent;
            glow.a = held ? 0.36f : pending ? 0.08f : Mathf.Lerp(0.14f, 0.24f, pulse);
            DrawCircle(Inflate(circle, 16f), glow, softCircleTexture);
            DrawCircle(circle, new Color(0.018f, 0.026f, 0.037f, pending ? 0.46f : 0.76f), circleTexture);
            DrawCircle(circle, resolvedAccent, ringTexture);
            DrawCircle(Inflate(circle, -size * 0.16f), new Color(1f, 1f, 1f, held ? 0.18f : 0.07f), ringTexture);
            DrawControlTicks(circle, resolvedAccent, held);

            EnsureStyles();
            centerLabelStyle.fontSize = Mathf.RoundToInt(Mathf.Clamp(size * 0.17f, 16f, 28f));
            centerLabelStyle.normal.textColor = pending ? MutedTextColor : TextColor;
            GUI.Label(new Rect(rect.x, rect.center.y - size * 0.17f, rect.width, size * 0.34f), label, centerLabelStyle);
        }

        public static void DrawJoystick(Rect rect, Vector2 input, bool held, float knobSize, Color accent)
        {
            DrawCircle(Inflate(rect, 18f), new Color(accent.r, accent.g, accent.b, held ? 0.2f : 0.1f), softCircleTexture);
            DrawCircle(rect, new Color(0.02f, 0.03f, 0.04f, 0.42f), circleTexture);
            DrawCircle(rect, new Color(0.82f, 0.92f, 1f, 0.36f), ringTexture);
            DrawCircle(Inflate(rect, -rect.width * 0.18f), new Color(1f, 0.78f, 0.38f, 0.22f), ringTexture);
            DrawCompassMarks(rect, accent);

            Vector2 knobCenter = rect.center + new Vector2(input.x, -input.y) * (rect.width * 0.32f);
            Rect knobRect = RectFromCenter(knobCenter, knobSize);
            DrawCircle(Inflate(knobRect, 10f), new Color(accent.r, accent.g, accent.b, held ? 0.28f : 0.12f), softCircleTexture);
            DrawCircle(knobRect, held ? new Color(0.65f, 0.96f, 1f, 0.82f) : new Color(0.1f, 0.16f, 0.2f, 0.78f), circleTexture);
            DrawCircle(knobRect, GoldColor, ringTexture);
        }

        public static void DrawSummonSlot(Rect rect, string label, bool held, bool pending, float fill01, Color accent)
        {
            DrawPanel(rect, pending ? new Color(0.5f, 0.56f, 0.62f, 0.45f) : accent);
            float diskSize = Mathf.Min(rect.height * 0.78f, rect.width * 0.34f);
            Rect disk = RectFromCenter(new Vector2(rect.x + diskSize * 0.72f, rect.center.y), diskSize);
            float readyPulse = fill01 >= 0.995f ? 0.5f + Mathf.Sin(Time.unscaledTime * 5.6f) * 0.5f : 0f;
            DrawCircle(
                Inflate(disk, 12f + readyPulse * 5f),
                new Color(accent.r, accent.g, accent.b, held ? 0.3f : 0.12f + readyPulse * 0.14f),
                softCircleTexture);
            DrawCircle(disk, new Color(0.02f, 0.03f, 0.045f, pending ? 0.56f : 0.82f), circleTexture);
            DrawProgressRing(disk, pending ? 0f : fill01, accent, 4f);
            DrawDiamond(disk.center, diskSize * 0.19f, pending ? MutedTextColor : GoldColor);

            EnsureStyles();
            leftLabelStyle.fontSize = Mathf.RoundToInt(Mathf.Clamp(rect.height * 0.19f, 13f, 18f));
            leftLabelStyle.normal.textColor = pending ? MutedTextColor : TextColor;
            GUI.Label(new Rect(rect.x + diskSize + 18f, rect.y + 8f, rect.width - diskSize - 24f, rect.height - 16f), label, leftLabelStyle);
        }

        public static void DrawAimGuide(Vector2 startGuiPoint, Vector2 input, float radius, float knobSize)
        {
            Rect baseRect = RectFromCenter(startGuiPoint, radius * 0.52f);
            DrawCircle(baseRect, new Color(0.46f, 0.9f, 1f, 0.08f), circleTexture);
            DrawCircle(baseRect, new Color(0.46f, 0.9f, 1f, 0.28f), ringTexture);
            Vector2 knobCenter = startGuiPoint + new Vector2(input.x, -input.y) * radius;
            Rect knobRect = RectFromCenter(knobCenter, knobSize);
            DrawCircle(Inflate(knobRect, 7f), new Color(0.46f, 0.9f, 1f, 0.22f), softCircleTexture);
            DrawCircle(knobRect, new Color(0.46f, 0.9f, 1f, 0.72f), circleTexture);
        }

        private static void DrawPanel(Rect rect, Color accent)
        {
            DrawSolid(rect, PanelBackColor);
            DrawSolid(new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height * 0.46f), PanelBackSoftColor);
            DrawBorder(rect, new Color(accent.r, accent.g, accent.b, 0.48f), 1.5f);
            DrawCornerLines(rect, accent);
        }

        private static void DrawBar(Rect rect, float fill01, Color fill, Color accent)
        {
            DrawSolid(rect, new Color(0.01f, 0.014f, 0.02f, 0.82f));
            Rect fillRect = rect;
            fillRect.width *= Mathf.Clamp01(fill01);
            DrawSolid(fillRect, fill);
            DrawSolid(new Rect(rect.x, rect.y, rect.width, 1.5f), new Color(1f, 1f, 1f, 0.18f));
            DrawBorder(rect, new Color(accent.r, accent.g, accent.b, 0.52f), 1f);
        }

        private static void DrawProgressRing(Rect rect, float fill01, Color color, float thickness)
        {
            DrawCircle(rect, new Color(1f, 1f, 1f, 0.14f), ringTexture);
            int steps = 48;
            int filled = Mathf.CeilToInt(Mathf.Clamp01(fill01) * steps);
            Vector2 center = rect.center;
            float radius = rect.width * 0.5f;
            for (int i = 0; i < filled; i++)
            {
                float a0 = (-90f + i * 360f / steps) * Mathf.Deg2Rad;
                float a1 = (-90f + (i + 0.76f) * 360f / steps) * Mathf.Deg2Rad;
                Vector2 p0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * radius;
                Vector2 p1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius;
                DrawLine(p0, p1, color, thickness);
            }
        }

        private static void DrawControlTicks(Rect rect, Color color, bool held)
        {
            float half = rect.width * 0.5f;
            float inner = half * 0.72f;
            float outer = half * 0.91f;
            Vector2 c = rect.center;
            Color tickColor = color;
            tickColor.a = held ? 0.86f : 0.52f;
            DrawLine(c + Vector2.up * inner, c + Vector2.up * outer, tickColor, 2f);
            DrawLine(c + Vector2.down * inner, c + Vector2.down * outer, tickColor, 2f);
            DrawLine(c + Vector2.left * inner, c + Vector2.left * outer, tickColor, 2f);
            DrawLine(c + Vector2.right * inner, c + Vector2.right * outer, tickColor, 2f);
        }

        private static void DrawCompassMarks(Rect rect, Color color)
        {
            Vector2 c = rect.center;
            float radius = rect.width * 0.44f;
            Color mark = new Color(color.r, color.g, color.b, 0.38f);
            DrawLine(c + Vector2.left * radius, c + Vector2.right * radius, mark, 1.5f);
            DrawLine(c + Vector2.up * radius, c + Vector2.down * radius, mark, 1.5f);
        }

        private static void DrawCornerLines(Rect rect, Color color)
        {
            float length = Mathf.Min(rect.width, rect.height) * 0.18f;
            Color c = new Color(color.r, color.g, color.b, 0.7f);
            DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.x + length, rect.y), c, 2f);
            DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.x, rect.y + length), c, 2f);
            DrawLine(new Vector2(rect.xMax, rect.y), new Vector2(rect.xMax - length, rect.y), c, 2f);
            DrawLine(new Vector2(rect.xMax, rect.y), new Vector2(rect.xMax, rect.y + length), c, 2f);
            DrawLine(new Vector2(rect.x, rect.yMax), new Vector2(rect.x + length, rect.yMax), c, 2f);
            DrawLine(new Vector2(rect.x, rect.yMax), new Vector2(rect.x, rect.yMax - length), c, 2f);
            DrawLine(new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax - length, rect.yMax), c, 2f);
            DrawLine(new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.yMax - length), c, 2f);
        }

        private static void DrawDiamond(Vector2 center, float radius, Color color)
        {
            Vector2 top = center + Vector2.up * radius;
            Vector2 right = center + Vector2.right * radius;
            Vector2 bottom = center + Vector2.down * radius;
            Vector2 left = center + Vector2.left * radius;
            DrawLine(top, right, color, 2f);
            DrawLine(right, bottom, color, 2f);
            DrawLine(bottom, left, color, 2f);
            DrawLine(left, top, color, 2f);
        }

        private static void DrawBorder(Rect rect, Color color, float thickness)
        {
            DrawSolid(new Rect(rect.x, rect.y, rect.width, thickness), color);
            DrawSolid(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            DrawSolid(new Rect(rect.x, rect.y, thickness, rect.height), color);
            DrawSolid(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private static void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
        {
            Matrix4x4 matrix = GUI.matrix;
            Vector2 delta = end - start;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            GUIUtility.RotateAroundPivot(angle, start);
            DrawSolid(new Rect(start.x, start.y - thickness * 0.5f, delta.magnitude, thickness), color);
            GUI.matrix = matrix;
        }

        private static void DrawCircle(Rect rect, Color color, Texture2D texture)
        {
            EnsureTextures();
            Texture2D resolvedTexture = texture != null ? texture : circleTexture;
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, resolvedTexture, ScaleMode.StretchToFill);
            GUI.color = previous;
        }

        private static void DrawSolid(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = previous;
        }

        private static Rect Inflate(Rect rect, float amount)
        {
            return new Rect(rect.x - amount, rect.y - amount, rect.width + amount * 2f, rect.height + amount * 2f);
        }

        private static Rect RectFromCenter(Vector2 center, float size)
        {
            return new Rect(center.x - size * 0.5f, center.y - size * 0.5f, size, size);
        }

        private static void EnsureStyles()
        {
            if (centerLabelStyle == null)
            {
                centerLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true
                };
                centerLabelStyle.normal.textColor = TextColor;
            }

            if (smallCenterLabelStyle == null)
            {
                smallCenterLabelStyle = new GUIStyle(centerLabelStyle);
                smallCenterLabelStyle.fontStyle = FontStyle.Normal;
            }

            if (leftLabelStyle == null)
            {
                leftLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true
                };
                leftLabelStyle.normal.textColor = TextColor;
            }

            if (titleLabelStyle == null)
            {
                titleLabelStyle = new GUIStyle(leftLabelStyle);
                titleLabelStyle.fontStyle = FontStyle.Bold;
            }
        }

        private static void ResetLabelColors()
        {
            centerLabelStyle.normal.textColor = TextColor;
            smallCenterLabelStyle.normal.textColor = TextColor;
            leftLabelStyle.normal.textColor = TextColor;
            titleLabelStyle.normal.textColor = TextColor;
        }

        private static void EnsureTextures()
        {
            if (circleTexture != null)
            {
                return;
            }

            circleTexture = CreateCircleTexture(128, 0f, 0.92f, soft: false);
            softCircleTexture = CreateCircleTexture(128, 0.25f, 1f, soft: true);
            ringTexture = CreateCircleTexture(128, 0.76f, 0.92f, soft: false);
        }

        private static Texture2D CreateCircleTexture(int size, float inner01, float outer01, bool soft)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            float center = (size - 1) * 0.5f;
            float radius = center;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / radius;
                    float alpha;
                    if (soft)
                    {
                        alpha = Mathf.Clamp01(1f - Mathf.InverseLerp(inner01, outer01, d));
                    }
                    else
                    {
                        alpha = d >= inner01 && d <= outer01 ? 1f : 0f;
                        alpha *= Mathf.Clamp01(1f - Mathf.InverseLerp(outer01, 1f, d));
                    }

                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return texture;
        }
    }
}
