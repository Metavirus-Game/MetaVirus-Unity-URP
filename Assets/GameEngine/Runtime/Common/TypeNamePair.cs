using System;

namespace GameEngine.ObjectPool
{
    public class TypeNamePair
    {
        private readonly string _name;
        private readonly Type _type;

        public string Key => $"{_name}_{_type.Name}_Pair";

        public TypeNamePair(string name, Type type)
        {
            _name = name;
            _type = type;
        }

        public override bool Equals(object obj)
        {
            var other = obj as TypeNamePair;
            if (other == null)
            {
                return false;
            }

            return other._name == _name && other._type == _type;
        }

        protected bool Equals(TypeNamePair other)
        {
            return _name == other._name && _type == other._type;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_name != null ? _name.GetHashCode() : 0) * 397) ^ (_type?.GetHashCode() ?? 0);
            }
        }

        public override string ToString()
        {
            return Key;
        }
    }
}