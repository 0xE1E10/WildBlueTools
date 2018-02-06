﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP.IO;
using UnityEngine;

namespace WildBlueIndustries
{
    public delegate bool PerformAnalysisDelegate();
    public delegate void DrawViewDelegate();

    public class GeoLabView : Dialog<GeoLabView>
    {
        public Part part;
        public ModuleGPS gps;
        public List<PlanetaryResource> resourceList;
        public PerformAnalysisDelegate performBiomAnalysisDelegate;
        public DrawViewDelegate drawView;

        Vector2 scrollPosResources = new Vector2(0, 0);

        public GeoLabView() :
        base("<color=white>Geology Lab</color>", 300, 330)
        {
            Resizable = false;
        }

        public void DrawView()
        {
            if (HighLogic.LoadedSceneIsFlight == false)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("<color=yellow>This view is unavailable in the VAB/SPH.</color>");
                GUILayout.EndVertical();
            }
            bool biomeUnlocked = Utils.IsBiomeUnlocked(this.part.vessel);
            GUILayout.BeginVertical();

            //Location
            GUILayout.BeginScrollView(new Vector2(0, 0), new GUIStyle(GUI.skin.textArea), GUILayout.Height(110));
            GUILayout.Label("<color=white><b>Location:</b> " + gps.body + " " + gps.bioName + "</color>");
            GUILayout.Label("<color=white><b>Lon:</b> " + gps.lon + "</color>");
            GUILayout.Label("<color=white><b>Lat:</b> " + gps.lat + "</color>");
            GUILayout.EndScrollView();

            //Abundance
            drawAbundanceGUI(biomeUnlocked);

            //Extra stuff
            if (drawView != null)
                drawView();

            GUILayout.EndVertical();
        }

        protected override void DrawWindowContents(int windowId)
        {
            DrawView();
        }

        protected void drawAbundanceGUI(bool biomeUnlocked)
        {
            GUILayout.BeginVertical();
            if (biomeUnlocked)
            {
                if (resourceList.Count > 0)
                {
                    scrollPosResources = GUILayout.BeginScrollView(scrollPosResources, new GUIStyle(GUI.skin.textArea));
                    foreach (PlanetaryResource resource in resourceList)
                    {
                        GUILayout.Label("<color=white>" + resource.resourceName + " abundance: " + getAbundance(resource.resourceName) + "</color>");
                    }
                }
                else
                {
                    scrollPosResources = GUILayout.BeginScrollView(scrollPosResources, new GUIStyle(GUI.skin.textArea));
                    GUILayout.Label("<color=yellow>No detectable resources in this area.</color>");
                }
            }

            else
            {
                scrollPosResources = GUILayout.BeginScrollView(scrollPosResources, new GUIStyle(GUI.skin.textArea));
                GUILayout.Label("<color=yellow>Unlock the biome to get the resource composition.</color>");
                if (GUILayout.Button("Perform biome analysis") && performBiomAnalysisDelegate != null)
                    performBiomAnalysisDelegate();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        protected string getAbundance(string resourceName)
        {
            AbundanceRequest request = new AbundanceRequest();
            double lattitude = ResourceUtilities.Deg2Rad(this.part.vessel.latitude);
            double longitude = ResourceUtilities.Deg2Rad(this.part.vessel.longitude);

            request.BiomeName = Utils.GetCurrentBiome(this.part.vessel).name;
            request.BodyId = this.part.vessel.mainBody.flightGlobalsIndex;
            request.Longitude = longitude;
            request.Latitude = lattitude;
            request.CheckForLock = true;
            request.ResourceName = resourceName;

            float abundance = ResourceMap.Instance.GetAbundance(request) * 100.0f;

            if (abundance > 0.001)
                return string.Format("{0:f2}%", abundance);
            else
                return "Unknown";
        }
    }

}
