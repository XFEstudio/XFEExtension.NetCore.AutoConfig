using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace XFEExtension.NetCore.AutoConfig.Generator;

[Generator]
public class ProfilePropertyAutoGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider, Generate);
    }

    public void Generate(SourceProductionContext context, Compilation compilation)
    {
        var syntaxTrees = compilation.SyntaxTrees;
        foreach (var syntaxTree in syntaxTrees)
        {
            var root = syntaxTree.GetRoot();
            var classDeclarations = GetClassDeclarations(root);
            var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToArray();
            var fileScopedNamespaceDeclarationSyntax = GetFileScopedNamespaceDeclaration(root);
            foreach (var classDeclaration in classDeclarations)
            {
                var fieldDeclarationSyntaxes = GetFieldDeclarations(classDeclaration);
                if (fieldDeclarationSyntaxes is null || !fieldDeclarationSyntaxes.Any())
                {
                    continue;
                }
                var className = classDeclaration.Identifier.ValueText;
                var attributeSyntax = SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.ParseName("global::XFEExtension.NetCore.AutoConfig.ProfileFieldAutoGenerateAttribute"))));
                var staticProperties = new List<PropertyDeclarationSyntax>();
                var instanceProperties = new List<PropertyDeclarationSyntax>();
                var basicProfileMembers = new List<MemberDeclarationSyntax>();
                var methodMembers = new List<MemberDeclarationSyntax>();
                var initializerBlockStatements = new List<StatementSyntax>()
                {
                    SyntaxFactory.ParseStatement($"this.SetProfileOperation();"),
                    SyntaxFactory.ParseStatement($@"this.CurrentProfilePath = $""{{ProfilePath}}{{(string.IsNullOrEmpty(ProfileExtension) ? this.CurrentProfileExtension : ProfileExtension)}}"";")
                };
                foreach (var fieldDeclarationSyntax in fieldDeclarationSyntaxes)
                {
                    var variableDeclaration = fieldDeclarationSyntax.Declaration.Variables.First();
                    var fieldName = variableDeclaration.Identifier.Text;
                    var propertyName = fieldName[0] == '_' ? fieldName[1].ToString().ToUpper() + fieldName.Substring(2) : fieldName[0].ToString().ToUpper() + fieldName.Substring(1);
                    var propertyType = fieldDeclarationSyntax.Declaration.Type;
                    var instancePropertyName = $"Instance{propertyName}";
                    var getMethodName = $"Get{propertyName}Property";
                    var setMethodName = $"Set{propertyName}Property";
                    initializerBlockStatements.Add(SyntaxFactory.ParseStatement($"this.PropertyInfoDictionary.Add(nameof({propertyName}), typeof({propertyType.ToString().Replace("?", string.Empty).Replace(" ", string.Empty)}));"));
                    initializerBlockStatements.Add(SyntaxFactory.ParseStatement($"this.PropertySetFuncDictionary.Add(nameof({propertyName}), (value) => this.{fieldName} = ({propertyType.ToString().Replace("?", string.Empty).Replace(" ", string.Empty)})value);"));
                    initializerBlockStatements.Add(SyntaxFactory.ParseStatement($"this.PropertyGetFuncDictionary.Add(nameof({propertyName}), () => this.{fieldName});"));
                    GetProfilePropertyAttributeList(fieldDeclarationSyntax).ForEach(attribute =>
                    {
                        if (attribute.ArgumentList is null)
                        {
                            return;
                        }
                        var argument = attribute.ArgumentList.Arguments.First();
                        if (argument.Expression is LiteralExpressionSyntax literalExpressionSyntax)
                        {
                            propertyName = literalExpressionSyntax.Token.ValueText;
                        }
                    });
                    #region Trivia头
                    var triviaText = $@"/// <inheritdoc cref=""{fieldName}""/>
/// <remarks>
/// <seealso cref=""{propertyName}""/> 是根据 <seealso cref=""{fieldName}""/> 自动生成的静态属性<br/><br/>
/// <code><seealso langword=""get""/>方法已生成以下代码:	○ <seealso cref=""{className}.{getMethodName}()""/>;<br/>";
                    #endregion
                    var getExpressionStatements = new List<StatementSyntax>()
                    {
                        SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"{getMethodName}()")).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                    };
                    if (fieldDeclarationSyntax.AttributeLists.Any(IsProfilePropertyAddGetAttribute))
                    {
                        GetProfilePropertyAddGetAttributeList(fieldDeclarationSyntax).ForEach(attribute =>
                        {
                            if (attribute.ArgumentList is null)
                            {
                                return;
                            }
                            var argument = attribute.ArgumentList.Arguments.First();
                            var funcText = string.Empty;
                            if (argument.Expression is LiteralExpressionSyntax literalExpressionSyntax)
                                funcText = literalExpressionSyntax.Token.ValueText;
                            if (argument.Expression is InterpolatedStringExpressionSyntax interpolatedStringExpressionSyntax)
                                funcText = interpolatedStringExpressionSyntax.Contents.ToString();
                            if (argument.Expression is InvocationExpressionSyntax invocationExpressionSyntax)
                                funcText = invocationExpressionSyntax.GetText().ToString();
                            getExpressionStatements.Add(SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression(funcText)).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                            #region Get方法注释
                            triviaText += $"\n///\t\t\t\t○ {funcText.Replace("\n", "<br/>").Replace("return", "<seealso langword=\"return\"/>").Replace(fieldName, $"<seealso langword=\"{fieldName}\"/>")};<br/>";
                            #endregion
                        });
                    }
                    else
                    {
                        getExpressionStatements.Add(SyntaxFactory.ReturnStatement(SyntaxFactory.ParseExpression($"Current.{fieldName}")));
                        #region Get方默认注释
                        triviaText += $@"
///				○ <seealso langword=""return""/> <seealso langword=""{fieldName}""/>;";
                        #endregion
                    }
                    #region Get方法尾及Set方法头注释
                    triviaText += $@"
/// </code>
/// <br/>
/// <code><seealso langword=""set""/>方法已生成以下代码:	○ <seealso cref=""{className}.{setMethodName}(ref {propertyType.ToString().Replace("<", "{").Replace(">", "}")})""/>;<br/>";
                    #endregion
                    var setExpressionStatements = new List<StatementSyntax>()
                    {
                        SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"{setMethodName}(ref value)")).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                    };
                    if (fieldDeclarationSyntax.AttributeLists.Any(IsProfilePropertyAddSetAttribute))
                    {
                        GetProfilePropertyAddSetAttributeList(fieldDeclarationSyntax).ForEach(attribute =>
                        {
                            if (attribute.ArgumentList is null)
                            {
                                return;
                            }
                            var argument = attribute.ArgumentList.Arguments.First();
                            var funcText = string.Empty;
                            if (argument.Expression is LiteralExpressionSyntax literalExpressionSyntax)
                                funcText = literalExpressionSyntax.Token.ValueText;
                            else if (argument.Expression is InterpolatedStringExpressionSyntax interpolatedStringExpressionSyntax)
                                funcText = interpolatedStringExpressionSyntax.Contents.ToString();
                            else if (argument.Expression is InvocationExpressionSyntax invocationExpressionSyntax)
                                funcText = invocationExpressionSyntax.GetText().ToString();
                            setExpressionStatements.Add(SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression(funcText)).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                            #region Set方法注释
                            triviaText += $"\n///\t\t\t\t○ {funcText.Replace("\n", "<br/>").Replace("<", "&lt;").Replace(">", "&gt;").Replace(fieldName, $" < seealso langword=\"{fieldName}\"/>").Replace("value", "<seealso langword=\"value\"/>")};<br/>";
                            #endregion
                        });
                    }
                    else
                    {
                        setExpressionStatements.Add(SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"Current.{fieldName} = value")));
                        #region Set方法默认注释
                        triviaText += $@"
///				○ <seealso langword=""{fieldName}""/> = <seealso langword=""value""/>;<br/>";
                        #endregion
                    }
                    setExpressionStatements.Add(SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"{className}.SaveProfile()")));
                    #region Set方法中的保存方法的注释
                    triviaText += $@"
///				○ <seealso cref=""{className}.SaveProfile()""/>";
                    #endregion
                    #region Trivia尾
                    triviaText += @"
/// </code>
/// </remarks>
";
                    #endregion
                    var staticProperty = SyntaxFactory.PropertyDeclaration(propertyType, propertyName)
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                        .AddAttributeLists(attributeSyntax)
                        .WithAccessorList(SyntaxFactory.AccessorList(
                            SyntaxFactory.List(
                            [
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                    .WithBody(SyntaxFactory.Block(getExpressionStatements)),
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                    .WithBody(SyntaxFactory.Block(setExpressionStatements))
                            ])))
                        .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(triviaText));
                    var getMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), getMethodName)
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia($@"/// <summary>
/// 当且仅当 <seealso cref=""{className}.{propertyName}""/> 被获取时触发此方法
/// </summary>
"));
                    var setMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), setMethodName)
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                        .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("value")).WithType(propertyType).WithModifiers([SyntaxFactory.Token(SyntaxKind.RefKeyword)]))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia($@"/// <summary>
/// 当且仅当 <seealso cref=""{className}.{propertyName}""/> 被赋值时触发此方法
/// </summary>
"));
                    methodMembers.Add(getMethod);
                    methodMembers.Add(setMethod);
                    instanceProperties.Add(SyntaxFactory.PropertyDeclaration(propertyType, instancePropertyName)
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                        .AddAttributeLists(attributeSyntax)
                        .WithAccessorList(SyntaxFactory.AccessorList(
                            SyntaxFactory.List(
                            [
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                    .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.ParseExpression($"{fieldName}")))
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                    .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.ParseExpression($"{fieldName} = value")))
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                            ])))
                        .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia($@"/// <inheritdoc cref=""{fieldName}""/>
/// <remarks>
/// <seealso cref=""{propertyName}""/> 是根据 <seealso cref=""{fieldName}""/> 自动生成的实例属性
/// </remarks>
")));
                    staticProperties.Add(staticProperty.NormalizeWhitespace());
                }
                basicProfileMembers.Add(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "Initialize")
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
                        .WithBody(SyntaxFactory.Block(initializerBlockStatements)));
                var basicProfileClassSyntaxTree = GenerateBasicProfileClassSyntaxTree(classDeclaration, usingDirectives, basicProfileMembers, fileScopedNamespaceDeclarationSyntax);
                var profileStaticPropertyClassSyntaxTree = GenerateProfileStaticPropertyClassSyntaxTree(classDeclaration, usingDirectives, staticProperties, fileScopedNamespaceDeclarationSyntax);
                var profileInstancePropertyClassSyntaxTree = GenerateProfileInstancePropertyClassSyntaxTree(classDeclaration, usingDirectives, instanceProperties, fileScopedNamespaceDeclarationSyntax);
                var profileMethodClassSyntaxTree = GenerateProfileMethodClassSyntaxTree(classDeclaration, usingDirectives, methodMembers, fileScopedNamespaceDeclarationSyntax);
                context.AddSource($"{className}.g.cs", basicProfileClassSyntaxTree.ToString());
                context.AddSource($"{className}.StaticProperties.g.cs", profileStaticPropertyClassSyntaxTree.ToString());
                context.AddSource($"{className}.InstanceProperties.g.cs", profileInstancePropertyClassSyntaxTree.ToString());
                context.AddSource($"{className}.Methods.g.cs", profileMethodClassSyntaxTree.ToString());
            }
        }
    }
    public static bool IsProfilePropertyAttribute(AttributeListSyntax attributeList) => attributeList.Attributes.Any(attribute => attribute.Name.ToString() == "ProfileProperty");

    public static List<AttributeSyntax> GetProfilePropertyAttributeList(FieldDeclarationSyntax fieldDeclaration) => fieldDeclaration.AttributeLists.Where(IsProfilePropertyAttribute).SelectMany(attributeList => attributeList.Attributes).ToList();

    public static bool IsAutoLoadProfileAttribute(AttributeListSyntax attributeList) => attributeList.Attributes.Any(attribute => attribute.Name.ToString() == "AutoLoadProfile");

    public static List<AttributeSyntax> GetAutoLoadProfileAttribute(FieldDeclarationSyntax fieldDeclaration) => fieldDeclaration.AttributeLists.Where(IsAutoLoadProfileAttribute).SelectMany(attributeList => attributeList.Attributes).ToList();

    public static bool IsProfilePropertyAddGetAttribute(AttributeListSyntax attributeList) => attributeList.Attributes.Any(attribute => attribute.Name.ToString() == "ProfilePropertyAddGet");

    public static List<AttributeSyntax> GetProfilePropertyAddGetAttributeList(FieldDeclarationSyntax fieldDeclaration) => fieldDeclaration.AttributeLists.Where(IsProfilePropertyAddGetAttribute).SelectMany(attributeList => attributeList.Attributes).ToList();

    public static bool IsProfilePropertyAddSetAttribute(AttributeListSyntax attributeList) => attributeList.Attributes.Any(attribute => attribute.Name.ToString() == "ProfilePropertyAddSet");

    public static List<AttributeSyntax> GetProfilePropertyAddSetAttributeList(FieldDeclarationSyntax fieldDeclaration) => fieldDeclaration.AttributeLists.Where(IsProfilePropertyAddSetAttribute).SelectMany(attributeList => attributeList.Attributes).ToList();

    public static FileScopedNamespaceDeclarationSyntax GetFileScopedNamespaceDeclaration(SyntaxNode rootNode)
    {
        var namespaceResults = rootNode.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>();
        if (namespaceResults != null && namespaceResults.Count() > 0)
            return namespaceResults.First();
        return null;
    }

    public static IEnumerable<FieldDeclarationSyntax> GetFieldDeclarations(ClassDeclarationSyntax classDeclaration) => classDeclaration.DescendantNodes()
                                                                                                                                       .OfType<FieldDeclarationSyntax>()
                                                                                                                                       .Where(fieldDeclarationSyntax => fieldDeclarationSyntax.AttributeLists.Any(IsProfilePropertyAttribute) && !fieldDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword));

    public static IEnumerable<ClassDeclarationSyntax> GetClassDeclarations(SyntaxNode rootNode) => rootNode.DescendantNodes()
                                                                                                           .OfType<ClassDeclarationSyntax>()
                                                                                                           .Where(classDeclaration => classDeclaration.BaseList is not null && classDeclaration.BaseList.Types.Any(type => type.ToString() == "XFEProfile"));

    private static MemberDeclarationSyntax GetClass(ClassDeclarationSyntax classDeclaration, ClassDeclarationSyntax profileClassDeclaration, FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax)
    {
        if (fileScopedNamespaceDeclarationSyntax is null)
        {
            var namespaceDeclaration = classDeclaration.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
            if (namespaceDeclaration is null)
                return profileClassDeclaration;
            else
                return SyntaxFactory.NamespaceDeclaration(namespaceDeclaration.Name)
                    .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia("#pragma warning disable\n#nullable enable"))
                    .AddMembers(profileClassDeclaration);
        }
        else
        {
            return SyntaxFactory.FileScopedNamespaceDeclaration(fileScopedNamespaceDeclarationSyntax.Name)
                .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia("#pragma warning disable\n#nullable enable"))
                .AddMembers(profileClassDeclaration);
        }
    }

    private static SyntaxTree GenerateBasicProfileClassSyntaxTree(ClassDeclarationSyntax classDeclaration, UsingDirectiveSyntax[] usingDirectiveSyntaxes, List<MemberDeclarationSyntax> memberDeclarationSyntaxes, FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax)
    {
        var className = classDeclaration.Identifier.ValueText;
        var staticConstructorSyntax = SyntaxFactory.ConstructorDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement($"Current = new();"),
                    SyntaxFactory.ParseStatement($"Current.Initialize();"),
                    SyntaxFactory.ParseStatement($"{className}.LoadProfile();")
                    ));
        if (classDeclaration.AttributeLists.Any(IsAutoLoadProfileAttribute))
        {
            var autoLoadProfileAttribute = classDeclaration.AttributeLists.First(IsAutoLoadProfileAttribute).Attributes.First();
            if (autoLoadProfileAttribute.ArgumentList != null)
            {
                var argument = autoLoadProfileAttribute.ArgumentList.Arguments.First();
                if (argument.Expression is LiteralExpressionSyntax literalExpressionSyntax && literalExpressionSyntax.Token.ValueText == "true")
                {
                    memberDeclarationSyntaxes.Add(staticConstructorSyntax);
                }
            }
            else
            {
                memberDeclarationSyntaxes.Add(staticConstructorSyntax);
            }
        }
        else
        {
            memberDeclarationSyntaxes.Add(staticConstructorSyntax);
        }
        memberDeclarationSyntaxes.Add(SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(className), "Current")
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
            .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(SyntaxFactory.ParseName("global::XFEExtension.NetCore.AutoConfig.ProfileInstanceAttribute")))))
            .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(
                [
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                 .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                 .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                ])))
            .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia($@"/// <summary>
/// 该配置文件的实例<br/><br/>
/// <seealso cref=""{className}.Current""/> 是 <seealso cref=""{className}""/> 的配置文件类的实例数据
/// </summary>
")));
        memberDeclarationSyntaxes.Add(SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("string"), "ProfilePath")
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
            .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(
                [
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                 .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                 .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                ])))
            .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression($@"$""{{(global::XFEExtension.NetCore.AutoConfig.XFEProfile.ProfilesDefaultPath)}}\\{{nameof({className})}}""")))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia($@"/// <summary>
/// 该配置文件的存储位置<br/><br/>
/// <seealso cref=""{className}.ProfilePath""/> 是 <seealso cref=""{className}""/> 的配置文件存储路径（不包含文件扩展名，如需设置扩展名，请查阅 <seealso cref=""{className}.ProfileExtension""/> ）
/// </summary>
")));
        memberDeclarationSyntaxes.Add(SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("string"), "ProfileExtension")
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
            .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(
                [
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                 .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                 .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                ])))
            .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression("string.Empty")))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia($@"/// <summary>
/// 该配置文件的文件扩展名<br/><br/>
/// 当为空时， <seealso cref=""global::XFEExtension.NetCore.AutoConfig.XFEProfile""/> 将根据配置文件类型自动选择合适的扩展名（.xpf  .json  .xml等）
/// </summary>
")));
        memberDeclarationSyntaxes.Add(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "LoadProfile")
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                        .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.ParseExpression($"Current = Current.InstanceLoadProfile() as {className}")))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia($@"/// <summary>
/// 配置文件加载方法<br/><br/>
/// <seealso cref=""{className}.LoadProfile""/> 是根据 <seealso cref=""{className}""/> 生成的加载配置文件的静态方法
/// </summary>
")));
        memberDeclarationSyntaxes.Add(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "SaveProfile")
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                        .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.ParseExpression($"Current.InstanceSaveProfile()")))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia($@"/// <summary>
/// 配置文件保存方法<br/><br/>
/// <seealso cref=""{className}.SaveProfile""/> 是根据 <seealso cref=""{className}""/> 生成的保存配置文件的静态方法
/// </summary>
")));
        memberDeclarationSyntaxes.Add(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("string"), "ExportProfile")
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                        .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.ParseExpression($"Current.InstanceExportProfile()")))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia($@"/// <summary>
/// 配置文件导出方法<br/><br/>
/// <seealso cref=""{className}.ExportProfile""/> 是根据 <seealso cref=""{className}""/> 生成的导出配置文件的静态方法
/// </summary>
/// <returns>导出的配置文件字符串</returns>
")));
        memberDeclarationSyntaxes.Add(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "ImportProfile")
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                        .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("profileString")).WithType(SyntaxFactory.ParseTypeName("string")))
                        .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.ParseExpression($"Current = Current.InstanceImportProfile(profileString) as {className}")))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia($@"/// <summary>
/// 配置文件导入方法<br/><br/>
/// <seealso cref=""{className}.ImportProfile""/> 是根据 <seealso cref=""{className}""/> 生成的导入配置文件的静态方法
/// </summary>
/// <param name=""profileString"">待导入配置文件字符串</param>
")));
        var profileClass = SyntaxFactory.ClassDeclaration(className)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .AddMembers([.. memberDeclarationSyntaxes])
            .NormalizeWhitespace();
        var profileClassCompilationUnit = SyntaxFactory.CompilationUnit()
            .AddUsings(usingDirectiveSyntaxes)
            .AddMembers(GetClass(classDeclaration, profileClass, fileScopedNamespaceDeclarationSyntax))
            .NormalizeWhitespace();
        return SyntaxFactory.SyntaxTree(profileClassCompilationUnit);
    }

    private static SyntaxTree GenerateProfileStaticPropertyClassSyntaxTree(ClassDeclarationSyntax classDeclaration, UsingDirectiveSyntax[] usingDirectiveSyntaxes, List<PropertyDeclarationSyntax> propertyDeclarationSyntaxes, FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax)
    {
        var className = classDeclaration.Identifier.ValueText;
        var triviaText = $@"/// <remarks>
/// <code><seealso cref=""{className}""/> 已自动实现以下属性：</code><br/>
/// <code>
";
        triviaText += string.Join("<br/>\n", propertyDeclarationSyntaxes.Select(propertyDeclarationSyntax => $"/// ○ <seealso cref=\"{propertyDeclarationSyntax.Identifier}\"/>")) + "\n/// </code><br/>\n/// <code>来自<seealso cref=\"global::XFEExtension.NetCore.AutoConfig.XFEProfile\"/></code>\n/// </remarks>\n";
        var memberDeclarationSyntaxes = new List<MemberDeclarationSyntax>();
        memberDeclarationSyntaxes.AddRange(propertyDeclarationSyntaxes);
        var profileClass = SyntaxFactory.ClassDeclaration(className)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .AddMembers([.. memberDeclarationSyntaxes])
            .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(triviaText))
            .NormalizeWhitespace();
        var profileClassCompilationUnit = SyntaxFactory.CompilationUnit()
            .AddUsings(usingDirectiveSyntaxes)
            .AddMembers(GetClass(classDeclaration, profileClass, fileScopedNamespaceDeclarationSyntax))
            .NormalizeWhitespace();
        return SyntaxFactory.SyntaxTree(profileClassCompilationUnit);
    }

    private static SyntaxTree GenerateProfileInstancePropertyClassSyntaxTree(ClassDeclarationSyntax classDeclaration, UsingDirectiveSyntax[] usingDirectiveSyntaxes, List<PropertyDeclarationSyntax> propertyDeclarationSyntaxes, FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax)
    {
        var className = classDeclaration.Identifier.ValueText;
        var memberDeclarationSyntaxes = new List<MemberDeclarationSyntax>();
        memberDeclarationSyntaxes.AddRange(propertyDeclarationSyntaxes);
        var profileClass = SyntaxFactory.ClassDeclaration(className)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .AddMembers([.. memberDeclarationSyntaxes])
            .NormalizeWhitespace();
        var profileClassCompilationUnit = SyntaxFactory.CompilationUnit()
            .AddUsings(usingDirectiveSyntaxes)
            .AddMembers(GetClass(classDeclaration, profileClass, fileScopedNamespaceDeclarationSyntax))
            .NormalizeWhitespace();
        return SyntaxFactory.SyntaxTree(profileClassCompilationUnit);
    }

    private static SyntaxTree GenerateProfileMethodClassSyntaxTree(ClassDeclarationSyntax classDeclaration, UsingDirectiveSyntax[] usingDirectiveSyntaxes, List<MemberDeclarationSyntax> memberDeclarationSyntaxes, FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax)
    {
        var className = classDeclaration.Identifier.ValueText;
        var profileClass = SyntaxFactory.ClassDeclaration(className)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .AddMembers([.. memberDeclarationSyntaxes])
            .NormalizeWhitespace();
        var profileClassCompilationUnit = SyntaxFactory.CompilationUnit()
            .AddUsings(usingDirectiveSyntaxes)
            .AddMembers(GetClass(classDeclaration, profileClass, fileScopedNamespaceDeclarationSyntax))
            .NormalizeWhitespace();
        return SyntaxFactory.SyntaxTree(profileClassCompilationUnit);
    }
}
