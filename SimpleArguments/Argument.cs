using System;
using System.Collections.Generic;

namespace eu.nerdfactor.SimpleArguments {
	/// <summary>
	/// Simple management for command line arguments.
	/// Uses static functions to manage a list of Arguments with not the most optimal perfomance. 
	/// But this is not the goal. It should be a very simple drop-in Argument management solution.
	/// You can create something like a ArgumentManager class if you prefer this to be done in a 
	/// object instead.
	/// </summary>
	public class Argument {

		/// <summary>
		/// Name of the argument.
		/// </summary>
		public String Name { get; set; }

		/// <summary>
		/// A shortcode alias of the argument.
		/// If this is empty, the first character of the name
		/// will be used as a alias.
		/// </summary>
		public String Alias { get; set; }

		/// <summary>
		/// Some description of the argument.
		/// Can be used in help text?
		/// </summary>
		public String Description { get; set; }

		/// <summary>
		/// Value of the argument.
		/// </summary>
		private String value;
		public String Value {
			get { return this.value; }
			set {			
				this.value = value;
				if (value != null && value != "") {
					// Encapsulate the property to make sure that the Exists
					// property is set correctly. You could do it the other
					// way around, but how to check if an argment that is
					// supposed to have a empty value exists.
					this.Exists = true;
				}
			}
		}

		/// <summary>
		/// Determine if the argument exists.
		/// A Argument should always be in the argument list in order to use
		/// the description for help text. That's why you can just check if the
		/// argument is null and have to check the value.
		/// </summary>
		public bool Exists { get; set; }

		/// <summary>
		/// The argument depends on other arguments.
		/// Just very simple one to one dependencies.
		/// </summary>
		public String[] Dependencies { get; set; }

		/// <summary>
		/// The action for the argument.
		/// </summary>
		public Func<List<Argument>, int> Action { get; set; }

		/// <summary>
		/// Determine if this action should be the last executed action.
		/// </summary>
		public bool LastAction { get; set; }

		/// <summary>
		/// A command line argument.
		/// </summary>
		public Argument() {}

		/// <summary>
		/// A command line argument.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="action"></param>
		public Argument(String name, Func<List<Argument>, int> action) : this(name, "", "", "", null, action, false) {

		}

		/// <summary>
		/// A command line argument.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="alias"></param>
		/// <param name="description"></param>
		/// <param name="value"></param>
		/// <param name="dependencies"></param>
		/// <param name="action"></param>
		/// <param name="lastAction"></param>
		public Argument(String name, 
			String alias = "", 
			String description = "", 
			String value = "", 
			String[] dependencies = null, 
			Func<List<Argument>, int> action = null, 
			bool lastAction = false) {

			this.Name = name;
			this.Alias = alias;
			this.Description = description;
			this.Value = value;
			this.Dependencies = dependencies;
			this.Action = action;
			this.LastAction = lastAction;
		}

		/// <summary>
		/// Check if the dependencies between the arguments is satisfied.
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public bool DependenciesSatisfied(List<Argument> arguments) {
			if (this.Dependencies == null || this.Dependencies.Length < 1) {
				return true;
			}
			foreach (String d in this.Dependencies) {
				foreach (Argument a in arguments) {
					if (a.Name == d && !a.Exists) {
						return false;
					}
				}
			}
			return true;
		}


		#region StaticManagementFunctions

		/// <summary>
		/// Parse command line arguments.
		/// Take the args array from the command line and fill the Argument list with its values.
		/// </summary>
		/// <param name="argsArray">Command line arguments.</param>
		/// <param name="arguments">List of possible Arguments.</param>
		/// <param name="argumentPrefix">Specifiy the possible prefixes for Arguments.</param>
		/// <returns></returns>
		public static List<Argument> ParseArgsArray(string[] argsArray, List<Argument> arguments, List<Char> argumentPrefix = null, bool autoAddHelp = true) {
			// As long as C# doesn't support list as default values in parameters, use this workaround
			argumentPrefix = argumentPrefix ?? new List<Char>() { '/', '-' };

			// The Dictionary will hold the values from the argsArray
			Dictionary<string, string> dic = new Dictionary<string, string>();
			if(argsArray.Length == 0 && autoAddHelp) {
				// If there are no args, add a help Argument.
				dic.Add("help", "");
			} else {
				for (int i = 0; i < argsArray.Length; i++) {
					// Check if the next arg starts with / or - to determine if it is a Argument.
					if (argsArray[i] != "" && argumentPrefix.Contains(argsArray[i][0]) && argsArray[i] != null) {
						var key = argsArray[i].TrimStart(new Char[] { '/', '-', ' ' });
						var value = "";
						// If the following arg doesn't start with / or - it is a value for the current Argument.
						if (i + 1 < argsArray.Length && !argumentPrefix.Contains(argsArray[i + 1][0])) {
							value = argsArray[i + 1];
							i++;
						}
						// Add the Argument with its value
						dic.Add(key, value);
					}
				}
			}

			// Now check if the found Arguments are provided as possible Arguments
			// and fill the values.
			foreach (Argument a in arguments) {
				// Use the name, the first character of the name or the alias to identify Arguments.
				foreach (String s in new String[] { a.Name, a.Name[0].ToString(), a.Alias }) {
					if (s != null && dic.ContainsKey(s)) {
						a.Value = dic[s];
						a.Exists = true;
					}
				}
			}

			return arguments;
		}

		/// <summary>
		/// Get a specific Argument from the list.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public static Argument GetArgument(String name, List<Argument> arguments) {
			foreach (Argument a in arguments) {
				if (a.Name == name) {
					return a;
				}
			}
			return null;
		}

		/// <summary>
		/// Check if the list as a specific Argument.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public static bool HasArgument(String name, List<Argument> arguments) {
			if(name == null || name.Length == 0 || arguments == null || arguments.Count == 0) {
				return false;
			}
			foreach (Argument a in arguments) {
				if (a.Name == name && a.Exists) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Check if the list has any Arguments with a action.
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public static bool HasActionArgument(List<Argument> arguments) {
			foreach (Argument a in arguments) {
				if (a.Action != null && a.Exists) {
					return true;
				}
			}
			return false;
		}


		/// <summary>
		/// Execute arguments in the list.
		/// Use Exceptions in the Arguments or specific return values to provide
		/// information to the outside about the execution.
		/// </summary>
		/// <param name="arguments"></param>
		public static int ExecuteArguments(List<Argument> arguments) {
			int result = 0;
			foreach (Argument a in arguments) {
				if (a.Action != null && a.DependenciesSatisfied(arguments) && a.Exists) {
					result = a.Action(arguments);
					// Check if this should have been the last action to execute.
					if (a.LastAction) {
						break;
					}
				}
			}
			return result;
		}

		#endregion
	}
}
