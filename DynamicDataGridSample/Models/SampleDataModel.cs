namespace DynamicDataGridSample.Models
{
    public class SampleDataModel : DynamicDataModel
    {
        public int Id
        {
            get => GetValue<int>(nameof(Id));
            set => SetValue(nameof(Id), value);
        }

        public string Name
        {
            get => GetValue<string>(nameof(Name)) ?? string.Empty;
            set => SetValue(nameof(Name), value);
        }

        public decimal Value
        {
            get => GetValue<decimal>(nameof(Value));
            set => SetValue(nameof(Value), value);
        }

        public DateTime CreatedAt
        {
            get => GetValue<DateTime>(nameof(CreatedAt));
            set => SetValue(nameof(CreatedAt), value);
        }

        public bool IsActive
        {
            get => GetValue<bool>(nameof(IsActive));
            set => SetValue(nameof(IsActive), value);
        }

        public override IEnumerable<ColumnDefinition> GetColumnDefinitions()
        {
            yield return new ColumnDefinition 
            { 
                Header = "ID",
                PropertyPath = nameof(Id),
                PropertyType = typeof(int)
            };

            yield return new ColumnDefinition 
            { 
                Header = "名前",
                PropertyPath = nameof(Name),
                PropertyType = typeof(string)
            };

            yield return new ColumnDefinition 
            { 
                Header = "値",
                PropertyPath = nameof(Value),
                PropertyType = typeof(decimal),
                StringFormat = "C"
            };

            yield return new ColumnDefinition 
            { 
                Header = "作成日時",
                PropertyPath = nameof(CreatedAt),
                PropertyType = typeof(DateTime),
                StringFormat = "yyyy/MM/dd HH:mm:ss"
            };

            yield return new ColumnDefinition 
            { 
                Header = "有効",
                PropertyPath = nameof(IsActive),
                PropertyType = typeof(bool)
            };
        }
    }
} 