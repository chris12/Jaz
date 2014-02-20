using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JazInterpreter
{
    class Program
    {
        public static List<Node> jazInput = new List<Node>();
        public static Dictionary<string, int> mainVariables = new Dictionary<string, int>();
        public static Stack<int> locationBeforeCall = new Stack<int>();
        public static Stack<object> mainStack = new Stack<object>();
        public static List<Dictionary<string, int>> varTables = new List<Dictionary<string, int>>();
        public static int currentTable = 0;
        public static StringBuilder sb = new StringBuilder();
        public static string fileName;

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                fileName = args[1].ToString();
            }
            else
            {
                Console.WriteLine("Enter a file name");
                fileName = Console.ReadLine();
            }
            string line, command, value;
            int firstSpace;
            //Stack<object> mainStack = new Stack<object>();


            //Read in the .jaz file and build a list to work with
            using (StreamReader reader = new StreamReader(fileName))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        line = line.TrimStart();
                        firstSpace = line.IndexOf(" ");
                        command = line.Substring(0, firstSpace);
                        value = line.Substring(firstSpace).Trim();
                    }
                    catch (Exception)
                    {
                        command = line;
                        value = "";
                    }
                    Node tempNode = new Node(command, value);
                    jazInput.Add(tempNode);
                }
            }
            varTables.Add(new Dictionary<string, int>());

            for (int i = 0; i < jazInput.Count(); i++)
            {
                Node readNode = jazInput[i];
                i = processCommand(readNode, i, varTables[0]);
            }
            
        }

        private static int processCommand(Node readNode, int currentPosition, Dictionary<string, int> currentDict)
        {
            int first, second;

            switch (readNode.command)
            {
                case "show":
                    Console.WriteLine(readNode.value);
                    sb.Append(readNode.value);
                    sb.Append(Environment.NewLine);
                    return currentPosition;
                case "goto":
                    return getLabel(readNode);
                case "lvalue":
                    if (!currentDict.ContainsKey(readNode.value))
                    {
                        currentDict.Add(readNode.value, 0);
                    }
                    mainStack.Push(readNode.value);
                    return currentPosition;
                case "rvalue":
                    if (!currentDict.ContainsKey(readNode.value))
                        currentDict.Add(readNode.value, 0);
                    mainStack.Push(currentDict[readNode.value]);
                    return currentPosition;
                case "push":
                    mainStack.Push(readNode.value);
                    return currentPosition;
                case "pop":
                    mainStack.Pop();
                    return currentPosition;
                case "copy":
                    mainStack.Push(mainStack.Peek());
                    return currentPosition;
                case ":=":
                    var val = mainStack.Pop();
                    var var = mainStack.Pop();
                    currentDict[var.ToString()] = int.Parse(val.ToString());
                    return currentPosition;
                case "print":
                    Console.WriteLine(mainStack.Peek());
                    sb.Append(mainStack.Peek());
                    sb.Append(Environment.NewLine);
                    return currentPosition;
                case "call":
                    if (getLabel(readNode) > 0)
                        return getLabel(readNode);
                    return currentPosition;
                case "halt":
                    using (StreamWriter sw = new StreamWriter(fileName.Substring(0,fileName.LastIndexOf('.')) + ".out"))
                    {
                        sw.Write(sb);
                    }
                    Environment.Exit(0);
                    return currentPosition;
                case "gofalse":
                    if (mainStack.Peek().Equals(0))
                    {
                        return (getLabel(readNode));
                    }
                    return currentPosition;
                case "gotrue":
                    if (!mainStack.Peek().Equals(0))
                    {
                        return getLabel(readNode);
                    }
                    return currentPosition;
                case "+":
                    first = int.Parse(mainStack.Pop().ToString());
                    second = int.Parse(mainStack.Pop().ToString());
                    mainStack.Push(second + first);
                    return currentPosition;
                case "-":
                    first = int.Parse(mainStack.Pop().ToString());
                    second = int.Parse(mainStack.Pop().ToString());
                    mainStack.Push(second - first);
                    return currentPosition;
                case "*":
                    first = int.Parse(mainStack.Pop().ToString());
                    second = int.Parse(mainStack.Pop().ToString());
                    mainStack.Push(second * first);
                    return currentPosition;
                case "/":
                    first = int.Parse(mainStack.Pop().ToString());
                    second = int.Parse(mainStack.Pop().ToString());
                    mainStack.Push(second / first);
                    return currentPosition;
                case "div":
                    first = int.Parse(mainStack.Pop().ToString());
                    second = int.Parse(mainStack.Pop().ToString());
                    mainStack.Push(second % first);
                    return currentPosition;
                case "&":
                    first = int.Parse(mainStack.Pop().ToString());
                    second = int.Parse(mainStack.Pop().ToString());
                    mainStack.Push(second & first);
                    return currentPosition;
                case "!":
                    string binary = (mainStack.Pop().ToString());

                    char[] binAr = binary.ToCharArray();
                    for (int b = 0; b < binAr.Length; b++)
                    {
                        if (binAr[b].Equals('0'))
                            binAr[b] = '1';
                        else
                            binAr[b] = '0';
                    }
                    binary = new string(binAr);
                    mainStack.Push(binary);
                    return currentPosition;
                case "|":
                    first = int.Parse(mainStack.Pop().ToString());
                    second = int.Parse(mainStack.Pop().ToString());
                    mainStack.Push(second | first);
                    return currentPosition;
                case "<>":
                    first = int.Parse(mainStack.Pop().ToString());
                    second = int.Parse(mainStack.Pop().ToString());
                    mainStack.Push((second != first) ? 1 : 0);
                    return currentPosition;
                case "<=":
                    first = int.Parse(mainStack.Pop().ToString());
                    second = int.Parse(mainStack.Pop().ToString());
                    mainStack.Push((second <= first) ? 1 : 0);
                    return currentPosition;
                case ">=":
                    first = int.Parse(mainStack.Pop().ToString());
                    second = int.Parse(mainStack.Pop().ToString());
                    mainStack.Push((second >= first) ? 1 : 0);
                    return currentPosition;
                case "<":
                    first = int.Parse(mainStack.Pop().ToString());
                    second = int.Parse(mainStack.Pop().ToString());
                    mainStack.Push((second < first) ? 1 : 0);
                    return currentPosition;
                case ">":
                    first = int.Parse(mainStack.Pop().ToString());
                    second = int.Parse(mainStack.Pop().ToString());
                    mainStack.Push((second > first) ? 1 : 0);
                    return currentPosition;
                case "=":
                    first = int.Parse(mainStack.Pop().ToString());
                    second = int.Parse(mainStack.Pop().ToString());
                    mainStack.Push((second == first) ? 1 : 0);
                    return currentPosition;
                case "begin":
                    int h =  begin(currentPosition);
                    varTables.RemoveAt(currentTable);
                    currentTable--;
                    return h;
                default:
                    return currentPosition;
            }

        }

        private static int getLabel(Node readNode)
        {
            for (int x = 0; x < jazInput.Count; x++)
            {
                Node findLabel = jazInput[x];
                if (findLabel.command.Equals("label") && findLabel.value == readNode.value)
                {
                    return x;
                }
            }
            return -1;
        }

        public static int begin(int position)
        {
            bool beforeCall = true;
            bool beforeReturn = true;
            varTables.Add(new Dictionary<string,int>());
            currentTable++;

            for (int i = position + 1; i < jazInput.Count(); i++)
            {
                Node readNode = jazInput[i];
                switch (readNode.command)
                {
                    case "end":
                        return i;
                    case "return":
                        i = locationBeforeCall.Pop();
                        beforeReturn = false;
                        continue;
                    case "call":
                        beforeCall = false;
                        locationBeforeCall.Push(i);
                        i = getLabel(readNode);
                        continue;
                }
                if (beforeCall && readNode.command.Equals("rvalue"))
                    i = processCommand(readNode, i, varTables[currentTable -1]);
                else if (!beforeCall && !beforeReturn && (readNode.command.Equals("lvalue") || readNode.command.Equals(":=")))
                    i = processCommand(readNode, i, varTables[currentTable-1]);
                else i = processCommand(readNode, i, varTables[currentTable]);
            }

            return position;
        }
    }
}
