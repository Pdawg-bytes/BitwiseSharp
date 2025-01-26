namespace BitwiseSharp.Core
{
    public class EnvironmentContext
    {
        private readonly Dictionary<string, ArbitraryNumber> _symbolTable = new();

        public bool TryGetVariable(string name, out ArbitraryNumber value) => _symbolTable.TryGetValue(name, out value);

        public bool TryCreateVariable(string name, ArbitraryNumber value)
        {
            if (_symbolTable.ContainsKey(name)) return false;

            _symbolTable[name] = value;
            return true;
        }

        public void SetVariable(string name, ArbitraryNumber value) => _symbolTable[name] = value;

        public bool HasVariable(string name) => _symbolTable.ContainsKey(name);
    }
}