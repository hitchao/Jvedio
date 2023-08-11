using Jvedio.Core.Global;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using static Jvedio.App;

namespace Jvedio.Core
{
    /// <summary>
    /// 自定义 user.config 存储路径
    /// <para>参考：<see href="https://stackoverflow.com/questions/2265271/custom-path-of-the-user-config">stackoverflow</see></para>
    /// </summary>
    class CustomSettingsProvider : SettingsProvider
    {

        private const string NAME = "name";
        private const string SERIALIZE_AS = "serializeAs";
        private const string CONFIG = "configuration";
        private const string USER_SETTINGS = "userSettings";
        private const string SETTING = "setting";

        #region "属性"


        /// <summary>
        /// user.config 存储路径
        /// </summary>
        private static string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data",
                Environment.UserName, "user-config.xml");

        private bool _loaded { get; set; }


        /// <summary>
        /// The setting key this is returning must set before the settings are used.
        /// e.g. <c>Properties.Settings.Default.SettingsKey = @"C:\temp\user.config";</c>
        /// </summary>
        private string UserConfigPath {
            get {
                return configPath;
            }
        }

        /// <summary>
        /// In memory storage of the settings values
        /// </summary>
        private Dictionary<string, SettingStruct> SettingsDictionary { get; set; }


        #endregion

        /// <summary>
        /// Loads the file into memory.
        /// </summary>
        public CustomSettingsProvider()
        {
            SettingsDictionary = new Dictionary<string, SettingStruct>();
        }

        /// <summary>
        /// Override.
        /// </summary>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(ApplicationName, config);
        }

        /// <summary>
        /// Override.
        /// </summary>
        public override string ApplicationName {
            get {
                return System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name;
            }

            set {
                // do nothing
            }
        }

        /// <summary>
        /// Must override this, this is the bit that matches up the designer properties to the dictionary values
        /// </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            // load the file
            if (!_loaded) {
                _loaded = true;
                LoadValuesFromFile();
            }

            // collection that will be returned.
            SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();

            // iterate thought the properties we get from the designer, checking to see if the setting is in the dictionary
            foreach (SettingsProperty setting in collection) {
                SettingsPropertyValue value = new SettingsPropertyValue(setting);
                value.IsDirty = false;

                // need the type of the value for the strong typing
                var t = Type.GetType(setting.PropertyType.FullName);

                if (SettingsDictionary.ContainsKey(setting.Name)) {
                    value.SerializedValue = SettingsDictionary[setting.Name].value;
                    value.PropertyValue = Convert.ChangeType(SettingsDictionary[setting.Name].value, t);
                } else // use defaults in the case where there are no settings yet
                  {
                    value.SerializedValue = setting.DefaultValue;
                    value.PropertyValue = Convert.ChangeType(setting.DefaultValue, t);
                }

                values.Add(value);
            }

            return values;
        }

        /// <summary>
        /// Must override this, this is the bit that does the saving to file.  Called when Settings.Save() is called
        /// </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            // grab the values from the collection parameter and update the values in our dictionary.
            foreach (SettingsPropertyValue value in collection) {
                var setting = new SettingStruct() {
                    value = value.PropertyValue == null ? string.Empty : value.PropertyValue.ToString(),
                    name = value.Name,
                    serializeAs = value.Property.SerializeAs.ToString(),
                };

                if (!SettingsDictionary.ContainsKey(value.Name)) {
                    SettingsDictionary.Add(value.Name, setting);
                } else {
                    SettingsDictionary[value.Name] = setting;
                }
            }

            // now that our local dictionary is up-to-date, save it to disk.
            SaveValuesToFile();
        }

        /// <summary>
        /// Loads the values of the file into memory.
        /// </summary>
        private void LoadValuesFromFile()
        {
            if (!File.Exists(UserConfigPath)) {
                try {
                    Directory.CreateDirectory(PathManager.CurrentUserFolder);
                } catch (Exception ex) {
                    Logger.Error(ex);
                }

                // if the config file is not where it's supposed to be create a new one.
                CreateEmptyConfig();
            }

            // load the xml
            var configXml = XDocument.Load(UserConfigPath);

            // get all of the <setting name="..." serializeAs="..."> elements.
            var settingElements = configXml.Element(CONFIG).Element(USER_SETTINGS).Element(typeof(Properties.Settings).FullName).Elements(SETTING);

            // iterate through, adding them to the dictionary, (checking for nulls, xml no liked nulls)
            // using "String" as default serializeAs...just in case, no real good reason.
            foreach (var element in settingElements) {
                var newSetting = new SettingStruct() {
                    name = element.Attribute(NAME) == null ? string.Empty : element.Attribute(NAME).Value,
                    serializeAs = element.Attribute(SERIALIZE_AS) == null ? "String" : element.Attribute(SERIALIZE_AS).Value,
                    value = element.Value ?? string.Empty,
                };
                SettingsDictionary.Add(element.Attribute(NAME).Value, newSetting);
            }
        }

        /// <summary>
        /// Creates an empty user.config file...looks like the one MS creates.
        /// This could be overkill a simple key/value pairing would probably do.
        /// </summary>
        private void CreateEmptyConfig()
        {
            var doc = new XDocument();
            var declaration = new XDeclaration("1.0", "utf-8", "true");
            var config = new XElement(CONFIG);
            var userSettings = new XElement(USER_SETTINGS);
            var group = new XElement(typeof(Properties.Settings).FullName);
            userSettings.Add(group);
            config.Add(userSettings);
            doc.Add(config);
            doc.Declaration = declaration;
            doc.Save(UserConfigPath);
        }

        /// <summary>
        /// Saves the in memory dictionary to the user config file
        /// </summary>
        private void SaveValuesToFile()
        {
            // load the current xml from the file.
            var import = XDocument.Load(UserConfigPath);

            // get the settings group (e.g. <Company.Project.Desktop.Settings>)
            var settingsSection = import.Element(CONFIG).Element(USER_SETTINGS).Element(typeof(Properties.Settings).FullName);

            // iterate though the dictionary, either updating the value or adding the new setting.
            foreach (var entry in SettingsDictionary) {
                var setting = settingsSection.Elements().FirstOrDefault(e => e.Attribute(NAME).Value == entry.Key);
                if (setting == null) // this can happen if a new setting is added via the .settings designer.
                {
                    var newSetting = new XElement(SETTING);
                    newSetting.Add(new XAttribute(NAME, entry.Value.name));
                    newSetting.Add(new XAttribute(SERIALIZE_AS, entry.Value.serializeAs));
                    newSetting.Value = entry.Value.value ?? string.Empty;
                    settingsSection.Add(newSetting);
                } else // update the value if it exists.
                  {
                    setting.Value = entry.Value.value ?? string.Empty;
                }
            }

            import.Save(UserConfigPath);
        }

        /// <summary>
        /// Helper struct.
        /// </summary>
        internal struct SettingStruct
        {
            internal string name;
            internal string serializeAs;
            internal string value;
        }


    }
}
