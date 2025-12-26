using System;
using UnityEngine;

namespace PythonInterpreter
{
    /// <summary>
    /// Contains demo scripts for testing the Python interpreter
    /// These scripts test all major features: lists, loops, classes, algorithms, etc.
    /// </summary>
    public static class DemoScripts
    {
        #region Script 1: The Kitchen Sink
        public static readonly string Script1_KitchenSink = @"# List Initialization
items = [0, 1, 2, 3, 4, 5]
print(""Original:"", items)

# Slicing
print(""Slice [1:4]:"", items[1:4])
print(""Slice [:3]:"", items[:3])
print(""Slice [3:]:"", items[3:])

# Negative Indexing
print(""Last item:"", items[-1])
print(""Second last:"", items[-2])

# Modification
items.append(6)
items.pop()
items[0] = 99
print(""Modified:"", items)
";
        #endregion

        #region Script 2: The Deep Diver
        public static readonly string Script2_DeepDiver = @"# Nested Loops (3 Levels)
count = 0
for i in range(3):
    for j in range(2):
        while count < 5:
            count += 1
            print(""Loop depth:"", i, j, count)

# Bitwise Operations
a = 60
b = 13
c = a & b
d = a | b
e = a ^ b
print(""Bitwise AND:"", c)

# Recursion
def fib(n):
    if n <= 1: return n
    return fib(n-1) + fib(n-2)

print(""Fibonacci(6):"", fib(6))
";
        #endregion

        #region Script 3: Object Oriented
        public static readonly string Script3_ObjectOriented = @"class Robot:
    def __init__(self, name):
        self.name = name
        self.battery = 100
    
    def work(self, cost):
        self.battery -= cost
        print(self.name + "" working. Battery: "" + str(self.battery))

bot = Robot(""FarmerBot"")
bot.work(10)
bot.work(20)
";
        #endregion

        #region Script 4: Data Structures
        public static readonly string Script4_DataStructures = @"# Dictionary
data = {""x"": 10, ""y"": 20}
print(data[""x""])

# Complex Nesting
complex_obj = [
    {""id"": 1, ""tags"": [""a"", ""b""]}, 
    {""id"": 2, ""tags"": [""c""]}
]
print(complex_obj[0][""tags""][1])

# Modifying deep element
complex_obj[1][""tags""].append(""d"")
print(complex_obj[1])
";
        #endregion

        #region Script 5: Game Interaction
        public static readonly string Script5_GameInteraction = @"say(""Starting Mission..."")
sleep(1.0)

x = get_pos_x()
y = get_pos_y()

while not is_goal(x, y):
    if is_block(x + 1, y):
        say(""Obstacle ahead!"")
        move(""up"")
        sleep(0.5)
    elif can_move(""right""):
        move(""right"")
    else:
        say(""Stuck!"")
        break
    
    x = get_pos_x()
    y = get_pos_y()

submit(""password123"")
";
        #endregion

        #region Script 6: Advanced Features
        public static readonly string Script6_AdvancedFeatures = @"# List Comprehension
nums = [1, 2, 3, 4, 5]
squares = [x * x for x in nums if x % 2 == 1]
print(""Odd Squares:"", squares)

# String Operations
text = ""apple,banana,cherry""
fruits = text.split("","")
print(""Splitted:"", fruits)

joined = "" | "".join(fruits)
print(""Joined:"", joined)

# Lambda Sorting
class Node:
    def __init__(self, val):
        self.val = val

nodes = [Node(10), Node(2), Node(5)]
nodes.sort(key=lambda n: n.val)

print(""Sorted Nodes:"", [n.val for n in nodes])
";
        #endregion

        #region Script 7: Pathfinding (A* Simulation)
        public static readonly string Script7_Pathfinding = @"grid = [[0, 0, 0], [0, 1, 0], [0, 0, 0]]
start = [0, 0]
end = [2, 2]

def get_neighbors(pos):
    res = []
    x = pos[0]
    y = pos[1]
    if x > 0: res.append([x-1, y])
    if x < 2: res.append([x+1, y])
    if y > 0: res.append([x, y-1])
    if y < 2: res.append([x, y+1])
    return res

def heuristic(a, b):
    return abs(a[0] - b[0]) + abs(a[1] - b[1])

open_list = [start]
came_from = {}
g_score = {str(start): 0}
f_score = {str(start): heuristic(start, end)}

while len(open_list) > 0:
    current = open_list[0]
    best_f = f_score.get(str(current), 999)
    
    for node in open_list:
        f = f_score.get(str(node), 999)
        if f < best_f:
            current = node
            best_f = f
            
    if current == end:
        print(""Path Found!"")
        break
        
    open_list.remove(current)
    
    for neighbor in get_neighbors(current):
        if grid[neighbor[1]][neighbor[0]] == 1:
            continue
            
        tentative_g = g_score.get(str(current), 999) + 1
        if tentative_g < g_score.get(str(neighbor), 999):
            came_from[str(neighbor)] = current
            g_score[str(neighbor)] = tentative_g
            f_score[str(neighbor)] = tentative_g + heuristic(neighbor, end)
            if neighbor not in open_list:
                open_list.append(neighbor)
";
        #endregion

        #region Simple Test Scripts
        public static readonly string SimpleTest_HelloWorld = @"print(""Hello, World!"")
print(""Python-like interpreter in Unity"")
";

        public static readonly string SimpleTest_Variables = @"x = 10
y = 20
z = x + y
print(""Result:"", z)
";

        public static readonly string SimpleTest_Loop = @"for i in range(5):
    print(""Count:"", i)
";

        public static readonly string SimpleTest_Function = @"def greet(name):
    print(""Hello, "" + name + ""!"")

greet(""Alice"")
greet(""Bob"")
";

        public static readonly string SimpleTest_Conditionals = @"x = 15

if x > 10:
    print(""x is greater than 10"")
elif x > 5:
    print(""x is greater than 5 but not 10"")
else:
    print(""x is 5 or less"")
";
        #endregion

        #region All Scripts Array
        /// <summary>
        /// All test scripts in order
        /// </summary>
        public static readonly string[] AllScripts = new string[]
        {
            Script1_KitchenSink,
            Script2_DeepDiver,
            Script3_ObjectOriented,
            Script4_DataStructures,
            Script5_GameInteraction,
            Script6_AdvancedFeatures,
            Script7_Pathfinding
        };

        /// <summary>
        /// Simple test scripts for quick validation
        /// </summary>
        public static readonly string[] SimpleScripts = new string[]
        {
            SimpleTest_HelloWorld,
            SimpleTest_Variables,
            SimpleTest_Loop,
            SimpleTest_Function,
            SimpleTest_Conditionals
        };
        #endregion

        #region Script Names
        public static readonly string[] ScriptNames = new string[]
        {
            "1. Kitchen Sink (Lists, Slicing, Indexing)",
            "2. Deep Diver (Nested Loops, Bitwise, Recursion)",
            "3. Object Oriented (Classes, Methods, Self)",
            "4. Data Structures (Dicts, Nesting)",
            "5. Game Interaction (Yielding, Sleep, Predicates)",
            "6. Advanced Features (List Comp, Lambdas, Strings)",
            "7. Pathfinding (A* Algorithm Simulation)"
        };

        public static readonly string[] SimpleScriptNames = new string[]
        {
            "Hello World",
            "Variables",
            "Loop",
            "Function",
            "Conditionals"
        };
        #endregion

        #region Helper Methods
        /// <summary>
        /// Gets a script by index
        /// </summary>
        public static string GetScript(int index)
        {
            if (index < 0 || index >= AllScripts.Length)
            {
                Debug.LogError("Script index out of range: " + index);
                return SimpleTest_HelloWorld;
            }
            return AllScripts[index];
        }

        /// <summary>
        /// Gets a simple script by index
        /// </summary>
        public static string GetSimpleScript(int index)
        {
            if (index < 0 || index >= SimpleScripts.Length)
            {
                Debug.LogError("Simple script index out of range: " + index);
                return SimpleTest_HelloWorld;
            }
            return SimpleScripts[index];
        }

        /// <summary>
        /// Gets script name by index
        /// </summary>
        public static string GetScriptName(int index)
        {
            if (index < 0 || index >= ScriptNames.Length)
                return "Unknown Script";
            return ScriptNames[index];
        }
        #endregion
    }
}
