using IL2CPPAssetBundleAPI;
using MelonLoader;
using PlagueButtonAPI;
using PlagueButtonAPI.Pages;
using UnityEngine;

[assembly: MelonInfo(typeof(GizmosAPI.MainClass), "GizmosAPI", "1.0", "Plague")]
[assembly: MelonGame]

namespace GizmosAPI
{
    public class MainClass : MelonMod
    {
        private IL2CPPAssetBundle Bundle = new IL2CPPAssetBundle();

        public override void OnApplicationStart()
        {
            if (Bundle.LoadBundle("GizmosAPI.Resources.outlines.assetbundle"))
            {
                Gizmos.Init(Bundle);
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "ui")
            {
                ButtonAPI.OnInit += () =>
                {
                    var Page = MenuPage.CreatePage(PlagueButtonAPI.Controls.WingSingleButton.Wing.Left, null, "GizmosMenu", "Gizmos API", Gridify: true).Item1;

                    Page.AddToggleButton("Visible", "Toggles Whether The Gizmos Are Visible", "Toggles Whether The Gizmos Are Visible", (val) =>
                    {
                        Gizmos.Enabled = val;
                    }, Gizmos.Enabled);

                    Page.AddToggleButton("Overrender", "Toggles Whether The Gizmos Overrender Everything They Can", "Toggles Whether The Gizmos Overrender Everything They Can", (val) =>
                    {
                        Gizmos.Overrender = val;
                    }, Gizmos.Overrender);
                };
            }
        }

        public override void OnUpdate()
        {
            Gizmos.Update();
        }
    }
}
