using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Model.Metadata
{
    public abstract class ClassInfo
    {
        public abstract int CID { get; }

        public abstract string TableMappingStrategy { get; }

        public readonly Type Type;
        public readonly string ClassName;
        private readonly string _tableName;
        public virtual string TableName {
            get => _tableName;
        }

        private Type? _rootType = null;
        public Type RootType
        {
            get
            {
                if (_rootType == null)
                {
                    _rootType = Type;
                    while (_rootType!.BaseType != typeof(BusinessObject))
                        _rootType = _rootType.BaseType;
                }
                return _rootType;
            }
        }

        private ClassInfo? _rootClassInfo = null;
        public ClassInfo RootClassInfo
        {
            get
            {
                if (_rootClassInfo == null)
                {
                    _rootClassInfo = RootType.GetField("ClassInfo").GetValue(null) as ClassInfo;
                }
                return _rootClassInfo;
            }
        }

        private Type? _baseType = null;
        public Type BaseType
        {
            get
            {
                if (_baseType == null)
                {
                    _baseType = Type;
                    if(_baseType!.BaseType != typeof(BusinessObject))
                        _baseType = _baseType.BaseType;
                }
                return _baseType!;
            }
        }

        private ClassInfo? _baseClassInfo = null;
        public ClassInfo BaseClassInfo
        {
            get
            {
                if(_baseClassInfo == null)
                {
                    _baseClassInfo = BaseType.GetField("ClassInfo", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).GetValue(null) as ClassInfo;
                }
                return _baseClassInfo;
            }
        }

        private bool? _isDerivation = null;
        public bool IsDerivation
        {
            get
            {
                if(!_isDerivation.HasValue)
                    _isDerivation = Type.BaseType != typeof(BusinessObject);
                return _isDerivation.Value;
            }
        }

        protected ClassInfo(Type type)
        {
            if(type is null)
                throw new ArgumentNullException(nameof(type));

            if(!type.IsAssignableTo(typeof(BusinessObject)))
                throw new Exception($"Type must be derived of {typeof(BusinessObject).FullName}");

            ClassName = type.Name;
            _tableName = type.Name;
            Type = type;

            var ns = type.Namespace;
            if(!string.IsNullOrEmpty(ns))
            {
                var split = ns.LastIndexOf(".");
                if(split > 0)
                    _tableName = ns.Substring(split+1) + _tableName;
            }
        }
    }

    public abstract class ClassInfo<BO> : ClassInfo
        where BO: BusinessObject
    {
        protected ClassInfo()
            : base(typeof(BO))
        {

        }
    }
}
