using Antlr4.Runtime;
using Interpreter.Grammar;

try
{
    var fileName = "Grammar\\test.txt";
    var text = File.ReadAllText(fileName);

    var inputStream = new AntlrInputStream(text);
    var speakLexer = new CodeGrammarLexer(inputStream);
    var commonTokenStream = new CommonTokenStream(speakLexer);
    var speakParser = new CodeGrammarParser(commonTokenStream);
    var errorListener = new ErrorListener(); // replace MyErrorListener with the name of your error listener class
    speakParser.AddErrorListener(errorListener);
    var chatContext = speakParser.program();
    var visitor = new CodeVisitor();
    visitor.Visit(chatContext);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}