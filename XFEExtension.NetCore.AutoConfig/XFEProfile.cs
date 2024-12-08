using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using XFEExtension.NetCore.FormatExtension;

namespace XFEExtension.NetCore.AutoConfig;

/// <summary>
/// XFE配置文件，实现配置文件读写自动化
/// </summary>
public abstract class XFEProfile
{
    private string id = Guid.NewGuid().ToString();
    /// <summary>
    /// 配置文件所在的默认目录
    /// </summary>
    public static string ProfilesDefaultPath { get; set; } = $@"{AppDomain.CurrentDomain.BaseDirectory}Profiles";
    /// <summary>
    /// 配置文件存储位置
    /// </summary>
    internal protected string ProfilePath { get; set; } = string.Empty;
    /// <summary>
    /// 配置文件扩展名
    /// </summary>
    internal protected string ProfileFileExtension { get; set; } = ".xpf";
    /// <summary>
    /// 默认配置文件存储和读取的操作模式
    /// </summary>
    internal protected ProfileOperationMode DefaultProfileOperationMode { get; set; } = ProfileOperationMode.XFEDictionary;
    /// <summary>
    /// 加载操作
    /// </summary>
    internal protected ProfileLoadOperation LoadOperation { get; set; } = XFEDictionaryLoadProfileOperation;
    /// <summary>
    /// 保存操作
    /// </summary>
    internal protected ProfileSaveOperation SaveOperation { get; set; } = XFEDictionarySaveProfileOperation;
    /// <summary>
    /// 配置文件 “属性名称-属性类型” 字典
    /// </summary>
    internal protected Dictionary<string, Type> PropertyInfoDictionary { get; set; } = [];
    /// <summary>
    /// 配置文件 “属性名称-属性设置方法” 字典
    /// </summary>
    internal protected Dictionary<string, SetValueDelegate> PropertySetFuncDictionary { get; set; } = [];
    /// <summary>
    /// 配置文件 “属性名称-属性获取方法” 字典
    /// </summary>
    internal protected Dictionary<string, GetValueDelegate> PropertyGetFuncDictionary { get; set; } = [];
    /// <summary>
    /// 通过XFE字典加载配置文件方法（默认）
    /// </summary>
    /// <param name="profileInstance">配置文件实例</param>
    /// <param name="profileString">配置文件字符串</param>
    /// <param name="propertyInfoDictionary">配置文件 “属性名称-属性类型” 字典</param>
    /// <param name="propertySetFuncDictionary">配置文件 “属性名称-属性设置方法” 字典</param>
    /// <returns>配置文件实例</returns>
    public static XFEProfile? XFEDictionaryLoadProfileOperation(XFEProfile profileInstance, string profileString, Dictionary<string, Type> propertyInfoDictionary, Dictionary<string, SetValueDelegate> propertySetFuncDictionary)
    {
        XFEDictionary propertyFileContent = profileString;
        foreach (var property in propertyFileContent)
            if (propertySetFuncDictionary.TryGetValue(property.Header, out var setValueDelegate) && propertyInfoDictionary.TryGetValue(property.Header, out var type))
                setValueDelegate(JsonSerializer.Deserialize(property.Content, type));
        return null;
    }
    /// <summary>
    /// 通过XFE字典保存配置文件方法（默认）
    /// </summary>
    /// <param name="profileInstance">配置文件实例</param>
    /// <param name="propertyInfoDictionary">配置文件 “属性名称-属性类型” 字典</param>
    /// <param name="propertyGetFuncDictionary">配置文件 “属性名称-属性值获取方法” 字典</param>
    /// <returns>保存内容</returns>
    public static string XFEDictionarySaveProfileOperation(XFEProfile profileInstance, Dictionary<string, Type> propertyInfoDictionary, Dictionary<string, GetValueDelegate> propertyGetFuncDictionary)
    {
        if (profileInstance is null)
            return string.Empty;
        var saveProfileDictionary = new XFEDictionary();
        foreach (var property in propertyGetFuncDictionary)
            saveProfileDictionary.Add(property.Key, JsonSerializer.Serialize(property.Value()));
        return saveProfileDictionary.ToString();
    }
    /// <summary>
    /// 通过Json加载配置文件方法
    /// </summary>
    /// <param name="profileInstance">配置文件实例</param>
    /// <param name="profileString">配置文件字符串</param>
    /// <param name="propertyInfoDictionary">配置文件 “属性名称-属性类型” 字典</param>
    /// <param name="propertySetFuncDictionary">配置文件 “属性名称-属性设置方法” 字典</param>
    /// <returns>配置文件实例</returns>
    public static XFEProfile? JsonLoadProfileOperation(XFEProfile profileInstance, string profileString, Dictionary<string, Type> propertyInfoDictionary, Dictionary<string, SetValueDelegate> propertySetFuncDictionary) => JsonSerializer.Deserialize(profileString, profileInstance.GetType()) is XFEProfile xFEProfile ? xFEProfile : null;

    /// <summary>
    /// 通过Json保存配置文件方法
    /// </summary>
    /// <param name="profileInstance">配置文件实例</param>
    /// <param name="propertyInfoDictionary">配置文件 “属性名称-属性类型” 字典</param>
    /// <param name="propertyGetFuncDictionary">配置文件 “属性名称-属性值获取方法” 字典</param>
    /// <returns>保存内容</returns>
    public static string JsonSaveProfileOperation(XFEProfile profileInstance, Dictionary<string, Type> propertyInfoDictionary, Dictionary<string, GetValueDelegate> propertyGetFuncDictionary) => profileInstance is null ? string.Empty : JsonSerializer.Serialize(profileInstance, profileInstance.GetType());
    /// <summary>
    /// 通过XML加载配置文件方法
    /// </summary>
    /// <param name="profileInstance">配置文件实例</param>
    /// <param name="profileString">配置文件字符串</param>
    /// <param name="propertyInfoDictionary">配置文件 “属性名称-属性类型” 字典</param>
    /// <param name="propertySetFuncDictionary">配置文件 “属性名称-属性设置方法” 字典</param>
    /// <returns>配置文件实例</returns>
    public static XFEProfile? XmlLoadProfileOperation(XFEProfile profileInstance, string profileString, Dictionary<string, Type> propertyInfoDictionary, Dictionary<string, SetValueDelegate> propertySetFuncDictionary) => !string.IsNullOrEmpty(profileString) && new XmlSerializer(profileInstance.GetType()).Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(profileString))) is XFEProfile xFEProfile ? xFEProfile : null;

    /// <summary>
    /// 通过XML保存配置文件方法
    /// </summary>
    /// <param name="profileInstance">配置文件实例</param>
    /// <param name="propertyInfoDictionary">配置文件 “属性名称-属性类型” 字典</param>
    /// <param name="propertyGetFuncDictionary">配置文件 “属性名称-属性值获取方法” 字典</param>
    /// <returns>保存内容</returns>
    public static string XmlSaveProfileOperation(XFEProfile profileInstance, Dictionary<string, Type> propertyInfoDictionary, Dictionary<string, GetValueDelegate> propertyGetFuncDictionary)
    {
        if (profileInstance is null)
            return string.Empty;
        using var stream = new MemoryStream();
        new XmlSerializer(profileInstance.GetType()).Serialize(stream, profileInstance);
        stream.Position = 0;
        return new StreamReader(stream).ReadToEnd();
    }
    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <returns>配置文件实例</returns>
    internal protected XFEProfile InstanceLoadProfile()
    {
        if (File.Exists(ProfilePath) && LoadOperation(this, File.ReadAllText(ProfilePath), PropertyInfoDictionary, PropertySetFuncDictionary) is XFEProfile xFEProfile)
        {
            xFEProfile.Initialize();
            return xFEProfile;
        }
        return this;
    }
    /// <summary>
    /// 保存配置文件
    /// </summary>
    /// <returns>保存内容</returns>
    internal protected void InstanceSaveProfile()
    {
        var saveContent = SaveOperation(this, PropertyInfoDictionary, PropertyGetFuncDictionary);
        var fileSavePath = Path.GetDirectoryName(ProfilePath);
        if (!Directory.Exists(fileSavePath) && fileSavePath is not null && fileSavePath != string.Empty)
            Directory.CreateDirectory(fileSavePath);
        File.WriteAllTextAsync(ProfilePath, saveContent);
        return;
    }
    /// <summary>
    /// 删除配置文件
    /// </summary>
    internal protected void InstanceDeleteProfile()
    {
        if (File.Exists(ProfilePath))
            File.Delete(ProfilePath);
    }
    /// <summary>
    /// 导出配置文件
    /// </summary>
    /// <returns></returns>
    internal protected string InstanceExportProfile() => SaveOperation(this, PropertyInfoDictionary, PropertyGetFuncDictionary);
    /// <summary>
    /// 导入配置文件
    /// </summary>
    /// <param name="profileString">配置文件字符串</param>
    /// <returns></returns>
    internal protected XFEProfile InstanceImportProfile(string profileString)
    {
        if (LoadOperation(this, profileString, PropertyInfoDictionary, PropertySetFuncDictionary) is XFEProfile xFEProfile)
        {
            xFEProfile.Initialize();
            return xFEProfile;
        }
        return this;
    }
    /// <summary>
    /// 设置配置文件加载和存储操作
    /// </summary>
    internal protected void SetProfileOperation()
    {
        switch (DefaultProfileOperationMode)
        {
            case ProfileOperationMode.XFEDictionary:
                LoadOperation = XFEDictionaryLoadProfileOperation;
                SaveOperation = XFEDictionarySaveProfileOperation;
                ProfileFileExtension = ".xpf";
                break;
            case ProfileOperationMode.Json:
                LoadOperation = JsonLoadProfileOperation;
                SaveOperation = JsonSaveProfileOperation;
                ProfileFileExtension = ".json";
                break;
            case ProfileOperationMode.Xml:
                LoadOperation = XmlLoadProfileOperation;
                SaveOperation = XmlSaveProfileOperation;
                ProfileFileExtension = ".xml";
                break;
            case ProfileOperationMode.Custom:
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Initialize() => SetProfileOperation();
    #region 已过时
    [Obsolete("SaveProfilesFunc属性现已过时，对于每个配置文件实例，请使用 XXXProfile.SaveOperation")]
    private static Func<object?, ProfileEntryInfo, string> SaveProfilesFunc { get; set; } = (i, p) =>
    {
        if (p.MemberInfo is FieldInfo fieldInfo)
            return JsonSerializer.Serialize(fieldInfo.GetValue(i));
        else if (p.MemberInfo is PropertyInfo propertyInfo)
            return JsonSerializer.Serialize(propertyInfo.GetValue(i));
        else
            return string.Empty;
    };
    [Obsolete("SaveProfilesFunc属性现已过时，对于每个配置文件实例，请使用 XXXProfile.LoadOperation")]
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
    [Obsolete("XFEProfile现在不再对配置文件实行统一的管理，请对每个配置文件单独操作")]
    public static List<ProfileInfo> Profiles { get; private set; } = [];
    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <param name="profileInfo">配置文件信息</param>
    /// <returns></returns>
    [Obsolete("XFEProfile现在不再对配置文件实行统一的管理，请对每个配置文件单独操作")]
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
    [Obsolete("XFEProfile现在不再对配置文件实行统一的管理，请对每个配置文件单独操作")]
    public static async Task LoadProfilesAsync(params ProfileInfo[] profileInfo) => await Task.Run(() => LoadProfiles(profileInfo));

    /// <summary>
    /// 储存指定的配置文件
    /// </summary>
    /// <param name="profileInfo">配置文件</param>
    /// <returns></returns>
    [Obsolete("XFEProfile现在不再对配置文件实行统一的管理，请对每个配置文件单独操作")]
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
    [Obsolete("XFEProfile现在不再对配置文件实行统一的管理，请对每个配置文件单独操作")]
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
    [Obsolete("XFEProfile现在不再对配置文件实行统一的管理，请对每个配置文件单独操作")]
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
    [Obsolete("XFEProfile现在不再对配置文件实行统一的管理，请对每个配置文件单独操作")]
    public static async Task SaveProfilesAsync()
    {
        foreach (var profile in Profiles)
            await SaveProfileAsync(profile);
    }

    /// <summary>
    /// 删除指定的配置文件
    /// </summary>
    /// <param name="profileInfo">指定的配置文件</param>
    [Obsolete("XFEProfile现在不再对配置文件实行统一的管理，请对每个配置文件单独操作")]
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
    [Obsolete("XFEProfile现在不再对配置文件实行统一的管理，请对每个配置文件单独操作")]
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
    [Obsolete("XFEProfile现在不再对配置文件实行统一的管理，请对每个配置文件单独操作")]
    public static void DeleteProfiles()
    {
        foreach (var profile in Profiles)
            DeleteProfile(profile);
    }

    /// <summary>
    /// 删除所有配置文件
    /// </summary>
    /// <returns></returns>
    [Obsolete("XFEProfile现在不再对配置文件实行统一的管理，请对每个配置文件单独操作")]
    public static async Task DeleteProfilesAsync()
    {
        foreach (var profile in Profiles)
            await DeleteProfileAsync(profile);
    }

    /// <summary>
    /// 设置储存配置文件的方法
    /// </summary>
    /// <param name="saveProfilesFunc">储存方法</param>
    [Obsolete("XFEProfile现在不再对配置文件实行统一的管理，请对每个配置文件单独操作")]
    public static void SetSaveProfilesFunction(Func<object?, ProfileEntryInfo, string> saveProfilesFunc) => SaveProfilesFunc = saveProfilesFunc;

    /// <summary>
    /// 设置加载配置文件的方法
    /// </summary>
    /// <param name="loadProfilesFunc">加载方法</param>
    [Obsolete("XFEProfile现在不再对配置文件实行统一的管理，请对每个配置文件单独操作")]
    public static void SetLoadProfilesFunction(Func<string, ProfileEntryInfo, object?> loadProfilesFunc) => LoadProfilesFunc = loadProfilesFunc;

    /// <summary>
    /// 可写在属性的set访问器后，用于自动储存
    /// </summary>
    /// <param name="profileInfo"></param>
    /// <returns></returns>
    [Obsolete("XFEProfile现在不再对配置文件实行统一的管理，请对每个配置文件单独操作")]
    protected static void AutoSave(ProfileInfo profileInfo) => SaveProfile(profileInfo);

    /// <summary>
    /// 可写在属性的set访问器后，用于自动储存
    /// </summary>
    /// <param name="profileInfo"></param>
    /// <returns></returns>
    [Obsolete("XFEProfile现在不再对配置文件实行统一的管理，请对每个配置文件单独操作")]
    protected static async Task AutoSaveAsync(ProfileInfo profileInfo) => await SaveProfileAsync(profileInfo);

    /// <summary>
    /// 导出指定的配置文件
    /// </summary>
    /// <param name="profileInfo">指定的配置文件</param>
    /// <returns></returns>
    [Obsolete("XFEProfile现在不再对配置文件实行统一的管理，请对每个配置文件单独操作")]
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
    [Obsolete("XFEProfile现在不再对配置文件实行统一的管理，请对每个配置文件单独操作")]
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
    [Obsolete("XFEProfile现在不再对配置文件实行统一的管理，请对每个配置文件单独操作")]
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
    [Obsolete("XFEProfile现在不再对配置文件实行统一的管理，请对每个配置文件单独操作")]
    public static void ImportProfiles(string profileString, bool autoSave = true)
    {
        var importProfiles = new XFEDictionary(profileString);
        foreach (var profile in Profiles)
        {
            if (importProfiles[profile.Profile.Name] is not null)
                ImportProfile(profile, importProfiles[profile.Profile.Name]!, autoSave);
        }
    }
    #endregion
}
/// <summary>
/// 配置文件保存方法
/// </summary>
/// <param name="profileInstance">配置文件实例</param>
/// <param name="propertyInfoDictionary">配置文件 “属性名称-属性类型” 字典</param>
/// <param name="propertyGetFuncDictionary">配置文件 “属性名称-属性值获取方法” 字典</param>
/// <returns>保存内容</returns>
public delegate string ProfileSaveOperation(XFEProfile profileInstance, Dictionary<string, Type> propertyInfoDictionary, Dictionary<string, GetValueDelegate> propertyGetFuncDictionary);
/// <summary>
/// 配置文件加载方法
/// </summary>
/// <param name="profileInstance">配置文件实例</param>
/// <param name="profileString">配置文件字符串</param>
/// <param name="propertyInfoDictionary">配置文件 “属性名称-属性类型” 字典</param>
/// <param name="propertySetFuncDictionary">配置文件 “属性名称-属性设置方法” 字典</param>
public delegate XFEProfile? ProfileLoadOperation(XFEProfile profileInstance, string profileString, Dictionary<string, Type> propertyInfoDictionary, Dictionary<string, SetValueDelegate> propertySetFuncDictionary);
/// <summary>
/// 设置配置文件属性值委托
/// </summary>
/// <param name="value">要设置的值</param>
public delegate void SetValueDelegate(object? value);
/// <summary>
/// 获取配置文件属性值委托
/// </summary>
/// <returns>获取的属性值</returns>
public delegate object? GetValueDelegate();