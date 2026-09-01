using HarmonyLib;
using MGSC;
using ModConfigMenu.Contracts;
using ModConfigMenu.Implementations;
using ModConfigMenu.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using BetterScatterAngle;


namespace BetterScatterAngle.Mcm
{

    internal abstract class McmConfigurationBase(ISave config)
    {

        //For internal version tracking.  For compatibility this class is copied between mods to avoid needing to update all mods when a change is made.
        //********************* CHANGE THE VERSION IF YOU MAKE CHANGES TO THIS FILE *********************
        //private static string Version = "1.0";

        protected const string WarningColorElement = "<color=#FBE343>";
        /// <summary>
        /// The common warning about the number keys being prefixed with Alpha.
        /// </summary>
        protected const string KeyCodeAlphaNote = $"{WarningColorElement}Note: The keys 1 through 0 are named Alpha1 through Alpha0.</color>";

        public ISave Config { get; set; } = config;

        /// <summary>
        /// Used to set the defaults established by the ModConfig class.
        /// </summary>
        private ModConfig Defaults { get; set; } = new ModConfig();

        /// <summary>
        /// Used to make the keys for read only entries unique.
        /// </summary>
        private static int UniqueId = 0;

        /// <summary>
        /// Attempts to configure the MCM, but logs an error and continues if it fails.
        /// </summary>
        public bool TryConfigure()
        {
            try
            {
                Configure();
                return true;
            }
            catch (FileNotFoundException)
            {
                Plugin.Logger.Log("Bypassing MCM. The 'Mod Configuration Menu' mod is not loaded. ");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex, "An error occurred when configuring MCM");
            }

            return false;
        }







        /// <summary>
        /// The ModConfig specific configuration.  Use the Create* and OnSave helper functions.
        /// </summary>
        public abstract void Configure();




        /// <summary>
        /// Creates a dropdown config for an enum type.
        /// </summary>
        /// <typeparam name="T">The object type to extract the data from</typeparam>
        /// <typeparam name="TEnum">The target enum.</typeparam>
        /// <param name="propertyName">The name of the property to use for the value.</param>
        /// <param name="tooltip"></param>
        /// <param name="label"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        internal IConfigValue CreateEnumDropdown<TEnum>(string propertyName, string tooltip, string label, string header = "General", bool sort = false)
            where TEnum : Enum
        {

            object defaultValue = AccessTools.Property(typeof(ModConfig), propertyName).GetValue(Defaults);
            object propertyValue = AccessTools.Property(typeof(ModConfig), propertyName).GetValue(Config);

            List<object> enumNames;

            if (sort)
            {
                enumNames = Enum.GetNames(typeof(TEnum)).OrderBy(x => x).ToList<object>();
            }
            else
            {
                enumNames = Enum.GetNames(typeof(TEnum)).ToList<object>();
            }

            var dropDown = new DropdownConfig(propertyName, propertyValue.ToString(), header, defaultValue.ToString(), tooltip, label, enumNames);

            return dropDown;
        }

        protected ConfigValue CreateRestartMessage()
        {
            return new ConfigValue("__Restart", "<color=#FF0000>The game must be restarted for any changes to take effect.", "Restart");

        }

        /// <summary>
        /// Formats a string with no spaces to having spaces before each uppercase letter.
        /// Used to make a property name easier to read.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static string FormatUpperCaseSpaces(string propertyName)
        {
            //Since the UI uppercases the text, add spaces to make it easier to read.
            Regex regex = new Regex(@"([A-Z0-9])");
            string formattedPropertyName = regex.Replace(propertyName.ToString(), " $1").TrimStart();
            return formattedPropertyName;
        }

        protected ConfigValue CreateConfigProperty<T>(string propertyName, string tooltip, string label, T min, T max, string header = "General") where T : struct
        {
            T defaultValue = (T)AccessTools.Property(typeof(ModConfig), propertyName).GetValue(Defaults);
            T propertyValue = (T)AccessTools.Property(typeof(ModConfig), propertyName).GetValue(Config);

            switch (typeof(T))
            {
                case Type floatType when floatType == typeof(float):
                case Type intType when intType == typeof(int):
                    return new ConfigValue(
                        propertyName,
                        propertyValue,
                        header,
                        defaultValue,
                        tooltip,
                        label,
                        Convert.ToSingle(min),
                        Convert.ToSingle(max)
                    );
                default:
                    throw new ApplicationException($"Unexpected numeric type '{typeof(T).Name}'");
            }
        }


        /// <summary>
        /// Creates a configuration value.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="tooltip"></param>
        /// <param name="label">If not set, will use the property name, adding spaced before each capital letter.</param>
        /// <param name="header"></param>
        /// <returns></returns>
        protected ConfigValue CreateConfigProperty(string propertyName, string tooltip, string label = "", string header = "General")
        {
            object defaultValue = AccessTools.Property(typeof(ModConfig), propertyName).GetValue(Defaults);
            object propertyValue = AccessTools.Property(typeof(ModConfig), propertyName).GetValue(Config);

            string formattedLabel = label == "" ? FormatUpperCaseSpaces(propertyName) : label;

            return new ConfigValue(propertyName, propertyValue, header, defaultValue, tooltip, formattedLabel);
        }

        /// <summary>
        /// Sets the ModConfig's property that matches the ConfigValue key.
        /// Returns false if the property could not be found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        protected bool SetModConfigValue(string key, object value)
        {
            MethodInfo propertyMethod = AccessTools.Property(typeof(ModConfig), key)?.GetSetMethod();

            Action<object> setMethod = null;   //The action to set the property or field's value.


            Type propertyType = null;

            //Try property
            if (propertyMethod != null)
            {
                setMethod = (value) => propertyMethod.Invoke(Config, new object[] { value });
                propertyType = propertyMethod.GetParameters()[0].ParameterType;
            }
            //Try field
            else
            {
                FieldInfo field = AccessTools.Field(typeof(ModConfig), key);
                if (field == null)
                {
                    return false;
                }

                setMethod = (value) => { field.SetValue(Config, value); };
                propertyType = field.FieldType;
            }


            if (propertyType.IsEnum)
            {
                //Convert from string to enum
                object enumValue = Enum.Parse(propertyType, value.ToString());
                setMethod(enumValue);
            }
            else
            {
                //todo: I don't think this is required
                ////Change type if needed
                //object convertedValue = Convert.ChangeType(value, propertyType);
                setMethod(value);
            }

            return true;
        }

        protected virtual bool OnSave(Dictionary<string, object> currentConfig, out string feedbackMessage)
        {
            feedbackMessage = "";

            foreach (var entry in currentConfig)
            {
                SetModConfigValue(entry.Key, entry.Value);
            }

            Config.Save();

            return true;
        }
    }
}
