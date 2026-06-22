using IniParser.Model;

namespace ShinRyuModManager.Templates
{
    public static class IniTemplate
    {
        public static IniData NewIni()
        {
            var data = new IniData
            {
                Configuration =
                {
                    AssigmentSpacer = string.Empty
                }
            };
            
            return UpdateIni(data);
        }
        
        public static IniData UpdateIni(IniData data)
        {
            var sections = ParlessIni.GetParlessSections();
            
            var sectionList = new SectionDataCollection();
            
            foreach (var section in sections)
            {
                var newSecData = new SectionData(section.Name);
                newSecData.Comments.AddRange(section.Comments.Select(c => " " + c));
                
                // Get existing SectionData
                // Create a new section if it did not exist
                var secData = data.Sections.GetSectionData(section.Name) ?? new SectionData(section.Name);
                
                // Clear old comments for the section and its keys
                secData.ClearComments();
                
                foreach (var key in section.Keys)
                {
                    // Create a new key with the default value if the key did not exist
                    var keyData = secData.Keys.GetKeyData(key.Name) ?? new KeyData(key.Name)
                    {
                        Value = key.DefaultValue.ToString()
                    };
                    
                    keyData.Comments.AddRange(key.Comments.Select(c => " " + c));
                    keyData.Comments.Add(" Default=" + key.DefaultValue);
                    
                    newSecData.Keys.AddKey(keyData);
                }
                
                sectionList.SetSectionData(section.Name, newSecData);
            }
            
            data.Sections = sectionList;
            
            // Update the ini version
            data["Parless"]["IniVersion"] = ParlessIni.CURRENT_VERSION.ToString();
            
            return data;
        }
    }
}
