using IL2CPPAssetBundleAPI;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GizmosAPI
{
    public class Gizmos
    {
        private static bool? _enabled = null;
        private static bool? _overrender = null;
        private static readonly string PrefsKey = "Gizmos";

        private static GameObject Template;

        internal static void Init(IL2CPPAssetBundle Bundle)
        {
            if (Bundle == null)
            {
                return;
            }

            Template = Bundle.Load<GameObject>("Sphere");
            Template.hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        /// <summary>
        /// Toggles wether the gizmos could be drawn or not.
        /// </summary>
        internal static bool Enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = PlayerPrefs.GetInt($"{PrefsKey}.Enabled", 1) == 1;
                }

                return _enabled.Value;
            }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    PlayerPrefs.SetInt($"{PrefsKey}.Enabled", value ? 1 : 0);
                }

                Objects = Objects.Where(o => o != null).ToList();

                foreach (var Obj in Objects)
                {
                    Obj?.SetActive(value);
                }
            }
        }

        internal static bool Overrender
        {
            get
            {
                if (_overrender == null)
                {
                    _overrender = (!PlayerPrefs.HasKey($"{PrefsKey}.Overrender") || (PlayerPrefs.GetInt($"{PrefsKey}.Overrender", 1) == 1));
                }

                return _overrender.Value;
            }
            set
            {
                if (_overrender != value)
                {
                    _overrender = value;
                    PlayerPrefs.SetInt($"{PrefsKey}.Overrender", value ? 1 : 0);
                }

                Objects = Objects.Where(o => o != null).ToList();

                foreach (var Obj in Objects)
                {
                    Obj?.GetComponent<MeshRenderer>().material.SetFloat("_ZTest", value ? 0f : 4f);
                }
            }
        }

        private static float ColourInterval = 0f;
        private static float ScaleInterval = 0f;
        internal static void Update()
        {
            if (Enabled)
            {
                if (Time.time > ColourInterval)
                {
                    ColourInterval = Time.time + 0.1f;

                    foreach (var Influence in ObjectInfluences)
                    {
                        if (Influence.Item1 != null && Influence.Item2 != null)
                        {
                            var NewColour = Influence.Item3();

                            if (Influence.Item2.material.GetColor("_WireColor") != NewColour)
                            {
                                Influence.Item2.material.SetColor("_WireColor", NewColour);
                            }
                        }
                    }
                }

                if (Time.time > ScaleInterval)
                {
                    ScaleInterval = Time.time + 1f;

                    foreach (var RawScale in ObjectScales)
                    {
                        if (RawScale.Item1 != null)
                        {
                            var Scale = RawScale.Item2() * 2f;
                            var NewScale = new Vector3(Scale, Scale, Scale);

                            if (RawScale.Item1.transform.localScale != NewScale)
                            {
                                RawScale.Item1.transform.localScale = NewScale;
                            }
                        }
                    }
                }
            }
        }

        public static List<GameObject> Objects = new List<GameObject>();
        public static List<(GameObject, MeshRenderer, Func<Color>)> ObjectInfluences = new List<(GameObject, MeshRenderer, Func<Color>)>();
        public static List<(GameObject, Func<float>)> ObjectScales = new List<(GameObject, Func<float>)>();

        public static GameObject MakeOutline(MeshRenderer renderer, Func<Color> ColourInfluence = null)
        {
            var Filter = renderer?.GetComponent<MeshFilter>();

            if (Filter != null && !GetChildren(renderer.transform).Any(o => o != null && o.GetComponentInChildren<MeshRenderer>(true) is var rendererthing && rendererthing?.material?.shader?.name != null && rendererthing.material.shader.name.Contains("Outline")))
            {
                var NewObj = Object.Instantiate(Template, renderer.transform);
                NewObj.hideFlags = HideFlags.DontUnloadUnusedAsset;

                NewObj.transform.localPosition = Vector3.zero;
                NewObj.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);

                NewObj.transform.localScale = new Vector3(1f, 1f, 1f);

                NewObj.GetComponent<MeshFilter>().sharedMesh = null;
                NewObj.GetComponent<MeshFilter>().sharedMesh = Filter.sharedMesh;

                if (ColourInfluence != null)
                {
                    ObjectInfluences.Add((NewObj, NewObj?.GetComponent<MeshRenderer>(), ColourInfluence));
                    ObjectInfluences = ObjectInfluences.Where(o => o.Item1 != null).ToList();
                }

                Objects.Add(NewObj);

                Objects = Objects.Where(o => o != null).ToList();

                NewObj.SetActive(Enabled);

                if (Overrender)
                {
                    NewObj.GetComponent<MeshRenderer>().material.SetFloat("_ZTest", 0f);
                }

                return NewObj;
            }

            MelonLogger.Error("Renderer Or Filter Is Null!");

            return null;
        }

        public static GameObject MakeOutline(GameObject obj, float Radius, Color Colour)
        {
            if (obj != null && !GetChildren(obj.transform).Any(o => o != null && o.GetComponentInChildren<MeshRenderer>(true) is var rendererthing && rendererthing?.material?.shader?.name != null && rendererthing.material.shader.name.Contains("Outline")))
            {
                var NewObj = Object.Instantiate(Template, obj.transform);
                NewObj.hideFlags = HideFlags.DontUnloadUnusedAsset;

                NewObj.transform.localPosition = Vector3.zero;
                NewObj.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);

                var Scale = Radius * 2f;
                NewObj.transform.localScale = new Vector3(Scale, Scale, Scale);

                NewObj.GetComponent<MeshRenderer>().material = new Material(NewObj?.GetComponent<MeshRenderer>().material);

                NewObj.GetComponent<MeshRenderer>().material.SetColor("_WireColor", Colour);

                Objects.Add(NewObj);

                Objects = Objects.Where(o => o != null).ToList();

                NewObj.SetActive(Enabled);

                if (Overrender)
                {
                    NewObj.GetComponent<MeshRenderer>().material.SetFloat("_ZTest", 0f);
                }

                return NewObj;
            }

            MelonLogger.Error("Object Is Null!");

            return null;
        }

        public static GameObject MakeOutline(GameObject obj, float Radius, Func<Color> ColourInfluence = null)
        {
            if (obj != null && !GetChildren(obj.transform).Any(o => o != null && o.GetComponentInChildren<MeshRenderer>(true) is var rendererthing && rendererthing?.material?.shader?.name != null && rendererthing.material.shader.name.Contains("Outline")))
            {
                var NewObj = Object.Instantiate(Template, obj.transform);
                NewObj.hideFlags = HideFlags.DontUnloadUnusedAsset;

                NewObj.transform.localPosition = Vector3.zero;
                NewObj.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);

                var Scale = Radius * 2f;
                NewObj.transform.localScale = new Vector3(Scale, Scale, Scale);

                NewObj.GetComponent<MeshRenderer>().material = new Material(NewObj?.GetComponent<MeshRenderer>().material);

                if (ColourInfluence != null)
                {
                    ObjectInfluences.Add((NewObj, NewObj?.GetComponent<MeshRenderer>(), ColourInfluence));
                    ObjectInfluences = ObjectInfluences.Where(o => o.Item1 != null).ToList();
                }

                Objects.Add(NewObj);

                Objects = Objects.Where(o => o != null).ToList();

                NewObj.SetActive(Enabled);

                if (Overrender)
                {
                    NewObj.GetComponent<MeshRenderer>().material.SetFloat("_ZTest", 0f);
                }

                return NewObj;
            }

            MelonLogger.Error("Object Is Null!");

            return null;
        }

        public static GameObject MakeOutline(GameObject obj, Func<float> Radius, Func<Color> ColourInfluence = null)
        {
            if (obj != null && !GetChildren(obj.transform).Any(o => o != null && o.GetComponentInChildren<MeshRenderer>(true) is var rendererthing && rendererthing?.material?.shader?.name != null && rendererthing.material.shader.name.Contains("Outline")))
            {
                var NewObj = Object.Instantiate(Template, obj.transform);
                NewObj.hideFlags = HideFlags.DontUnloadUnusedAsset;

                NewObj.transform.localPosition = Vector3.zero;
                NewObj.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);

                var Scale = Radius() * 2f;
                ObjectScales.Add((NewObj, Radius));
                ObjectScales = ObjectScales.Where(o => o.Item1 != null).ToList();

                NewObj.transform.localScale = new Vector3(Scale, Scale, Scale);

                if (ColourInfluence != null)
                {
                    ObjectInfluences.Add((NewObj, NewObj?.GetComponent<MeshRenderer>(), ColourInfluence));
                }

                Objects.Add(NewObj);

                Objects = Objects.Where(o => o != null).ToList();

                NewObj.SetActive(Enabled);

                if (Overrender)
                {
                    NewObj.GetComponent<MeshRenderer>().material.SetFloat("_ZTest", 0f);
                }

                return NewObj;
            }

            MelonLogger.Error("Object Is Null!");

            return null;
        }

        private static List<Transform> GetChildren(Transform Parent)
        {
            var Children = new List<Transform>();

            for (var i = 0; i < Parent.childCount; i++)
            {
                Children.Add(Parent.GetChild(i));
            }

            return Children;
        }
    }
}