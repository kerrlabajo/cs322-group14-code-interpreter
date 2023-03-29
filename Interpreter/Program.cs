using Antlr4.Runtime;
using Interpreter.Grammar;

var fileName = "Grammar\\test.txt";
var text = File.ReadAllText(fileName);

var inputStream = new AntlrInputStream(text);
var speakLexer = new CodeGrammarLexer(inputStream);
var commonTokenStream = new CommonTokenStream(speakLexer);
var speakParser = new CodeGrammarParser(commonTokenStream);
var chatContext = speakParser.program();
var visitor = new CodeVisitor();
visitor.Visit(chatContext);