using System;
using System.Collections.Generic;
using System.Linq;
using Improbable.Gdk.CodeGeneration.Utils;

namespace Improbable.Gdk.CodeGeneration.CodeWriter.Scopes
{
    /// <summary>
    /// A unit of code with a scope and indented string output.
    /// </summary>
    public abstract class ScopeBlock : ICodeBlock
    {
        private static readonly string DefaultContentSeparator = $"{Environment.NewLine}{Environment.NewLine}";

        private readonly List<ICodeBlock> content = new List<ICodeBlock>();

        protected string Annotation;
        private readonly Text declaration;

        protected ScopeBlock(string declaration)
        {
            this.declaration = new Text(declaration);
        }

        internal void Add(ICodeBlock block)
        {
            content.Add(block);
        }

        // ReSharper disable once OptionalParameterHierarchyMismatch
        public virtual string Format(int indentLevel = 0)
        {
            return Format(indentLevel, DefaultContentSeparator);
        }

        protected string Format(int indentLevel, string contentSeparator)
        {
            var indent = CommonGeneratorUtils.GetIndentSpaces(indentLevel);

            var scopeAnnotation = string.Empty;
            if (!string.IsNullOrEmpty(Annotation))
            {
                scopeAnnotation = $"{indent}[{Annotation}]{Environment.NewLine}";
            }

            var scopeDeclaration = string.Empty;
            if (declaration.HasValue())
            {
                scopeDeclaration = $"{declaration.Format(indentLevel)}{Environment.NewLine}";
            }

            var scopeOutput = string.Empty;
            if (content.Any())
            {
                var indentedContents = content.Select(scopeContent => scopeContent.Format(indentLevel + 1));
                scopeOutput = $"{string.Join(contentSeparator, indentedContents)}{Environment.NewLine}";
            }

            return $"{scopeAnnotation}{scopeDeclaration}{indent}{{{Environment.NewLine}{scopeOutput}{indent}}}";
        }
    }
}
