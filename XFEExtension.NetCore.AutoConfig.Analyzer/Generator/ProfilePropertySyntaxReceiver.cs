using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace XFEExtension.NetCore.AutoConfig.Generator
{
    public class ProfilePropertySyntaxReceiver : ISyntaxReceiver
    {
        public List<FieldDeclarationSyntax> CandidateFields { get; } = new List<FieldDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is FieldDeclarationSyntax fieldDeclarationSyntax)
            {
                if (fieldDeclarationSyntax.AttributeLists.Any(ProfilePropertyAutoGenerator.IsProfilePropertyAttribute))
                {
                    CandidateFields.Add(fieldDeclarationSyntax);
                }
            }
        }
    }
}
