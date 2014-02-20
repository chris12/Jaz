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
        public static Dictionary<string,int> mainVariables = new Dictionary<string,int>();
        public static Stack<int> locationBeforeCall = new Stack<int>();
        public static Stack<object> mainStack = new Stack<object>();
        
        static void Main(string[] args)
        {
            string fileName;
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
                        line = line.TrimStart(" ".ToArray());
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


            for (int i = 0; i < jazInput.Count(); i++)
            {
                Node readNode = jazInput[i];
                i = processCommand(readNode, i, ref mainVariables);
            }
            Console.ReadLine();
        }

        private static int processCommand(Node readNode, int currentPosition ,ref Dictionary<string, int> currentDict)
        {
            int first, second;

            switch (readNode.command)
            {
                case "show":
                    Console.WriteLine(readNode.value);
                    return currentPosition;
                case "goto":
                    return getLabel(readNode);
                case "lvalue":
                    if(!currentDict.ContainsKey(readNode.value))
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
                case "pop" :
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
                    return currentPosition;
                case "call":
                    if (getLabel(readNode) > 0)
                        return getLabel(readNode);
                    return currentPosition;
                case "halt":
                    Console.WriteLine("Halt reached press any key to end");
                    Console.ReadLine();
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
                case "div" :
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
                case"begin":
                    return begin(currentPosition,ref currentDict);
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

        public static int begin(int position, ref Dictionary<string, int> passedDict)
        {
            bool beforeCall = true;
            bool beforeReturn = true;
            Dictionary<string, int> functionDict = new Dictionary<string, int>();

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
                    processCommand(readNode, position,ref passedDict);
                else if (!beforeCall && !beforeReturn && (readNode.command.Equals("lvalue") || readNode.command.Equals(":=")))
                    processCommand(readNode, position,ref passedDict);
                else processCommand(readNode, position, ref functionDict);
            }

            return position;
        }
    }
}
