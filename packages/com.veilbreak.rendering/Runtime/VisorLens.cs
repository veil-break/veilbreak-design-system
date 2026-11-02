using UnityEngine;

namespace Veilbreak.Rendering
{
    // The curve of the helmet visor (guide/06-visor.md): one barrel mapping shared by everything that draws or reads the screen,
    // so the scene, the UI and input agree. VisorLensFeature warps the main camera with it, a UI filter can warp the panel with
    // the same formula, and hosts convert pointer positions with Source before UI Toolkit picking or world raycasts.
    // A screen point shows the image from Source(point): the centre is magnified by 1 + Barrel and the rim compressed, with the
    // four corners fixed, so nothing leaves the screen. The host owns the configuration: Barrel (token fx.visor-barrel, 0 is the
    // identity everywhere, so the camera pass stands by) and FirstQuality (the first quality level that bends; lower levels
    // keep a flat view and the identity mapping).
    public static class VisorLens
    {
        public static float Barrel;
        public static int FirstQuality;

        public static bool Active => Barrel > 0 && QualitySettings.GetQualityLevel() >= FirstQuality;
        public static float Strength => Active ? Barrel : 0;

        // Screen pixels (either y origin: the mapping is symmetric about the centre) to the source pixels shown there.
        public static Vector2 Source(Vector2 screen) => Source(screen, new Vector2(Screen.width, Screen.height), Strength);

        public static Vector2 Source(Vector2 screen, Vector2 size, float k)
        {
            if (k <= 0 || size.x <= 0 || size.y <= 0) return screen;
            var half = size / 2; var d = (screen - half) / half.y;
            var aspect = size.x / size.y; var r2 = Vector2.Dot(d, d) / (aspect * aspect + 1);
            return half + (screen - half) * ((1 + k * r2) / (1 + k));
        }

        // The inverse: where the source pixel `source` appears on screen (fixed-point iteration; converges in a few steps).
        public static Vector2 Shown(Vector2 source) => Shown(source, new Vector2(Screen.width, Screen.height), Strength);

        public static Vector2 Shown(Vector2 source, Vector2 size, float k)
        {
            if (k <= 0 || size.x <= 0 || size.y <= 0) return source;
            var half = size / 2; var aspect = size.x / size.y; var screen = source;
            for (var i = 0; i < 6; i++)
            {
                var d = (screen - half) / half.y; var r2 = Vector2.Dot(d, d) / (aspect * aspect + 1);
                screen = half + (source - half) * ((1 + k) / (1 + k * r2));
            }
            return screen;
        }
    }
}
