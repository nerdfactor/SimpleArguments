using System;
using System.Collections.Generic;

namespace eu.nerdfactor.SimpleArguments {
	/// <summary>
	/// Example Program that shows how to use a very simple Argument 
	/// management solution.
	/// </summary>
	public class Program {

		// Use constants to name Arguments just to be sure not to have typos.
		const string ARG_SOME = "some";
		const string ARG_THING = "thing";
		const string ARG_DIFF = "diff";
		const string ARG_OUTPUT = "output";
		const string ARG_FORCE = "force";
		const string ARG_SILENT = "silent";

		static int Main(string[] args) {
			// Create a list of the possible arguments and parse the command line arguments to fill 
			// them with values. Define the Arguments it the order you want them to be executed.
			List<Argument> arguments = new List<Argument>();
			try {
				arguments = Argument.ParseArgsArray(args, new List<Argument>() {
					// A very simple Argument that executes a action. It requires some value to be passed
					// through a different Argument. The action is defined with a static function.
					new Argument(ARG_SOME, null, "Do some action.", null, new string[]{ARG_THING}, SomeAction),

					// A Argument to provide some value. Maybe a path to some configuration file. Has a default
					// value that will be used if the arg doesn't exist.
					new Argument(ARG_THING, null, "Add information about a thing.", "thing.ini", null, null),

					// A different Argument to execute a action. This action is defined inline. Use it for
					// very short tasks. For example just call a method on some  controller or manager type
					// object.
					new Argument(ARG_DIFF, null, "Do a different action.", null, null, (List<Argument> a) => {
						// Do something short in here ...
						return 0;
					}),

					// A Argument to provide some value. Maybe a path to the logfile.
					new Argument(ARG_OUTPUT, null, "Write output to this file", "output.log", null, null),

					// A Argument that provides a switch value. It does not contain some specific value. Just
					// the fact that it exists, should be enough.
					new Argument(ARG_FORCE, null, "Forces the thing.", null, null, null),

					// A Argument that provides a switch value. Because there already is a Argument that starts
					// with a S we define a different alias.
					new Argument(ARG_SILENT, "q", "Don't show anything.", null, null, null),
				});
			} catch (Exception) {
				// Handle the possible exceptions during argument parsing ...
			}

			// Execute all the Arguments.
			try {
				return Argument.ExecuteArguments(arguments);
			} catch (Exception) {
				// Handle the possible exceptions during execution ...
				// Maybe return some different exit code for different Exceptions.
				return 1;
			}
		}

		public static int SomeAction(List<Argument> args) {
			// Do something more complex in here.
			// Check for argument value:
			if (!Argument.HasArgument(ARG_FORCE, args)) {
				// if not return some error code
				return 2;
			}

			// Get the value of some argument:
			String important = Argument.GetArgument(ARG_THING, args).Value;

			return 0;
		}
	}

}
