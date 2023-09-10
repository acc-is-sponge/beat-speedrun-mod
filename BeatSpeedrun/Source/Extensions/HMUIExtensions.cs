using HMUI;
using IPA.Utilities;
using UnityEngine;

namespace BeatSpeedrun.Extensions
{
    public static class HMUIImageViewExtensions
    {
        public static void SetGradient(this ImageView view, Color a, Color b)
        {
            view.material = BeatSaberMarkupLanguage.Utilities.ImageResources.NoGlowMat;
            view.color0 = a;
            view.color1 = b;
            ImageGradient(ref view) = true;
        }

        public static void SetSkew(this ImageView view, float skew)
        {
            IamgeSkew(ref view) = skew;
        }

        private static readonly FieldAccessor<ImageView, bool>.Accessor ImageGradient = FieldAccessor<ImageView, bool>.GetAccessor("_gradient");
        private static readonly FieldAccessor<ImageView, float>.Accessor IamgeSkew = FieldAccessor<ImageView, float>.GetAccessor("_skew");
    }
}
