namespace BitwiseSharp.Core
{
    /// <summary>
    /// Represents the environment context that holds variables and manages symbol table lookups and assignments.
    /// </summary>
    public class EnvironmentContext
    {
        /// <summary>
        /// A dictionary that maps variable names to their respective values.
        /// </summary>
        private readonly Dictionary<string, ArbitraryNumber> _symbolTable = new();

        /// <summary>
        /// Attempts to retrieve the value of a variable by its name.
        /// </summary>
        /// <param name="name">The name of the variable to retrieve.</param>
        /// <param name="value">The value associated with the variable if found.</param>
        /// <returns><c>true</c> if the variable was found; otherwise, <c>false</c>.</returns>
        public bool TryGetVariable(string name, out ArbitraryNumber value) => _symbolTable.TryGetValue(name, out value);

        /// <summary>
        /// Attempts to create a new variable with the specified name and value.
        /// </summary>
        /// <param name="name">The name of the variable to create.</param>
        /// <param name="value">The value to assign to the variable.</param>
        /// <returns><c>true</c> if the variable was successfully created; otherwise, <c>false</c>.</returns>
        public bool TryCreateVariable(string name, ArbitraryNumber value)
        {
            if (_symbolTable.ContainsKey(name)) return false;

            _symbolTable[name] = value;
            return true;
        }

        /// <summary>
        /// Attempts to remove a variable from the symbol table.
        /// </summary>
        /// <param name="name">The name of the variable to remove.</param>
        /// <returns><c>true</c> if the variable was successfully removed; otherwise, <c>false</c>.</returns>
        public bool TryRemoveVariable(string name) => _symbolTable.Remove(name);

        /// <summary>
        /// Sets the value of an existing variable.
        /// </summary>
        /// <param name="name">The name of the variable to set.</param>
        /// <param name="value">The new value to assign to the variable.</param>
        public void SetVariable(string name, ArbitraryNumber value) => _symbolTable[name] = value;

        /// <summary>
        /// Checks if a variable with the specified name exists in the environment context.
        /// </summary>
        /// <param name="name">The name of the variable to check.</param>
        /// <returns><c>true</c> if the variable exists; otherwise, <c>false</c>.</returns>
        public bool HasVariable(string name) => _symbolTable.ContainsKey(name);
    }
}
