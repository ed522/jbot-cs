using Jbot.Nametable;

using JetBrains.Annotations;

namespace Jbot.Data;

[PublicAPI]
public class DataObject
{
    public required ObjectTemplate Template { get; init; }
    public required IDictionary<string, DataField> Fields { get; init; }
    
    public DataObject(ObjectTemplate template)
    {
        this.Template = template;
        this.Fields = template.Fields.ToDictionary<FieldTemplate, string, DataField>
            (f => f.Name, DataField.ofUninitialized);
    }

    public DataField Get(string name)
    {
        return this.Fields[name];
    }

    public void Validate()
    {

        // check every field is under the correct key - next test depends on this
        // not LINQ because extracting state is worse than the foreach
        foreach ((string key, DataField value) in this.Fields)
        {
            if (key != value.Template.Name)
            {
                throw new InvalidOperationException($"Field '{value.Template.Name}' should be " + 
                                                    $"stored under that key, but is stored " + 
                                                    $"instead as '{key}'");
            }
        }
        
        // check all fields are present, and no extras
        if (this.Fields.Count != this.Template.Fields.Count)
        {
            foreach (FieldTemplate fieldTemplate in this.Template.Fields)
            {
                if (!this.Fields.ContainsKey(fieldTemplate.Name))
                {
                    throw new InvalidOperationException($"Required field '{fieldTemplate.Name}' " + 
                                                        $"is missing from the object");
                }
            }
            foreach (string key in this.Fields.Keys)
            {
                if (this.Template.GetFieldOrNull(key) == null)
                {
                    throw new InvalidOperationException($"Invalid field {key} found");
                }
            }
        }
        
        // validate individual fields
        foreach (DataField field in this.Fields.Values)
        {
            field.Validate();
        }
    }
    
    public DataField this[string key] => this.Get(key);
}
