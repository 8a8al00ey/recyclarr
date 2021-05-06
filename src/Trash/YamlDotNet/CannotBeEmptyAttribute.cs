using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Trash.YamlDotNet
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CannotBeEmptyAttribute : RequiredAttribute
    {
        public override bool IsValid(object? value)
        {
            return base.IsValid(value) &&
                   value is IEnumerable list &&
                   list.GetEnumerator().MoveNext();
        }
    }
}
