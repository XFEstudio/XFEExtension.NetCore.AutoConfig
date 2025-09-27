using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using XFEExtension.NetCore.AutoConfig.Analyzer.Generator;

namespace XFEExtension.NetCore.AutoConfig.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AutoConfigDiagnostics : DiagnosticAnalyzer
    {
        public const string AddGetNoResultErrorId = "XFE0002";
        public const string AddSetNoSetResultWarningId = "XFW0001";

        public static readonly DiagnosticDescriptor AddGetNoResultError = new DiagnosticDescriptor(AddGetNoResultErrorId,
                                                                                                   "Get方法没有返回值",
                                                                                                   "设置了自定义的Get方法但是没有返回值：'{0}'",
                                                                                                   "XFEExtension.NetCore.AutoConfig.Diagnostics",
                                                                                                   DiagnosticSeverity.Error,
                                                                                                   true,
                                                                                                   "设置了自定义的Get方法但是没有返回值.",
                                                                                                   "https://www.xfegzs.com/codespace/diagnostics/XFE0002.html");

        public static readonly DiagnosticDescriptor AddSetNoSetResultWarning = new DiagnosticDescriptor(AddSetNoSetResultWarningId,
                                                                                                        "Set方法没有设置值",
                                                                                                        "设置了自定义的Set方法但是没有对实际字段进行操作：'{0}'",
                                                                                                        "XFEExtension.NetCore.AutoConfig.Diagnostics",
                                                                                                        DiagnosticSeverity.Warning,
                                                                                                        true,
                                                                                                        "设置了自定义的Set方法但是没有对实际字段进行操作.",
                                                                                                        "https://www.xfegzs.com/codespace/diagnostics/XFW0001.html");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AddGetNoResultError, AddSetNoSetResultWarning);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(ProfileExtensionAnalyzer, SyntaxKind.Attribute);
        }
        public void ProfileExtensionAnalyzer(SyntaxNodeAnalysisContext context)
        {
            foreach (var syntaxTree in context.Compilation.SyntaxTrees)
            {
                var root = syntaxTree.GetRoot();
                foreach (var classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    foreach (var fieldDeclaration in ProfilePropertyAutoGenerator.GetFieldDeclarations(classDeclaration))
                    {
                        if (fieldDeclaration.AttributeLists.Any(ProfilePropertyAutoGenerator.IsProfilePropertyAddGetAttribute))
                        {
                            var getAttributeHasResult = false;
                            var attributeSyntaxList = ProfilePropertyAutoGenerator.GetProfilePropertyAddGetAttributeList(fieldDeclaration);
                            foreach (var attributeSyntax in attributeSyntaxList)
                            {
                                if (attributeSyntax.ArgumentList is null)
                                {
                                    continue;
                                }
                                var argument = attributeSyntax.ArgumentList.Arguments.First();
                                var funcText = string.Empty;
                                if (argument.Expression is LiteralExpressionSyntax)
                                {
                                    funcText = argument.Expression.GetText().ToString();
                                }
                                else if (argument.Expression is InterpolatedStringExpressionSyntax)
                                {
                                    funcText = argument.Expression.GetText().ToString();
                                }
                                else if (argument.Expression is InvocationExpressionSyntax)
                                {
                                    funcText = argument.Expression.GetText().ToString();
                                }
                                if (funcText.Contains("return"))
                                {
                                    getAttributeHasResult = true;
                                }
                            }
                            if (!getAttributeHasResult)
                            {
                                var diagnostic = Diagnostic.Create(AddGetNoResultError, attributeSyntaxList.Last().GetLocation(), fieldDeclaration.Declaration.Variables.First().Identifier.ValueText);
                                context.ReportDiagnostic(diagnostic);
                            }
                        }
                        if (fieldDeclaration.AttributeLists.Any(ProfilePropertyAutoGenerator.IsProfilePropertyAddSetAttribute))
                        {
                            var setAttributeSetResult = false;
                            var attributeSyntaxList = ProfilePropertyAutoGenerator.GetProfilePropertyAddSetAttributeList(fieldDeclaration);
                            foreach (var attributeSyntax in attributeSyntaxList)
                            {
                                if (attributeSyntax.ArgumentList is null)
                                {
                                    continue;
                                }
                                var argument = attributeSyntax.ArgumentList.Arguments.First();
                                var funcText = string.Empty;
                                if (argument.Expression is LiteralExpressionSyntax)
                                {
                                    funcText = argument.Expression.GetText().ToString();
                                }
                                else if (argument.Expression is InterpolatedStringExpressionSyntax)
                                {
                                    funcText = argument.Expression.GetText().ToString();
                                }
                                else if (argument.Expression is InvocationExpressionSyntax)
                                {
                                    funcText = argument.Expression.GetText().ToString();
                                }
                                if (Regex.IsMatch(funcText, $@"{fieldDeclaration.Declaration.Variables.First().Identifier.ValueText}\s*=\s*value"))
                                {
                                    setAttributeSetResult = true;
                                }
                            }
                            if (!setAttributeSetResult)
                            {
                                var diagnostic = Diagnostic.Create(AddSetNoSetResultWarning, attributeSyntaxList.Last().GetLocation(), fieldDeclaration.Declaration.Variables.First().Identifier.ValueText);
                                context.ReportDiagnostic(diagnostic);
                            }
                        }
                    }
                }
            }
        }
    }
}
