// The SyntaxHighlightingTextBox code was originally taken
// from the ULogViewer project, by Carina Studio.
//
// https://github.com/carina-studio/AppSuiteBase/tree/master/SyntaxHighlighting
//
// MIT License
// 
// Copyright(c) 2021 Carina Studio
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace Pororoca.Desktop.Controls;

/// <summary>
/// Set of syntax highlighting definition.
/// </summary>
public class SyntaxHighlightingDefinitionSet
{
    /// <summary>
    /// Get name of the set.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Get list of token definitions.
    /// </summary>
    public List<SyntaxHighlightingDefinition> TokenDefinitions { get; set; } = new();

    /// <summary>
    /// Check whether at least one token or span definition has been added to the set or not.
    /// </summary>
    public bool HasDefinitions => TokenDefinitions.Count > 0;

    /// <summary>
    /// Check whether at least one token or span definition in the set is valid or not.
    /// </summary>
    public bool HasValidDefinitions => TokenDefinitions.Any(x => x.IsValid);

    /// <summary>
    /// Initialize new <see cref="SyntaxHighlightingDefinitionSet"/> instance.
    /// </summary>
    /// <param name="name">Name.</param>
    public SyntaxHighlightingDefinitionSet(string name) =>
        Name = name;

    /// <inheritdoc/>
    public override string ToString() =>
        $"{{{Name}}}";
}