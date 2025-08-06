
1. All .cs scripts in the archive

## ProgramExec
	GameControllerBase.cs
	PerScene_0.cs
	ProgramExec
	ConsoleManager.cs
	CoroutineRunner.cs
	GameBuiltinMethods.cs
	GlobalScriptManager.cs
	PythonAST.cs
	PythonInterpreter.cs
	PythonLexer.cs
	PythonParser.cs
	PythonToken.cs
	ScriptRunner.cs
## TextEditor
	DragableItem.cs
	InputFieldLineNumbers.cs
	PythonCodeEditorSyntaxHighlight.cs
## ad
	LoadNextScene.cs

## UTIL scripts
	INITManager.cs
	UTIL/UTIL.cs




### PythonToken.cs (complete leaf)
```cs
	enum TokenType
	class Token (leaf)
		-> enum TokenType
```


### PythonLexer.cs ( -> PythonToken.cs)
```cs
	class PythonLexer
		-> class Token
		-> enum TokenType
```


### PythonAST.cs (complete leaf)
```cs
	class Expressions (subclass of `Expr`)
	1. `NumberExpr`
	2. `StringExpr`
	3. `BooleanExpr`
	4. `NameExpr`
	5. `ListExpr`
	6. `BinaryExpr`
	7. `UnaryExpr`
	8. `CallExpr`
	9. `AttributeExpr`
	10. `IndexExpr`
	11. `SliceExpr`

	class Statements (subclass of `Stmt`)
	12. `ExpressionStmt`
	13. `AssignStmt`
	14. `InPlaceAssignStmt`
	15. `IfStmt`
	16. `WhileStmt`
	17. `ForStmt`
	18. `FunctionDefStmt`
	19. `ReturnStmt`
	20. `PassStmt`
	21. `GlobalStmt`
	22. `BreakStmt`
	23. `ContinueStmt`

	each of these classes
		-> enum TokenType

	🧱 Abstract base classes
	class
	* `AstNode`
	* `Expr`
	* `Stmt`
```
	


### PythonParser.cs (  -> PythonToken.cs,  -> PythonAST.cs)
```cs
	class PythonParser
		-> class Token
		-> enum TokenType
		-> subClasses Stmt (example: return new IfStmt(condition, thenBranch, elseBranch, ifToken.Line);)
		-> SubClasses Expr (example: return new BooleanExpr(val, token.Line);)

		-----------------------------------------------------
		public PythonParser(List<Token> tokens) (constructor)
		public List<Stmt> Parse()
```



### PythonInterpreter.cs ( -> PythonToken.cs,  -> PythonAST.cs, -> ConsoleManager.cs, GameBuiltinMethods.cs )
```cs
	class ReturnException
	class BreakException
	class ContinueException
	class PythonInterpreter
		-> enum TokenType
		-> subclasses of Stmt (example: else if (stmt is IfStmt ifs))
		-> subclasses of Expr (example: else if (expr is BooleanExpr booe))
		-> method ExecutionTracker.NotifyLineExecution(int lineNum)
		-> method ConsoleManager.AddMessage(output, ConsoleMessageType.Print);
		-> method GameBuiltinMethods.IsBuiltinFunction(fname)
		-> IEnumerator GameBuiltinMethods.ExecuteBuiltinFunction(fname, args.ToArray(), setValue)

		-------------------------------------------------
		public IEnumerator Execute(List<Stmt> statements)
```
	



### ConsoleManager.cs (complete leaf) `using TMPro.TMP_InputField`
```cs
	class ConsoleManager
		--------------------------------
		public static method AddMessage(string)
		public static method LogInfo(string)
		public static method LogError(string)
		public static method Clear()
	enum ConsoleMessageType
```


### CoroutineRunner.cs ( -> ConsoleManager.cs, -> GlobalScriptManager.cs)
```cs
	class CoroutineRunner
		-----------------------------
		static IEnumerator SafeExecute

		-> method ConsoleManager.LogError(errorMessage)
		-> method GlobalScriptManager.ResetAllRunners();
```



### GlobalScriptManager.cs ( -> ScriptRunner.cs )
```cs
	static class GlobalScriptManager
		------------------------------------
		public static event OnStopAllRunners
		public static event OnClearConsole

		public static void RegisterRunner(ScriptRunner runner)
		public static void UnregisterRunner(ScriptRunner runner)
		public static void StartRunner(ScriptRunner runner)
		public static void ResetAllRunners()
		public static void OnScriptError(ScriptRunner erroredRunner)
		public static void OnScriptComplete(ScriptRunner completedRunner)
		public static ScriptRunner GetCurrentRunningScript()
		public static bool IsAnyScriptRunning()
		public static void Cleanup()

		-> private field
			static HashSet<ScriptRunner> registeredRunners
		-> ScriptRunner.SetState()
			// example: 
				r.SetState(ScriptRunner.ScriptState.Running);
				r.SetState(ScriptRunner.ScriptState.Stop);
				// these .SetState are called in public method mentioned above that get to deal with ScriptRunner()

```


### ScriptRunner.cs ( -> PythonLexer.cs, -> PythonParser.cs, -> PythonAST.cs, -> PythonInterpreter.cs, -> ConsoleManager.cs,  -> CoroutineRunner.cs, -> GlobalScriptRunner.cs, GameBuiltinMethods.cs ) `using TMPro.TMP_InputField`

----------------------------------------------------------------------------------------------------------------------
```cs
	 class ScriptRunner : MonoBehaviour
	 	enum ScriptState(stop, running)
		-> ExecutionTracker
		-> PythonLexer
		-> PythonParser
		-> Stmt
		-> PythonInterpreter
		-> ConsoleManager
		-> CoroutineRunner
		-> GlobalScriptRunner
		-> GameBuiltinMethods
	
		--------------------
		public void StopExecution()
		public void SetState(ScriptState newState)
		public ScriptState GetCurrentState()

		-> example to python flow kind script reference: (method + routine)
			void RunScript()
				var lexer = new PythonLexer(scriptInput.text);
				var parser = new PythonParser(lexer.Tokens);
				List<Stmt> ast = parser.Parse();
				var interpreter = new PythonInterpreter();
				ExecutionTracker.NotifyExecutionStarted(this);
				StartCoroutine(ExecuteWithSceneReset(interpreter, ast));
			IEnumerator ExecuteWithSceneReset()
				yield return CoroutineRunner.SafeExecute(
					interpreter.Execute(ast),
					stepDelay,
					ReportError,
					OnExecutionComplete
				);

		->  method ConsoleManager.LogInfo("===stop===");
			method ConsoleManager.LogInfo("===reset===");
			method ConsoleManager.LogError(errorMessage);
			method ConsoleManager.LogInfo("===stop===");

		->  method GlobalScriptManager.RegisterRunner(this);
			subscribeChannel GlobalScriptManager.OnStopAllRunners += StopExecution;
			subscribeChannel GlobalScriptManager.OnClearConsole += ClearConsole;

			method GlobalScriptManager.UnregisterRunner(this);
			unSubscribeChannel GlobalScriptManager.OnStopAllRunners -= StopExecution;
			unSubscribeChannel GlobalScriptManager.OnClearConsole -= ClearConsole;

			GlobalScriptManager.StartRunner(this); 		// (when button pressed)
			GlobalScriptManager.ResetAllRunners();
			GlobalScriptManager.OnScriptError(this); 	// inside this.RunScript()
			GlobalScriptManager.OnScriptComplete(this); // inside this.OnExecutionComplete()
			GlobalScriptManager.OnScriptError(this);	// inside ReportError(string)
			(GlobalScriptManager.GetCurrentRunningScript() == this) // only report to user if script stopped by reset button, not by other script run and auto stop all

		-> IEnumerator GameBuiltinMethods.ResetScene();	

	class ExecutionTracker
		--------------------------------------
		public static event  OnLineExecuted (Channel)
		public static event  OnExecutionStarted (Channel)
		public static event  OnExecutionStopped (Channel)

		public static void NotifyLineExecution(int lineNumber)
		public static void NotifyExecutionStarted(ScriptRunner script)
		public static void NotifyExecutionStopped()
		public static ScriptRunner GetCurrentExecutingScript()
		public static ScriptRunner GetCurrentExecutingScript()
```


### InputFieldLineNumbers.cs ( -> ScriptRunner.cs ) `just an if check

----------------------------------------------------------------------------------------------------------------------
```cs
	class InputFieldLineNumbers : MonoBehaviour 
		-> ScriptRunner

		[SerializeField] ScriptRunner associatedScript;
		// an if check Only update if this was our associated script(taken from [SerielizeField])
		if (executingScriptRunner == this.associatedScriptRunner) 
			// these checks is made in private
			method OnExecutionStarted(ScriptRunner),  
			method OnExecutionStopped(ScriptRunner executingScript)
			method OnLineExecuted(ScriptRunner executingScript, int lineNumber)

		public void ForceSyncScroll()
		public void SetSyncUpdateInterval(float interval)
```


### DraggableItem.cs( leaf ) 
	class DraggableItem : MonoBehaviour, , IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler


### PythonCodeEditorSyntaxHighlight.cs( -> )
	

-----------------------------------------------------------------------------------------------------------
```cs
class PythonCodeEditorSyntaxHighlight: MonoBehaviour
	-> class SyntaxHighlighter
	-> static class GameBuiltinMethods
	public HashSet<string> PythonKeywords ("if", "while", "for", "def", "True", "False" .... )
	public HashSet<string> PythonBuiltins ("len", "sleep", "range", "print", "str" .... )
	public string GetPlainText() // Get the raw text without rich text formatting
	public void SetText(string text)
		-> method GameBuiltinMethods.GetAllAvailableCommands()
```
-----------------------------------------------------------------------------------------------------------------------
```cs
	class SyntaxHighlighter
		-> class TextSegment
	public void Initialize(Color keyword, Color stringCol, Color number, Color comment, .... )
	public string ProcessText(string text) 
		// Flow inside ProcessText
		// using private method
		List<TextSegment> ProcessSegments(string text) 
		string ProcessLine(string line)
```

-----------------------------------------------------------------------------------------------------------------------
```cs
class TextSegment

		public int Start
		public int Length
		public string ColorCode // Hex Style #EAEAEA
```

### GameBuiltinMethods.cs( -> GameControllerBase.cs)
	class GameBuiltinMethods
		-> class GameControllerBase

		FLOW: 
		// RegisterGameController()
		// if Any of GameControllerBase.MAP_actionCmd, _predicateCmd, valueCmd 
			contains key as "function_name" than perform action or routine or return bool
		// UnregisterGameController()

------------------------------------------------------------------------------------------------------------
```cs
	public static void RegisterGameController(GameControllerBase controller) // does sceneController = controller
	public static void UnregisterGameController()
	public static GameControllerBase GetCurrentController()

	public static bool IsBuiltinFunction(string functionName)
	public static List<string> GetAllAvailableCommands() // for that scene/GameControllerBase Attached
	public static IEnumerator ExecuteBuiltinFunction(string functionName, object[] args, Action<object> setValue)
	public static IEnumerator ResetScene()
```

### GameControllerBase.cs( -> GameBuiltinMethods.cs )
	class GameControllerBase
		-> GameBuiltinMethods

		-> method GameBuiltinMethods.RegisterGameController(this)
		-> method GameBuiltinMethods.UnregisterGameController() // scenecontroller = null

------------------------------------------------------------------------------------------------------------
```cs
	public Dictionary<string, Func<object[], IEnumerator>> actionCommands
	public Dictionary<string, Func<object[], IEnumerator>> predicateCommands
	public Dictionary<string, Func<object[], object>> valueGetterCommands


	public abstract IEnumerator SceneReset()
	public List<string> GetAllCommandNames()
	public IEnumerator ExecuteActionCommand(string commandName, object[] args)
	public IEnumerator ExecutePredicateCommand(string commandName, object[] args, System.Action<bool> onResult)
	public object ExecuteValueGetterCommand(string commandName, object[] args)
	public bool HasCommand(string commandName)
		

	void Awake()
		RegisterCommands();

```
		

### GameControllerBase_0.cs ( -> GameBuiltinMethods.cs )
	----------------------------------------
	all [SerielizedField] as per requirement

```cs
overide void RegisterCommands()
	RegisterAction("say", SayCommand);
	RegisterAction("submit", SubmitCommand);
	RegisterAction("move", MoveCommand);
	// a function that intake object[] and output IEnumerator
	RegisterAction(string commandName, Func<object[], IEnumerator> action) 
		this.actionCommands[commandName] = action; // created in base class
	RegisterPredicate(string commandName, Func<object[], IEnumerator> predicate)
		this.predicateCommands[commandName] = predicate; // created in base class
	RegisterValueGetter(string commandName, Func<object[], object> getter)
		this.valueGetterCommands[commandName] = getter; // created in base class
```

#### example actionCommand

```cs
	[Header("move")]
	[SerializeField] float move_duration = 0.3f;
	private IEnumerator MoveCommand(object[] args)
	{
		if (args.Length == 2)
		{
			string dx = args[0].ToString().ToLower();
			string dy = args[1].ToString().ToLower();
			Debug.Log(dx + "//" + dy);
			if (!(dx.fmatch(@"^([-+]?1|0)$") && dy.fmatch(@"^([-+]?1|0)$")))
			{
				throw new Exception("invalid direction x, y can only be -1, 0, +1");
			}
			Vector3 moveVector = new Vector2(dx.parseInt(), dy.parseInt());
			yield return util_moveSquashAnim(this.playerTransform, duration: this.move_duration, moveVector, 0.9f, 0.9f);
		}
		else if (args.Length == 1)
		{
			string direction = args[0].ToString().ToLower();
			Vector3 moveVector = Vector3.zero;
			switch (direction)
			{
				case "up": moveVector = Vector3.up; break;
				case "down": moveVector = Vector3.down; break;
				case "left": moveVector = Vector3.left; break;
				case "right": moveVector = Vector3.right; break;
				default:
					throw new Exception($"Invalid dir: {direction}, can only be 'east', 'west', 'north', 'south'");
			}
			yield return util_moveSquashAnim(this.playerTransform, duration: this.move_duration, moveVector, 0.9f, 0.9f);
		}
		else
		{
			throw new Exception("move() can take only either 1 or 2 argument");
		}
		#region commented old method
		/*
		if (playerTransform != null)
		{
			Vector3 startPos = playerTransform.position;
			Vector3 endPos = startPos + moveVector;
			float elapsed = 0f;
			while (elapsed < 1f)
			{
				elapsed += Time.deltaTime * moveSpeed;
				playerTransform.position = Vector3.Lerp(startPos, endPos, elapsed);
				yield return null;
			}
			playerTransform.position = endPos;
		}
		*/
		#endregion
	}
```

#### example predicateCommand
```cs
		#region is_goal(x, y)
		private IEnumerator IsGoalCommand(object[] args)
		{
			if (args.Length == 2)
			{
				string str_x = args[0].ToString().ToLower();
				string str_y = args[1].ToString().ToLower();

				Debug.Log(str_x + "//" + str_y);

				// @"^([-+]?1|0)$"
				// ^-?\d+$

				if (!(str_x.fmatch(@"^([-+]?\d+)$") && str_y.fmatch(@"^([-+]?\d+)$")))
				{
					throw new Exception($"invalid direction {str_x}, {str_y}");
				}

				v2 pos = new v2(str_x.parseInt(), str_y.parseInt() );

				bool result = (pos == this.goalPos);
				SetPredicateResult("is_goal", result);
				yield return null;
			}
			else
			{
				throw new Exception("is_goal() can exactly 2 argument");
			}

			//if (args.Length != 0)
			//	throw new Exception("is_player() takes no arguments");
			//yield return null;
			//bool result = (this.playerTransform.gameObject.name.ToLower() == "player");
			//SetPredicateResult("is_player", result);
		}
		#endregion
```

#### example valueGetterCommand
```cs
		#region get_goal_x, _y
		private object GetGoalXCommand(object[] args)
		{
			if (args.Length != 0)
				throw new Exception("get_goal_x() takes no args");

			return this.goalPos.x;
		}
		private object GetGoalYCommand(object[] args)
		{
			if (args.Length != 0)
				throw new Exception("get_goal_y() takes no args");

			return this.goalPos.y;
		} 
		#endregion
```