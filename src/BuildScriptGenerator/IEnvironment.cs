// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// An abstraction over <see cref="System.Environment"/>.
    /// </summary>
    public interface IEnvironment
    {
        /// <summary>
        /// Gets values of an environment variable.
        /// </summary>
        /// <param name="name">Name of the environment variable. Is case-sensitive.</param>
        /// <returns>
        /// The value of the environment variable specified by variable,
        /// or null if the environment variable is not found.
        /// </returns>
        string GetEnvironmentVariable(string name);

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
    }
}