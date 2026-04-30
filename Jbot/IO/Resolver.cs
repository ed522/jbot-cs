using System.Reflection;

using Jbot.Model;

namespace Jbot.IO;

internal static class Resolver
{
    // find: static methods
    private const BindingFlags METHOD_BIND_FLAGS = BindingFlags.Static |
                                                   BindingFlags.Public |
                                                   BindingFlags.NonPublic;

    // find: all instance fields, including ones up the hierarchy
    private const BindingFlags FIELD_BIND_FLAGS = BindingFlags.Instance | BindingFlags.NonPublic |
                                                  BindingFlags.Public | BindingFlags.FlattenHierarchy;

    private static bool IsTransient(MemberInfo member) =>
        member.GetCustomAttribute<TransientAttribute>() is not null;

    private static bool IsInitializer(MemberInfo member) =>
        member.GetCustomAttribute<ReflectionInitializerAttribute>() is not null;

    private static MethodBase? ResolveInitializer(
        FieldTemplate[] templates, IReadOnlyList<MethodBase> methods, string objectName,
        out Dictionary<string, string> parameterMap, out Dictionary<string, int> parameterIndexMap
    )
    {
        parameterMap = [];
        parameterIndexMap = [];
        MethodBase[] inits = methods.Where(IsInitializer).ToArray();

        if (inits.Length > 1)
        {
            throw new InvalidOperationException($"Object {objectName} defines too many initializers");
        }

        // bail early - no options
        if (inits.Length == 0)
        {
            return null;
        }

        MethodBase init = inits[0];
        ParameterInfo[] paramInfo = init.GetParameters();

        // throw since it's attributed
        if (paramInfo.Length != 0 && paramInfo.Length != templates.Length)
        {
            throw new InvalidOperationException($"Object {objectName}'s initializer takes the " +
                                                $"wrong number of arguments - either take zero or" +
                                                $" one per field");
        }

        // if this is not a no-args constructor, check that all of the parameters have fields 
        // with corresponding bound types
        if (paramInfo.Length != 0)
        {
            List<string?> parameterNames = paramInfo.Select(p => p.Name).ToList();

            // parameter.Name is null (should only be for return types)
            if (parameterNames.Any(n => n is null))
            {
                throw new InvalidOperationException($"Object {objectName}'s initializer is missing " +
                                                    $"parameter names, so bindings cannot be created");
            }

            foreach (FieldTemplate template in templates)
            {
                // finds the first valid parameter name from a template's bound members
                // if there are none it propogates
                string? paramName =
                    template.BoundMembers?.Where(parameterNames.Contains).FirstOrDefault();

                if (paramName is not null)
                {
                    parameterIndexMap.Add(paramName, parameterNames.IndexOf(paramName));
                    parameterNames.Remove(paramName);
                    parameterMap.Add(template.Name, paramName);
                }
            }

            if (parameterNames.Count != 0)
            {
                throw new InvalidOperationException($"Object {objectName}'s initializer is not fully " +
                                                    $"covered by the object's fields (extra parameters: " +
                                                    $"{parameterNames}). Check that the initializer's " +
                                                    $"parameter names are correct and that the fields " +
                                                    $"have correct binding declarations.");
            }
        }

        return inits[0];
    }

    public static ObjectInfo ResolveObject(ObjectTemplate template)
    {
        // find every needed field, make sure they're also all there
        Dictionary<string, FieldInfo> fields = [];
        Dictionary<string, PropertyInfo> props = [];

        // choose a valid bound type and resolve it
        Type? objType = template.ResolveType();

        if (objType is null)
        {
            string boundTypesMsg = (template.BoundTypeNames?.Count ?? 0) > 0
                ? "none"
                : string.Join(",", template.BoundTypeNames ?? []);

            throw new InvalidOperationException($"Object {template.Name} cannot be reflectively " +
                                                $"converted without a valid bound type (bound types: " +
                                                $"{boundTypesMsg})");
        }

        // ignore private fields
        IEnumerable<FieldInfo> possibleFields = objType.GetFields(FIELD_BIND_FLAGS)
                                                       .Where(f => !f.IsPrivate);

        IEnumerable<PropertyInfo> possibleProps = objType.GetProperties(FIELD_BIND_FLAGS)
                                                         .Where(p => p.GetGetMethod() is not null);
        // treat properties and fields the same
        IEnumerable<MemberInfo> members = 
            possibleFields.Concat<MemberInfo>(possibleProps)
                          .Where(m => !IsTransient(m));

        // go through all props/fields and find a field template that binds to it
        foreach (MemberInfo member in members)
        {
            // ... but only if it's not annotated with [Transient]
            if (IsTransient(member)) continue;

            // find the name of a template that binds to this field, or null if none
            string? key = (from f in template.Fields
                           where f.BoundMembers?.Contains(member.Name) ?? false
                           select f.Name).FirstOrDefault();

            // if there is some field that binds to this field, assign it
            if (key is not null && member is FieldInfo fieldInfo)
            {
                fields.Add(key, fieldInfo);
            }
            else if (key is not null && member is PropertyInfo propInfo)
            {
                props.Add(key, propInfo);
            }
        }
        // fields finished

        // find initializer
        // 1. constructor with params 2. factory with params 3. empty constructor 4. empty factory

        ConstructorInfo[] constructors =
            objType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic);

        MethodInfo[] factories =
            objType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        MethodBase? resolvedInitializer = ResolveInitializer(template.Fields.ToArray(),
            constructors, template.Name,
            out Dictionary<string, string> map, out Dictionary<string, int> indexMap);

        bool isConstructor = true;

        // repeatedly reassign it if it's null (not coalescing since we need to know if it's a 
        // constructor)
        if (resolvedInitializer is null)
        {
            // static methods
            resolvedInitializer = ResolveInitializer(template.Fields.ToArray(),
                factories, template.Name, out map, out indexMap);

            isConstructor = false;
        }

        if (resolvedInitializer is null)
        {
            // empty constructor
            resolvedInitializer = objType.GetConstructor(Type.EmptyTypes);
            isConstructor = true;
            map = [];
            indexMap = [];
        }

        if (resolvedInitializer is null)
        {
            // empty factory
            resolvedInitializer =
                (from m in objType.GetMethods(METHOD_BIND_FLAGS)
                 where IsInitializer(m)
                 where m.GetParameters().Length == 0
                 select m
                ).FirstOrDefault();

            isConstructor = false;
            map = [];
            indexMap = [];
        }

        // too much nesting but this is necessary for lazy eval - we can't go looking up 
        // constructors if we've already found one
        if (resolvedInitializer is null)
        {
            throw new InvalidOperationException($"Object {template.Name} has no valid " +
                                                $"initializer");
        }

        // structure will be cached
        return new ObjectInfo(objType, fields, props,
            new Initializer(resolvedInitializer, isConstructor, map, indexMap));
    }

    internal readonly struct Initializer(
        MethodBase initializer,
        bool isConstructor,
        IReadOnlyDictionary<string, string> parameterMap,
        IReadOnlyDictionary<string, int> parameterIndexMap
    )
    {
        public bool IsConstructor { get; } = isConstructor; // TODO determine if necessary
        public MethodBase Method { get; } = initializer;
        public IReadOnlyDictionary<string, string> ParameterMap { get; } = parameterMap;
        public IReadOnlyDictionary<string, int> ParameterIndexMap { get; } = parameterIndexMap;
    }

    internal readonly struct ObjectInfo(
        Type type,
        Dictionary<string, FieldInfo> fields,
        Dictionary<string, PropertyInfo> props,
        Initializer initializer
    )
    {
        public Type TargetType { get; } = type;
        public Dictionary<string, FieldInfo> Fields { get; } = fields;
        public Dictionary<string, PropertyInfo> Props { get; } = props;
        public Initializer Initializer { get; } = initializer;
    }
}
