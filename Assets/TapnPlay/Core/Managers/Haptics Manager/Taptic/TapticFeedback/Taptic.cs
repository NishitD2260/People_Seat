using System.Runtime.InteropServices;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Taptic : MonoBehaviour
{
        public static bool tapticOn = true;

        public static void Warning()
        {
                if (!tapticOn || Application.isEditor)
                {
                        return;
                }
                AndroidTaptic.Haptic(HapticTypes.Warning);
        }
        public static void Failure()
        {
                if (!tapticOn || Application.isEditor)
                {
                        return;
                }
                AndroidTaptic.Haptic(HapticTypes.Failure);
        }
        public static void Success()
        {
                if (!tapticOn || Application.isEditor)
                {
                        return;
                }
                AndroidTaptic.Haptic(HapticTypes.Success);
        }
        public static void Light()
        {
                if (!tapticOn || Application.isEditor)
                {
                        return;
                }
                AndroidTaptic.Haptic(HapticTypes.LightImpact);
        }
        public static void Medium()
        {
                if (!tapticOn || Application.isEditor)
                {
                        return;
                }
                AndroidTaptic.Haptic(HapticTypes.MediumImpact);
        }
        public static void Heavy()
        {
                if (!tapticOn || Application.isEditor)
                {
                        return;
                }
                AndroidTaptic.Haptic(HapticTypes.HeavyImpact);
        }
        public static void Default()
        {
                if (!tapticOn || Application.isEditor)
                {
                        return;
                }
#if UNITY_IOS || UNITY_ANDROID
                Handheld.Vibrate();
#endif
        }
        public static void Vibrate()
        {
                if (!tapticOn || Application.isEditor)
                {
                        return;
                }
                AndroidTaptic.Vibrate();
        }
        public static void Selection()
        {
                if (!tapticOn || Application.isEditor)
                {
                        return;
                }
                AndroidTaptic.Haptic(HapticTypes.Selection);
        }
}