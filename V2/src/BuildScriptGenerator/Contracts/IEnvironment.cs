// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// An abstraction over <see cref="System.Environment"/>.
    /// </summary>
    public interface IEnvironment
    {
        EnvironmentType Type { get; }

        /// <summary>
        /// Gets the value of an environment variable.
        /// </summary>
        /// <param name="name">Name of the environment variable. Case-sensitive.</param>
        /// <param name="defaultValue">Value to be returned if the variable isn't found.</param>
        /// <returns>
        /// The value of the given environment variable, or the default value if the environment variable isn't found.
        /// </returns>
        string GetEnvironmentVariable(string name, string defaultValue = null);

        /// <summary>
        /// Gets the value of an environment variable as a boolean, if found. The check
        /// is case insensitive.
        /// </summary>
        /// <param name="name">The name of the environment variable.</param>
        /// <returns>true if the variable exists and has value "true";
        /// false if it has value "false", or no value otherwise.
        /// </returns>
        bool? GetBoolEnvironmentVariable(string name);

        /// <summary>
        /// Retrieves the value of an environment variable that might contain a list of comma-separated values.
        /// </summary>
        /// <param name="name">The name of the environment variable.</param>
        /// <returns>A list with the values from the environment variables if any was found;
        /// null if the variable wasn't defined or had no value.</returns>
        IList<string> GetEnvironmentVariableAsList(string name);

        /// <summary>
        /// Retrieves all environment variable names and their values.
        /// </summary>
        /// <returns>
        /// A dictionary that contains all environment variable names and their values;
        /// otherwise, an empty dictionary if no environment variables are found.
        /// </returns>
        IDictionary GetEnvironmentVariables();

        /// <summary>
        /// Creates, modifies, or deletes an environment variable.
        /// </summary>
        /// <param name="name">The name of an environment variable.</param>
        /// <param name="value">A value to assign to variable.</param>
        void SetEnvironmentVariable(string name, string value);

        /// <summary>
        /// Returns a string array containing the command-line arguments for the current process.
        /// </summary>
        /// <returns>
        /// An array of string where each element contains a command-line argument.
        /// The first element is the executable file name, and the following zero or more elements
        /// contain the remaining command-line arguments.
        /// </returns>
        string[] GetCommandLineArgs();
    }
}
