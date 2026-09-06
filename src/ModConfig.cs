using BetterScatterAngle;
using BetterScatterAngle.Mcm;
using MGSC;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BetterScatterAngle
{
    public class ModConfig : PersistentConfig<ModConfig>
    {
        public ModConfig()
        {
        }

        public ModConfig(string configPath) : base(configPath)
        {

        }



        // ====== combined ======
        // default, min, max value respectively
        public static int[] Scatter_Range_Sample_Array = new int[] { 50, 20, 100 };
        public static int[] Scatter_Penalty_Perc_Array = new int[] { 0, 0, 300 };
        public static float[] Scatter_Penalty_Flat_Array = new float[] { 0f, 0f, 100f };
        public static float[] Scatter_Gaussian_StdDev_Array = new float[] { 2f, 1f, 30f };
        public static float[] Hitbox_Base_Target_Size_Array = new float[] { 0.5f, 0.35f, 0.71f };

        public int Scatter_Range_Sample { get; set; } = Scatter_Range_Sample_Array[0];
        public int Scatter_Penalty_Perc_int { get; set; } = Scatter_Penalty_Perc_Array[0];
        public float Scatter_Penalty_Flat { get; set; } = (float)Scatter_Penalty_Flat_Array[0];

        public bool Scatter_Gaussian_Calculation { get; set; } = false;

        public float Scatter_Gaussian_StdDev { get; set; } = (float)Scatter_Gaussian_StdDev_Array[0];


        public bool Hitbox_Base_Targetting { get; set; } = false;
        public float Hitbox_Base_Target_Size { get; set; } = (float)Hitbox_Base_Target_Size_Array[0];

        public void Save(string configPath)
        {
            string json = JsonConvert.SerializeObject(this, SerializerSettings);
            File.WriteAllText(Plugin.ConfigDirectories.ConfigPath, json);
        }
    }
}

