using System.Reflection;
using System.Text.Json;
using XFEExtension.NetCore.FormatExtension;

namespace XFEExtension.NetCore.AutoConfig;

/// <summary>
/// XFE配置文件，实现配置文件读写自动化
/// </summary>
public abstract class XFEProfile
{
    private static Func<object?, ProfileEntryInfo, string> SaveProfilesFunc { get; set; } = (i, p) =>
    {
        if (p.MemberInfo is FieldInfo fieldInfo)
            return JsonSerializer.Serialize(fieldInfo.GetValue(i));
        else if (p.MemberInfo is PropertyInfo propertyInfo)
            return JsonSerializer.Serialize(propertyInfo.GetValue(i));
        else
            return string.Empty;
    };
    private static Func<string, ProfileEntryInfo, object?> LoadProfilesFunc { get; set; } = (x, p) =>
    {
        if (p.MemberInfo is FieldInfo fieldInfo)
            return JsonSerializer.Deserialize(x, fieldInfo.FieldType);
        else if (p.MemberInfo is PropertyInfo propertyInfo)
            return JsonSerializer.Deserialize(x, propertyInfo.PropertyType);
        else
            return null;
    };

    /// <summary>
    /// 配置文件清单
    /// </summary>
    public static List<ProfileInfo> Profiles { get; private set; } = [];

    /// <summary>
    /// 配置文件所在的默认目录
    /// </summary>
    public static string ProfilesDefaultPath { get; set; } = $"{AppDomain.CurrentDomain.BaseDirectory}/Profiles";

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <param name="profileInfo">配置文件信息</param>
    /// <returns></returns>
    public static void LoadProfiles(params ProfileInfo[] profileInfo)
    {
        Profiles.AddRange(profileInfo);
        foreach (var profile in Profiles)
        {
            var instance = profile.GetProfileInstance();
            if (!File.Exists(profile.Path))
                continue;
            XFEDictionary propertyFileContent = File.ReadAllText(profile.Path);
            for (int i = 0; i < profile.MemberInfo.Count; i++)
            {
                for (int j = 0; j < propertyFileContent.Count; j++)
                {
                    var memberInfo = profile.MemberInfo[i];
                    var property = propertyFileContent.ElementAt(j);
                    if (property.Header == memberInfo.Name)
                    {
                        if (memberInfo.MemberInfo is FieldInfo fieldInfo)
                            fieldInfo.SetValue(instance, LoadProfilesFunc(property.Content, memberInfo));
                        else if (memberInfo.MemberInfo is PropertyInfo propertyInfo)
                            propertyInfo.SetValue(instance, LoadProfilesFunc(property.Content, memberInfo));
                        continue;
                    }
                    foreach (var propertySecFind in propertyFileContent)
                    {
                        if (propertySecFind.Header == memberInfo.Name)
                        {
                            if (profile.MemberInfo[i].MemberInfo is FieldInfo fieldInfo)
                                fieldInfo.SetValue(instance, LoadProfilesFunc(propertySecFind.Content, memberInfo));
                            else if (profile.MemberInfo[i].MemberInfo is PropertyInfo propertyInfo)
                                propertyInfo.SetValue(instance, LoadProfilesFunc(propertySecFind.Content, memberInfo));
                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <param name="profileInfo">配置文件信息</param>
    /// <returns></returns>
    public static async Task LoadProfilesAsync(params ProfileInfo[] profileInfo) => await Task.Run(() => LoadProfiles(profileInfo));

    /// <summary>
    /// 储存指定的配置文件
    /// </summary>
    /// <param name="profileInfo">配置文件</param>
    /// <returns></returns>
    public static void SaveProfile(ProfileInfo profileInfo)
    {
        var waitSaveProfile = Profiles.Find(x => x.Profile == profileInfo.Profile);
        if (waitSaveProfile is null)
            return;
        var saveProfileDictionary = new XFEDictionary();
        var instance = profileInfo.GetProfileInstance();
        foreach (var property in waitSaveProfile.MemberInfo)
        {
            saveProfileDictionary.Add(property.Name, SaveProfilesFunc(instance, property));
        }
        var fileSavePath = Path.GetDirectoryName(waitSaveProfile.Path);
        if (!Directory.Exists(fileSavePath) && fileSavePath is not null && fileSavePath != string.Empty)
            Directory.CreateDirectory(fileSavePath);
        File.WriteAllText(waitSaveProfile.Path, saveProfileDictionary);
    }

    /// <summary>
    /// 储存配置文件
    /// </summary>
    /// <returns></returns>
    public static void SaveProfiles()
    {
        foreach (var profile in Profiles)
            SaveProfile(profile);
    }

    /// <summary>
    /// 储存指定的配置文件
    /// </summary>
    /// <param name="profileInfo">配置文件</param>
    /// <returns></returns>
    public static async Task SaveProfileAsync(ProfileInfo profileInfo)
    {
        var waitSaveProfile = Profiles.Find(x => x.Profile == profileInfo.Profile);
        if (waitSaveProfile is null)
            return;
        var saveProfileDictionary = new XFEDictionary();
        var instance = profileInfo.GetProfileInstance();
        foreach (var property in waitSaveProfile.MemberInfo)
            saveProfileDictionary.Add(property.Name, SaveProfilesFunc(instance, property));
        var fileSavePath = Path.GetDirectoryName(waitSaveProfile.Path);
        if (!Directory.Exists(fileSavePath) && fileSavePath is not null && fileSavePath != string.Empty)
            Directory.CreateDirectory(fileSavePath);
        await File.WriteAllTextAsync(waitSaveProfile.Path, saveProfileDictionary);
    }

    /// <summary>
    /// 储存配置文件
    /// </summary>
    /// <returns></returns>
    public static async Task SaveProfilesAsync()
    {
        foreach (var profile in Profiles)
            await SaveProfileAsync(profile);
    }

    /// <summary>
    /// 删除指定的配置文件
    /// </summary>
    /// <param name="profileInfo">指定的配置文件</param>
    public static void DeleteProfile(ProfileInfo profileInfo)
    {
        var waitSaveProfile = Profiles.Find(x => x.Profile == profileInfo.Profile);
        if (waitSaveProfile is null)
            return;
        if (File.Exists(waitSaveProfile.Path))
            File.Delete(waitSaveProfile.Path);
    }

    /// <summary>
    /// 删除指定的配置文件
    /// </summary>
    /// <param name="profileInfo">指定的配置文件</param>
    /// <returns></returns>
    public static async Task DeleteProfileAsync(ProfileInfo profileInfo)
    {
        await Task.Run(() =>
        {
            var waitSaveProfile = Profiles.Find(x => x.Profile == profileInfo.Profile);
            if (waitSaveProfile is null)
                return;
            if (File.Exists(waitSaveProfile.Path))
                File.Delete(waitSaveProfile.Path);
        });
    }

    /// <summary>
    /// 删除所有配置文件
    /// </summary>
    public static void DeleteProfiles()
    {
        foreach (var profile in Profiles)
            DeleteProfile(profile);
    }

    /// <summary>
    /// 删除所有配置文件
    /// </summary>
    /// <returns></returns>
    public static async Task DeleteProfilesAsync()
    {
        foreach (var profile in Profiles)
            await DeleteProfileAsync(profile);
    }

    /// <summary>
    /// 设置储存配置文件的方法
    /// </summary>
    /// <param name="saveProfilesFunc">储存方法</param>
    public static void SetSaveProfilesFunction(Func<object?, ProfileEntryInfo, string> saveProfilesFunc) => SaveProfilesFunc = saveProfilesFunc;

    /// <summary>
    /// 设置加载配置文件的方法
    /// </summary>
    /// <param name="loadProfilesFunc">加载方法</param>
    public static void SetLoadProfilesFunction(Func<string, ProfileEntryInfo, object?> loadProfilesFunc) => LoadProfilesFunc = loadProfilesFunc;

    /// <summary>
    /// 可写在属性的set访问器后，用于自动储存
    /// </summary>
    /// <param name="profileInfo"></param>
    /// <returns></returns>
    protected static void AutoSave(ProfileInfo profileInfo) => SaveProfile(profileInfo);

    /// <summary>
    /// 可写在属性的set访问器后，用于自动储存
    /// </summary>
    /// <param name="profileInfo"></param>
    /// <returns></returns>
    protected static async Task AutoSaveAsync(ProfileInfo profileInfo) => await SaveProfileAsync(profileInfo);

    /// <summary>
    /// 导出指定的配置文件
    /// </summary>
    /// <param name="profileInfo">指定的配置文件</param>
    /// <returns></returns>
    public static string ExportProfile(ProfileInfo profileInfo)
    {
        var waitSaveProfile = Profiles.Find(x => x.Profile == profileInfo.Profile);
        if (waitSaveProfile is null)
            return string.Empty;
        var saveProfileDictionary = new XFEDictionary();
        var instance = profileInfo.GetProfileInstance();
        foreach (var property in waitSaveProfile.MemberInfo)
            saveProfileDictionary.Add(property.Name, SaveProfilesFunc(instance, property));
        return saveProfileDictionary.ToString();
    }

    /// <summary>
    /// 导出所有配置文件
    /// </summary>
    /// <returns></returns>
    public static string ExportProfiles()
    {
        var exportProfiles = new XFEDictionary();
        foreach (var profile in Profiles)
            exportProfiles.Add(profile.Profile.Name, ExportProfile(profile));
        return exportProfiles.ToString();
    }

    /// <summary>
    /// 导入指定的配置文件<br/>
    /// 本方法仅支持导入由<seealso cref="ExportProfile(ProfileInfo)"/>导出的配置文件<br/><br/>
    /// 如需导入由<seealso cref="ExportProfiles"/>导出的配置文件，请使用<seealso cref="ImportProfiles(string,bool)"/>
    /// </summary>
    /// <param name="profileInfo">指定的配置文件</param>
    /// <param name="profileString">配置文件字符串</param>
    /// <param name="autoSave">导入后是否自动储存</param>
    public static void ImportProfile(ProfileInfo profileInfo, string profileString, bool autoSave = true)
    {
        var instance = profileInfo.GetProfileInstance();
        var waitSaveProfile = Profiles.Find(x => x.Profile == profileInfo.Profile);
        if (waitSaveProfile is null)
            return;
        var importProfileDictionary = new XFEDictionary(profileString);
        foreach (var property in waitSaveProfile.MemberInfo)
        {
            if (importProfileDictionary[property.Name] is not null)
            {
                if (property.MemberInfo is FieldInfo fieldInfo)
                    fieldInfo.SetValue(instance, LoadProfilesFunc(importProfileDictionary[property.Name]!, property));
                else if (property.MemberInfo is PropertyInfo propertyInfo)
                    propertyInfo.SetValue(instance, LoadProfilesFunc(importProfileDictionary[property.Name]!, property));
            }
        }
        if (autoSave)
            SaveProfile(profileInfo);
    }

    /// <summary>
    /// 导入所有配置文件<br/>
    /// 本方法仅支持导入由<seealso cref="ExportProfiles"/>导出的配置文件<br/><br/>
    /// 如需导入由<seealso cref="ExportProfile(ProfileInfo)"/>导出的配置文件，请使用<seealso cref="ImportProfile(ProfileInfo, string,bool)"/>
    /// </summary>
    /// <param name="profileString">配置文件字符串</param>
    /// <param name="autoSave">导入后是否自动储存</param>
    public static void ImportProfiles(string profileString, bool autoSave = true)
    {
        var importProfiles = new XFEDictionary(profileString);
        foreach (var profile in Profiles)
        {
            if (importProfiles[profile.Profile.Name] is not null)
                ImportProfile(profile, importProfiles[profile.Profile.Name]!, autoSave);
        }
    }
}
