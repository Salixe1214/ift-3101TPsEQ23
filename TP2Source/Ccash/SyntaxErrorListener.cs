using System.IO;
using Antlr4.Runtime;
using Ccash.SemanticAnalysis;

namespace Ccash
{
    public class SyntaxErrorListener: BaseErrorListener, IAntlrErrorListener<int>
    {
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
                                         string msg, RecognitionException e)
        {
            ErrorManager.AddSyntaxError(msg, line, charPositionInLine);
        }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine,
                                string msg, RecognitionException e)
        {
            ErrorManager.AddSyntaxError(msg, line, charPositionInLine);
        }
    }
}
