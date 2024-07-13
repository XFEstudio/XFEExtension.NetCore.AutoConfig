using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace XFEExtension.NetCore.AutoConfig.Generator
{
    [Generator]
    public class ProfilePropertyAutoGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ProfilePropertySyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxTrees = context.Compilation.SyntaxTrees;
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
                    var properties = new List<PropertyDeclarationSyntax>();
                    var methods = new List<MethodDeclarationSyntax>();
                    foreach (var fieldDeclarationSyntax in fieldDeclarationSyntaxes)
                    {
                        var variableDeclaration = fieldDeclarationSyntax.Declaration.Variables.First();
                        var fieldName = variableDeclaration.Identifier.Text;
                        var propertyName = fieldName[0] == '_' ? fieldName[1].ToString().ToUpper() + fieldName.Substring(2) : fieldName[0].ToString().ToUpper() + fieldName.Substring(1);
                        var getMethodName = $"Get{propertyName}Property";
                        var setMethodName = $"Set{propertyName}Property";
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
                        var propertyType = fieldDeclarationSyntax.Declaration.Type;
                        #region Trivia头
                        var triviaText = $@"/// <inheritdoc cref=""{fieldName}""/>
/// <remarks>
/// <seealso cref=""{propertyName}""/> 是根据 <seealso cref=""{fieldName}""/> 自动生成的属性<br/><br/>
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
/// <code><seealso langword=""set""/>方法已生成以下代码:	○ <seealso cref=""{className}.{setMethodName}({propertyType})""/>;<br/>";
                        #endregion
                        var setExpressionStatements = new List<StatementSyntax>()
                        {
                            SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"{setMethodName}(value)")).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
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
                                triviaText += $"\n///\t\t\t\t○ {funcText.Replace("\n", "<br/>").Replace(fieldName, $"<seealso langword=\"{fieldName}\"/>").Replace("value", "<seealso langword=\"value\"/>")};<br/>";
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
                        setExpressionStatements.Add(SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"global::XFEExtension.NetCore.AutoConfig.XFEProfile.SaveProfile(new(typeof({className}), ProfilePath))")));
                        #region Set方法中的保存方法的注释
                        triviaText += $@"
///				○ <seealso cref=""global::XFEExtension.NetCore.AutoConfig.XFEProfile.SaveProfile(ProfileInfo)""/>";
                        #endregion
                        #region Trivia尾
                        triviaText += @"
/// </code>
/// </remarks>
";
                        #endregion
                        var property = SyntaxFactory.PropertyDeclaration(propertyType, propertyName)
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                            .AddAttributeLists(attributeSyntax)
                            .WithAccessorList(SyntaxFactory.AccessorList(
                                SyntaxFactory.List(new[]
                                {
                                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                        .WithBody(SyntaxFactory.Block(getExpressionStatements)),
                                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                        .WithBody(SyntaxFactory.Block(setExpressionStatements))
                                })))
                            .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(triviaText));
                        var getMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), getMethodName)
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                        var setMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), setMethodName)
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                            .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("value")).WithType(propertyType))
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                        methods.Add(getMethod);
                        methods.Add(setMethod);
                        properties.Add(property.NormalizeWhitespace());
                    }
                    var profileClassSyntaxTree = GenerateProfileClassSyntaxTree(classDeclaration, usingDirectives, properties, methods, fileScopedNamespaceDeclarationSyntax);
                    context.AddSource($"{className}.g.cs", profileClassSyntaxTree.ToString());
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
                                                                                                               .Where(classDeclaration => classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword) && !classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword));

        private static SyntaxTree GenerateProfileClassSyntaxTree(ClassDeclarationSyntax classDeclaration, UsingDirectiveSyntax[] usingDirectiveSyntaxes, List<PropertyDeclarationSyntax> propertyDeclarationSyntaxes, List<MethodDeclarationSyntax> methodDeclarationSyntaxes, FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax)
        {
            var className = classDeclaration.Identifier.ValueText;
            var triviaText = $@"/// <remarks>
/// <code><seealso cref=""{className}""/> 已自动实现以下属性：</code><br/>
/// <code>
";
            triviaText += string.Join("<br/>\n", propertyDeclarationSyntaxes.Select(propertyDeclarationSyntax => $"/// ○ <seealso cref=\"{propertyDeclarationSyntax.Identifier}\"/>")) + "\n/// </code><br/>\n/// <code>来自<seealso cref=\"global::XFEExtension.NetCore.AutoConfig.XFEProfile\"/></code>\n/// </remarks>\n";
            var memberDeclarations = new List<MemberDeclarationSyntax>
            {
                SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("string"), "ProfilePath")
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(
                    new[]
                    {
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                     .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                     .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                    })))
                .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression(@"""""")))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia($@"/// <summary>
/// 该配置文件的自动存储和读取路径<br/>
/// 设置的时候记得带上文件名和后缀<br/>
/// <seealso cref=""ProfilePath""/> 是 <seealso cref=""{className}""/> 配置文件类的自动存储读取路径
/// </summary>
")),
                SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(className), "Current")
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(SyntaxFactory.ParseName("global::XFEExtension.NetCore.AutoConfig.ProfileInstanceAttribute")))))
                .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(
                    new[]
                    {
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                     .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                     .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                    })))
                .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression($"new {className}()")))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia($@"/// <summary>
/// 该配置文件的实例<br/>
/// <seealso cref=""Current""/> 是 <seealso cref=""{className}""/> 配置文件类的实例数据
/// </summary>
"))
            };
            memberDeclarations.AddRange(propertyDeclarationSyntaxes);
            memberDeclarations.AddRange(methodDeclarationSyntaxes);
            var staticConstructorSyntax = SyntaxFactory.ConstructorDeclaration(className)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                    .WithBody(SyntaxFactory.Block(
                        SyntaxFactory.ParseStatement($"global::XFEExtension.NetCore.AutoConfig.XFEProfile.LoadProfiles([new(typeof({className}), ProfilePath)]);")));
            if (classDeclaration.AttributeLists.Any(IsAutoLoadProfileAttribute))
            {
                var autoLoadProfileAttribute = classDeclaration.AttributeLists.First(attributeList => IsAutoLoadProfileAttribute(attributeList)).Attributes.First();
                if (autoLoadProfileAttribute.ArgumentList != null)
                {
                    var argument = autoLoadProfileAttribute.ArgumentList.Arguments.First();
                    if (argument.Expression is LiteralExpressionSyntax literalExpressionSyntax && literalExpressionSyntax.Token.ValueText == "true")
                    {
                        memberDeclarations.Add(staticConstructorSyntax);
                    }
                }
                else
                {
                    memberDeclarations.Add(staticConstructorSyntax);
                }
            }
            else
            {
                memberDeclarations.Add(staticConstructorSyntax);
            }
            var profileClass = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(memberDeclarations.ToArray())
                .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(triviaText))
                .NormalizeWhitespace();
            MemberDeclarationSyntax memberDeclaration;
            if (fileScopedNamespaceDeclarationSyntax is null)
            {
                var namespaceDeclaration = classDeclaration.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
                if (namespaceDeclaration is null)
                    memberDeclaration = profileClass;
                else
                    memberDeclaration = SyntaxFactory.NamespaceDeclaration(namespaceDeclaration.Name)
                        .AddMembers(profileClass);
            }
            else
            {
                memberDeclaration = SyntaxFactory.FileScopedNamespaceDeclaration(fileScopedNamespaceDeclarationSyntax.Name)
                    .AddMembers(profileClass);
            }
            var profileClassCompilationUnit = SyntaxFactory.CompilationUnit()
                .AddUsings(usingDirectiveSyntaxes)
                .AddMembers(memberDeclaration)
                .NormalizeWhitespace();
            return SyntaxFactory.SyntaxTree(profileClassCompilationUnit);
        }
    }
}
