using System.Linq;
using System.Collections.Generic;
using IniParser.Model;

namespace ShinRyuModManager.Templates
{
    public static class IniTemplate
    {
        public static IniData NewIni()
        {
            IniData data = new IniData();
            data.Configuration.AssigmentSpacer = string.Empty;
            return UpdateIni(data);
        }

        public static IniData UpdateIni(IniData data)
        {
            List<IniSection> sections = ParlessIni.GetParlessSections();

            SectionDataCollection sectionList = new SectionDataCollection();

            foreach (IniSection section in sections)
            {
                SectionData newSecData = new SectionData(section.Name);
                newSecData.Comments.AddRange(section.Comments.Select(c => " " + c));

                // Get existing SectionData
                SectionData secData = data.Sections.GetSectionData(section.Name);

                if (secData == null)
                {
                    // Create a new section if it did not exist
                    secData = new SectionData(section.Name);
                }

                // Clear old comments for the section and its keys
                secData.ClearComments();

                foreach (IniKey key in section.Keys)
                {
                    KeyData keyData = secData.Keys.GetKeyData(key.Name);

                    if (keyData == null)
                    {
                        // Create a new key with the default value if the key did not exist
                        keyData = new KeyData(key.Name);
                        keyData.Value = key.DefaultValue.ToString();
                    }

                    keyData.Comments.AddRange(key.Comments.Select(c => " " + c));
                    keyData.Comments.Add(" Default=" + key.DefaultValue);

                    newSecData.Keys.AddKey(keyData);
                }

                sectionList.SetSectionData(section.Name, newSecData);
            }

            data.Sections = sectionList;

            // Update the ini version
            data["Parless"]["IniVersion"] = ParlessIni.CurrentVersion.ToString();

            return data;
        }
    }
}
