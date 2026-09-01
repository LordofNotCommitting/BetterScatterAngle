using HarmonyLib;
using ModConfigMenu;
using ModConfigMenu.Contracts;
using ModConfigMenu.Objects;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace BetterScatterAngle.Mcm
{
    internal class McmConfiguration : McmConfigurationBase
    {

        public McmConfiguration(ModConfig config) : base (config) { }

        public override void Configure()
        {
            ModConfig defaults = new ModConfig();

            ModConfigMenuAPI.RegisterModConfig("Accurate Scatter", new List<IConfigValue>()
            {
                new ConfigValue("__RestartNote", @"<color=#FF0000>The game must be restarted if any changes are made.</color>" , "Restart"),


                 CreateConfigProperty<int>(
                    nameof(ModConfig.Scatter_Range_Sample),
                    "Scatter Distance Tile Sample range to simulate better scattering. Default: " + ModConfig.Scatter_Range_Sample_Array[0],
                    "Scatter Distance Tile Sample",
                    ModConfig.Scatter_Range_Sample_Array[1],
                    ModConfig.Scatter_Range_Sample_Array[2]
                ),

                CreateConfigProperty<float>(
                    nameof(ModConfig.Scatter_Penalty_Perc),
                    "To compensate guns being more accurate due to this mod, Penalty can be introduced. This value add % based penalty to Scatter. Default: " + ModConfig.Scatter_Penalty_Perc_Array[0],
                    "Scatter Penalty %",
                    ModConfig.Scatter_Penalty_Perc_Array[1],
                    ModConfig.Scatter_Penalty_Perc_Array[2]
                ),


                CreateConfigProperty<float>(
                    nameof(ModConfig.Scatter_Penalty_Flat),
                    "To compensate guns being more accurate due to this mod, Penalty can be introduced. This value add flat scatter (1:1 degree) as penalty to scatter. Default: " + ModConfig.Scatter_Penalty_Flat_Array[0],
                    "Scatter Penalty Flat",
                    ModConfig.Scatter_Penalty_Flat_Array[1],
                    ModConfig.Scatter_Penalty_Flat_Array[2]
                ),

                new ConfigValue("__ScatterPenaltyNote", @"Scatter Penalty Total = (Final Scatter * (100% + Scatter penalty %)) + Scatter penalty Flat" , "General"),


                new ConfigValue("__GaussianNote1", @"Gaussian Bullet Distribution makes scatter less random and focus bullet on central cone" , "Gaussian Scatter"),
                new ConfigValue("__GaussianNote2", @"Vanilla : [ . . . . . . . . . . . . . ]" , "Gaussian Scatter"),
                new ConfigValue("__GaussianNote3", @"Gaussian: [   .  . : : ::: : :  .  .   ]" , "Gaussian Scatter"),

                CreateConfigProperty(nameof(ModConfig.Scatter_Gaussian_Calculation),
                    "Enable Gaussian Scatter which should focus bullet in central area",
                    "Enable Gaussian Scatter",
                    "Gaussian Scatter"
                ),

                CreateConfigProperty<float>(
                    nameof(ModConfig.Scatter_Gaussian_StdDev),
                    "Gaussian Scatter Standard Deviation modifier value... Which makes bullet more centered the higher this value is. Default: " + ModConfig.Scatter_Gaussian_StdDev_Array[0],
                    "Gaussian Scatter Standard Deviation",
                    ModConfig.Scatter_Gaussian_StdDev_Array[1],
                    ModConfig.Scatter_Gaussian_StdDev_Array[2],
                    "Gaussian Scatter"
                ),

                





    }, OnSave);
        }
         
    }
}
